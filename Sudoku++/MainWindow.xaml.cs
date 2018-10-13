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
            Rules.AddStandardRegions(rule, 3, 3, new List<int> { 0, 3 }, new List<int> { 0, 3 }, true, true, new List<int> { 0, 3 });
            rule.EndInit();
            TestGame(Game.FromString(rule, new List<string> { ".1............6....8......44...........6.8...8............4..2...4........7......",".......9.6.8.............5..4..2..........9.1........795.............8.....9.2..." }));*/
            Rule rule = new Rule(33, 33, 9);
            Rules.AddStandardRegions(rule, 3, 3, new List<int> { 0, 0, 0, 6, 6, 12, 12, 12, 18, 18, 24, 24, 24 }, new List<int> { 0, 12, 24, 6, 18, 0, 12, 24, 6, 18, 0, 12, 24 }, true, false, null);
            rule.EndInit();
            TestGame(Game.FromString(rule, new List<string> { "2.....9......31.........8.793.2........6...5..8.4...7...3..9.......7......2..4...", "..4......327..........1.8.26.2.........5.9.........7.5...9.8.................7...", "2.7.1..........152...8.4....4....7......52.9..1.....3......3..1...6........9....8", "....9........7........5..........5.8351........6....................2......1.4...", "...83.................9....3.7.....96....1.........813...1.5.....3...............", "6..7.1......3.......8..........68..29...............78.4..9.....7.....4..5..3....", "....3............3....5..........2.5...3.8...9.3...6...6...5......9..........4.2.", "........6......1.....2.9...8.9....5........18....47.........93....5.3.........8..", "...1...6..4..........7.9.........9.6....1...78.5..........2........9.............", "....4........1.....2............48..287.............75...7.2...1...........4.....", "4....2...9.8........5..6.......6..5.8...4........7.2.6.7.5........1.7....1....8.5", "....1..........1......8............8...2.9...4.3...........1.7......4...267....5.", ".......8.......41......7...9.5..........74....7..8......3.....8...9..6.524.3....." }));
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
