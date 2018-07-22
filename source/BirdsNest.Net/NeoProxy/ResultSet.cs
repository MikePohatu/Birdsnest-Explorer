using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NeoProxy
{
    public class ResultSet
    {
        public List<BirdsNestNode> Nodes { get; private set; } = new List<BirdsNestNode>();
        public List<BirdsNestRelationship> Edges { get; private set; } = new List<BirdsNestRelationship>();

        public void Append (ResultSet additionalresults)
        {
            if (additionalresults != null)
            {
                this.Nodes.AddRange(additionalresults.Nodes);
                this.Edges.AddRange(additionalresults.Edges);
            }
        }
    }
}
