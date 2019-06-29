using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace loxcli {
    internal class Resolver : IExprVisitor<object>, IStmtVisitor<object> {

        private readonly Interpreter interpreter;
        private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType currentFunction = FunctionType.NONE;
        private ClassType currentClass = ClassType.NONE;

        internal Resolver(Interpreter interpreter) {
            this.interpreter = interpreter;
        }

        internal void Resolve(List<Stmt> statements) {
            foreach (Stmt s in statements) {
                Resolve(s);
            }
        }

        private void Resolve(Stmt stmt) {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr) {
            expr.Accept(this);
        }

        private void ResolveFunction(Function function, FunctionType type) {
            FunctionType enclosingType = currentFunction;
            currentFunction = type;


            BeginScope();
            foreach (Token p in function.param) {
                Declare(p);
                Define(p);
            }
            Resolve(function.body);
            EndScope();
            currentFunction = enclosingType;
        }

        private void BeginScope() {
            scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope() {
            scopes.Pop();
        }

        private void Declare(Token name) {
            if (scopes.Count == 0) {
                return;
            }
            Dictionary<string, bool> scope = scopes.Peek();
            if (scope.ContainsKey(name.lexeme)) {
                Lox.Error(name, "Variable with this name already exists in this scope");
            }
            scope.Add(name.lexeme, false);
        }

        private void Define(Token name) {
            if (scopes.Count == 0) {
                return;
            }
            scopes.Peek()[name.lexeme] = true;
        }

        private void ResolveLocal(Expr expr, Token name) {
            // The c# stack is the wrong way up!
            // Elements are added at the zero index, and everything
            // moves up by one! Seriously!
            for (int i = 0; i < scopes.Count; i += 1) {
                if (scopes.ElementAt(i).ContainsKey(name.lexeme)) {
                    interpreter.Resolve(expr, i);
                    return;
                }
            }
        }

        public object VisitAsignExpr(Assign expr) {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitBinaryExpr(Binary expr) {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object VisitBlockStmt(Block Stmt) {
            BeginScope();
            Resolve(Stmt.statements);
            EndScope();
            return null;
        }

        public object VisitCallExpr(Call expr) {
            Resolve(expr.callee);

            foreach (Expr arg in expr.arguments) {
                Resolve(arg);
            }
            return null;
        }

        public object VisitClassStmt(Class stmt) {
            ClassType enclosingClass = currentClass;
            currentClass = ClassType.CLASS;


            Declare(stmt.name);
            Define(stmt.name);

            BeginScope();
            scopes.Peek().Add("this", true);

            foreach (Function method in stmt.methods) {
                FunctionType decl = FunctionType.METHOD;
                if (method.name.lexeme.Equals("init")) {
                    decl = FunctionType.INITIALIZER;
                }
                ResolveFunction(method, decl);
            }

            EndScope();

            currentClass = enclosingClass;
            return null;
        }

        public object VisitExpressionStmt(Expression stmt) {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Function stmt) {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);
            return null;
        }

        public object VisitGetExpr(Get expr) {
            Resolve(expr.obj);
            return null;
        }

        public object VisitGroupingExpr(Grouping expr) {
            Resolve(expr.Expression);
            return null;
        }

        public object VisitIfStmt(If stmt) {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) {
                Resolve(stmt.elseBranch);
            }
            return null;
        }

        public object VisitLiteralExpr(Literal expr) {
            return null;
        }

        public object VisitLogicalExpr(Logical expr) {
            Resolve(expr.left);
            Resolve(expr.right);
            return null;
        }

        public object VisitPrintStmt(Print stmt) {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Return stmt) {
            if (currentFunction == FunctionType.NONE) {
                Lox.Error(stmt.keyword, "Cannot return from top-level code.");
            }

            if (stmt.value != null) {
                if (currentFunction == FunctionType.INITIALIZER) {
                    Lox.Error(stmt.keyword, "Can't return a value from an initializer.");
                }
                Resolve(stmt.value);
            }
            return null;
        }

        public object VisitSetExpr(Set expr) {
            Resolve(expr.value);
            Resolve(expr.obj);
            return null;
        }

        public object VisitThisExpr(This expr) {
            if (currentClass == ClassType.NONE) {
                Lox.Error(expr.keyword, "Can't use this outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.keyword);
            return null;
        }

        public object VisitUnaryExpr(Unary expr) {
            Resolve(expr.Right);
            return null;
        }

        public object VisitVariableExpr(Variable expr) {
            if (scopes.Count > 0 && !scopes.Peek()[expr.Name.lexeme]) {
                Lox.Error(expr.Name, "Can't read local variable in it's own initializer.");
            }

            ResolveLocal(expr, expr.Name);
            return null;
        }

        public object VisitVarStmt(Var stmt) {
            Declare(stmt.name);
            if (stmt.initializer != null) {
                Resolve(stmt.initializer);
            }
            Define(stmt.name);
            return null;
        }

        public object VisitWhileStmt(While stmt) {
            Resolve(stmt.condition);
            Resolve(stmt.body);
            return null;
        }

        private enum FunctionType {
            NONE,
            FUNCTION,
            INITIALIZER,
            METHOD
        }

        private enum ClassType {
            NONE,
            CLASS
        }

    }
}
