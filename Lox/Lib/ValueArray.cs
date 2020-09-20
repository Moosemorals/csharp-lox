using System;
using System.Collections.Generic;
using System.Text;

namespace Lox.Lib
{
    public class ValueArray : DynamicArray<Value>
    {
        
        public ValueArray()
        {
            count = 0;
            capacity = 0;
            values = null;
        }

    }
}
