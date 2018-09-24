using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    static class Rules
    {
        private static void AddStandardRegions(Rule rule, int width, int height)
        {
            int n = width * height;

            // rows
            for (int row = 0; row < n; row++)
            {
                var region = new Region(n) { Name = $"Row {row + 1}" };
                rule.Regions.Add(region);

                for (int column = 0; column < n; column++)
                    region.Cells[column] = new Cell(row, column);
            }

            // columns
            for (int column = 0; column < n; column++)
            {
                var region = new Region(n) { Name = $"Column {column + 1}" };
                rule.Regions.Add(region);

                for (int row = 0; row < n; row++)
                    region.Cells[row] = new Cell(row, column);
            }

            // boxes
            for (int Row = 0; Row < width; Row++)
                for (int Column = 0; Column < height; Column++)
                {
                    var region = new Region(width * height) { VisualType = RegionVisualType.Stroke, Name = $"Box {Row * width + Column + 1}" };
                    rule.Regions.Add(region);

                    for (int row = 0; row < height; row++)
                        for (int column = 0; column < width; column++)
                        {
                            var cell = new Cell(Row * height + row, Column * width + column);
                            region.Cells[row * width + column] = cell;
                        }
                }
        }

        public static Rule GetStandardRule(int width, int height)
        {
            int n = width * height;
            var rule = new Rule($"Standard {n} × {n}", n, n, n);

            AddStandardRegions(rule, width, height);

            rule.EndInit();
            return rule;
        }

        public static Rule GetXRule(int width, int height)
        {
            int n = width * height;
            var rule = new Rule($"X-Sudoku {n} × {n}", n, n, n);

            AddStandardRegions(rule, width, height);

            var region = new Region(n)
            {
                Name = "Extra Box 1",
                VisualType = RegionVisualType.Highlight
            };
            for (int i = 0; i < n; i++)
                region.Cells[i] = new Cell(i, i);
            rule.Regions.Add(region);

            region = new Region(n)
            {
                Name = "Extra Box 2",
                VisualType = RegionVisualType.Highlight
            };
            for (int i = 0; i < n; i++)
                region.Cells[i] = new Cell(i, n - 1 - i);
            rule.Regions.Add(region);

            rule.EndInit();
            return rule;
        }
    }
}
