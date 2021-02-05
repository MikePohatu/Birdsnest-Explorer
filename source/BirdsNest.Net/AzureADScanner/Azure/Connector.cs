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
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureADScanner.Azure
{
    public class Connector
    {
        private static Connector _instance;
        private string[] _scopes = { ".default" };
        private IConfidentialClientApplication _app;
        private int _retrycount = 5;

        /// <summary>
        /// The number of times a request can retry due to throttling
        /// </summary>
        private int _throttlingretry = 5;

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

        public void Init(Configuration config)
        {
            this.RootUrl = config.RootURL + "/" + config.Version;
            string loginroot = "https://login.microsoftonline.com/";
            this._retrycount = config.RetryCount;
            this._throttlingretry = config.ThrottlingRetryCount;

            this._app = ConfidentialClientApplicationBuilder.Create(config.ID).WithClientSecret(config.Secret).WithAuthority(new Uri(loginroot + config.Tenant)).Build();
            ClientCredentialProvider authenticationProvider = new ClientCredentialProvider(this._app);
            this.Client = new GraphServiceClient(authenticationProvider);
        }

        public async Task<string> GetToken()
        {
            AuthenticationResult result = await this._app.AcquireTokenForClient(this._scopes).ExecuteAsync();
            return result.AccessToken;
        }

        public async Task<HttpClient> GetHttpClientWithToken()
        {
            string token = await this.GetToken();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            return httpClient;
        }

        public async Task<BatchResponseContent> MakeBatchRequest(HttpClient httpClient, BatchRequestContent content, Func<Dictionary<string, HttpResponseMessage>, int> responsehandler)
        {
            //make the batch request
            string requesturl = Connector.Instance.RootUrl + "/$batch";
            BatchResponseContent batchResponseContent = null;

            while (string.IsNullOrWhiteSpace(requesturl) == false)
            {
                HttpResponseMessage response = await this.PostAsync(httpClient, requesturl, content);
                batchResponseContent = new BatchResponseContent(response);

                Dictionary<string, HttpResponseMessage> responses = await batchResponseContent.GetResponsesAsync();

                if (responsehandler != null) { responsehandler(responses); }

                requesturl = await batchResponseContent.GetNextLinkAsync();
            }

            return batchResponseContent;
        }

        public async Task<HttpResponseMessage> PostAsync(HttpClient httpClient, string url, HttpContent content)
        {
            HttpResponseMessage response;
            HttpResponseMessage finalresponse = null;
            int throttled = 0;
            int retried = 0;
                
            while (throttled < this._throttlingretry && retried < this._retrycount && finalresponse == null)
            {
                response = await httpClient.PostAsync(url, content);

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    finalresponse = response;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throttled++;
                    await this.ResponseDelayAsync(response);
                }
                else
                {
                    retried++;
                    Console.WriteLine("Error with POST to URL: " + url);
                    Console.WriteLine(response.ReasonPhrase);
                }
            }

            return finalresponse;
        }

        public async Task<HttpResponseMessage> GetAsync(HttpClient httpClient, string url)
        {
            HttpResponseMessage response;
            HttpResponseMessage finalresponse = null;
            int throttled = 0;
            int retried = 0;

            while (throttled < this._throttlingretry && retried < this._retrycount && finalresponse == null)
            {
                response = await httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    finalresponse = response;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throttled++;
                    await this.ResponseDelayAsync(response);
                }
                else
                {
                    retried++;
                    Console.WriteLine("Error with GET to URL: " + url);
                    Console.WriteLine(response.ReasonPhrase);
                }
            }

            return finalresponse;
        }

        public async Task MakeGraphClientRequestAsync(Func<Task> asyncRequest)
        {
            if (asyncRequest == null) { throw new ArgumentNullException(); }

            int retrycount = 0;
            int throttleretry = 0;

            while (retrycount < this._retrycount && throttleretry < this._throttlingretry)
            {
                try
                {
                    await asyncRequest();
                    break;
                }
                catch (ServiceException e)
                {
                    if (e.IsMatch(GraphErrorCode.ActivityLimitReached.ToString()) || e.IsMatch(GraphErrorCode.ThrottledRequest.ToString()))
                    {
                        if (throttleretry >= this._throttlingretry)
                        {
                            Console.WriteLine("Throttling retry limit hit. Aborting operation");
                            break;
                        }
                        await Task.Delay(10000);
                        throttleretry++;
                    }
                    else
                    {
                        if (retrycount >= this._retrycount)
                        {
                            Console.WriteLine("Retry limit hit. Aborting operation");
                            Console.WriteLine(e.Message);
                            break;
                        }
                        
                        if (e.IsMatch(GraphErrorCode.ServiceNotAvailable.ToString()))
                        {
                            retrycount++;
                            await Task.Delay(10000);
                        }
                        else
                        {
                            Console.WriteLine("Graph client error: " + e.Message);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected Graph client error: " + e.Message);
                    break;
                }
            }
        }

        private async Task ResponseDelayAsync(HttpResponseMessage response)
        {
            TimeSpan? retryseconds = response.Headers.RetryAfter.Delta;
            if (retryseconds == null)
            {
                //retry in 60 seconds as a default
                retryseconds = new TimeSpan(0, 0, 60);
            }
            int delay = retryseconds.GetValueOrDefault().Milliseconds;

            Console.WriteLine("Throttling limits hit. Backing off for " + delay + " milliseconds");
            await Task.Delay(delay);
        }
    }
}
