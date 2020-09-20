using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Text;

namespace Lox.Lib
{
    public class Chunk
    {
        public Chunk()
        {
            count = 0;
            capacity = 0;
            code = null;
        }

        public void AddByte(OpCode b)
        {
            AddByte((byte)b);
        }

        public void AddByte(byte b)
        {
            if (capacity < count + 1) {
                int oldCapacity = capacity;
                capacity = GrowCapacity(oldCapacity);
                code = GrowArray(code, oldCapacity, capacity);
            }

            code[count] = b;
            count += 1; 
        }

        public void Disassemble(TextWriter o, string name) 
        {
            o.WriteLine("-- {0} --", name);

            for (int offset =0; offset < count; ) {
                offset = DisassembleInstruction(o, offset);
            }
        }

        private int SimpleInstruction(TextWriter o, string name, int offset)
        {
            o.WriteLine(name);
            return offset + 1;
        }

        private int DisassembleInstruction(TextWriter o, int offset)
        {
            o.Write("{0:X4} ", offset);

            OpCode instruction = (OpCode)code[offset];
            switch (instruction) {
                case OpCode.Return:
                    return SimpleInstruction(o, "OP_RETURN", offset);
                default:
                    o.Write("Unknown opcode");
                    return offset + 1;

            }
        }

        private int GrowCapacity(int cap)
        {
            return cap < 8 ? 8 : cap * 2;
        }

        private byte[] GrowArray(byte[] from, int oldCap, int newCap)
        {
            byte[] to = new byte[newCap];
            if (from != null) {
                Buffer.BlockCopy(from, 0, to, 0, oldCap);
            }
            return to;
        }

       public byte[] code;
        int count;
        int capacity;
    }
}
