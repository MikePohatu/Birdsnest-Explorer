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
using System.Text;
using System.Threading.Tasks;

namespace AzureADScanner.Azure
{
    public class AadGroups: IDataCollectorAsync
    {
        public string ProgressMessage { get { return "Creating group nodes: "; } }
        public List<string> GroupIDs { get; private set; } = new List<string>();

        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.AadGroup + " {id:prop.ID}) " +
                "SET n:" + Types.AadObject + " " +
                "SET n.name = prop.Name " +
                "SET n.description = prop.Description " +
                "SET n.mailenabled = prop.MailEnabled " +
                "SET n.lastscan=$ScanID " +
                "SET n.scannerid=$ScannerID " +
                "SET n.layout='mesh' " +
                "RETURN n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            return this.CollectDataAsync().Result;
        }

        public async Task<NeoQueryData> CollectDataAsync()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                IGraphServiceGroupsCollectionPage page = await Connector.Instance.Client.Groups.Request().GetAsync();

                while (page != null)
                {
                    foreach (Group group in page.CurrentPage)
                    {
                        propertylist.Add(new
                        {
                            ID = group.Id,
                            Name = group.DisplayName,
                            Description = group.Description,
                            MailEnabled = group.MailEnabled
                        });

                        this.GroupIDs.Add(group.Id);
                    }

                    page = await page.NextPageRequest?.GetAsync();
                }

                
            }
            catch { }

            querydata.Properties = propertylist;
            return querydata;
        }
    }
}
