using Console.Auth.Directory;
using Console.Auth.Local;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Console.Auth
{
    public class AuthConfigurations
    {
        private List<IAuthConfiguration> _configs = new List<IAuthConfiguration>();
        public List<string> ConfigurationNames { get; private set; } = new List<string>();

        public AuthConfigurations(IConfigurationSection configsection)
        {
            var sections = configsection.GetChildren();
            foreach (IConfigurationSection section in sections)
            {
                if (section["Type"] == "ActiveDirectory")
                {
                    DirectoryConfiguration conf = section.Get<DirectoryConfiguration>();
                    _configs.Add(conf);
                    this.ConfigurationNames.Add(conf.Name);
                }

                else if (section["Type"] == "LocalServer")
                {
                    LocalAuthConfiguration conf = section.Get<LocalAuthConfiguration>();
                    _configs.Add(conf);
                    this.ConfigurationNames.Add(conf.Name);
                }
            }
        }

        public IAuthConfiguration GetAuthConfiguration(string name)
        {
            foreach (IAuthConfiguration prov in _configs)
            {
                if (name == prov.Name) { return prov; }
            }

            return null;
        }
    }
}
