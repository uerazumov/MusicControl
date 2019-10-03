using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using Brushes = System.Windows.Media.Brushes;

namespace MusicControl
{
    public partial class ScheduleLine : INotifyPropertyChanged
    {
        MainWindow _window;
        public ScheduleLine()
        {
            InitializeComponent();
            DataContext = this;

            EditSessionButton.ControlCommand = DoEdit;
            SaveSessionButton.ControlCommand = DoSave;
            RemoveSessionButton.ControlCommand = DoRemove;
            _window = Application.Current.Windows.OfType<MainWindow>().SingleOrDefault(w => w.IsActive);
        }

        public SolidColorBrush BackgroundBrush
        {
            get
            {
                if (IsLineEnabled) return new SolidColorBrush(Colors.Transparent);
                return new SolidColorBrush(Colors.Gray);
            }
        }

        public static DependencyProperty ScheduleParametrsProperty =
    DependencyProperty.Register("ScheduleParametrs", typeof(Schedule), typeof(ScheduleLine), new UIPropertyMetadata(null, Refresh));

        public static void Refresh(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ScheduleLine scheduleLine = (ScheduleLine)property;
            scheduleLine.ScheduleParametrs = (Schedule)args.NewValue;
            scheduleLine.IsLineEnabled = scheduleLine.ScheduleParametrs.IsEnabled;
        }

        public Schedule ScheduleParametrs
        {
            get
            {
                return (Schedule)GetValue(ScheduleParametrsProperty);
            }

            set
            {
                _selectedClient = -1;
                SetValue(ScheduleParametrsProperty, value);
                if (ScheduleParametrs.Client == null) _state = ViewModel.SessionState.New;
                if (ScheduleParametrs.IsEnabled) IsLineEnabled = true;
                else IsLineEnabled = false;
                if (ScheduleParametrs.Client != null) IsEditMode = false;
                else IsEditMode = true;
                if (IsEditMode)
                {
                    ClientComboBox.SelectedIndex = -1;
                    DurationComboBox.SelectedIndex = -1;
                }
                var template = ClientComboBox.Template;
                var tb = (TextBox)template.FindName("PART_EditableTextBox", ClientComboBox);
                if (tb != null)
                    tb.Text = String.Empty;
                DoPropertyChanged("IsSelected");
                DoPropertyChanged("IsDateEnabled");
            }
        }

        public bool IsDateEnabled
        {
            get
            {
                if (_window.GetVM().CalendarDate.Date + ScheduleParametrs.StartTime < DateTime.Now)
                    return false;
                return IsSelected;
            }
            set
            {
                DoPropertyChanged("IsDateEnabled");
                DoPropertyChanged("IsCheckEnabled");
            }
        }

        public bool IsSelected
        {
            get
            {
                return ScheduleParametrs.IsSelected;
            }
            set
            {
                ScheduleParametrs.IsSelected = value;
                DoPropertyChanged("IsSelected");
                DoPropertyChanged("IsCheckEnabled");
            }
        }

        public List<String> ClientList
        {
            get
            {
                if (_window.GetVM().Clients.Count != 0)
                    return _window.GetVM().Clients;
                return new List<string>();
            }
        }

        public List<String> Durations
        {
            get
            {
                var durations = new List<String>();
                for (int i = 0; i < ScheduleParametrs.SessionDurations.Count; i++)
                {
                    if (ScheduleParametrs.SessionDurations[i].Minutes != 0)
                        durations.Add(ScheduleParametrs.SessionDurations[i].Hours.ToString() + ":" + ScheduleParametrs.SessionDurations[i].Minutes.ToString());
                    else durations.Add(ScheduleParametrs.SessionDurations[i].Hours.ToString() + ":00");
                }
                return durations;
            }
        }

        public String Time
        {
            get
            {
                var time = String.Empty;
                if (ScheduleParametrs.StartTime.Hours < 10) time += "0" + ScheduleParametrs.StartTime.Hours.ToString();
                else time += ScheduleParametrs.StartTime.Hours.ToString();
                if (ScheduleParametrs.StartTime.Minutes < 10) time += ":0" + ScheduleParametrs.StartTime.Minutes.ToString();
                else time += ":" + ScheduleParametrs.StartTime.Minutes.ToString();
                return time;
            }
        }

        public Visibility EditVisibility
        {
            get
            {
                if (!_isLineEnabled) return Visibility.Hidden;
                else if (_isEditMode) return Visibility.Visible;
                else if (ScheduleParametrs.Client != null) return Visibility.Hidden;
                return Visibility.Visible;
            }
        }

        public Visibility InfoVisibility
        {
            get
            {
                if (!_isLineEnabled) return Visibility.Hidden;
                else if (_isEditMode) return Visibility.Hidden;
                else if (ScheduleParametrs.Client == null) return Visibility.Hidden;
                return Visibility.Visible;
            }
        }

        public String ClientName
        {
            get
            {
                if (ScheduleParametrs.Client != null) return ScheduleParametrs.Client.ClientName;
                return "-";
            }
        }

        public String SessionDuration
        {
            get
            {
                if (ScheduleParametrs.Session != null)
                {
                    var duration = (TimeSpan)ScheduleParametrs.Session.SessionDuration;
                    var time = String.Empty;
                    if (duration.Hours < 10) time += "0" + duration.Hours.ToString();
                    else time += ScheduleParametrs.StartTime.Hours.ToString();
                    if (duration.Minutes < 10) time += ":0" + duration.Minutes.ToString();
                    else time += ":" + duration.Minutes.ToString();
                    return time;
                }
                else return "--:--";
            }
        }

        public bool IsPrepayment
        {
            get
            {
                if (ScheduleParametrs.Client != null) return ScheduleParametrs.Prepayment;
                return false;
            }
        }

        public bool IsCheckEnabled
        {
            get
            {
                if (_window.GetVM().CalendarDate.Date + ScheduleParametrs.StartTime < DateTime.Now) return false;
                if (!IsSelected) return false;
                if (!_isLineEnabled) return false;
                else if (_isEditMode) return true;
                else if (ScheduleParametrs.Client != null) return false;
                return true;
            }
        }

        private bool _isEditMode;

        public bool IsEditMode
        {
            get { return _isEditMode; }
            set
            {
                _isEditMode = value;
                DoPropertyChanged("InfoVisibility");
                DoPropertyChanged("EditVisibility");
                DoPropertyChanged("IsCheckEnabled");
                DoPropertyChanged("IsPrepayment");
            }
        }

        private ViewModel.SessionState _state;
        private int _oldClientID;

        private void Edit()
        {
            _state = ViewModel.SessionState.Changed;
            _oldClientID = ScheduleParametrs.Client.ClientID;
            IsEditMode = true;
            ClientComboBox.SelectedIndex = _window.GetVM().ClientList.FindIndex(x => x == ScheduleParametrs.Client);
            DurationComboBox.SelectedIndex = ScheduleParametrs.SessionDurations.FindIndex(x => x == ScheduleParametrs.Session.SessionDuration);
        }

        private void Remove()
        {
            _state = ViewModel.SessionState.Removed;
            _window.GetVM().ChangeSession(ScheduleParametrs.Session, ViewModel.SessionState.Removed);
            IsEditMode = true;
            ScheduleParametrs.Client = null;
            ScheduleParametrs.Session = null;
            ClientComboBox.SelectedIndex = -1;
            _selectedClient = -1;
            DurationComboBox.SelectedIndex = -1;
            PrepaymentCheckBox.IsChecked = false;
            var temp = _window.GetVM().SelectedScheduleLine;
            _window.GetVM().SelectedScheduleLine = 0;
            _window.GetVM().SelectedScheduleLine = temp;
        }

        public void Save()
        {
            if ((_selectedClient != -1) && (DurationComboBox.SelectedIndex != -1))
            {
                if (_state == ViewModel.SessionState.New)
                {
                    var session = new Session(ScheduleParametrs.SessionDurations[DurationComboBox.SelectedIndex], _window.GetVM().GetNewSessionStartTime(), _window.GetVM().ClientList[_selectedClient], new TimeSpan(0), (bool)PrepaymentCheckBox.IsChecked);
                    _window.GetVM().ChangeSession(session, ViewModel.SessionState.New);
                    //_window.GetVM().ChangeSession(_window.GetVM().ClientList[ClientComboBox.SelectedIndex].ClientID, ViewModel.SessionState.New, PrepaymentCheckBox.IsChecked, ScheduleParametrs.SessionDurations[DurationComboBox.SelectedIndex]);
                }
                else if (_state == ViewModel.SessionState.Changed)
                {
                    var session = DataAccessManager.GetInstance().GetSessionByID(ScheduleParametrs.Session.SessionID);
                    session.ClientID = _window.GetVM().ClientList[_selectedClient].ClientID;
                    session.Prepayment = (bool)PrepaymentCheckBox.IsChecked;
                    session.SessionDuration = ScheduleParametrs.SessionDurations[DurationComboBox.SelectedIndex];
                    _window.GetVM().ChangeSession(session, ViewModel.SessionState.Changed);
                    //_window.GetVM().ChangeSession(_window.GetVM().ClientList[ClientComboBox.SelectedIndex].ClientID, _oldClientID, ScheduleParametrs.Session.SessionID, ViewModel.SessionState.Changed, PrepaymentCheckBox.IsChecked, ScheduleParametrs.SessionDurations[DurationComboBox.SelectedIndex]);
                }
                IsEditMode = false;
                DoPropertyChanged("ClientList");
                DoPropertyChanged("Durations");
                DoPropertyChanged("Time");
                DoPropertyChanged("ClientName");
                DoPropertyChanged("SessionDuration");
                var temp = _window.GetVM().SelectedScheduleLine;
                _window.GetVM().SelectedScheduleLine = 0;
                _window.GetVM().SelectedScheduleLine = temp;
            }
        }

        void OnComboboxTextChanged(object sender, RoutedEventArgs e)
        {
            
            var tb = (TextBox)e.OriginalSource;
            if (tb.Text == String.Empty)
                ClientComboBox.IsDropDownOpen = false;
            else ClientComboBox.IsDropDownOpen = true;
            if (ClientComboBox.Text == "") ClientComboBox.SelectedIndex = -1;
            tb.Select(tb.SelectionStart + tb.SelectionLength, 0);
            CollectionView cv = (CollectionView)CollectionViewSource.GetDefaultView(ClientComboBox.ItemsSource);
            cv.Filter = s =>
                ((string)s).IndexOf(ClientComboBox.Text, StringComparison.CurrentCultureIgnoreCase) >= 0;
            if ((ClientComboBox.SelectedValue != null) && (_window.GetVM().Clients.Count != 0))
                if (_window.GetVM().Clients.First(x => x == ClientComboBox.SelectedValue.ToString()) != null)
                {
                    cv.Filter = s => ((string)s).Length >= 0;
                    Console.WriteLine(cv.IndexOf(ClientComboBox.SelectedValue));
                    Console.WriteLine(cv.IndexOf(ClientComboBox.SelectedIndex));
                    _selectedClient = cv.IndexOf(ClientComboBox.SelectedValue);
                    ClientComboBox.SelectedIndex = -1;
                    tb.SelectionBrush = Brushes.Transparent;
                    tb.CaretBrush = Brushes.Transparent;
                    DurationComboBox.Focus();
                }
        }

        private void ClientListGotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)e.OriginalSource;
            tb.SelectionBrush = Brushes.AliceBlue;
            tb.CaretBrush = Brushes.Black;
        }

        private int _selectedClient;

        private ICommand _doRemove;

        public ICommand DoRemove
        {
            get
            {
                if (_doRemove == null)
                {
                    _doRemove = new Command(
                        p => true,
                        p => Remove());
                }
                return _doRemove;
            }
        }

        private ICommand _doSave;

        public ICommand DoSave
        {
            get
            {
                if (_doSave == null)
                {
                    _doSave = new Command(
                        p => true,
                        p => Save());
                }
                return _doSave;
            }
        }

        private ICommand _doEdit;

        public ICommand DoEdit
        {
            get
            {
                if (_doEdit == null)
                {
                    _doEdit = new Command(
                        p => true,
                        p => Edit());
                }
                return _doEdit;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private bool _isLineEnabled;

        public bool IsLineEnabled
        {
            get { return _isLineEnabled; }
            set
            {
                _isLineEnabled = value;
                IsEnabled = value;
                DoPropertyChanged("IsLineEnabled");
                DoPropertyChanged("ClientList");
                DoPropertyChanged("Durations");
                DoPropertyChanged("Time");
                DoPropertyChanged("EditVisibility");
                DoPropertyChanged("InfoVisibility");
                DoPropertyChanged("ClientName");
                DoPropertyChanged("SessionDuration"); 
                DoPropertyChanged("BackgroundBrush"); 
            }
        }
    }
}
