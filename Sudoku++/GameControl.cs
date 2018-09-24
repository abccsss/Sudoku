using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Sudoku
{
    public class GameControl : UserControl
    {
        private Rule Rule { get; }
        private Game Puzzle { get; set; }
        private Game Game { get; set; }
        private List<Game> UndoStack { get; } = new List<Game>();
	    private List<Game> RedoStack { get; } = new List<Game>();

        private Drawing GridDrawing { get; }
        private Grid RootGrid { get; }
        private CellControl[,] Cells { get; }

        public bool IsCreationMode { get; set; }

        public event EventHandler GameChanged = new EventHandler((sender, e) => { });
        public bool HasContradictions { get; private set; } = false;

        private bool HasSelection { get; set; } = false;
        private bool IsCandidatesFilled { get; set; } = false;
        private int CandidateFillingStep { get; set; } = -1;

        public bool CanUndo => UndoStack.Count > 1;
        public bool CanRedo => RedoStack.Count > 0;
        public bool CanFill => !IsCreationMode && !IsCandidatesFilled;

        private DateTime LastKeyDown = DateTime.MinValue;

        private Cell SelectedCell { get; set; } = Cell.None;

        public StepFrame Hint { get; private set; }
        private List<FrameworkElement> HintSwitches { get; } = new List<FrameworkElement>();
        private List<FrameworkElement> HintVisuals { get; } = new List<FrameworkElement>();

        private const double ThickWidth = 8, ThinWidth = 3, CellSize = 100;

        public GameControl(Rule rule)
        {
            Focusable = true;
            FocusVisualStyle = null;
            Width = rule.Width * CellSize + ThickWidth;
            Height = rule.Height * CellSize + ThickWidth;

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            Rule = rule;
            GridDrawing = DrawingHelper.GetGridDrawing(AppResources.TextBrush, rule, CellSize, ThickWidth, ThinWidth);

            RootGrid = new Grid();
            Content = RootGrid;

            Cells = new CellControl[rule.Height, rule.Width];
            for (int r = 0; r < rule.Height; r++)
                for (int c = 0; c < rule.Width; c++)
                    if (rule.IsAvailable[r, c])
                    {
                        Cells[r, c] = new CellControl(r, c, rule.Digits)
                        {
                            Margin = new Thickness(c * CellSize + ThickWidth / 2, r * CellSize + ThickWidth / 2, 0, 0)
                        };
                        Cells[r, c].MouseLeftButtonDown += OnCellMouseLeftButtonDown;
                        RootGrid.Children.Add(Cells[r, c]);
                    }
        }

        public void SetPuzzle(Game puzzle)
        {
            if (Puzzle != null)
                throw new InvalidOperationException();

            Puzzle = puzzle;

            var game = puzzle.Clone();
            for (int r = 0; r < Rule.Height; r++)
                for (int c = 0; c < Rule.Width; c++)
                    if (Rule.IsAvailable[r, c] && game.Value(r, c) == -1)
                        for (int i = 0; i < Rule.Digits; i++)
                            game.SetCandidate(r, c, i, false);
            SetGame(game, true);

            InvalidateVisual();
        }

        public void SetGame(Game game, bool isUserOperation)
        {
            if (game != null)
                Game = game;

            for (int r = 0; r < Rule.Height; r++)
                for (int c = 0; c < Rule.Width; c++)
                {
                    if (Rule.IsAvailable[r, c])
                    {
                        Game.SetValue(r, c, game.Value(r, c));
                        for (int value = 0; value < Rule.Digits; value++)
                            Game.SetCandidate(r, c, value, game.IsCandidate(r, c, value));

                        if (IsCreationMode || Puzzle.Value(r, c) >= 0)
                            DrawValue(r, c, true);
                        else if (game.Value(r, c) >= 0)
                            DrawValue(r, c, false);
                        else
                            DrawCandidates(r, c);
                    }
                }

            if (isUserOperation)
                OnUserOperation();

            CheckContradictions();
        }

        public void FillCandidates()
        {
            if (!CanFill)
                throw new InvalidOperationException();

            if (HasContradictions)
                return;

            var game = GetProperGame();
            IsCandidatesFilled = true;
            CandidateFillingStep = UndoStack.Count;

            SetGame(game, true);
        }

        public void Undo()
        {
            if (!CanUndo)
                throw new InvalidOperationException();

            RedoStack.Add(Game.Clone());
            UndoStack.RemoveAt(UndoStack.Count - 1);
            var game = UndoStack[UndoStack.Count - 1];

            if (UndoStack.Count == CandidateFillingStep)
                IsCandidatesFilled = false;

            RemoveHints();
            SetGame(game.Clone(), false);
            GameChanged(this, null);
        }

        public void Redo()
        {
            if (!CanRedo)
                throw new InvalidOperationException();

            var game = RedoStack[RedoStack.Count - 1];
            RedoStack.RemoveAt(RedoStack.Count - 1);
            UndoStack.Add(game.Clone());

            if (UndoStack.Count == CandidateFillingStep + 1)
                IsCandidatesFilled = true;

            RemoveHints();
            SetGame(game.Clone(), false);
            GameChanged(this, null);
        }

        private void DrawValue(int row, int column, bool isGiven)
        {
            if (HasSelection)
                Cells[SelectedCell.Row, SelectedCell.Column].selectionRectangle.Visibility = Visibility.Hidden;
            HasSelection = false;

            Cells[row, column].SetValue(Game.Value(row, column), isGiven);
        }

        private void DrawValue(bool isGiven)
        {
            DrawValue(SelectedCell.Row, SelectedCell.Column, isGiven);
        }

        private void DrawCandidates(int row, int column)
        {
            Cells[row, column].SetCandidates(Game);
        }

        public void SetHint(StepFrame hint)
        {
            // to be improved

            Hint = hint;

            const double ArrowBend = 1.5;
            const double ArrowSize = 12;

            RemoveHints();

            double radius = 45.0 / ((int)Math.Sqrt(Rule.Digits - 1) + 1);
            List<Ellipse> ellipses = new List<Ellipse>();

            // for arrows to decide how to bend themselves
            PointCollection points = new PointCollection();

            foreach (var node in hint.Nodes)
            {
                foreach (var candidate in node.Candidates)
                {
                    var cell = candidate.Cell;
                    var el = Cells[cell.Row, cell.Column].Ellipses[candidate.Value];

                    points.Add(el.TranslatePoint(new Point(radius, radius), RootGrid));
                }
            }

            // colorings
            for (int i = 0; i < hint.Nodes.Count; i++)
            {
                // avoid repeated nodes
                bool flag = false;
                for (int j = i + 1; j < hint.Nodes.Count; j++)
                    if (hint.Nodes[i].Candidates.Count == hint.Nodes[j].Candidates.Count)
                    {
                        bool flagg = false;
                        for (int k = 0; k < hint.Nodes[i].Candidates.Count; k++)
                            if (hint.Nodes[i].Candidates[k] != hint.Nodes[j].Candidates[k])
                            {
                                flagg = true;
                                break;
                            }
                        if (!flagg)
                        {
                            flag = true;
                            break;
                        }
                    }
                if (flag) continue;

                ellipses.Clear();
                var node = hint.Nodes[i];
                foreach (var candidate in node.Candidates)
                {
                    var cell = candidate.Cell;
                    var el = Cells[cell.Row, cell.Column].Ellipses[candidate.Value];
                    el.Fill = AppResources.HintNodeBrushes[(int)node.Color];
                    el.Visibility = Visibility.Visible;
                    HintSwitches.Add(el);
                    ellipses.Add(el);

                    points.Add(el.TranslatePoint(new Point(radius, radius), RootGrid));
                }

                // lines that connect different candidates of a coloring
                for (int j = 0; j < ellipses.Count - 1; j++)
                {
                    var p1 = ellipses[j].TranslatePoint(new Point(radius, radius), RootGrid);
                    var p2 = ellipses[j + 1].TranslatePoint(new Point(radius, radius), RootGrid);
                    double dx = p2.X - p1.X, dy = p2.Y - p1.Y, l = Math.Sqrt(dx * dx + dy * dy);

                    // Let  n  be the unit normal vector to the right, and  d  be the directional unit vector
                    // Point  p  is in the way iff  (p - p1)·d > r  and  (p - p2)·d < -r  and  (p - p1)·n < r 
                    bool left = false, right = false;
                    foreach (var p in points)
                    {
                        double d = ((p.X - p1.X) * dy - (p.Y - p1.Y) * dx) / l;
                        if (d > radius || d < -radius) continue;
                        if ((p.X - p1.X) * dx + (p.Y - p1.Y) * dy < radius * l) continue;
                        if ((p.X - p2.X) * dx + (p.Y - p2.Y) * dy > -radius * l) continue;

                        if (d == 0)
                            right = true;
                        else if (d > 0)
                            left = true;
                        else right = true;
                    }

                    string data;
                    if (left == right) // straight
                    {
                        data = string.Format("M {0} {1} L {2} {3}",
                            p2.X - dx / l * radius, p2.Y - dy / l * radius, p1.X + dx / l * radius, p1.Y + dy / l * radius);
                    }
                    else if (left) // bend right
                    {
                        data = string.Format("M {0} {1} C {2} {3} {4} {5} {6} {7}",
                            p2.X - (dx + dy) / l * radius * .71, p2.Y - (dy - dx) / l * radius * .71,
                            p2.X - (dx + dy) / l * (ArrowBend + .71) * radius, p2.Y - (dy - dx) / l * (ArrowBend + .71) * radius,
                            p1.X + (dx - dy) / l * (ArrowBend + .71) * radius, p1.Y + (dy + dx) / l * (ArrowBend + .71) * radius,
                            p1.X + (dx - dy) / l * radius * .71, p1.Y + (dy + dx) / l * radius * .71);
                    }
                    else // bend left
                    {
                        data = string.Format("M {0} {1} C {2} {3} {4} {5} {6} {7}",
                            p2.X - (dx - dy) / l * radius * .71, p2.Y - (dy + dx) / l * radius * .71,
                            p2.X - (dx - dy) / l * (ArrowBend + .71) * radius, p2.Y - (dy + dx) / l * (ArrowBend + .71) * radius,
                            p1.X + (dx + dy) / l * (ArrowBend + .71) * radius, p1.Y + (dy - dx) / l * (ArrowBend + .71) * radius,
                            p1.X + (dx + dy) / l * radius * .71, p1.Y + (dy - dx) / l * radius * .71);
                    }

                    var path = new Path
                    {
                        Data = Geometry.Parse(data),
                        StrokeThickness = 8,
                        Stroke = AppResources.HintNodeBrushes[(int)node.Color],
                        Margin = new Thickness(0, 0, -100, -100)
                    };
                    HintVisuals.Add(path);
                    RootGrid.Children.Add(path);
                }
            }

            // highlighted cells
            foreach (var cell in hint.Highlights)
            {
                if (!Rule.IsAvailable[cell.Row, cell.Column]) continue;
                var rect = Cells[cell.Row, cell.Column].highlightRectangle;
                rect.Visibility = Visibility.Visible;
                HintSwitches.Add(rect);
            }

            // arrows
            foreach (var arrow in hint.Arrows)
            {
                double dd = 0;
                Candidate c1 = arrow.From.Candidates[0],
                    c2 = arrow.To.Candidates[0];

                foreach (Candidate x1 in arrow.From.Candidates)
                    foreach (Candidate x2 in arrow.To.Candidates)
                    {
                        double ddd = (x1.Row - x2.Row) * (x1.Row - x2.Row) + (x1.Column - x2.Column) * (x1.Column - x2.Column);
                        if (1 / ddd > dd)
                        {
                            dd = 1 / ddd;
                            c1 = x1;
                            c2 = x2;
                        }
                    }

                var el1 = Cells[c1.Row, c1.Column].Ellipses[c1.Value];
                var el2 = Cells[c2.Row, c2.Column].Ellipses[c2.Value];
                var p1 = el1.TranslatePoint(new Point(radius, radius), RootGrid);
                var p2 = el2.TranslatePoint(new Point(radius, radius), RootGrid);
                double dx = p2.X - p1.X, dy = p2.Y - p1.Y, l = Math.Sqrt(dx * dx + dy * dy);

                // Let  n  be the unit normal vector to the right, and  d  be the directional unit vector
                // Point  p  is in the way iff  (p - p1)·d > r  and  (p - p2)·d < -r  and  (p - p1)·n < r 
                bool left = false, right = false;
                foreach (var p in points)
                {
                    double d = ((p.X - p1.X) * dy - (p.Y - p1.Y) * dx) / l;
                    if (d > radius || d < -radius) continue;
                    if ((p.X - p1.X) * dx + (p.Y - p1.Y) * dy < radius * l) continue;
                    if ((p.X - p2.X) * dx + (p.Y - p2.Y) * dy > -radius * l) continue;

                    if (d == 0)
                        right = true;
                    else if (d > 0)
                        left = true;
                    else right = true;
                }

                string data;
                if (left == right) // straight
                {
                    data = string.Format("M {0} {1} L {2} {3}",
                        p2.X - dx / l * radius, p2.Y - dy / l * radius, p1.X + dx / l * radius, p1.Y + dy / l * radius);
                }
                else if (left) // bend right
                {
                    data = string.Format("M {0} {1} C {2} {3} {4} {5} {6} {7}",
                        p2.X - (dx + dy) / l * radius * .71, p2.Y - (dy - dx) / l * radius * .71,
                        p2.X - (dx + dy) / l * (ArrowBend + .71) * radius, p2.Y - (dy - dx) / l * (ArrowBend + .71) * radius,
                        p1.X + (dx - dy) / l * (ArrowBend + .71) * radius, p1.Y + (dy + dx) / l * (ArrowBend + .71) * radius,
                        p1.X + (dx - dy) / l * radius * .71, p1.Y + (dy + dx) / l * radius * .71);
                }
                else // bend left
                {
                    data = string.Format("M {0} {1} C {2} {3} {4} {5} {6} {7}",
                        p2.X - (dx - dy) / l * radius * .71, p2.Y - (dy + dx) / l * radius * .71,
                        p2.X - (dx - dy) / l * (ArrowBend + .71) * radius, p2.Y - (dy + dx) / l * (ArrowBend + .71) * radius,
                        p1.X + (dx + dy) / l * (ArrowBend + .71) * radius, p1.Y + (dy - dx) / l * (ArrowBend + .71) * radius,
                        p1.X + (dx + dy) / l * radius * .71, p1.Y + (dy - dx) / l * radius * .71);
                }

                var path = new Path
                {
                    Data = Geometry.Parse(data),
                    StrokeThickness = 5,
                    Stroke = AppResources.ArrowBrush,
                    Margin = new Thickness(0, 0, -100, -100)
                };

                // solid arrows needs (10000, 3) to remove the ugly bit at the arrow tip
                path.StrokeDashArray = new DoubleCollection
                {
                    arrow.IsDashed ? 4 : 10000,
                    3
                };
                path.StrokeDashOffset = -3;

                // let p1 = arrowhead and p2 = directional unit
                p1 = left == right ?
                    new Point(p2.X - dx / l * radius, p2.Y - dy / l * radius) : left ?
                    new Point(p2.X - (dx + dy) / l * radius * .71, p2.Y - (dy - dx) / l * radius * .71) :
                    new Point(p2.X - (dx - dy) / l * radius * .71, p2.Y - (dy + dx) / l * radius * .71);
                Point p3;
                path.Data.GetFlattenedPathGeometry().GetPointAtFractionLength(2.2 * ArrowSize / l, out p2, out p3);
                p2 = new Point((p2.X - p1.X) / (2.2 * ArrowSize), (p2.Y - p1.Y) / (2.2 * ArrowSize));

                var arrowhead = new Path
                {
                    Data = Geometry.Parse(string.Format("M {0} {1} L {2} {3} {4} {5} {6} {7}",
                        p1.X, p1.Y,
                        p1.X + p2.X * 3 * ArrowSize + p2.Y * ArrowSize, p1.Y + p2.Y * 3 * ArrowSize - p2.X * ArrowSize,
                        p1.X + p2.X * 2.2 * ArrowSize, p1.Y + p2.Y * 2.2 * ArrowSize,
                        p1.X + p2.X * 3 * ArrowSize - p2.Y * ArrowSize, p1.Y + p2.Y * 3 * ArrowSize + p2.X * ArrowSize)),
                    Fill = AppResources.ArrowBrush,
                    Margin = new Thickness(0, 0, -100, -100)
                };

                HintVisuals.Add(path);
                HintVisuals.Add(arrowhead);
                RootGrid.Children.Add(path);
                RootGrid.Children.Add(arrowhead);
            }
        }

        public void RemoveHints()
        {
            foreach (var element in HintSwitches)
                element.Visibility = Visibility.Hidden;
            foreach (var element in HintVisuals)
                RootGrid.Children.Remove(element);
            HintVisuals.Clear();

            Hint = null;
        }

        public Game GetProperGame()
        {
            if (IsCandidatesFilled)
                return Game.Clone();
            else
            {
                var game = new Game(Rule);
                for (int r = 0; r < Rule.Height; r++)
                    for (int c = 0; c < Rule.Width; c++)
                        if (Game.Value(r, c) >= 0)
                            game.SetValue(r, c, Game.Value(r, c));

                return game;
            }
        }

        private void CheckContradictions()
        {
            var isInvalid = new bool[Rule.Height, Rule.Width];
            var valueCount = new int[Rule.Digits];

            HasContradictions = false;
            for (int r = 0; r < Rule.Height; r++)
                for (int c = 0; c < Rule.Width; c++)
                    if (Rule.IsAvailable[r, c])
                        Cells[r, c].redCircle.Visibility = Visibility.Hidden;

            foreach (var region in Rule.Regions)
            {
                for (int i = 0; i < Rule.Digits; i++)
                    valueCount[i] = 0;

                foreach (var cell in region.Cells)
                {
                    if (Game.Value(cell) != -1)
                        valueCount[Game.Value(cell)]++;
                }

                for (int i = 0; i < Rule.Digits; i++)
                    if (valueCount[i] >= 2)
                    {
                        HasContradictions = true;
                        foreach (var cell in region.Cells)
                        {
                            if (Game.Value(cell) == i)
                                Cells[cell.Row, cell.Column].redCircle.Visibility = Visibility.Visible;
                        }
                    }
            }

            if (IsCandidatesFilled)
            {
                for (int r = 0; r < Rule.Height; r++)
                    for (int c = 0; c < Rule.Width; c++)
                        if (Rule.IsAvailable[r, c] && Game.Value(r, c) == -1)
                        {
                            bool flag = false;
                            for (int i = 0; i < Rule.Digits; i++)
                                if (Game.IsCandidate(r, c, i))
                                {
                                    flag = true;
                                    break;
                                }
                            if (!flag)
                                HasContradictions = true;
                        }
            }
        }

        private void OnUserOperation()
        {
            RemoveHints();
            RedoStack.Clear();
            UndoStack.Add(Game.Clone());

            if (UndoStack.Count <= CandidateFillingStep)
                CandidateFillingStep = -1;

            GameChanged(this, null);
        }

        private void OnCellMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (HasSelection)
                Cells[SelectedCell.Row, SelectedCell.Column].selectionRectangle.Visibility = Visibility.Hidden;

            var cell = (CellControl)sender;
            SelectedCell = new Cell(cell.Row, cell.Column);
            HasSelection = true;
            cell.selectionRectangle.Visibility = Visibility.Visible;

            Focus();
            e.Handled = true;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (HasSelection)
                Cells[SelectedCell.Row, SelectedCell.Column].selectionRectangle.Visibility = Visibility.Hidden;
            SelectedCell = Cell.None;
            HasSelection = false;
        }

        private void SetValue(Cell cell, int value)
        {
            if (Game.Value(cell) != value)
            {
                var game = Game.Clone();
                game.SetValue(cell, value);
                SetGame(game, true);
            }
        }

        private int ValueByKey(Key key)
        {
            switch (key)
            {
                case Key.D1:
                case Key.NumPad1:
                    return 0;
                case Key.D2:
                case Key.NumPad2:
                    return 1;
                case Key.D3:
                case Key.NumPad3:
                    return 2;
                case Key.D4:
                case Key.NumPad4:
                    return 3;
                case Key.D5:
                case Key.NumPad5:
                    return 4;
                case Key.D6:
                case Key.NumPad6:
                    return 5;
                case Key.D7:
                case Key.NumPad7:
                    return 6;
                case Key.D8:
                case Key.NumPad8:
                    return 7;
                case Key.D9:
                case Key.NumPad9:
                    return 8;
                case Key.D0:
                case Key.NumPad0:
                    return 9;
                case Key.Q:
                    return 10;
                case Key.W:
                    return 11;
                case Key.E:
                    return 12;
                case Key.R:
                    return 13;
                case Key.T:
                    return 14;
                case Key.Y:
                    return 15;
                case Key.U:
                    return 16;
                case Key.I:
                    return 17;
                case Key.O:
                    return 18;
                case Key.P:
                    return 19;
                case Key.A:
                    return 20;
                case Key.S:
                    return 21;
                case Key.D:
                    return 22;
                case Key.F:
                    return 23;
                case Key.G:
                    return 24;
                case Key.H:
                    return 25;
                case Key.J:
                    return 26;
                case Key.K:
                    return 27;
                case Key.L:
                    return 28;
                case Key.OemSemicolon:
                    return 29;
            }
            return -1;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!HasSelection) return;
            if (Puzzle.Value(SelectedCell) >= 0) return;

            if (e.Key == Key.Escape)
            {
                Cells[SelectedCell.Row, SelectedCell.Column].selectionRectangle.Visibility = Visibility.Hidden;
                HasSelection = false;
                return;
            }

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                Game.SetValue(SelectedCell, -1);
                SetGame(Game, true);
            }

            int value = ValueByKey(e.Key);
            if (value >= 0 && value < Rule.Digits)
            {
                if (IsCreationMode)
                    SetValue(SelectedCell, value);
                else
                {
                    LastKeyDown = DateTime.UtcNow;

                    if (Game.Value(SelectedCell) >= 0)
                        SetValue(SelectedCell, value);
                    else
                        Task.Run(() =>
                        {
                            DateTime dt = LastKeyDown;
                            Thread.Sleep(180);

                            if (LastKeyDown == dt)
                                Dispatcher.Invoke(() =>
                                {
                                    SetValue(SelectedCell, value);
                                });
                        });
                }
                return;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (IsCreationMode)
                return;

            LastKeyDown = DateTime.MinValue;

            if (!HasSelection) return;
            if (Game.Value(SelectedCell) >= 0) return;

            int value = ValueByKey(e.Key);
            if (value == -1 || value >= Rule.Digits) return;

            Game.SetCandidate(SelectedCell, value, !Game.IsCandidate(SelectedCell, value));
            DrawCandidates(SelectedCell.Row, SelectedCell.Column);
        }

        // overrides default rendering behavior
        protected override void OnRender(DrawingContext dc)
        {
            dc.PushTransform(new TranslateTransform(ThickWidth / 2, ThickWidth / 2));
            dc.DrawDrawing(GridDrawing);

            if (SelectedCell != Cell.None)
            {
                dc.DrawRectangle(AppResources.HighlightBrush, null,
                    new Rect(SelectedCell.Column * CellSize, SelectedCell.Row * CellSize, CellSize, CellSize));
            }

            base.OnRender(dc);
        }
    }
}
