using System;
using System.Collections.Generic;
using System.Text;

namespace Lox.Lib
{
    public class Hashtable
    {
        private readonly Dictionary<string, Value> table;

        public Hashtable()
        {
            table = new Dictionary<string, Value>();
        }

        public Value this[ObjString key] {
            get {
                return table[key.Chars];
            }
            set {
                Set(key.Chars, value);
            }
        }

        private void Set(string key, Value value)
        {

                if (table.ContainsKey(key)) {
                    table.Remove(key);
                }
                table.Add(key, value);
        }

        public bool ContainsKey(ObjString key) => table.ContainsKey(key.Chars);

        public void AddAll(Hashtable other)
        {
            foreach (var kv in other.table) {
                Set(kv.Key, kv.Value);
            }
        }

        public void Delete(ObjString key)
        {
            if (table.ContainsKey(key.Chars)) {
                table.Remove(key.Chars);
            }
        }

    }
}
