using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    static class Rules
    {
        public static void AddStandardRegions(Rule rule, int width, int height, List<int> initRow, List<int> initColumn, bool num, bool diagonal, List<int> diagonals)
        {
            int n = width * height;

            for(int i = 0; i < initRow.Count; i++)
            {
                // rows
                for (int row = 0; row < n; row++)
                {
                    var region = new Region(n) { Name = (num ? $"Main Box {i + 1} " : $"") + $"Row {row + 1}" };
                    rule.Regions.Add(region);

                    for (int column = 0; column < n; column++)
                    {
                        region.Cells[column] = new Cell(row + initRow[i], column + initColumn[i]);
                        rule.IsAvailable[row + initRow[i], column + initColumn[i]] = true;
                    }
                }

                // columns
                for (int column = 0; column < n; column++)
                {
                    var region = new Region(n) { Name = (num ? $"Main Box {i + 1} " : $"") + $"Column {column + 1}" };
                    rule.Regions.Add(region);

                    for (int row = 0; row < n; row++)
                        region.Cells[row] = new Cell(row + initRow[i], column + initColumn[i]);
                }

                // boxes
                for (int Row = 0; Row < width; Row++)
                    for (int Column = 0; Column < height; Column++)
                    {
                        var region = new Region(width * height) { VisualType = RegionVisualType.Stroke, Name = (num ? $"Main Box {i + 1} " : $"") + $"Box {Row * width + Column + 1}" };
                        rule.Regions.Add(region);

                        for (int row = 0; row < height; row++)
                            for (int column = 0; column < width; column++)
                            {
                                var cell = new Cell(Row * height + row + initRow[i], Column * width + column + initColumn[i]);
                                region.Cells[row * width + column] = cell;
                            }
                    }

                // diagonals
                if (diagonal)
                {
                    int diagcnt = 0;
                    for (int j = 0; j < diagonals.Count; j++)
                    {
                        int k = n - diagonals[j];
                        diagcnt++;
                        var region1 = new Region(k) { VisualType = RegionVisualType.Highlight, Name = (num ? $"Main Box {i + 1} " : $"") + $"Diagon {diagcnt}" };
                        diagcnt++;
                        var region2 = new Region(k) { VisualType = RegionVisualType.Highlight, Name = (num ? $"Main Box {i + 1} " : $"") + $"Diagon {diagcnt}" };
                        for (int x = 0; x < k; x++)
                        {
                            region1.Cells[x] = new Cell(x + initRow[i], x + initColumn[i] + diagonals[j]);
                            region2.Cells[x] = new Cell(x + initRow[i], n - 1 - x + initColumn[i] - diagonals[j]);
                        }
                        rule.Regions.Add(region1);
                        rule.Regions.Add(region2);
                        if (k < n)
                        {
                            diagcnt++;
                            var region3 = new Region(k) { VisualType = RegionVisualType.Highlight, Name = (num ? $"Main Box {i + 1} " : $"") + $"Diagon {diagcnt}" };
                            diagcnt++;
                            var region4 = new Region(k) { VisualType = RegionVisualType.Highlight, Name = (num ? $"Main Box {i + 1} " : $"") + $"Diagon {diagcnt}" };
                            for (int x = 0; x < k; x++)
                            {
                                region3.Cells[x] = new Cell(x + initRow[i] + diagonals[j], x + initColumn[i]);
                                region4.Cells[x] = new Cell(x + initRow[i] + diagonals[j], n - 1 - x + initColumn[i]);
                            }
                            rule.Regions.Add(region3);
                            rule.Regions.Add(region4);
                        }
                    }
                }
            }
        }
    }
}
