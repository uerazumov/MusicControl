using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MusicControl
{
    /// <summary>
    /// Логика взаимодействия для SessionPage.xaml
    /// </summary>
    public partial class SessionPage : Page
    {
        public SessionPage()
        {
            InitializeComponent();
            SessionsList.SelectedIndex = 0;
            MainWindow window = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(w => w.IsActive);
            //StopButton.IsButtonEnabled = window.GetVM().StopIsEnabled;
            //PauseButton.IsButtonEnabled = window.GetVM().PauseIsEnabled;
            //StartButton.IsButtonEnabled = window.GetVM().StartIsEnabled;
            //SessionsList.ItemsSource = window.GetVM().Sessions;
        }
    }
}
