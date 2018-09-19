using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    class Rule
    {
        public int Height { get; }
        public int Width { get; }
        public int Digits { get; }

        public string Name { get; }

        public List<Region> Regions { get; } = new List<Region>();
        public bool[,] IsAvailable { get; }

        private List<Region>[,] _regionByCell { get; }
        public List<Region> RegionByCell(int row, int column) => _regionByCell[row, column];
        public List<Region> RegionByCell(Cell cell) => _regionByCell[cell.Row, cell.Column];

        public Rule(string name, int height, int width, int digits)
        {
            Height = height;
            Width = width;
            Digits = digits;
            IsAvailable = new bool[height, width];
            _regionByCell = new List<Region>[height, width];
        }

        public void EndInit()
        {
            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    if (IsAvailable[r, c])
                        _regionByCell[r, c] = new List<Region>();

            foreach (var region in Regions)
                foreach (var cell in region.Cells)
                    _regionByCell[cell.Row, cell.Column].Add(region);
        }
    }

    class Region
    {
        public Cell[] Cells { get; }
        public RegionVisual Visual { get; set; }
        
        public int KillerSum { get; set; }

        public Region(int cellCount)
        {
            Cells = new Cell[cellCount];
        }
    }

    [Flags]
    enum RegionVisual
    {
        None = 0,
        Stroke = 1,
        Highlight = 2,
        Rainbow = 4,
        Killer = 8
    }

    struct Cell
    {
        public int Row { get; }
        public int Column { get; }

        public Cell(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public static bool operator ==(Cell left, Cell right) => left.Row == right.Row && left.Column == right.Column;

        public static bool operator !=(Cell left, Cell right) => left.Row != right.Row || left.Column != right.Column;

        public override bool Equals(object obj) => obj is Cell cell && this == cell;

        public override int GetHashCode() => Row << 16 + Column;
    }

    struct Candidate
    {
        public Cell Cell { get; }
        public int Value { get; }

        public Candidate(Cell cell, int value)
        {
            Cell = cell;
            Value = value;
        }

        public Candidate(int row, int column, int value)
        {
            Cell = new Cell(row, column);
            Value = value;
        }
    }
}
