using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Cache;
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
           return  constants.Add(v);
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

        private int constantInstruction(TextWriter o, string name, int offset)
        {
            byte constant = values[offset + 1];
            o.WriteLine("{0,-16} {1,4:X} '{2}'", name, constant, constants.values[constant].ToString());
            return offset + 2;
        }

        private int DisassembleInstruction(TextWriter o, int offset)
        {
            o.Write("{0:X4} ", offset);
            if (offset > 0 && lines[offset] == lines[offset-1]) {
                o.Write("   | ");
            } else {
                o.Write("{0,4} ", lines[offset]);
            }

            OpCode instruction = (OpCode)values[offset];
            switch (instruction) {
                case OpCode.Return:
                    return SimpleInstruction(o, "OP_RETURN", offset);
                case OpCode.Constant:
                    return constantInstruction(o, "OP_CONSTANT", offset);
                default:
                    o.Write("Unknown opcode");
                    return offset + 1;

            }
        }

        #endregion
    }
}
