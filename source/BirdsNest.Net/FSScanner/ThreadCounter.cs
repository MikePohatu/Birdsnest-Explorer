using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FSScanner
{
    public static class ThreadCounter
    {
        private static int _activethreads = 0;
        public static int ActiveThreadCount { get { return _activethreads;} }
        public static int MaxThreads { get; set; } = Environment.ProcessorCount * 2;
        public static bool IsThreadAvailable { get { return _activethreads < MaxThreads ? true : false; } }

        public static void Increment()
        { Interlocked.Increment(ref _activethreads); }

        public static void Decrement()
        { Interlocked.Decrement(ref _activethreads); }
    }
}
