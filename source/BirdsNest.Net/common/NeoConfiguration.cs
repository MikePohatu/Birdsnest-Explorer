using System;
using Newtonsoft.Json;

namespace common
{
    public class NeoConfiguration: IDisposable
    {
        [JsonProperty("DB_URI")]
        public string DB_URI { get; set; }

        [JsonProperty("DB_Username")]
        public string DB_Username { get; set; }

        [JsonProperty("DB_Password")]
        public string DB_Password { get; set; }

        public virtual void Dispose()
        {
            this.DB_Password = string.Empty;
        }
    }
}
