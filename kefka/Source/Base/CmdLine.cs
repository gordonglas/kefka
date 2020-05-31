using kefka.Source.Processors;
using System;
using System.Collections.Generic;
using System.Text;

namespace kefka.Source.Base
{
    public class CmdLine
    {
        public string[] _args { get; set; }
        public string _type { get; set; }

        public CmdLine(string[] args)
        {
            _args = args;
        }

        string _helpText = @"

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

  TODO: If both output-path and output-file are omitted,
    input must be a single source
    and output is sent to STDOUT.

Examples:
  kefka --eol=lf path/to/file1.js path/to/file2.js -op output/path
  kefka --eol=lf path/to/file.js -of output/file.js
";

        public void DisplayHelp()
        {
            Console.WriteLine(_helpText);
        }

        public CmdProcessor Parse()
        {
            if (_args.Length == 0)
            {
                DisplayHelp();
                return null;
            }

            _type = _args[0].ToLower();

            CmdProcessor processor = null;

            if (_type == "--help" || _type == "-h" || _type == "help" || _type == "/?")
            {
                DisplayHelp();
                return null;
            }
            else if ((processor = CmdProcessor.Factory(_type)) != null)
            {
                if (!processor.ParseCmdLine(this))
                    return processor;
            }
            else
            {
                DisplayHelp();
                return null;
            }

            return processor;
        }
    }
}
