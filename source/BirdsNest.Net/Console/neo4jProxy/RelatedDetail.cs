#region license
// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
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
#endregion
using System.Collections.Generic;
using Console.Vue;

namespace Console.neo4jProxy
{
    public class RelatedDetail
    {


        public BirdsNestNode Node { get; private set; }

        public VForLabelledNodeList InNodesByEdgeLabel { get; private set; }
            = new VForLabelledNodeList("InNodesByEdgeLabel");
        public VForLabelledNodeList OutNodesByEdgeLabel { get; private set; }
            = new VForLabelledNodeList("OutNodesByEdgeLabel");
        public VForLabelledNodeList InNodesByLabel { get; private set; }
            = new VForLabelledNodeList("InNodesByLabel");
        public VForLabelledNodeList OutNodesByLabel { get; private set; }
            = new VForLabelledNodeList("OutNodesByLabel");
        public Dictionary<string, BirdsNestNode> RelatedNodes { get; private set; } = new Dictionary<string, BirdsNestNode>();


        public RelatedDetail(BirdsNestNode node, ResultSet set)
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
            if (this.RelatedNodes.ContainsKey(node.DbId.ToString()) == false)
            {
                this.RelatedNodes.Add(node.DbId.ToString(), node);
            }
        }

        public void AddDirectEdge(BirdsNestRelationship edge)
        {
            Dictionary<string, List<long>> workinglist;
            List<long> edgelist;
            BirdsNestNode relatednode;
            string dir = string.Empty;
            bool inbound = false;

            //figure out inbound or outbound edge
            if (edge.Source == this.Node.DbId)
            {
                this.RelatedNodes.TryGetValue(edge.Target.ToString(), out relatednode);
                workinglist = this.OutNodesByEdgeLabel.LabelledNodes;
            }
            else
            {
                this.RelatedNodes.TryGetValue(edge.Source.ToString(), out relatednode);
                workinglist = this.InNodesByEdgeLabel.LabelledNodes;
                inbound = true;
            }

            //look for existing list for edge label, create and add if not theref
            if (workinglist.TryGetValue(edge.Label, out edgelist))
            {
                edgelist.Add(relatednode.DbId);
            }
            else
            {
                List<long> newlist = new List<long>();
                newlist.Add(relatednode.DbId);
                workinglist.Add(edge.Label, newlist);
            }

            foreach (string label in relatednode.Labels)
            {
                if (inbound) { workinglist = this.InNodesByLabel.LabelledNodes; }
                else { workinglist = this.OutNodesByLabel.LabelledNodes; }

                List<long> nodelist;
                if (workinglist.TryGetValue(label, out nodelist))
                {
                    nodelist.Add(relatednode.DbId);
                }
                else
                {
                    List<long> newlist = new List<long>();
                    newlist.Add(relatednode.DbId);
                    workinglist.Add(label, newlist);
                }


            }
        }
    }
}