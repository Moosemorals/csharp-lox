using System;
using System.Text;

namespace loxcli {
    class ASTPrinter : IExprVisitor<String> {

        public string Print(Expr expr) {
            return expr.Accept(this);
        }

        public string VisitAsignExpr(Assign expr) {
            throw new NotImplementedException();
        }

        public string VisitBinaryExpr(Binary expr) {
            return Parenthesize(expr.Operator.lexeme, expr.Left, expr.Right);
        }

        public string VisitCallExpr(Call expr) {
            return Parenthesize("call", expr.arguments.ToArray());
        }

        public string VisitGroupingExpr(Grouping expr) {
            return Parenthesize("group", expr.Expression);
        }


        public string VisitLiteralExpr(Literal expr) {
            if (expr.Value == null) {
                return "nil";
            }
            return expr.Value.ToString();
        }

        public string VisitLogicalExpr(Logical expr) {
            return Parenthesize(expr.op.lexeme, expr.left, expr.right);
        }

        public string VisitUnaryExpr(Unary expr) {
            return Parenthesize(expr.Operator.lexeme, expr.Right);
        }

        public string VisitVariableExpr(Variable expr) {
            return expr.Name.lexeme;
        }

        private string Parenthesize(string name, params Expr[] terms) {
            StringBuilder result = new StringBuilder();

            result.Append("(").Append(name);
            foreach (Expr expr in terms) {
                result.Append(" ");
                result.Append(expr.Accept(this));
            }
            result.Append(")");

            return result.ToString();
        }
    }
}
