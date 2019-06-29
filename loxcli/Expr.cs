﻿using System.Collections.Generic;

namespace loxcli {
    public abstract class Expr {
        public abstract R Accept<R>(IExprVisitor<R> visitor);
    }

    public interface IExprVisitor<R> {
        R VisitAsignExpr(Assign expr);
        R VisitBinaryExpr(Binary expr);
        R VisitCallExpr(Call expr);
        R VisitGetExpr(Get expr);
        R VisitGroupingExpr(Grouping expr);
        R VisitLiteralExpr(Literal expr);
        R VisitLogicalExpr(Logical expr);
        R VisitSetExpr(Set expr);
        R VisitThisExpr(This @this);
        R VisitUnaryExpr(Unary expr);
        R VisitVariableExpr(Variable expr);
    }

    public class Assign : Expr {
        public Token Name;
        public Expr Value;

        public Assign(Token name, Expr value) {
            Name = name;
            Value = value;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitAsignExpr(this);
        }
    }

    public class Binary : Expr {
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

    public class Call : Expr {
        public Expr callee;
        public Token paren;
        public List<Expr> arguments;

        public Call(Expr callee, Token paren, List<Expr> arguments) {
            this.callee = callee;
            this.paren = paren;
            this.arguments = arguments;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitCallExpr(this);
        }
    }

    public class Get : Expr {
        public Expr obj;
        public Token name;

        public Get(Expr obj, Token name) {
            this.obj = obj;
            this.name = name;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitGetExpr(this);
        }
    }

    public class Grouping : Expr {
        public Expr Expression;

        public Grouping(Expr expression) {
            Expression = expression;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitGroupingExpr(this);
        }
    }

    public class Literal : Expr {
        public object Value;

        public Literal(object value) {
            Value = value;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitLiteralExpr(this);
        }
    }

    public class Logical : Expr {
        public Expr left;
        public Token op;
        public Expr right;

        public Logical(Expr left, Token op, Expr right) {
            this.left = left;
            this.op = op;
            this.right = right;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitLogicalExpr(this);
        }
    }

    public class Set : Expr {
        public Expr obj;
        public Token name;
        public Expr value;

        public Set(Expr obj, Token name, Expr value) {
            this.obj = obj;
            this.name = name;
            this.value = value;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitSetExpr(this);
        }
    }

    public class This : Expr {
        public Token keyword;

        public This(Token keyword) {
            this.keyword = keyword;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitThisExpr(this);
        }
    }

    public class Unary : Expr {
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

    public class Variable : Expr {
        public Token Name;

        public Variable(Token name) {
            Name = name;
        }

        public override R Accept<R>(IExprVisitor<R> visitor) {
            return visitor.VisitVariableExpr(this);
        }
    }

}
