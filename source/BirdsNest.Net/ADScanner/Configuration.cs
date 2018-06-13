using System;
using System.Security;
using Newtonsoft.Json;

namespace ADScanner
{
    public class Configuration:IDisposable
    {
        [JsonProperty("AD_Domain")]
        public string AD_Domain { get; set; }

        [JsonProperty("AD_Username")]
        public string AD_Username { get; set; }

        [JsonProperty("AD_Password")]
        public string AD_Password { get; set; }

        [JsonProperty("DB_Server")]
        public string DB_Server { get; set; }

        [JsonProperty("DB_Port")]
        public string DB_Port { get; set; }

        [JsonProperty("DB_Username")]
        public string DB_Username { get; set; }

        [JsonProperty("DB_Password")]
        public string DB_Password { get; set; }

        public void Dispose()
        {
            this.DB_Password = string.Empty;
            this.AD_Password = string.Empty;
        }
    }
}