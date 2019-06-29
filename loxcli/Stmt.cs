using System.Collections.Generic;

namespace loxcli {
    public abstract class Stmt {
        public abstract R Accept<R>(IStmtVisitor<R> visitor);
    }

    public interface IStmtVisitor<R> {
        R VisitBlockStmt(Block Stmt);
        R VisitClassStmt(Class Stmt);
        R VisitExpressionStmt(Expression stmt);
        R VisitFunctionStmt(Function function);
        R VisitIfStmt(If stmt);
        R VisitPrintStmt(Print stmt);
        R VisitReturnStmt(Return @return);
        R VisitWhileStmt(While stmt);
        R VisitVarStmt(Var stmt);
    }

    public class Block : Stmt {
        public List<Stmt> statements;

        public Block(List<Stmt> statements) {
            this.statements = statements;
        }

        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitBlockStmt(this);
        }
    }

    public class Class : Stmt {
        public Token name;
        public List<Function> methods;

        public Class(Token name, List<Function> methods) {
            this.name = name;
            this.methods = methods;
        }
        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitClassStmt(this);
        }
    }

    public class Expression : Stmt {
        public Expr expression;

        public Expression(Expr expression) {
            this.expression = expression;
        }

        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitExpressionStmt(this);
        }
    }

    public class Function : Stmt {
        public Token name;
        public List<Token> param;
        public List<Stmt> body;

        public Function(Token name, List<Token> param, List<Stmt> body) {
            this.name = name;
            this.param = param;
            this.body = body;
        }

        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitFunctionStmt(this);
        }
    }

    public class If : Stmt {
        public Expr condition;
        public Stmt thenBranch;
        public Stmt elseBranch;

        public If(Expr condition, Stmt thenBranch, Stmt elseBranch) {
            this.condition = condition;
            this.thenBranch = thenBranch;
            this.elseBranch = elseBranch;
        }

        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitIfStmt(this);
        }
    }

    public class Print : Stmt {
        public Expr expression;

        public Print(Expr expression) {
            this.expression = expression;
        }

        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitPrintStmt(this);
        }
    }

    public class Return : Stmt {
        public Token keyword;
        public Expr value;

        public Return(Token keyword, Expr value) {
            this.keyword = keyword;
            this.value = value;
        }

        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitReturnStmt(this);
        }
    }

    public class Var : Stmt {
        public Token name;
        public Expr initializer;

        public Var(Token name, Expr initializer) {
            this.name = name;
            this.initializer = initializer;
        }

        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitVarStmt(this);
        }
    }

    public class While : Stmt {
        public Expr condition;
        public Stmt body;

        public While(Expr condition, Stmt body) {
            this.condition = condition;
            this.body = body;
        }

        public override R Accept<R>(IStmtVisitor<R> visitor) {
            return visitor.VisitWhileStmt(this);
        }
    }
}
