﻿using System.Collections.Generic;
using System.DirectoryServices;
using ADScanner.Neo4j;

namespace ADScanner.ActiveDirectory
{
    /// <summary>
    /// ADEntity is the base class for User and Computer classes
    /// </summary>
    public abstract class ADEntity: ADGroupMemberObject
    {
        public string PrimaryGroupID { get; private set; }

        public ADEntity(SearchResult result, string scanid) : base(result, scanid)
        {
            this.PrimaryGroupID = ADSearchResultConverter.GetSinglestringValue(result, "primaryGroupID");
            this.Properties.Add("primarygroupid", this.PrimaryGroupID);
        }
    }
}