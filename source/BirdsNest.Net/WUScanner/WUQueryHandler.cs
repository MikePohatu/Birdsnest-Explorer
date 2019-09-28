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

            foreach (UpdateCollection col in new List<UpdateCollection> { declined, notapproved, approved, staleapproval })
            //foreach (UpdateCollection col in new List<UpdateCollection> { declined})
            {
                foreach (IUpdate update in col)
                {
                    resultobject.Updates.Add(Update.GetUpdateObject(update));
                    if (update.IsSuperseded)
                    {
                        List<object> supersededmappings = GetRelatedUpdates(update, UpdateRelationship.UpdatesThatSupersedeThisUpdate);
                        resultobject.SupersededUpdates.AddRange(supersededmappings);
                    }
                }
            }
        }


        public static List<object> GetRelatedUpdates(IUpdate update, UpdateRelationship type)
        {
            List<object> ret = new List<object>();
            UpdateCollection related = update.GetRelatedUpdates(type);
            foreach (IUpdate relupdate in related)
            {
                object o = Update.GetRelatedUpdateObject(update, relupdate);
                ret.Add(o);
            }

            return ret;
        }
    }
}
