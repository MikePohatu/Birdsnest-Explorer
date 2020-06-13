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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace Console.Plugins
{
    public class PluginManager
    {
        private readonly ILogger<PluginManager> _logger;
        private readonly IWebHostEnvironment _env;

        public SortedDictionary<string, Plugin> Plugins { get; private set; } = new SortedDictionary<string, Plugin>();
        public SortedDictionary<string, string> NodeDisplayNames { get; private set; } = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> EdgeDisplayNames { get; private set; } = new SortedDictionary<string, string>();
        public Dictionary<string, string> SubTypeProperties { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Icons { get; private set; } = new Dictionary<string, string>();
        public SortedDictionary<string, DataType> NodeDataTypes { get; private set; } = new SortedDictionary<string, DataType>();
        public SortedDictionary<string, List<string>> NodeProperties { get; private set; } = new SortedDictionary<string, List<string>>();
        public SortedDictionary<string, DataType> EdgeDataTypes { get; private set; } = new SortedDictionary<string, DataType>();
        public SortedDictionary<string, List<string>> EdgeProperties { get; private set; } = new SortedDictionary<string, List<string>>();

        public PluginManager(ILogger<PluginManager> logger, IWebHostEnvironment env)
        {
            this._logger = logger;
            this._env = env;
            this.Reload();
        }

        private bool Reload()
        {
            return this.ReloadAsync().Result;
        }

        public async Task<bool> ReloadAsync()
        {
            this._logger.LogInformation("PluginManager reload initiated");
            var plugins = new SortedDictionary<string, Plugin>();

            var icons = new Dictionary<string, string>();
            var subtypes = new Dictionary<string, string>();
            var nodedatatypes = new SortedDictionary<string, DataType>();
            var nodeprops = new SortedDictionary<string, List<string>>();
            var edgedatatypes = new SortedDictionary<string, DataType>();
            var edgeprops = new SortedDictionary<string, List<string>>();
            var nodeDisplayNames = new SortedDictionary<string, string>();
            var edgeDisplayNames = new SortedDictionary<string, string>();

            try
            {

                string pluginsdirpath = this._env.ContentRootPath + "/Plugins";
                string csspath = this._env.ContentRootPath + "/wwwroot/dynamic/plugins.css";

                this._logger.LogInformation("Loading plugins from: " + pluginsdirpath);
                IEnumerable<string> pluginfilenames = Directory.EnumerateFiles(pluginsdirpath, "plugin-*.json");

                foreach (string filename in pluginfilenames)
                {
                    try
                    {
                        this._logger.LogInformation("Loading " + filename);
                        string json = await File.ReadAllTextAsync(filename);
                        JsonSerializerSettings settings = new JsonSerializerSettings();
                        Plugin plug = JsonConvert.DeserializeObject<Plugin>(json);
                        if (string.IsNullOrWhiteSpace(plug.Name))
                        {
                            throw new ArgumentException("Plugin name is not set. Name is required");
                        }
                        plugins.Add(plug.Name, plug);

                        if (plug.NodeDataTypes != null)
                        {
                            foreach (string key in plug.NodeDataTypes.Keys)
                            {
                                var datatype = plug.NodeDataTypes[key];

                                if (datatype != null)
                                {
                                    if (!nodedatatypes.TryAdd(key, datatype))
                                    {
                                        this._logger.LogError("Error loading property types for label: " + key);
                                    }

                                    if (!nodeprops.TryAdd(key, datatype.Properties.Keys.OrderBy(name => name).ToList()))
                                    {
                                        this._logger.LogError("Error loading properties for label: " + key);
                                    }

                                    if (!nodeDisplayNames.TryAdd(datatype.DisplayName, key))
                                    {
                                        this._logger.LogError("Error loading display name for label: " + key);
                                    }

                                    if (string.IsNullOrWhiteSpace(datatype.SubType) == false)
                                    {
                                        if (!subtypes.TryAdd(key, datatype.SubType))
                                        {
                                            this._logger.LogError("Error loading subtype: " + key);
                                        }
                                    }

                                    if (!icons.TryAdd(key, datatype.Icon))
                                    {
                                        this._logger.LogError("Error loading icon: " + key);
                                    }
                                }
                            }
                        }

                        if (plug.EdgeDataTypes != null)
                        {
                            foreach (string key in plug.EdgeDataTypes.Keys)
                            {
                                var propdeets = plug.EdgeDataTypes[key];
                                if (propdeets != null)
                                {
                                    if (!edgedatatypes.TryAdd(key, propdeets))
                                    {
                                        this._logger.LogError("Error loading property types for label: " + key);
                                    }

                                    if (!edgeDisplayNames.TryAdd(propdeets.DisplayName, key))
                                    {
                                        this._logger.LogError("Error loading display name for label: " + key);
                                    }

                                    if (!edgeprops.TryAdd(key, propdeets.Properties.Keys.OrderBy(name => name).ToList()))
                                    {
                                        this._logger.LogError("Error loading properties for label: " + key);
                                    }

                                    if (string.IsNullOrWhiteSpace(propdeets.SubType) == false)
                                    {
                                        if (!subtypes.TryAdd(key, propdeets.SubType))
                                        {
                                            this._logger.LogError("Error loading subtype: " + key);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this._logger.LogError(e.Message);
                    }

                }

                pluginfilenames = Directory.EnumerateFiles(pluginsdirpath, "plugin-*.css");
                string combinedcss = string.Empty;

                foreach (string filename in pluginfilenames)
                {
                    combinedcss = combinedcss + File.ReadAllText(filename) + Environment.NewLine;
                }

                this._logger.LogInformation("Writing " + csspath);
                if (File.Exists(csspath)) { File.SetAttributes(csspath, FileAttributes.Normal); }
                File.WriteAllText(csspath, combinedcss);                
            }
            catch(Exception e)
            {
                this._logger.LogError(e.Message);
                return false;
            }
            this.Icons = icons;
            this.Plugins = plugins;
            this.NodeDisplayNames = nodeDisplayNames;
            this.EdgeDisplayNames = edgeDisplayNames;
            this.SubTypeProperties = subtypes;
            this.NodeDataTypes = nodedatatypes;
            this.NodeProperties = nodeprops;
            this.EdgeDataTypes = edgedatatypes;
            this.EdgeProperties = edgeprops;
            return true;
        }
    }
}
