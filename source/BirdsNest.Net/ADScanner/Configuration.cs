using Newtonsoft.Json;
using common;

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

        public override void Dispose()
        {
            this.AD_Password = string.Empty;
            base.Dispose();
        }
    }
}