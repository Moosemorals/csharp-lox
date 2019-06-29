using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace loxcli {
    internal class LoxClass : ILoxCallable {
        internal readonly string name;
        private readonly Dictionary<string, LoxFunction> methods;

        internal LoxClass(string name, Dictionary<string, LoxFunction> methods) {
            this.name = name;
            this.methods = methods;
        }

        public int Arity() {
            LoxFunction initilizer = FindMethod("init");
            if (initilizer == null) {
                return 0;
            }
            return initilizer.Arity();
        }

        public object Call(Interpreter interpreter, List<object> arguments) {
            LoxInstance instance = new LoxInstance(this);
            LoxFunction initilizer = FindMethod("init");
            if (initilizer != null) {
                initilizer.Bind(instance).Call(interpreter, arguments);
            }
            return instance;
        }

        internal LoxFunction FindMethod(string name) {
            if (methods.ContainsKey(name)) {
                return methods[name];
            }
            return null;
        }

        public override string ToString() {
            return name;
        }
    }
}
