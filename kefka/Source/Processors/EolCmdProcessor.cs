using kefka.Source.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using static kefka.Source.Base.EolUtil;

namespace kefka.Source.Processors
{
    public class EolCmdProcessor : CmdProcessor
    {
        private string _helpText = @"

Convert line endings
* Supports large files.
* Supports UTF-8 input.
* Anything that starts with 'TODO' below, has not been implemented yet.

Usage:
  kefka --eol=lf [input-files] [-op output-path]
  kefka --eol=lf [input-file] [-of output-file]

Options:
  --eol=TYPE
        Set to line ending type that you want to convert to.
        Only supports UTF-8 input/output.
        TYPE values:
            lf    line-feed
            TODO: crlf  carriage-return/line-feed
            TODO: cr    carriage-return
  [input-files]
        Optional space-delimited list of input files.
        TODO: Can use simple wildcard.
        TODO: If omitted, reads from STDIN.
  [-op output-path]
        Output path. Same input filename will be used.
  [-of output-file]
        Output file. Must have single input source.
  --no-remove-bom
        Do not remove byte-order-mark if one exists.
        Default is to remove it.
  --in-place
        Modifies original [input-files].
        Ignores -op and -of.

  TODO: If both output-path and output-file are omitted,
    input must be a single source
    and output is sent to STDOUT.

Examples:
  kefka --eol=lf path/to/file1.js path/to/file2.js -op output/path
  kefka --eol=lf path/to/file.js -of output/file.js
";

        private string _eolTypeParam;
        private List<string> _inputFilesParam;
        private string _outputPathParam;
        private string _outputFileParam;
        private bool _removeBom = true;
        private bool _inPlace;

        public static bool IsType(string type)
        {
            return type.StartsWith("--eol=");
        }

        override public string GetHelpText()
        {
            return _helpText;
        }

        override public bool ParseCmdLine(CmdLine cmdLine)
        {
            ParseEolTypeError error = EolUtil.ParseEqualsEolType(cmdLine._type, out string eolType);
            if (error != ParseEolTypeError.Success)
            {
                AppendError($"Missing or invalid -eol= param value.");
                return false;
            }
            _eolTypeParam = eolType;

            // kefka --eol=lf path/to/file.txt -op output/path
            // kefka --eol=lf path/to/file.txt -of output/file.txt

            _inputFilesParam = new List<string>();
            bool? isOutputPath = null;
            bool? isOutputFile = null;
            for (int i = 1; i < cmdLine._args.Length; i++)
            {
                string arg = cmdLine._args[i];

                if (isOutputPath == null && arg == "-op")
                {
                    isOutputPath = true;
                    continue;
                }
                else if (isOutputPath == true)
                {
                    _outputPathParam = arg;
                    isOutputPath = false;
                    break;
                }

                if (isOutputFile == null && arg == "-of")
                {
                    isOutputFile = true;
                    continue;
                }
                else if (isOutputFile == true)
                {
                    _outputFileParam = arg;
                    isOutputFile = false;
                    break;
                }

                if (arg == "--no-remove-bom")
                {
                    _removeBom = false;
                    continue;
                }

                if (arg == "--in-place")
                {
                    _inPlace = true;
                    continue;
                }

                _inputFilesParam.Add(arg);
            }

            if (_inputFilesParam.Count == 0)
            {
                AppendError("Missing [input-files] param.");
                return false;
            }

            if (!_inPlace)
            {
                bool hasOutputPath = !string.IsNullOrWhiteSpace(_outputPathParam);
                bool hasOutputFile = !string.IsNullOrWhiteSpace(_outputFileParam);

                if (isOutputPath == true && !hasOutputPath)
                {
                    AppendError("Missing [output-path] param.");
                    return false;
                }
                else if (isOutputFile == true && !hasOutputFile)
                {
                    AppendError("Missing [output-file] param.");
                    return false;
                }

                if (!hasOutputPath && !hasOutputFile)
                {
                    AppendError("[output-path] or [output-file] param required.");
                    return false;
                }
                else if (hasOutputPath && hasOutputFile)
                {
                    AppendError("Cannot have both [output-path] and [output-file] params.");
                    return false;
                }

                if (hasOutputFile && _inputFilesParam.Count > 1)
                {
                    AppendError("Single [output-file] with multiple input files. Maybe you meant to use -op instead?");
                    return false;
                }
            }

            return true;
        }

        override public bool RunAndWait()
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

                if (_inPlace)
                {
                    // make sure temp path exists
                    absoluteOutputPath = Path.GetTempPath();
                    try
                    {
                        Directory.CreateDirectory(absoluteOutputPath);
                    }
                    catch
                    {
                        AppendError("Unable to create temp path needed for --in-place option or you do not have permission.");
                        return false;
                    }
                    if (!Directory.Exists(absoluteOutputPath))
                    {
                        AppendError("Temp folder does not exist or you don't have read permission.");
                        return false;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(_outputPathParam))
                    {
                        // check if output path exists
                        absoluteOutputPath = Path.GetFullPath(_outputPathParam);
                        if (!Directory.Exists(absoluteOutputPath))
                        {
                            AppendError("Output directory does not exist or you don't have read permission.");
                            return false;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(_outputFileParam))
                    {
                        // check if path portion of output file exists
                        absoluteOutputPath = Path.GetDirectoryName(_outputFileParam);
                        if (absoluteOutputPath != "" && !Directory.Exists(absoluteOutputPath))
                        {
                            AppendError("Output directory does not exist or you don't have read permission.");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendError(ex.ToString());
                return false;
            }

            foreach (string inputFile in inputFiles)
            {
                // TODO: use multiple threads to handle multiple files at once.

                try
                {
                    string outputFile = null;
                    if (_inPlace)
                    {
                        outputFile = Path.Combine(absoluteOutputPath, Guid.NewGuid() + Path.GetFileName(inputFile));
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(_outputPathParam))
                        {
                            outputFile = Path.Combine(absoluteOutputPath, Path.GetFileName(inputFile));
                        }
                        else if (!string.IsNullOrWhiteSpace(_outputFileParam))
                        {
                            outputFile = Path.Combine(absoluteOutputPath, Path.GetFileName(_outputFileParam));
                        }
                        else
                        {
                            throw new Exception("invalid params");
                        }
                    }

                    // stream file into memory with a relatively small buffer, so we can handle large files.
                    using (FileStream ifs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    using (FileStream ofs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        long bufSize = Math.Min(65536, ifs.Length);
                        byte[] buf = new byte[bufSize];

                        if (_eolTypeParam == EOL_TYPE_LF)
                        {
                            ConvertToLF(ifs, ofs, buf);
                        }
                        // TODO: handle other eol types
                    }

                    if (_inPlace)
                    {
                        // overwrite inputFile with it's new contents
                        using (FileStream ifs = new FileStream(outputFile, FileMode.Open, FileAccess.Read))
                        using (FileStream ofs = new FileStream(inputFile, FileMode.Create, FileAccess.Write))
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

                        // delete temp file
                        try
                        {
                            File.Delete(outputFile);
                        }
                        catch
                        {
                            // OS will clean up the temp file eventually.
                        }
                    }
                }
                catch (Exception ex)
                {
                    AppendError(ex.ToString());
                    return false;
                }
            }

            return true;
        }

        private void ConvertToLF(FileStream ifs, FileStream ofs, byte[] buf)
        {
            int readOffset = 0;
            bool firstRead = true;

            long remainingBytes = ifs.Length;
            while (remainingBytes > 0)
            {
                int bytesRead = ifs.Read(buf, readOffset, buf.Length - readOffset);
                if (bytesRead == 0)
                    throw new Exception("bytesRead == 0");

                remainingBytes -= bytesRead;

                if (firstRead)
                {
                    if (_removeBom && buf[0] == 0xEF && buf[1] == 0xBB && buf[2] == 0xBF)
                    {
                        if (bytesRead == 3)
                            break;

                        for (int i = 3, pos = 0; i < bytesRead; i++, pos++)
                        {
                            buf[pos] = buf[i];
                        }
                        bytesRead -= 3;
                    }
                    firstRead = false;
                }

                // if not end of stream and buf ends with \r
                if (remainingBytes == 0 && buf[bytesRead - 1] == CARRIAGE_RETURN)
                {
                    // only do replaces against buf[0] through buf[bytesRead-2]
                    ReplaceWithLF(ofs, buf, bytesRead - 2);
                    // move trailing \r into buf[0] to preserve it for next read/replace
                    buf[0] = buf[bytesRead - 1];
                    readOffset = 1;
                }
                else
                {
                    ReplaceWithLF(ofs, buf, bytesRead - 1);
                    readOffset = 0;
                }
            }
        }

        private void ReplaceWithLF(FileStream ofs, byte[] buf, int bufEnd)
        {
            byte[] outbuf = new byte[bufEnd + 1];
            int outpos = 0;
            int pos = 0;

            while (pos <= bufEnd)
            {
                if (buf[pos] == CARRIAGE_RETURN)
                {
                    if (pos == bufEnd)
                        break;

                    if (pos + 1 <= bufEnd && buf[pos + 1] == LINE_FEED)
                    {
                        pos += 2;
                    }
                    else
                    {
                        pos++;
                    }

                    outbuf[outpos] = LINE_FEED;
                }
                else
                {
                    outbuf[outpos] = buf[pos];
                    pos++;
                }

                outpos++;
            }

            if (outpos > 0)
                ofs.Write(outbuf, 0, outpos);
        }
    }
}
