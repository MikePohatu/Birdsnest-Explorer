using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using common;

namespace FSScanner
{
    public class Configuration: INeoConfiguration
    {
        [JsonProperty("credentials")]
        public List<Credential> Credentials { get; set; }

        [JsonProperty("datastores")]
        public List<DataStore> Datastores { get; set; }

        [JsonProperty("DB_URI")]
        public string DB_URI { get; set; }

        [JsonProperty("DB_Username")]
        public string DB_Username { get; set; }

        [JsonProperty("DB_Password")]
        public string DB_Password { get; set; }

        public static Configuration LoadConfiguration (string filepath)
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
            foreach (Credential cred in Credentials)
            {
                cred.Dispose();
            }
        }
    }
}