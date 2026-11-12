using JollyGalaxian.Game;
using System.Windows;

namespace JollyGalaxian
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GalaxianGame? _game;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_game != null)
            {
                return;
            }

            await GameView.WaitForReadyAsync();

            _game = new GalaxianGame(GameView);
            _game.Input.Attach(this);
            _game.Start();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (_game != null)
            {
                _game.Input.Detach(this);
                _game.Stop();
                ((IDisposable)_game).Dispose();
                _game = null;
            }
        }
    }
}