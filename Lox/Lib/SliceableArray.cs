using System;
using System.Collections.Generic;
using System.Text;

namespace Lox.Lib
{
    public class SliceableArray<T>
    {
        private int _offset;
        private T[] _backingArray;

        public SliceableArray(int length)
        {
            _backingArray = new T[length];
            _offset = 0;
        }

        private SliceableArray() { }

        public T this[int index] {
            get { return _backingArray[_offset + index]; }
            set { _backingArray[_offset + index] = value; }
        }

        public SliceableArray<T> Slice(int offset = 0)
        {
            return new SliceableArray<T> {
                _backingArray = this._backingArray,
                _offset = offset,
            };
        }

        public T[] Take(int len)
        {
            T[] result = new T[len];
            Array.Copy(_backingArray, _offset, result, 0, len);
            return result;
        }

        public int Length => _backingArray.Length - _offset;
    }
}
