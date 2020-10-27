using System;
using System.IO;
using System.Text;

using Lox.Lib;

namespace Lox
{
    class Program
    {

        private static VM vm;

        private static void Repl()
        {
            while (true) {
                Console.Out.Write("> ");

                string line = Console.In.ReadLine();

                if (!string.IsNullOrEmpty(line)) {
                    vm.Interpret(line);
                } 
            }
        }

        private static void RunFile(string path)
        {
            string source = File.ReadAllText(path, Encoding.UTF8);
            InterpretResult result = vm.Interpret(source);

            Environment.Exit((int)result);
        }

        static void Main(string[] args)
        {

            vm = new VM(Console.Out);
            if (args.Length == 0) {
                Repl();
            } else if (args.Length == 1) {
                RunFile(args[0]);
            } else {
                Console.Error.WriteLine("Usage: Lox [path]");
            }
        }
    }
}
