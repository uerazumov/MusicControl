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
    public partial class MainWindow : Window
    {
        private readonly ViewModel _vm;
        public MainWindow()
        {
            _vm = (ViewModel)Application.Current.Resources["ViewModel"];
            InitializeComponent();
            Uri iconUri = new Uri("VisualResources\\icon.ico", UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }

        private void Close(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        public void SetNavigationService(Page page)
        {
            _vm.SetNavigationService(page);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _vm.AssignMainWindow(this);
            //Main.NavigationService.Navigated += (obj, args) => { Main.NavigationService.RemoveBackEntry(); };
        }
    }
}
