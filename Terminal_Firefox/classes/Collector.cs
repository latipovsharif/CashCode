using System;
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
                DBWrapper.Instance.Command.CommandText =
                    "select id from collector where login=@login and pass=@password";
                DBWrapper.Instance.Command.Parameters.Add("@login", login);
                DBWrapper.Instance.Command.Parameters.Add("@password", password);

                int.TryParse(DBWrapper.Instance.Command.ExecuteScalar().ToString(), out collector.Id);

                if (collector.Id > 0) {
                    collector.passw = password;
                    collector.user = login;
                    return collector;
                }
            } catch (Exception exception) {
                Log.Error(String.Format("Невозможно получить инкассатора с логином {0} и паролем {1}", login, password), exception);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }
            return collector;
        }
    }
}
