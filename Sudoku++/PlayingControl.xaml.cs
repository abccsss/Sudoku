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
    /// Interaction logic for PlayingControl.xaml
    /// </summary>
    public partial class PlayingControl : UserControl
    {
        public GameControl GameControl { get; }
        private bool IsCompleted { get; set; }

        private Step _activeStep;
        private Step ActiveStep { get { return GameControl.Hint == null ? null : _activeStep; } set { _activeStep = value; } }
        private DateTime LastHintTime { get; set; }
        private const double HintInterval = AppResources.DevMode ? 0 : 60000;

        public PlayingControl(Game game, bool canSkip)
        {
            FontFamily = AppResources.FontFamily;
            Foreground = AppResources.TextBrush;
            if (!canSkip)
                SkipButton.Visibility = Visibility.Collapsed;

            InitializeComponent();

            GameControl = new GameControl(game.Rule);
            GameControl.SetPuzzle(game);
            GameControl.GameChanged += OnGameChanged;
            viewbox.Child = GameControl;
        }

        private void CheckIsCompleted()
        {
            bool isCompleted = true;
            if (GameControl.HasContradictions)
                isCompleted = false;
            else
            {
                Game game = GameControl.GetProperGame();

                for (int r = 0; r < game.Rule.Height; r++)
                    for (int c = 0; c < game.Rule.Width; c++)
                        if (game.Rule.IsAvailable[r, c] && game.Value(r, c) == -1)
                        {
                            isCompleted = false;
                            goto brk;
                        }
                brk:;
            }

            IsCompleted = isCompleted;
            if (IsCompleted)
                HintText.Text = "Congratulations! You have completed the puzzle.";
        }

        private void OnGameChanged(object sender, EventArgs e)
        {
            UndoButton.IsEnabled = GameControl.CanUndo;
            RedoButton.IsEnabled = GameControl.CanRedo;
            FillButton.IsEnabled = GameControl.CanFill;

            CheckIsCompleted();
        }

        private void OnUndoClick(object sender, RoutedEventArgs e)
        {
            if (GameControl.CanUndo)
            {
                GameControl.Undo();
                HintText.Text = string.Empty;
            }
        }

        private void OnRedoClock(object sender, RoutedEventArgs e)
        {
            if (GameControl.CanRedo)
            {
                GameControl.Redo();
                HintText.Text = string.Empty;
            }
        }

        private void OnFillClick(object sender, RoutedEventArgs e)
        {
            if (IsCompleted)
                return;

            if (GameControl.CanFill)
                GameControl.FillCandidates();
        }

        private void OnHintClick(object sender, RoutedEventArgs e)
        {
            if (IsCompleted)
                return;

            if (GameControl.CanFill)
                GameControl.FillCandidates();
            else if (ActiveStep == null)
            {
                HintText.Text = string.Empty;

                if ((DateTime.UtcNow - LastHintTime).TotalMilliseconds < HintInterval)
                {
                    HintText.Text = "You can only get 1 hint per minute.";
                }
                else if (!GameControl.HasContradictions)
                {
                    LastHintTime = DateTime.UtcNow;

                    var game = GameControl.GetProperGame();
                    Step step = Strategy.FindStep(game, Strategy.MaxDifficulty, true);

                    if (step == null)
                        HintText.Text = "No hints available.";
                    else
                    {
                        ActiveStep = step;
                        GameControl.SetHint(step.Frames[0]);
                        HintText.Text = step.Frames[0].Caption;
                    }
                }
            }
            else
            {
                GameControl.SetGame(ActiveStep.NewGame, true);
                ActiveStep = null;
            }
        }

        private void OnSkipClick(object sender, RoutedEventArgs e)
        {
            if (IsCompleted)
                return;

            if (GameControl.CanFill)
                GameControl.FillCandidates();
            else
            {
                HintText.Text = string.Empty;

                if (!GameControl.HasContradictions)
                {
                    var game = GameControl.GetProperGame();
                    var _game = Strategy.Solve(game, 1);

                    if (_game == null)
                        return;

                    GameControl.SetGame(game, true);
                }
            }
        }
    }
}
