#region license
// Copyright (c) 2019-2020 "20Road"
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
using System.Text;
using System.Threading.Tasks;

namespace WUScanner
{
    public static class WUQueryHandler
    { 
        public static void PopulateUpdateResults(IUpdateServer server, UpdateResults resultobject)
        {
            Console.WriteLine("Reading update information. This may take several minutes. Please wait...");
            List<UpdateCollection> collections = new List<UpdateCollection>();
            var approvedstates = Enum.GetValues(typeof(ApprovedStates));

            //This breaks the query up. If we use a straight GetUpdates() it is likely to timeout. This 
            //allows the query to be filtered to give more control.
            foreach (IUpdateClassification classification in server.GetUpdateClassifications())
            {
                //Drivers cause timeouts so we skip them
                if (classification.Title == "Drivers") { continue; }
                Console.Write("Processing " + classification.Title);
                UpdateClassificationCollection classcol = new UpdateClassificationCollection();
                classcol.Add(classification);
                foreach (ApprovedStates state in approvedstates)
                {
                    //Console.WriteLine(state);
                    Console.Write(".");
                    collections.Add(server.GetUpdates(state, DateTime.MinValue, DateTime.MaxValue, null, classcol));
                }
                Console.WriteLine();
            }
            
            foreach (UpdateCollection col in collections)
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
