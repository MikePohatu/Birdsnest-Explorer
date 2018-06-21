using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FSScanner
{
    public class DataStore
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("host")]
        public string Host { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("filesystems")]
        public List<FileSystem> FileSystems { get; set; }
    }
}
