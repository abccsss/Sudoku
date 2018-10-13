using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    static class Rules
    {
        public static void AddStandardRegions(Rule rule, int width, int height, List<int> initRow, List<int> initColumn, bool enumBigRegion, bool diagonal, List<int> diagonals)
        {
            int n = width * height;

            for(int i = 0; i < initRow.Count; i++)
            {
                var bigRegion = new BigRegion(initRow[i], initColumn[i], n, n);
                bigRegion.Name = enumBigRegion ? "Main Box " + (i + 1) : "";
                rule.BigRegions.Add(bigRegion);
                // rows
                for (int row = 0; row < n; row++)
                {
                    var region = new Region(n);
                    region.AddName($"Row {row + 1}");
                    region.BigRegions.Add(bigRegion);
                    rule.Regions.Add(region);
                    bigRegion.Regions.Add(region);

                    for (int column = 0; column < n; column++)
                    {
                        region.Cells[column] = new Cell(row + initRow[i], column + initColumn[i]);
                        rule.IsAvailable[row + initRow[i], column + initColumn[i]] = true;
                    }
                }

                // columns
                for (int column = 0; column < n; column++)
                {
                    var region = new Region(n);
                    region.AddName($"Column {column + 1}");
                    region.BigRegions.Add(bigRegion);
                    rule.Regions.Add(region);
                    bigRegion.Regions.Add(region);

                    for (int row = 0; row < n; row++)
                        region.Cells[row] = new Cell(row + initRow[i], column + initColumn[i]);
                }

                // boxes
                for (int Row = 0; Row < width; Row++)
                    for (int Column = 0; Column < height; Column++)
                    {
                        var region = new Region(n) { VisualType = RegionVisualType.Stroke };
                        region.AddName($"Box {Row * width + Column + 1}");
                        region.BigRegions.Add(bigRegion);
                        rule.Regions.Add(region);
                        bigRegion.Regions.Add(region);

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
                        var region1 = new Region(k) { VisualType = RegionVisualType.Highlight };
                        region1.AddName($"Diagon {diagcnt}");
                        region1.BigRegions.Add(bigRegion);
                        diagcnt++;
                        var region2 = new Region(k) { VisualType = RegionVisualType.Highlight };
                        region2.AddName($"Diagon {diagcnt}");
                        region2.BigRegions.Add(bigRegion);
                        for (int x = 0; x < k; x++)
                        {
                            region1.Cells[x] = new Cell(x + initRow[i], x + initColumn[i] + diagonals[j]);
                            region2.Cells[x] = new Cell(x + initRow[i], n - 1 - x + initColumn[i] - diagonals[j]);
                        }
                        rule.Regions.Add(region1);
                        bigRegion.Regions.Add(region1);
                        rule.Regions.Add(region2);
                        bigRegion.Regions.Add(region2);
                        if (k < n)
                        {
                            diagcnt++;
                            var region3 = new Region(k) { VisualType = RegionVisualType.Highlight };
                            region3.AddName($"Diagon {diagcnt}");
                            region3.BigRegions.Add(bigRegion);
                            diagcnt++;
                            var region4 = new Region(k) { VisualType = RegionVisualType.Highlight };
                            region4.AddName($"Diagon {diagcnt}");
                            region4.BigRegions.Add(bigRegion);
                            for (int x = 0; x < k; x++)
                            {
                                region3.Cells[x] = new Cell(x + initRow[i] + diagonals[j], x + initColumn[i]);
                                region4.Cells[x] = new Cell(x + initRow[i] + diagonals[j], n - 1 - x + initColumn[i]);
                            }
                            rule.Regions.Add(region3);
                            bigRegion.Regions.Add(region3);
                            rule.Regions.Add(region4);
                            bigRegion.Regions.Add(region4);
                        }
                    }
                }
            }
        }
    }
}
