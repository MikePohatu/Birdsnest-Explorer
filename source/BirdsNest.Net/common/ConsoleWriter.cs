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
                Console.WriteLine(message);
            }
        }

        public static void WriteInfo(string message)
        {
            lock (locker)
            {
                PrepareForWrite(message);
                Console.WriteLine("Info: " + message);
            }
        }

        public static void WriteLine()
        {
            lock (locker)
            {
                PrepareForWrite(string.Empty);
                Console.WriteLine();
            }
        }

        public static void WriteLine(string message)
        {
            lock (locker)
            {
                PrepareForWrite(message);
                FitBufferWidthToString(message);
                Console.WriteLine(message);
            }
        }

        public static void Write(string message)
        {
            lock (locker)
            {
                //ClearProgress();
                Console.SetCursorPosition(0, _loggingline);
                FitBufferWidthToString(message);
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
                    //clear the line before write
                    Console.SetCursorPosition(0, _progresslinenumbers[index]);
                    Console.Write(new string(' ', Console.BufferWidth));

                    Console.SetCursorPosition(0, _progresslinenumbers[index]);
                    FitBufferWidthToString(message);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("Progress: ");
                    Console.ResetColor();
                    Console.WriteLine(message);
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
                    Console.WriteLine(new string(' ', Console.BufferWidth));
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
            Console.SetCursorPosition(0, _loggingline);
            FitBufferWidthToString(message);
        }

        private static void ShuntProgressMessages()
        {

            _loggingline++;
            ClearProgress();
            for (int i = 0; i < _progresslinenumbers.Length; i++)
            {
                _progresslinenumbers[i] = _loggingline + i + 1;

                if (_progresslinenumbers[i]>=Console.BufferHeight)
                {
                    //Console.WriteLine();
                    ExtendBufferHeight();
                }

                WriteProgress(_progressmessages[i], i + 1);
            }
        }

        private static void FitBufferWidthToString(string message)
        {
            if (message == null) { return; }
            //add 10 to the message for the prepended category etc
            if (Console.BufferWidth < message.Length + 10) { Console.BufferWidth = message.Length + 50; }
        }

        private static void ExtendBufferHeight()
        {
            Console.BufferHeight = Console.BufferHeight + 500;
        }
    }
}
