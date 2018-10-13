using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
//using System.Diagnostics;
//debug.writeline

namespace Sudoku.Strategies
{
    public class Graph
    {
    }

    public class Node
    {
        public NodeType NodeType;
        public List<Candidate> Candidates;
        public List<WeakLink> Links;
        public bool IsTrue;
    }

    public enum NodeType
    {
        SingleCandidate = 0,
        InDifferentCells = 1,
        WithDifferentValues = 2
    }

    public abstract class Link
    {
        public Node[] Nodes;
        public bool IsXLink;
        public bool IsXYLink;
        public bool IsALSLink;
        public bool IsURLink;
        public bool IsThreeWay;
        public List<WeakLink> ChildrenLinks;
    }

    public class WeakLink : Link
    {
        public int Length;
        public List<Chain> Chains;
    }

    public class ALS
    {
        public List<Cell> Cells;
        public List<StrongLink> Links;
        public List<int> Values;
    }

    public class UR
    {
        public Cell[] Cells;
        public int[] Values;
        public StrongLink Link;
    }

    public class StrongLink : Link
    {
        public List<ALS> ALSs;
        public UR UR;
    }

    public class ThreeWayLink : Link
    {
        
    }

    public class Chain
    {
        public List<(Link Link, int Direction)> Links;
        public List<Chain> From;
        public List<Chain> To;
        public void Reduce()
        {
            for(int i = 0; i < Links.Count - 1; i++)
            {
                if (Links[i].Link.Nodes[Links[i].Direction] == Links[i + 1].Link.Nodes[1 - Links[i + 1].Direction])
                {
                    Links.RemoveAt(i);
                    Links.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
