using System;
using System.IO;

using Lox.Lib;

using Xunit;

namespace Tests
{
    public class Chapter1
    {
        [Fact]
        public void Chunk_AddByte()
        {
            //Arrange
            Chunk c = new Chunk();

            // Act
            c.AddByte(OpCode.Return);

            // Assert
            Assert.Equal((byte)OpCode.Return, c.code[0]);
        }

        [Fact]
        public void Disassemble_Chunk()
        {
            // Arrange
            string expected = string.Format("-- chunk --{0}0000 OP_RETURN{0}", Environment.NewLine);
            Chunk c = new Chunk();
            c.AddByte(OpCode.Return);
            StringWriter writer = new StringWriter();

            // Act
            c.Disassemble(writer, "chunk");
            string actual = writer.ToString();


            // Assert
            Assert.Equal(expected, actual); 

        }
    }
}
