using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMScanner
{
    public class Configuration : IDisposable
    {
        [JsonProperty("SiteServer")]
        public string SiteServer { get; set; }

        [JsonProperty("ScannerID")]
        public string ScannerID { get; set; }


        [JsonProperty("Domain")]
        public string Domain { get; set; }

        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }

        public static Configuration LoadConfiguration(string filepath)
        {
            Configuration conf;
            using (StreamReader file = File.OpenText(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                conf = (Configuration)serializer.Deserialize(file, typeof(Configuration));
            }
            return conf;
        }

        public void Dispose()
        {
            this.Password = string.Empty;
        }
    }
}
