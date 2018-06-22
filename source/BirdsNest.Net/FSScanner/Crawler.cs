using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace FSScanner
{
    public class Crawler
    {
        public void Crawl(string rootpath, NetworkCredential cred)
        {
            try
            {
                NetworkConnection netcred = new NetworkConnection(rootpath, cred);
            }
            catch
            {
                Console.WriteLine("Unable to connect to " + rootpath + " with " + cred.UserName);
                return;
            }
            DirectoryInfo dirinfo = new DirectoryInfo(rootpath);
            Console.WriteLine(dirinfo.FullName);
        }
    }
}
