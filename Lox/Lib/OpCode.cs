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
        Pop,
        GetLocal,
        SetLocal,
        GetGlobal,
        DefineGlobal,
        SetGlobal,
        Equal,
        Greater,
        Less,
        Add,
        Subtract,
        Multiply,
        Divide,
        Not,
        Negate,
        Print,
        Return,
    }
}
