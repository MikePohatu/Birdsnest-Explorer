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
            Dictionary<string, string> icons = new Dictionary<string, string>();
            Dictionary<string, string> subtypes = new Dictionary<string, string>();

            try
            {
                IEnumerable<string> pluginfilenames = Directory.EnumerateFiles(_pluginspath, "plugin-*.json");

                foreach (string filename in pluginfilenames)
                {
                    this._logger.LogInformation("Loading " + filename);
                    string json = File.ReadAllText(filename);
                    Plugin plug = JsonConvert.DeserializeObject<Plugin>(json);
                    plugins.Add(plug.Name, plug);
                    nodelabels.AddRange(plug.NodeLabels);
                    edgelabels.AddRange(plug.EdgeLabels);

                    foreach (string key in plug.Icons.Keys)
                    {
                        if (!icons.TryAdd(key, plug.Icons[key]))
                        {
                            this._logger.LogError("Error loading icon: " + key);
                        }
                    }

                    foreach (string key in plug.SubTypeProperties.Keys)
                    {
                        if (!subtypes.TryAdd(key, plug.SubTypeProperties[key]))
                        {
                            this._logger.LogError("Error loading subtype: " + key);
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
            return true;
        }
    }
}
