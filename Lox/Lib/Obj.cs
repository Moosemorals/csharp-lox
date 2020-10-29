using System;
using System.Collections.Generic;
using System.Text;

namespace Lox.Lib
{
    public abstract class Obj
    {
        public Obj(ObjType type)
        {
            Type = type;
        }
        public virtual ObjType Type { get; private set; }
    }

    public class ObjString : Obj
    {
        public ObjString() : base(ObjType.String) { }

        public string Chars;
        public int Length => Chars.Length;

        public static ObjString CopyString(string src)
        {
            return new ObjString {
                Chars = new string(src),
            };
        }

        public override string ToString()
        {
            return Chars;
        }
    }

    public class ObjFunction : Obj
    {
        public ObjFunction() : base(ObjType.Function)
        {
            arity = 0;
            name = null;
            chunk = new Chunk();
        }

        public int arity;

        public Chunk chunk;

        public ObjString name;

        public override string ToString()
        {
            return $"<fn {(name == null ? "<script>" : name.Chars)}>";
        }
    }

    public class ObjNative : Obj
    {
        public ObjNative() : base(ObjType.Native) { }

        public Func<int, Value[], Value> Func {get;set;}

        public override string ToString()
        {
            return $"<native fn>";
        }
    }

    public enum ObjType
    {
        Function,
        String,
        Native,
    }
}
