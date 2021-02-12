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
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AzureADScanner.Azure
{
    public class Connector
    {
        private static Connector _instance;
        private string[] _scopes = { ".default" };
        private IConfidentialClientApplication _app;

        /// <summary>
        /// The number of times a request can retried
        /// </summary>
        private int _maxretrycount = 5;

        /// <summary>
        /// The current number of times retry has occured. Reset after successful request
        /// </summary>
        private int _retrycount = 0;

        /// <summary>
        /// The number of times a request can retry due to throttling
        /// </summary>
        private int _maxthrottlingretry = 5;

        /// <summary>
        /// The current number of times retry has occured following throttling. Reset after successful request
        /// </summary>
        private int _throttlingretrycount = 0;

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
            this._maxretrycount = config.RetryCount;
            this._maxthrottlingretry = config.ThrottlingRetryCount;

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

        public async Task<HttpResponseMessage> PostAsync(HttpClient httpClient, string url, HttpContent content)
        {
            HttpResponseMessage response;
            HttpResponseMessage finalresponse = null;
                
            while (this._throttlingretrycount < this._maxthrottlingretry && this._retrycount < this._maxretrycount && finalresponse == null)
            {
                response = await httpClient.PostAsync(url, content);

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    finalresponse = response;
                    this._retrycount = 0;
                    this._throttlingretrycount = 0;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    this._throttlingretrycount++;
                    await this.ResponseDelayAsync(response.Headers.RetryAfter.Delta);
                }
                // FailedDependency can occur if one of the responses includes a 429
                else if (response.StatusCode == HttpStatusCode.FailedDependency)
                {
                    this._throttlingretrycount++;
                    await this.ResponseDelayAsync(response.Headers.RetryAfter.Delta);
                }
                else
                {
                    this._retrycount++;
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

            while (throttled < this._maxthrottlingretry && retried < this._maxretrycount && finalresponse == null)
            {
                response = await httpClient.GetAsync(url);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    finalresponse = response;
                    this._retrycount = 0;
                    this._throttlingretrycount = 0;
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    throttled++;
                    await this.ResponseDelayAsync(response.Headers.RetryAfter.Delta);
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

        public async Task<BatchResponseContent> MakeBatchRequest(HttpClient httpClient, BatchRequestContent content, Func<Dictionary<string, HttpResponseMessage>, Task> asyncresponsehandler)
        {
            //make the batch request
            string requesturl = Connector.Instance.RootUrl + "/$batch";
            BatchResponseContent batchResponseContent = null;

            while (string.IsNullOrWhiteSpace(requesturl) == false)
            {
                HttpResponseMessage response = await this.PostAsync(httpClient, requesturl, content);
                batchResponseContent = new BatchResponseContent(response);

                Dictionary<string, HttpResponseMessage> responses = await batchResponseContent.GetResponsesAsync();

                if (asyncresponsehandler != null) {
                    await asyncresponsehandler(responses);
                }

                requesturl = await batchResponseContent.GetNextLinkAsync();
            }

            return batchResponseContent;
        }

        public async Task MakeGraphClientRequestAsync(Func<Task> asyncRequest)
        {
            if (asyncRequest == null) { throw new ArgumentNullException(); }

            while (this._retrycount < this._maxretrycount && this._throttlingretrycount < this._maxthrottlingretry)
            {
                try
                {
                    await asyncRequest();
                    this._retrycount = 0;
                    this._throttlingretrycount = 0;
                    break;
                }
                catch (ServiceException e)
                {
                    switch (e.StatusCode)
                    {
                        case HttpStatusCode.TooManyRequests:
                            if (this._throttlingretrycount >= this._maxthrottlingretry)
                            {
                                Console.WriteLine("Throttling retry limit hit. Aborting operation");
                                break;
                            }
                            Console.Write("#");
                            Console.WriteLine(e.RawResponseBody);
                            RetryConditionHeaderValue retry = e.ResponseHeaders.RetryAfter;
                            await this.ResponseDelayAsync(retry?.Delta);

                            this._throttlingretrycount++;
                            break;
                        case HttpStatusCode.FailedDependency:
                            break;
                        default:
                            if (this._retrycount >= this._maxretrycount)
                            {
                                Console.WriteLine("Graph client error: " + e.Message);
                                Console.WriteLine("Retry limit hit. Aborting operation");
                                Console.WriteLine(e.Message);
                                break;
                            }

                            if (e.StatusCode == HttpStatusCode.ServiceUnavailable)
                            {
                                Console.WriteLine("Service unavailable. Will retry in 60 seconds");
                                this._retrycount++;
                                await Task.Delay(60000);
                            }
                            else
                            {
                                Console.WriteLine("Graph client error: " + e.Message);
                                break;
                            }
                            break;

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected Graph client error: " + e.Message);
                    break;
                }
            }
        }

        private async Task ResponseDelayAsync(TimeSpan? retryseconds)
        {
            if (retryseconds == null)
            {
                //retry in 60 seconds as a default times number of retries (plus 1 to stop 0 second delays)
                retryseconds = new TimeSpan(0, 0, 60 * (this._throttlingretrycount + 1) * (this._retrycount + 1));
            }

            // set the delay to the suggested back off plus a second to give a little buffer
            int delay = retryseconds.GetValueOrDefault().Milliseconds + 1000;

            Console.WriteLine("Throttling limits hit. Backing off for " + delay + " milliseconds");
            await Task.Delay(delay);
        }
    }
}
