using System.Windows.Navigation;

namespace MusicControl
{
    class ViewModel
    {
        private MainWindow _mainWindow;
        private NavigationService _navigationService;

        public ViewModel() { }
        public void AssignMainWindow(MainWindow mw)
        {
            _mainWindow = mw;
        }
    }
}
