using System.Text;

namespace Console.neo4jProxy
{
    public class SearchEdge
    {
        private int min = 0;
        private int max = 1;
        
        public string RelationshipType { get; set; }
        public int RelationshipMin
        {
            get { return this.min; }
            set { this.min = value < 0 ? 0 : value; }
        }
        public int RelationshipMax
        {
            get { return this.max; }
            set { this.max = value < 0 ? 0 : value; }
        }
        public enum QueryDirection { Right, Left, Bidirectional }
        public QueryDirection Direction { get; set; } = QueryDirection.Right;
        public SearchNode Source { get; set; }
        public SearchNode Target { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (this.Direction != QueryDirection.Right) { builder.Append("<"); }
            builder.Append("-[");

            if (string.IsNullOrEmpty(this.RelationshipType)) { builder.Append("*"); }
            else { builder.Append(":" + this.RelationshipType); }

            if ((this.RelationshipMin == 0) && (this.RelationshipMax == int.MaxValue))
            {
                //this is a no limit search. require extra logging I think
            }
            else
            {
                builder.Append(this.RelationshipMin + ".." + this.RelationshipMax);
            }

            builder.Append("]-");
            if (this.Direction != QueryDirection.Left) { builder.Append(">"); }
            return builder.ToString();
        }
    }
}
