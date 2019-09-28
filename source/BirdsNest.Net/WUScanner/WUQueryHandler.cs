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
        public static void PopulateUpdateResults(IUpdateServer server, UpdateResults resultobject)
        {
            UpdateCollection declined = server.GetUpdates(ApprovedStates.Declined, DateTime.MinValue, DateTime.MaxValue, null, null);
            UpdateCollection notapproved = server.GetUpdates(ApprovedStates.NotApproved, DateTime.MinValue, DateTime.MaxValue, null, null);
            UpdateCollection approved = server.GetUpdates(ApprovedStates.LatestRevisionApproved, DateTime.MinValue, DateTime.MaxValue, null, null);
            UpdateCollection staleapproval = server.GetUpdates(ApprovedStates.HasStaleUpdateApprovals, DateTime.MinValue, DateTime.MaxValue, null, null);

            //foreach (UpdateCollection col in new List<UpdateCollection> { declined, notapproved, approved, staleapproval })
            foreach (UpdateCollection col in new List<UpdateCollection> { declined})
            {
                foreach (IUpdate update in col)
                {
                    resultobject.Updates.Add(new Update(update) as object);
                    if (update.IsSuperseded)
                    {
                        //update.Id.UpdateId.
                        resultobject.SupersededUpdates.AddRange(GetRelatedUpdates(update, UpdateRelationship.UpdatesThatSupersedeThisUpdate));
                    }
                }
            }
        }


        public static List<KeyValuePair<string, string>> GetRelatedUpdates(IUpdate update, UpdateRelationship type)
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
