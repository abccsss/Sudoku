using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sudoku.Strategies;

namespace Sudoku
{
    public class Game
    {
        public Rule Rule { get; }

        private int[,] _value;
        public int Value(int row, int column) => _value[row, column];
        public int Value(Cell cell) => _value[cell.Row, cell.Column];

        private bool[,,] _isCandidate;
        public bool IsCandidate(int row, int column, int value) => _isCandidate[row, column, value];
        public bool IsCandidate(Cell cell, int value) => _isCandidate[cell.Row, cell.Column, value];

        private int[,] _candidateCount;
        public int CandidateCount(int row, int column) => _candidateCount[row, column];
        public int CandidateCount(Cell cell) => _candidateCount[cell.Row, cell.Column];

        private int[,] _candidateByRegion;//[#region, value]
        public int CandidateByRegion(int region, int value) => _candidateByRegion[region, value];

        public Graph Graph { get; }
        public bool HasGraph;

        public Game(Rule rule)
        {
            Rule = rule;

            HasGraph = false;

            _value = new int[rule.Width, rule.Height];
            _isCandidate = new bool[rule.Width, rule.Height, rule.Digits];
            _candidateCount = new int[rule.Width, rule.Height];
            _candidateByRegion = new int[rule.Regions.Count, rule.Digits];

            for (int r = 0; r < rule.Height; r++)
                for (int c = 0; c < rule.Width; c++)
                {
                    _value[r, c] = -1;

                    if (rule.IsAvailable[r, c])
                    {
                        _candidateCount[r, c] = rule.Digits;
                        for (int i = 0; i < rule.Digits; i++)
                            _isCandidate[r, c, i] = true;
                    }
                }

            for (int reg = 0; reg < rule.Regions.Count; reg++){
                for (int val = 0; val < rule.Digits; val++){
                    _candidateByRegion[reg, val] = rule.Regions[reg].Cells.Length;
                }
            }
        }

        private static char CharFromValue(int value)
        {
            if (value >= 0 && value < 9)
                return (char)('1' + value);
            if (value >= 9 && value < 35)
                return (char)('a' + value - 9);
            return '.';
        }

        private static int ValueFromChar(char c)
        {
            if (c >= '1' && c <= '9')
                return c - '1';
            if (c >= 'a' && c <= 'z')
                return c - 'a' + 9;
            if (c >= 'A' && c <= 'Z')
                return c - 'A' + 9;
            return -1;
        }

        public override string ToString()
        {
            string s = string.Empty;
            for (int r = 0; r < Rule.Height; r++)
                for (int c = 0; c < Rule.Width; c++)
                    s += CharFromValue(Value(r, c));
            return s;
        }

        public static Game FromString(Rule rule, string s)
        {
            var game = new Game(rule);
            int index = 0;

            for (int r = 0; r < rule.Height; r++)
                for (int c = 0; c < rule.Width; c++)
                {
                    if (index == s.Length)
                        return game;

                    if (s[index] == '.' || s[index] == '-')
                    {
                        index++;
                        continue;
                    }

                    int value = -1;
                    while (value == -1 && index < s.Length)
                        value = ValueFromChar(s[index++]);

                    if (value == -1)
                        continue; // return game;
                    game.SetValue(r, c, value);
                }

            return game;
        }

        public void SetValue(int row, int column, int value)
        {
            _value[row, column] = value;

            if (value != -1)
            {
                for (int i = 0; i < Rule.Digits; i++)
                    SetCandidate(row, column, i, false);

                foreach (var region in Rule.RegionByCell(row, column))
                    foreach (var cell in region.Cells)
                        SetCandidate(cell, value, false);
            }
        }

        public void SetValue(Cell cell, int value) => SetValue(cell.Row, cell.Column, value);

        public void SetCandidate(int row, int column, int value, bool isCandidate)
        {
            bool b = _isCandidate[row, column, value];
            if (b == isCandidate) return;

            _isCandidate[row, column, value] = isCandidate;
            _candidateCount[row, column] += (isCandidate ? 1 : -1);

            foreach (var region in Rule.RegionByCell(row, column))
            {
                _candidateByRegion[Rule.Regions.IndexOf(region), value] += (isCandidate ? 1 : -1);
            }
        }
        
        public void SetCandidate(Cell cell, int value, bool isCandidate) => SetCandidate(cell.Row, cell.Column, value, isCandidate);

        public Game Clone()
        {
            var game = new Game(Rule);
            for (int r = 0; r < Rule.Height; r++)
                for (int c = 0; c < Rule.Height; c++)
                    if (Rule.IsAvailable[r, c])
                    {
                        if (Value(r, c) == -1)
                            for (int i = 0; i < Rule.Digits; i++)
                                game.SetCandidate(r, c, i, IsCandidate(r, c, i));
                        else
                            game.SetValue(r, c, Value(r, c));
                    }

            return game;
        }

        private bool SetAllSingles()
        {
            for (int r = 0; r < Rule.Height; r++)
                for (int c = 0; c < Rule.Width; c++)
                {
                    if (!Rule.IsAvailable[r, c] || Value(r, c) != -1)
                        continue;

                    if (CandidateCount(r, c) == 0)
                        return false;
                    if (CandidateCount(r, c) == 1)
                        for (int i = 0; i < Rule.Digits; i++)
                            if (IsCandidate(r, c, i))
                            {
                                SetValue(r, c, i);
                                return SetAllSingles();
                            }
                }

            return true;
        }

        public static Game Generate(Rule rule)
        {
            var random = new Random();

            while (true)
            {
                var game = new Game(rule);
                var perm = Algorithms.RandomPermutation(rule.Width * rule.Height);
                bool flag = false;

                for (int i = 0; i < rule.Width * rule.Height; i++)
                {
                    int r = perm[i] / rule.Width, c = perm[i] % rule.Width;
                    if (!rule.IsAvailable[r, c] || game.Value(r, c) != -1)
                        continue;

                    int cc = game.CandidateCount(r, c);
                    if (cc == 0)
                    {
                        flag = true;
                        break;
                    }

                    int index = random.Next(cc);
                    int value = -1;
                    while (index >= 0)
                        if (game.IsCandidate(r, c, ++value))
                            index--;
                    game.SetValue(r, c, value);

                    game.SetAllSingles();
                }

                if (!flag)
                    return game;
            }
        }

        private int _Solve(int maxSolutions)
        {
            if (maxSolutions <= 0)
                return 0;

            Cell cell = Cell.None;
            int candCount = Rule.Digits;

            // start from the cell with the fewest candidates
            for (int r = 0; r < Rule.Height; r++)
                for (int c = 0; c < Rule.Width; c++)
                    if (Rule.IsAvailable[r, c] && Value(r, c) == -1 && CandidateCount(r, c) < candCount)
                    {
                        candCount = CandidateCount(r, c);
                        cell = new Cell(r, c);
                    }
            if (cell == Cell.None)
                return 1;

            int solutions = 0;
            for (int i = 0; i < Rule.Digits; i++)
            {
                if (!IsCandidate(cell, i))
                    continue;

                var newGame = Clone();
                newGame.SetValue(cell, i);

                // TODO: reduce
                newGame.SetAllSingles();

                solutions += newGame._Solve(maxSolutions - solutions);
            }

            return solutions > maxSolutions ? maxSolutions : solutions;
        }

        public int Solve(int maxSolutions)
        {
            if (!SetAllSingles())
                return 0;

            return _Solve(maxSolutions);
        }

        public Game GeneratePuzzle(double min, double max, bool symmetric)
        {
            int cellCount = 0, h = Rule.Height, w = Rule.Width;

            for (int r = 0; r < h; r++)
                for (int c = 0; c < w; c++)
                {
                    if (symmetric)
                    {
                        if (r > (h - 1) / 2) break;
                        if (h % 2 == 1 && r == h / 2 && c > (w - 1) / 2) break;
                    }

                    if (Rule.IsAvailable[r, c] ||
                        (symmetric && Rule.IsAvailable[h - r - 1, w - c - 1]))
                        cellCount++;
                }

            var perm = Algorithms.RandomPermutation(cellCount);
            var rPerm = new int[cellCount];
            var cPerm = new int[cellCount];
            int cellIndex = 0;

            for (int r = 0; r < h; r++)
                for (int c = 0; c < w; c++)
                    if (Rule.IsAvailable[r, c] ||
                        (symmetric && Rule.IsAvailable[h - r - 1, w - c - 1]))
                    {
                        rPerm[perm[cellIndex]] = r;
                        cPerm[perm[cellIndex]] = c;
                        if (++cellIndex == cellCount) goto brk;
                    }
            brk:;

            var remove = new bool[cellCount];
            int removedCount = 0;

            for (int i = 0; i < cellCount; i++)
            {
                remove[i] = true;

                var game = new Game(Rule);
                for (int j = 0; j < cellCount; j++)
                    if (!remove[j])
                    {
                        if (Rule.IsAvailable[rPerm[j], cPerm[j]])
                            game.SetValue(rPerm[j], cPerm[j], Value(rPerm[j], cPerm[j]));
                        if (symmetric && Rule.IsAvailable[h - rPerm[j] - 1, w - cPerm[j] - 1])
                            game.SetValue(h - rPerm[j] - 1, w - cPerm[j] - 1, Value(h - rPerm[j] - 1, w - cPerm[j] - 1));
                    }

                remove[i] = game.Solve(2) == 1;

                if (remove[i]) removedCount++;
                if (removedCount + .51 + cellCount * min > cellCount) break;

                if (i + .5 > removedCount + cellCount * max)
                    return null;
            }

            var result = new Game(Rule);
            for (int j = 0; j < cellCount; j++)
                if (!remove[j])
                {
                    if (Rule.IsAvailable[rPerm[j], cPerm[j]])
                        result.SetValue(rPerm[j], cPerm[j], Value(rPerm[j], cPerm[j]));
                    if (symmetric && Rule.IsAvailable[h - rPerm[j] - 1, w - cPerm[j] - 1])
                        result.SetValue(h - rPerm[j] - 1, w - cPerm[j] - 1, Value(h - rPerm[j] - 1, w - cPerm[j] - 1));
                }
            return result;
        }
    }
}
