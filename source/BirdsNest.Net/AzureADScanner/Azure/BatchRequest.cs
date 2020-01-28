using Newtonsoft.Json;

namespace AzureADScanner.Azure
{
    public class BatchRequest
    {
        [JsonProperty("id")]
        string ID { get; set; }

        [JsonProperty("method")]
        string Method { get; set; }

        [JsonProperty("url")]
        string URL { get; set; }
    }
}
