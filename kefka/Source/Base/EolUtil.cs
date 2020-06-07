using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace kefka.Source.Base
{
    class EolUtil
    {
        public const string EOL_TYPE_LF = "lf";
        public const string EOL_TYPE_CRLF = "crlf";
        public const string EOL_TYPE_CR = "cr";

        public const byte CARRIAGE_RETURN = 0x0D;
        public const byte LINE_FEED = 0x0A;

        public enum ParseEolTypeError
        {
            Success,
            Missing,
            Invalid
        }

        public static ParseEolTypeError ParseEqualsEolType(string type,
            out string eolType)
        {
            eolType = null;

            string[] tok = type.Split('=');
            string _type = tok[1];
            if (_type.Trim() == "")
            {
                return ParseEolTypeError.Missing;
            }

            if (!(_type == EOL_TYPE_LF ||
                  _type == EOL_TYPE_CRLF ||
                  _type == EOL_TYPE_CR))
            {
                return ParseEolTypeError.Invalid;
            }

            eolType = _type;
            return ParseEolTypeError.Success;
        }
    }
}
