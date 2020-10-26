using System;
using System.IO;

using Lox.Lib;

using Xunit;

namespace Tests
{
    public class Chunk_Tests
    {
        [Fact]
        public void Chunk_AddByte()
        {
            //Arrange
            Chunk c = new Chunk();

            // Act
            c.Add(OpCode.Return, 1);

            // Assert
            Assert.Equal((byte)OpCode.Return, c.values[0]);
        }

        [Fact]
        public void Disassemble_Chunk()
        {
            // Arrange
            string expected = string.Format("-- chunk --{0}0000    1 OP_RETURN{0}", Environment.NewLine);
            Chunk c = new Chunk();
            c.Add(OpCode.Return, 1);
            StringWriter writer = new StringWriter();

            // Act
            c.Disassemble(writer, "chunk");
            string actual = writer.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void DynamicArray_Chunk_Extend()
        {
            // Arrange
            Chunk c = new Chunk();

            // Act
            for (byte i = 0; i < 16; i += 1) {
                c.Add(i);
            }

            // Assert
            Assert.Equal(16, c.values.Length);
            Assert.Equal(0, c.values[0]);
            Assert.Equal(15, c.values[15]);

        }

        [Fact]
        public void Chunk_Constants()
        {
            // Arrange
            Chunk c = new Chunk();

            // Act
            int con = c.AddConstant(Value.Number(1.12));

            c.Add(OpCode.Constant, 1);
            c.Add((byte)con);

            // Assert
            Assert.Equal((byte)(con), c.values[1]);
        }

        [Fact]
        public void Disassemble_Constants()
        {
            // Arrange
            string expected = string.Format("-- chunk --{0}0000    1 OP_CONSTANT         0 '1.2'{0}", Environment.NewLine);
            Chunk c = new Chunk();
            int con = c.AddConstant(Value.Number(1.2));
            c.Add(OpCode.Constant, 1);
            c.Add((byte)con, 1);

            StringWriter writer = new StringWriter();

            // Act
            c.Disassemble(writer, "chunk");
            string actual = writer.ToString();


            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Disassemble_MultiLine()
        {
            // Arrange
            string expected = string.Format("-- chunk --{0}0000    1 OP_CONSTANT         0 '1.2'{0}0002    2 OP_RETURN{0}", Environment.NewLine);
            Chunk c = new Chunk();
            int con = c.AddConstant(Value.Number(1.2));
            c.Add(OpCode.Constant, 1);
            c.Add((byte)con, 1);
            c.Add(OpCode.Return, 2);

            StringWriter writer = new StringWriter();

            // Act
            c.Disassemble(writer, "chunk");
            string actual = writer.ToString();


            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Disassemble_LineContinues()
        {
            // Arrange
            string expected = string.Format("-- chunk --{0}0000    1 OP_CONSTANT         0 '1.2'{0}0002    | OP_RETURN{0}", Environment.NewLine);
            Chunk c = new Chunk();
            int con = c.AddConstant(Value.Number(1.2));
            c.Add(OpCode.Constant, 1);
            c.Add((byte)con, 1);
            c.Add(OpCode.Return, 1);

            StringWriter writer = new StringWriter();

            // Act
            c.Disassemble(writer, "chunk");
            string actual = writer.ToString();


            // Assert
            Assert.Equal(expected, actual);
        }
 
    }

}
