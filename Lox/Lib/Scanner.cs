using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Lox.Lib
{
    public class Scanner
    {
        private int start;
        private int current;
        private int line;
        private readonly string source;

        public Scanner(string Source)
        {
            source = Source;
            start = 0;
            current = 0;
            line = 1;
        }
        public Token ScanToken()
        {
            SkipWhitespace();
            start = current;
            if (IsAtEnd()) {
                return MakeToken(TokenType.Eof);
            }

            char c = Advance();
            if (IsAlpha(c)) {
                return ScanIdentifier();
            }

            if (IsDigit(c)) {
                return ScanNumber();
            }

            return c switch
            {
                '(' => MakeToken(TokenType.LeftParen),
                ')' => MakeToken(TokenType.RightParen),
                '{' => MakeToken(TokenType.LeftBrace),
                '}' => MakeToken(TokenType.RightBrace),
                ';' => MakeToken(TokenType.Semicolon),
                ',' => MakeToken(TokenType.Comma),
                '.' => MakeToken(TokenType.Dot),
                '-' => MakeToken(TokenType.Minus),
                '+' => MakeToken(TokenType.Plus),
                '/' => MakeToken(TokenType.Slash),
                '*' => MakeToken(TokenType.Star),
                '!' => MakeToken(Match('=') ? TokenType.BangEqual : TokenType.Bang),
                '=' => MakeToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal),
                '<' => MakeToken(Match('=') ? TokenType.LessEqual : TokenType.Less),
                '>' => MakeToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater),
                '"' => ScanString(),
                _ => ErrorToken("Unexepected character."),
            };
        }

        private char Advance()
        {
            current += 1;
            return source[current - 1];
        }

        private TokenType CheckKeyword(int s, int l, string rest, TokenType type)
        {
            if (current - start == s + l && source.Substring(start + s, l) == rest) {
                return type;
            }

            return TokenType.Identifier;
        }

        private TokenType IdentifierType()
        {
            switch (source[start]) {
                case 'a': return CheckKeyword(1, 2, "nd", TokenType.And);
                case 'c': return CheckKeyword(1, 4, "lass", TokenType.Class);
                case 'e': return CheckKeyword(1, 3, "lse", TokenType.Else);
                case 'f':
                    if (current - start > 1) {
                        switch (source[start + 1]) {
                            case 'a': return CheckKeyword(2, 3, "lse", TokenType.False);
                            case 'o': return CheckKeyword(2, 1, "r", TokenType.For);
                            case 'u': return CheckKeyword(2, 1, "n", TokenType.Fun);
                        }
                    }
                    break;
                case 'i': return CheckKeyword(1, 1, "f", TokenType.If);
                case 'n': return CheckKeyword(1, 2, "il", TokenType.Nil);
                case 'o': return CheckKeyword(1, 1, "r", TokenType.Or);
                case 'p': return CheckKeyword(1, 4, "rint", TokenType.Print);
                case 'r': return CheckKeyword(1, 5, "eturn", TokenType.Return);
                case 's': return CheckKeyword(1, 4, "uper", TokenType.Super);
                case 't':
                    if (current - start > 1) {
                        switch (source[start + 1]) {
                            case 'h': return CheckKeyword(2, 2, "is", TokenType.This);
                            case 'r': return CheckKeyword(2, 2, "ue", TokenType.True);
                        }
                    }
                    break; 
                case 'v': return CheckKeyword(1, 2, "ar", TokenType.Var);
                case 'w': return CheckKeyword(1, 4, "hile", TokenType.While);
            }
            return TokenType.Identifier;
        }


        private bool IsAlpha(char c) => (c >= 'a' && c <= 'z')
                                     || (c >= 'A' && c <= 'Z')
                                     || c == '_';

        private bool IsAtEnd() => current >= source.Length;

        private bool IsDigit(char c) => c >= '0' && c <= '9';

        private bool Match(char expected)
        {
            if (IsAtEnd()) {
                return false;
            }
            if (source[current] != expected) {
                return false;
            }
            current += 1;
            return true;
        }

        private char Peek()
        {
            if (IsAtEnd()) {
                return '\0';
            }
            return source[current];
        }

        private char PeekNext()
        {
            if (IsAtEnd()) {
                return '\0';
            }
            return source[current + 1];
        }

        private Token ScanIdentifier()
        {
            while (IsAlpha(Peek()) || IsDigit(Peek())) {
                Advance();
            }

            return MakeToken(IdentifierType());
        }

        private Token ScanNumber()
        {
            while (IsDigit(Peek())) {
                Advance();
            }

            if (Peek() == '.' && IsDigit(PeekNext())) {
                Advance();
                while (IsDigit(Peek())) {
                    Advance();
                }
            }

            return MakeToken(TokenType.Number);
        }

        private Token ScanString()
        {
            while (Peek() != '"' && !IsAtEnd()) {
                if (Peek() == '\n') {
                    line += 1;
                }
                Advance();
            }
            if (IsAtEnd()) {
                return ErrorToken("Unterminated string.");
            }

            // Eat the closing quote
            Advance();
            return MakeToken(TokenType.String);
        }

        private void SkipWhitespace()
        {
            while (true) {
                char c = Peek();
                switch (c) {
                    case ' ':
                    case '\r':
                    case '\t':
                        Advance();
                        break;
                    case '\n':
                        line += 1; ;
                        Advance();
                        break;
                    case '/':
                        if (PeekNext() == '/') {
                            while (Peek() != '\n' && !IsAtEnd()) {
                                Advance();
                            }
                        } else {
                            return;
                        }
                        break;
                    default:
                        return;
                }
            }
        }



        private Token MakeToken(TokenType type)
        {
            return new Token {
                Type = type,
                Start = start,
                Length = current - start,
                Line = line,
                Lexeme = source[start..current],
            };
        }

        private Token ErrorToken(string message)
        {
            return new Token {
                Type = TokenType.Error,
                Start = -1,
                Length = message.Length,
                Line = line,
                Lexeme = message,
            };
        }


    }
}
