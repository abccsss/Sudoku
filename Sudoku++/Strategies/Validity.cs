using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Strategies
{
    public enum InvalidType
    {
        Valid,
        CellNoCandidate,
        RegionNoCandidate,
        SuperfluousCandidate
    }
    // 0: valid
    // 1: a cell containing no candidate
    // 2: a region with no cell containing some candidate
    // 3: a region with a cell containing a candidate such that another cell containing that value

    class Validity : Strategy
    {
        public override Step FindStep(Game game, bool generateHints)
        {
            Type = InvalidType.Valid;
            CheckValidity(game);
            if (Type == InvalidType.CellNoCandidate)
            {
                if (!generateHints) return Step.Invalid;
                var step = new Step
                {
                    NewGame = game,
                    Frames =
                    {
                        new StepFrame
                        {
                            Highlights = {
                                InvalidCell[0]
                            },
                            Caption = "Validity check: No candidates in R" + (InvalidCell[0].Row + 1) + "C" + (InvalidCell[0].Column + 1)
                        }
                    }
                };
                return step;
            }
            if (Type == InvalidType.SuperfluousCandidate)
            {
                var frame = new StepFrame
                {
                    Caption = "Validity check: Superfluous candidate " + (InvalidValue + 1) + " in " + InvalidRegion.Name
                };
                foreach(var cell in InvalidCell)
                {
                    var node = new HintNode
                    {
                        Candidates =
                        {
                            new Candidate(cell, InvalidValue)
                        },
                        Color = HintColor.Red
                    };
                    frame.Nodes.Add(node);
                    game.SetCandidate(cell, InvalidValue, false);
                }
                foreach(var cell in InvalidRegion.Cells)
                {
                    frame.Highlights.Add(cell);
                }
                return new Step
                {
                    Frames = { frame },
                    NewGame = game
                };
            }
            if (Type == InvalidType.RegionNoCandidate)
            {
                if (!generateHints) return Step.Invalid;
                var frame = new StepFrame
                {
                    Caption = "Validity check: No candidate " + (InvalidValue + 1) + " in " + InvalidRegion.Name
                };
                foreach (var cell in InvalidRegion.Cells)
                {
                    frame.Highlights.Add(cell);
                }
                return new Step
                {
                    Frames = { frame },
                    NewGame = game
                };
            }
            return null;
        }

        public InvalidType Type;

        public List<Cell> InvalidCell = new List<Cell>();
        public Region InvalidRegion;
        public int InvalidValue;
        
        public Validity()
        {
            
        }

        public void CheckValidity(Game game)
        {
            for(int r = 0; r < game.Rule.Height; r++)
            {
                for(int c = 0; c < game.Rule.Width; c++)
                {
                    if (game.Rule.IsAvailable[r, c] && game.Value(r, c) == -1 && game.CandidateCount(r, c) == 0)
                    {
                        Type = InvalidType.CellNoCandidate;
                        InvalidCell.Add(new Cell(r, c));
                        return;
                    }
                }
            }
            for(int reg = 0; reg < game.Rule.Regions.Count; reg++)
            {
                bool[] tmp = new bool[game.Rule.Digits];
                for (int val = 0; val < game.Rule.Digits; val++) tmp[val] = false;
                for(int cnt = 0; cnt < game.Rule.Regions[reg].Cells.Length; cnt++)
                {
                    int val = game.Value(game.Rule.Regions[reg].Cells[cnt]);
                    if (val >= 0)
                    {
                        tmp[val] = true;
                        if (game.CandidateByRegion(reg, val) > 0)
                        {
                            Type = InvalidType.SuperfluousCandidate;
                            InvalidRegion = game.Rule.Regions[reg];
                            InvalidValue = val;
                            for(int cntt=0; cntt < game.Rule.Regions[reg].Cells.Length; cntt++)
                            {
                                if (game.IsCandidate(game.Rule.Regions[reg].Cells[cntt], val))
                                {
                                    InvalidCell.Add(game.Rule.Regions[reg].Cells[cntt]);
                                }
                            }
                            return;
                        }
                    }
                }
                if (game.Rule.Regions[reg].Cells.Length == game.Rule.Digits)
                {
                    for(int val = 0; val < game.Rule.Digits; val++)
                    {
                        if (!tmp[val] && game.CandidateByRegion(reg, val) == 0)
                        {
                            Type = InvalidType.RegionNoCandidate;
                            InvalidRegion = game.Rule.Regions[reg];
                            InvalidValue = val;
                            return;

                        }
                    }
                }
            }
        }

    }

}
