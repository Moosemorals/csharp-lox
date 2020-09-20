using System;
using System.Collections.Generic;
using System.Text;

namespace Lox.Lib
{
    public enum OpCode : byte
    {
        Constant,
        Nil,
        True,
        False,
        Equal,
        Greater,
        Less,
        Add,
        Subtract,
        Multiply,
        Divide,
        Not,
        Negate,
        Return,
    }
}
