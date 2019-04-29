using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Console.neo4jProxy
{
    public class Search
    {
        public SearchNode Head { get; set; }
        public SearchNode Tail { get; set; }

        public void AddToTail(SearchEdge edge, SearchNode node)
        {
            if (this.Head == null)
            {
                //exception/logging to add
            }

            edge.Source = this.Tail;
            edge.Target = node;
            this.Tail = node;
        }

        public void AddToTail(SearchNode node)
        {
            if (this.Head == null)
            {
                //exception/logging to add
            }
            SearchEdge edge = new SearchEdge();
            edge.Source = this.Tail;
            edge.Target = node;
            this.Tail = node;
        }
    }
}
