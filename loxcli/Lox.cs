using System;
using System.Collections.Generic;
using System.IO;

namespace loxcli {
    class LoxCli {
        private static readonly Interpreter interpreter = new Interpreter();
        private static bool hadError = false;
        private static bool hadRuntimeError = false;

        private static void RunFile(string path) {
            string code = File.ReadAllText(path);
            Run(code);

            if (hadError || hadRuntimeError) {
                System.Environment.Exit(1);
            }
        }

        private static void RunPrompt() {
            while (true) {
                Run(Console.ReadLine());
                hadError = false;
            }
        }

        private static void Run(String src) {
            Scanner scanner = new Scanner(src);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens);

            List<Stmt> statements = parser.Parse();

            if(hadError) {
                return;
            }

            interpreter.Interpret(statements);
        }

        public static void Error(int line, string message) {
            Report(line, "", message);
        }

        public static void Error(Token token, string message) {
            if (token.type == TokenType.EOF) {
                Report(token.line, " at end ", message);
            } else {
                Report(token.line, " at '" + token.lexeme + "'", message);
            }
        }

        public static void RuntimeError(RuntimeError error) {
            Console.WriteLine(error.Message + "\n[line " + error.token.line + "]");
            hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message) {
            Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
            hadError = true;
        }

        static void Main(string[] args) {
            if (args.Length > 1) {
                Console.Write("Usage: loxcli [script]");
                return;
            } else if (args.Length == 1) {
                RunFile(args[0]);
            } else {
                RunPrompt();
            }
        }
    }
}
