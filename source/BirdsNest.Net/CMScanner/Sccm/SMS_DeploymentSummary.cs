namespace CMScanner.Sccm
{
    /// <summary>
    /// Class represents the SMS_DeploymentSummary WMI class
    /// </summary>
    public class SMS_DeploymentSummary: IDeployment
    {
        public SccmItemType Type { get { return SccmItemType.SMS_DeploymentSummary; } }

        public string CollectionID { get; set; }
        public string CollectionName { get; set; }
        public string DeploymentID { get; set; }
        public string ID { get; set; }
        public int DeploymentIntent { get; set; }
        public string PackageID { get; set; }
        public string Name { get { return this.DeploymentID; } }
        public SccmItemType FeatureType { get; set; }
        public string CIID { get; set; }
        public string SoftwareName { get; set; }
    }
}
