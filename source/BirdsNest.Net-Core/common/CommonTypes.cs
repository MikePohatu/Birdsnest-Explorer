namespace common
{
    public static class CommonTypes
    {
        //Nodes - use upper case
        public static string Orphaned { get { return "ORPHANED"; } }
        public static string Device { get { return "DEVICE"; } }

        //edges - use camel case
        public static string Uses { get { return "Uses"; } }
        public static string GivesAccessTo { get { return "GivesAccessTo"; } }
        public static string ConnectedTo { get { return "ConnectedTo"; } }
        public static string WritesDataTo { get { return "WritesDataTo"; } }
        public static string ReadsDataFrom { get { return "ReadsDataFrom"; } }
    }
}
