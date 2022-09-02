using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using TetrisClient.rectangle;
using TetrisClient.score;

namespace TetrisClient.windows
{
    public partial class MultiplayerWindow : Window
    {
        private HubConnection _connection;
        private Random _p1Random;
        private static TetrisEngine _engine = new();
        private CustomRectangle[,] _enemyBoard;
        private Matrix firstMatrix;
        private Matrix nextMat;
        private Score score;


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

        public MultiplayerWindow()
        {
            InitializeComponent();

            // De url waar de meegeleverde TetrisHub op draait:
            string url = "http://127.0.0.1:5000/TetrisHub";

            // De Builder waarmee de connectie aangemaakt wordt:
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();
            
            
            initMethods();

            // De eerste paramater moet gelijk zijn met de methodenaam in TetrisHub.cs
            // Wat er tussen de <..> staat bepaald wat de type van de paramater `seed` is.
            // Op deze manier loopt het onderstaande gelijk met de methode in TetrisHub.cs.
            _connection.On<int>("ReadyUp", seed =>
            {
                // Seed van de andere client:
                MessageBox.Show(seed.ToString());
            });
            

            // Let op: het starten van de connectie moet *nadat* alle event listeners zijn gezet!
            // Als de methode waarin dit voorkomt al `async` (asynchroon) is, dan kan `Task.Run` weggehaald worden.
            // In het startersproject staat dit in de constructor, daarom is dit echter wel nodig:
            
            Task.Run(async () => await _connection.StartAsync());
            
        }

        // Events kunnen `async` zijn in WPF:
        private async void StartGame_OnClick(object sender, RoutedEventArgs e)
        {
            // Als de connectie nog niet is geïnitialiseerd, dan kan er nog niks verstuurd worden:
            if (_connection.State != HubConnectionState.Connected)
            {
                return;
            }

            int seed = Guid.NewGuid().GetHashCode();

            _p1Random = new Random(seed);

            // Het aanroepen van de TetrisHub.cs methode `ReadyUp`.
            // Hier geven we de int mee die de methode `ReadyUp` verwacht.
            await _connection.InvokeAsync("ReadyUp", seed);
        }
        
        /// <summary>
        /// Sets all connections with server
        /// First param calls method from TetrisHub.cs
        /// Second param sends the player seed
        /// </summary>
        private void initMethods()
        {
            
            _connection.On<int>("ReadyUp", seed =>
                Task.Run(async () => await _connection.InvokeAsync("StartGame", seed)));


            _connection.On<int>("StartGame", seed => Dispatcher.BeginInvoke(new Action(() =>
                StartGame(seed))));
            
            _connection.On<string>(
                "SendBoard", board => Dispatcher.BeginInvoke(
                    new Action(() => _enemyBoard = JsonConvert.DeserializeObject<CustomRectangle[,]>(board))));


            _connection.On<string>("SendMatrix",
                matrix => Dispatcher.BeginInvoke(new Action(() =>
                    firstMatrix = JsonConvert.DeserializeObject<Matrix>(matrix))));


            _connection.On<string>("SendNextMatrix",
                nextMatrix => Dispatcher.BeginInvoke(new Action(() =>
                    nextMat = JsonConvert.DeserializeObject<Matrix>(nextMatrix))));
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
                UpdateGame();
            }
        }

        /// <summary>
        /// Starts game and gives seed to the engine and makes new board for other player, starts gameloop
        /// </summary>
        public void StartGame(int seed)
        {
            ScoreBlock.Text = "Score: " + _score;
            if (StartWindow.GameState == GameState.Playing)
            {
                _engine.InitializeGame(seed);
                _enemyBoard = _engine.GetTetrisBoard();
                Gameloop();
            }
        }

        /// <summary>
        /// Draw function for each rectangle, takes new offsets and rotation to check in engine if its possible
        /// then draws into grid. Checks if any rows are full, if so draws board again based on tetrisboard in engine
        /// </summary>
        private void Draw(int offY,int offX, Matrix matrix, Panel grid)
        {
            if (_engine.GameState != GameState.Playing) return;
            
            NextMatrix.Source = _images[_engine.GetMatrixIndex(_engine.NextMatrix)];

            // if full rows are detected, change score based on deletedrows and redraws whole board to new representation
            if (_engine.RowFull())
            {
                _score += _engine.Score.CalculateScore(_engine.DeletedRows);
                ScoreBlock.Text = "Score: " + _score;

                grid.Children.Clear();
                foreach (CustomRectangle r in _engine.GetTetrisBoard())
                {
                    if (r != null)
                    {
                        Grid.SetRow(r.Rectangle, r.Y); // Zet de rij
                        Grid.SetColumn(r.Rectangle, r.X); // Zet de kolom
                        grid?.Children.Add(r.Rectangle);

                    }
                }
            }
            // checks if next position is possible
            _engine.CheckNextPosition(offY, offX, matrix);

            // removes current matrix from grid
            foreach (Rectangle r in _rectanglesToRemove)
            {
                grid.Children.Remove(r);
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
                        grid.Children.Add(r.Rectangle);
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
        /// Renders grid for both players
        /// </summary>
        private void RenderGrid(int offY, int offX, Matrix matrix)
        {
            Draw( offY,  offX, matrix, tetrisGrid);
            Draw( offY,  offX, matrix, TetrisGrid2);

        }

        /// <summary>
        /// Moves a block down for both players
        /// </summary>
        private async void BlockDown()
        {
            Draw(1, 0, _engine.CurrentMatrix, TetrisGrid2);
            Draw(1, 0, _engine.CurrentMatrix, tetrisGrid);

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
        
        /// <summary>
        /// Resets engine, score and screen after play again button is clicked
        /// </summary>
        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Keypresses for left, right, down and rotations
        /// </summary>
        private void TetrisGrid_KeyDown(object sender, KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.Down:
                    RenderGrid(1, 0, _engine.CurrentMatrix);
                    break;
                case Key.Left:
                    RenderGrid(0, -1, _engine.CurrentMatrix);
                    break;
                case Key.Right:
                    RenderGrid(0, 1, _engine.CurrentMatrix);
                    break;
                case Key.Z:
                    RenderGrid(0, 0, _engine.CurrentMatrix.Rotate90CounterClockwise());
                    break;
                case Key.X:
                    RenderGrid(0, 0, _engine.CurrentMatrix.Rotate90());
                    break;
            }
            UpdateGame();
        }

        /// <summary>
        /// Sends data to server for other player to receive
        /// </summary>
        private void SendData()
        {
  
            Task.Run(async () =>
                await _connection.InvokeAsync("score", JsonConvert.SerializeObject(_engine.Score)));
            Task.Run(async () =>
                await _connection.InvokeAsync("tetris_board", JsonConvert.SerializeObject(_engine.GetTetrisBoard())));
            Task.Run(async () =>
                await _connection.InvokeAsync("Matrix", JsonConvert.SerializeObject(_engine.CurrentMatrix)));
            Task.Run(async () =>
                await _connection.InvokeAsync("nextMatrix", JsonConvert.SerializeObject(_engine.NextMatrix)));
        }
        

        private void UpdateGame()
        {
            SendData();
        }
       
    }
}

