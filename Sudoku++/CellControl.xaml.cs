using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Sudoku
{
    /// <summary>
    /// Interaction logic for CellControl.xaml
    /// </summary>
    public partial class CellControl : UserControl
    {
        public int Row { get; }
        public int Column { get; }
        public int CandidateCount { get; }

        public Ellipse[] Ellipses { get; }
        public Grid[] Candidates { get; }

        public CellControl(int row, int column, int candidateCount)
        {
            InitializeComponent();

            Row = row;
            Column = column;
            CandidateCount = candidateCount;

            highlightRectangle.Fill = AppResources.HighlightBrush;
            selectionRectangle.Fill = AppResources.HighlightBrush;
            redCircle.Stroke = AppResources.ArrowBrush;

            Ellipses = new Ellipse[candidateCount];
            Candidates = new Grid[candidateCount];

            int k = (int)Math.Sqrt(candidateCount - 1) + 1;
            for (int r = 0; r < k; r++)
                for (int c = 0; c < k && r * k + c < candidateCount; c++)
                {
                    var grid = new Grid()
                    {
                        Width = 90.0 / k,
                        Height = 90.0 / k,
                        Margin = new Thickness(5 + c * 90.0 / k, 5 + r * 90.0 / k, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Visibility = Visibility.Hidden
                    };

                    candidatesGrid.Children.Add(grid);

                    var ell = new Ellipse();
                    ell.Visibility = Visibility.Hidden;
                    grid.Children.Add(ell);

                    var candidateText = new TextBlock
                    {
                        Foreground = AppResources.TextBrush,
                        FontSize = 60.0 / k,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Text = Convert.ToString(r * k + c + 1)
                    };
                    grid.Children.Add(candidateText);

                    Ellipses[r * k + c] = ell;
                    Candidates[r * k + c] = grid;
                }
        }

        internal void SetValue(int value, bool isGiven)
        {
            valueText.Text = value == -1 ? string.Empty : (value + 1).ToString();
            valueText.Foreground = isGiven ? AppResources.TextBrush : AppResources.ValueBrush;
            valueText.FontWeight = isGiven ? FontWeights.Bold : FontWeights.Normal;

            if (value != -1)
                ClearCandidates();
        }

        internal void SetCandidates(Game game)
        {
            valueText.Text = string.Empty;
            for (int i = 0; i < CandidateCount; i++)
                Candidates[i].Visibility = game.IsCandidate(Row, Column, i) ? Visibility.Visible : Visibility.Hidden;
        }

        private void ClearCandidates()
        {
            foreach (var grid in Candidates)
                grid.Visibility = Visibility.Hidden;
        }
    }
}