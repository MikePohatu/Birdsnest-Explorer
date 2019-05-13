using System;

namespace common
{
    public static class ConsoleWriter
    {
        private static readonly object locker = new object();
        private static int _loggingline = 1;
        private static string[] _progressmessages = new string[1];
        private static int[] _progresslinenumbers = new int[1] { 2 };

        public static bool ShowProgress { get; set; } = true;

        public static void SetProgressLineCount(int count)
        {
            _progressmessages = new string[count];
            _progresslinenumbers = new int[count];
            for (int i = 0; i < count; i++)
            {
                _progresslinenumbers[i] = _loggingline + i + 1;
            }
        }

        public static void WriteError(string message)
        {
            lock (locker)
            {
                PrepareForWrite(message);
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
                PrepareForWrite(message);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Warn: ");
                Console.ResetColor();
                Console.Write(message);
            }
        }

        public static void WriteInfo(string message)
        {
            lock (locker)
            {
                PrepareForWrite(message);
                Console.Write("Info: " + message);
            }
        }

        public static void WriteLine()
        {
            WriteLine(string.Empty);
        }

        public static void WriteLine(string message)
        {
            lock (locker)
            {
                PrepareForWrite(message);
                Console.Write(message);
            }
        }

        public static void Write(string message)
        {
            lock (locker)
            {
                Console.SetCursorPosition(0, _loggingline);
                Console.Write(message);
            }
        }

        public static void WriteProgress(string message)
        {
            WriteProgress(message, 1);
        }

        public static void WriteProgress(string message, int progresslinenumber)
        {
            if (ShowProgress == true)
            {
                lock (locker)
                {
                    if (progresslinenumber < 1) { throw new IndexOutOfRangeException("Progress line less than 1"); }
                    int index = progresslinenumber - 1;
                    _progressmessages[index] = message;
                    int linenum = _progresslinenumbers[index];
                    //clear the line before write
                    Console.SetCursorPosition(0, linenum);
                    Console.Write(new string(' ', Console.BufferWidth-1));

                    FitBufferWidthToString(message);
                    Console.SetCursorPosition(0, linenum);
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Progress: ");
                    Console.ResetColor();
                    Console.Write(message);
                }
            }
        }

        public static void ClearProgress()
        {
            lock (locker)
            {
                for (int i = 0; i < _progresslinenumbers.Length; i++)
                {
                    //clear the line
                    Console.SetCursorPosition(0, _progresslinenumbers[i]);
                    Console.Write(new string(' ', Console.BufferWidth-1));
                }
            }

        }
        public static void InitLine(int linenumber)
        {
            lock (locker)
            {
                Console.Clear();
                //Console.SetBufferSize(300, Console.BufferHeight);
                _loggingline = linenumber;
                for (int i = 0; i < _progresslinenumbers.Length; i++)
                {
                    _progresslinenumbers[i] = _loggingline + i + 1;
                }
                Console.SetCursorPosition(0, linenumber);
            }
        }

        /// <summary>
        /// Prep the console for writing a message. Shunt progress messages, set cursor, and fit 
        /// BufferWidth if necessary
        /// </summary>
        /// <param name="message"></param>
        private static void PrepareForWrite(string message)
        {
            ShuntProgressMessages();
            FitBufferWidthToString(message);
            Console.SetCursorPosition(0, _loggingline);
        }

        private static void ShuntProgressMessages()
        {
            
            int lastlinenum = _progresslinenumbers[_progresslinenumbers.Length-1];
            ClearProgress();

            if (lastlinenum + 1 < Console.BufferHeight-1)
            {
                _loggingline++;
            }
            else
            {
                //add an extra line. This will push the oldest message off the buffer and shunt everything up
                Console.SetCursorPosition(Console.BufferWidth - 1, lastlinenum);
                Console.WriteLine();
                Console.WriteLine();
            }

            for (int i = 0; i < _progresslinenumbers.Length; i++)
            {
                _progresslinenumbers[i] = _loggingline + i + 1;
                WriteProgress(_progressmessages[i], i + 1);
            }
        }

        private static void FitBufferWidthToString(string message)
        {
            if (message == null) { return; }
            //add 10 to the message for the prepended category, /n etc
            if (Console.BufferWidth < message.Length + 10) { Console.BufferWidth = message.Length + 50; }
        }
    }
}
