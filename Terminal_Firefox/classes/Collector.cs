using System;
using System.Data.SQLite;
using NLog;

namespace Terminal_Firefox.classes {
    public class Collector {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        
        public int Id;
        public string passw { get; set; }
        public string user { get; set; }


        public static Collector FindCollector(string login, string password) {
            var collector = new Collector();
            try {

                using (SQLiteConnection connection = new SQLiteConnection(SQLiteDatabase.DbConnection)) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand()) {
                        command.CommandText = "select id from collector where login=@login and pass=@password";
                        
                        command.Parameters.Add(new SQLiteParameter("@login", login));
                        command.Parameters.Add(new SQLiteParameter("@password", password));

                        int.TryParse(command.ExecuteScalar().ToString(), out collector.Id);

                    }
                }

                if (collector.Id > 0) {
                    collector.passw = password;
                    collector.user = login;
                    return collector;
                }
            } catch (Exception exception) {
                Log.Error(String.Format("Невозможно получить инкассатора с логином {0} и паролем {1}", login, password), exception);
            } 

            return collector;
        }
    }
}
