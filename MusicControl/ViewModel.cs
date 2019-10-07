using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MusicControl
{
    public class ViewModel : INotifyPropertyChanged
    {
        private enum PageState
        {
            MainPage,
            SessionPage,
            ClientInfoPage,
            CalendarPage,
            HistoryPage
        }

        public enum SessionState
        {
            Removed,
            Changed,
            New
        }

        private bool _newClientInfoErrorVisibility;
        private DateTime _calendarDate;
        private TextBox _clientNameTextBox;
        private List<Client> _clients;
        private ComboBox _clientsComboBox;
        private ComboBox _clientSessionsComboBox;
        private List<TextBox> _clientTimeTextBoxes;
        private System.Timers.Timer _clockTimer;
        private bool _isEditMode;
        private bool _isAddingNewClient;
        private int _selectedScheduleLine;
        private bool _isSessionExist;
        private bool _isSessionPaussed;
        private bool _isSessionStarted;
        private MainWindow _mainWindow;
        private NavigationService _navigationService;
        private string _newClientName;
        private PageState _pageState;
        private TimeSpan _pauseDuration;
        private System.Timers.Timer _pauseTimer;
        private TimeSpan _sessionDuration;
        private Uri _sessionPage;
        private System.Timers.Timer _sessionTimer;
        private DateTime _timerStartTime;
        private List<Session> _todaysSessions;

        private bool _addBoxVisibility;

        public Visibility AddBoxVisibility
        {
            get
            {
                if (_addBoxVisibility) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        public Visibility NewClientInfoErrorVisibility
        {
            get
            {
                if (_newClientInfoErrorVisibility) return Visibility.Visible;
                return Visibility.Hidden;
            }
            set
            {
                if (value == Visibility.Visible)
                    _newClientInfoErrorVisibility = true;
                else _newClientInfoErrorVisibility = false;
                DoPropertyChanged("NewClientInfoErrorVisibility");
            }
        }

        public Visibility AddButtonVisibility
        {
            get
            {
                if (!_addBoxVisibility) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        public bool AddTimeIsEnabled
        {
            get { return _isSessionStarted && (_todaysSessions.Count != 0); }
        }

        public HashSet<DateTime> BookedCalendarDates
        {
            get
            {
                var sessions = new List<Session>();
                for (int i = 0; i < _clients.Count; i++)
                {
                    for (int j = 0; j < _clients[i].Sessions.Count; j++)
                        sessions.Add(_clients[i].Sessions[j]);
                }
                var dates = new List<DateTime>();
                var hashDates = new HashSet<DateTime>();
                if (sessions.Count != 0)
                {
                    dates.Add(sessions[0].StartSessionTime.Date);
                    for (int i = 1; i < sessions.Count; i++)
                    {
                        dates.Add(sessions[i].StartSessionTime.Date);
                    }
                    dates = dates.Distinct().ToList();
                    for(int i = 0; i < dates.Count; i++)
                    {
                        hashDates.Add(dates[i]);
                    }
                }
                //Костыль для обновления выделения дат
                CalendarDate = _calendarDate - new TimeSpan(31, 0, 0, 0);
                CalendarDate = _calendarDate + new TimeSpan(31, 0, 0, 0);
                return hashDates;
            }
        }

        public DateTime CalendarDate
        {
            get { return _calendarDate; }
            set
            {
                //if (value != _calendarDate) CalendarVisibility = Visibility.Hidden;
                _calendarDate = value;
                UpdateSchedule();
                DoPropertyChanged("CalendarDateButtonContent");
                DoPropertyChanged("CalendarDate");
            }
        }

        public String CalendarDateButtonContent
        {
            get { return _calendarDate.ToString("dd.MM.yyyy"); }
        }

        private Visibility _calendarVisibility;

        public Visibility CalendarVisibility
        {
            get
            {
                return _calendarVisibility;
            }
            set
            {
                _calendarVisibility = value;
                DoPropertyChanged("CalendarVisibility");
            }
        }

        public string ClientID
        {
            get
            {
                if (_isAddingNewClient||(_clients.Count == 0)||(_selectedClient > _clients.Count)) return "-";
                return _clients[_selectedClient].ClientID.ToString();
            }
        }

        public bool IsAddingClient
        {
            get
            {
                return _isAddingNewClient;
            }
            set
            {
                _isAddingNewClient = value;
                DoPropertyChanged("ClientID");
                DoPropertyChanged("TotalHours");
                DoPropertyChanged("UnpaidTime");
                DoPropertyChanged("TotalHoursPerYear");
                DoPropertyChanged("ClientTimeBalance");
                DoPropertyChanged("CurrentClientSessionTime");
                DoPropertyChanged("IsRemoveEnabled");
                DoPropertyChanged("IsEditEnabled");
                DoPropertyChanged("ClientSessions");
                if (value && (_clientSessionsComboBox != null))
                {
                    _clientSessionsComboBox.SelectedIndex = -1;
                    _clientsComboBox.SelectedValue = String.Empty;
                }
                if (value && (_clientsComboBox != null)) _clientsComboBox.Text = String.Empty;
            }
        }

        public bool ClientInfoIsEnabled
        {
            get
            { return !_addBoxVisibility; }
        }

        public List<Client> ClientList
        {
            get { return _clients; }
        }

        public String ClientTimeBalance
        {
            get
            {
                if (!_isAddingNewClient)
                    if (_clients.Count != 0)
                    {
                        if (_clients[_selectedClient].TimeBalance.Minutes != 0) return _clients[_selectedClient].TimeBalance.Hours.ToString() + "ч. " + _clients[_selectedClient].TimeBalance.Minutes.ToString() + "мин.";
                        return _clients[_selectedClient].TimeBalance.Hours.ToString() + "ч. ";
                    }
                return "--:--";
            }
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
                var namesakes = new List<string>();
                var namesakesCount = new List<int>();
                for (int i = 0; i < _clients.Count - 1; i++)
                    for (int j = i + 1; j < _clients.Count; j++)
                        if (String.Equals(clients[i], clients[j]))
                        {
                            if (namesakes.Count == 0)
                            {
                                namesakes.Add(clients[j]);
                                namesakesCount.Add(1);
                            }
                            for (int k = 0; k < namesakes.Count; k++)
                            {
                                if (String.Equals(namesakes[k], clients[j]))
                                {
                                    clients[j] += " (" + namesakesCount[k] + ")";
                                    namesakesCount[k]++;
                                }
                                else
                                {
                                    namesakes.Add(clients[j]);
                                    namesakesCount.Add(1);
                                }
                            }
                        }
                DoPropertyChanged("IsRemoveEnabled");
                DoPropertyChanged("IsEditEnabled");
                return clients;
            }
        }

        public ComboBox ClientsComboBox
        {
            get
            {
                return _clientsComboBox;
            }
            set
            {
                _clientsComboBox = value;
            }
        }

        public ComboBox ClientSessionsComboBox
        {
            get
            {
                return _clientSessionsComboBox;
            }
            set
            {
                _clientSessionsComboBox = value;
            }
        }

        public List<String> ClientSessions
        {
            get
            {
                
                var clientSessions = new List<String>();
                if (!IsAddingClient)
                    if (_clients.Count != 0)
                        for (int i = 0; i < _clients[_selectedClient].Sessions.Count; i++)
                        {
                            clientSessions.Add(_clients[_selectedClient].Sessions[i].StartSessionTime.ToString("yyyy/MM/dd HH:mm") + "-" + (_clients[_selectedClient].Sessions[i].StartSessionTime + _clients[_selectedClient].Sessions[i].SessionDuration).ToString("HH:mm"));
                        }
                return clientSessions;
            }
        }

        public List<string> ClientTimeTextBoxesText
        {
            get
            {
                var texts = new List<string>();
                if (_clientTimeTextBoxes != null) for (int i = 0; i < _clientTimeTextBoxes.Count; i++)
                        texts.Add(_clientTimeTextBoxes[i].Text);
                return texts;
            }
        }

        public String ClientUnpaidTime
        {
            get
            {
                if (_unpaidTime != null) return ((TimeSpan)_unpaidTime).Hours.ToString() + ":" + ((TimeSpan)_unpaidTime).Minutes.ToString();
                return "--:--";
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

        public bool ComboboxIsEnabled
        {
            get { return !_isSessionStarted && (_todaysSessions.Count != 0); }
        }

        public String CurrentClientSessionTime
        {
            get
            {
                if (!_isAddingNewClient)
                    if (_clients.Count != 0)
                    {
                        if ((_clients[_selectedClient].Sessions.Count != 0) && (_selectedClientSession >= 0))
                        {
                            if (_clients[_selectedClient].Sessions[_selectedClientSession].CurrentDuration.Minutes != 0) return _clients[_selectedClient].Sessions[_selectedClientSession].CurrentDuration.Hours.ToString() + "ч. " + _clients[_selectedClient].Sessions[_selectedClientSession].CurrentDuration.Minutes.ToString() + "мин.";
                            return _clients[_selectedClient].Sessions[_selectedClientSession].CurrentDuration.Hours.ToString() + "ч. ";
                        }
                    }
                return "--:--";
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

        public bool IsRemoveEnabled
        {
            get
            {
                if (_isAddingNewClient) return false;
                if (_clients.Count != 0) return true;
                return false;
            }
        }

        public bool IsEditEnabled
        {
            get
            {
                if (_isAddingNewClient) return false;
                if (_clientsComboBox.SelectedItem != null) return true;
                return false;
            }
        }

        public string NewClientName
        {
            get { return _newClientName; }
            set
            {
                _newClientName = value;
                DoPropertyChanged("NewClientName");
            }
        }

        public bool PauseIsEnabled
        {
            get { return _isSessionStarted && !_isSessionPaussed; }
        }

        private TimeSpan _pauseTime;
        public string PauseTime
        {
            get { return new DateTime(_pauseTime.Ticks).ToString("HH:mm:ss"); ; }
        }

        public List<Schedule> ScheduleList
        {
            get { return _scheduleList; }
            set
            {
                _scheduleList = value;
                DoPropertyChanged("ScheduleList");

            }
        }

        private List<Schedule> _scheduleList;

        private int _selectedClient;
        public int SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                IsAddingClient = false;
                if (value == -1) _selectedClient = 0;
                else _selectedClient = value;
                DoPropertyChanged("SelectedClient");
                DoPropertyChanged("ClientID");
                DoPropertyChanged("TotalHours");
                DoPropertyChanged("UnpaidTime");
                DoPropertyChanged("TotalHoursPerYear");
                DoPropertyChanged("ClientTimeBalance");
                DoPropertyChanged("ClientSessions");
                SelectedClientSession = 0;
                if (ClientSessions.Count > 0) _clientSessionsComboBox.SelectedValue = ClientSessions[0];
            }
        }

        private int _selectedClientSession;
        public int SelectedClientSession
        {
            get { return _selectedClientSession; }
            set
            {
                if (value != -1)
                {
                    _selectedClientSession = value;
                    DoPropertyChanged("SelectedClientSession");
                    DoPropertyChanged("UnpaidTime");
                    DoPropertyChanged("CurrentClientSessionTime");
                }
            }
        }

        public int SelectedScheduleLine
        {
            get { return _selectedScheduleLine; }
            set
            {
                if ((_scheduleList.Count > 0) && (value != -1))
                {
                    //Ошибка с обновлением после выбора строчки
                    if (_selectedScheduleLine != -1) _scheduleList[_selectedScheduleLine].IsSelected = false;
                    _scheduleList[value].IsSelected = true;
                    //Костыль для обновления кнопок
                    var tempSchedule = _scheduleList;
                    _scheduleList = new List<Schedule>();
                    for (int i = 0; i < 48; i++)
                        _scheduleList.Add(new Schedule(new List<TimeSpan>(), new TimeSpan(0), null, null, false, true, false));
                    DoPropertyChanged("ScheduleList");
                    _scheduleList = tempSchedule;
                    ////////////////////////////////
                    DoPropertyChanged("ScheduleList");
                }
                _selectedScheduleLine = value;
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

        private TimeSpan _sessionTime;
        public string SessionTime
        {
            get
            {
                if (_sessionTime.Ticks >= 0) return new DateTime(_sessionTime.Ticks).ToString("HH:mm:ss");
                else
                {
                    return "-" + new DateTime(-_sessionTime.Ticks).ToString("HH:mm:ss");
                }
            }

        }

        public System.Windows.Media.Brush SessionTimeColor
        {
            get
            {
                if (_sessionTime.Seconds < 0) return System.Windows.Media.Brushes.Red;
                return System.Windows.Media.Brushes.White;
            }
        }

        public bool StartIsEnabled
        {
            get { return (!_isSessionStarted || _isSessionPaussed) && (_todaysSessions.Count != 0); }
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

        public bool StopIsEnabled
        {
            get { return _isSessionStarted; }
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

        public string TotalHours
        {
            get
            {
                if (!_isAddingNewClient)
                {
                    var time = new TimeSpan(0);
                    if (_clients.Count != 0)
                    {
                        for (int i = 0; i < _clients[_selectedClient].Sessions.Count; i++)
                        {
                            time += _clients[_selectedClient].Sessions[i].CurrentDuration;
                        }
                        if (time.Minutes != 0) return time.Hours.ToString() + "ч. " + time.Minutes.ToString() + "мин.";
                        return time.Hours.ToString() + "ч. ";
                    }
                }
                return "--:--";
            }
        }

        public string TotalHoursPerYear
        {
            get
            {
                if (!_isAddingNewClient)
                {
                    var time = new TimeSpan(0);
                    if (_clients.Count != 0)
                    {
                        for (int i = 0; i < _clients[_selectedClient].Sessions.Count; i++)
                        {
                            if (_clients[_selectedClient].Sessions[i].StartSessionTime > DateTime.Now - new TimeSpan(365, 0, 0, 0))
                                time += _clients[_selectedClient].Sessions[i].CurrentDuration;
                        }
                        if (time.Minutes != 0) return time.Hours.ToString() + "ч. " + time.Minutes.ToString() + "мин.";
                        return time.Hours.ToString() + "ч. ";
                    }
                }
                return "--:--";
            }
        }

        public bool UpdateIsEnabled
        {
            get { return !_isSessionStarted; }
        }

        private Nullable<TimeSpan> _unpaidTime;

        public String UnpaidTime
        {
            get
            {
                if (!_isAddingNewClient)
                    if (_clients.Count != 0)
                    {
                        if (_clients[_selectedClient].UnpaidTime.Ticks != 0)
                        {
                            if (_clients[_selectedClient].UnpaidTime.Minutes != 0) return _clients[_selectedClient].UnpaidTime.Hours.ToString() + "ч. " + _clients[_selectedClient].UnpaidTime.Minutes.ToString() + "мин.";
                            return _clients[_selectedClient].UnpaidTime.Hours.ToString() + "ч. ";
                        }
                    }
                return "--:--";
            }
        }

        public Visibility UnpaidTimeVisibility
        {
            get
            {
                if (_unpaidTime != null) return Visibility.Visible;
                return Visibility.Hidden;
            }
        }

        public ViewModel()
        {
            DoPropertyChanged("Schedule");
            _pageState = PageState.MainPage;
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

            _clients = DataAccessManager.GetInstance().GetClients().ToList();

            //DataAccessManager.GetInstance().Connection.Insert(new Client("Иванов Иван Иванович", new TimeSpan(2, 0, 0), new TimeSpan(0)));
            //DataAccessManager.GetInstance().Connection.Insert(new Client("Петров Петр Иванович", new TimeSpan(3, 0, 0), new TimeSpan(0)));
            //DataAccessManager.GetInstance().Connection.Insert(new Client("Семёнов Иван Григорьевич", new TimeSpan(0), new TimeSpan(0)));
            //DataAccessManager.GetInstance().Connection.Insert(new Client("Семёнов Иван Викторович", new TimeSpan(0), new TimeSpan(0)));
            //DataAccessManager.GetInstance().Connection.Insert(new Client("Караванов Генадий Иванович", new TimeSpan(5, 30, 0), new TimeSpan(0)));
            //DataAccessManager.GetInstance().Connection.Insert(new Client("Сидоров Сергей Сергеевич", new TimeSpan(1, 0, 0), new TimeSpan(2, 0, 0)));
            //DataAccessManager.GetInstance().Connection.Insert(new Client("Куликов Петр Петрович", new TimeSpan(0), new TimeSpan(0)));
            //DataAccessManager.GetInstance().Connection.Insert(new Client("Григорьев Иван Анатольевич", new TimeSpan(1, 30, 0), new TimeSpan(0)));

            _clients = DataAccessManager.GetInstance().GetClients().ToList();

            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(2, 0, 0), new DateTime(2019, 6, 23, 11, 30, 0), _clients[0], new TimeSpan(1, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(3, 0, 0), new DateTime(2019, 7, 21, 14, 00, 0), _clients[0], new TimeSpan(3, 0, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(3, 0, 0), new DateTime(2019, 9, 5, 11, 00, 0), _clients[0], new TimeSpan(2, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(4, 30, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 9, 30, 0), _clients[0], new TimeSpan(0), true));

            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 6, 22, 11, 30, 0), _clients[1], new TimeSpan(1, 0, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(3, 30, 0), new DateTime(2019, 7, 19, 14, 00, 0), _clients[1], new TimeSpan(3, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(2, 0, 0), new DateTime(2019, 9, 5, 9, 00, 0), _clients[1], new TimeSpan(2, 0, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(3, 30, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 30, 0), _clients[1], new TimeSpan(0), false));

            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(2, 0, 0), new DateTime(2019, 6, 3, 11, 30, 0), _clients[2], new TimeSpan(2, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 30, 0), new DateTime(2019, 7, 1, 14, 00, 0), _clients[2], new TimeSpan(1, 30, 0), false));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 9, 5, 15, 00, 0), _clients[2], new TimeSpan(0, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 30, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 01, 30, 0), _clients[2], new TimeSpan(0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 30, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 19, 00, 0), _clients[2], new TimeSpan(0), true));

            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(2, 30, 0), new DateTime(2019, 6, 4, 11, 30, 0), _clients[4], new TimeSpan(2, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 7, 2, 14, 00, 0), _clients[4], new TimeSpan(1, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 8, 23, 16, 30, 0), _clients[4], new TimeSpan(1, 0, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(2, 0, 0), new DateTime(2019, 9, 5, 23, 30, 0), _clients[4], new TimeSpan(1, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 0, 0), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 22, 30, 0), _clients[4], new TimeSpan(0), true));

            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(2, 30, 0), new DateTime(2018, 6, 5, 11, 30, 0), _clients[5], new TimeSpan(1, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(1, 0, 0), new DateTime(2019, 7, 6, 14, 00, 0), _clients[5], new TimeSpan(1, 30, 0), true));
            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(2, 0, 0), new DateTime(2019, 9, 5, 21, 30, 0), _clients[5], new TimeSpan(1, 30, 0), true));

            //DataAccessManager.GetInstance().Connection.Insert(new Session(new TimeSpan(5, 30, 0), new DateTime(2018, 1, 5, 16, 30, 0), _clients[6], new TimeSpan(5, 30, 0), true));

            _clients.Sort((x, y) => String.Compare(x.ClientName, y.ClientName));
            CalendarDate = DateTime.Now;
            CustomLetterDayConverter.dict = BookedCalendarDates;
        }

        private void AddNewClient()
        {
            IsAddingClient = true;
            NewClientName = "Введите ФИО";
            for (int i = 0; i < _clientTimeTextBoxes.Count; i++)
                _clientTimeTextBoxes[i].Text = "0";
            _isEditMode = false;
            _addBoxVisibility = true;
            DoPropertyChanged("ClientInfoIsEnabled");
            DoPropertyChanged("AddBoxVisibility");
            DoPropertyChanged("AddButtonVisibility");

            var template = _clientNameTextBox.Template;
            var tb = (TextBox)template.FindName("InteriorTextBox", _clientNameTextBox);
            tb.Focus();
            //_clientNameTextBox.SelectAll();
        }

        private void AddTime()
        {
            if ((_isSessionStarted) && (_todaysSessions.Count != 0))
            {
                if (_selectedSession <= _todaysSessions.Count - 2)
                {
                    int i = 1;
                    while ((_clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).TimeBalance >= new TimeSpan(0, 15, 0)) && (_startTime + _todaysSessions[_selectedSession].SessionDuration + (new TimeSpan(1, 10, 0) - _pauseTime) + new TimeSpan(0, i * 15, 0) < _todaysSessions[_selectedSession + 1].StartSessionTime))
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

        private void ApplyNewClient()
        {
            //Не работает валидация через текстбоксы
            bool valid = Regex.IsMatch(_clientNameTextBox.Text, @"^[\p{L} \.'\-]+$");
            for (int i = 0; i < _clientTimeTextBoxes.Count; i++)
                valid &= Regex.IsMatch(_clientTimeTextBoxes[i].Text, @"^\d+$");
            //var valid = !Validation.GetHasError(_clientNameTextBox);
            //for (int i = 0; i < _clientTimeTextBoxes.Count; i++)
            //    valid &= !Validation.GetHasError(_clientTimeTextBoxes[i]);
            if (valid)
            {
                if (!_isEditMode)
                {
                    _addBoxVisibility = false;
                    var client = new Client(_clientNameTextBox.Text, new TimeSpan(int.Parse(_clientTimeTextBoxes[0].Text), int.Parse(_clientTimeTextBoxes[1].Text), 0), new TimeSpan(int.Parse(_clientTimeTextBoxes[2].Text), int.Parse(_clientTimeTextBoxes[3].Text), 0));
                    DataAccessManager.GetInstance().Connection.Insert(client);
                    _clients = DataAccessManager.GetInstance().GetClients().ToList();
                    //_clients.Add(new Client(GetNewClientID(), _clientNameTextBox.Text, new TimeSpan(int.Parse(_clientTimeTextBoxes[0].Text), int.Parse(_clientTimeTextBoxes[1].Text), 0), new TimeSpan(int.Parse(_clientTimeTextBoxes[2].Text), int.Parse(_clientTimeTextBoxes[3].Text), 0), new List<Session>()));
                    var temp = _clients[_clients.Count - 1];
                    _clients.Sort((x, y) => String.Compare(x.ClientName, y.ClientName));
                    DoPropertyChanged("Clients");
                    DoPropertyChanged("ClientSessions");
                    ClientsComboBox.SelectedIndex = _clients.IndexOf(temp);
                    ClientsComboBox.SelectedValue = temp.ClientName;
                    //SelectedClient = _clients.IndexOf(temp);
                }
                else
                {
                    var client = _clients[_selectedClient];
                    client.ClientName = _clientNameTextBox.Text;
                    client.TimeBalance = new TimeSpan(int.Parse(_clientTimeTextBoxes[0].Text), int.Parse(_clientTimeTextBoxes[1].Text), 0);
                    client.UnpaidTime = new TimeSpan(int.Parse(_clientTimeTextBoxes[2].Text), int.Parse(_clientTimeTextBoxes[3].Text), 0);
                    var temp = client;
                    DataAccessManager.GetInstance().Connection.Update(client);
                    _clients = DataAccessManager.GetInstance().GetClients().ToList();
                    _clients.Sort((x, y) => String.Compare(x.ClientName, y.ClientName));
                    DoPropertyChanged("Clients");
                    ClientsComboBox.SelectedIndex = _clients.IndexOf(temp);
                    ClientsComboBox.SelectedValue = temp.ClientName;
                    //SelectedClient = _clients.IndexOf(temp);
                    //Костыль для обновления первого пользователя
                    if (SelectedClient == 0) ClientsComboBox.SelectedValue = temp.ClientName;
                }
                _addBoxVisibility = false;
                DoPropertyChanged("ClientInfoIsEnabled");
                DoPropertyChanged("AddBoxVisibility");
                DoPropertyChanged("AddButtonVisibility");
                NewClientInfoErrorVisibility = Visibility.Hidden;
                _clientsComboBox.IsDropDownOpen = false;
            }
            else NewClientInfoErrorVisibility = Visibility.Visible;
        }

        public void AssignMainWindow(MainWindow mw)
        {
            _mainWindow = mw;
        }

        private void Cancel()
        {
            NewClientInfoErrorVisibility = Visibility.Hidden;
            _addBoxVisibility = false;
            if (_isEditMode) IsAddingClient = false;
            else IsAddingClient = true;
            DoPropertyChanged("ClientInfoIsEnabled");
            DoPropertyChanged("AddBoxVisibility");
            DoPropertyChanged("AddButtonVisibility");
            NewClientName = "Введите ФИО";
        }

        public void ChangeSession(Session session, SessionState sessionState)
        {
            if (sessionState == SessionState.Removed)
            {
                DataAccessManager.GetInstance().Connection.Delete(session);
                CustomLetterDayConverter.dict = BookedCalendarDates;
            }
            else if (sessionState == SessionState.Changed)
            {
                DataAccessManager.GetInstance().Connection.Update(session);
            }
            else if (sessionState == SessionState.New)
            {
                DataAccessManager.GetInstance().Connection.Insert(session);
                CustomLetterDayConverter.dict = BookedCalendarDates;
            }
            _todaysSessions = DataAccessManager.GetInstance().GetSessionsByDate(DateTime.Now).ToList();
            DoPropertyChanged("Sessions");
            DoPropertyChanged("BookedCalendarDates");
            UpdateSchedule();
        }

        public DateTime GetNewSessionStartTime()
        {
            return _calendarDate.Date + _scheduleList[_selectedScheduleLine].StartTime;
        }

        private void ClockTick(object sender, EventArgs e)
        {
            _clock = DateTime.Now.ToString("HH:mm");
            DoPropertyChanged("Clock");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void EditClient()
        {
            if (_clients.Count != 0)
            {
                _clientsComboBox.SelectedValue = Clients[_selectedClient];
                _isEditMode = true;
                NewClientName = _clients[_selectedClient].ClientName;
                _clientTimeTextBoxes[0].Text = _clients[_selectedClient].TimeBalance.Hours.ToString();
                _clientTimeTextBoxes[1].Text = _clients[_selectedClient].TimeBalance.Minutes.ToString();
                _clientTimeTextBoxes[2].Text = _clients[_selectedClient].UnpaidTime.Hours.ToString();
                _clientTimeTextBoxes[3].Text = _clients[_selectedClient].UnpaidTime.Minutes.ToString();
                _addBoxVisibility = true;
                DoPropertyChanged("ClientInfoIsEnabled");
                DoPropertyChanged("AddBoxVisibility");
                DoPropertyChanged("AddButtonVisibility");
                _clientNameTextBox.Focus();
            }
        }

        private bool IsTimeOver(TimeSpan time)
        {
            return (time.Hours == 0) && (time.Minutes == 0) && (time.Seconds == 0);
        }

        private void OpenCalendar()
        {
            _pageState = PageState.CalendarPage;
            if (_calendarVisibility == Visibility.Hidden)
                CalendarVisibility = Visibility.Visible;
            else CalendarVisibility = Visibility.Hidden;
            CustomLetterDayConverter.dict = BookedCalendarDates;
        }

        private void OpenClientInfoPage()
        {
            NewClientInfoErrorVisibility = Visibility.Hidden;
            IsAddingClient = true;
            _pageState = PageState.ClientInfoPage;
            _addBoxVisibility = false;
            DoPropertyChanged("AddBoxVisibility");
            DoPropertyChanged("AddButtonVisibility");
            _navigationService?.Navigate(new Uri("ClientInfoPage.xaml", UriKind.Relative));
        }

        private void OpenHistoryPage()
        {
            _pageState = PageState.HistoryPage;
            //TODO
        }

        private void OpenMainMenuPage()
        {
            _pageState = PageState.MainPage;
            _navigationService?.Navigate(new Uri("MainMenuPage.xaml", UriKind.Relative));
        }

        private void OpenSessionPage()
        {
            _pageState = PageState.SessionPage;
            if (!_isSessionExist)
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

        private void OpenSchedulePage()
        {
            CalendarDate = DateTime.Now;
            _selectedScheduleLine = -1;
            _pageState = PageState.CalendarPage;
            CalendarVisibility = Visibility.Hidden;
            _navigationService?.Navigate(new Uri("ScheduleMonthPage.xaml", UriKind.Relative));
        }

        private void PauseSession()
        {
            if ((!_isSessionPaussed) && (!IsTimeOver(_pauseTime)) && (_pageState == PageState.SessionPage))
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

        private void PauseTick(object sender, EventArgs e)
        {
            _pauseTime = (TimeSpan)(_timerStartTime + _pauseDuration - DateTime.Now);
            DoPropertyChanged("PauseTime");
        }

        private void RemoveClient()
        {
            if (_clients.Count != 0)
            {
                DataAccessManager.GetInstance().Connection.Delete(_clients[_selectedClient]);
                DataAccessManager.GetInstance().RemoveClientSessions(_clients[_selectedClient]);
                CustomLetterDayConverter.dict = BookedCalendarDates;
                _clients = DataAccessManager.GetInstance().GetClients().ToList();
                _clients.Sort((x, y) => String.Compare(x.ClientName, y.ClientName));
                _todaysSessions = DataAccessManager.GetInstance().GetSessionsByDate(DateTime.Now).ToList();
                _selectedClient = 0;
                DoPropertyChanged("Sessions");
                DoPropertyChanged("Clients");
                ClientsComboBox.SelectedIndex = 0;
                if (_clients.Count > 0) ClientsComboBox.SelectedValue = _clients[0].ClientName;
                //SelectedClient = 0;
                IsAddingClient = true;
                _clientsComboBox.IsDropDownOpen = false;
            }
        }

        private void SessionTick(object sender, EventArgs e)
        {
            _sessionTime = (TimeSpan)(_timerStartTime + _sessionDuration - DateTime.Now);
            DoPropertyChanged("SessionTime");
            DoPropertyChanged("SessionTimeColor");
        }

        public void SetClientTextBoxes(TextBox textBox, List<TextBox> textBoxes)
        {
            _clientTimeTextBoxes = textBoxes;
            _clientNameTextBox = textBox;
            for (int i = 0; i < _clientTimeTextBoxes.Count; i++)
                _clientTimeTextBoxes[i].Text = "0";
            _newClientName = "Введите ФИО";
            DoPropertyChanged("NewClientName");
        }

        public void SetNavigationService(Object page)
        {
            _navigationService = NavigationService.GetNavigationService(page as MainMenuPage);
        }

        private void StartSession()
        {
            if ((_isSessionExist) && (_pageState == PageState.SessionPage))
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

        private void StopSession()
        {
            if (_isSessionStarted)
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
                _clients.First(x => x.ClientID == _todaysSessions[_selectedSession].ClientID).Sessions.First(x => x.SessionID == _todaysSessions[_selectedSession].SessionID).SessionDuration = new TimeSpan(0);
                _todaysSessions[_selectedSession].SessionDuration = new TimeSpan(0);
                _todaysSessions[_selectedSession].CurrentDuration += _sessionDuration - _sessionTime;
                _sessionTime = new TimeSpan(0, 0, 0);
                DoPropertyChanged("SessionTime");
                DoPropertyChanged("StopIsEnabled");
                DoPropertyChanged("UpdateIsEnabled");
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

        private void UpdateSchedule()
        {
            _scheduleList = new List<Schedule>();
            var counter = 0;
            var thatDaySessions = DataAccessManager.GetInstance().GetSessionsByDate(_calendarDate).ToList();
            var beforeDaySessions = DataAccessManager.GetInstance().GetSessionsByDate(_calendarDate - new TimeSpan(1, 0, 0, 0)).ToList();
            var nextDaySessions = DataAccessManager.GetInstance().GetSessionsByDate(_calendarDate + new TimeSpan(1, 0, 0, 0)).ToList();
            beforeDaySessions.Sort((x, y) => DateTime.Compare(x.StartSessionTime, y.StartSessionTime));
            thatDaySessions.Sort((x, y) => DateTime.Compare(x.StartSessionTime, y.StartSessionTime));
            nextDaySessions.Sort((x, y) => DateTime.Compare(x.StartSessionTime, y.StartSessionTime));
            var duration = new TimeSpan(0);
            if (beforeDaySessions.Count > 0)
            {
                var currentDuration = beforeDaySessions[beforeDaySessions.Count - 1].SessionDuration;
                //var beforeDate = _calendarDate - new TimeSpan(1, 0, 0, 0);
                //if (beforeDate.Date < DateTime.Now.Date) currentDuration = beforeDaySessions[beforeDaySessions.Count - 1].CurrentDuration;
                if (beforeDaySessions[beforeDaySessions.Count - 1].StartSessionTime + currentDuration > new DateTime(_calendarDate.Year, _calendarDate.Month, _calendarDate.Day, 0, 0, 0))
                {
                    var sessionDuration = beforeDaySessions[beforeDaySessions.Count - 1].StartSessionTime + currentDuration;
                    duration = new TimeSpan(sessionDuration.Hour, sessionDuration.Minute, sessionDuration.Second);
                }
            }
            for (int i = 0; i < thatDaySessions.Count; i++)
            {
                while (duration.Ticks > 0)
                {
                    _scheduleList.Add(new Schedule(new List<TimeSpan>(), new TimeSpan(18000000000 * counter), null, null, false, false, false));
                    counter++;
                    duration -= new TimeSpan(0, 30, 0);
                }
                duration = thatDaySessions[i].GetStartSessionTimeSpan() - new TimeSpan(18000000000 * counter);
                while (duration.Ticks > 0)
                {
                    _scheduleList.Add(new Schedule(GetDurations(thatDaySessions[i].GetStartSessionTimeSpan(), counter), new TimeSpan(18000000000 * counter), null, null, false, true, false));
                    counter++;
                    duration -= new TimeSpan(0, 30, 0);
                }
                if (i != thatDaySessions.Count - 1)
                {
                    _scheduleList.Add(new Schedule(GetDurations(thatDaySessions[i + 1].GetStartSessionTimeSpan(), counter), new TimeSpan(18000000000 * counter), _clients.First(x => x.ClientID == thatDaySessions[i].ClientID), thatDaySessions[i], thatDaySessions[i].Prepayment, true, false));
                    counter++;
                    duration = thatDaySessions[i].SessionDuration - new TimeSpan(0, 30, 0);
                }
                else
                {
                    var startTime = new TimeSpan(48, 0, 0);
                    if (nextDaySessions.Count > 0)
                        startTime = nextDaySessions[0].GetStartSessionTimeSpan() + new TimeSpan(1, 0, 0, 0, 0);
                    _scheduleList.Add(new Schedule(GetDurations(startTime, counter), new TimeSpan(18000000000 * counter), _clients.First(x => x.ClientID == thatDaySessions[i].ClientID), thatDaySessions[i], thatDaySessions[i].Prepayment, true, false));
                    counter++;
                    duration = thatDaySessions[i].SessionDuration - new TimeSpan(0, 30, 0);
                }
            }
            if (duration > new TimeSpan(24, 0, 0) - new TimeSpan(18000000000 * counter))
                duration = new TimeSpan(24, 0, 0) - new TimeSpan(18000000000 * counter);
            while (duration.Ticks > 0)
            {
                _scheduleList.Add(new Schedule(new List<TimeSpan>(), new TimeSpan(18000000000 * counter), null, null, false, false, false));
                counter++;
                duration -= new TimeSpan(0, 30, 0);
            }
            duration = new TimeSpan(24, 0, 0) - new TimeSpan(18000000000 * (counter));
            while (duration.Ticks > 0)
            {
                var startTime = new TimeSpan(24, 0, 0);
                if (nextDaySessions.Count != 0) startTime = nextDaySessions[0].GetStartSessionTimeSpan() + new TimeSpan(1, 0, 0, 0, 0);
                else startTime = new TimeSpan(1, 23, 0, 0, 0);
                _scheduleList.Add(new Schedule(GetDurations(startTime, counter), new TimeSpan(18000000000 * counter), null, null, false, true, false));
                counter++;
                duration -= new TimeSpan(0, 30, 0);
            }
            DoPropertyChanged("ScheduleList");
        }

        private List<TimeSpan> GetDurations(TimeSpan time, int counter)
        {
            var durations = new List<TimeSpan>();
            var tempDuration = time - new TimeSpan(18000000000 * (counter));
            while (tempDuration.Ticks > 0)
            {
                if (tempDuration >= new TimeSpan(1, 0, 0, 0)) tempDuration = new TimeSpan(23, 30, 0);
                durations.Add(tempDuration);
                tempDuration -= new TimeSpan(0, 30, 0);
            }
            durations.Sort((x, y) => TimeSpan.Compare(x, y));
            return durations;
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

        private void UpdateTime()
        {
            DoPropertyChanged("SelectedSession");
            DoPropertyChanged("TimeBalance");
            DoPropertyChanged("PauseTime");
            DoPropertyChanged("StartTime");
            DoPropertyChanged("EndTime");
            DoPropertyChanged("SessionTime");
        }

        private void UpdateTodaysSessions()
        {
            _todaysSessions.Clear();
            _todaysSessions = DataAccessManager.GetInstance().GetSessionsByDate(DateTime.Now).ToList();
            DoPropertyChanged("Sessions");
            _todaysSessions.Sort((x, y) => DateTime.Compare(x.StartSessionTime, y.StartSessionTime));
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

        private ICommand _doOpenCalendar;

        public ICommand DoOpenCalendar
        {
            get
            {
                if (_doOpenCalendar == null)
                {
                    _doOpenCalendar = new Command(
                        p => true,
                        p => OpenCalendar());
                }
                return _doOpenCalendar;
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

        private ICommand _doRemoveClient;

        public ICommand DoRemoveClient
        {
            get
            {
                if (_doRemoveClient == null)
                {
                    _doRemoveClient = new Command(
                        p => true,
                        p => RemoveClient());
                }
                return _doRemoveClient;
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
    }
}
