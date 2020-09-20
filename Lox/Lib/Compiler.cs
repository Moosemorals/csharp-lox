using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Security.Principal;
using System.Text;

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
                new ParseRule { Type = TokenType.Bang, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.BangEqual, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Class, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Comma, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Dot, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Else, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Eof, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Equal, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.EqualEqual, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Error, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.False, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.For, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Fun, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Greater, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.GreaterEqual, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Identifier, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.If, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.LeftBrace, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.LeftParen, Prefix = Grouping, Infix = null, Precidence=  Precidence.None },
                new ParseRule { Type = TokenType.Less, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.LessEqual, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Minus, Prefix = Unary, Infix = Binary, Precidence = Precidence.Term },
                new ParseRule { Type = TokenType.Nil, Prefix = null, Infix = null, Precidence = Precidence.None },
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
                new ParseRule { Type = TokenType.String, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.Super, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.This, Prefix = null, Infix = null, Precidence = Precidence.None },
                new ParseRule { Type = TokenType.True, Prefix = null, Infix = null, Precidence = Precidence.None },
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
            Expression();
            Consume(TokenType.Eof, "Expect end of expression.");

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

        private void Binary()
        {
            TokenType operatorType = previous.Type;

            ParseRule rule = GetRule(operatorType);
            ParsePrecidence(rule.Precidence + 1);

            switch (operatorType) {
                case TokenType.Plus: EmitByte(OpCode.Add); break;
                case TokenType.Minus: EmitByte(OpCode.Subtract); break;
                case TokenType.Star: EmitByte(OpCode.Multiply); break;
                case TokenType.Slash: EmitByte(OpCode.Divide); break;
                default:
                    throw new Exception("Unreachable code reached");
            }
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

        void ErrorAt(Token token, string message)
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

        private ParseRule GetRule(TokenType type)
        {
            return rules[(int)type];
        }

        private void Grouping()
        {
            Expression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
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

        private void Number()
        {
            Value v = new Value { V = double.Parse(previous.Lexeme) };
            EmitConstant(v);
        }

        private void ParsePrecidence(Precidence precidence)
        {
            Advance();
            Action prefixRule = GetRule(previous.Type).Prefix;
            if (prefixRule == null) {
                Error("Expect expression");
                return;
            }

            prefixRule();

            while (precidence < GetRule(current.Type).Precidence) {
                Advance();
                Action infixRule = GetRule(previous.Type).Infix;
                infixRule();
            }
        }

        private void Unary()
        {
            TokenType operatorType = previous.Type;

            ParsePrecidence(Precidence.Unary);

            switch (operatorType) {
                case TokenType.Minus: EmitByte(OpCode.Negate); break;
                default:
                    throw new Exception("Unreachable code reached");
            }
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
