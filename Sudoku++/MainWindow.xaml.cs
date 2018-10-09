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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Background = AppResources.BackgroundBrush;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            /*Rule rule = new Rule(12, 12, 9);
            Rules.AddStandardRegions(rule, 1, 9, new List<int> { 0, 3 }, new List<int> { 0, 3 }, true, false, null);
            rule.EndInit();
            TestGame(Game.FromString(rule, ".2....9........8..6.7....4.7........1.2......5.......9........9....3...8....83....6........9..3.71........4.......9.1.........3..7.1...4........"));*/
            Rule rule = new Rule(9, 9, 9);
            Rules.AddStandardRegions(rule, 3, 3, new List<int> { 0 }, new List<int> { 0 }, false, true, new List<int> { 0 });
            rule.EndInit();
            TestGame(Game.FromString(rule, "...9.5.........1.6.5.3.........3..6.3.6..........9..1......1...7.2.....8......7.."));
        }

        private void TestRule(Rule rule)
        {
            var game = Game.Generate(rule);
            Game puzzle = null;
            while (puzzle == null)
                puzzle = game.GeneratePuzzle(0, 1, false);
            Clipboard.SetDataObject(puzzle.ToString());
            var pc = new PlayingControl(puzzle, true);

            rootGrid.Children.Clear();
            rootGrid.Children.Add(pc);
        }

        private void TestGame(Game game)
        {
            var pc = new PlayingControl(game, true);

            rootGrid.Children.Clear();
            rootGrid.Children.Add(pc);
        }
    }
}
