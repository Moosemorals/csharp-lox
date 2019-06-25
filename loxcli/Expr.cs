using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace loxcli {
    abstract class Expr {

        public abstract R Accept<R>(IExprVisitor<R> visitor);
    }

    interface IExprVisitor<R> {
        R VisitAsignExpr(Assign expr);
        R VisitBinaryExpr(Binary expr);
        R VisitGroupingExpr(Grouping expr);
        R VisitLiteralExpr(Literal expr);
        R VisitUnaryExpr(Unary expr);
    }



    class Assign : Expr {
        public Token Token;
        public Expr Value;

        public Assign(Token token, Expr value) {
            Token = token;
            Value = value;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitAsignExpr(this);
        }
    }

    class Binary : Expr {
        public Expr Left;
        public Token Operator;
        public Expr Right;

        public Binary(Expr left, Token @operator, Expr right) {
            Left = left;
            Operator = @operator;
            Right = right;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitBinaryExpr(this);
        }
    }

    class Grouping : Expr {
        public Expr Expression;

        public Grouping(Expr expression) {
            Expression = expression;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitGroupingExpr(this);
        }
    }

    class Literal : Expr {
        public object Value;

        public Literal(object value) {
            Value = value;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitLiteralExpr(this);
        }
    }

    class Unary : Expr {
        public Token Operator;
        public Expr Right;

        public Unary(Token @operator, Expr right) {
            Operator = @operator;
            Right = right;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitUnaryExpr(this);
        }
    }

}
