using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TetrisClient.rectangle;

namespace TetrisClient.windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private  TetrisEngine _engine;
        private readonly List<Rectangle> _rectanglesToRemove = new List<Rectangle>();
        private int _score;
        
        // Images for nextMatrix, Order is important!
        private readonly ImageSource[] _images = new ImageSource[]
        {
            new BitmapImage(new Uri(@"/../resources/Tetris_L.png", UriKind.Relative)),
            new BitmapImage(new Uri(@"/../resources/Tetris_J.png", UriKind.Relative)),
            new BitmapImage(new Uri(@"/../resources/Tetris_Z.png", UriKind.Relative)),
            new BitmapImage(new Uri(@"/../resources/Tetris_S.png", UriKind.Relative)),
            new BitmapImage(new Uri(@"/../resources/Tetris_T.png", UriKind.Relative)),
            new BitmapImage(new Uri(@"/../resources/Tetris_O.png", UriKind.Relative)),
            new BitmapImage(new Uri(@"/../resources/Tetris_I.png", UriKind.Relative)),
        };
        
        /// <summary>
        /// At startup create new engine and call gameloop
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _engine = new TetrisEngine();
            ScoreBlock.Text = "Score: " + _score;
            if (StartWindow.GameState == GameState.Playing)
            {
                _engine.InitializeGame();
                Gameloop();
            }
        }
        
        /// <summary>
        /// While gameloop is playing, keep calling blockdown on timer
        /// </summary>
        private async Task Gameloop()
        {
            while (_engine.GameState == GameState.Playing)
            {
                await Task.Delay(500);
                BlockDown();
            }
        }
        
        /// <summary>
        /// Draw function for each rectangle, takes new offsets and rotation to check in engine if its possible
        /// then draws into grid. Checks if any rows are full, if so draws board again based on tetrisboard in engine
        /// </summary>
        private void Draw(int offY,int offX, Matrix matrix)
        {
            if (_engine.GameState != GameState.Playing) return;
            
            NextMatrix.Source = _images[_engine.GetMatrixIndex(_engine.NextMatrix)];
            
            // if full rows are detected, change score based on deletedrows and redraws whole board to new representation
            if (_engine.RowFull()) 
            {
                _score += _engine.Score.CalculateScore(_engine.DeletedRows);
                ScoreBlock.Text = "Score: " + _score;

                TetrisGrid.Children.Clear();
                foreach (CustomRectangle r in _engine.GetTetrisBoard()) // get all rectangles from Tetrisboard in engine
                {
                    if (r != null)
                    {
                        Grid.SetRow(r.Rectangle, r.Y); // Zet de rij
                        Grid.SetColumn(r.Rectangle, r.X); // Zet de kolom
                        TetrisGrid.Children.Add(r.Rectangle);
                    }
                }
            }
            // checks if next position is possible
            _engine.CheckNextPosition(offY, offX, matrix);
            
            // removes current matrix from grid
            foreach (Rectangle r in _rectanglesToRemove)
            {
                TetrisGrid.Children.Remove(r);
            }
            
            // draws rectangles for new matrix position
            foreach (CustomRectangle r in _engine.GetRectanglesToDraw())
            {
                if(r.Y > 0)
                {
                    try
                    {
                        Grid.SetRow(r.Rectangle, r.Y); // Zet de rij
                        Grid.SetColumn(r.Rectangle, r.X); // Zet de kolom
                        TetrisGrid.Children.Add(r.Rectangle);
                        if (!_engine.Collision)
                        {
                            _rectanglesToRemove.Add(r.Rectangle);
                        }
                    }catch (ArgumentException exception)
                    {
                        _engine.GameState = GameState.GameOver;
                        StartMenu.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// Moves a block down by one
        /// </summary>
        private void BlockDown()
        {
            Draw(1, 0, _engine.CurrentMatrix);
        }

        /// <summary>
        /// Keypresses for left, right, down and rotations
        /// </summary>
        private void TetrisGrid_KeyDown(object sender, KeyEventArgs e)
        {
            
            switch (e.Key)
            {
                case Key.Down:
                    Draw(1, 0, _engine.CurrentMatrix);
                    break;
                case Key.Left:
                    Draw(0, -1, _engine.CurrentMatrix);
                    break;
                case Key.Right:
                    Draw(0, 1,_engine.CurrentMatrix);
                    break;
                case Key.Z:
                    Draw(0, 0, _engine.CurrentMatrix.Rotate90CounterClockwise());
                    break;
                case Key.X:
                    Draw(0, 0, _engine.CurrentMatrix.Rotate90());
                    break;
            }
        }

        /// <summary>
        /// Resets engine, score and screen after play again button is clicked
        /// </summary>
        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            _engine.GameState = GameState.Playing;
            _score = 0;
            ScoreBlock.Text = "Score: " + _score;
            TetrisGrid.Children.Clear();
            
            _engine = new TetrisEngine();
            _engine.InitializeGame();
            this.StartMenu.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Closes window and goes back to startmenu
        /// </summary>
        private void return_Click(object sender, RoutedEventArgs e)
        {
            StartWindow win2 = new StartWindow();
            win2.Show();
            Close();
        }
    }
}