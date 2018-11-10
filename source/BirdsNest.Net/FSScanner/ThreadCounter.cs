using System.Threading;

namespace FSScanner
{
    public static class ThreadCounter
    {
        
        private static int _activethreads = 1;
        private static readonly object _locker = new object();
        public static int ActiveThreadCount { get { return _activethreads;} }
        public static int MaxThreads { get; set; } = 5;
        private static bool IsThreadAvailable { get { return _activethreads < MaxThreads ? true : false; } }

        /// <summary>
        /// Request a new thread number. Returns -1 if one not available
        /// </summary>
        /// <returns></returns>
        public static int RequestThread()
        {
            lock (_locker)
            {
                if (_activethreads < MaxThreads)
                {
                    return Interlocked.Increment(ref _activethreads);
                }
                else
                {
                    return -1;
                }
            }
        }

        public static int Decrement()
        { return Interlocked.Decrement(ref _activethreads); }
    }
}
