using System;
using System.Collections.Generic;

namespace loxcli {
    class Scanner {

        private static readonly Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType> {
            { "and", TokenType.AND },
            { "class", TokenType.CLASS },
            { "else", TokenType.ELSE },
            { "false", TokenType.FALSE },
            { "for", TokenType.FOR },
            { "fun", TokenType.FUN },
            { "if", TokenType.IF },
            { "nil", TokenType.NIL },
            { "or", TokenType.OR },
            { "print", TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super", TokenType.SUPER },
            { "this", TokenType.THIS },
            { "true", TokenType.TRUE },
            { "var", TokenType.VAR },
            { "while", TokenType.WHILE },
        };

        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public Scanner(string src) {
            this.source = src;
        }

        public List<Token> ScanTokens() {
            while (!IsAtEnd()) {
                start = current;
                ScanToken();
            }

            tokens.Add(new Token(TokenType.EOF, "", null, line));
            return tokens;
        }

        private void ScanToken() {
            char c = Advance();
            switch (c) {
                case '&':
                    if (Match('&')) {
                        AddToken(TokenType.AND);
                    } else {
                        Lox.Error(line, "Unexpected character");
                    }
                    break;
                case '|':
                    if (Match('|')) {
                        AddToken(TokenType.OR);
                    } else {
                        Lox.Error(line, "Unexpected character");
                    }
                    break;
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
                case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
                case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
                case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
                case '/':
                    if (Match('/')) {
                        //Comments run to the end of the line
                        while (Peek() != '\n' && !IsAtEnd()) {
                            Advance();
                        }
                    } else {
                        AddToken(TokenType.SLASH);
                    }
                    break;
                case ' ':
                case '\r':
                case '\t':
                    // Ignore whitespace
                    break;
                case '\n':
                    line += 1;
                    break;
                case '"': ParseString(); break;
                default:
                    if (IsDigit(c)) {
                        ParseNumber();
                    } else if (IsAlpha(c)) {
                        ParseIdentifier();
                    } else {
                        Lox.Error(line, "Unexpected character");
                    }
                    break;
            }
        }

        private void ParseIdentifier() {
            while (IsAlphaNumeric(Peek())) {
                Advance();
            }

            string word = source.Substring(start, current - start);
            TokenType type = keywords.ContainsKey(word) ? type = keywords[word] : TokenType.IDENTIFIER;
            AddToken(type);
        }

        private void ParseNumber() {
            while (IsDigit(Peek())) {
                Advance();
            }

            // Look for fractional part
            if (Peek() == '.' && IsDigit(PeekNext())) {
                //Consume the '.'
                Advance();

                while (IsDigit(Peek())) {
                    Advance();
                }
            }

            AddToken(TokenType.NUMBER, Double.Parse(source.Substring(start, current - start)));
        }

        private void ParseString() {
            while (Peek() != '"' && !IsAtEnd()) {
                if (Peek() == '\n') {
                    line += 1;
                }
                Advance();
            }

            // Handle unterminated strings
            if (IsAtEnd()) {
                Lox.Error(line, "Unterminated string");
                return;
            }

            // Handle closing quote
            Advance();

            // Trim the quotes and store the string
            string value = source.Substring(start + 1, current - start - 2);
            AddToken(TokenType.STRING, value);
        }

        private bool Match(char expected) {
            if (IsAtEnd()) { return false; }
            if (source[current] != expected) { return false; }

            current += 1;
            return true;
        }

        private char Peek() {
            if (IsAtEnd()) {
                return '\0';
            }
            return source[current];
        }

        private char PeekNext() {
            if (current + 1 >= source.Length) {
                return '\0';
            }
            return source[current + 1];
        }

        private bool IsAlpha(char c) {
            return (c >= 'a' && c <= 'z') ||
                (c >= 'A' && c <= 'Z') ||
                (c == '_');
        }

        private bool IsAlphaNumeric(char c) {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsDigit(char c) {
            return c >= '0' && c <= '9';
        }

        private bool IsAtEnd() {
            return current >= source.Length;
        }

        private char Advance() {
            current++;
            return source[current - 1];
        }

        private void AddToken(TokenType type) {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal) {
            string lexeme = source.Substring(start, current - start);
            tokens.Add(new Token(type, lexeme, literal, line));
        }
    }
}
