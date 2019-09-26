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
            _window.GetVM().SetClientTextBoxes(AddNewClientTextBox, textBoxes);
            _window.GetVM().ClientsComboBox = ClientsList;
        }

        private void ClientListGotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)e.OriginalSource;
            tb.SelectionBrush = Brushes.AliceBlue;
            tb.CaretBrush = Brushes.Black;
        }

        void OnComboboxTextChanged(object sender, RoutedEventArgs e)
        {
            ClientsList.IsDropDownOpen = true;
            var tb = (TextBox)e.OriginalSource;
            if (ClientsList.Text == "") ClientsList.SelectedIndex = -1;
            tb.Select(tb.SelectionStart + tb.SelectionLength, 0);
            CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(ClientsList.ItemsSource);
            cv.Filter = s =>
                ((string)s).IndexOf(ClientsList.Text, StringComparison.CurrentCultureIgnoreCase) >= 0;
            if (ClientsList.SelectedValue != null)
                if (_window.GetVM().Clients.First(x => x == ClientsList.SelectedValue.ToString()) != null)
                {
                    //Придумать, как решить проблему при пользователях с одинаковым именем
                    _window.GetVM().SelectedClient = _window.GetVM().Clients.IndexOf(_window.GetVM().Clients.First(x => x == ClientsList.SelectedValue.ToString()));
                    ClientsList.SelectedIndex = -1;
                    tb.SelectionBrush = Brushes.Transparent;
                    tb.CaretBrush = Brushes.Transparent;
                    SessionsList.Focus();
                }
        }
    }
}
