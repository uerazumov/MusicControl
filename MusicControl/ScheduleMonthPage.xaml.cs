using System.Windows;
using System.Windows.Controls;

namespace MusicControl
{
    /// <summary>
    /// Логика взаимодействия для ScheduleMonthPage.xaml
    /// </summary>
    public partial class ScheduleMonthPage : Page
    {
        public ScheduleMonthPage()
        {
            InitializeComponent();
        }

        public void CloseCalendar(object sender, RoutedEventArgs e)
        {
            SessionCalendar.Visibility = Visibility.Hidden;
        }
    }
}
