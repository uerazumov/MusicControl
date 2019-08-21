using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Timers;
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
        private System.Timers.Timer _pauseTimer;
        private System.Timers.Timer _sessionTimer;
        private System.Timers.Timer _clockTimer;
        private DateTime _timerStartTime;
        private TimeSpan _sessionDuration;
        private TimeSpan _pauseDuration;
        private List<Session> _sessions;

        private int _selectedSession;
        public int SelectedSession
        {
            get { return _selectedSession; }
            set
            {
                if (!_isSessionStarted)
                {
                    UpdateNewSessionParametrs(value);
                    UpdateTime();
                    _selectedSession = value;
                }
            }
        }

        public System.Windows.Media.Brush SessionTimeColor
        {
            get
            {
                if (_sessionTime.Seconds < 0) return System.Windows.Media.Brushes.Red;
                else return System.Windows.Media.Brushes.Black;
            }
        }

        public bool ComboboxIsEnabled
        {
            get { return !_isSessionStarted; }
        }


        public Nullable<TimeSpan> _unpaidTime;
        public Visibility UnpaidTimeVisibility
        {
            get
            {
                if (_unpaidTime != null) return Visibility.Visible;
                else return Visibility.Hidden;
            }
        }

        public String UnpaidTime
        {
            get
            {
                if (_unpaidTime != null) return ((TimeSpan)_unpaidTime).Hours.ToString() + ":" + ((TimeSpan)_unpaidTime).Minutes.ToString();
                else return "--:--";
            }
        }

        public bool StartIsEnabled
        {
            get { return !_isSessionStarted || _isSessionPaussed; }
        }

        public bool StopIsEnabled
        {
            get { return _isSessionStarted; }
        }

        public bool PauseIsEnabled
        {
            get { return !_isSessionPaussed; }
        }

        public List<String> Sessions
        {
            get
            {
                var sessions = new List<String>();
                for(int i = 0; i < _sessions.Count; i++)
                {
                    sessions.Add(_sessions[i].Client.ClientName + " " + _sessions[i].StartSessionTime.ToString("HH:mm") + "-" + _sessions[i].EndSessionTime.ToString("HH:mm"));
                }
                return sessions;
            }
        }

        private TimeSpan _timeBalance;
        public String TimeBalance
        {
            get { return _timeBalance.Hours.ToString() + ":" + _timeBalance.Minutes.ToString(); }
        }
        private TimeSpan _pauseTime;
        public string PauseTime
        {
            get { return new DateTime(_pauseTime.Ticks).ToString("HH:mm:ss"); ; }
        }

        private DateTime? _startTime;
        public string StartTime
        {
            get
            {
                if (_startTime == null) return "--:--";
                else return ((DateTime)_startTime).ToString("HH:mm");
            }
        }

        private DateTime? _endTime;
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
            get
            {
                if(_sessionTime.Ticks >= 0) return new DateTime(_sessionTime.Ticks).ToString("HH:mm:ss");
                else
                {
                    return "-" + new DateTime(-_sessionTime.Ticks).ToString("HH:mm:ss");
                }
            }

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
            _sessionPage = new Uri("SessionPage.xaml", UriKind.Relative);
            _isSessionExist = false;
            _isSessionStarted = false;
            _isSessionPaussed = false;
            DoPropertyChanged("PauseIsEnabled");
            DoPropertyChanged("ComboboxIsEnabled");
            DoPropertyChanged("StartIsEnabled");
            DoPropertyChanged("StopIsEnabled");
            _clockTimer = new System.Timers.Timer();
            _clockTimer.Elapsed += new ElapsedEventHandler(ClockTick);
            _clockTimer.Interval = 1000;
            _clockTimer.Start();
            ClockTick(new object(), new EventArgs());
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

        private void OpenSchedulePage()
        {
            //TODO
        }

        private void OpenClientInfoPage()
        {
            //TODO
        }

        private void UpdateNewSessionParametrs(int index)
        {
            _sessionTime = _sessions[index].SessionDuration;
            _pauseTime = new TimeSpan(0, 1, 10, 0);
            _sessionDuration = _sessionTime;
            _pauseDuration = _pauseTime;
            _timeBalance = _sessions[index].Client.TimeBalance;
            _isSessionExist = true;
            _isSessionPaussed = false;
            _isSessionStarted = false;
            DoPropertyChanged("ComboboxIsEnabled");
            DoPropertyChanged("StartIsEnabled");
            DoPropertyChanged("StopIsEnabled");
            DoPropertyChanged("PauseIsEnabled");
            _startTime = null;
            _endTime = null;
            _unpaidTime = null;
            DoPropertyChanged("UnpaidTime");
            DoPropertyChanged("UnpaidTimeVisibility");
        }

        private void OpenSessionPage()
        {
            if(!_isSessionExist)
            {
                //TODO
                _sessions = new List<Session>();
                _sessions.Add(new Session(new TimeSpan(2, 0, 0), new Client(1, "Иванов Иван Иванович", new TimeSpan(3, 0, 0), new TimeSpan(0, 0, 0)), new DateTime(2019, 8, 20, 11, 30, 0), 1));
                _sessions.Add(new Session(new TimeSpan(3, 0, 0), new Client(2, "Петров Петр Петрович", new TimeSpan(2, 0, 0), new TimeSpan(0, 0, 0)), new DateTime(2019, 8, 21, 14, 00, 0), 2));
                _sessions.Add(new Session(new TimeSpan(4, 30, 0), new Client(3, "Семёнов Семён Семёнович", new TimeSpan(1, 30, 0), new TimeSpan(0, 0, 0)), new DateTime(2019, 8, 21, 21, 45, 0), 3));
                UpdateNewSessionParametrs(0);
                _sessionTimer = new System.Timers.Timer();
                _sessionTimer.Elapsed += new ElapsedEventHandler(SessionTick);
                _sessionTimer.Interval = 300;
                _pauseTimer = new System.Timers.Timer();
                _pauseTimer.Elapsed += new ElapsedEventHandler(PauseTick);
                _pauseTimer.Interval = 300;
            }
            _navigationService?.Navigate(_sessionPage);
            UpdateTime();
        }

        private void UpdateTime()
        {
            DoPropertyChanged("SelectedSession");
            DoPropertyChanged("TimeBalance");
            DoPropertyChanged("PauseTime");
            DoPropertyChanged("StartTime");
            DoPropertyChanged("EndTime");
            DoPropertyChanged("SessionTime");
        }

        private void AddTime()
        {
            if(_isSessionStarted)
            {
                if (_selectedSession <= _sessions.Count - 2)
                {
                    int i = 1;
                    while((_sessions[_selectedSession].Client.TimeBalance >= new TimeSpan(0, 15, 0)) && (_startTime + _sessions[_selectedSession].SessionDuration + (new TimeSpan(1, 10, 0) - _pauseTime) + new TimeSpan(0, i * 15, 0) < _sessions[_selectedSession + 1].StartSessionTime))
                    {
                        i++;
                        _sessionTime += new TimeSpan(0, 15, 0);
                        _sessionDuration += new TimeSpan(0, 15, 0);
                        _sessions[_selectedSession].Client.TimeBalance -= new TimeSpan(0, 15, 0);
                    }
                    _timeBalance = _sessions[_selectedSession].Client.TimeBalance;
                    DoPropertyChanged("SessionTime");
                    DoPropertyChanged("TimeBalance");
                }
                else
                {
                    _sessionTime += _sessions[_selectedSession].Client.TimeBalance;
                    _sessionDuration += _sessions[_selectedSession].Client.TimeBalance;
                    DoPropertyChanged("SessionTime");
                    _sessions[_selectedSession].Client.TimeBalance = new TimeSpan(0, 0, 0);
                    _timeBalance = _sessions[_selectedSession].Client.TimeBalance;
                    DoPropertyChanged("TimeBalance");
                }
            }
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
            if (_isSessionExist)
            {
                if (!_isSessionStarted)
                {
                    _timerStartTime = DateTime.Now;
                    _sessionTimer.Start();
                    _startTime = DateTime.Now;
                    DoPropertyChanged("StartTime");
                    _isSessionStarted = true;
                    DoPropertyChanged("StartIsEnabled");
                    DoPropertyChanged("StopIsEnabled");
                    DoPropertyChanged("ComboboxIsEnabled");
                    _sessionDuration = _sessionTime;
                }
                else if (_isSessionPaussed)
                {
                    _timerStartTime = DateTime.Now;
                    _isSessionPaussed = false;
                    DoPropertyChanged("PauseIsEnabled");
                    DoPropertyChanged("StartIsEnabled");
                    _sessionTimer.Start();
                    _pauseTimer.Stop();
                    _sessionDuration = _sessionTime;
                }
            }
        }

        private void UpdateStopSessionParametrs()
        {
            _isSessionStarted = false;
            _isSessionExist = false;
            _isSessionPaussed = false;
            DoPropertyChanged("PauseIsEnabled");
            DoPropertyChanged("ComboboxIsEnabled");
            DoPropertyChanged("StartIsEnabled");
            DoPropertyChanged("StopIsEnabled");
            _sessionTimer.Stop();
            _pauseTimer.Stop();
        }

        private void StopSession()
        {
            if(_isSessionStarted)
            {
                UpdateStopSessionParametrs();
                _endTime = DateTime.Now;
                DoPropertyChanged("EndTime");
                if (_sessionTime.Ticks >= 0)
                {
                    _timeBalance += new TimeSpan(_sessionTime.Hours, (_sessionTime.Minutes / 15) * 15, 0);
                    _sessionTime = new TimeSpan(0, 0, 0);
                    _sessions[_selectedSession].Client.TimeBalance = _timeBalance;
                    _sessions[_selectedSession].SessionDuration = _sessionTime;
                    DoPropertyChanged("TimeBalance");
                    DoPropertyChanged("SessionTime");
                }
                else
                {
                    int temp = 0;
                    if (_sessionTime.Minutes > 31) temp++;
                    _unpaidTime = new TimeSpan(Math.Abs(_sessionTime.Hours), (Math.Abs(_sessionTime.Minutes / 30) + temp) * 30, 0);
                    DoPropertyChanged("UnpaidTime");
                    DoPropertyChanged("UnpaidTimeVisibility");
                }
            }
        }

        private void PauseSession()
        {
            if((!_isSessionPaussed)&&(!IsTimeOver(_pauseTime)))
            {
                _pauseDuration = _pauseTime;
                _timerStartTime = DateTime.Now;
                _sessionTimer.Stop();
                _isSessionPaussed = true;
                DoPropertyChanged("PauseIsEnabled");
                DoPropertyChanged("StartIsEnabled");
                _pauseTimer.Start();
            }
        }

        private void ClockTick(object sender, EventArgs e)
        {
            _clock = DateTime.Now.ToString("HH:mm");
            DoPropertyChanged("Clock");
        }

        private void SessionTick(object sender, EventArgs e)
        {
            _sessionTime = (TimeSpan)(_timerStartTime + _sessionDuration - DateTime.Now);
            DoPropertyChanged("SessionTime");
            DoPropertyChanged("SessionTimeColor");
        }

        private void PauseTick(object sender, EventArgs e)
        {
            _pauseTime = (TimeSpan)(_timerStartTime + _pauseDuration - DateTime.Now);
            DoPropertyChanged("PauseTime");
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

        private ICommand _doAddTime;

        public ICommand DoAddTime
        {
            get
            {
                if (_doAddTime == null)
                {
                    _doAddTime = new Command(
                        p => true,
                        p => AddTime());
                }
                return _doAddTime;
            }
        }

        
    }
}
