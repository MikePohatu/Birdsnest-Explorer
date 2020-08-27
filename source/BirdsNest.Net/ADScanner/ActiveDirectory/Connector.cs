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
using System;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace ADScanner.ActiveDirectory
{
    public static class Connector
    {
        public static PrincipalContext CreatePrincipalContext(Configuration config)
        {
            PrincipalContext context;
            if (config.SSL == true)
            {
                ContextOptions options = ContextOptions.Negotiate | ContextOptions.SecureSocketLayer;

                if (string.IsNullOrWhiteSpace(config.Username) || string.IsNullOrWhiteSpace(config.Password))
                {
                    context = new PrincipalContext(ContextType.Domain, config.Domain, config.ContainerDN, options);
                }
                else
                {
                    context = new PrincipalContext(ContextType.Domain, config.Domain, config.ContainerDN, options, config.Username, config.Password);
                }
                
            }
            else
            {
                if (string.IsNullOrWhiteSpace(config.Username) || string.IsNullOrWhiteSpace(config.Password))
                {
                    context = new PrincipalContext(ContextType.Domain, config.Domain, config.ContainerDN);
                }
                else
                {
                    context = new PrincipalContext(ContextType.Domain, config.Domain, config.ContainerDN, config.Username, config.Password);
                }
            }

            return context;
        }
    }
}
