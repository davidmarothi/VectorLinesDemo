using MonoGame.Framework;
using VectorLinesDemo.Shared;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VectorLinesDemo.Win8
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : SwapChainBackgroundPanel
    {
        readonly Game1 _game;

        public MainPage()
        {
            this.InitializeComponent();

            var launchArguments = string.Empty;
            _game = XamlGame<Game1>.Create(launchArguments, Window.Current.CoreWindow, this);
        }
    }
}
