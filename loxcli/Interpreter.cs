using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace loxcli {
    class Interpreter : IExprVisitor<object> {

        public void Interpret(Expr expression) {
            try {
                object value = Evaluate(expression);
                Console.WriteLine(Stringify(value));
            } catch(RuntimeError error) {
                LoxCli.RuntimeError(error);
            }
        }

        public object VisitAsignExpr(Assign expr) {
            throw new NotImplementedException();
        }

        public object VisitBinaryExpr(Binary expr) {
            object left = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);

            switch (expr.Operator.type) {
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.GREATER:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left <= (double)right;                
                case TokenType.MINUS:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if (left.GetType() == typeof(double) && right.GetType() == typeof(double)) {
                        return (double)left + (double)right;
                    }
                    if (left.GetType() == typeof(string) && right.GetType() == typeof(string)) {
                        return (string)left + (string)right;
                    }

                    throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings");
                case TokenType.SLASH:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.Operator, left, right);
                    return (double)left * (double)right;
            }

            throw new Exception("Reached unreachable code.");
        }

        public object VisitGroupingExpr(Grouping expr) {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Literal expr) {
            return expr.Value;
        }

        public object VisitUnaryExpr(Unary expr) {
            object right = Evaluate(expr.Right);

            switch (expr.Operator.type) {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.Operator, right);
                    return -(double)right;
            }

            throw new Exception("Hit un-reachable code");
        }

        private void CheckNumberOperand(Token op, Object operand) {
            if (operand.GetType() == typeof(double)) {
                return;
            }
            throw new RuntimeError(op, "Operand must be a number");
        }

        private void CheckNumberOperands(Token op, object left, object right) {
            if (left.GetType() == typeof(double) && right.GetType() == typeof(double)) {
                return;
            }

            throw new RuntimeError(op, "Operands must be numbers");
        }

        private bool IsTruthy(object obj) {
            if (obj == null) return false;
            if (obj.GetType() == typeof(bool)) {
                return (bool)obj;
            }
            return true;
        }

        private bool IsEqual(object a, object b) {
            if (a == null && b == null) {
                return true;
            }
            if (a == null) {
                return false;
            }

            return a.Equals(b);
        }

        private string Stringify(object obj) {
            if (obj == null) {
                return "nil";
            }

            return obj.ToString();
        }

        private object Evaluate(Expr expr) {
            return expr.Accept(this);
        }
    }

    class RuntimeError : Exception {
        public Token token;
        public RuntimeError(Token token, String message) : base(message) {
            this.token = token;
        }
    }
}
