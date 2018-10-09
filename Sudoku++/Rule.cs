﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    public class Rule
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

        private bool InitEnded { get; set; } = false;

        public Rule(int height, int width, int digits)
        {
            Height = height;
            Width = width;
            Digits = digits;

            IsAvailable = new bool[height, width];
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    IsAvailable[r, c] = false;

            _regionByCell = new List<Region>[height, width];
        }

        public void EndInit()
        {
            if (InitEnded)
                throw new InvalidOperationException();

            InitEnded = true;

            for (int r = 0; r < Height; r++)
                for (int c = 0; c < Width; c++)
                    if (IsAvailable[r, c])
                        _regionByCell[r, c] = new List<Region>();

            var tmpRegions = new List<Region>();
            foreach (var region in Regions) tmpRegions.Add(region);

            foreach (var region in Regions)
            {
                bool flag = true;
                foreach (var regionn in _regionByCell[region.Cells[0].Row, region.Cells[0].Column])
                {
                    if (region.Cells.Length != regionn.Cells.Length) continue;
                    bool flagg = true;
                    for(int i = 0; i < region.Cells.Length; i++)
                    {
                        if(region.Cells[i] != regionn.Cells[i])
                        {
                            flagg = false;
                            break;
                        }
                    }
                    if (flagg)
                    {
                        flag = false;
                        break;
                    }
                }
                if (!flag)
                {
                    tmpRegions.Remove(region);
                    continue;
                }
                foreach (var cell in region.Cells)
                    _regionByCell[cell.Row, cell.Column].Add(region);
            }

            Regions.Clear();
            foreach (var region in tmpRegions) Regions.Add(region);
        }
    }

    public class Region
    {
        public Cell[] Cells { get; }
        public RegionVisualType VisualType { get; set; }
        public string Name { get; set; }
        
        public int KillerSum { get; set; }

        public Region(int cellCount)
        {
            Cells = new Cell[cellCount];
        }
    }

    [Flags]
    public enum RegionVisualType
    {
        None = 0,
        Stroke = 1,
        Highlight = 2,
        Rainbow = 4,
        Killer = 8
    }

    public struct Cell
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

        public override int GetHashCode() => Row << 8 + Column;

        public static Cell None { get; } = new Cell(-1, -1);
    }

    public struct Candidate
    {
        public Cell Cell { get; }
        public int Row => Cell.Row;
        public int Column => Cell.Column;

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

        public static bool operator ==(Candidate c1, Candidate c2) => c1.Cell == c2.Cell && c1.Value == c2.Value;

        public static bool operator !=(Candidate c1, Candidate c2) => !(c1 == c2);

        public override bool Equals(object obj) => obj is Candidate c && this == c;

        public override int GetHashCode() => Cell.GetHashCode() << 8 + Value;
    }
}
