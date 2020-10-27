using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Lox.Lib
{
    public abstract class DynamicArray<T>
    {
        public T[] values;
        protected int count;
        protected int capacity;

        public int Add(T v)
        {
            if (capacity < count + 1) {
                int oldCapacity = capacity;
                capacity = capacity < 8 ? 8 : capacity * 2;
                values = GrowArray(values, oldCapacity, capacity);
            }

            values[count] = v;
            count += 1;
            return count - 1;
        }

        private T[] GrowArray(T[] from, int oldCap, int newCap)
        {
            T[] to = new T[newCap];
            if (from != null) {
                Buffer.BlockCopy(from, 0, to, 0, oldCap);
            }
            return to;
        }

    }
}
