using System.Windows;

namespace TetrisClient.windows
{
    public partial class StartWindow : Window
    {
        public static GameState GameState {  get; private set; }
        public StartWindow()
        {
            InitializeComponent();
        }
        
        
        private void startGame_Click(object sender, RoutedEventArgs e)
        {
            GameState = GameState.Playing;
            MainWindow win2 = new MainWindow();
            win2.Show();
            this.Close();
        }

        private void multiplayer_Click(object sender, RoutedEventArgs e)
        {
            GameState = GameState.Playing;
            MultiplayerWindow win2 = new MultiplayerWindow();
            win2.Show();
            this.Close();
        }
    }
}