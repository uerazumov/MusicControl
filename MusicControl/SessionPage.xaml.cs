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
            //StopButton.IsButtonEnabled = window.GetStopButtonStatus();
            //PauseButton.IsButtonEnabled = window.GetPauseButtonStatus();
            //StartButton.IsButtonEnabled = window.GetStartButtonStatus();
            SessionsList.ItemsSource = window.GetSessions();
        }
    }
}
