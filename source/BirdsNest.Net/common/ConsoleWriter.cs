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
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Progress: ");
            Console.ResetColor();
            Console.WriteLine(message);
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void ClearProgress()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, 0);
            Console.WriteLine();
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("Warn: ");
            Console.ResetColor();
            Console.WriteLine(message);
        }

        public static void WriteInfo(string message)
        {
            Console.WriteLine("Info: " + message);
        }

        public static void InitLine(int linenumber)
        {
            Console.Clear();
            Console.SetCursorPosition(0, linenumber);
        }
    }
}
