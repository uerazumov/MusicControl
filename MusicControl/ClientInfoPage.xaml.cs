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
    /// Логика взаимодействия для ClientInfoPage.xaml
    /// </summary>
    public partial class ClientInfoPage : Page
    {
        private MainWindow _window;
        public ClientInfoPage()
        {
            InitializeComponent();
            _window = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(w => w.IsActive);
        }

        private void SetClientNameTextBox(object sender, RoutedEventArgs e)
        {
            var textBoxes = new List<TextBox>();
            textBoxes.Add(AddNewClientTimeBalanceHoursTextBox);
            textBoxes.Add(AddNewClientTimeBalanceMinutesTextBox);
            textBoxes.Add(AddNewClientUnpaidTimeHoursTextBox);
            textBoxes.Add(AddNewClientUnpaidTimeMinutesTextBox);
            _window.SetClientTextBoxesToViewModel(AddNewClientTextBox, textBoxes);
        }
    }
}
