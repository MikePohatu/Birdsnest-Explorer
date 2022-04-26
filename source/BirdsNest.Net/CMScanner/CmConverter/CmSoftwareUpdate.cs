﻿#region license
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
using common;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Neo4j.Driver;
using System.Collections.Generic;

namespace CMScanner.CmConverter
{
    public class CmSoftwareUpdate : IDataCollector
    {
        public string ProgressMessage { get { return "Creating software update nodes"; } }
        public string Query
        {
            get
            {
                return "UNWIND $Properties AS prop " +
                "MERGE (n:" + Types.CMSoftwareUpdate + "{id:prop.ID}) " +
                "SET n:" + Types.CMConfigurationItem + " " +
                "SET n.name = prop.Name " +
                "SET n.ArticleID = prop.ArticleID " +
                "SET n.BulletinID = prop.BulletinID " +
                "SET n.DateCreated = prop.DateCreated " +
                "SET n.DateRevised = prop.DateRevised " +
                "SET n.IsBundle = prop.IsBundle " +
                "SET n.IsDeployed = prop.IsDeployed " +
                "SET n.IsExpired = prop.IsExpired " +
                "SET n.IsLatest = prop.IsLatest " +
                "SET n.IsSuperseded = prop.IsSuperseded " +
                "SET n.Description = prop.Description " +
                "SET n.NumMissing = prop.NumMissing " +
                "SET n.PercentCompliant = prop.PercentCompliant " +
                "SET n.SeverityName = prop.SeverityName " +
                "SET n.Size = prop.Size " +
                "SET n.lastscan=$ScanID " +
                "SET n.scannerid=$ScannerID " +
                "RETURN n.name";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            List<object> propertylist = new List<object>();

            try
            {
                // This query selects all collections
                string cmquery = "select * from SMS_SoftwareUpdate";

                // Run query
                using (IResultObject results = Connector.Instance.Connection.QueryProcessor.ExecuteQuery(cmquery))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        propertylist.Add(new
                        {
                            ID = ResultObjectHandler.GetString(resource, "CI_ID"),
                            ArticleID = ResultObjectHandler.GetString(resource, "ArticleID"),
                            BulletinID = ResultObjectHandler.GetString(resource, "BulletinID"),
                            DateCreated = ResultObjectHandler.GetDateTime(resource, "DateCreated").ToString(),
                            DateRevised = ResultObjectHandler.GetDateTime(resource, "DateRevised").ToString(),
                            Name = ResultObjectHandler.GetString(resource, "LocalizedDisplayName"),
                            IsBundle = ResultObjectHandler.GetBool(resource, "IsBundle"),
                            IsDeployed = ResultObjectHandler.GetBool(resource, "IsDeployed"),
                            IsExpired = ResultObjectHandler.GetBool(resource, "IsExpired"),
                            IsLatest = ResultObjectHandler.GetBool(resource, "IsLatest"),
                            IsSuperseded = ResultObjectHandler.GetBool(resource, "IsSuperseded"),
                            Description = ResultObjectHandler.GetString(resource, "LocalizedDescription"),
                            NumMissing = ResultObjectHandler.GetInt(resource, "NumMissing"),
                            PercentCompliant = ResultObjectHandler.GetInt(resource, "PercentCompliant"),
                            SeverityName = ResultObjectHandler.GetString(resource, "SeverityName"),
                            Size = ResultObjectHandler.GetInt(resource, "Size")
                        });
                    }
                }
            }
            catch { }

            querydata.Properties = propertylist;
            return querydata;
        }

        public string GetSummaryString(IResultSummary summary)
        {
            return summary.Counters.NodesCreated + " created";
        }

        public static CmSoftwareUpdate GetInstance() { return new CmSoftwareUpdate(); }
    }
}
