using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

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
            SessionsList.SelectedValue = "";
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
            _window.GetVM().ClientSessionsComboBox = SessionsList;
        }

        private void ClientListGotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)e.OriginalSource;
            tb.SelectionBrush = Brushes.AliceBlue;
            tb.CaretBrush = Brushes.Black;
        }

        private void TextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)e.OriginalSource;
            (new Thread(() => {
                Thread.Sleep(TimeSpan.FromSeconds(0.01));
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    tb.SelectAll();
                }));
            })).Start();
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)e.OriginalSource;
            tb.SelectionLength = 0;
        }

        void OnComboboxTextChanged(object sender, RoutedEventArgs e)
        {
            
            ClientsList.IsDropDownOpen = true;
            var tb = (TextBox)e.OriginalSource;
            if (ClientsList.Text == "") ClientsList.SelectedIndex = -1;
            tb.Select(tb.SelectionStart + tb.SelectionLength, 0);
            //if (tb.Text == String.Empty) SetEmptyClient();
            CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(ClientsList.ItemsSource);
            cv.Filter = s =>
                ((string)s).IndexOf(ClientsList.Text, StringComparison.CurrentCultureIgnoreCase) >= 0;
            if ((ClientsList.SelectedValue != null) && (_window.GetVM().Clients.Count != 0))
                if (_window.GetVM().Clients.First(x => x == ClientsList.SelectedValue.ToString()) != null)
                {
                    cv.Filter = s => ((string)s).Length >= 0;
                    _window.GetVM().SelectedClient = cv.IndexOf(ClientsList.SelectedValue);
                    ClientsList.SelectedIndex = -1;
                    tb.SelectionBrush = Brushes.Transparent;
                    tb.CaretBrush = Brushes.Transparent;
                    SessionsList.Focus();
                }
        }

        //void SetEmptyClient()
        //{
        //    ClientIDTextBox.Text = "-";
        //    FullHoursCountTextBox.Text = "--:--";
        //    HooursCountAtYearTextBox.Text = "--:--";
        //    SessionsList.SelectedIndex = -1;
        //    FactTimeTextBox.Text = "--:--";
        //    TimeBalanceTextBox.Text = "--:--";
        //    UnpaidTimeTextBox.Text = "--:--";
        //}
    }
}
