using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Linq;

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
        private List<Session> _todaysSessions;
        private List<Client> _clients;

        public int ClientID
        {
            get { return _clients[_selectedClient].ClientID; }
        }

        public List<String> Clients
        {
            get
            {
                var clients = new List<String>();
                for (int i = 0; i < _clients.Count; i++)
                {
                    clients.Add(_clients[i].ClientName);
                }
                return clients;
            }
        }

        public List<String> ClientSessions
        {
            get
            {
                var clientSessions = new List<String>();
                for (int i = 0; i < _clients[_selectedClient].Sessions.Count; i++)
                {
                    clientSessions.Add(_clients[_selectedClient].Sessions[i].StartSessionTime.ToString("yyyy/MM/dd/ HH:mm") + "-" + (_clients[_selectedClient].Sessions[i].StartSessionTime + _clients[_selectedClient].Sessions[i].SessionDuration).ToString("HH:mm"));
                }
                return clientSessions;
            }
        }

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

        private int _selectedClientSession;
        public int SelectedClientSession
        {
            get { return _selectedClientSession; }
            set
            {
                _selectedClientSession = value;
                DoPropertyChanged("SelectedClientSession");
                DoPropertyChanged("UnpaidTime");
                DoPropertyChanged("CurrentClientSessionTime");
            }
        }

        private int _selectedClient;
        public int SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                DoPropertyChanged("ClientID");
                DoPropertyChanged("TotalHours");
                DoPropertyChanged("TotalHoursPerYear");
                DoPropertyChanged("ClientTimeBalance");
                DoPropertyChanged("ClientSessions");
                SelectedClientSession = 0;
            }
        }

        public String CurrentClientSessionTime
        {
            get
            {
                if ((_clients[_selectedClient].Sessions.Count != 0) && (_selectedClientSession >= 0))
                {
                    if (_clients[_selectedClient].Sessions[_selectedClientSession].CurrentDuration.Minutes != 0) return _clients[_selectedClient].Sessions[_selectedClientSession].CurrentDuration.Hours.ToString() + "ч. " + _clients[_selectedClient].Sessions[_selectedClientSession].CurrentDuration.Minutes.ToString() + "мин.";
                    else return _clients[_selectedClient].Sessions[_selectedClientSession].CurrentDuration.Hours.ToString() + "ч. ";
                }
                else return "--:--";
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
            get { return !_isSessionStarted && (_todaysSessions.Count != 0); }
        }

        public string TotalHours
        {
            get
            {
                var time = new TimeSpan(0);
                for (int i = 0; i < _clients[_selectedClient].Sessions.Count; i++)
                {
                    time += _clients[_selectedClient].Sessions[i].CurrentDuration;
                }
                if (time.Minutes != 0) return time.Hours.ToString() + "ч. " + time.Minutes.ToString() + "мин.";
                else return time.Hours.ToString() + "ч. ";
            }
        }

        public String ClientTimeBalance
        {
            get
            {
                if (_clients[_selectedClient].TimeBalance.Minutes != 0) return _clients[_selectedClient].TimeBalance.Hours.ToString() + "ч. " + _clients[_selectedClient].TimeBalance.Minutes.ToString() + "мин.";
                else return _clients[_selectedClient].TimeBalance.Hours.ToString() + "ч. ";
            }
        }

        public string TotalHoursPerYear
        {
            get
            {
                var time = new TimeSpan(0);
                for (int i = 0; i < _clients[_selectedClient].Sessions.Count; i++)
                {
                    if (_clients[_selectedClient].Sessions[i].StartSessionTime > DateTime.Now - new TimeSpan(365,0,0,0))
                    time += _clients[_selectedClient].Sessions[i].CurrentDuration;
                }
                if (time.Minutes != 0) return time.Hours.ToString() + "ч. " + time.Minutes.ToString() + "мин.";
                else return time.Hours.ToString() + "ч. ";
            }
        }

        public bool AddTimeIsEnabled
        {
            get { return _isSessionStarted && (_todaysSessions.Count != 0); }
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

        private bool _addBoxVisibility;

        public Visibility AddBoxVisibility
        {
            get
            {
                if (_addBoxVisibility) return Visibility.Visible;
                else return Visibility.Hidden;
            }
        }

        public Visibility AddButtonVisibility
        {
            get
            {
                if (!_addBoxVisibility) return Visibility.Visible;
                else return Visibility.Hidden;
            }
        }

        public bool ClientInfoIsEnabled
        {
            get
            {
                return !_addBoxVisibility;
            }
        }

        public String UnpaidTime
        {
            get
            {
                if (_clients[_selectedClient].UnpaidTime.Ticks != 0)
                {
                    if (_clients[_selectedClient].UnpaidTime.Minutes != 0) return _clients[_selectedClient].UnpaidTime.Hours.ToString() + "ч. " + _clients[_selectedClient].UnpaidTime.Minutes.ToString() + "мин.";
                    else return _clients[_selectedClient].UnpaidTime.Hours.ToString() + "ч. ";
                }
                else return "--:--";
            }
        }

        public String ClientUnpaidTime
        {
            get
            {
                if (_unpaidTime != null) return ((TimeSpan)_unpaidTime).Hours.ToString() + ":" + ((TimeSpan)_unpaidTime).Minutes.ToString();
                else return "--:--";
            }
        }

        public bool StartIsEnabled
        {
            get { return (!_isSessionStarted || _isSessionPaussed) && (_todaysSessions.Count != 0); }
        }

        public bool StopIsEnabled
        {
            get { return _isSessionStarted; }
        }

        public bool UpdateIsEnabled
        {
            get { return !_isSessionStarted; }
        }

        public bool PauseIsEnabled
        {
            get { return _isSessionStarted && !_isSessionPaussed; }
        }

        public List<String> Sessions
        {
            get
            {
                var sessions = new List<String>();
                if (_todaysSessions.Count != 0) for (int i = 0; i < _todaysSessions.Count; i++)
                {
                    sessions.Add(_clients.First(x => x.ClientID == _todaysSessions[i].ClientID).ClientName + " " + _todaysSessions[i].StartSessionTime.ToString("HH:mm") + "-" + _todaysSessions[i].EndSessionTime.ToString("HH:mm"));
                }
                return sessions;
            }
        }

        private TimeSpan _timeBalance;
        public String TimeBalance
        {
            get
            {
                if (_timeBalance.Minutes != 0) return _timeBalance.Hours.ToString() + ":" + _timeBalance.Minutes.ToString();
                else return _timeBalance.Hours.ToString() + "ч. ";
            }
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
            DoPropertyChanged("AddTimeIsEnabled");
            DoPropertyChanged("StartIsEnabled");
            DoPropertyChanged("StopIsEnabled");
            DoPropertyChanged("UpdateIsEnabled");
            _clockTimer = new System.Timers.Timer();
            _clockTimer.Elapsed += new ElapsedEventHandler(ClockTick);
            _clockTimer.Interval = 1000;
            _clockTimer.Start();
            ClockTick(new object(), new EventArgs());
            _todaysSessions = new List<Session>();

            _clients = new List<Client>();

            List<Session> sessions = new List<Session>();
            sessions = new List<Session>();
            sessions.Add(new Session(new TimeSpan(2, 0, 0), new DateTime(2019, 6, 23, 11, 30, 0), 1, 1, new TimeSpan(1, 30, 0)));
            sessions.Add(new Session(new TimeSpan(3, 0, 0), new DateTime(2019, 7, 21, 14, 00, 0), 2, 1, new TimeSpan(3, 0, 0)));
            sessions.Add(new Session(new TimeSpan(4, 30, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 21, 30, 0), 3, 1, new TimeSpan(0)));
            _clients.Add(new Client(1, "Иванов Иван Иванович", new TimeSpan(2, 0, 0), new TimeSpan(0), sessions));

            sessions = new List<Session>();
            sessions.Add(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 6, 22, 11, 30, 0), 4, 2, new TimeSpan(1, 0, 0)));
            sessions.Add(new Session(new TimeSpan(3, 30, 0), new DateTime(2019, 7, 19, 14, 00, 0), 5, 2, new TimeSpan(3, 30, 0)));
            sessions.Add(new Session(new TimeSpan(3, 30, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 11, 30, 0), 6, 2, new TimeSpan(0)));
            _clients.Add(new Client(2, "Петров Петр Иванович", new TimeSpan(3, 0, 0), new TimeSpan(0), sessions));

            sessions = new List<Session>();
            sessions.Add(new Session(new TimeSpan(2, 0, 0), new DateTime(2019, 6, 3, 11, 30, 0), 7, 3, new TimeSpan(2, 30, 0)));
            sessions.Add(new Session(new TimeSpan(1, 30, 0), new DateTime(2019, 7, 1, 14, 00, 0), 8, 3, new TimeSpan(1, 30, 0)));
            sessions.Add(new Session(new TimeSpan(1, 30, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 01, 30, 0), 9, 3, new TimeSpan(0)));
            sessions.Add(new Session(new TimeSpan(1, 30, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 16, 30, 0), 10, 3, new TimeSpan(0)));
            _clients.Add(new Client(3, "Семёнов Иван Григорьевич", new TimeSpan(0), new TimeSpan(0), sessions));

            sessions = new List<Session>();
            _clients.Add(new Client(3, "Семёнов Иван Григорьевич", new TimeSpan(0), new TimeSpan(0), sessions));

            sessions = new List<Session>();
            sessions.Add(new Session(new TimeSpan(2, 30, 0), new DateTime(2019, 6, 4, 11, 30, 0), 11, 4, new TimeSpan(2, 30, 0)));
            sessions.Add(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 7, 2, 14, 00, 0), 12, 4, new TimeSpan(1, 30, 0)));
            sessions.Add(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 8, 23, 16, 30, 0), 13, 4, new TimeSpan(1, 0, 0)));
            sessions.Add(new Session(new TimeSpan(1, 0, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 16, 30, 0), 14, 4, new TimeSpan(0)));
            _clients.Add(new Client(4, "Караванов Генадий Иванович", new TimeSpan(5, 30, 0), new TimeSpan(0), sessions));

            sessions = new List<Session>();
            sessions.Add(new Session(new TimeSpan(2, 30, 0), new DateTime(2018, 6, 5, 11, 30, 0), 15, 5, new TimeSpan(1, 30, 0)));
            sessions.Add(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 7, 6, 14, 00, 0), 16, 5, new TimeSpan(1, 30, 0)));
            _clients.Add(new Client(5, "Сидоров Сергей Сергеевич", new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0), sessions));

            sessions = new List<Session>();
            sessions.Add(new Session(new TimeSpan(5, 30, 0), new DateTime(2018, 1, 5, 16, 30, 0), 16, 6, new TimeSpan(5, 30, 0)));
            _clients.Add(new Client(6, "Куликов Петр Петрович", new TimeSpan(0), new TimeSpan(0), sessions));

            sessions = new List<Session>();
            _clients.Add(new Client(7, "Григорьев Иван Анатольевич", new TimeSpan(1, 30, 0), new TimeSpan(0), sessions));
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
            _addBoxVisibility = false;
            DoPropertyChanged("AddBoxVisibility");
            DoPropertyChanged("AddButtonVisibility");
            _navigationService?.Navigate(new Uri("ClientInfoPage.xaml", UriKind.Relative));
        }

        private void UpdateTodaysSessions()
        {
            var todaysClients = _clients.FindAll(x => x.Sessions.FindAll(y => y.StartSessionTime.Date == DateTime.Now.Date).Count != 0);
            for (int i = 0; i < todaysClients.Count; i++)
            {
                var tempSessions = todaysClients[i].Sessions.FindAll(x => x.StartSessionTime.Date == DateTime.Now.Date);
                for (int j = 0; j < tempSessions.Count; j++)
                {
                    _todaysSessions.Add(tempSessions[j]);
                }
            }
        }

        private void UpdateNewSessionParametrs(int index)
        {
            UpdateTodaysSessions();
            if (_todaysSessions.Count != 0) _sessionTime = _todaysSessions[index].SessionDuration;
            else _sessionTime = new TimeSpan(0);
            _pauseTime = new TimeSpan(0, 1, 10, 0);
            _sessionDuration = _sessionTime;
            _pauseDuration = _pauseTime;
            if (_todaysSessions.Count != 0) _timeBalance = _clients.First(x => x.ClientID == _todaysSessions[index].ClientID).TimeBalance;
            else _timeBalance = new TimeSpan(0);
            _isSessionExist = true;
            _isSessionPaussed = false;
            _isSessionStarted = false;
            DoPropertyChanged("ComboboxIsEnabled");
            DoPropertyChanged("AddTimeIsEnabled");
            DoPropertyChanged("StartIsEnabled");
            DoPropertyChanged("StopIsEnabled");
            DoPropertyChanged("UpdateIsEnabled");
            DoPropertyChanged("PauseIsEnabled");
            _startTime = null;
            _endTime = null;
            _unpaidTime = null;
            DoPropertyChanged("UnpaidTime");
            DoPropertyChanged("UnpaidTimeVisibility");
        }

        private void AddNewClient()
        {
            _addBoxVisibility = true;
            DoPropertyChanged("ClientInfoIsEnabled");
            DoPropertyChanged("AddBoxVisibility");
            DoPropertyChanged("AddButtonVisibility");
        }

        private void ApplyNewClient()
        {
            _addBoxVisibility = false;
            DoPropertyChanged("ClientInfoIsEnabled");
            DoPropertyChanged("AddBoxVisibility");
            DoPropertyChanged("AddButtonVisibility");
        }

        private void Cancel()
        {
            _addBoxVisibility = false;
            DoPropertyChanged("ClientInfoIsEnabled");
            DoPropertyChanged("AddBoxVisibility");
            DoPropertyChanged("AddButtonVisibility");
        }

        private void EditClient()
        {
            //TODO
        }

        private void OpenSessionPage()
        {
            if(!_isSessionExist)
            {
                //TODO

                if (_todaysSessions.Count != 0) UpdateNewSessionParametrs(0);
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
            if ((_isSessionStarted)&&(_todaysSessions.Count != 0))
            {
                if (_selectedSession <= _todaysSessions.Count - 2)
                {
                    int i = 1;
                    while((_clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance >= new TimeSpan(0, 15, 0)) && (_startTime + _todaysSessions[_selectedSession].SessionDuration + (new TimeSpan(1, 10, 0) - _pauseTime) + new TimeSpan(0, i * 15, 0) < _todaysSessions[_selectedSession + 1].StartSessionTime))
                    {
                        i++;
                        _sessionTime += new TimeSpan(0, 15, 0);
                        _sessionDuration += new TimeSpan(0, 15, 0);
                        _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance -= new TimeSpan(0, 15, 0);
                    }
                    _timeBalance = _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance;
                    DoPropertyChanged("SessionTime");
                    DoPropertyChanged("TimeBalance");
                }
                else
                {
                    _sessionTime += _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance;
                    _sessionDuration += _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance;
                    DoPropertyChanged("SessionTime");
                    _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance = new TimeSpan(0, 0, 0);
                    _timeBalance = _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance;
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
                    DoPropertyChanged("UpdateIsEnabled");
                    DoPropertyChanged("PauseIsEnabled");
                    DoPropertyChanged("AddTimeIsEnabled");
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
            UpdateTodaysSessions();
            _isSessionStarted = false;
            _isSessionPaussed = false;
            DoPropertyChanged("PauseIsEnabled");
            DoPropertyChanged("ComboboxIsEnabled");
            DoPropertyChanged("AddTimeIsEnabled");
            DoPropertyChanged("StartIsEnabled");
            DoPropertyChanged("StopIsEnabled");
            DoPropertyChanged("UpdateIsEnabled");
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
                    _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance = _timeBalance;
                    DoPropertyChanged("TimeBalance");
                }
                else
                {
                    int temp = 0;
                    if (_sessionTime.Minutes > 31) temp++;
                    _unpaidTime = new TimeSpan(Math.Abs(_sessionTime.Hours), (Math.Abs(_sessionTime.Minutes / 30) + temp) * 30, 0);
                    _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).UnpaidTime = (TimeSpan)_unpaidTime;
                    _sessionTime = new TimeSpan(0, 0, 0);
                    DoPropertyChanged("UnpaidTime");
                    DoPropertyChanged("UnpaidTimeVisibility");
                }
                _todaysSessions[_selectedSession].SessionDuration = _sessionTime;
                _todaysSessions[_selectedSession].CurrentDuration += _sessionDuration - _sessionTime;
                _sessionTime = new TimeSpan(0, 0, 0);
                DoPropertyChanged("SessionTime");
                DoPropertyChanged("StopIsEnabled");
                DoPropertyChanged("UpdateIsEnabled");
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
        

        private ICommand _doAddNewClient;

        public ICommand DoAddNewClient
        {
            get
            {
                if (_doAddNewClient == null)
                {
                    _doAddNewClient = new Command(
                        p => true,
                        p => AddNewClient());
                }
                return _doAddNewClient;
            }
        }

        private ICommand _doEditClient;

        public ICommand DoEditClient
        {
            get
            {
                if (_doEditClient == null)
                {
                    _doEditClient = new Command(
                        p => true,
                        p => EditClient());
                }
                return _doEditClient;
            }
        }

        private ICommand _doUpdateNewSessionParametrs;

        public ICommand DoUpdateNewSessionParametrs
        {
            get
            {
                if (_doUpdateNewSessionParametrs == null)
                {
                    _doUpdateNewSessionParametrs = new Command(
                        p => true,
                        p => UpdateNewSessionParametrs(0));
                }
                return _doUpdateNewSessionParametrs;
            }
        }

        private ICommand _doApplyNewClient;

        public ICommand DoApplyNewClient
        {
            get
            {
                if (_doApplyNewClient == null)
                {
                    _doApplyNewClient = new Command(
                        p => true,
                        p => ApplyNewClient());
                }
                return _doApplyNewClient;
            }
        }

        private ICommand _doCancel;

        public ICommand DoCancel
        {
            get
            {
                if (_doCancel == null)
                {
                    _doCancel = new Command(
                        p => true,
                        p => Cancel());
                }
                return _doCancel;
            }
        }
    }
}
