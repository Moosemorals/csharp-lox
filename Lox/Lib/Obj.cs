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
                Chars = src
            };
        }

        public override string ToString()
        {
            return Chars;
        }
    }

    public enum ObjType
    {
        String,
    }
}
