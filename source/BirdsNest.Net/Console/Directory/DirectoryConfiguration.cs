using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Directory
{
    public class DirectoryConfiguration
    {
        public string Domain { get; set; }
        public string AdminGroup { get; set; }
        public string UserGroup { get; set; }
        public string ContainerDN { get; set; }
        public bool SSL { get; set; } = false;
    }
}
