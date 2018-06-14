using System.DirectoryServices;
using System.Text;
using System;
using System.Security.Principal;

namespace ADScanner.ActiveDirectory
{
    internal static class ADSearchResultConverter
    {
        public static string GetSinglestringValue(SearchResult result, string property)
        {
            if (result.Properties[property] != null && result.Properties[property].Count > 0)
            {
                if (result.Properties[property][0].GetType().IsArray)
                {
                    byte[] resulttext = (byte[])result.Properties[property][0];
                    return Encoding.UTF8.GetString(resulttext);
                }
                else
                {
                    return result.Properties[property][0].ToString();
                }
                    
            }
            return string.Empty;
        }

        public static string GetSidAsString(SearchResult result)
        { 
            string property = "objectSid";
            if (result.Properties[property] != null && result.Properties[property].Count > 0)
            {
                SecurityIdentifier sid = new SecurityIdentifier((byte[])result.Properties[property][0], 0);
                return sid.ToString();
            }
            else
            {
                return null;
            }
        }
    }
}
