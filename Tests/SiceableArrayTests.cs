using System;
using System.Collections.Generic;
using System.Text;

using Lox.Lib;

using Xunit;

namespace Tests
{
    public class SiceableArrayTests 
    {
        [Fact]
        public void SA_Slice()
        {
            SliceableArray<int> sliceable = new SliceableArray<int>(10);

            for (int i =0 ; i < sliceable.Length; i += 1) {
                sliceable[i] = i;
            }

            Assert.Equal(2, sliceable[2]);

            SliceableArray<int> child = sliceable.Slice(2);

            Assert.Equal(8, child.Length);

            Assert.Equal(child[0], sliceable[2]);

            child[0] = 15;

            Assert.Equal(child[0], sliceable[2]);

            Assert.Equal(15, sliceable[2]);
        }
        
        [Fact]
        public void SA_Take()
        {
            SliceableArray<int> sliceable = new SliceableArray<int>(10);

            for (int i =0 ; i < sliceable.Length; i += 1) {
                sliceable[i] = i;
            }

            int[] actual = sliceable.Take(4);

            Assert.Equal(10, sliceable.Length);

            Assert.Equal(4, actual.Length);

            Assert.Equal(0, sliceable[0]);
            actual[0] = 15; 
            Assert.Equal(0, sliceable[0]);

        }
    }
}
