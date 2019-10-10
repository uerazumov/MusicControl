using System;
using System.Windows;
using System.Windows.Controls;

namespace MusicControl
{
    public partial class MainWindow : Window
    {
        private readonly ViewModel _vm;
        public MainWindow()
        {
            _vm = (ViewModel)Application.Current.Resources["ViewModel"];
            InitializeComponent();
        }

        public ViewModel GetVM()
        { 
            return _vm;
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
