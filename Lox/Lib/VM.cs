using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private const int FramesMax = 64;
        private const int StackMax = FramesMax * byte.MaxValue;

        private readonly CallFrame[] frames = new CallFrame[FramesMax];
        private int frameCount;
        private CallFrame frame;

        private readonly TextWriter writer;
        private readonly SliceableArray<Value> stack = new SliceableArray<Value>(StackMax);
        private int stackTop = 0;
        private Hashtable globals = new Hashtable();

        public VM(TextWriter writer)
        {
            this.writer = writer;

            DefineNative("clock", ClockNative);
        }

        public InterpretResult Interpret(string source)
        {
            ObjFunction function = new Compiler(Compiler.FunctionType.Script).Compile(writer, source);

            if (function == null) {
                return InterpretResult.CompileError;
            }

            Push(Value.Obj(function));

            CallValue(Value.Obj(function), 0);

            return Run();
        }

        private InterpretResult Run()
        {
            frame = frames[frameCount - 1];
            while (true) {
#if DEBUG
                writer.WriteLine("->[{0}]", string.Join(", ", stack.Take(stackTop).Select(v => v.ToString())));
                frame.function.chunk.DisassembleInstruction(writer, frame.ip);
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
                            Push(frame.slots[slot]);
                            break;
                        }
                    case OpCode.SetLocal: {
                            byte slot = ReadByte();
                            frame.slots[slot] = Peek(0);
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
                            frame.ip += offset;
                            break;
                        }
                    case OpCode.JumpIfFalse: {
                            ushort offset = ReadShort();
                            if (IsFalsy(Peek(0))) {
                                frame.ip += offset;
                            }
                            break;
                        }
                    case OpCode.Loop: {
                            ushort offset = ReadShort();
                            frame.ip -= offset;
                            break;
                        }
                    case OpCode.Call: {
                            int argCount = ReadByte();
                            if (!CallValue(Peek(argCount), argCount)) {
                                return InterpretResult.RuntimeError;
                            }
                            frame = frames[frameCount - 1];
                            break;
                        }
                    case OpCode.Return: {
                            Value result = Pop();

                            frameCount -= 1;
                            if (frameCount == 0) {
                                Pop();
                                return InterpretResult.OK;
                            }

                            stackTop = frame.slots.Offset;
                            Push(result);

                            frame = frames[frameCount - 1];
                            break;
                        }

                }
            }
        }

        private bool CallValue(Value callee, int argCount)
        {
            if (callee.IsObj) {
                switch (callee.ObjType) {
                    case ObjType.Function:
                        return Call(callee.AsFunction, argCount);
                    case ObjType.Native: {
                            Func<int, Value[], Value> native = callee.AsNative;
                            Value result = native(argCount, stack.Take(argCount));
                            stackTop -= argCount + 1;
                            Push(result);
                            return true;
                        }
                    default:
                        break;
                }
            }

            RuntimeError("Can only call functions and classes");
            return false;
        }

        private bool Call(ObjFunction function, int argCount)
        {
            if (argCount != function.arity) {
                RuntimeError("Expected {0} arguments but got {1}", function.arity, argCount);
                return false;
            }

            if (frameCount == FramesMax) {
                RuntimeError("Stack overflow.");
                return false;
            }

            frames[frameCount++] = new CallFrame {
                function = function,
                ip = 0,
                slots = stack.Slice(stackTop - argCount - 1),
            };

            return true;
        }

        private bool IsFalsy(Value value)
        {
            return value.IsNil || (value.IsBool && !value.AsBool);
        }

        private byte ReadByte()
        {
            return frame.function.chunk.values[frame.ip++];
        }

        private Value ReadConstant()
        {
            return frame.function.chunk.GetConstant(ReadByte());
        }

        private ushort ReadShort()
        {
            frame.ip += 2;

            return (ushort)((frame.function.chunk.values[frame.ip - 2] << 8) | frame.function.chunk.values[frame.ip - 1]);
        }

        private ObjString ReadString()
        {
            return ReadConstant().AsString;
        }

        private void ResetStack()
        {
            stackTop = 0;
            frameCount = 0;
        }

        private void RuntimeError(string format, params object[] args)
        {
            CallFrame frame = frames[frameCount - 1];

            int line = frame.function.chunk.GetLine(frame.ip);

            writer.WriteLine(format, args);

            for (int i = frameCount - 1; i >= 0; i -= 1) {
                CallFrame f = frames[i];
                ObjFunction func = f.function;
                int instruction = frame.ip - 1;
                writer.WriteLine($"[line {func.chunk.GetLine(instruction)} in {(func.name == null ? "<script>" : func.name.Chars)}");
            }


            ResetStack();
        }

        private void DefineNative(string name, Func<int, Value[], Value> func)
        {
            Push(Value.Obj(ObjString.CopyString(name)));
            Push(Value.Obj(new ObjNative { Func = func }));
            globals[stack[0].AsString] = stack[1];
            Pop();
            Pop();
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

        private Value ClockNative(int argCount, Value[] args)
        {
            return Value.Number(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        }

        class CallFrame
        {
            public ObjFunction function;
            public int ip;
            public SliceableArray<Value> slots;
        }
    }

    public enum InterpretResult
    {
        OK,
        CompileError,
        RuntimeError,
    }
}
