﻿#region license
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
using System.IO;

namespace ADScanner
{
    public class Configuration : IDisposable
    {
        [JsonProperty("ID")]
        public string ID { get; set; } = string.Empty;

        [JsonProperty("Domain")]
        public string Domain { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("ContainerDN")]
        public string ContainerDN { get; set; }

        [JsonProperty("SSL")]
        public bool SSL { get; set; } = false;

        [JsonProperty("TimeoutSeconds")]
        public int TimeoutSeconds { get; set; } = 900;

        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }

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
            this.Password = string.Empty;
        }
    }
}