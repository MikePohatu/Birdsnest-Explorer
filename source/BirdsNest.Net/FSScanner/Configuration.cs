using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using common;

namespace FSScanner
{
    public class Configuration:IDisposable
    {
        [JsonProperty("credentials")]
        public List<Credential> Credentials { get; set; }

        [JsonProperty("datastores")]
        public List<DataStore> Datastores { get; set; }

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