using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace common
{
    public class Credential: IDisposable
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        public void Dispose()
        {
            this.Password = null;
        }
    }
}
