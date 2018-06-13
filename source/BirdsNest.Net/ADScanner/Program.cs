using System;
using System.Diagnostics;

namespace ADScanner
{
    class Program
    {
        private Configuration _config;

        static void Main(string[] args)
        {
            string configfile = string.Empty;

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
        }

        private void LoadConfig(string configfile)
        {
            try
            {
                this._config = FileHandler.ReadConfigurationFromFile(configfile);
            }
            catch(Exception e)
            {
                Console.WriteLine("Error loading configuration file: " + e.Message);
                Environment.Exit(1001);
            }
        }
    }
}
