namespace CMScanner.Sccm
{
    /// <summary>
    /// Class represents the SMS_DeploymentSummary WMI class
    /// </summary>
    public class SMS_DeploymentInfo: IDeployment
    {
        public SccmItemType Type { get { return SccmItemType.SMS_DeploymentInfo; } }

        public string TargetName { get; set; }
        public string CollectionID { get; set; }
        public string CollectionName { get; set; }
        public string DeploymentID { get; set; }
        public string ID { get { return this.DeploymentID; } }

        public string DeploymentName { get; set; }
        public int DeploymentIntent { get; set; }


        public string Name { get { return this.DeploymentName; } }

        public int DeploymentType { get; set; }
        public int DeploymentTypeID { get; set; }
        public SccmItemType FeatureType { get; set; }
        public string TargetID { get; set; }
        public int TargetSecurityTypeID { get; set; }
        public string TargetSubName { get; set; }
    }
}
