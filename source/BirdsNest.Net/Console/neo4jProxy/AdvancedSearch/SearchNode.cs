using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy.AdvancedSearch
{
    public class SearchNode
    {
        [JsonProperty("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("Label")]
        public string Label { get; set; } = string.Empty;

        public string TokenizedName { get; set; }

        public string ToSearchString()
        {
            return this.BuildSearchString(false);
        }

        public string ToTokenizedSearchString()
        {
            return this.BuildSearchString(true);
        }

        private string BuildSearchString(bool tokenized)
        {
            if (tokenized && string.IsNullOrWhiteSpace(this.TokenizedName))
            {
                throw new ArgumentException("Cannot build Edge tokenized search string. Edge has not been tokenized");
            }

            string name = tokenized ? this.TokenizedName : this.Name;
            string s = string.Empty;
            if (string.IsNullOrEmpty(this.Label)) { s = "(" + this.Name + ")"; }
            else { s = "(" + name + ":" + this.Label + ")"; }
            return s;
        }

        public void Tokenize(SearchTokens tokens)
        {
            this.TokenizedName = tokens.GetNameToken(this.Name);
        }
    }
}
