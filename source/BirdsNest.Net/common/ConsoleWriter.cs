using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common
{
    public static class ConsoleWriter
    {
        public static void WriteProgress(string message)
        {
            int curr = ClearProgress();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Progress: ");
            Console.ResetColor();
            Console.Write(message);
            Console.SetCursorPosition(0, curr);
        }

        public static int ClearProgress()
        {
            int curr = Console.CursorTop;
            Console.Write(new string(' ', Console.WindowWidth * 5));
            Console.SetCursorPosition(0, curr);
            return curr;
        }

        public static void WriteError(string message)
        {
            ClearProgress();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void WriteWarning(string message)
        {
            ClearProgress();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("Warn: ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void WriteInfo(string message)
        {
            ClearProgress();
            Console.WriteLine("Info: " + message);
        }

        public static void WriteLine()
        {
            ClearProgress();
            Console.WriteLine();
        }

        public static void WriteLine(string message)
        {
            Console.WriteLine(message);
        }

        public static void Write(string message)
        {
            ClearProgress();
            Console.Write(message);
        }

        public static void InitLine(int linenumber)
        {
            Console.Clear();
            Console.SetCursorPosition(0, linenumber);
        }
    }
}
