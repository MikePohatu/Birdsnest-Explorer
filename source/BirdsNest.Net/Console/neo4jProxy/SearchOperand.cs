using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy
{
    public class SearchOperand: ISearchConditionObject
    {
        private string _type = "AND";

        public ISearchConditionObject Left { get; set; }
        public ISearchConditionObject Right { get; set; }
        public string Type
        {
            get { return this._type; }
            set
            {
                string s = value.ToUpper();
                if ((s == "AND") || (s == "OR")) { this._type = s; }
                else { throw new ArgumentException(value + " is not valid. Must be AND or OR"); }
            }
        }

        public string GetSearchString()
        {
            return "(" + Left.GetSearchString() + ") " + this.Type + " (" + Right.GetSearchString() + ")";
        }

    }
}
