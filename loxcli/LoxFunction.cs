using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace loxcli {
    public class LoxFunction : ILoxCallable {

        private readonly Function declaration;
        private readonly Environment closure;
        private readonly bool isInitializer;

        public LoxFunction(Function declaration, Environment closure, bool isInitializer) {
            this.closure = closure;
            this.declaration = declaration;
            this.isInitializer = isInitializer;
        }

        public int Arity() {
            return declaration.param.Count;
        }

        internal LoxFunction Bind(LoxInstance instance) {
            Environment environment = new Environment(closure);
            environment.Define("this", instance);
            return new LoxFunction(declaration, environment, isInitializer);
        }

        public object Call(Interpreter interpreter, List<object> arguments) {
            Environment environment = new Environment(closure);

            for (int i = 0; i < declaration.param.Count; i += 1) {
                environment.Define(declaration.param[i].lexeme, arguments[i]);
            }

            try {
                interpreter.ExecuteBlock(declaration.body, environment);
            } catch (ReturnEx returnValue) {
                if (isInitializer) {
                    return closure.GetAt(0, "this");
                }
                return returnValue.value;
            }

            if (isInitializer) {
                return closure.GetAt(0, "this");
            }
            return null;
        }

        public override string ToString() {
            return "<fn " + declaration.name.lexeme + ">";
        }
    }
}
