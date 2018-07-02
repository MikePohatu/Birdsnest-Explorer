using Newtonsoft.Json;
using common;
using System.IO;

namespace ADScanner
{
    public class Configuration: NeoConfiguration
    {
        [JsonProperty("AD_DomainPath")]
        public string AD_DomainPath { get; set; }

        [JsonProperty("AD_Username")]
        public string AD_Username { get; set; }

        [JsonProperty("AD_Password")]
        public string AD_Password { get; set; }

        public new static Configuration ReadConfigurationFromFile(string filepath)
        {
            Configuration conf;
            using (StreamReader file = File.OpenText(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                conf = (Configuration)serializer.Deserialize(file, typeof(Configuration));
            }
            return conf;
        }

        public override void Dispose()
        {
            this.AD_Password = string.Empty;
            base.Dispose();
        }
    }
}