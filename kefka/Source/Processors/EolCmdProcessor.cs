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

        private string _eolType;
        private List<string> _inputFiles;
        private string _output;

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
                Error = "Missing --eol= param value.";
                return false;
            }

            if (type == EOL_TYPE_LF ||
                type == EOL_TYPE_CRLF ||
                type == EOL_TYPE_CR)
            {
                _eolType = type;
            }
            else
            {
                Error = "Invalid --eol= param value.";
                return false;
            }

            // kefka --eol=lf path/to/file.txt -o output/path/

            _inputFiles = new List<string>();
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
                    _output = arg;
                    isOutput = false;
                    break;
                }
                else
                {
                    _inputFiles.Add(arg);
                }
            }

            if (_inputFiles.Count == 0)
            {
                Error = "Missing [input-files] param.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(_output))
            {
                Error = "Missing [output] param.";
                return false;
            }

            return true;
        }

        override public void RunAndWait()
        {
            // TODO: possibly use multiple threads to handle multiple files at once.
            // TODO: stream file into memory with small buffer, so we can do replaces on large files.
            
            if (_eolType == EOL_TYPE_LF)
            {

            }
        }
    }
}
