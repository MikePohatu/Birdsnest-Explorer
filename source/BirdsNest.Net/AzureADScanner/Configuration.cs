#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureADScanner
{
    public class Configuration : IDisposable
    {
        [JsonProperty("ScannerID")]
        public string ScannerID { get; set; } = "AzureADScanner";

        [JsonProperty("Tenant")]
        public string Tenant { get; set; }

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("RootURL")]
        public string RootURL { get; set; } = "https://graph.microsoft.com";

        [JsonProperty("Version")]
        public string Version { get; set; } = "v1.0";

        [JsonProperty("Secret")]
        public string Secret { get; set; }

        /// <summary>
        /// The number of times a request can retry due to error (not including throttling)
        /// </summary>
        [JsonProperty("RetryCount")]
        public int RetryCount { get; set; } = 5;

        /// <summary>
        /// The number of times a request can retry due to throttling
        /// </summary>
        [JsonProperty("ThrottlingRetryCount")]
        public int ThrottlingRetryCount { get; set; } = 5;


        public static Configuration LoadConfiguration(string filepath)
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
            this.ID = string.Empty;
            this.Secret = string.Empty;
        }
    }
}
