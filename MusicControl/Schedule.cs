using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicControl
{
    public class Schedule
    {
        private List<Client> _clientList;
        private List<TimeSpan> _sessionDurations;
        private Client _client;
        private bool _prepayment;
        private TimeSpan? _duration;
        private TimeSpan _startTime;
        private bool _isEnabled;


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
        public List<Client> ClientList
        {
            get { return _clientList; }
            set { _clientList = value; }
        }
        public Client Client
        {
            get { return _client; }
            set { _client = value; }
        }
        public TimeSpan? Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }
        public bool Prepayment
        {
            get { return _prepayment; }
            set { _prepayment = value; }
        }

        public Schedule(List<Client> clientList, List<TimeSpan> sessionDurations, TimeSpan startTime, Client client, TimeSpan? duration, bool prepayment, bool isEnabled)
        {
            _client = client;
            _duration = duration;
            _prepayment = prepayment;
            _clientList = clientList;
            _sessionDurations = sessionDurations;
            _startTime = startTime;
            _isEnabled = isEnabled;
        }
    }
}
