using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku.Strategies
{
    class HiddenSetStrategy : Strategy
    {
        public int Size;

        public HiddenSetStrategy(int size)
        {
            Size = size;
        }

        public List<Cell> AllCells;
        public List<int> AllValues;
        public Region Region;
        public List<Candidate> Eliminations;

        public int FindHiddenSet(Game game)
        {
            for (int i = 0; i < game.Rule.Regions.Count; i++) 
            {
                var region = game.Rule.Regions[i];
                if (region.Cells.Length < game.Rule.Digits) continue;
                var valList = new List<int>();
                int valCnt = 0;
                for (int j = 0; j < game.Rule.Digits; j++)
                {
                    if (game.CandidateByRegion(i, j) > 0)
                    {
                        valCnt++;
                        if (game.CandidateByRegion(i, j) <= Size) valList.Add(j);
                    }
                }
                if (valList.Count < Size) continue;
                if (valCnt < 2 * Size) continue;
                var p = new int[Size];
                for (int ii = 0; ii < Size; ii++) p[ii] = ii;
                do
                {
                    var q = new int[game.Rule.Digits];
                    for (int ii = 0; ii < game.Rule.Digits; ii++) q[ii] = 0;
                    int cnt = 0;
                    for (int ii = 0; ii < game.Rule.Digits; ii++)
                        for(int j = 0; j < Size; j++)
                        {
                            if (game.IsCandidate(region.Cells[ii], valList[p[j]]))
                            {
                                if (q[ii] == 0)
                                {
                                    if (cnt == Size) goto brk;
                                    cnt++;
                                }
                                q[ii]++;
                            }
                        }
                    bool flag = false;
                    for (int ii = 0; ii < game.Rule.Digits; ii++)
                        if (q[ii] > 0)
                        {
                            if (q[ii] < game.CandidateCount(region.Cells[ii]))
                            {
                                flag = true;
                                break;
                            }
                        }
                    if (flag)
                    {
                        Region = region;
                        for (int j = 0; j < Size; j++) AllValues.Add(valList[p[j]]);
                        for (int j = 0; j < game.Rule.Digits; j++)
                        {
                            if (q[j] > 0) AllCells.Add(region.Cells[j]);
                        }
                        foreach (var cell in AllCells)
                        {
                            for (int j = 0; j < game.Rule.Digits; j++) 
                            {
                                if (AllValues.Contains(j)) continue;
                                if (game.IsCandidate(cell, j))
                                    Eliminations.Add(new Candidate(cell, j));
                            }
                        }
                        return 1;
                    }
                    brk:;
                } while (Algorithms.NextCombination(p, valList.Count, Size));
            }
            return 0;
        }

        public override Step FindStep(Game game, bool generateHints)
        {
            if (Size * 2 > game.Rule.Digits) return null;
            AllCells = new List<Cell>();
            AllValues = new List<int>();
            Eliminations = new List<Candidate>();
            Region = null;
            int n = FindHiddenSet(game);
            if (n == 1)
            {
                if (Size == 1) game.SetValue(AllCells[0], AllValues[0]);
                else foreach (var candi in Eliminations)
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
                    if (Size > 1)
                    {
                        foreach (var candi in Eliminations) frame.Nodes.Add(new HintNode { Candidates = { candi }, Color = HintColor.Red });
                    }
                    foreach (var cell in AllCells)
                        foreach (var val in AllValues)
                            if (game.IsCandidate(cell, val) || Size == 1)
                                frame.Nodes.Add(new HintNode { Candidates = { new Candidate(cell, val) }, Color = HintColor.Green });
                    if (Size > 1)
                    {
                        frame.Caption = "Hidden " + Algorithms.TupleName(Size) + ": Values ";
                        for (int i = 0; i < Size; i++)
                        {
                            frame.Caption = frame.Caption + (AllValues[i] + 1);
                            if (i < Size - 1) frame.Caption = frame.Caption + ", ";
                        }
                        frame.Caption = frame.Caption + " in " + Region.Name;
                    }
                    else frame.Caption = "Hidden Single: R" + (AllCells[0].Row + 1) + "C" + (AllCells[0].Column + 1) + " = " + (AllValues[0] + 1);
                    step.Frames.Add(frame);
                }
                return step;
            }
            return null;
        }
    }
}
