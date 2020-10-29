using System;
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

        public int Count => count;

        #region Disassembler

        public void Disassemble(TextWriter o, string name)
        {
            o.WriteLine("-- {0} --", name);

            for (int offset = 0; offset < count;) {
                offset = DisassembleInstruction(o, offset);
            }
        }

        private int ByteInstruction(TextWriter o, string name, int offset)
        {
            byte slot = values[offset + 1];
            o.WriteLine("{0,-16} {1,4:X}", name, slot);
            return offset + 2;
        }

        private int ConstantInstruction(TextWriter o, string name, int offset)
        {
            byte constant = values[offset + 1];
            o.WriteLine("{0,-16} {1,4:X} '{2}'", name, constant, constants.values[constant].ToString());
            return offset + 2;
        }

        private int JumpInstruction(TextWriter o, string name, int sign, int offset)
        {
            ushort jump = (ushort)((values[offset + 1] << 8) | values[offset + 2]);
            o.WriteLine("{0,-16} {1,4:X} -> {2,4:X}", name, offset, offset+ 3 + sign * jump);
            return offset + 3;
        }

        private int SimpleInstruction(TextWriter o, string name, int offset)
        {
            o.WriteLine(name);
            return offset + 1;
        }

        public int DisassembleInstruction(TextWriter writer, int offset)
        {
            writer.Write("{0:X4} ", offset);
            if (offset > 0 && lines[offset] == lines[offset - 1]) {
                writer.Write("   | ");
            } else {
                writer.Write("{0,4} ", lines[offset]);
            }

            OpCode instruction = (OpCode)values[offset];
            switch (instruction) {
                case OpCode.Constant:
                    return ConstantInstruction(writer, "OP_CONSTANT", offset);
                case OpCode.Nil:
                    return SimpleInstruction(writer, "OP_NIL", offset);
                case OpCode.True:
                    return SimpleInstruction(writer, "OP_TRUE", offset);
                case OpCode.False:
                    return SimpleInstruction(writer, "OP_FALSE", offset);
                case OpCode.Pop:
                    return SimpleInstruction(writer, "OP_POP", offset);
                case OpCode.GetLocal:
                    return ByteInstruction(writer, "OP_GET_LOCAL", offset);
                case OpCode.SetLocal:
                    return ByteInstruction(writer, "OP_SET_LOCAL", offset);
                case OpCode.GetGlobal:
                    return ConstantInstruction(writer, "OP_GET_GLOBAL", offset);
                case OpCode.DefineGlobal:
                    return ConstantInstruction(writer, "OP_DEFINE_GLOBAL", offset);
                case OpCode.SetGlobal:
                    return ConstantInstruction(writer, "OP_SET_GLOBAL", offset);
                case OpCode.Equal:
                    return SimpleInstruction(writer, "OP_EQUALS", offset);
                case OpCode.Greater:
                    return SimpleInstruction(writer, "OP_GREATER", offset);
                case OpCode.Less:
                    return SimpleInstruction(writer, "OP_LESS", offset);
                case OpCode.Add:
                    return SimpleInstruction(writer, "OP_ADD", offset);
                case OpCode.Subtract:
                    return SimpleInstruction(writer, "OP_SUBTRACT", offset);
                case OpCode.Multiply:
                    return SimpleInstruction(writer, "OP_MULTIPLY", offset);
                case OpCode.Divide:
                    return SimpleInstruction(writer, "OP_DIVIDE", offset);
                case OpCode.Not:
                    return SimpleInstruction(writer, "OP_NOT", offset);
                case OpCode.Negate:
                    return SimpleInstruction(writer, "OP_NEGATE", offset);
                case OpCode.Print:
                    return SimpleInstruction(writer, "OP_PRINT", offset);
                case OpCode.Jump:
                    return JumpInstruction(writer, "OP_JUMP", 1, offset);
                case OpCode.JumpIfFalse:
                    return JumpInstruction(writer, "OP_JUMP_IF_FALSE", 1, offset);
                case OpCode.Loop:
                    return JumpInstruction(writer, "OP_LOOP", -1, offset);
                case OpCode.Call:
                    return ByteInstruction(writer, "OP_CALL", offset);
                case OpCode.Return:
                    return SimpleInstruction(writer, "OP_RETURN", offset);
                default:
                    writer.WriteLine("Unknown opcode {0}", instruction);
                    return offset + 1;

            }
        }

        #endregion
    }
}
