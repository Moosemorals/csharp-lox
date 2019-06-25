using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace loxcli {
    class Parser {

        private readonly List<Token> Tokens;
        private int current = 0;

        public Parser(List<Token> tokens) {
            this.Tokens = tokens;
        }

        public Expr Parse() {
            try {
                return Expression();
            } catch (ParseError) {
                return null;
            }
        }


        private Expr Expression() {
            return Equality();
        }

        private Expr Equality() {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL)) {
                Token op = Previous();
                Expr right = Comparison();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Comparison() {
            Expr expr = Addition();

            while (Match(TokenType.GREATER_EQUAL, TokenType.GREATER, TokenType.LESS, TokenType.LESS_EQUAL)) {
                Token op = Previous();
                Expr right = Addition();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }
        private Expr Addition() {
            Expr expr = Multiplication();

            while (Match(TokenType.MINUS, TokenType.PLUS)) {
                Token op = Previous();
                Expr right = Multiplication();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Multiplication() {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR)) {
                Token op = Previous();
                Expr right = Unary();
                expr = new Binary(expr, op, right);
            }
            return expr;
        }

        private Expr Unary() {
            if (Match(TokenType.BANG, TokenType.MINUS)) {
                Token op = Previous();
                Expr right = Unary();
                return new Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary() {
            if (Match(TokenType.FALSE)) {

                return new Literal(false);
            }
            if (Match(TokenType.TRUE)) {

                return new Literal(true);
            }
            if (Match(TokenType.NIL)) {

                return new Literal(null);
            }
            if (Match(TokenType.STRING, TokenType.NUMBER)) {

                return new Literal(Previous().literal);
            }
            if (Match(TokenType.LEFT_PAREN)) {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
                return new Grouping(expr);

            }
            throw Error(Peek(), "Expect expression");
        }


        private bool Match(params TokenType[] types) {
            foreach (TokenType type in types) {
                if (Check(type)) {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private Token Consume(TokenType type, String message) {
            if (Check(type)) {
                return Advance();
            }

            throw Error(Peek(), message);
        }

        private bool Check(TokenType type) {
            if (IsAtEnd()) {
                return false;
            }
            return Peek().type == type;
        }

        private Token Advance() {
            if (!IsAtEnd()) {
                current++;
            }
            return Previous();
        }

        private bool IsAtEnd() {
            return Peek().type == TokenType.EOF;
        }

        private Token Peek() {
            return Tokens[current];
        }

        private Token Previous() {
            return Tokens[current - 1];
        }

        private ParseError Error(Token token, String message) {
            LoxCli.Error(token, message);
            return new ParseError();
        }

        private void Synchronise() {
            Advance();

            while (!IsAtEnd()) {
                if (Previous().type == TokenType.SEMICOLON) {
                    return;
                }

                switch (Peek().type) {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }
    }

    class ParseError : Exception { }
}
