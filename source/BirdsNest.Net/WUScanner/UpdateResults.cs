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
using Microsoft.UpdateServices.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WUScanner
{
    public class UpdateResults
    {
        public List<object> Updates { get; private set; } = new List<object>();

        /// <summary>
        /// Mappings of superseded updates. Key Value pairs in format update_id,superseding_update_id
        /// </summary>
        public List<object> SupersededUpdates { get; private set; } = new List<object>();
        public List<object> BundledUpdates { get; private set; } = new List<object>();
    }
}
