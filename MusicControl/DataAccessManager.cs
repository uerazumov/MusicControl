using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicControl
{
    public class DataAccessManager
    {
        private static DataAccessManager instance;

        public readonly SQLiteConnection Connection;

        private DataAccessManager()
        {
            Connection = new SQLiteConnection("MusicControl.db");
            Connection.CreateTable<Client>();
            Connection.CreateTable<Session>();
        }

        public IEnumerable<Session> GetSessionsByClient(Client client)
        {
            return from session in Connection.Table<Session>() where session.ClientID == client.ClientID select session;
        }

        public Session GetSessionByID(int id)
        {
            return Connection.Get<Session>(id);
        }

        public IEnumerable<Session> GetSessionsByDate(DateTime date)
        {
            return from session in Connection.Table<Session>().ToList() where session.StartSessionTime.Date == date.Date select session;
        }

        public IEnumerable<Client> GetClients()
        {
            return Connection.Table<Client>();
        }

        public void RemoveClientSessions(Client client)
        {
            foreach (var session in Connection.Table<Session>())
            {
                if (session.ClientID == client.ClientID)
                    Connection.Delete(session);
            }

        }

        public static DataAccessManager GetInstance()
        {
            if (instance == null)
                instance = new DataAccessManager();
            return instance;
        }
    }
}
