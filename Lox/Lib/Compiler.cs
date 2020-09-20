using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Lox.Lib
{
    public class Compiler
    {
        public void Compile(TextWriter o, string source) {

            Scanner scanner = new Scanner(source);

            int line = -1;
            while (true) {
                Token token = scanner.ScanToken();
                if (token.Line != line) {
                    o.Write("{0,4:4}", token.Line);
                    line = token.Line;
                } else {
                    o.Write("    | ");
                }
                o.WriteLine("{0,12}: {1}", token.Type, token.Lexeme);

                if (token.Type == TokenType.Eof) {
                    break;
                }
            } 
        }
    }
}
