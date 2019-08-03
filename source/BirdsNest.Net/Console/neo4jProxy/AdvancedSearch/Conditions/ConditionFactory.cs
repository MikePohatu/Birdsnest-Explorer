using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Console.neo4jProxy.AdvancedSearch.Conditions
{
    public static class ConditionFactory
    {
        public static ICondition GetCondition(string json)
        {
            ICondition c = JsonConvert.DeserializeObject<ICondition>(json);
            ICondition retcond = null;

            switch (c.Type.ToUpper())
            {
                case "AND":
                    retcond = JsonConvert.DeserializeObject<AndOrCondition>(json);
                    break;
                case "OR":
                    retcond = JsonConvert.DeserializeObject<AndOrCondition>(json);
                    break;
                case "STRING":
                    retcond = JsonConvert.DeserializeObject<StringCondition>(json);
                    break;
                case "MATH":
                    retcond = JsonConvert.DeserializeObject<MathCondition>(json);
                    break;
                case "REGEX":
                    retcond = JsonConvert.DeserializeObject<RegExCondition>(json);
                    break;
                default:
                    break;
            }

            return retcond;
        }
    }
}
