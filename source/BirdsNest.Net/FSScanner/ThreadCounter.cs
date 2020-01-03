#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
//
#endregion
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
