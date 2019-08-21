using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicControl
{
    class Session
    {
        private TimeSpan _sessionDuration;
        private Client _client;
        private DateTime _startSessionTime;
        private DateTime _endSessionTime;
        private int _sessionID;

        public int SessionID
        {
            get { return _sessionID; }
            set { _sessionID = value; }
        }
        public TimeSpan SessionDuration
        {
            get { return _sessionDuration; }
            set { _sessionDuration = value; }
        }
        public Client Client
        {
            get { return _client; }
            set { _client = value; }
        }
        public DateTime StartSessionTime
        {
            get { return _startSessionTime; }
            set { _startSessionTime = value; }
        }

        public DateTime EndSessionTime
        {
            get { return _endSessionTime; }
            set { _endSessionTime = value; }
        }

        public Session(TimeSpan sessionDuration, Client client, DateTime startSessionTime, int sessionID)
        {
            _sessionDuration = sessionDuration;
            _client = client;
            _startSessionTime = startSessionTime;
            _sessionID = sessionID;
            _endSessionTime = startSessionTime + sessionDuration;
        }
    }
}
