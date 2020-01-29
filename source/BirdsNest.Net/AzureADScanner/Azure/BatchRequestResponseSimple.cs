using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureADScanner.Azure
{
    public class BatchRequestResponseSimple
    {
        [JsonProperty("@odata.context")]
        public string ODataContext { get; set; }

        [JsonProperty("value")]
        public List<BatchRequestResponseValueSimple> Values { get; set; }
    }
}
