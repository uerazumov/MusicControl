using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MusicControl
{
    public class ViewModel : INotifyPropertyChanged
    {
        private MainWindow _mainWindow;
        private NavigationService _navigationService;
        private string _clock;

        public string Clock
        {
            get { return _clock; }
            set
            {
                _clock = value;
                DoPropertyChanged("Clock");
            }
        }

        public ViewModel()
        {
            StartClock();
        }

        public void SetNavigationService(Object page)
        {
            _navigationService = NavigationService.GetNavigationService(page as MainMenuPage);
        }

        public void AssignMainWindow(MainWindow mw)
        {
            _mainWindow = mw;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void StartClock()
        {
            Thread ClockUpdate = new Thread(ClockTick);
            ClockUpdate.IsBackground = true;
            ClockUpdate.Start();
        }

        private void OpenSchedule()
        {
            Environment.Exit(0);
        }

        public void ClockTick()
        {
            while (true)
            Application.Current.Dispatcher.Invoke(() =>
            {
                _clock = DateTime.Now.ToString("HH:mm:ss");
                DoPropertyChanged("Clock");
            });
        }

        private ICommand _doOpenSchedule;

        public ICommand DoOpenSchedule
        {
            get
            {
                if (_doOpenSchedule == null)
                {
                    _doOpenSchedule = new Command(
                        p => true,
                        p => OpenSchedule());
                }
                return _doOpenSchedule;
            }
        }
    }
}
