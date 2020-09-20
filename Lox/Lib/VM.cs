using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Lox.Lib
{
    public class VM
    {
        private Chunk chunk;
        private int ip;
        private readonly TextWriter o;
        private readonly Value[] stack = new Value[byte.MaxValue];
        private int stackTop = 0;

        public VM(TextWriter writer)
        {
            o = writer;
        }

        public void Push(Value v)
        {
            stack[stackTop++] = v;
        }

        public Value Pop()
        {
            return stack[--stackTop];
        }

        public InterpretResult Interpret(Chunk c)
        {
            chunk = c;

            ip = 0;
            return Run();
        }

        private InterpretResult Run()
        {
            while (true) {
#if DEBUG
                o.WriteLine("->[{0}]", string.Join(", ", stack.Take(stackTop).Select(v => v.V)));
                chunk.DisassembleInstruction(o, ip);
#endif

                byte instruction = ReadByte();
                switch ((OpCode)instruction) {
                    case OpCode.Constant:
                        Value constant = ReadConstant();
                        Push(constant);
                        break;
                    case OpCode.Add: {
                            double r = Pop().V;
                            double l = Pop().V;
                            Push(new Value { V = l + r });
                        }
                        break;
                    case OpCode.Subtract: {
                            double r = Pop().V;
                            double l = Pop().V;
                            Push(new Value { V = l - r });
                        }
                        break;
                    case OpCode.Multiply: {
                            double r = Pop().V;
                            double l = Pop().V;
                            Push(new Value { V = l * r });
                        }
                        break;
                    case OpCode.Divide: {
                            double r = Pop().V;
                            double l = Pop().V;
                            Push(new Value { V = l / r });
                        }
                        break;
                    case OpCode.Negate:
                        Push(new Value { V = -Pop().V });
                        break;
                    case OpCode.Return:
                        o.WriteLine("{0}", Pop());
                        return InterpretResult.OK;
                }
            }
        }

        private byte ReadByte()
        {
            return chunk.values[ip++];
        }

        private Value ReadConstant()
        {
            return chunk.GetConstant(ReadByte());
        }

    }

    public enum InterpretResult
    {
        OK,
        CompileError,
        RuntimeError,
    }
}
