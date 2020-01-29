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
//
#endregion
using common;
using Microsoft.Graph;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AzureADScanner.Azure
{
    public class AadGroupMemberships: IDataCollector
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
            NeoQueryData querydata = new NeoQueryData();
            Dictionary<string, string> idmappings = new Dictionary<string, string>();  //mapping dictionary for which groupid is associated with which request id
            List<object> propertylist = new List<object>();
            int requestcount = 0;

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + Connector.Instance.GetToken().Result);

                while (this.GroupIDs?.Count > 0)
                {
                    //create the 20 batch requests
                    BatchRequestContent batchRequestContent = new BatchRequestContent();
                    List<string> batchIDs = ListExtensions.ListPop<string>(this.GroupIDs, 20);
                    foreach (string groupid in batchIDs)
                    {
                        requestcount++;
                        string requestid = requestcount.ToString();
                        idmappings.Add(requestid, groupid);

                        HttpRequestMessage httpRequestMessage1 = new HttpRequestMessage(HttpMethod.Get, Connector.Instance.RootUrl + "/groups/" + groupid + "/members?$select=id");
                        batchRequestContent.AddBatchRequestStep(new BatchRequestStep(requestid, httpRequestMessage1, null));
                    }

                    List<object> newprops = MakeBatchRequest(httpClient, batchRequestContent, idmappings);
                    propertylist.AddRange(newprops);
                }
            }

            querydata.Properties = propertylist;
            return querydata;
        }

        private List<object> MakeBatchRequest(HttpClient httpClient, BatchRequestContent batchRequestContent, Dictionary<string, string> idmappings)
        {
            List<object> propertylist = new List<object>();

            //make the batch request
            string requesturl = Connector.Instance.RootUrl + "/$batch";
            while (string.IsNullOrWhiteSpace(requesturl) == false)
            {
                HttpResponseMessage response = httpClient.PostAsync(requesturl, batchRequestContent).Result;
                BatchResponseContent batchResponseContent = new BatchResponseContent(response);
                Dictionary<string, HttpResponseMessage> responses = batchResponseContent.GetResponsesAsync().Result;

                foreach (string key in responses.Keys)
                {
                    string groupid;
                    if (idmappings.TryGetValue(key, out groupid))
                    {
                        HttpResponseMessage httpResponseMsg;

                        if (responses.TryGetValue(key, out httpResponseMsg))
                        {
                            string rawdata = httpResponseMsg.Content.ReadAsStringAsync().Result;
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
                requesturl = batchResponseContent.GetNextLinkAsync().Result;
            }

            return propertylist;
        }
    }
}
