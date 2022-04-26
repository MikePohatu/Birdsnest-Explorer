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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Console.Plugins
{
    public class PluginManager
    {
        private readonly ILogger<PluginManager> _logger;
        private readonly IWebHostEnvironment _env;

        public SortedDictionary<string, Plugin> Plugins { get; private set; } = new SortedDictionary<string, Plugin>();

        [JsonIgnore]
        public SortedDictionary<string, Plugin> Extensions { get; private set; } = new SortedDictionary<string, Plugin>();
        public int ExtensionCount { get { return this.Extensions.Count; } }
        public List<string> DisabledEdges { get; private set; } = new List<string>();
        public SortedDictionary<string, string> NodeDisplayNames { get; private set; } = new SortedDictionary<string, string>();
        public SortedDictionary<string, string> EdgeDisplayNames { get; private set; } = new SortedDictionary<string, string>();
        public Dictionary<string, List<string>> SubTypeProperties { get; private set; } = new Dictionary<string, List<string>>();
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
            return this.ReloadAsync().GetAwaiter().GetResult();
        }

        public async Task<bool> ReloadAsync()
        {
            this._logger.LogInformation("PluginManager reload initiated");
            var plugins = new SortedDictionary<string, Plugin>();
            var extensions = new SortedDictionary<string, Plugin>();

            var icons = new Dictionary<string, string>();
            var subtypes = new Dictionary<string, List<string>>();
            var nodedatatypes = new SortedDictionary<string, DataType>();
            var nodeprops = new SortedDictionary<string, List<string>>();
            var edgedatatypes = new SortedDictionary<string, DataType>();
            var edgeprops = new SortedDictionary<string, List<string>>();
            var nodeDisplayNames = new SortedDictionary<string, string>();
            var edgeDisplayNames = new SortedDictionary<string, string>();
            var disabledEdges = new List<string>();


            string pluginsdirpath = this._env.ContentRootPath + "/Plugins";
            string csspath = this._env.ContentRootPath + "/wwwroot/dynamic/plugins.css";

            //Process the plugin files
            #region
            try
            {
                this._logger.LogInformation("Loading plugins from: " + pluginsdirpath);
                IEnumerable<string> pluginfilenames = Directory.EnumerateFiles(pluginsdirpath, "plugin-*.json");

                foreach (string filename in pluginfilenames)
                {
                    this._logger.LogInformation("Loading " + filename);
                    string json = await File.ReadAllTextAsync(filename);
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    Plugin plug = JsonConvert.DeserializeObject<Plugin>(json);
                    if (string.IsNullOrWhiteSpace(plug.Name))
                    {
                        throw new ArgumentException("Plugin name is not set. Name is required");
                    }

                    if (string.IsNullOrWhiteSpace(plug.Extends))
                    {
                        plugins.Add(plug.Name, plug);
                    }
                    else
                    {
                        extensions.Add(plug.Name, plug);
                    }
                }
            }
            catch (Exception e)
            {
                this._logger.LogError(e.Message);
                return false;
            }
            #endregion


            //process css files
            #region
            try
            {
                var pluginfilenames = Directory.EnumerateFiles(pluginsdirpath, "plugin-*.css");
                string combinedcss = string.Empty;

                foreach (string filename in pluginfilenames)
                {
                    combinedcss = combinedcss + File.ReadAllText(filename) + Environment.NewLine;
                }

                this._logger.LogInformation("Writing " + csspath);
                if (File.Exists(csspath)) { File.SetAttributes(csspath, FileAttributes.Normal); }
                File.WriteAllText(csspath, combinedcss);
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "Error processing css files", null);
                return false;
            }
            #endregion


            //process plugin extensions
            #region
            try
            {
                foreach (Plugin plug in extensions.Values)
                {
                    string extendsName = plug.Extends;
                    Plugin current = plug;

                    //traverse up the tree until we get to the root, which we then extend
                    while (string.IsNullOrWhiteSpace(extendsName) == false)
                    {
                        Plugin parent;
                        if (extensions.TryGetValue(extendsName, out parent))
                        {
                            current = parent;
                            extendsName = current.Extends;
                        }
                        else if (plugins.TryGetValue(extendsName, out parent))
                        {
                            this.Extend(parent, plug);
                            break;
                        }
                        else
                        {
                            this._logger.LogError($"Unable to find plugin {extendsName}");
                            break;
                        }
                    }
                }
            }
            catch (NotSupportedException e)
            {
                this._logger.LogError($"Error processing plugin extensions: {e.Message}");
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "Error processing plugin extensions", null);
                return false;
            }
            #endregion

            //Process plugin properties
            #region
            foreach (Plugin plug in plugins.Values)
            {
                if (plug.NodeDataTypes != null)
                {
                    foreach (string key in plug.NodeDataTypes.Keys)
                    {
                        DataType datatype = plug.NodeDataTypes[key];

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

                            if (datatype.SubTypes.Count > 0)
                            {
                                List<string> outsubs;
                                if (subtypes.TryGetValue(key, out outsubs))
                                {
                                    outsubs.AddRange(datatype.SubTypes);
                                }
                                else
                                {
                                    if (!subtypes.TryAdd(key, datatype.SubTypes))
                                    {
                                        this._logger.LogError("Error loading subtype: " + key);
                                    }
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
                        DataType propdeets = plug.EdgeDataTypes[key];
                        if (propdeets != null)
                        {
                            if (propdeets.Enabled == false) { disabledEdges.Add(key); }
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

                            if (propdeets.SubTypes.Count > 0)
                            {
                                List<string> outsubs;
                                if (subtypes.TryGetValue(key, out outsubs))
                                {
                                    outsubs.AddRange(propdeets.SubTypes);
                                }
                                else
                                {
                                    if (!subtypes.TryAdd(key, propdeets.SubTypes))
                                    {
                                        this._logger.LogError("Error loading subtype: " + key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            this.Icons = icons;
            this.Plugins = plugins;
            this.Extensions = extensions;
            this.NodeDisplayNames = nodeDisplayNames;
            this.EdgeDisplayNames = edgeDisplayNames;
            this.SubTypeProperties = subtypes;
            this.NodeDataTypes = nodedatatypes;
            this.NodeProperties = nodeprops;
            this.EdgeDataTypes = edgedatatypes;
            this.EdgeProperties = edgeprops;
            this.DisabledEdges = disabledEdges;
            return true;
        }


        public void Extend(DataType datatype, DataType extendingtype)
        {
            foreach (string propname in extendingtype.Properties.Keys)
            {
                if (datatype.Properties.ContainsKey(propname) == false)
                {
                    datatype.Properties.Add(propname, extendingtype.Properties[propname]);
                }
                else
                {
                    this._logger.LogWarning($"Property definition already exists: {propname}");
                }
            }

            foreach (string subtype in extendingtype.SubTypes)
            {
                if (datatype.SubTypes.Contains(subtype) == false)
                {
                    datatype.SubTypes.Add(subtype);
                }
            }
        }


        /// <summary>
        /// Extend this plugin, passing in the extending plugin 
        /// </summary>
        /// <param name="extendingplugin"></param>
        public void Extend(Plugin plugin, Plugin extendingplugin)
        {
            //First check if this extension has already been applied. This will be true in a nested scenario
            if (plugin.ExtendedBy.Contains(extendingplugin.Name)) { return; }
            else { plugin.ExtendedBy.Add(extendingplugin.Name); }

            foreach (string nodetype in extendingplugin.NodeDataTypes.Keys)
            {
                DataType datatype = extendingplugin.NodeDataTypes[nodetype];
                if (plugin.NodeDataTypes.ContainsKey(nodetype))
                {
                    //extend the datatype from the extending plugin i.e. add properties
                    this.Extend(plugin.NodeDataTypes[nodetype], datatype);
                }
                else
                {
                    //add the datatype from the extending plugin
                    plugin.NodeDataTypes.Add(nodetype, datatype);
                }
            }

            foreach (string edgetype in extendingplugin.EdgeDataTypes.Keys)
            {
                DataType datatype = extendingplugin.EdgeDataTypes[edgetype];
                if (plugin.EdgeDataTypes.ContainsKey(edgetype))
                {
                    //extend the datatype from the extending plugin i.e. add properties
                    this.Extend(plugin.EdgeDataTypes[edgetype], datatype);
                }
                else
                {
                    //add the datatype from the extending plugin
                    plugin.EdgeDataTypes.Add(edgetype, datatype);
                }
            }

            foreach (string reportname in extendingplugin.Reports.Keys)
            {
                if (plugin.Reports.ContainsKey(reportname))
                {
                    this._logger.LogWarning($"Report already exists: {reportname}");
                }
                else
                {
                    //add the datatype from the extending plugin
                    plugin.Reports.Add(reportname, extendingplugin.Reports[reportname]);
                }
            }
        }
    }
}
