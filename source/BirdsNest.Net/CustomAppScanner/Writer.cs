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
#endregion
using System.Collections.Generic;
using Neo4j.Driver.V1;
using System;
using System.Text;
using common;

namespace CustomAppScanner
{
    public class Writer
    {
        public int UpdateApplication(Application app, ISession session)
        {
            string query = "MERGE (app:" + Types.Application +" {name:$Name}) "+
                "SET app.lastscan=$ScanId " +
                "RETURN app";

            IStatementResult result = session.WriteTransaction(tx => tx.Run(query, app));
            return result.Summary.Counters.NodesDeleted;
        }
    }
}
