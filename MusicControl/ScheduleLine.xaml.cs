using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace MusicControl
{
    public partial class ScheduleLine : INotifyPropertyChanged
    {
        public ScheduleLine()
        {
            InitializeComponent();
            DataContext = this;
            IsEditMode = false;
            IsLineEnabled = true;

            EditSessionButton.ControlCommand = DoEdit;
        }

        public static DependencyProperty ScheduleParametrsProperty =
    DependencyProperty.Register("ScheduleParametrs", typeof(Schedule), typeof(ScheduleLine));

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
                if (_isEditMode) return Visibility.Visible;
                if (ScheduleParametrs.Client != null) return Visibility.Hidden;
                return Visibility.Visible;
            }
        }

        public Visibility InfoVisibility
        {
            get
            {
                if (_isEditMode) return Visibility.Hidden;
                if (ScheduleParametrs.Client == null) return Visibility.Hidden;
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
                if (ScheduleParametrs.Client != null) return false;
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
            }
        }

        private void Edit()
        {
            IsEditMode = true;
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

        public static DependencyProperty IsLineEnabledProperty =
    DependencyProperty.Register("IsLineEnabled", typeof(bool), typeof(ScheduleLine), new UIPropertyMetadata(false, Refresh));

        public static void Refresh(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            ScheduleLine scheduleLine = (ScheduleLine)property;
            scheduleLine.IsLineEnabled = (bool)args.NewValue;
            scheduleLine.DoPropertyChanged("IsLineEnabled");
            scheduleLine.DoPropertyChanged("ClientList");
            scheduleLine.DoPropertyChanged("Durations");
            scheduleLine.DoPropertyChanged("Time");
            scheduleLine.DoPropertyChanged("EditVisibility");
            scheduleLine.DoPropertyChanged("ClientName"); 
            scheduleLine.DoPropertyChanged("InfoVisibility");
            scheduleLine.DoPropertyChanged("SessionDuration");
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
            }
        }
    }
}
