using System;
using System.Collections.Generic;
using System.Text;

namespace Lox.Lib
{
    internal class ParseRule
    {
        public TokenType Type;
        public Action<bool> Prefix;
        public Action<bool> Infix;
        public Precidence Precidence;
    }
}
