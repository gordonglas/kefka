using kefka.Source.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace kefka.Source.Processors
{
    public abstract class CmdProcessor
    {
        public string Error { get; set; }

        abstract public bool ParseCmdLine(CmdLine cmdLine);
        abstract public void RunAndWait();
    }
}
