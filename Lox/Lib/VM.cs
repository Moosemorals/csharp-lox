using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Windows.Markup;

namespace Lox.Lib
{
    public class VM
    {
        private Chunk chunk;
        private int ip;
        private readonly TextWriter writer;
        private readonly Value[] stack = new Value[byte.MaxValue];
        private int stackTop = 0;
        private Hashtable globals = new Hashtable();

        public VM(TextWriter writer)
        {
            this.writer = writer;
        }

        public InterpretResult Interpret(string source)
        {
            Chunk chunk = new Chunk();

            Compiler compiler = new Compiler();

            if (!compiler.Compile(writer, source, chunk)) {
                return InterpretResult.CompileError;
            }

            writer.WriteLine("---End of compile---");

            return Interpret(chunk);
        }

        public InterpretResult Interpret(Chunk c)
        {
            chunk = c;
            ip = 0;
            return Run();
        }

        private bool IsFalsy(Value value)
        {
            return value.IsNil || (value.IsBool && !value.AsBool);
        }

        private InterpretResult Run()
        {
            while (true) {
#if DEBUG
                writer.WriteLine("->[{0}]", string.Join(", ", stack.Take(stackTop).Select(v => v.ToString())));
                chunk.DisassembleInstruction(writer, ip);
#endif

                byte instruction = ReadByte();
                switch ((OpCode)instruction) {
                    case OpCode.Constant:
                        Value constant = ReadConstant();
                        Push(constant);
                        break;
                    case OpCode.Nil: Push(Value.Nil); break;
                    case OpCode.True: Push(Value.Bool(true)); break;
                    case OpCode.False: Push(Value.Bool(false)); break;
                    case OpCode.Pop: Pop(); break;
                    case OpCode.GetLocal: {
                            byte slot = ReadByte();
                            Push(stack[slot]);
                            break;
                        }
                    case OpCode.SetLocal: {
                            byte slot = ReadByte();
                            stack[slot] = Peek(0);
                            break;
                        }
                    case OpCode.GetGlobal: {
                            ObjString name = ReadString();
                            if (!globals.ContainsKey(name)) {
                                RuntimeError("Undefined variable '{0}'.", name.Chars);
                                return InterpretResult.RuntimeError;
                            }
                            Value value = globals[name];
                            Push(value);
                            break;
                        }
                    case OpCode.DefineGlobal: {
                            ObjString name = ReadString();
                            globals[name] = Peek(0);
                            Pop();
                            break;
                        }
                    case OpCode.SetGlobal: {
                            ObjString name = ReadString();
                            if (!globals.ContainsKey(name)) {
                                RuntimeError("Undefined variable {0}.", name.Chars);
                                return InterpretResult.RuntimeError;
                            }
                            globals[name] = Peek(0);
                            break;
                        }
                    case OpCode.Equal: {
                            Value b = Pop();
                            Value a = Pop();
                            Push(Value.Bool(a.IsEqual(b)));
                            break;
                        }
                    case OpCode.Greater: {
                            if (!Peek(0).IsNumber || !Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = Pop().AsNumber;
                            double l = Pop().AsNumber;
                            Push(Value.Bool(l > r));
                            break;
                        }
                    case OpCode.Less: {
                            if (!Peek(0).IsNumber || !Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = Pop().AsNumber;
                            double l = Pop().AsNumber;
                            Push(Value.Bool(l < r));
                            break;
                        }
                    case OpCode.Add: {
                            if (Peek(0).IsString && Peek(1).IsString) {
                                ObjString b = Pop().AsString;
                                ObjString a = Pop().AsString;
                                Push(Value.Obj(ObjString.CopyString(a.Chars + b.Chars)));
                            } else if (Peek(0).IsNumber && Peek(1).IsNumber) {
                                double r = Pop().AsNumber;
                                double l = Pop().AsNumber;
                                Push(Value.Number(l + r));

                            } else {
                                RuntimeError("Operands must be two numbers or two strings");
                                return InterpretResult.RuntimeError;
                            }
                            break;
                        }
                    case OpCode.Subtract: {
                            if (!Peek(0).IsNumber || !Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = Pop().AsNumber;
                            double l = Pop().AsNumber;
                            Push(Value.Number(l - r));
                            break;
                        }
                    case OpCode.Multiply: {
                            if (!Peek(0).IsNumber || !Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = Pop().AsNumber;
                            double l = Pop().AsNumber;
                            Push(Value.Number(l * r));
                            break;
                        }
                    case OpCode.Divide: {
                            if (!Peek(0).IsNumber || !Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = Pop().AsNumber;
                            double l = Pop().AsNumber;
                            Push(Value.Number(l / r));
                            break;
                        }
                    case OpCode.Not:
                        Push(Value.Bool(IsFalsy(Pop())));
                        break;
                    case OpCode.Negate:
                        if (!Peek(0).IsNumber) {
                            RuntimeError("Operand must be a number");
                            return InterpretResult.RuntimeError;
                        }
                        Push(Value.Number(-Pop().AsNumber));
                        break;
                    case OpCode.Print: {
                            writer.WriteLine("{0}", Pop());
                            break;
                        }
                    case OpCode.Jump: {
                            ushort offset = ReadShort();
                            ip += offset;
                            break;
                        }
                    case OpCode.JumpIfFalse: {
                            ushort offset = ReadShort();
                            if (IsFalsy(Peek(0))) {
                                ip += offset;
                            }
                            break;
                        }
                    case OpCode.Loop: {
                            ushort offset = ReadShort();
                            ip -= offset;
                            break;
                        }
                    case OpCode.Return:
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

        private ushort ReadShort()
        {
            ip += 2;
            
            return (ushort)((chunk.values[ip - 2] << 8) | chunk.values[ip - 1]);
        }

        private ObjString ReadString()
        {
            return ReadConstant().AsString;
        }

        private void ResetStack()
        {
            stackTop = 0;
        }

        private void RuntimeError(string format, params object[] args)
        {
            writer.WriteLine(format, args);
            writer.WriteLine("[line {0}] in script", chunk.GetLine(ip));
            ResetStack();
        }

        private Value Peek(int distance)
        {
            return stack[stackTop - 1 - distance];
        }

        private Value Pop()
        {
            return stack[--stackTop];
        }

        private void Push(Value v)
        {
            stack[stackTop++] = v;
        }
    }

    public enum InterpretResult
    {
        OK,
        CompileError,
        RuntimeError,
    }
}
