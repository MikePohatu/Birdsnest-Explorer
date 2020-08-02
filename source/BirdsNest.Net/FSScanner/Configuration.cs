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
using System.Collections.Generic;
using Newtonsoft.Json;
using common;

namespace FSScanner
{
    public class Configuration: IDisposable
    {
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

        /// <summary>
        /// Validate a loaded configuration. Will throw an ArgumentException if an issue is found
        /// </summary>
        /// <returns></returns>
        public void Validate()
        {
            List<FileSystem> fslist = new List<FileSystem>();

            foreach (DataStore ds in this.Datastores)
            {
                foreach (FileSystem fs in ds.FileSystems)
                {
                    fslist.Add(fs);
                }
            }

            foreach (FileSystem fs in fslist)
            { 
                foreach (FileSystem checkingfs in fslist)
                {
                    if (fs != checkingfs)
                    {
                        if (fs.ID == checkingfs.ID)
                        {
                            throw new ArgumentException($"Found duplicate file system IDs in {fs.Path} & {checkingfs.Path}");
                        }

                        if (fs.Path == checkingfs.Path)
                        {
                            throw new ArgumentException($"Found duplicate file system paths in {fs.ID} & {checkingfs.ID}");
                        }
                    }
                }
            }
        }
    }
}