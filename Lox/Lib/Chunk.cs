﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
using System.Reflection.Metadata;
using System.Text;

namespace Lox.Lib
{
    public class Chunk : DynamicArray<byte>
    {
        private readonly ValueArray constants;
        private readonly IList<int> lines;

        public Chunk()
        {
            count = 0;
            capacity = 0;
            values = null;
            lines = new List<int>();
            constants = new ValueArray();
        }

        public int Add(OpCode b, int line)
        {
            return Add((byte)b, line);
        }

        public int Add(byte b, int line)
        {
            lines.Add(line);
            return Add(b);
        }

        public int AddConstant(Value v)
        {
            return constants.Add(v);
        }

        public Value GetConstant(byte index)
        {
            return constants.values[index];
        }

        public int GetLine(int instruction)
        {
            return lines[instruction];
        }

        #region Disassembler

        public void Disassemble(TextWriter o, string name)
        {
            o.WriteLine("-- {0} --", name);

            for (int offset = 0; offset < count;) {
                offset = DisassembleInstruction(o, offset);
            }
        }

        private int SimpleInstruction(TextWriter o, string name, int offset)
        {
            o.WriteLine(name);
            return offset + 1;
        }

        private int ConstantInstruction(TextWriter o, string name, int offset)
        {
            byte constant = values[offset + 1];
            o.WriteLine("{0,-16} {1,4:X} '{2}'", name, constant, constants.values[constant].ToString());
            return offset + 2;
        }

        public int DisassembleInstruction(TextWriter o, int offset)
        {
            o.Write("{0:X4} ", offset);
            if (offset > 0 && lines[offset] == lines[offset - 1]) {
                o.Write("   | ");
            } else {
                o.Write("{0,4} ", lines[offset]);
            }

            OpCode instruction = (OpCode)values[offset];
            switch (instruction) {
                case OpCode.Constant:
                    return ConstantInstruction(o, "OP_CONSTANT", offset);
                case OpCode.Nil:
                    return SimpleInstruction(o, "OP_NIL", offset);
                case OpCode.True:
                    return SimpleInstruction(o, "OP_TRUE", offset);
                case OpCode.False:
                    return SimpleInstruction(o, "OP_FALSE", offset);
                case OpCode.Equal:
                    return SimpleInstruction(o, "OP_EQUALS", offset);
                case OpCode.Greater:
                    return SimpleInstruction(o, "OP_GREATER", offset);
                case OpCode.Less:
                    return SimpleInstruction(o, "OP_LESS", offset);

                case OpCode.Add:
                    return SimpleInstruction(o, "OP_ADD", offset);
                case OpCode.Subtract:
                    return SimpleInstruction(o, "OP_SUBTRACT", offset);
                case OpCode.Multiply:
                    return SimpleInstruction(o, "OP_MULTIPLY", offset);
                case OpCode.Divide:
                    return SimpleInstruction(o, "OP_DIVIDE", offset);
                case OpCode.Not:
                    return SimpleInstruction(o, "OP_NOT", offset);
                case OpCode.Negate:
                    return SimpleInstruction(o, "OP_NEGATE", offset);
                case OpCode.Return:
                    return SimpleInstruction(o, "OP_RETURN", offset);
                default:
                    o.Write("Unknown opcode");
                    return offset + 1;

            }
        }

        #endregion
    }
}
