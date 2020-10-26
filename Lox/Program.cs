using System;
using System.IO;
using System.Text;

using Lox.Lib;

namespace Lox
{
    class Program
    {

        private static void Repl()
        {
            while (true) {
                Console.Out.WriteLine("> ");

                string line = Console.In.ReadLine();

                if (!string.IsNullOrEmpty(line)) {
                    Interpret(Console.Out, line);
                }

            }
        }

        private static void RunFile(string path)
        {
            string source = File.ReadAllText(path, Encoding.UTF8);
            InterpretResult result = Interpret(Console.Out, source);

            Environment.Exit((int)result);
        }

        private static InterpretResult Interpret(TextWriter writer, string source)
        {
            Chunk chunk = new Chunk();

            Compiler compiler = new Compiler();

            if (!compiler.Compile(writer, source, chunk)) {
                return InterpretResult.CompileError;
            }

            writer.WriteLine("---End of compile---");

            VM vm = new VM(writer);
            return vm.Interpret(chunk); 
        }

        static void Main(string[] args)
        {
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
