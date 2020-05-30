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
            // kefka --eol=lf path/to/file.txt -op output/path
            // kefka --eol=lf path/to/file.txt -of output/file.txt

            CmdLine cmdLine = new CmdLine(args);
            CmdProcessor processor = cmdLine.Parse();
            if (processor == null)
                return 1;

            if (processor.HasError())
                return PrintErrors(processor);

            if (!processor.RunAndWait())
            {
                if (processor.HasError())
                    return PrintErrors(processor);
            }

            return 0;
        }

        private static int PrintErrors(CmdProcessor processor)
        {
            foreach (string error in processor.GetErrors())
            {
                Console.Error.WriteLine(error);
            }
            return 1;
        }
    }
}
