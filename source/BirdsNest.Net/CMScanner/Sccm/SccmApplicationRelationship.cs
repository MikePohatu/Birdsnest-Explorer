namespace CMScanner.Sccm
{
    public class SccmApplicationRelationship
    {
        public enum RelationShipType { Install, NoInstall }

        public string ToDeploymentTypeCIID { get; set; }
        public string ToApplicationCIID { get; set; }
        public RelationShipType Type { get; set; }
        public string FromApplicationCIID { get; set; }
        public string FromDeploymentTypeCIID { get; set; }

        /// <summary>
        /// Set the type based on int - 2=NoInstall,3=Install
        /// </summary>
        /// <param name="type"></param>
        public void SetType(int type)
        {
            if (type == 3) { this.Type = RelationShipType.Install; }
            else if (type == 2) { this.Type = RelationShipType.NoInstall; }
        }
    }
}
