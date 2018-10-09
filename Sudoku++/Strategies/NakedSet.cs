using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Strategies
{
    class NakedSetStrategy : Strategy
    {
        public int Size;

        public NakedSetStrategy(int size)
        {
            Size = size;
        }

        public List<Cell> AllCells;
        public List<int> AllValues;
        public Region Region;
        public List<Candidate> Eliminations;

        public int FindNakedSingle(Game game)
        {
            for (int r = 0; r < game.Rule.Height; r++)
                for (int c = 0; c < game.Rule.Width; c++)
                {
                    if (game.Rule.IsAvailable[r, c] && game.CandidateCount(r, c) == 1)
                    {
                        AllCells.Add(new Cell(r, c));
                        for (int v = 0; v < game.Rule.Digits; v++)
                            if (game.IsCandidate(r, c, v))
                            {
                                AllValues.Add(v);
                                break;
                            }
                        return 1;
                    }
                }
            return 0;
        }

        public int FindNakedSet(Game game)
        {
            foreach (var region in game.Rule.Regions)
            {
                if (region.Cells.Length <= Size) continue;
                var cellList = new List<Cell>();
                int cellCnt = 0;
                foreach (var cell in region.Cells)
                {
                    if (game.CandidateCount(cell) > 0)
                    {
                        cellCnt++;
                        if (game.CandidateCount(cell) <= Size)cellList.Add(cell);
                    }
                }
                if (cellList.Count < Size) continue;
                if (region.Cells.Length == game.Rule.Digits && Size * 2 > cellCnt) continue;
                var p = new int[Size];
                for (int i = 0; i < Size; i++) p[i] = i;
                do
                {
                    var q = new int[game.Rule.Digits];
                    for (int i = 0; i < game.Rule.Digits; i++) q[i] = 0;
                    int cnt = 0;
                    for (int i = 0; i < Size; i++)
                    {
                        for (int j = 0; j < game.Rule.Digits; j++)
                        {
                            if (game.IsCandidate(cellList[p[i]], j))
                            {
                                if (q[j] == 0)
                                {
                                    if (cnt == Size) goto brk;
                                    cnt++;
                                }
                                q[j]++;
                            }
                        }
                    }
                    bool flag = false;
                    for (int j = 0; j < game.Rule.Digits; j++)
                        if (q[j] > 0)
                        {
                            if (q[j] < game.CandidateByRegion(game.Rule.Regions.IndexOf(region), j))
                            {
                                flag = true;
                                break;
                            }
                        }
                    if (flag)
                    {
                        Region = region;
                        for (int i = 0; i < Size; i++) AllCells.Add(cellList[p[i]]);
                        for (int j = 0; j < game.Rule.Digits; j++)
                        {
                            if (q[j] > 0) AllValues.Add(j);
                        }
                        foreach(var cell in region.Cells)
                        {
                            if (AllCells.Contains(cell)) continue;
                            foreach(var val in AllValues)
                            {
                                if (game.IsCandidate(cell, val))
                                    Eliminations.Add(new Candidate(cell, val));
                            }
                        }
                        return 1;
                    }
                    brk:;
                } while (Algorithms.NextCombination(p, cellList.Count, Size));
            }
            return 0;
        }

        public override Step FindStep(Game game, bool generateHints)
        {
            AllCells = new List<Cell>();
            AllValues = new List<int>();
            Eliminations = new List<Candidate>();
            Region = null;
            if (Size == 1)
            {
                int n = FindNakedSingle(game);
                if (n == 0)
                    return null;
                game.SetValue(AllCells[0], AllValues[0]);
                var step = new Step
                {
                    NewGame = game
                };
                if (generateHints)
                {
                    step.Frames.Add(new StepFrame
                    {
                        Caption = "Naked Single: R" + (AllCells[0].Row + 1) + "C" + (AllCells[0].Column + 1) + " = " + (AllValues[0] + 1),
                        Highlights = { AllCells[0] },
                        Nodes = {new HintNode
                        {
                            Candidates = { new Candidate(AllCells[0], AllValues[0]) },
                            Color = HintColor.Green
                        } }
                    });
                }
                return step;
            }
            int nn = FindNakedSet(game);
            if (nn == 1)
            {
                foreach(var candi in Eliminations)
                {
                    game.SetCandidate(candi.Cell, candi.Value, false);
                }
                var step = new Step
                {
                    NewGame = game
                };
                if (generateHints)
                {
                    var frame = new StepFrame();
                    foreach (var cell in Region.Cells) frame.Highlights.Add(cell);
                    foreach (var candi in Eliminations) frame.Nodes.Add(new HintNode { Candidates = { candi }, Color = HintColor.Red });
                    foreach (var cell in AllCells)
                        foreach (var val in AllValues)
                            if (game.IsCandidate(cell, val))
                                frame.Nodes.Add(new HintNode { Candidates = { new Candidate(cell, val) }, Color = HintColor.Green });
                    frame.Caption = "Naked " + Algorithms.TupleName(Size) + ": Values ";
                    for(int i = 0; i < Size; i++)
                    {
                        frame.Caption = frame.Caption + (AllValues[i] + 1);
                        if (i < Size - 1) frame.Caption = frame.Caption + ", ";
                    }
                    frame.Caption = frame.Caption + " in " + Region.Name;
                    step.Frames.Add(frame);
                }
                return step;
            }
            return null;
        }
    }
}
