using kefka.Source.Processors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

Kefka is a cross-platform file transform tool,
which can currently perform two different tasks.
For detailed usage info on a task,
run the following:

kefka -h eol         End-of-line conversion.
kefka -h concat      Concat files.

kefka -v             Version info.
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
                if (_args.Length == 1)
                {
                    DisplayHelp();
                }
                else
                {
                    string topic = "--" + _args[1].ToLower() + "=";
                    if ((processor = CmdProcessor.Factory(topic)) != null)
                    {
                        Console.WriteLine(processor.GetHelpText());
                    }
                    else
                    {
                        Console.WriteLine("Unrecognized help topic.");
                        DisplayHelp();
                    }
                }
                return null;
            }
            if (_type == "--version" || _type == "-v")
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"Kefka v{version}");
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
