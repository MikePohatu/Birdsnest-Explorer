using System.Linq;
using System.Text;
using System.Collections.Generic;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

namespace CMScanner.Sccm
{
    public class SccmConnector
    {
        private WqlConnectionManager _connection = new WqlConnectionManager();

        /// <summary>
        /// Connect using the current application user context
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public bool Connect(string server)
        {
            try { this._connection.Connect(server); }
            catch { return false; }
            return true;
        }

        /// <summary>
        /// Connect using the specified connection credentials
        /// </summary>
        /// <param name="authuser"></param>
        /// <param name="authpw"></param>
        /// <param name="authdomain"></param>
        /// <param name="server"></param>
        /// <returns></returns>
        public bool Connect(string authuser, string authpw, string authdomain, string server)
        {
            try { this._connection?.Connect(server, authdomain + "\\" + authuser, authpw); }
            catch { return false; }
            return true;
        }

        public List<SccmCollection> GetCollections()
        {
            List<SccmCollection> cols = new List<SccmCollection>();

            try
            {
                // This query selects all collections
                string query = "select * from SMS_Collection";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmCollection col = Factory.GetCollectionFromSMS_CollectionResults(resource);
                        cols.Add(col);
                    }
                }
            }
            catch { }
            return cols;
        }


        public List<SccmApplication> GetApplications()
        {
            List<SccmApplication> applications = new List<SccmApplication>();
            try
            {
                // This query selects all collections
                string query = "select * from SMS_Application WHERE IsLatest='TRUE'";
                
                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmApplication app = Factory.GetApplicationFromSMS_ApplicationResults(resource);
                        applications.Add(app);
                    }
                }
                
            }
            catch { }
            return applications;
        }

        public List<SccmApplicationRelationship> GetApplicationRelationships(string applicationciid)
        {
            List<SccmApplicationRelationship> items = new List<SccmApplicationRelationship>();
            try
            {
                // This query selects all relationships
                string query = "select * from SMS_AppDependenceRelation";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmApplicationRelationship apprel = Factory.GetAppRelationshipFromSMS_AppDependenceRelationResults(resource);
                        items.Add(apprel);
                    }
                }
            }
            catch { }
            return items;
        }

        public List<SMS_DeploymentSummary> GetApplicationDeployments()
        {
            List<SMS_DeploymentSummary> items = new List<SMS_DeploymentSummary>();
            
            try
            {
                // This query selects all relationships of the specified app ID
                string query = "select * from SMS_DeploymentSummary WHERE FeatureType=" + (int)SccmItemType.Application;

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SMS_DeploymentSummary dep = Factory.GetDeploymentSummaryFromSMS_DeploymentSummaryResults(resource);
                        items.Add(dep);
                    }
                }
            }
            catch { }
            return items;
        }

        public List<SMS_DeploymentSummary> GetPackageProgramDeployments()
        {
            List<SMS_DeploymentSummary> items = new List<SMS_DeploymentSummary>();

            try
            {
                // This query selects all relationships of the specified app ID
                string query = "select * from SMS_DeploymentSummary WHERE FeatureType='" + SccmItemType.PackageProgram + "'";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SMS_DeploymentSummary dep = Factory.GetDeploymentSummaryFromSMS_DeploymentSummaryResults(resource);
                        items.Add(dep);
                    }
                }
            }
            catch { }
            return items;
        }

        public List<SMS_DeploymentSummary> GetDeploymentSummarys()
        {
            List<SMS_DeploymentSummary> items = new List<SMS_DeploymentSummary>();

            try
            {
                // This query selects all relationships of the specified app ID
                string query = "select * from SMS_DeploymentSummary";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SMS_DeploymentSummary dep = Factory.GetDeploymentSummaryFromSMS_DeploymentSummaryResults(resource);
                        items.Add(dep);
                    }
                }
            }
            catch { }
            return items;
        }


        public List<SccmTaskSequence> GetTaskSequences()
        {
            List<SccmTaskSequence> items = new List<SccmTaskSequence>();
            try
            {
                string query = "select * from SMS_TaskSequencePackage";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmTaskSequence item = Factory.GetTaskSequenceFromSMS_TaskSequenceResults(resource);
                        items.Add(item);
                    }
                }
                items = items.OrderBy(o => o.Name).ToList();
            }
            catch { }
            return items;
        }

        public List<SccmDevice> GetAllDevices()
        {
            List<SccmDevice> devices = new List<SccmDevice>();
            try
            {
                string query = "select * from SMS_R_System";
                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        devices.Add(SccmDevice.GetSccmDevice(resource));
                    }
                }
            }
            catch
            { }
            return devices;
        }

        public List<SccmUser> GetAllUsers()
        {
            List<SccmUser> users = new List<SccmUser>();
            try
            {
                string query = "select * from SMS_R_User";
                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        users.Add(SccmUser.GetSccmUser(resource));
                    }
                }
            }
            catch
            { }
            return users;
        }

        public List<object> GetCollectionMemberships()
        {
            List<object> retlist = new List<object>();
            try
            {
                string query = "select CollectionID,ResourceID from SMS_FullCollectionMembership";
                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        string colids = resource["CollectionID"].StringValue;
                        string resourceid = resource["ResourceID"].StringValue;

                        foreach (string collectionid in colids.Split(','))
                        {
                            retlist.Add(new { resourceid, collectionid });
                        }
                    }
                }
            }
            catch
            { }
            return retlist;
        }


        public List<SccmPackage> GetPackages()
        {
            List<SccmPackage> items = new List<SccmPackage>();
            try
            {
                // This query selects all collections
                int type = (int)PackageType.RegularSoftwareDistribution;

                string query = "select * from SMS_PackageBaseclass WHERE PackageType='" + type + "'";
                             

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmPackage item = Factory.GetPackageFromSMS_PackageBaseclassResults(resource);
                        if (item != null) { items.Add(item); }
                    }
                }
                items = items.OrderBy(o => o.Name).ToList();
            }
            catch { }
            return items;
        }



        public List<SccmPackageProgram> GetPackagePrograms()
        {
            List<SccmPackageProgram> items = new List<SccmPackageProgram>();
            try
            {
                // This query selects all collections
                int type = (int)PackageType.RegularSoftwareDistribution;

                string query = "select * from SMS_Program";


                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmPackageProgram item = Factory.GetPackageProgramFromSMS_ProgramResults(resource);
                        if (item != null) { items.Add(item); }
                    }
                }
                items = items.OrderBy(o => o.Name).ToList();
            }
            catch { }
            return items;
        }
        public List<IDeployment> GetSMS_DeploymentInfoDeployments(string targettame, SccmItemType itemtype)
        {
            List<IDeployment> items = new List<IDeployment>();

            try
            {
                // This query selects all relationships of the specified app ID
                int type = (int)itemtype;
                string query = "select * from SMS_DeploymentInfo WHERE TargetName='" + targettame + "' AND DeploymentType='" + type.ToString() + "'";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SMS_DeploymentInfo dep = Factory.GetDeploymentInfoFromSMS_DeploymentInfoResults(resource);
                        items.Add(dep);
                    }
                }
                //items = items.OrderBy(o => o.Name).ToList();
            }
            catch { }
            return items;
        }

        public List<SccmConfigurationBaseline> GetConfigurationBaselinesListFromSearch(string search)
        {
            List<SccmConfigurationBaseline> items = new List<SccmConfigurationBaseline>();
            try
            {
                // This query selects all collections

                string query;
                if (string.IsNullOrWhiteSpace(search)) { query = "select * from SMS_ConfigurationBaselineInfo WHERE IsLatest='TRUE'"; }
                else { query = "select * from SMS_ConfigurationBaselineInfo WHERE LocalizedDisplayName LIKE '%" + search + "%'"; }

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmConfigurationBaseline item = Factory.GetConfigurationBaselineFromSMS_ConfigurationBaselineInfo(resource);
                        items.Add(item);
                    }
                }
                items = items.OrderBy(o => o.Name).ToList();
            }
            catch { }
            return items;
        }

        public List<ISccmObject> GetConfigurationBaselineSccmObjectsListFromSearch(string search)
        {
            List<ISccmObject> items = new List<ISccmObject>();
            try
            {
                // This query selects all collections

                string query;
                if (string.IsNullOrWhiteSpace(search)) { query = "select * from SMS_ConfigurationBaselineInfo"; }
                else { query = "select * from SMS_ConfigurationBaselineInfo WHERE LocalizedDisplayName LIKE '%" + search + "%'"; }

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmConfigurationBaseline item = Factory.GetConfigurationBaselineFromSMS_ConfigurationBaselineInfo(resource);
                        items.Add(item);
                    }
                }
                items = items.OrderBy(o => o.Name).ToList();
            }
            catch { }
            return items;
        }

        public List<SccmConfigurationBaseline> GetConfigurationBaselines()
        {
            List<SccmConfigurationBaseline> items = new List<SccmConfigurationBaseline>();
            try
            {
                string query = "select * from SMS_ConfigurationBaselineInfo";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmConfigurationBaseline item = Factory.GetConfigurationBaselineFromSMS_ConfigurationBaselineInfo(resource);
                        items.Add(item);
                    }
                }
            }
            catch { }
            return items;
        }

        public List<SMS_CIRelation> GetCIRelations(string fromciid)
        {
            List<SMS_CIRelation> items = new List<SMS_CIRelation>();
            try
            {
                // This query selects all collections

                string query = "select * from SMS_CIRelation where FromCIID ='" + fromciid + "'";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SMS_CIRelation item = Factory.GetCIRelationFromSMS_CIRelation(resource);
                        items.Add(item);
                    }
                }
            }
            catch { }
            return items;
        }

        public List<SccmCollection> GetIncrementalUpdateCollections()
        {
            List<SccmCollection> items = new List<SccmCollection>();
            try
            {
                string query = "select * from SMS_Collection WHERE RefreshType='6' AND CollectionID NOT LIKE 'SMS%'";

                // Run query
                using (IResultObject results = this._connection.QueryProcessor.ExecuteQuery(query))
                {
                    // Enumerate through the collection of objects returned by the query.
                    foreach (IResultObject resource in results)
                    {
                        SccmCollection item = Factory.GetCollectionFromSMS_CollectionResults(resource);
                        items.Add(item);
                    }
                }
            }
            catch { }
            return items;
        }
    }
}
