﻿using common;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMScanner.CmConverter
{
    public class CmUserAdConnections : ICmCollector
    {
        public string ProgressMessage { get { return "Creating CM to AD user mappings: "; } }
        public string Query
        {
            get
            {
                return "MATCH (user:" + Types.CMUser + ") " +
                "MATCH (aduser:" + Types.User + " {id: user.sid }) " +
                "MERGE p=(aduser)-[r:" + Types.CMHasObject + "]->(user) " +
                "SET r.lastscan=$ScanID " +
                "SET r.scannerid=$ScannerID " +
                "RETURN p";
            }
        }

        public NeoQueryData CollectData()
        {
            NeoQueryData querydata = new NeoQueryData();
            return querydata;
        }

        public string GetSummaryString(IResultSummary summary)
        {
            return summary.Counters.RelationshipsCreated + " created";
        }
    }
}
