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
        private Uri _sessionPage;        
        private bool _isSessionExist;
        private bool _isSessionStarted;
        private bool _isSessionPaussed;
        private Thread _session;
        private Thread _pause;
        private Timer _sessionTimer;

        private TimeSpan _pauseTime;
        public string PauseTime
        {
            get { return _pauseTime.ToString(); }
        }

        private Nullable<DateTime> _startTime;
        public string StartTime
        {
            get
            {
                if (_startTime == null) return "--:--";
                else return ((DateTime)_startTime).ToString("HH:mm");
            }
        }

        private Nullable<DateTime> _endTime;
        public string EndTime
        {
            get
            {
                if (_endTime == null) return "--:--";
                else return ((DateTime)_endTime).ToString("HH:mm");
            }
        }

        private TimeSpan _sessionTime;
        public string SessionTime
        {
            get { return _sessionTime.ToString(); }
        }

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
            _sessionPage = new Uri("SessionPage.xaml", UriKind.Relative);
            _isSessionExist = false;
            _isSessionStarted = false;
            _isSessionPaussed = false;
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

        private void OpenSchedulePage()
        {
            //TODO
        }

        private void OpenClientInfoPage()
        {
            //TODO
        }

        private void UpdateNewSessionParametrs()
        {
            _sessionTime = new TimeSpan(0, 2, 30, 0);
            _pauseTime = new TimeSpan(0, 1, 10, 0);
            _session = new Thread(SessionTick);
            _session.IsBackground = true;
            _pause = new Thread(PauseTick);
            _pause.IsBackground = true;
            _isSessionExist = true;
            _startTime = null;
            _endTime = null;
            
        }

        private void OpenSessionPage()
        {
            if(!_isSessionExist)
            {
                //TODO
                UpdateNewSessionParametrs();
            }
            _navigationService?.Navigate(_sessionPage);
        }

        private void OpenMainMenuPage()
        {
            _navigationService?.Navigate(new Uri("MainMenuPage.xaml", UriKind.Relative));
        }

        private void OpenHistoryPage()
        {
            //TODO
        }

        private bool IsTimeOver(TimeSpan time)
        {
            return (time.Hours == 0) && (time.Minutes == 0) && (time.Seconds == 0);
        }

        private void StartSession()
        {
            if((!_isSessionStarted)&&(_isSessionExist))
            {
                _session.Start();
                _startTime = DateTime.Now;
                DoPropertyChanged("StartTime");
                _isSessionStarted = true;
            }
            else if(_isSessionPaussed)
            {
                _isSessionPaussed = false;
                _session.Start();
                _pause.Abort();
            }
        }

        private void StopSession()
        {
            if(_isSessionStarted)
            {
                _isSessionStarted = false;
                _isSessionExist = false;
                _endTime = DateTime.Now;
                DoPropertyChanged("EndTime");
            }
        }

        private void PauseSession()
        {
            if((!_isSessionPaussed)&&(IsTimeOver(_pauseTime)))
            {
                _session.Abort();
                _isSessionPaussed = true;
                _pause.Start();
            }
        }

        public void ClockTick()
        {
            while (true)
            Application.Current.Dispatcher.Invoke(() =>
            {
                _clock = DateTime.Now.ToString("HH:mm");
                DoPropertyChanged("Clock");
            });
        }

        public void SessionTick()
        {
            _sessionTimer = new Timer(NewSessionTick, null, 10, 1000); //Тут нужно получить время сессии
            //
            //    Application.Current.Dispatcher.Invoke(() =>
            //    {
            //        _sessionTime -= new TimeSpan(0, 0, 0, 1);
            //        DoPropertyChanged("SessionTime");
            //    });
        }

        private void NewSessionTick(object sender)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _sessionTime -= new TimeSpan(0, 0, 0, 1);
                DoPropertyChanged("SessionTime");
            });
        }

        public void PauseTick()
        {
            while (!IsTimeOver(_pauseTime))
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _pauseTime.Add(-new TimeSpan(0,0,0,1));
                    DoPropertyChanged("PauseTime");
                });
        }

        private ICommand _doOpenSchedulePage;

        public ICommand DoOpenSchedulePage
        {
            get
            {
                if (_doOpenSchedulePage == null)
                {
                    _doOpenSchedulePage = new Command(
                        p => true,
                        p => OpenSchedulePage());
                }
                return _doOpenSchedulePage;
            }
        }

        private ICommand _doOpenClientInfoPage;

        public ICommand DoOpenClientInfoPage
        {
            get
            {
                if (_doOpenClientInfoPage == null)
                {
                    _doOpenClientInfoPage = new Command(
                        p => true,
                        p => OpenClientInfoPage());
                }
                return _doOpenClientInfoPage;
            }
        }

        private ICommand _doOpenSessionPage;

        public ICommand DoOpenSessionPage
        {
            get
            {
                if (_doOpenSessionPage == null)
                {
                    _doOpenSessionPage = new Command(
                        p => true,
                        p => OpenSessionPage());
                }
                return _doOpenSessionPage;
            }
        }

        private ICommand _doOpenHistoryPage;

        public ICommand DoOpenHistoryPage
        {
            get
            {
                if (_doOpenHistoryPage == null)
                {
                    _doOpenHistoryPage = new Command(
                        p => true,
                        p => OpenHistoryPage());
                }
                return _doOpenHistoryPage;
            }
        }

        private ICommand _doOpenMainMenuPage;

        public ICommand DoOpenMainMenuPage
        {
            get
            {
                if (_doOpenMainMenuPage == null)
                {
                    _doOpenMainMenuPage = new Command(
                        p => true,
                        p => OpenMainMenuPage());
                }
                return _doOpenMainMenuPage;
            }
        }

        private ICommand _doStartSession;

        public ICommand DoStartSession
        {
            get
            {
                if (_doStartSession == null)
                {
                    _doStartSession = new Command(
                        p => true,
                        p => StartSession());
                }
                return _doStartSession;
            }
        }

        private ICommand _doPauseSession;

        public ICommand DoPauseSession
        {
            get
            {
                if (_doPauseSession == null)
                {
                    _doPauseSession = new Command(
                        p => true,
                        p => PauseSession());
                }
                return _doPauseSession;
            }
        }

        private ICommand _doStopSession;

        public ICommand DoStopSession
        {
            get
            {
                if (_doStopSession == null)
                {
                    _doStopSession = new Command(
                        p => true,
                        p => StopSession());
                }
                return _doStopSession;
            }
        }
    }
}
