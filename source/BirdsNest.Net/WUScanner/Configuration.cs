using System;
using Newtonsoft.Json;
using System.IO;

namespace WUScanner
{
    public class Configuration
    {
        [JsonProperty("username")]
        public string UserName { get; set; } = string.Empty;

        [JsonProperty("password")]
        public string Password { get; set; } = string.Empty;

        [JsonProperty("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonProperty("servername")]
        public string ServerName { get; set; } = string.Empty;

        [JsonProperty("port")]
        public int Port { get; set; } = 8530;

        [JsonProperty("usessl")]
        public bool UseSSL { get; set; } = false;


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
    }

    
}