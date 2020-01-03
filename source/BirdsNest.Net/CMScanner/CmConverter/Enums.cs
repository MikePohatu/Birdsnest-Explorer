namespace CMScanner.CmConverter
{
    public enum TaskSequenceType { Generic = 1, OSD = 2 }
    public enum SccmItemType
    {
        Application = 1,
        PackageProgram = 2,
        MobileProgram = 3,
        Script = 4,
        SoftwareUpdateGroup = 5,
        ConfigurationBaseline = 6,
        TaskSequence = 7,
        ContentDistribution = 8,
        DistributionPointGroup = 9,
        DistributionPointHealth = 10,
        ConfigurationPolicy = 11,
        SoftwareUpdate = 37,
        Device,
        User,
        Collection,
        SMS_DeploymentInfo,
        SMS_DeploymentSummary,
        Package,
        ConfigurationItem
    }

    public enum PackageType
    {
        RegularSoftwareDistribution = 0,
        Driver = 3,
        TaskSequence = 4,
        SoftwareUpdate = 5,
        DeviceSetting = 6,
        VirtualApplication = 7,
        Image = 257,
        BootImage = 258,
        OperatingSystemInstall = 259
    }
}
