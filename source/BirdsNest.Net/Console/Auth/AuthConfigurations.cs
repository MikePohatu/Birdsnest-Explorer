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
#endregion
using Console.Auth.Windows.Directory;
using Console.Auth.Windows.Local;
using Console.Auth.LDAP;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Console.Auth
{
    public class AuthConfigurations
    {
        private List<IAuthConfiguration> _configs = new List<IAuthConfiguration>();
        public List<string> ConfigurationNames { get; private set; } = new List<string>();

        public AuthConfigurations(IConfiguration configuration)
        {
            
            var sections = configuration.GetSection("Authorization").GetChildren();
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

                else if (section["Type"] == "LDAP")
                {
                    LdapConfiguration conf = section.Get<LdapConfiguration>();
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
