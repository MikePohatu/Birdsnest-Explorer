#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Console.neo4jProxy
{
    public class NeoConfiguration: IDisposable
    {
        [JsonProperty("dbURI")]
        public string DB_URI { get; set; }

        [JsonProperty("dbUsername")]
        public string DB_Username { get; set; }

        [JsonProperty("dbPassword")]
        public string DB_Password { get; set; }

        [JsonProperty("dbTimeout")]
        public int DB_Timeout { get; set; } = 15;

        public virtual void Dispose()
        {
            this.DB_Password = string.Empty;
        }

        public static NeoConfiguration LoadConfigurationFile(string filepath)
        {
            NeoConfiguration conf = new NeoConfiguration();

            using (StreamReader reader = File.OpenText(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                conf = (NeoConfiguration)serializer.Deserialize(reader, typeof(NeoConfiguration));
            }
            return conf;
        }

        public static NeoConfiguration LoadJsonString(string json)
        {
            NeoConfiguration conf = null;
            JsonSerializer serializer = new JsonSerializer();
            if (string.IsNullOrEmpty(json) == false) 
            {
                using (StringReader reader = new StringReader(json))
                {
                    conf = (NeoConfiguration)serializer.Deserialize(reader, typeof(NeoConfiguration));
                }
            }
            
                
            return conf;
        }

        public static NeoConfiguration LoadIConfigurationSection(IConfiguration configSection)
        {
            NeoConfiguration conf = new NeoConfiguration();
            conf.DB_URI = configSection.GetValue<string>("dbURI");
            conf.DB_Username = configSection.GetValue<string>("dbUsername");
            conf.DB_Password = configSection.GetValue<string>("dbPassword");
            conf.DB_Timeout = configSection.GetValue<int>("dbTimeout");

            return conf;
        }
    }
}
