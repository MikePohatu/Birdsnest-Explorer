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
            using (new NetworkConnection(rootpath, cred))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(rootpath);
                Console.WriteLine(dirinfo.Attributes.ToString());
            }  
        }
    }
}
