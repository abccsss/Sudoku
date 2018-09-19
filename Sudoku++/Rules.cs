using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sudoku
{
    static class Rules
    {
        public static Rule GetStandardRule(int width, int height)
        {
            int n = width * height;
            var rule = new Rule($"Standard {n} × {n}", n, n, n);

            for (int Row = 0; Row < width; Row++)
                for (int Column = 0; Column < height; Column++)
                {
                    var region = new Region(n);
                    rule.Regions.Add(region);

                    for (int row = 0; row < height; row++)
                        for (int column = 0; column < width; column++)
                        {
                            var cell = new Cell(Row * height + row, Column * width + column);
                            region.Cells[row * width + column] = cell;
                        }
                }
            
            return rule;
        }
    }
}
