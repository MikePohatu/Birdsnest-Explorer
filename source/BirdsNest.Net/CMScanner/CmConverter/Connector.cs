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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;
using Core.Logging;

namespace CMScanner.CmConverter
{
    public class Connector
    {
        public WqlConnectionManager Connection { get; private set; }


        private static Connector _instance;

        private Connector()
        {
            this.Connection = new WqlConnectionManager();
        }

        public static Connector Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Connector();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Connect using the specified connection credentials
        /// </summary>
        /// <param name="authuser"></param>
        /// <param name="authpw"></param>
        /// <param name="authdomain"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public void Connect(string authuser, string authpw, string authdomain, string server)
        {
            if (string.IsNullOrWhiteSpace(authuser) || string.IsNullOrWhiteSpace(authpw))
            {
                this.Connection.Connect(server);
            }
            else
            {
                this.Connection.Connect(server, authdomain + "\\" + authuser, authpw);
            } 
        }
    }
}
