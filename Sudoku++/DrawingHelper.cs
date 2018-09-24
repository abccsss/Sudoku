using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Sudoku
{
    static class DrawingHelper
    {
        public static Drawing GetGridDrawing(Brush brush, Rule rule, double cellSize, double thickWidth, double thinWidth)
        {
            var thinPen = new Pen(brush, thinWidth);
            thinPen.Freeze();

            var thickPen = new Pen(brush, thickWidth)
            {
                StartLineCap = PenLineCap.Square,
                EndLineCap = PenLineCap.Square
            };
            thickPen.Freeze();

            var visual = new DrawingVisual();

            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(AppResources.BackgroundBrush, null, new Rect(-thickWidth / 2, -thickWidth / 2,
                    rule.Width * cellSize + thickWidth, rule.Height * cellSize + thickWidth));

                // colorings
                bool[,] colored = new bool[rule.Height, rule.Width];

                int rainbows = 0, rainbowIndex = 0;
                foreach (var region in rule.Regions)
                    if (region.VisualType == RegionVisualType.Rainbow)
                        rainbows++;

                foreach (var region in rule.Regions)
                {
                    Brush br = null;
                    if (region.VisualType == RegionVisualType.Highlight)
                        br = AppResources.RegionHighlightBrush;
                    else if (region.VisualType == RegionVisualType.Rainbow)
                        br = AppResources.GetRainbowBrush(rainbows, rainbowIndex++);
                    else continue;

                    foreach (var cell in region.Cells)
                        if (!colored[cell.Row, cell.Column])
                        {
                            dc.DrawRectangle(br, null, new Rect(cell.Column * cellSize, cell.Row * cellSize, cellSize, cellSize));
                            colored[cell.Row, cell.Column] = true;
                        }
                }

                // blackout
                for (int r = 0; r < rule.Height; r++)
                    for (int c = 0; c < rule.Width; c++)
                        if (!rule.IsAvailable[r, c] &&
                            (r == 0 || rule.IsAvailable[r - 1, c]) &&
                            (r == rule.Height - 1 || rule.IsAvailable[r + 1, c]) &&
                            (c == 0 || rule.IsAvailable[r, c - 1]) &&
                            (c == rule.Width - 1 || rule.IsAvailable[r, c + 1]))
                            dc.DrawRectangle(brush, null, new Rect(
                                c * cellSize - thickWidth / 2, r * cellSize - thickWidth / 2,
                                cellSize + thickWidth, cellSize + thickWidth));

                // grid lines
                bool[,] hThickLine = new bool[rule.Height + 1, rule.Width],
                    vThickLine = new bool[rule.Height, rule.Width + 1];

                for (int r = 0; r <= rule.Height; r++)
                    for (int c = 0; c < rule.Width; c++)
                        hThickLine[r, c] =
                            (r == 0 || !rule.IsAvailable[r - 1, c]) ^
                            (r == rule.Height || !rule.IsAvailable[r, c]);

                for (int r = 0; r < rule.Height; r++)
                    for (int c = 0; c <= rule.Width; c++)
                        vThickLine[r, c] =
                            (c == 0 || !rule.IsAvailable[r, c - 1]) ^
                            (c == rule.Height || !rule.IsAvailable[r, c]);

                // remove thick lines on the border if e.g. (0, i) and (n - 1, i) belongs to the same region
                foreach (var region in rule.Regions)
                    if (region.VisualType == RegionVisualType.Stroke)
                        foreach (var cell in region.Cells)
                        {
                            if (cell.Row == 0 && region.Cells.Contains(new Cell(rule.Height - 1, cell.Column)))
                            {
                                hThickLine[0, cell.Column] = false;
                                hThickLine[rule.Height, cell.Column] = false;
                            }
                            if (cell.Column == 0 && region.Cells.Contains(new Cell(cell.Row, rule.Width - 1)))
                            {
                                vThickLine[cell.Row, 0] = false;
                                vThickLine[cell.Row, rule.Width] = false;
                            }
                        }

                // add thick lines on boundary of regions
                foreach (var region in rule.Regions)
                    if (region.VisualType == RegionVisualType.Stroke)
                        foreach (var cell in region.Cells)
                        {
                            if (cell.Row > 0 && !region.Cells.Contains(new Cell(cell.Row - 1, cell.Column)))
                                hThickLine[cell.Row, cell.Column] = true;
                            if (cell.Row < rule.Height - 1 && !region.Cells.Contains(new Cell(cell.Row + 1, cell.Column)))
                                hThickLine[cell.Row + 1, cell.Column] = true;

                            if (cell.Column > 0 && !region.Cells.Contains(new Cell(cell.Row, cell.Column - 1)))
                                vThickLine[cell.Row, cell.Column] = true;
                            if (cell.Column < rule.Width - 1 && !region.Cells.Contains(new Cell(cell.Row, cell.Column + 1)))
                                vThickLine[cell.Row, cell.Column + 1] = true;
                        }

                // draw the lines
                for (int r = 0; r <= rule.Height; r++)
                    for (int c = 0; c < rule.Width; c++)
                        if ((r > 0 && rule.IsAvailable[r - 1, c]) ||
                            (r < rule.Height && rule.IsAvailable[r, c]))
                            dc.DrawLine(hThickLine[r, c] ? thickPen : thinPen,
                                new Point(c * cellSize, r * cellSize),
                                new Point((c + 1) * cellSize, r * cellSize));

                for (int r = 0; r < rule.Height; r++)
                    for (int c = 0; c <= rule.Width; c++)
                        if ((c > 0 && rule.IsAvailable[r, c - 1]) ||
                            (c < rule.Width && rule.IsAvailable[r, c]))
                            dc.DrawLine(vThickLine[r, c] ? thickPen : thinPen,
                                new Point(c * cellSize, r * cellSize),
                                new Point(c * cellSize, (r + 1) * cellSize));
            }

            visual.Drawing.Freeze();
            return visual.Drawing;
        }
    }
}
