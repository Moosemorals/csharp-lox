using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Lox.Lib;

using Xunit;

namespace Tests
{
    public class VM_Tests
    {
        [Fact]
        public void Stack_Push_Pop()
        {
            // Arrange
            StringWriter writer = new StringWriter();
            VM vm = new VM(writer);

            // Act
            vm.Push(new Value { V = 7 });

            // Assert
            Value v = vm.Pop();
            Assert.NotNull(v);
            Assert.Equal(7, v.V,2);
        }
    }
}
