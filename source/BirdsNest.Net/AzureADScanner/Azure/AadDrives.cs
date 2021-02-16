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
    public class AadDrives : IDataCollectorAsync
    {
        public List<string> DriveIDs { get; set; }

        public string ProgressMessage { get { return "Creating drive nodes: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                    "MATCH(item:" + Types.AadObject + " { id: prop.ItemID}) " +
                    "MERGE(drive:" + Types.AadDrive + " { id: prop.ID}) " +
                    "SET drive.description = prop.Description " +
                    "SET drive.drivetype = prop.DriveType " +
                    "SET drive.name = prop.Name " +
                    "MERGE p = (item)-[r:" + Types.AadHasDrive + "]->(drive) " +
                    "SET r.lastscan = $ScanID " +
                    "SET r.scannerid = $ScannerID " +
                    "RETURN p";
            }
        }

        public NeoQueryData CollectData()
        {
            return this.CollectDataAsync().Result;
        }

        public async Task<NeoQueryData> CollectDataAsync()
        {
            this.DriveIDs = new List<string>();

            NeoQueryData querydata = new NeoQueryData();
            querydata.Properties = new List<object>();

            //requests.Add(Connector.Instance.Client.Drives.Request().Top(999));
            foreach (string id in AadGroups.Instance.O365GroupIDs) { querydata.Properties.AddRange( await ProcessRequestAsync(id, Connector.Instance.Client.Groups[id].Drives.Request().Top(999))); }
            foreach (string id in AadSites.Instance.SiteIDs) { querydata.Properties.AddRange(await ProcessRequestAsync(id, Connector.Instance.Client.Sites[id].Drives.Request().Top(999))); }
            foreach (string id in AadUsers.Instance.UserIDs) { querydata.Properties.AddRange(await ProcessRequestAsync(id, Connector.Instance.Client.Users[id].Drives.Request().Top(999))); }

            return querydata;
        }

        private async Task<List<object>> ProcessRequestAsync(string id, dynamic request ) {
            List<object> props = new List<object>();
            dynamic page = null;

            await Connector.Instance.MakeGraphClientRequestAsync(async () =>
            {
                page = await request.GetAsync();
            });

            while (page != null)
            {
                foreach (Drive drive in page.CurrentPage)
                {
                    this.DriveIDs.Add(drive.Id);

                    props.Add(new
                    {
                        ID = drive.Id,
                        Description = drive.Description,
                        DriveType = drive.DriveType,
                        Name = drive.Name,
                        ItemID = id
                    });
                }

                if (page.NextPageRequest == null) { break; }

                await Connector.Instance.MakeGraphClientRequestAsync(async () =>
                {
                    Console.Write(".");
                    page = await page.NextPageRequest.GetAsync();
                });
            }

            return props;
        }
    }
}
