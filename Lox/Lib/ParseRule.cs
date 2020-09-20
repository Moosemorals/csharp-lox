using System;
using System.Collections.Generic;
using System.Text;

namespace Lox.Lib
{
    internal class ParseRule
    {
        public TokenType Type;
        public Action Prefix;
        public Action Infix;
        public Precidence Precidence;
    }
}
