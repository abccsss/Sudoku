using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    class Step
    {
        public List<Candidate> Eliminations { get; } = new List<Candidate>();
        public List<Candidate> Singles { get; } = new List<Candidate>();

        // Frames[0] is shown by default
        public List<StepFrame> Frames { get; } = new List<StepFrame>();
    }

    class StepFrame
    {
        public string Caption { get; set; }
        public List<HintNode> Nodes { get; } = new List<HintNode>();
        public List<HintArrow> Arrows { get; } = new List<HintArrow>();
    }

    class HintNode
    {
        public List<Cell> Cells { get; } = new List<Cell>();
        public int Value { get; set; }
        public HintColor Color { get; set; }
    }

    class HintArrow
    {
        public bool IsDashed { get; set; }
        public HintNode From { get; set; }
        public HintNode To { get; set; }
    }

    enum HintColor
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
