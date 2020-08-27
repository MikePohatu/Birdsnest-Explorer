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
