using Console.neo4jProxy;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Console.ViewModels
{
    public class RelatedDetailViewModel
    {
        public BirdsNestNode Node { get; private set; }
        public Dictionary<string, List<BirdsNestNode>> LabelledEdgeNodes { get; private set; } = new Dictionary<string, List<BirdsNestNode>>();
        public Dictionary<string, List<BirdsNestNode>> RelatedLabelledNodes { get; private set; } = new Dictionary<string, List<BirdsNestNode>>();
        public Dictionary<long, BirdsNestNode> RelatedNodes { get; private set; } = new Dictionary<long, BirdsNestNode>();

        public RelatedDetailViewModel(BirdsNestNode node)
        {
            this.Node = node;
        }

        public void AddRelatedNode(BirdsNestNode node)
        {
            if (this.RelatedNodes.ContainsKey(node.DbId)==false)
            {
                this.RelatedNodes.Add(node.DbId, node);

                List<BirdsNestNode> nodelist;
                if (this.RelatedLabelledNodes.TryGetValue(node.Label, out nodelist))
                {
                    nodelist.Add(node);
                }
                else
                {
                    List<BirdsNestNode> newlist = new List<BirdsNestNode>();
                    newlist.Add(node);
                    this.RelatedLabelledNodes.Add(node.Label, newlist);
                }
            }
        }

        public void AddDirectEdge(BirdsNestRelationship edge)
        {
            List<BirdsNestNode> edgelist;
            BirdsNestNode relatednode;

            if (edge.Source == this.Node.DbId) { this.RelatedNodes.TryGetValue(edge.Target, out relatednode); }
            else { this.RelatedNodes.TryGetValue(edge.Target, out relatednode); }

            if (this.LabelledEdgeNodes.TryGetValue(edge.Label, out edgelist))
            {
                edgelist.Add(relatednode);
            }
            else
            {
                List<BirdsNestNode> newlist = new List<BirdsNestNode>();
                newlist.Add(relatednode);
                this.LabelledEdgeNodes.Add(edge.Label, newlist);
            }
        }
    }
}
