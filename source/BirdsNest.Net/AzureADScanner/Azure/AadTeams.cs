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
using System.Text;
using System.Threading.Tasks;

namespace AzureADScanner.Azure
{
    public class AadTeams : IDataCollectorAsync
    {
        public string ProgressMessage { get { return "Creating Team nodes: "; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.AadTeam + " { id:prop.ID}) " +
                "SET n:" + Types.AadObject + " " +
                "SET n.name = prop.Name " +
                "SET n.description = prop.Description " +
                "SET n.isarchived = prop.IsArchived " +
                "SET n.lastscan=$ScanID " +
                "SET n.scannerid=$ScannerID " +
                "RETURN n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            return this.CollectDataAsync().GetAwaiter().GetResult();
        }

        public async Task<NeoQueryData> CollectDataAsync()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            IGraphServiceTeamsCollectionRequest request = Connector.Instance.Client.Teams.Request();
            request.Top(999);

            IGraphServiceTeamsCollectionPage page = null;

            await Connector.Instance.MakeGraphClientRequestAsync(async () =>
            {
                page = await request.GetAsync();
            });

            while (page != null)
            {
                foreach (Team team in page.CurrentPage)
                {
                    propertylist.Add(new
                    {
                        ID = team.Id,
                        Description = team.Description,
                        IsArchived = team.IsArchived,
                        Name = team.DisplayName
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
