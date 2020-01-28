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
using System.Text;
using System.Threading.Tasks;

namespace AzureADScanner.Azure
{
    public class AadUserToAdUserConnections : IDataCollector
    {
        public string ProgressMessage { get { return "Creating AD user to AAD user connections: "; } }
        public string Query
        {
            get
            {
                return "MATCH (n:" + Types.User + ") " +
                    "WITH collect(DISTINCT n) as adusers " +
                    "UNWIND adusers as user " +
                    "MATCH(aaduser: "+Types.AadUser+" { userprincipalname: user.userprincipalname}) " +
                    "MERGE p = (user) -[r:"+Types.AadSync+"]->(aaduser) " +
                    "SET r.lastscan = $ScanID " +
                    "SET r.scannerid = $ScannerID " +
                    "RETURN p";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();
            querydata.Properties = propertylist;
            return querydata;
        }
    }
}
