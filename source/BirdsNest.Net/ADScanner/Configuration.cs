using System;
using Newtonsoft.Json;
using System.IO;

namespace ADScanner
{
    public class Configuration: IDisposable
    {
        [JsonProperty("ID")]
        public string ID { get; set; } = string.Empty;

        [JsonProperty("AD_DomainPath")]
        public string AD_DomainPath { get; set; }

        [JsonProperty("AD_Username")]
        public string AD_Username { get; set; }

        [JsonProperty("AD_Password")]
        public string AD_Password { get; set; }

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
            this.AD_Password = string.Empty;
        }
    }
}