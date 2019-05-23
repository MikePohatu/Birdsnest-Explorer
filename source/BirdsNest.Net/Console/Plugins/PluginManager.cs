using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace Console.Plugins
{
    public static class PluginManager
    {
        private static string _path = "Plugins";
        public static Dictionary<string, Plugin> Plugins { get; private set; } = new Dictionary<string, Plugin>();
        public static List<string> NodeLabels { get; private set; } = new List<string>();
        public static List<string> EdgeLabels { get; private set; } = new List<string>();

        public static Dictionary<string, string> Icons { get; private set; } = new Dictionary<string, string>();

        public static bool Reload()
        {
            Dictionary<string, Plugin> plugins = new Dictionary<string, Plugin>();
            List<string> nodelabels = new List<string>();
            List<string> edgelabels = new List<string>();
            Dictionary<string, string> icons = new Dictionary<string, string>();
        
            try
            {
                IEnumerable<string> pluginfilenames = Directory.EnumerateFiles(_path, "plugin-*.json");

                foreach (string filename in pluginfilenames)
                {

                    string json = File.ReadAllText(filename);
                    Plugin plug = JsonConvert.DeserializeObject<Plugin>(json);
                    plugins.Add(plug.Name, plug);
                    nodelabels.AddRange(plug.NodeLabels);
                    edgelabels.AddRange(plug.EdgeLabels);
                    foreach (string key in plug.Icons.Keys)
                    {
                        if (!icons.TryAdd(key, plug.Icons[key]))
                        {
                            //logging to add
                        }

                    }
                }
            }
            catch
            {
                return false;
            }
            Icons = icons;
            Plugins = plugins;
            NodeLabels = nodelabels;
            EdgeLabels = edgelabels;
            return true;
        }
    }
}
