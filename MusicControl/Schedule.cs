using System;
using System.Collections.Generic;

namespace MusicControl
{
    public class Schedule
    {
        private List<TimeSpan> _sessionDurations;
        private Client _client;
        private bool _prepayment;
        private Session _session;
        private TimeSpan _startTime;
        private bool _isEnabled;
        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { _isSelected = value; }
        }
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }
        public TimeSpan StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }
        public List<TimeSpan> SessionDurations
        {
            get { return _sessionDurations; }
            set { _sessionDurations = value; }
        }
        public Client Client
        {
            get { return _client; }
            set { _client = value; }
        }
        public Session Session
        {
            get { return _session; }
            set { _session = value; }
        }
        public bool Prepayment
        {
            get { return _prepayment; }
            set { _prepayment = value; }
        }

        public Schedule(List<TimeSpan> sessionDurations, TimeSpan startTime, Client client, Session session, bool prepayment, bool isEnabled, bool isSelected)
        {
            _client = client;
            _session = session;
            _prepayment = prepayment;
            _sessionDurations = sessionDurations;
            _startTime = startTime;
            _isEnabled = isEnabled;
            _isSelected = isSelected;
        }
    }
}
