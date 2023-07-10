#region license
// Copyright (c) 2019-2023 "20Road"
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
using Microsoft.UpdateServices.Administration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WUScanner
{
    public static class Update
    {
        public static object GetUpdateObject(IUpdate wsusupdate)
        {
            string kb = string.Join(", ", wsusupdate.KnowledgebaseArticles.Cast<string>().ToList());
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("KB", kb);
            properties.Add("IsSuperseded", wsusupdate.IsSuperseded);
            properties.Add("HasSupersededUpdates", wsusupdate.HasSupersededUpdates);
            properties.Add("IsDeclined", wsusupdate.IsDeclined);
            properties.Add("Description", wsusupdate.Description);
            properties.Add("ID", wsusupdate.Id.UpdateId.ToString());
            properties.Add("AdditionalInformationUrls", string.Join(", ", wsusupdate.AdditionalInformationUrls.Cast<string>().ToList()));
            properties.Add("CreationDate", wsusupdate.CreationDate.ToString());
            properties.Add("UpdateType", GetUpdateType(wsusupdate.UpdateType));
            properties.Add("Name", wsusupdate.Title);
            properties.Add("Classification", wsusupdate.UpdateClassificationTitle);
            properties.Add("Products", string.Join(", ", wsusupdate.ProductTitles.Cast<string>().ToList()));

            return properties;
        }

        public static object GetRelatedUpdateObject(IUpdate update, IUpdate relatedupdate)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("updateid", update.Id.UpdateId.ToString());
            properties.Add("relatedid", relatedupdate.Id.UpdateId.ToString());
            return properties;
        }

        public static string GetUpdateType(UpdateType type)
        {
            switch (type)
            {
                case UpdateType.Software:
                    return "Software";
                case UpdateType.Driver:
                    return "Driver";
                case UpdateType.DriverSet:
                    return "DriverSet";
                case UpdateType.SoftwareApplication:
                    return "SoftwareApplication";
                default:
                    throw new ArgumentException("Invalid UpdateType passed");
            }

        }
    }
}
