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
        private string _csspath = "wwwroot/css";
        private ILogger _logger;


        public Dictionary<string, Plugin> Plugins { get; private set; } = new Dictionary<string, Plugin>();
        public List<string> NodeLabels { get; private set; } = new List<string>();
        public List<string> EdgeLabels { get; private set; } = new List<string>();
        public Dictionary<string, string> SubTypeProperties { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Icons { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, DataType> NodePropertyDetails { get; private set; } = new Dictionary<string, DataType>();
        public SortedDictionary<string, List<string>> NodeProperties { get; private set; } = new SortedDictionary<string, List<string>>();
        public Dictionary<string, DataType> EdgePropertyDetails { get; private set; } = new Dictionary<string, DataType>();
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
            var nodepropdetails = new Dictionary<string, DataType>();
            var nodeprops = new SortedDictionary<string, List<string>>();
            var edgepropdetails = new Dictionary<string, DataType>();
            var edgeprops = new SortedDictionary<string, List<string>>();

            try
            {
                IEnumerable<string> pluginfilenames = Directory.EnumerateFiles(_pluginspath, "plugin-*.json");

                foreach (string filename in pluginfilenames)
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
                            var propdeets = plug.NodeDataTypes[key];

                            if (propdeets != null)
                            {
                                if (!nodepropdetails.TryAdd(key, propdeets))
                                {
                                    this._logger.LogError("Error loading property types for label: " + key);
                                }

                                if (!nodeprops.TryAdd(key, propdeets.Properties.Select(x => x.Name).OrderBy(name => name).ToList()))
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

                                if (!icons.TryAdd(key, propdeets.Icon))
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
                                if (!edgepropdetails.TryAdd(key, propdeets))
                                {
                                    this._logger.LogError("Error loading property types for label: " + key);
                                }

                                if (!edgeprops.TryAdd(key, propdeets.Properties.Select(x => x.Name).OrderBy(name => name).ToList()))
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
            NodePropertyDetails = nodepropdetails;
            NodeProperties = nodeprops;
            EdgePropertyDetails = edgepropdetails;
            EdgeProperties = edgeprops;
            return true;
        }
    }
}
