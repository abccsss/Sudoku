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
            var rule = Rules.GetXRule(3, 3);
            //TestRule(rule);
            TestGame(Game.FromString(rule, ".4...5...1..8...2............2.34.89.........41.65.7............8...9..5...4...9."));
        }

        private void TestRule(Rule rule)
        {
            var game = Game.Generate(rule);
            Game puzzle = null;
            while (puzzle == null)
                puzzle = game.GeneratePuzzle(0, 1, true);
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
