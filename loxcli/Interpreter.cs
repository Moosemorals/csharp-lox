using System;
using System.Collections.Generic;

namespace loxcli {
    public class Interpreter : IExprVisitor<object>, IStmtVisitor<object> {

        private class ClockFunc : ILoxCallable {

            private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            public int Arity() {
                return 0;
            }

            public object Call(Interpreter interpreter, List<object> arguments) {
                return (DateTime.UtcNow - EPOCH).TotalMilliseconds;
            }

            public override string ToString() {
                return "<native function>";
            }
        }

        internal readonly Environment globals = new Environment();
        private readonly Dictionary<Expr, int> locals = new Dictionary<Expr, int>();
        private Environment environment;

        public Interpreter() {
            environment = globals;
            globals.Define("clock", new ClockFunc());
        }

        public void Interpret(List<Stmt> statements) {
            try {
                foreach (Stmt s in statements) {
                    Execute(s);
                }
            } catch (RuntimeError error) {
                Lox.RuntimeError(error);
            }
        }

        public object VisitAsignExpr(Assign expr) {
            object value = Evaluate(expr.Value);

            if (locals.ContainsKey(expr)) {
                int distance = locals[expr];
                environment.AssignAt(distance, expr.Name, value);
            } else {
                globals.Assign(expr.Name, value);
            }

            return value;
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

        public object VisitCallExpr(Call expr) {
            object callee = Evaluate(expr.callee);

            List<object> arguments = new List<object>();
            foreach (Expr a in expr.arguments) {
                arguments.Add(Evaluate(a));
            }

            if (!(callee.GetType() != typeof(ILoxCallable))) {
                throw new RuntimeError(expr.paren, "Can only call functions and clases.");
            }

            ILoxCallable function = (ILoxCallable)callee;
            if (arguments.Count != function.Arity()) {
                throw new RuntimeError(expr.paren, "Expected " + function.Arity() + " arguments but got " + arguments.Count + ".");
            }

            return function.Call(this, arguments);
        }

        public object VisitGroupingExpr(Grouping expr) {
            return Evaluate(expr.Expression);
        }

        public object VisitGetExpr(Get expr) {
            object obj = Evaluate(expr.obj);
            if (obj.GetType() == typeof(LoxInstance)) {
                return ((LoxInstance)obj).Get(expr.name);
            }

            throw new RuntimeError(expr.name, "Only instances have properties.");
        }

        public object VisitLiteralExpr(Literal expr) {
            return expr.Value;
        }

        public object VisitLogicalExpr(Logical expr) {
            Object left = Evaluate(expr.left);

            if (expr.op.type == TokenType.OR) {
                if (IsTruthy(left)) {
                    return left;
                }
            } else {
                if (!IsTruthy(left)) {
                    return left;
                }
            }

            return Evaluate(expr.right);
        }

        public object VisitSetExpr(Set expr) {
            object obj = Evaluate(expr.obj);

            if (obj.GetType() != typeof(LoxInstance)) {
                throw new RuntimeError(expr.name, "Only instances have fields.");
            }

            object value = Evaluate(expr.value);
            ((LoxInstance)obj).Set(expr.name, value);
            return value;
        }

        public object VisitThisExpr(This expr) {
            return LookUpVariable(expr.keyword, expr);
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

        public object VisitVariableExpr(Variable expr) {
            return LookUpVariable(expr.Name, expr);
        }

        private object LookUpVariable(Token name, Expr expr) {
            if (locals.ContainsKey(expr)) {
                int distance = locals[expr];
                return environment.GetAt(distance, name.lexeme);
            } else {
                return globals.Get(name);
            }
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

        private void Execute(Stmt stmt) {
            stmt.Accept(this);
        }

        internal void Resolve(Expr expr, int depth) {
            locals.Add(expr, depth);
        }

        internal void ExecuteBlock(List<Stmt> statements, Environment environment) {
            Environment previous = this.environment;

            try {
                this.environment = environment;

                foreach (Stmt s in statements) {
                    Execute(s);
                }
            } finally {
                this.environment = previous;
            }
        }

        public object VisitBlockStmt(Block stmt) {
            ExecuteBlock(stmt.statements, new Environment(environment));
            return null;
        }

        public object VisitClassStmt(Class stmt) {
            environment.Define(stmt.name.lexeme, null);

            Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
            foreach (Function method in stmt.methods) {
                LoxFunction function = new LoxFunction(method, environment, method.name.lexeme.Equals("init"));
                methods.Add(method.name.lexeme, function);
            }

            LoxClass klass = new LoxClass(stmt.name.lexeme, methods);
            environment.Assign(stmt.name, klass);
            return null;
        }

        public object VisitExpressionStmt(Expression stmt) {
            Evaluate(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Function stmt) {
            LoxFunction function = new LoxFunction(stmt, environment, false);
            environment.Define(stmt.name.lexeme, function);
            return null;
        }

        public object VisitIfStmt(If stmt) {
            if (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.thenBranch);
            } else {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitPrintStmt(Print stmt) {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitReturnStmt(Return stmt) {
            object value = null;
            if (stmt.value != null) {
                value = Evaluate(stmt.value);
            }

            throw new ReturnEx(value);
        }

        public object VisitVarStmt(Var stmt) {
            object value = null;
            if (stmt.initializer != null) {
                value = Evaluate(stmt.initializer);
            }

            environment.Define(stmt.name.lexeme, value);
            return null;
        }

        public object VisitWhileStmt(While stmt) {
            while (IsTruthy(Evaluate(stmt.condition))) {
                Execute(stmt.body);
            }
            return null;
        }
    }

    class RuntimeError : Exception {
        public Token token;
        public RuntimeError(Token token, String message) : base(message) {
            this.token = token;
        }
    }

    class ReturnEx : Exception {
        internal readonly object value;

        internal ReturnEx(object value) : base() {
            this.value = value;
        }
    }
}
