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
using common;
using Microsoft.Graph;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureADScanner.Azure
{
    public class AadGroupMemberships: IDataCollectorAsync
    {
        public List<string> GroupIDs { get; set; }

        public string ProgressMessage { get { return "Creating group membership connections: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                    "MATCH(g:" +Types.AadGroup+" { id: prop.groupid}) " +
                    "MATCH(m:" + Types.AadObject + " { id: prop.memberid}) " +
                    "MERGE p = (m)-[r:" +Types.AadMemberOf+"]->(g) " +
                    "SET r.lastscan = $ScanID " +
                    "SET r.scannerid = $ScannerID " +
                    "RETURN p";
            }
        }

        public NeoQueryData CollectData()
        {
            return this.CollectDataAsync().GetAwaiter().GetResult();
        }

        public async Task<NeoQueryData> CollectDataAsync()
        {
            NeoQueryData querydata = new NeoQueryData();
            if (this.GroupIDs?.Count > 0)
            {
                Dictionary<string, string> idmappings = new Dictionary<string, string>();  //mapping dictionary for which groupid is associated with which request id
                List<object> propertylist = new List<object>();
                int requestcount = 0;

                var enumerator = this.GroupIDs.GetEnumerator();
                BatchRequestContent batchRequestContent;

                using (HttpClient httpClient = await Connector.Instance.GetHttpClientWithToken())
                {
                    while (enumerator.MoveNext())
                    {
                        batchRequestContent = new BatchRequestContent();

                        //create the 20 batch requests
                        for (int i = 0; i < 20; i++)
                        {
                            string groupid = enumerator.Current;
                            string requestid = requestcount.ToString();
                            idmappings.Add(requestid, groupid);
                            HttpRequestMessage requestmessage = new HttpRequestMessage(HttpMethod.Get, Connector.Instance.RootUrl + "/groups/" + groupid + "/members?$select=id");
                            batchRequestContent.AddBatchRequestStep(new BatchRequestStep(requestid, requestmessage, null));

                            requestcount++;

                            if (enumerator.MoveNext() == false) { break; }
                        }

                        // now make the request, passing in the anonymous response handler
                        await Connector.Instance.MakeBatchRequest(httpClient, batchRequestContent, async (Dictionary<string, HttpResponseMessage> responses) =>
                        {
                            foreach (string key in responses.Keys)
                            {
                                string groupid;
                                if (idmappings.TryGetValue(key, out groupid))
                                {
                                    HttpResponseMessage httpResponseMsg;

                                    if (responses.TryGetValue(key, out httpResponseMsg))
                                    {
                                        string rawdata = await httpResponseMsg.Content.ReadAsStringAsync();
                                        BatchRequestResponseSimple simpleresponse = JsonConvert.DeserializeObject<BatchRequestResponseSimple>(rawdata);
                                        foreach (BatchRequestResponseValueSimple val in simpleresponse.Values)
                                        {
                                            propertylist.Add(new
                                            {
                                                groupid = groupid,
                                                memberid = val.ID
                                            });
                                        }
                                    }
                                }
                            }
                        });

                    }
                }

                querydata.Properties = propertylist;
            }
            
            return querydata;
        }
    }
}
