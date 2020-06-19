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
using Newtonsoft.Json;
using System.IO;

namespace ADScanner
{
    public class Configuration: IDisposable
    {
        [JsonProperty("ID")]
        public string ID { get; set; } = string.Empty;

        [JsonProperty("AD_DomainPath")]
        public string AD_DomainPath { get; set; }

        [JsonProperty("AD_Username")]
        public string AD_Username { get; set; }

        [JsonProperty("AD_Password")]
        public string AD_Password { get; set; }

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
            this.AD_Password = string.Empty;
        }
    }
}