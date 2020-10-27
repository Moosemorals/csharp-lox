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
            Stack stack = new Stack(byte.MaxValue);
            while (true) {
#if DEBUG
                writer.WriteLine("->[{0}]", stack.ToString());
                chunk.DisassembleInstruction(writer, ip);
#endif

                byte instruction = ReadByte();
                switch ((OpCode)instruction) {
                    case OpCode.Constant:
                        Value constant = ReadConstant();
                        stack.Push(constant);
                        break;
                    case OpCode.Nil: stack.Push(Value.Nil); break;
                    case OpCode.True: stack.Push(Value.Bool(true)); break;
                    case OpCode.False: stack.Push(Value.Bool(false)); break;
                    case OpCode.Pop: stack.Pop(); break;
                    case OpCode.GetLocal: {
                            byte slot = ReadByte();
                            stack.Push(stack[slot]);
                            break;
                        }
                    case OpCode.SetLocal: {
                            byte slot = ReadByte();
                            stack[slot] = stack.Peek(0);
                            break;
                        }
                    case OpCode.GetGlobal: {
                            ObjString name = ReadString();
                            if (!globals.ContainsKey(name)) {
                                RuntimeError("Undefined variable '{0}'.", name.Chars);
                                return InterpretResult.RuntimeError;
                            }
                            Value value = globals[name];
                            stack.Push(value);
                            break;
                        }
                    case OpCode.DefineGlobal: {
                            ObjString name = ReadString();
                            globals[name] = stack.Peek(0);
                            stack.Pop();
                            break;
                        }
                    case OpCode.SetGlobal: {
                            ObjString name = ReadString();
                            if (!globals.ContainsKey(name)) {
                                RuntimeError("Undefined variable {0}.", name.Chars);
                                return InterpretResult.RuntimeError;
                            }
                            globals[name] = stack.Peek(0);
                            break;
                        }
                    case OpCode.Equal: {
                            Value b = stack.Pop();
                            Value a = stack.Pop();
                            stack.Push(Value.Bool(a.IsEqual(b)));
                            break;
                        }
                    case OpCode.Greater: {
                            if (!stack.Peek(0).IsNumber || !stack.Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = stack.Pop().AsNumber;
                            double l = stack.Pop().AsNumber;
                            stack.Push(Value.Bool(l > r));
                            break;
                        }
                    case OpCode.Less: {
                            if (!stack.Peek(0).IsNumber || !stack.Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = stack.Pop().AsNumber;
                            double l = stack.Pop().AsNumber;
                            stack.Push(Value.Bool(l < r));
                            break;
                        }
                    case OpCode.Add: {
                            if (stack.Peek(0).IsString && stack.Peek(1).IsString) {
                                ObjString b = stack.Pop().AsString;
                                ObjString a = stack.Pop().AsString;
                                stack.Push(Value.Obj(ObjString.CopyString(a.Chars + b.Chars)));
                            } else if (stack.Peek(0).IsNumber && stack.Peek(1).IsNumber) {
                                double r = stack.Pop().AsNumber;
                                double l = stack.Pop().AsNumber;
                                stack.Push(Value.Number(l + r));

                            } else {
                                RuntimeError("Operands must be two numbers or two strings");
                                return InterpretResult.RuntimeError;
                            }
                            break;
                        }
                    case OpCode.Subtract: {
                            if (!stack.Peek(0).IsNumber || !stack.Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = stack.Pop().AsNumber;
                            double l = stack.Pop().AsNumber;
                            stack.Push(Value.Number(l - r));
                            break;
                        }
                    case OpCode.Multiply: {
                            if (!stack.Peek(0).IsNumber || !stack.Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = stack.Pop().AsNumber;
                            double l = stack.Pop().AsNumber;
                            stack.Push(Value.Number(l * r));
                            break;
                        }
                    case OpCode.Divide: {
                            if (!stack.Peek(0).IsNumber || !stack.Peek(1).IsNumber) {
                                RuntimeError("Operands must be numbers");
                                return InterpretResult.RuntimeError;
                            }
                            double r = stack.Pop().AsNumber;
                            double l = stack.Pop().AsNumber;
                            stack.Push(Value.Number(l / r));
                            break;
                        }
                    case OpCode.Not:
                        stack.Push(Value.Bool(IsFalsy(stack.Pop())));
                        break;
                    case OpCode.Negate:
                        if (!stack.Peek(0).IsNumber) {
                            RuntimeError("Operand must be a number");
                            return InterpretResult.RuntimeError;
                        }
                        stack.Push(Value.Number(-stack.Pop().AsNumber));
                        break;
                    case OpCode.Print: {
                            writer.WriteLine("{0}", stack.Pop());
                            break;
                        }
                    case OpCode.Jump: {
                            ushort offset = ReadShort();
                            ip += offset;
                            break;
                        }
                    case OpCode.JumpIfFalse: {
                            ushort offset = ReadShort();
                            if (IsFalsy(stack.Peek(0))) {
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

        private void RuntimeError(string format, params object[] args)
        {
            writer.WriteLine(format, args);
            writer.WriteLine("[line {0}] in script", chunk.GetLine(ip));
        }

        private ref struct Stack
        {
            public Stack(int length)
            {
                _values = new Value[length];
                stackTop = 0;
            }

            private Span<Value> _values;
            private int stackTop;

            public void Reset()
            {
                stackTop = 0;
            }

            public Value Peek(int distance)
            {
                return _values[stackTop - 1 - distance];
            }

            public Value this[int index] {
                get {
                    return _values[index];
                }
                set {
                    _values[index] = value;
                }
            }

            public void Push(Value v)
            {
                _values[stackTop++] = v;
            }

            public Value Pop()
            {
                return _values[--stackTop];
            }

            public Span<Value> SubStack(int from)
            {
                return _values.Slice(from);
            }

            public Span<Value> Take(int count)
            {
                return _values.Slice(0, count);
            }

            public override string ToString()
            {
                return string.Join(", ", Take(stackTop).ToArray().Select(v => v.ToString()));
            }
        }
    }

    public enum InterpretResult
    {
        OK,
        CompileError,
        RuntimeError,
    }
}
