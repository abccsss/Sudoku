using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class Step
    {
        public Game NewGame { get; set; }

        // Frames[0] is shown by default
        public List<StepFrame> Frames { get; } = new List<StepFrame>();

        public static Step Invalid { get; } = new Step();
    }

    public class StepFrame
    {
        public string Caption { get; set; }

        public List<Cell> Highlights { get; } = new List<Cell>();
        public List<HintNode> Nodes { get; } = new List<HintNode>();
        public List<HintArrow> Arrows { get; } = new List<HintArrow>();
    }

    public class HintNode
    {
        public List<Candidate> Candidates { get; } = new List<Candidate>();
        public HintColor Color { get; set; }
    }

    public class HintArrow
    {
        public bool IsDashed { get; set; }
        public HintNode From { get; set; }
        public HintNode To { get; set; }
    }

    public enum HintColor
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Orange = 3,
        Purple = 4,
        Brown = 5,
        LightRed = 10,
        LightGreen = 11,
        LightBlue = 12,
        LightOrange = 13,
        LightPurple = 14,
        LightBrown = 15
    }
}
