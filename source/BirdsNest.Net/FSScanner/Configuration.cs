using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using common;

namespace FSScanner
{
    public class Configuration: IDisposable
    {
        [JsonProperty("ScannerID")]
        public string ScannerID { get; set; } = string.Empty;

        [JsonProperty("credentials")]
        public List<Credential> Credentials { get; set; } = new List<Credential>();

        [JsonProperty("datastores")]
        public List<DataStore> Datastores { get; set; } = new List<DataStore>();

        [JsonProperty("maxthreads")]
        public int MaxThreads
        {
            get { return ThreadCounter.MaxThreads; }
            set
            {
                ThreadCounter.SetMaxThreads(value);
            }
        }

        [JsonProperty("showprogress")]
        public bool ShowProgress { get; set; } = true;

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