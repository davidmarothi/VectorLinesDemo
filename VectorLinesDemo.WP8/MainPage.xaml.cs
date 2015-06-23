using MonoGame.Framework;
using VectorLinesDemo.Shared;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace VectorLinesDemo.WP8
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
        }

        public MainPage(string launchArguments)
        {
            _game = XamlGame<Game1>.Create(launchArguments, Window.Current.CoreWindow, this);
        }
    }
}
