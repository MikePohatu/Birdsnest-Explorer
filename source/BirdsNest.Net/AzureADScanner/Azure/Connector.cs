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
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AzureADScanner.Azure
{
    public class Connector
    {
        private static Connector _instance;
        private string[] _scopes = { ".default" };
        private IConfidentialClientApplication _app;

        public string RootUrl { get; private set; }

        public GraphServiceClient Client { get; private set; }

        public IConfidentialClientApplication App { get { return Connector.Instance._app; } }

        public static Connector Instance
        {
            get
            {
                if (_instance == null) { _instance = new Connector(); }
                return _instance;
            }
        }

        private Connector() { }

        public void Init(string id, string secret, string tenant, string rooturl)
        {
            string loginroot = "https://login.microsoftonline.com/";
            this.RootUrl = rooturl;

            this._app = ConfidentialClientApplicationBuilder.Create(id).WithClientSecret(secret).WithAuthority(new Uri(loginroot+ tenant)).Build();
            ClientCredentialProvider authenticationProvider = new ClientCredentialProvider(this._app);
            this.Client = new GraphServiceClient(authenticationProvider);
        }

        public async Task<string> GetToken()
        {
            AuthenticationResult result = await this._app.AcquireTokenForClient(this._scopes).ExecuteAsync();
            return result.AccessToken;
        }
    }
}
