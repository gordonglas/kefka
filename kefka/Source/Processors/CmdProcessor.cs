using kefka.Source.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace kefka.Source.Processors
{
    public abstract class CmdProcessor
    {
        public static CmdProcessor Factory(string typeParam)
        {
            if (EolCmdProcessor.IsType(typeParam))
            {
                return new EolCmdProcessor();
            }
            else if (ConcatCmdProcessor.IsType(typeParam))
            {
                return new ConcatCmdProcessor();
            }

            return null;
        }

        private object _lockError = new object();
        private List<string> _errors = new List<string>();

        public void AppendError(string error)
        {
            lock (_lockError)
            {
                _errors.Add(error);
            }
        }

        public bool HasError()
        {
            lock (_lockError)
            {
                return _errors.Count > 0;
            }
        }

        public List<string> GetErrors()
        {
            lock (_lockError)
            {
                return new List<string>(_errors);
            }
        }

        abstract public string GetHelpText();
        abstract public bool ParseCmdLine(CmdLine cmdLine);
        abstract public bool RunAndWait();
    }
}
