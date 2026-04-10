#region license
// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Threading;

namespace common
{
    public static class ConsoleWriter
    {
        private static readonly object _lock = new object();

        private static readonly string _progressPrefix = "Progress: ";
        private static int _loggingline = -1;
        private static Timer _progressTimer;

        /// <summary>
        /// The progress messsages for each thread
        /// </summary>
        private static string[] _progressmessages = new string[1];

        /// <summary>
        /// Record of the line number for the progress messages of each thread, init single value to line 2
        /// </summary>
        private static int[] _progresslinenumbers = new int[1] { _loggingline + 1 };

        public static bool ShowProgress { get; set; } = true;

        public static void Init()
        {
            lock (_lock )
            {
                if (ShowProgress)
                {
                    _progressTimer = new Timer(ProgressMessageTicker, new object(), 1000, 1000);
                }
                Console.SetBufferSize(Console.BufferWidth, 1000);
            }
        }

        /// <summary>
        /// Set the number of lines required for the progress messages
        /// </summary>
        /// <param name="count"></param>
        public static void SetProgressLineCount(int count)
        {
            lock (_lock )
            {
                _progressmessages = new string[count];
                _progresslinenumbers = new int[count];
                for (int i = 0; i < count; i++)
                {
                    _progresslinenumbers[i] = _loggingline + 1 + i;
                }
            }
        }

        public static void WriteError(string message)
        {
            lock (_lock )
            {
                PrepareForWrite(message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error: ");
                Console.ResetColor();
                WriteRawMessage(message);
            }
        }

        public static void WriteWarning(string message)
        {
            lock (_lock )
            {
                PrepareForWrite(message);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("Warn: ");
                Console.ResetColor();
                WriteRawMessage(message);
            }
        }

        public static void WriteInfo(string message)
        {
            lock (_lock )
            {
                PrepareForWrite(message);
                WriteRawMessage("Info: " + message);
            }
        }

        public static void WriteLine()
        {
            WriteLine(string.Empty);
        }

        public static void WriteLine(string message)
        {
            lock (_lock )
            {
                PrepareForWrite(message);
                WriteRawMessage(message);
            }
        }

        /// <summary>
        /// Pass an array of tabstop values, and an array of strings. Strings will be written at the tabstop 
        /// values
        /// </summary>
        /// <param name="tabstops"></param>
        /// <param name="strings"></param>
        public static void WriteTabbedLine(int[] tabstops, string[] strings)
        {
            lock (_lock )
            {
                PrepareForWrite(string.Empty);
                int origRow = Console.CursorTop;
                int counter = 0;

                for (int i = 0; i < tabstops.Length; i++)
                {
                    counter = i;
                    if (i < strings.Length)
                    {
                        Console.SetCursorPosition(tabstops[i], origRow);    //move to the cursor position for the tab stop
                        Console.Write(strings[i]);                         //write the string
                    }
                    else { break; }
                }

                //write any remaining strings
                counter++;
                for (int i = counter++; i < strings.Length; i++)
                {
                    Console.Write(strings[i]);
                    counter = i;
                }

                Console.WriteLine();
            }
        }


        public static void Write(string message)
        {
            lock (_lock )
            {
                PrepareForWrite(message);
                Console.Write(message);
            }
        }

        public static void WriteProgress(string message)
        {
            WriteProgress(message, 1);
        }

        public static void WriteProgress(string message, int progresslinenumber)
        {
            if (ShowProgress == false) { return; }

            if (progresslinenumber < 1) { throw new IndexOutOfRangeException("Progress line less than 1"); }
            int index = progresslinenumber - 1;

            lock (_lock )
            {
                _progressmessages[index] = message;
            }
        }

        public static void ClearProgress()
        {
            lock (_lock )
            {
                for (int i = 0; i < _progresslinenumbers.Length; i++)
                {
                    //clear the line
                    var progline = _progresslinenumbers[i];
                    Console.SetCursorPosition(0, progline);
                    Console.Write(new string(' ', Console.BufferWidth));
                    Console.SetCursorPosition(0, progline);
                }
            }
        }

        /// <summary>
        /// Prep the console for writing a message. Shunt progress messages, set cursor, and fit 
        /// BufferWidth if necessary
        /// </summary>
        /// <param name="message"></param>
        private static void PrepareForWrite(string message)
        {
            ScrollBuffer();
            Console.SetCursorPosition(0, _loggingline);
        }

        private static void ScrollBuffer()
        {
            var bufferH = Console.BufferHeight;
            var bufferW = Console.BufferWidth;
            var oldLoggingLine = _loggingline;

            int lastlineNum = _progresslinenumbers[_progresslinenumbers.GetUpperBound(0)];

            //check the window height for scrolling through the buffer
            var lineBuffer = 1; // can add a buffer at the bottom to allow for some over flow. 1 is minimum for zero indexing
            var maxHCoordinate = Console.WindowHeight - lineBuffer;

            ClearProgress();

            //we're at the bottom 
            if (lastlineNum >= maxHCoordinate)
            {
                //add extra line. This will push the oldest message out of the window and shunt everything up
                Console.SetCursorPosition(Console.WindowWidth - 1, maxHCoordinate);
                Console.WriteLine();

                //scroll the window in the buffer
                var newH = lastlineNum - Console.WindowHeight + lineBuffer;
                Console.SetWindowPosition(0, newH);

                for (int i = 0; i < _progresslinenumbers.Length; i++)
                {
                    //line numbers based on amount up from the bottom of the window
                    var progline = _progresslinenumbers[i];
                }
            }
            //not at the bottom, increment everthing
            else
            {
                _loggingline++;
                for (int i = 0; i < _progresslinenumbers.Length; i++)
                {
                    _progresslinenumbers[i]++;
                    var progline = _progresslinenumbers[i];
                }
            }

            if (ShowProgress)
            {
                RewriteProgressMessages();
            }
        }

        private static void RewriteProgressMessages()
        {
            for (int i = 0; i < _progresslinenumbers.Length; i++)
            {
                var message = _progressmessages[i];
                int linenum = _progresslinenumbers[i];
                var prefix = $"{_progressPrefix} {i + 1} | ";

                //var messageLength = message == null ? 0 : message.Length;
                //var paddingLength = Math.Max(0, Console.BufferWidth - prefix.Length - messageLength - 1);
                //var padding = new string(' ', paddingLength);

                //set cursor to the progress line
                Console.SetCursorPosition(0, linenum);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(prefix);
                Console.ResetColor();
                //Console.Write($"{linenum}");
                WriteRawMessage(message);
            }
        }

        private static void ProgressMessageTicker(object o)
        {
            if (ShowProgress == false) return;

            lock (_lock )
            {
                RewriteProgressMessages();
            }
        }

        private static void WriteRawMessage(string message)
        {
            var maxLength = Console.BufferWidth - Console.CursorLeft;
            var messageLength = message == null ? 0 : message.Length;
            var writtenMessage = string.Empty;

            if (messageLength > maxLength )
            {
                writtenMessage = message.Substring(0, maxLength);
            }
            else
            {
                var paddingLength = Math.Max(0, maxLength - messageLength);
                var padding = new string(' ', paddingLength);
                writtenMessage = message + padding;
            }

            Console.Write(writtenMessage);
        }
    }
}
