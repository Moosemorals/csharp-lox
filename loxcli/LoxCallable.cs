using System.Collections.Generic;

namespace loxcli {
    public interface ILoxCallable {
        int Arity();
        object Call(Interpreter interpreter, List<object> arguments);
        
    }
}