using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UpdateServices.Administration;

namespace WUScanner
{
    public static class Update
    {
        public static object GetUpdateObject (IUpdate wsusupdate)
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
