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
        public IConfidentialClientApplication _app;

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

        public void Init(string id, string secret, string tenant)
        {
            string loginroot = "https://login.microsoftonline.com/";
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
