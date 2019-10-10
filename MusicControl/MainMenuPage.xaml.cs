using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicControl
{
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        public void OnLoaded(object sender, RoutedEventArgs e)
        {
            MainWindow window = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(w => w.IsActive);
            window.SetNavigationService(this);
        }
    }
}
