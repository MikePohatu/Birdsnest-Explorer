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
using Console.neo4jProxy;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Console.ViewModels
{
    public class NodeRelatedDetailViewModel
    {
        public BirdsNestNode Node { get; private set; }
        public Dictionary<string, List<BirdsNestNode>> LabelledEdgeNodes { get; private set; } = new Dictionary<string, List<BirdsNestNode>>();
        public Dictionary<string, List<BirdsNestNode>> LabeledNodeLists { get; private set; } = new Dictionary<string, List<BirdsNestNode>>();
        public Dictionary<long, BirdsNestNode> RelatedNodes { get; private set; } = new Dictionary<long, BirdsNestNode>();

        public NodeRelatedDetailViewModel(BirdsNestNode node, ResultSet set)
        {
            this.Node = node;

            foreach (BirdsNestNode newnode in set.Nodes)
            {
                this.AddRelatedNode(newnode);
            }
            foreach (BirdsNestRelationship newrel in set.Edges)
            {
                this.AddDirectEdge(newrel);
            }
        }

        public void AddRelatedNode(BirdsNestNode node)
        {
            if (this.RelatedNodes.ContainsKey(node.DbId)==false)
            {
                this.RelatedNodes.Add(node.DbId, node);
            }
        }

        public void AddDirectEdge(BirdsNestRelationship edge)
        {
            List<BirdsNestNode> edgelist;
            BirdsNestNode relatednode;
            string nodelabel = string.Empty;
            string edgelabel = string.Empty;
            string dir = string.Empty;

            if (edge.Source == this.Node.DbId) {
                this.RelatedNodes.TryGetValue(edge.Target, out relatednode);
                dir = "○ -> ";
            }
            else {
                this.RelatedNodes.TryGetValue(edge.Source, out relatednode);
                dir = "○ <- ";
            }

            edgelabel = dir + edge.Label;
            if (this.LabelledEdgeNodes.TryGetValue(edgelabel, out edgelist))
            {
                edgelist.Add(relatednode);
            }
            else
            {
                List<BirdsNestNode> newlist = new List<BirdsNestNode>();
                newlist.Add(relatednode);
                this.LabelledEdgeNodes.Add(edgelabel, newlist);
            }

            foreach (string label in relatednode.Labels)
            {
                nodelabel = dir + label;
                

                List<BirdsNestNode> nodelist;
                if (this.LabeledNodeLists.TryGetValue(nodelabel, out nodelist))
                {
                    nodelist.Add(relatednode);
                }
                else
                {
                    List<BirdsNestNode> newlist = new List<BirdsNestNode>();
                    newlist.Add(relatednode);
                    this.LabeledNodeLists.Add(nodelabel, newlist);
                }

               
            }
        }
    }
}
