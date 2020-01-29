using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureADScanner.Azure
{
    public class BatchRequestResponseValueSimple
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("@odata.type")]
        public string ODataType { get; set; }
    }
}
