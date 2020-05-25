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
  kefka --eol=lf [input-files] [-o output]

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
  TODO: [--in-place]
        Modify the input-files directly.
        Ignores [-o output].
  [-o output]
        Output path or file.
        Path must end with slash.
        TODO: If omitted, [input-files] must be single file
        and sends output to STDOUT.
        TODO: If [input-files] is omitted,
        and this is specified, this is the output file.

Examples:
  kefka --eol=lf path/to/file.txt -o output/path/
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
