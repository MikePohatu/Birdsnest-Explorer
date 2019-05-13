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

            while (i < 12000)
            {
                dotscount = dotscount + dotsmult;
                string dots = new String('.', dotscount);

                ConsoleWriter.WriteInfo(i.ToString() + ":" + Console.BufferHeight + ":" + dots);
                if (i % 300 == 0) { dotsmult = dotsmult * -1; }
                i++;
                //Console.WriteLine(i);
            }
            //Console.WriteLine(Console.BufferHeight);
            Console.ReadLine();
        }
    }
}
