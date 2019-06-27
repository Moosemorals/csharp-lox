﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace loxcli {
    class Parser {

        private readonly List<Token> Tokens;
        private int current = 0;

        public Parser(List<Token> tokens) {
            this.Tokens = tokens;
        }

        public List<Stmt> Parse() {
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd()) {
                statements.Add(Declaration());
            }
            return statements;
        }

        private Expr Expression() {
            return Assignment();
        }

        private Stmt Declaration() {
            try {
                if (Match(TokenType.VAR)) {
                    return VarDeclaration();
                }
                return Statement();
            } catch (ParseError) {
                Synchronise();
                return null;
            }
        }

        private Stmt Statement() {
            if (Match(TokenType.FOR)) {
                return ForStatement();
            }
            if (Match(TokenType.IF)) {
                return IfStatement();
            }
            if (Match(TokenType.PRINT)) {
                return PrintStatement(); 
            }
            if (Match(TokenType.WHILE)) {
                return WhileStatement();
            }
            if (Match(TokenType.LEFT_BRACE)) {
                return new Block(Block());
            }
            return ExpressionStatement();
        }

        private Stmt ForStatement() {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");

            Stmt initializer;
            if (Match(TokenType.SEMICOLON)) {
                initializer = null;
            } else if (Match(TokenType.VAR)) {
                initializer = VarDeclaration();
            } else {
                initializer = ExpressionStatement();
            }

            Expr condition;
            if (!Check(TokenType.SEMICOLON)) {
                condition = Expression();
            } else {
                condition = new Literal(true);
            }
            Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN)) {
                increment = Expression();
            }

            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            Stmt body = Statement();

            if (increment != null) {
                body = new Block(new List<Stmt> { body, new Expression(increment) });
            }

            body = new While(condition, body);

            if (initializer != null) {
                body = new Block(new List<Stmt> { initializer, body });
            }

            return body;
        }

        private Stmt IfStatement() {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();
            Stmt elseBranch = null;

            if (Match(TokenType.ELSE)) {
                elseBranch = Statement();
            }

            return new If(condition, thenBranch, elseBranch);
        }

        private Stmt PrintStatement() {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value");
            return new Print(value);
        }

        private Stmt VarDeclaration() {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name");

            Expr initializer = null;
            if (Match(TokenType.EQUAL)) {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

            return new Var(name, initializer);
        }

        private Stmt WhileStatement() {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new While(condition, body);
        }

        private Stmt ExpressionStatement() {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
            return new Expression(expr);
        }

        private List<Stmt> Block() {
            List<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd()) {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
            return statements;
        }

        private Expr Assignment() {
            Expr expr = Or();

            if (Match(TokenType.EQUAL)) {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr.GetType() == typeof(Variable)) {
                    Token name = ((Variable)expr).Name;
                    return new Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }
            return expr;
        }

        private Expr Or() {
            Expr expr = And();

            while (Match(TokenType.OR)) {
                Token op = Previous();
                Expr right = And();
                expr = new Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And() {
            Expr expr = Equality();

            while (Match(TokenType.AND)) {
                Token op = Previous();
                Expr right = Equality();
                expr = new Logical(expr, op, right);
            }

            return expr;
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

            return Call();
        }

        private Expr FinishCall(Expr callee) {
            List<Expr> arguments = new List<Expr>();

            if (!Check(TokenType.RIGHT_PAREN)) {
                do {
                    if (arguments.Count() >= 8) {
                        Error(Peek(), "Can't have more than 8 arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new Call(callee, paren, arguments);

        }

        private Expr Call() {
            Expr expr = Primary();

            while (true) {
                if (Match(TokenType.LEFT_PAREN)) {
                    expr = FinishCall(expr);
                } else {
                    break;
                }
            }

            return expr;
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
            if (Match(TokenType.IDENTIFIER)) {
                return new Variable(Previous());
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
