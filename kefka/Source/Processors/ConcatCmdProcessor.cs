using kefka.Source.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static kefka.Source.Base.EolUtil;

namespace kefka.Source.Processors
{
    public class ConcatCmdProcessor : CmdProcessor
    {
        private string _helpText = @"

Concat files
* Supports large files.
* Can use line-ending delimiters between files.
* Anything that starts with 'TODO' below, has not been implemented yet.

Usage:
  kefka --concat [input-files] [-of output-file]
        [-d=DELIMITER_TYPE] [-dn=DELIMITER_NUMBER]
        [-e=EOF_EOL_TYPE]

Options:
  --concat
        Indicates file concatination.
  [input-files]
        Space-delimited list of input files.
  [-of output-file]
        Output file.
  [-d=DELIMITER_TYPE]
        Optional delimiter.
        If omitted, will not use a delimiter.
        DELIMITER_TYPE values:
            lf    line-feed
            crlf  carriage-return/line-feed
            cr    carriage-return
  [-dn=DELIMITER_NUMBER]
        Optional delimiter number.
        The number of times the delimiter will repeat.
        If omitted, will use 1.
  [-e=EOF_EOL_TYPE]
        Optional end-line at end of file.
        If omitted, no end-line at end of file.
        EOF_EOL_TYPE values:
            lf    line-feed
            crlf  carriage-return/line-feed
            cr    carriage-return

Example:
  kefka --concat path/to/file1.js path/to/file2.js
        -of output/file.js -d=lf -dn=2 -e=lf
";

        private List<string> _inputFilesParam;
        private string _outputFileParam;
        private string _eolTypeDelimiter;
        private int _delimiterRepeat;
        private string _eofEolType;

        public static bool IsType(string type)
        {
            return type.StartsWith("--concat");
        }

        override public string GetHelpText()
        {
            return _helpText;
        }

        public override bool ParseCmdLine(CmdLine cmdLine)
        {
            // kefka --concat path/to/file1.js path/to/file2.js
            //       -of output/file.js -d=lf -dn=2 -e=lf

            _delimiterRepeat = 1;

            _inputFilesParam = new List<string>();
            bool? isOutputFile = null;
            for (int i = 1; i < cmdLine._args.Length; i++)
            {
                string arg = cmdLine._args[i];

                if (arg == "-of")
                {
                    isOutputFile = true;
                    continue;
                }
                else if (isOutputFile == true)
                {
                    _outputFileParam = arg;
                    isOutputFile = false;
                    continue;
                }

                if (arg.StartsWith("-d="))
                {
                    ParseEolTypeError error = EolUtil.ParseEqualsEolType(arg, out string eolType);
                    if (error != ParseEolTypeError.Success)
                    {
                        AppendError($"Missing or invalid -d= param value.");
                        return false;
                    }
                    _eolTypeDelimiter = eolType;
                    continue;
                }
                else if (arg.StartsWith("-dn="))
                {
                    string[] tok = arg.Split('=');
                    if (!int.TryParse(tok[1], out int delimRepeat))
                    {
                        AppendError("Missing or invalid -dn= param value.");
                        return false;
                    }
                    _delimiterRepeat = delimRepeat;
                    continue;
                }
                else if (arg.StartsWith("-e="))
                {
                    ParseEolTypeError error = EolUtil.ParseEqualsEolType(arg, out string eolType);
                    if (error != ParseEolTypeError.Success)
                    {
                        AppendError($"Missing or invalid -e= param value.");
                        return false;
                    }
                    _eofEolType = eolType;
                    continue;
                }

                _inputFilesParam.Add(arg);
            }

            if (_inputFilesParam.Count == 0)
            {
                AppendError("Missing [input-files] param.");
                return false;
            }
            else if (_inputFilesParam.Count == 1)
            {
                AppendError("At least 2 [input-files] are required for concatination.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_outputFileParam))
            {
                AppendError("[output-file] param required.");
                return false;
            }

            return true;
        }

        public override bool RunAndWait()
        {
            List<string> inputFiles = new List<string>();
            string absoluteOutputPath = null;
            try
            {
                // enumerate input files
                foreach (string inputFileParam in _inputFilesParam)
                {
                    // convert relative to absolute path with respect to current working directory
                    string absoluteInputFile = Path.GetFullPath(inputFileParam);

                    if (!File.Exists(absoluteInputFile))
                    {
                        AppendError($"Input file \"{absoluteInputFile}\" not found or you don't have read permission.");
                        return false;
                    }

                    inputFiles.Add(absoluteInputFile);
                }

                // check if path portion of output file exists
                absoluteOutputPath = Path.GetDirectoryName(_outputFileParam);
                if (!Directory.Exists(absoluteOutputPath))
                {
                    AppendError("Output directory does not exist or you don't have read permission.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                AppendError(ex.ToString());
                return false;
            }

            try
            {
                string outputFile = Path.Combine(absoluteOutputPath, Path.GetFileName(_outputFileParam));
                using (FileStream ofs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                {
                    foreach (string inputFile in inputFiles)
                    {
                        // stream file into memory with a relatively small buffer, so we can handle large files.
                        using (FileStream ifs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                        {
                            long bufSize = Math.Min(65536, ifs.Length);
                            byte[] buf = new byte[bufSize];

                            long remainingBytes = ifs.Length;
                            while (remainingBytes > 0)
                            {
                                int bytesRead = ifs.Read(buf, 0, buf.Length);
                                if (bytesRead == 0)
                                    break;

                                ofs.Write(buf, 0, bytesRead);

                                remainingBytes -= bytesRead;
                            }
                        }

                        if (_eolTypeDelimiter != null)
                        {
                            while (_delimiterRepeat-- > 0)
                            {
                                if (_eolTypeDelimiter == EOL_TYPE_LF)
                                {
                                    ofs.WriteByte(LINE_FEED);
                                }
                                else if (_eolTypeDelimiter == EOL_TYPE_CRLF)
                                {
                                    ofs.WriteByte(CARRIAGE_RETURN);
                                    ofs.WriteByte(LINE_FEED);
                                }
                                else if (_eolTypeDelimiter == EOL_TYPE_CR)
                                {
                                    ofs.WriteByte(CARRIAGE_RETURN);
                                }
                            }
                        }
                    }

                    if (_eofEolType == EOL_TYPE_LF)
                    {
                        ofs.WriteByte(LINE_FEED);
                    }
                    else if (_eofEolType == EOL_TYPE_CRLF)
                    {
                        ofs.WriteByte(CARRIAGE_RETURN);
                        ofs.WriteByte(LINE_FEED);
                    }
                    else if (_eofEolType == EOL_TYPE_CR)
                    {
                        ofs.WriteByte(CARRIAGE_RETURN);
                    }
                }
            }
            catch (Exception ex)
            {
                AppendError(ex.ToString());
                return false;
            }

            return true;
        }
    }
}
