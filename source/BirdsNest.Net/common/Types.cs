#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
using System;
using System.Collections.Generic;
using System.Text;

namespace common
{
    public static class Types
    {
        //Nodes - use upper case
        //edges - use camel case

        //general nodes
        public static string Orphaned { get { return "Orphaned"; } }
        public static string Device { get { return "Device"; } }

        //builtin nodes
        public static string BuiltinUser { get { return "BuiltinUser"; } }
        public static string BuiltinGroup { get { return "BuiltinGroup"; } }
        public static string BuiltinObject { get { return "BuiltinObject"; } }

        //general edges
        public static string Uses { get { return "USES"; } }
        public static string AppliesPermissionTo { get { return "APPLIES_PERMISSION_TO"; } }
        public static string ConnectedTo { get { return "CONNECTED_TO"; } }
        public static string WritesDataTo { get { return "WRITES_DATA_TO"; } }
        public static string ReadsDataFrom { get { return "READS_DATA_FROM"; } }

        //FSScanner nodes
        public static string Folder { get { return "FS_Folder"; } }
        public static string BlockedFolder { get { return "FS_BlockedFolder"; } }
        public static string DfsRoot { get { return "FS_DfsRoot"; } }
        public static string Datastore { get { return "FS_Datastore"; } }

        //FSScanner edges
        public static string AppliesInhertitanceTo { get { return "FS_APPLIES_INHERITANCE_TO"; } }
        public static string BlockedInheritance { get { return "FS_BLOCKED_INHERITANCE"; } }
        public static string Hosts { get { return "FS_HOSTS"; } }

        //ADScanner nodes
        public static string User { get { return "AD_User"; } }
        public static string Computer { get { return "AD_Computer"; } }
        public static string Group { get { return "AD_Group"; } }
        public static string Deleted { get { return "AD_Deleted"; } }
        public static string ADObject { get { return "AD_Object"; } }
        public static string ForeignSecurityPrincipal { get { return "AD_ForeignSecurityPrincipal"; } }

        //ADScanner edges
        public static string MemberOf { get { return "AD_MEMBER_OF"; } }

        //AzureAD nodes
        public static string AadUser { get { return "AAD_User"; } }
        public static string AadGroup { get { return "AAD_Group"; } }
        public static string AadObject { get { return "AAD_Object"; } }

        //AzureAD edges
        public static string AadMemberOf { get { return "AAD_MEMBER_OF"; } }
        public static string AadSync { get { return "AAD_SYNC"; } }

        //CMScanner nodes 
        public static string CMCollection { get { return "CM_Collection"; } }
        public static string CMDevice { get { return "CM_Device"; } }
        public static string CMUser { get { return "CM_User"; } }
        public static string CMConfigurationItem { get { return "CM_ConfigurationItem"; } }
        public static string CMApplication { get { return "CM_Application"; } }
        public static string CMPackage { get { return "CM_Package"; } }
        public static string CMPackageProgram { get { return "CM_PackageProgram"; } }
        public static string CMTaskSequence { get { return "CM_TaskSequence"; } }
        public static string CMSoftwareUpdate { get { return "CM_SoftwareUpdate"; } }
        public static string CMSoftwareUpdateGroup { get { return "CM_SoftwareUpdateGroup"; } }
        public static string CMClientSettings { get { return "CM_ClientSettings"; } }

        public static string CMMemberOf { get { return "CM_MEMBER_OF"; } }
        public static string CMHasProgram { get { return "CM_HAS_PROGRAM"; } }
        public static string CMHasObject { get { return "CM_HAS_OBJECT"; } }
        public static string CMLimitingCollection { get { return "CM_LIMITING_COLLECTION"; } }
        public static string CMReferences { get { return "CM_REFERENCES"; } }
        public static string CMHasDeployment { get { return "CM_HAS_DEPLOYMENT"; } }
        public static string CMSuperSededBy{ get { return "CM_SUPERSEDED_BY"; } }
        //WUScanner
        public static string WUUpdate { get { return "WU_Update"; } }
        public static string Supersedes { get { return "SUPERSEDES"; } }

        //CustomAppScanner nodes
        public static string Application { get { return "Application"; } }
    }
}
