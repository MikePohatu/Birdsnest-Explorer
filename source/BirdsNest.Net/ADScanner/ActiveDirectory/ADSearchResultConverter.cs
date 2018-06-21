using System.DirectoryServices;
using System.Text;
using System.Collections.Generic;
using System.Security.Principal;

namespace ADScanner.ActiveDirectory
{
    internal static class ADSearchResultConverter
    {
        public static string GetSinglestringValue(SearchResult result, string property)
        {
            if (result?.Properties[property] != null && result.Properties[property].Count > 0)
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

        public static int GetIntSingleValue(SearchResult result, string property)
        {
            if (result?.Properties[property] != null && result.Properties[property].Count > 0)
            {
                return (int)result.Properties[property][0];
            }
            return 0;
        }

        public static bool GetBoolSingleValue(SearchResult result, string property)
        {
            if (result?.Properties[property] != null && result.Properties[property].Count > 0)
            {
                return (bool)result.Properties[property][0];
            }
            return false;
        }

        public static string GetSidAsString(SearchResult result)
        { 
            string property = "objectSid";
            if (result?.Properties[property] != null && result.Properties[property].Count > 0)
            {
                SecurityIdentifier sid = new SecurityIdentifier((byte[])result.Properties[property][0], 0);
                return sid.ToString();
            }
            return null;
        }

        public static string GetRidFromSid(string sid)
        {
            if (string.IsNullOrWhiteSpace(sid) == false)
            {
                string[] sidbits = sid.Split('-');
                return sidbits[sidbits.GetUpperBound(0)];
            }
            return null;
        }

        public static List<string> GetStringList(SearchResult result, string property)
        {
            List<string> results = new List<string>();

            if (result?.Properties[property] != null && result.Properties[property].Count > 0)
            {
                foreach (var prop in result.Properties[property])
                {
                    if (prop.GetType().IsArray)
                    {
                        byte[] resulttext = (byte[])prop;
                        results.Add(Encoding.UTF8.GetString(resulttext));
                    }
                    else
                    {
                        results.Add(prop.ToString());
                    }
                }
            }
            return results;
        }
    }
}
