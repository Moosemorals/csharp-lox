using System;

using Lox.Lib;

namespace Lox
{
    class Program
    {
        static void Main(string[] args)
        {

            Chunk c = new Chunk();
            int con = c.AddConstant(new Value { V = 1.2 });
            c.Add(OpCode.Constant, 1);
            c.Add((byte)con, 1);

            con = c.AddConstant(new Value { V = 3.4 });
            c.Add(OpCode.Constant, 1);
            c.Add((byte)con, 1);

            c.Add(OpCode.Subtract, 1);
            c.Add(OpCode.Return, 2);

            VM vm = new VM(System.Console.Out);
            vm.Interpret(c);


            System.Console.ReadKey();
        }
    }
}
