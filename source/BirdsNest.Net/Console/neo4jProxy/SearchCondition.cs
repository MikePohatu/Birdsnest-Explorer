using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Console.neo4jProxy
{
    public class SearchCondition: ISearchConditionObject
    {
        public ISearchObject Parent { get; set; }
        public string Property { get; set; }
        public string Value { get; set; }
        public bool CaseSensitive { get; set; } = false;
        public bool UseWildCards { get; set; } = false;

        [JsonConverter(typeof(StringEnumConverter))]
        public SearchConditionType Type { get; set; } = SearchConditionType.Equals;

        public string GetSearchString()
        {
            string ret = string.Empty;
            if ((this.CaseSensitive == true) || (this.UseWildCards == false))
            {
                if (this.Type == SearchConditionType.Exists)
                { ret = Parent.Identifier + "." + this.Property + this.GetTypeString() + this.Value; }
                else
                { ret = "exists:(" + Parent.Identifier + "." + this.Property + ")"; }
                
            }
            else
            {

            }
            return ret;
        }

        private string GetTypeString()
        {
            //public enum SearchConditionType { Equals, StartsWith, EndsWith, GreaterThan, LessThan, Exists }
            switch (this.Type)
            {
                case SearchConditionType.Equals:
                    return " = ";
                case SearchConditionType.StartsWith:
                    return " STARTS WITH ";
                case SearchConditionType.EndsWith:
                    return " ENDS WITH ";
                case SearchConditionType.GreaterThan:
                    return " > ";
                case SearchConditionType.LessThan:
                    return " < ";
                case SearchConditionType.Exists:
                    return ":";
                default:
                    return " = ";
            }
        }
    }
}
