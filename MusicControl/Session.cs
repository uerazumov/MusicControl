using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicControl
{
    public class Session
    {
        private TimeSpan _sessionDuration;
        private DateTime _startSessionTime;
        private DateTime _endSessionTime;
        private int _sessionID;
        private int _clientID;
        private TimeSpan _currentDuration;

        public TimeSpan CurrentDuration
        {
            get { return _currentDuration; }
            set { _currentDuration = value; }
        }
        public int ClientID
        {
            get { return _clientID; }
            set { _clientID = value; }
        }
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

        public Session(TimeSpan sessionDuration, DateTime startSessionTime, int sessionID, int clientID, TimeSpan currentDuration)
        {
            _sessionDuration = sessionDuration;
            _startSessionTime = startSessionTime;
            _sessionID = sessionID;
            _clientID = clientID;
            _endSessionTime = startSessionTime + sessionDuration;
            _currentDuration = currentDuration;
        }
    }
}
