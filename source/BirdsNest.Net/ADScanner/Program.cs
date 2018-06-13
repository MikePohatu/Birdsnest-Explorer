using System;
using System.Diagnostics;

namespace ADScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            string _appdir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string configfile = _appdir + @"\config.json";
            

            foreach (string arg in args)
            {
                string[] param = arg.Split(new[] { ":" }, 2, StringSplitOptions.None);
                Debug.WriteLine(param[0] + " " + param[1]);
                switch (param[0].ToUpper())
                {
                    case "/CONFIG":
                        configfile = param[1];
                        break;
                    default:
                        break;
                }
            }

            using (Configuration _config = LoadConfig(configfile))
            {

            }
        }

        private static Configuration LoadConfig(string configfile)
        {
            try
            {
                return FileHandler.ReadConfigurationFromFile(configfile);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error loading configuration file: " + e.Message);
                Environment.Exit(1001);
            }

            return null;
        }
    }
}
