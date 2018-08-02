using System;
using System.IO;
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

        public static NeoConfiguration LoadConfiguration(string filepath)
        {
            NeoConfiguration conf = new NeoConfiguration();

            using (StreamReader file = File.OpenText(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                conf = (NeoConfiguration)serializer.Deserialize(file, typeof(NeoConfiguration));
            }
            return conf;
        }
    }
}
