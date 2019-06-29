using System.Collections.Generic;

namespace loxcli {
    internal class LoxInstance {
        private readonly LoxClass klass;
        private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

        internal LoxInstance(LoxClass loxClass) {
            this.klass = loxClass;
        }

        internal object Get(Token name) {
            if (fields.ContainsKey(name.lexeme)) {
                return fields[name.lexeme];
            }

            LoxFunction method = klass.FindMethod(name.lexeme);

            if (method != null) {
                return method.Bind(this);
            }

            throw new RuntimeError(name, "Undefined property '" + name.lexeme + "'.");
        }

        internal void Set(Token name, object value) {
            if (!fields.ContainsKey(name.lexeme)) {
                fields.Add(name.lexeme, value);
            } else {
                fields[name.lexeme] = value;
            }
        }

        public override string ToString() {
            return klass.name + " Instance";
        }
    }
}