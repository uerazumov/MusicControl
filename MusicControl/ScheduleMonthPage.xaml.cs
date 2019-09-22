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
using System.Windows.Threading;

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
