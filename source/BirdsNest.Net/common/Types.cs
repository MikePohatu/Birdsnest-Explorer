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
        public static string Orphaned { get { return "ORPHANED"; } }
        public static string Device { get { return "DEVICE"; } }

        //builtin nodes
        public static string BuiltInUser { get { return "BUILTIN_USER"; } }
        public static string BuiltInGroup { get { return "BUILTIN_GROUP"; } }

        //general edges
        public static string Uses { get { return "Uses"; } }
        public static string GivesAccessTo { get { return "GivesAccessTo"; } }
        public static string ConnectedTo { get { return "ConnectedTo"; } }
        public static string WritesDataTo { get { return "WritesDataTo"; } }
        public static string ReadsDataFrom { get { return "ReadsDataFrom"; } }

        //FSScanner nodes
        public static string Folder { get { return "FS_FOLDER"; } }
        public static string BlockedFolder { get { return "FS_BLOCKEDFOLDER"; } }
        public static string DfsRoot { get { return "FS_DFSROOT"; } }
        public static string Datastore { get { return "FS_DATASTORE"; } }

        //FSScanner edges
        public static string InheritsFrom { get { return "FS_InheritsFrom"; } }
        public static string BlocksInheritanceFrom { get { return "FS_BlocksInheritanceFrom"; } }
        public static string HostedOn { get { return "FS_HostedOn"; } }

        //ADScanner nodes
        public static string User { get { return "AD_USER"; } }
        public static string Computer { get { return "AD_COMPUTER"; } }
        public static string Group { get { return "AD_GROUP"; } }
        public static string Deleted { get { return "AD_DELETED"; } }

        //ADScanner edges
        public static string MemberOf { get { return "AD_MemberOf"; } }

        //CustomAppScanner nodes
        public static string Application { get { return "APPLICATION"; } }
    }
}
