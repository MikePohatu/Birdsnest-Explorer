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
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("IsSuperseded", wsusupdate.IsSuperseded);
            properties.Add("HasSupersededUpdates", wsusupdate.HasSupersededUpdates);
            properties.Add("IsDeclined", wsusupdate.IsDeclined);
            properties.Add("Title", wsusupdate.Title);
            properties.Add("Description", wsusupdate.Description);
            properties.Add("ID", wsusupdate.Id.UpdateId.ToString());
            properties.Add("AdditionalInformationUrls", string.Join(", ", wsusupdate.AdditionalInformationUrls));
            properties.Add("CreationDate", wsusupdate.CreationDate.ToString());
            properties.Add("UpdateType", wsusupdate.UpdateType.ToString());


            return properties;
        }

        public static object GetRelatedUpdateObject(IUpdate update, IUpdate relatedupdate)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties.Add("updateid", update.Id.UpdateId.ToString());
            properties.Add("relatedid", relatedupdate.Id.UpdateId.ToString());
            return properties;
        }
    }
}
