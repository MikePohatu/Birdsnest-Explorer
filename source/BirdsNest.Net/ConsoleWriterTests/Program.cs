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
using System;
using common;

namespace ConsoleWriterTests
{
    class Program
    {
        static void Main(string[] args)
        {

            //Console.SetBufferSize(Console.BufferWidth, 10000);
            //Console.WriteLine(Console.BufferHeight);
            int i = 1;
            int dotsmult = 1;
            int dotscount = 0;
            ConsoleWriter.SetProgressLineCount(2);

            while (i < 12000)
            {
                dotscount = dotscount + dotsmult;
                string dots = new String('.', dotscount);

                ConsoleWriter.WriteInfo(i.ToString() + ":" + Console.BufferHeight + ":" + dots);
                ConsoleWriter.WriteProgress(i.ToString() + ":1",1);
                ConsoleWriter.WriteProgress(i.ToString() + ":2",2);
                if (i % 300 == 0) { dotsmult = dotsmult * -1; }
                i++;
                //Console.WriteLine(i);
            }
            //Console.WriteLine(Console.BufferHeight);
            ConsoleWriter.ShowProgress = false;
            ConsoleWriter.ClearProgress();
            ConsoleWriter.WriteInfo("Tester finished");
            ConsoleWriter.WriteLine("1");
            ConsoleWriter.WriteLine("1");
            ConsoleWriter.WriteLine("1");
            ConsoleWriter.WriteLine();
            //ConsoleWriter.ClearProgress();
            ConsoleWriter.WriteLine();
            ConsoleWriter.Write("Exiting.");
            for (int j = 0; j < 3; j++)
            {
                System.Threading.Thread.Sleep(500);
                ConsoleWriter.Write(".");
            }
            Console.ReadLine();
        }
    }
}
