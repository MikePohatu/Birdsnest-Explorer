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
    public class AadSites : IDataCollectorAsync
    {
        public static AadSites Instance { get; } = new AadSites();
        private AadSites() { }

        public List<string> SiteIDs { get; set; }

        public string ProgressMessage { get { return "Creating site nodes: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.AadSite + " {id:prop.ID}) " +
                "SET n:" + Types.AadObject + " " +
                "SET n.name = prop.Name " +
                "SET n.sitename = prop.SiteName " +
                "SET n.displayname = prop.DisplayName " +
                "SET n.weburl = prop.WebUrl " +
                "SET n.description = prop.Description " +
                "SET n.lastscan=$ScanID " +
                "SET n.scannerid=$ScannerID " +
                "SET n.layout='mesh' " +
                "RETURN n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            return this.CollectDataAsync().GetAwaiter().GetResult();
        }

        public async Task<NeoQueryData> CollectDataAsync()
        {
            this.SiteIDs = new List<string>();
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            IGraphServiceSitesCollectionRequest request = Connector.Instance.Client.Sites.Request();
            request.Top(999);

            IGraphServiceSitesCollectionPage page = null;

            await Connector.Instance.MakeGraphClientRequestAsync(async () =>
            {
                page = await request.GetAsync();
            });

            while (page != null)
            {
                foreach (Site site in page.CurrentPage)
                {
                    this.SiteIDs.Add(site.Id); //record the site ID so it can be used later
                    string sitename = string.IsNullOrEmpty(site.DisplayName) ? site.Name : site.DisplayName;
                    sitename = string.IsNullOrEmpty(sitename) ? site.WebUrl : sitename;

                    propertylist.Add(new
                    {
                        ID = site.Id,
                        WebUrl = site.WebUrl,
                        DisplayName = site.DisplayName,
                        Name = sitename,
                        SiteName = site.Name,
                        Description = site.Description
                    });
                }

                if (page.NextPageRequest == null) { break; }

                await Connector.Instance.MakeGraphClientRequestAsync(async () =>
                {
                    page = await page.NextPageRequest.GetAsync();
                });
            }

            querydata.Properties = propertylist;
            return querydata;
        }
    }
}
