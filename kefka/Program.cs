using kefka.Source.Base;
using kefka.Source.Processors;
using System;
using System.Diagnostics;

namespace kefka
{
    class Program
    {
        static int Main(string[] args)
        {
            // kefka --eol=lf path/to/file.txt -o output/path/

            CmdLine cmdLine = new CmdLine(args);
            CmdProcessor processor = cmdLine.Parse();
            if (processor == null)
            {
                string error = processor.Error;
                if (!string.IsNullOrWhiteSpace(error))
                    Console.Error.WriteLine(error);
                return 1;
            }

            processor.RunAndWait();
            return 0;
        }
    }
}
