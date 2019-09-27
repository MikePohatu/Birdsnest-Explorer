using Microsoft.UpdateServices.Administration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WUScanner
{
    public static class WUQueryHandler
    { 



        public static List<KeyValuePair<string, string>> GetRelatedUpdates(IUpdateServer server, IUpdate update, UpdateRelationship type)
        {
            List<KeyValuePair<string, string>> ret = new List<KeyValuePair<string, string>>();
            foreach (IUpdate relupdate in update.GetRelatedUpdates(type))
            {
                var kvpair = new KeyValuePair<string, string>(update.Id.UpdateId.ToString(), relupdate.Id.UpdateId.ToString());
                ret.Add(kvpair);
            }

            return ret;
        }
    }
}
