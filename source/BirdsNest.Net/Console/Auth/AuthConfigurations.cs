using Console.Auth.Directory;
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
                if (section.Key == "ActiveDirectorySettings")
                {
                    DirectoryConfiguration dirconf = section.Get<DirectoryConfiguration>();
                    _configs.Add(dirconf);
                    this.ConfigurationNames.Add(dirconf.Name);
                }

                //else if (section.Key == "LocalServer")
                //{ providers.Add(section.Get<DirectoryProvider>()); }
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
