using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Lox.Lib
{


    public class Value
    {
        public ValueType Type { get; private set; }
        private object As { get; set; }

        public static Value Number(double n)
        {
            return new Value {
                Type = ValueType.Number,
                As = n
            };
        }

        public static Value Bool(bool b)
        {
            return new Value {
                Type = ValueType.Bool,
                As = b
            };
        }

        public static Value Nil = new Value { Type = ValueType.Nil };

        public static Value Obj(Obj o)
        {
            return new Value {
                Type = ValueType.Obj,
                As = o,
            };
        }

        public override string ToString()
        {
            if (IsObj) {
                return AsObj.ToString();
            }
            return As != null ? As.ToString() : "Nil";
        }

        public bool IsEqual(Value b)
        {
            if (Type != b.Type) {
                return false;
            }

            return Type switch
            {
                ValueType.Bool => AsBool == b.AsBool,
                ValueType.Nil => true,
                ValueType.Number => AsNumber == b.AsNumber,
                ValueType.Obj => AsString.Chars.Equals(b.AsString.Chars),
                _ => throw new Exception("Unreachable code reached"),
            };
        }

        public double AsNumber => (double)As;
        public bool AsBool => (bool)As;
        public Obj AsObj => (Obj)As;
        public ObjType ObjType => AsObj.Type;
        public ObjString AsString => (ObjString)As;
 
        public bool IsNumber => Type == ValueType.Number;
        public bool IsNil => Type == ValueType.Nil;
        public bool IsBool => Type == ValueType.Bool;
        public bool IsObj => Type == ValueType.Obj;
        public bool IsString => IsObj && ObjType == ObjType.String;


    }

    public enum ValueType
    {
        Bool,
        Nil,
        Number,
        Obj,
    }
}
