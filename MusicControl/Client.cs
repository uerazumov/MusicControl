using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicControl
{
    public class Client
    {
        private int _clientID;
        private string _clientName;
        private TimeSpan _timeBalance;
        private TimeSpan _unpaidTime;

        [Ignore]
        public List<Session> Sessions => DataAccessManager.GetInstance().GetSessionsByClient(this).ToList();

        public TimeSpan UnpaidTime
        {
            get { return _unpaidTime; }
            set { _unpaidTime = value; }
        }

        public TimeSpan TimeBalance
        {
            get { return _timeBalance; }
            set { _timeBalance = value; }
        }

        [PrimaryKey, AutoIncrement]
        public int ClientID
        {
            get { return _clientID; }
            set { _clientID = value; }
        }
        
        public string ClientName
        {
            get { return _clientName; }
            set { _clientName = value; }
        }

        public Client(string clientName, TimeSpan timeBalance, TimeSpan unpaidTime)
        {
            _clientName = clientName;
            _timeBalance = timeBalance;
            _unpaidTime = unpaidTime;
        }

        public Client() { }
    }
}
