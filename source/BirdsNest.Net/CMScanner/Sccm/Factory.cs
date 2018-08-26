using Microsoft.ConfigurationManagement.ManagementProvider;

namespace CMScanner.Sccm
{
    public static class Factory
    {
        public static SccmDeployableItem GetSccmDeployableItemFromDeploymentSummary(SMS_DeploymentSummary deployment)
        {
            if (deployment.FeatureType == SccmItemType.Application)
            {
                var newitem = new SccmApplication();
                newitem.Name = deployment.SoftwareName;
                newitem.ID = deployment.CIID;
                return newitem;
            }

            else if (deployment.FeatureType == SccmItemType.SoftwareUpdateGroup)
            {
                var newitem = new SccmSoftwareUpdateGroup();
                newitem.Name = deployment.SoftwareName;
                newitem.ID = deployment.CIID;
                return newitem;
            }

            else if (deployment.FeatureType == SccmItemType.SoftwareUpdate)
            {
                var newitem = new SccmSoftwareUpdate();
                newitem.Name = deployment.SoftwareName;
                newitem.ID = deployment.CIID;
                return newitem;
            }

            else if (deployment.FeatureType == SccmItemType.TaskSequence)
            {
                var newitem = new SccmTaskSequence();
                newitem.Name = deployment.SoftwareName;
                newitem.ID = deployment.PackageID;
                return newitem;
            }

            else if (deployment.FeatureType == SccmItemType.PackageProgram)
            {
                var newitem = new SccmPackageProgram();
                newitem.Name = deployment.SoftwareName;
                newitem.ID = deployment.PackageID;
                return newitem;
            }

            else if (deployment.FeatureType == SccmItemType.ConfigurationBaseline)
            {
                var newitem = new SccmConfigurationBaseline();
                newitem.Name = deployment.SoftwareName;
                newitem.ID = deployment.CIID;
                return newitem;
            }
            return null;
        }

        public static SccmDeployableItem GetSccmDeployableItemFromSMS_DeploymentSummaryResults(IResultObject resource)
        {
            SccmItemType featuretype = (SccmItemType)ResultObjectHandler.GetInt(resource, "FeatureType");

            if (featuretype == SccmItemType.Application)
            {
                var newitem = new SccmApplication();
                newitem.Name = ResultObjectHandler.GetString(resource,"SoftwareName");
                newitem.ID = ResultObjectHandler.GetString(resource, "CI_ID");
                return newitem;
            }

            else if (featuretype == SccmItemType.SoftwareUpdateGroup)
            {
                var newitem = new SccmSoftwareUpdateGroup();
                newitem.Name = ResultObjectHandler.GetString(resource, "SoftwareName");
                newitem.ID = ResultObjectHandler.GetString(resource, "CI_ID");
                return newitem;
            }

            else if (featuretype == SccmItemType.SoftwareUpdate)
            {
                var newitem = new SccmSoftwareUpdate();
                newitem.Name = ResultObjectHandler.GetString(resource, "SoftwareName");
                newitem.ID = ResultObjectHandler.GetString(resource, "CI_ID");
                return newitem;
            }

            else if (featuretype == SccmItemType.TaskSequence)
            {
                var newitem = new SccmTaskSequence();
                newitem.Name = ResultObjectHandler.GetString(resource, "SoftwareName");
                newitem.ID = ResultObjectHandler.GetString(resource, "PackageID");
                return newitem;
            }

            else if (featuretype == SccmItemType.PackageProgram)
            {
                var newitem = new SccmPackageProgram();
                newitem.Name = ResultObjectHandler.GetString(resource, "SoftwareName");
                newitem.ID = ResultObjectHandler.GetString(resource, "PackageID");
                return newitem;
            }

            else if (featuretype == SccmItemType.ConfigurationBaseline)
            {
                var newitem = new SccmConfigurationBaseline();
                newitem.Name = ResultObjectHandler.GetString(resource, "SoftwareName");
                newitem.ID = ResultObjectHandler.GetString(resource, "CI_ID");
                return newitem;
            }
            return null;
        }

        public static SccmApplication GetApplicationFromSMS_ApplicationResults(IResultObject resource)
        {
            SccmApplication item = new SccmApplication();

            item.IsDeployed = ResultObjectHandler.GetBool(resource, "IsDeployed");
            item.IsEnabled = ResultObjectHandler.GetBool(resource, "IsEnabled");
            item.IsSuperseded = ResultObjectHandler.GetBool(resource, "IsSuperseded");
            item.IsSuperseding = ResultObjectHandler.GetBool(resource, "IsSuperseding");
            item.IsLatest = ResultObjectHandler.GetBool(resource, "IsLatest");
            item.ID = ResultObjectHandler.GetString(resource, "CI_ID");
            item.Name = ResultObjectHandler.GetString(resource, "LocalizedDisplayName");

            return item;
        }

        public static SccmCollection GetCollectionFromSMS_CollectionResults(IResultObject resource)
        {
            SccmCollection item = new SccmCollection();

            item.ID = ResultObjectHandler.GetString(resource, "CollectionID");
            item.Name = ResultObjectHandler.GetString(resource, "Name");
            item.LimitingCollectionID = ResultObjectHandler.GetString(resource, "LimitToCollectionID");
            item.Comment = ResultObjectHandler.GetString(resource, "Comment");
            item.IncludeExcludeCollectionCount = ResultObjectHandler.GetInt(resource, "IncludeExcludeCollectionsCount");
            int typeint = ResultObjectHandler.GetInt(resource, "CollectionType");
            item.CollectionType = (CollectionType)typeint;

            return item;
        }

        public static SccmApplicationRelationship GetAppRelationshipFromSMS_AppDependenceRelationResults(IResultObject resource)
        {
            SccmApplicationRelationship item = new SccmApplicationRelationship();
            item.FromApplicationCIID = ResultObjectHandler.GetString(resource, "FromApplicationCIID");
            item.ToApplicationCIID = ResultObjectHandler.GetString(resource, "ToApplicationCIID");
            item.ToDeploymentTypeCIID = ResultObjectHandler.GetString(resource, "ToDeploymentTypeCIID");
            item.FromDeploymentTypeCIID = ResultObjectHandler.GetString(resource, "FromDeploymentTypeCIID");
            item.SetType(ResultObjectHandler.GetInt(resource, "TypeFlag"));

            return item;
        }

        public static SMS_DeploymentSummary GetDeploymentSummaryFromSMS_DeploymentSummaryResults(IResultObject resource)
        {
            SMS_DeploymentSummary item = new SMS_DeploymentSummary();

            item.CollectionID = ResultObjectHandler.GetString(resource, "CollectionID");
            item.CollectionName = ResultObjectHandler.GetString(resource, "CollectionName");
            item.DeploymentID = ResultObjectHandler.GetString(resource, "DeploymentID");
            item.DeploymentIntent = ResultObjectHandler.GetInt(resource, "DeploymentIntent");
            item.SoftwareName = ResultObjectHandler.GetString(resource, "SoftwareName");
            item.PackageID = ResultObjectHandler.GetString(resource, "PackageID");
            item.CIID = ResultObjectHandler.GetString(resource, "CI_ID");
            item.SoftwareName = ResultObjectHandler.GetString(resource, "SoftwareName");
            item.FeatureType = (SccmItemType)ResultObjectHandler.GetInt(resource, "FeatureType");
            return item;
        }

        public static SMS_DeploymentInfo GetDeploymentInfoFromSMS_DeploymentInfoResults(IResultObject resource)
        {
            SMS_DeploymentInfo item = new SMS_DeploymentInfo();

            item.CollectionID = ResultObjectHandler.GetString(resource, "CollectionID");
            item.CollectionName = ResultObjectHandler.GetString(resource, "CollectionName");
            item.DeploymentID = ResultObjectHandler.GetString(resource, "DeploymentID");
            item.DeploymentIntent = ResultObjectHandler.GetInt(resource, "DeploymentIntent");
            item.DeploymentName = ResultObjectHandler.GetString(resource, "DeploymentName");
            item.DeploymentType = ResultObjectHandler.GetInt(resource, "DeploymentType");
            item.DeploymentTypeID = ResultObjectHandler.GetInt(resource, "DeploymentTypeID");
            item.FeatureType = (SccmItemType)ResultObjectHandler.GetInt(resource, "FeatureType");
            item.TargetID = ResultObjectHandler.GetString(resource, "TargetID");
            item.TargetSecurityTypeID = ResultObjectHandler.GetInt(resource, "TargetSecurityTypeID");
            item.TargetSubName = ResultObjectHandler.GetString(resource, "TargetSubName");
            item.TargetName = ResultObjectHandler.GetString(resource, "TargetName");
            return item;
        }

        public static SccmTaskSequence GetTaskSequenceFromSMS_TaskSequenceResults(IResultObject resource)
        {
            SccmTaskSequence item = new SccmTaskSequence();

            item.ID = ResultObjectHandler.GetString(resource, "PackageID");
            item.Name = ResultObjectHandler.GetString(resource, "Name");
            int tstype = ResultObjectHandler.GetInt(resource, "Type");
            item.TaskSequenceType = (TaskSequenceType)tstype;
            return item;
        }

        public static SccmPackageProgram GetPackageProgramFromSMS_ProgramResults(IResultObject resource)
        {
            SccmPackageProgram item = new SccmPackageProgram();

            item.ProgramName = ResultObjectHandler.GetString(resource, "ProgramName");
            item.DependentProgram = ResultObjectHandler.GetString(resource, "DependentProgram");
            item.Description = ResultObjectHandler.GetString(resource, "Description");
            item.PackageName = ResultObjectHandler.GetString(resource, "PackageName");
            item.PackageID = ResultObjectHandler.GetString(resource, "PackageID");
            item.ID = item.PackageID + ";;" + item.ProgramName;
            //item.Name = item.PackageName + " (" + item.ProgramName + ")";
            item.Name = item.ProgramName;
            return item;
        }

        public static SccmPackage GetPackageFromSMS_PackageBaseclassResults(IResultObject resource)
        {
            SccmPackage item = new SccmPackage();

            item.Name = ResultObjectHandler.GetString(resource, "Name");
            item.ID = ResultObjectHandler.GetString(resource, "PackageID");
            item.Description = ResultObjectHandler.GetString(resource, "Description");
            item.PackageType = (PackageType)ResultObjectHandler.GetInt(resource, "PackageType");
            return item;
        }

        public static SccmConfigurationBaseline GetConfigurationBaselineFromSMS_ConfigurationBaselineInfo(IResultObject resource)
        {
            SccmConfigurationBaseline item = new SccmConfigurationBaseline();

            item.Name = ResultObjectHandler.GetString(resource, "LocalizedDisplayName");
            item.ID = ResultObjectHandler.GetString(resource, "CI_ID");
            return item;
        }

        public static SccmConfigurationItem GetConfigurationItemFromSMS_ConfigurationItemSettingReference(IResultObject resource)
        {
            SccmConfigurationItem item = new SccmConfigurationItem();

            item.Name = ResultObjectHandler.GetString(resource, "SettingName");
            item.ID = ResultObjectHandler.GetString(resource, "CI_ID");
            return item;
        }

        public static SMS_CIRelation GetCIRelationFromSMS_CIRelation(IResultObject resource)
        {
            SMS_CIRelation item = new SMS_CIRelation();

            item.FromCIID = ResultObjectHandler.GetString(resource, "FromCIID");
            item.ToCIID = ResultObjectHandler.GetString(resource, "ToCIID");
            item.RelationType = ResultObjectHandler.GetInt(resource, "RelationType");
            return item;
        }

        public static SccmConfigurationItem GetSccmConfigurationItem(IResultObject resource)
        {
            SccmConfigurationItem item = new SccmConfigurationItem();

            item.ID = ResultObjectHandler.GetString(resource, "CI_ID");
            item.Name = ResultObjectHandler.GetString(resource, "LocalizedDisplayName");
            return item;
        }

        public static SccmSoftwareUpdate GetSccmSoftwareUpdate(IResultObject resource)
        {
            SccmSoftwareUpdate item = new SccmSoftwareUpdate();

            item.ID = ResultObjectHandler.GetString(resource, "CI_ID");
            item.Name = ResultObjectHandler.GetString(resource, "LocalizedDisplayName");
            return item;
        }

        public static SccmDevice GetSccmDeviceFromSMS_R_System(IResultObject resource)
        {
            SccmDevice item = new SccmDevice();

            item.ID = ResultObjectHandler.GetString(resource, "SMBIOSGUID");
            item.Name = ResultObjectHandler.GetString(resource, "Name");
            return item;
        }

        public static SccmUser GetSccmUserFromSMS_R_User(IResultObject resource)
        {
            SccmUser item = new SccmUser();

            item.ID = ResultObjectHandler.GetString(resource, "ResourceID");
            item.Name = ResultObjectHandler.GetString(resource, "UniqueUserName");
            item.FullUserName = ResultObjectHandler.GetString(resource, "FullUserName");
            return item;
        }

        public static SccmUser GetSccmUserFromSMS_FullCollectionMembership(IResultObject resource)
        {
            SccmUser item = new SccmUser();

            item.ID = ResultObjectHandler.GetString(resource, "ResourceID");
            item.Name = ResultObjectHandler.GetString(resource, "UniqueUserName");
            item.FullUserName = ResultObjectHandler.GetString(resource, "FullUserName");
            return item;
        }
    }
}
