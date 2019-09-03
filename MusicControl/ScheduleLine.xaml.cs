using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Drawing;
using System.Windows.Media;

namespace MusicControl
{
    public partial class ScheduleLine : INotifyPropertyChanged
    {
        public ScheduleLine()
        {
            InitializeComponent();
            DataContext = this;

            EditSessionButton.ControlCommand = DoEdit;
            SaveSessionButton.ControlCommand = DoSave;
            RemoveSessionButton.ControlCommand = DoRemove;
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
                SetValue(ScheduleParametrsProperty, value);
                DoPropertyChanged("ScheduleParametrs");
                DoPropertyChanged("Time");
                DoPropertyChanged("Durations");
                DoPropertyChanged("ClientList");
            }
        }

        public List<String> ClientList
        {
            get
            {
                var clients = new List<String>();
                for (int i = 0; i < ScheduleParametrs.ClientList.Count; i++)
                {
                    clients.Add(ScheduleParametrs.ClientList[i].ClientName);
                }
                return clients;
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
                if (ScheduleParametrs.Duration != null)
                {
                    var duration = (TimeSpan)ScheduleParametrs.Duration;
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

        private void Edit()
        {
            IsEditMode = true;
            ClientComboBox.SelectedIndex = ScheduleParametrs.ClientList.FindIndex(x => x == ScheduleParametrs.Client);
            DurationComboBox.SelectedIndex = ScheduleParametrs.SessionDurations.FindIndex(x => x == ScheduleParametrs.Duration);
        }

        private void Remove()
        {
            IsEditMode = true;
            ScheduleParametrs.Client = null;
            ScheduleParametrs.Duration = null;
            ClientComboBox.SelectedIndex = -1;
            DurationComboBox.SelectedIndex = -1;
            PrepaymentCheckBox.IsChecked = false;
        }

        public void Save()
        {
            if ((ClientComboBox.SelectedIndex != -1) && (DurationComboBox.SelectedIndex != -1))
            {
                ScheduleParametrs.Client = ScheduleParametrs.ClientList[ClientComboBox.SelectedIndex];
                ScheduleParametrs.Duration = ScheduleParametrs.SessionDurations[DurationComboBox.SelectedIndex];
                ScheduleParametrs.Prepayment = (bool)PrepaymentCheckBox.IsChecked;
                IsEditMode = false;
                DoPropertyChanged("ClientList");
                DoPropertyChanged("Durations");
                DoPropertyChanged("Time");
                DoPropertyChanged("ClientName");
                DoPropertyChanged("SessionDuration");
            }
        }

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

    //    public static DependencyProperty IsLineEnabledProperty =
    //DependencyProperty.Register("IsLineEnabled", typeof(bool), typeof(ScheduleLine), new UIPropertyMetadata(false, Refresh));



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
