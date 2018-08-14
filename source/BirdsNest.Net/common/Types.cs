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

        //general edges
        public static string Uses { get { return "USES"; } }
        public static string GivesAccessTo { get { return "GIVES_ACCESS_TO"; } }
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
        public static string HostedOn { get { return "FS_HOSTED_ON"; } }

        //ADScanner nodes
        public static string User { get { return "AD_User"; } }
        public static string Computer { get { return "AD_Computer"; } }
        public static string Group { get { return "AD_Group"; } }
        public static string Deleted { get { return "AD_Deleted"; } }
        public static string ADObject { get { return "AD_Object"; } }

        //ADScanner edges
        public static string MemberOf { get { return "AD_MEMBER_OF"; } }

        //CustomAppScanner nodes
        public static string Application { get { return "Application"; } }
    }
}
