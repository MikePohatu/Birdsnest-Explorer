#region license
// Copyright (c) 2019-2023 "20Road"
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
using Console.Vue;
using System.Collections.Generic;

namespace Console.neo4jProxy
{
    public class RelatedDetail
    {
        private Dictionary<string, BirdsNestNode> _relatedNodes = new Dictionary<string, BirdsNestNode>();

        public BirdsNestNode Node { get; private set; }

        public int RelatedCount { get { return this._relatedNodes.Count; } }

        public VForLabelledNodeList InNodesByEdgeLabel { get; private set; } = new VForLabelledNodeList("InNodesByEdgeLabel");
        public VForLabelledNodeList OutNodesByEdgeLabel { get; private set; } = new VForLabelledNodeList("OutNodesByEdgeLabel");
        public VForLabelledNodeList InNodesByLabel { get; private set; } = new VForLabelledNodeList("InNodesByLabel");
        public VForLabelledNodeList OutNodesByLabel { get; private set; } = new VForLabelledNodeList("OutNodesByLabel");


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
            if (this._relatedNodes.ContainsKey(node.DbId) == false)
            {
                this._relatedNodes.Add(node.DbId, node);
            }
        }

        public void AddDirectEdge(BirdsNestRelationship edge)
        {
            BirdsNestNode workingnode;
            VForLabelledNodeList nodelist;

            //figure out inbound or outbound edge
            if (edge.Source == this.Node.DbId)
            {
                if (this._relatedNodes.TryGetValue(edge.Target, out workingnode) == false)
                {
                    throw new System.Exception("Unable to resolve target node with DbId: " + edge.Target);
                }
                nodelist = this.OutNodesByLabel;
                this.OutNodesByEdgeLabel.AddNode(workingnode, edge.Label);

            }
            else
            {
                if (this._relatedNodes.TryGetValue(edge.Source, out workingnode) == false)
                {
                    throw new System.Exception("Unable to resolve source node with DbId: " + edge.Source);
                }
                nodelist = this.InNodesByLabel;
                this.InNodesByEdgeLabel.AddNode(workingnode, edge.Label);
            }

            foreach (string label in workingnode.Labels)
            {
                nodelist.AddNode(workingnode, label);
            }
        }
    }
}