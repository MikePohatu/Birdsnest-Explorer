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
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Console.Plugins
{
    public class PluginManager
    {
        private string _pluginspath = "Plugins";
        private string _csspath = "wwwroot/dynamic";
        private ILogger _logger;


        public Dictionary<string, Plugin> Plugins { get; private set; } = new Dictionary<string, Plugin>();
        public List<string> NodeLabels { get; private set; } = new List<string>();
        public List<string> EdgeLabels { get; private set; } = new List<string>();
        public Dictionary<string, string> SubTypeProperties { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Icons { get; private set; } = new Dictionary<string, string>();
        public SortedDictionary<string, DataType> NodeDataTypes { get; private set; } = new SortedDictionary<string, DataType>();
        public SortedDictionary<string, List<string>> NodeProperties { get; private set; } = new SortedDictionary<string, List<string>>();
        public SortedDictionary<string, DataType> EdgeDataTypes { get; private set; } = new SortedDictionary<string, DataType>();
        public SortedDictionary<string, List<string>> EdgeProperties { get; private set; } = new SortedDictionary<string, List<string>>();

        public PluginManager(ILogger logger)
        {
            this._logger = logger;
            this.Reload();
        }

        public bool Reload()
        {
            this._logger.LogInformation("PluginManager reload initiated");
            Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();
            List<string> nodelabels = new List<string>();
            List<string> edgelabels = new List<string>();
            var icons = new Dictionary<string, string>();
            var subtypes = new Dictionary<string, string>();
            var nodedatatypes = new SortedDictionary<string, DataType>();
            var nodeprops = new SortedDictionary<string, List<string>>();
            var edgedatatypes = new SortedDictionary<string, DataType>();
            var edgeprops = new SortedDictionary<string, List<string>>();

            try
            {
                string pluginsdirpath = Directory.GetCurrentDirectory() + "\\" + _pluginspath;
                IEnumerable<string> pluginfilenames = Directory.EnumerateFiles(pluginsdirpath, "plugin-*.json");

                foreach (string filename in pluginfilenames)
                {
                    try
                    {
                        this._logger.LogInformation("Loading " + filename);
                        string json = File.ReadAllText(filename);
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
                                nodelabels.Add(key);
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
                                edgelabels.Add(key);
                                var propdeets = plug.EdgeDataTypes[key];
                                if (propdeets != null)
                                {
                                    if (!edgedatatypes.TryAdd(key, propdeets))
                                    {
                                        this._logger.LogError("Error loading property types for label: " + key);
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

                pluginfilenames = Directory.EnumerateFiles(_pluginspath, "plugin-*.css");
                string combinedcss = string.Empty;

                foreach (string filename in pluginfilenames)
                {
                    combinedcss = combinedcss + File.ReadAllText(filename) + Environment.NewLine;
                }

                string css = this._csspath + "/plugins.css";
                this._logger.LogInformation("Writing " + css);
                if (File.Exists(css)) { File.SetAttributes(css, FileAttributes.Normal); }
                File.WriteAllText(css, combinedcss);

                nodelabels.Sort();
                edgelabels.Sort();
                
            }
            catch(Exception e)
            {
                this._logger.LogError(e.Message);
                return false;
            }
            Icons = icons;
            Plugins = plugins;
            NodeLabels = nodelabels;
            EdgeLabels = edgelabels;
            SubTypeProperties = subtypes;
            NodeDataTypes = nodedatatypes;
            NodeProperties = nodeprops;
            EdgeDataTypes = edgedatatypes;
            EdgeProperties = edgeprops;
            return true;
        }
    }
}
