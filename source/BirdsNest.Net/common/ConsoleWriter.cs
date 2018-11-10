using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace common
{
    public static class ConsoleWriter
    {
        private static object locker = new object();
        private static int _loggingline = 1;
        private static string[] _progressmessages = new string[1];
        private static int[] _progresslinenumbers = new int[1];

        public static void SetProgressLineCount(int count)
        {
            _progressmessages = new string[count];
            _progresslinenumbers = new int[count];
            for (int i = 0; i < count; i++)
            {
                _progresslinenumbers[i] = _loggingline + i + 1;
            }
        }

        public static void WriteProgress(string message)
        {
            WriteProgress(message, 1);
        }

        public static void WriteProgress(string message, int progresslinenumber)
        {
            lock (locker)
            {
                if (progresslinenumber < 1) { throw new IndexOutOfRangeException("Progress line less than 1"); }
                int index = progresslinenumber - 1;
                _progressmessages[index] = message;
                //clear the line before write
                Console.SetCursorPosition(0, _progresslinenumbers[index]);
                Console.Write(new string(' ', Console.BufferWidth));

                Console.SetCursorPosition(0, _progresslinenumbers[index]);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Progress: ");
                Console.ResetColor();
                Console.WriteLine(message);
            }
        }

        public static void ClearProgress()
        {
            lock (locker)
            {
                //int abslinenum = 0;
                for (int i = 0; i < _progresslinenumbers.Length; i++)
                {
                    //clear the line
                    Console.SetCursorPosition(0, _progresslinenumbers[i]);
                    Console.Write(new string(' ', Console.BufferWidth));
                }
            }
        }

        public static void WriteError(string message)
        {
            lock (locker)
            {
                ShuntProgressMessages();
                Console.SetCursorPosition(0, _loggingline);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error: ");
                Console.ResetColor();
                Console.WriteLine(message);
            }
        }

        public static void WriteWarning(string message)
        {
            lock (locker)
            {
                ShuntProgressMessages();
                Console.SetCursorPosition(0, _loggingline);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Warn: ");
                Console.ResetColor();
                Console.WriteLine(message);
            }
        }

        public static void WriteInfo(string message)
        {
            lock (locker)
            {
                //ClearProgress();
                ShuntProgressMessages();
                Console.SetCursorPosition(0, _loggingline);
                Console.WriteLine("Info: " + message);
            }
        }

        public static void WriteLine()
        {
            lock (locker)
            {
                ShuntProgressMessages();
                Console.SetCursorPosition(0, _loggingline);
                Console.WriteLine();
            }
        }

        public static void WriteLine(string message)
        {
            lock (locker)
            {
                _loggingline++;
                ClearProgress();
                Console.SetCursorPosition(0, _loggingline);
                Console.WriteLine(message);
            }
        }

        public static void Write(string message)
        {
            lock (locker)
            {
                //Console.SetCursorPosition(0, _loggingline);
                Console.Write(message);
            }
        }

        public static void InitLine(int linenumber)
        {
            lock (locker)
            {
                Console.Clear();
                Console.SetBufferSize(300, Console.BufferHeight);
                _loggingline = linenumber;
                Console.SetCursorPosition(0, linenumber);
            }
        }

        private static void ShuntProgressMessages()
        {

            _loggingline++;
            ClearProgress();
            for (int i = 0; i < _progresslinenumbers.Length; i++)
            {
                _progresslinenumbers[i] = _loggingline + i + 1;

                if (string.IsNullOrEmpty(_progressmessages[i]) == false)
                {
                    WriteProgress(_progressmessages[i], i + 1);
                }
            }
        }
    }
}
