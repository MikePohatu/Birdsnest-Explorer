using System.IO;
using Newtonsoft.Json;

namespace ADScanner
{
    public class FileHandler
    {
        public static Configuration ReadConfigurationFromFile(string filepath)
        {
            Configuration conf;
            using (StreamReader file = File.OpenText(filepath))
            {
                JsonSerializer serializer = new JsonSerializer();
                conf = (Configuration)serializer.Deserialize(file, typeof(Configuration));
            }
            return conf;
        }
    }
}
