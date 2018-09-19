using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    class Game
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

        public Game(Rule rule)
        {
            Rule = rule;

            _value = new int[rule.Width, rule.Height];
            _isCandidate = new bool[rule.Width, rule.Height, rule.Digits];

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
        }

        public void SetValue(int row, int column, int value)
        {
            _value[row, column] = value;
            _candidateCount[row, column] = 0;
            for (int i = 0; i < Rule.Digits; i++)
                _isCandidate[row, column, i] = false;

            foreach (var region in Rule.RegionByCell(row, column))
                foreach (var cell in region.Cells)
                    SetCandidate(cell, value, false);
        }

        public void SetValue(Cell cell, int value) => SetValue(cell.Row, cell.Column, value);

        public void SetCandidate(int row, int column, int value, bool isCandidate)
        {
            bool b = _isCandidate[row, column, value];
            if (b == isCandidate) return;

            _isCandidate[row, column, value] = isCandidate;
            _candidateCount[row, column] += (isCandidate ? 1 : -1);
        }

        public void SetCandidate(Cell cell, int value, bool isCandidate) => SetCandidate(cell.Row, cell.Column, value, isCandidate);
    }
}
