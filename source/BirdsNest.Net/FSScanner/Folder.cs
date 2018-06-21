using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSScanner
{
    public class Folder
    {
        public Folder PermParent { get; set; }
        public string Path { get; set; }
        List<string> ReadSids { get; } = new List<string>();
        List<string> ModifySids { get; } = new List<string>();
        List<string> FullControlSids { get; } = new List<string>();
    }
}
