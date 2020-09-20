using System;
using System.Collections.Generic;
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

        public override string ToString()
        {
            return As != null ? As.ToString() : "Nil";
        }

        public double AsNumber => (double)As;
        public bool AsBool => (bool)As;

        public bool IsNumber => Type == ValueType.Number;
        public bool IsNil => Type == ValueType.Nil;
        public bool IsBool => Type == ValueType.Bool;

        public bool IsEqual(Value b)
        {
            if (Type != b.Type) {
                return false;
            }

            switch (Type) {
                case ValueType.Bool: return AsBool == b.AsBool;
                case ValueType.Nil: return true;
                case ValueType.Number: return AsNumber == b.AsNumber;
                default:
                    throw new Exception("Unreachable code reached");
            }
        }
    }

    public enum ValueType
    {
        Bool,
        Nil,
        Number,
    }
}
