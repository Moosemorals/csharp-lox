using System;
using System.Collections.Generic;

namespace loxcli {
    public class Environment {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();
        public readonly Environment enclosing;

        public Environment() {
            enclosing = null;
        }

        public Environment(Environment enclosing) {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value) {
            values.Add(name, value);
        }

        internal Environment Ancestor(int distance) {
            Environment environment = this;
            for (int i = 0; i < distance; i += 1) {
                environment = environment.enclosing;
            }
            return environment;
        }

        public object Get(Token name) {
            if (values.ContainsKey(name.lexeme)) {
                return values[name.lexeme];
            }

            if (enclosing != null) {
                return enclosing.Get(name);
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'.");
        }

        internal object GetAt(int distance, string name) {
            return Ancestor(distance).values[name];
        }

        public void Assign(Token name, Object value) {
            if (values.ContainsKey(name.lexeme)) {
                values[name.lexeme] = value;
                return;
            }

            if (enclosing != null) {
                enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, "Undefined variable '" + name.lexeme + "'");
        }

        public void AssignAt(int distance, Token name, object value) {
            Ancestor(distance).values[name.lexeme] = value;
        }


    }
}
