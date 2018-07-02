using Newtonsoft.Json;

namespace FSScanner
{
    public class FileSystem
    {
        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("credentialid")]
        public string CredentialID { get; set; }
    }
}
