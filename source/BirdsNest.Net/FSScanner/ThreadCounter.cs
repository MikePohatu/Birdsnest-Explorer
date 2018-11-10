using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FSScanner
{
    public static class ThreadCounter
    {
        
        private static int _activethreads = 1;
        public static int ActiveThreadCount { get { return _activethreads;} }
        public static int MaxThreads { get; set; } = 5;
        public static bool IsThreadAvailable { get { return _activethreads < MaxThreads ? true : false; } }
        public static object Locker { get; private set; } = new object();

        public static int Increment()
        { return Interlocked.Increment(ref _activethreads); }

        public static int Decrement()
        { return Interlocked.Decrement(ref _activethreads); }
    }
}
