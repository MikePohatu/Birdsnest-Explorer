using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text;

namespace Console.neo4jProxy
{
    public class SearchEdge: ISearchObject
    {
        private int min = 0;
        private int max = 1;

        [JsonProperty("identifier")]
        public string Identifier { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("min")]
        public int Min
        {
            get { return this.min; }
            set { this.min = value < 0 ? 0 : value; }
        }
        [JsonProperty("max")]
        public int Max
        {
            get { return this.max; }
            set { this.max = value < 0 ? 0 : value; }
        }
        public enum QueryDirection { Right, Left, Bidirectional }

        [JsonConverter(typeof(StringEnumConverter))]
        public QueryDirection Direction { get; set; } = QueryDirection.Right;

        [JsonProperty("condition")]
        public ISearchConditionObject Condition { get; set; }

        public string GetPathString()
        {
            StringBuilder builder = new StringBuilder();
            if (this.Direction != QueryDirection.Right) { builder.Append("<"); }
            builder.Append("-[");

            builder.Append(this.Identifier);

            if (string.IsNullOrEmpty(this.Type)) { builder.Append("*"); }
            else { builder.Append(":" + this.Type); }

            if ((this.Min == 0) && (this.Max == int.MaxValue))
            {
                //this is a no limit search. require extra logging I think
            }
            else
            {
                builder.Append(this.Min + ".." + this.Max);
            }

            builder.Append("]-");
            if (this.Direction != QueryDirection.Left) { builder.Append(">"); }
            return builder.ToString();
        }

        public string GetWhereString()
        {
            return this.Condition.GetSearchString();
        }
    }
}
