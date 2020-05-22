using kefka.Source.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace kefka.Source.Processors
{
    public class EolCmdProcessor : CmdProcessor
    {
        private const string EOL_TYPE_LF = "lf";
        private const string EOL_TYPE_CRLF = "crlf";
        private const string EOL_TYPE_CR = "cr";

        private string _eolTypeParam;
        private List<string> _inputFilesParam;
        private string _outputParam;

        public static bool IsType(string type)
        {
            return type.StartsWith("--eol=");
        }

        override public bool ParseCmdLine(CmdLine cmdLine)
        {
            string[] tok = cmdLine._type.Split('=');
            string type = tok[1];
            if (type.Trim() == "")
            {
                AppendError("Missing --eol= param value.");
                return false;
            }

            if (type == EOL_TYPE_LF ||
                type == EOL_TYPE_CRLF ||
                type == EOL_TYPE_CR)
            {
                _eolTypeParam = type;
            }
            else
            {
                AppendError("Invalid --eol= param value.");
                return false;
            }

            // kefka --eol=lf path/to/file.txt -o output/path/

            _inputFilesParam = new List<string>();
            bool? isOutput = null;
            for (int i = 1; i < cmdLine._args.Length; i++)
            {
                string arg = cmdLine._args[i];

                if (isOutput == null && arg == "-o")
                {
                    isOutput = true;
                }
                else if (isOutput == true)
                {
                    _outputParam = arg;
                    isOutput = false;
                    break;
                }
                else
                {
                    _inputFilesParam.Add(arg);
                }
            }

            if (_inputFilesParam.Count == 0)
            {
                AppendError("Missing [input-files] param.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_outputParam))
            {
                AppendError("Missing [output] param.");
                return false;
            }

            return true;
        }

        override public bool RunAndWait()
        {
            // enumerate input files
            List<string> inputFiles = new List<string>();
            foreach (string inputFileParam in _inputFilesParam)
            {
                if (!File.Exists(inputFileParam))
                {
                    AppendError($"Input file \"{inputFileParam}\" not found or you don't have read permission.");
                    return false;
                }

                inputFiles.Add(inputFileParam);
            }

            // check if output path exists
            if (!Directory.Exists(_outputParam))
            {
                AppendError("Output directory does not exist or you don't have read permission.");
                return false;
            }

            foreach (string inputFile in inputFiles)
            {
                // TODO: use multiple threads to handle multiple files at once.

                // TODO: either try to determine encoding of input file, or allow to specify it as command line param. For now, assumes UTF-8 without BOM.

                try
                {
                    // stream file into memory with a relatively small buffer, so we can handle large files.
                    using (FileStream ifs = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                    using (FileStream ofs = new FileStream(_outputParam, FileMode.Create, FileAccess.Write))
                    {
                        long bufSize = Math.Min(1024 * 1024, ifs.Length);
                        byte[] buf = new byte[bufSize];

                        long remainingBytes = ifs.Length;
                        while (remainingBytes > 0)
                        {
                            // say we're converting \r\n to \n
                            // input file = "some text\r\nmore text\r\n"
                            // bufSize = 10 // goes up just past first \r
                            // first ifs.Read() call happens.
                            // buf = "some text\r"
                            // algo that replaces strings must be careful when buf ends with \r (cause could be \r\n squence)
                            // if not end of stream and buf[bytesRead-1] ends with \r
                            //   only do replaces against buf[0] through buf[bytesRead-2]
                            //   then move buf[bytesRead-1] into buf[0]
                            //   set vars to preserve first byte in the buf during our next ifs.Read: so it will: ifs.Read(buf, 1, bufSize - 1);
                            // else // could be end of stream and/or buf does NOT end in \r
                            //   safe to do replaces against entire buf (up through buf[bytesRead-1])
                            //   set vars to not preserve first byte in the buf during our next ifs.Read: so it will: ifs.Read(buf, 0, bufSize);

                            // once we determine what section of the buf to do replaces against (in logic above),
                            // must do replaces by:
                            //  loop through bytes of section of the buf to do replaces against
                            //    if byte is not part of a eol-sequence we're looking for, copy byte to new "target buf"
                            //  write target buf to output file.

                            if (_eolTypeParam == EOL_TYPE_LF)
                            {

                            }
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
    }
}
