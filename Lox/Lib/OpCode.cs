using System;
using System.Collections.Generic;
using System.Text;

namespace Lox.Lib
{
    public enum OpCode : byte
    {
        Constant,
        Add,
        Subtract,
        Multiply,
        Divide,
        Negate,
        Return,
    }
}
