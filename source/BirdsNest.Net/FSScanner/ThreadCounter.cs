using System.Threading;

namespace FSScanner
{
    public static class ThreadCounter
    {
        private static readonly object _locker = new object();
        private static bool[] _threads = new bool[5];
        public static int ActiveThreadCount { get; private set; } = 0;
        public static int MaxThreads { get { return _threads.Length; } }

        public static void SetMaxThreads(int max)
        {
            _threads = new bool[max];
        }

        /// <summary>
        /// Request a new thread number. Returns -1 if one not available
        /// </summary>
        /// <returns></returns>
        public static int RequestThread()
        {
            lock (_locker)
            {
                for (int i=0; i<_threads.Length;i++)
                {
                    if (_threads[i]==false)
                    {
                        _threads[i] = true;
                        ActiveThreadCount++;
                        return i + 1;
                    }
                }

                return -1;
            }
        }

        public static void Release(int threadnumber)
        {
            lock (_locker)
            {
                ActiveThreadCount--;
                _threads[threadnumber - 1] = false;
            }
        }
    }
}
