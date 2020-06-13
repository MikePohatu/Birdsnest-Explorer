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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace common
{
    public static class NeoWriter
    {
        public static string ScanID { get; set; }

        public static IResultSummary RunQuery(string query, NeoQueryData data, ISession session)
        {
            if (string.IsNullOrWhiteSpace(data.ScanID))
            {
                data.ScanID = NeoWriter.ScanID;
            }

            var result = session.WriteTransaction(tx => tx.Run(query, data));
            return result.Summary;
        }
    }
}
