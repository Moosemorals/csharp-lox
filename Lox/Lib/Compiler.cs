using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;
using System.Xml.Linq;

namespace Lox.Lib
{
    public class Compiler
    {
        private Token current;
        private Token previous;
        private Scanner scanner;
        private bool hadError;
        private bool panicMode;
        private TextWriter writer;
        private Chunk compilingChunk;

        private readonly ParseRule[] rules;

        public Compiler()
        {
            rules = new[] {
                new ParseRule { Type = TokenType.And, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Bang, Prefix = Unary, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.BangEqual, Prefix = null, Infix = Binary, Precidence = Precidence.Equality },
                new ParseRule { Type = TokenType.Class, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Comma, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Dot, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Else, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Eof, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Equal, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.EqualEqual, Prefix = null, Infix = Binary, Precidence = Precidence.Equality },
                new ParseRule { Type = TokenType.Error, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.False, Prefix = Literal, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.For, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Fun, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Greater, Prefix = null, Infix = Binary, Precidence = Precidence.Comparison },
                new ParseRule { Type = TokenType.GreaterEqual, Prefix = null, Infix = Binary, Precidence = Precidence.Comparison },
                new ParseRule { Type = TokenType.Identifier, Prefix = Variable, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.If, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.LeftBrace, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.LeftParen, Prefix = Grouping, Infix = null, Precidence=  Precidence.None },
                new ParseRule { Type = TokenType.Less, Prefix = null, Infix = Binary, Precidence = Precidence.Comparison },
                new ParseRule { Type = TokenType.LessEqual, Prefix = null, Infix = Binary, Precidence = Precidence.Comparison },
                new ParseRule { Type = TokenType.Minus, Prefix = Unary, Infix = Binary, Precidence = Precidence.Term },
                new ParseRule { Type = TokenType.Nil, Prefix = Literal, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Number, Prefix = Number, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Or, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Plus, Prefix = null, Infix = Binary, Precidence = Precidence.Term },
                new ParseRule { Type = TokenType.Print, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Return, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.RightBrace, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.RightParen, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Semicolon, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Slash, Prefix = null, Infix = Binary, Precidence = Precidence.Factor },
                new ParseRule { Type = TokenType.Star, Prefix = null, Infix = Binary, Precidence = Precidence.Factor },
                new ParseRule { Type = TokenType.String, Prefix = String, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Super, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.This, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.True, Prefix = Literal, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Var, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.While, Prefix = null, Infix = null, Precidence = Precidence.None },
            };
        }

        public bool Compile(TextWriter writer, string source, Chunk chunk)
        {

            this.writer = writer;
            hadError = false;
            scanner = new Scanner(source);
            compilingChunk = chunk;

            Advance();

            while (!Match(TokenType.Eof)) {
                Declaration();
            }

            EndCompiler();

            return !hadError;
        }

        private void Advance()
        {
            previous = current;
            while (true) {
                current = scanner.ScanToken();
                if (current.Type != TokenType.Error) {
                    break;
                }

                ErrorAtCurrent(current.Lexeme);
            }
        }

        private void Binary(bool canAssign)
        {
            TokenType operatorType = previous.Type;

            ParseRule rule = GetRule(operatorType);
            ParsePrecidence(rule.Precidence + 1);

            switch (operatorType) {
                case TokenType.BangEqual: EmitBytes(OpCode.Equal, (byte)OpCode.Not); break;
                case TokenType.EqualEqual: EmitByte(OpCode.Equal); break;
                case TokenType.Greater: EmitByte(OpCode.Greater); break;
                case TokenType.GreaterEqual: EmitBytes(OpCode.Less, (byte)OpCode.Not); break;
                case TokenType.Less: EmitByte(OpCode.Less); break;
                case TokenType.LessEqual: EmitBytes(OpCode.Greater, (byte)OpCode.Not); break;

                case TokenType.Plus: EmitByte(OpCode.Add); break;
                case TokenType.Minus: EmitByte(OpCode.Subtract); break;
                case TokenType.Star: EmitByte(OpCode.Multiply); break;
                case TokenType.Slash: EmitByte(OpCode.Divide); break;
                default:
                    throw new Exception("Unreachable code reached");
            }
        }

        private bool Check(TokenType type)
        {
            return current.Type == type;
        }

        private void Consume(TokenType type, string message)
        {
            if (current.Type == type) {
                Advance();
                return;
            }

            ErrorAtCurrent(message);
        }

        private Chunk CurrentChunk()
        {
            return compilingChunk;
        }

        private void Declaration()
        {
            if (Match(TokenType.Var)) {
                VarDeclaration();
            } else { 
                Statement();
            }

            if (panicMode) {
                Synchronize();
            }
        }

        private void DefineVariable(byte global)
        {
            EmitBytes(OpCode.DefineGlobal, global);
        }

        private void EmitByte(byte b)
        {
            CurrentChunk().Add(b, previous.Line);
        }

        private void EmitBytes(OpCode op, byte b2)
        {
            EmitByte((byte)op);
            EmitByte(b2);
        }

        private void EmitByte(OpCode op)
        {
            EmitByte((byte)op);
        }

        private void EmitConstant(Value value)
        {
            EmitBytes(OpCode.Constant, MakeConstant(value));
        }

        private void EmitReturn()
        {
            EmitByte(OpCode.Return);
        }

        private void EndCompiler()
        {
            EmitReturn();

#if DEBUG
            if (!hadError) {
                CurrentChunk().Disassemble(writer, "code");
            }
#endif
        }

        private void Error(string message)
        {
            ErrorAt(previous, message);
        }

        private void ErrorAt(Token token, string message)
        {
            if (panicMode) {
                return;
            }
            panicMode = true;

            writer.Write("[line {0} error", token.Line);

            if (token.Type == TokenType.Eof) {
                writer.Write(" at end");
            } else if (token.Type == TokenType.Error) {
                // Nothing
            } else {
                writer.Write(" at '{0}'", token.Lexeme);
            }

            writer.WriteLine(": {0}", message);
            hadError = true;

        }

        private void ErrorAtCurrent(string message)
        {
            ErrorAt(current, message);
        }

        private void Expression()
        {
            ParsePrecidence(Precidence.Assignment);
        }

        private void ExpressionStatement()
        {
            Expression();
            Consume(TokenType.Semicolon, "Expect ';' after expression.");
            EmitByte(OpCode.Pop);
        }

        private ParseRule GetRule(TokenType type)
        {
            return rules[(int)type];
        }

        private void Grouping(bool canAssign)
        {
            Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
        }

        private byte IdentifierConstant(Token name)
        {
            return MakeConstant(Value.Obj(ObjString.CopyString(name.Lexeme)));
        }

        private void Literal(bool canAssign)
        {
            switch (previous.Type) {
                case TokenType.False: EmitByte(OpCode.False); break;
                case TokenType.Nil: EmitByte(OpCode.Nil); break;
                case TokenType.True: EmitByte(OpCode.True); break;
                default:
                    throw new Exception("Unreachable code reached");
            }
        }

        private void NamedVariable(Token name, bool canAssign)
        {
            byte arg = IdentifierConstant(name);
            if (canAssign && Match(TokenType.Equal)) {
                Expression();
                EmitBytes(OpCode.SetGlobal, arg);
            } else {
                EmitBytes(OpCode.GetGlobal, arg);
            }
        }

        private byte MakeConstant(Value v)
        {
            int constant = CurrentChunk().AddConstant(v);
            if (constant > byte.MaxValue) {
                Error("Too many constants in one chunk");
                return 0;
            }

            return (byte)constant;
        }

        private bool Match(TokenType type)
        {
            if (!Check(type)) {
                return false;
            }
            Advance();
            return true;
        }

        private void Number(bool canAssign)
        {
            double value = double.Parse(previous.Lexeme);
            EmitConstant(Value.Number(value));
        }

        private void ParsePrecidence(Precidence precidence)
        {
            Advance();
            Action<bool> prefixRule = GetRule(previous.Type).Prefix;
            if (prefixRule == null) {
                Error("Expect expression");
                return;
            }

            bool canAssign = precidence <= Precidence.Assignment;
            prefixRule(canAssign);

            while (precidence < GetRule(current.Type).Precidence) {
                Advance();
                Action<bool> infixRule = GetRule(previous.Type).Infix;
                infixRule(canAssign);
            }

            if (canAssign && Match(TokenType.Equal)) {
                Error("Invalid assignment target.");
            }
        }

        private byte ParseVariable(string errorMessage)
        {
            Consume(TokenType.Identifier, errorMessage);
            return IdentifierConstant(previous);
        }

        private void PrintStatement()
        {
            Expression();
            Consume(TokenType.Semicolon, "Expect ';' after value.");
            EmitByte(OpCode.Print);
        }

        public void Statement()
        {
            if (Match(TokenType.Print)) {
                PrintStatement();
            } else {
                ExpressionStatement();
            }
        }

        private void String(bool canAssign)
        {
            EmitConstant(Value.Obj(ObjString.CopyString(previous.Lexeme[1..^1])));
        }

        private void Synchronize()
        {
            panicMode = false;

            while (current.Type != TokenType.Eof) {
                if (previous.Type == TokenType.Semicolon) {
                    return;
                }
                switch (current.Type) {
                    case TokenType.Class:
                    case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                    default:
                        // do nothing
                        break;
                }
                Advance();
            }
        }

        private void Unary(bool canAssign)
        {
            TokenType operatorType = previous.Type;

            ParsePrecidence(Precidence.Unary);

            switch (operatorType) {
                case TokenType.Bang: EmitByte(OpCode.Not); break;
                case TokenType.Minus: EmitByte(OpCode.Negate); break;
                default:
                    throw new Exception("Unreachable code reached");
            }
        }

        private void VarDeclaration()
        {
            byte global = ParseVariable("Expect variable name after var.");

            if (Match(TokenType.Equal)) {
                Expression();
            } else {
                EmitByte(OpCode.Nil);
            }

            Consume(TokenType.Semicolon, "Expect ';' after varible delclaration");
            DefineVariable(global);
        }

        private void Variable(bool canAssign)
        {
            NamedVariable(previous, canAssign);
        }
    }

    enum Precidence
    {
        None,
        Assignment,
        Or,
        And,
        Equality,
        Comparison,
        Term,
        Factor,
        Unary,
        Call,
        Primary,
    }
}
