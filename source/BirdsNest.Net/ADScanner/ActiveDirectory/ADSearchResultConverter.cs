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
//
#endregion
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
            string res = string.Empty;
            if (result?.Properties[property] != null && result.Properties[property].Count > 0)
            {
                if (result.Properties[property][0].GetType().IsArray)
                {
                    byte[] resulttext = (byte[])result.Properties[property][0];
                    res = Encoding.UTF8.GetString(resulttext);
                }
                else
                {
                    res = result.Properties[property][0].ToString();
                }
                    
            }
            if (res == null) { return string.Empty; }
            return res;
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
