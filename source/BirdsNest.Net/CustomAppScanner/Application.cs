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
using Newtonsoft.Json;
using ADScanner.ActiveDirectory;
using FSScanner;

namespace CustomAppScanner
{
    public class Application
    {
        public string ScanId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("business_owner")]
        public ADUser BusinessOwner { get; set; }

        [JsonProperty("technical_sme")]
        public ADUser TechnicalSME { get; set; }

        [JsonProperty("access_groups")]
        public List<ADGroup> AccessGroups { get; set; }

        [JsonProperty("svc_users")]
        public List<ADUser> SvcUsers { get; set; }

        [JsonProperty("folders_read")]
        public List<Folder> FoldersToReadFrom { get; set; }

        [JsonProperty("folders_write")]
        public List<Folder> FoldersToWriteTo { get; set; }
    }
}
