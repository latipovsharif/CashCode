using System;

namespace Terminal_Firefox.classes {
    public class Collector {
        public int Id;
        public string passw { get; set; }
        public string user { get; set; }

        public bool SetCollector(string login, string password) {
            var collector = new Collector();
            try {
                DBWrapper.Instance.Command.CommandText =
                    "select id from collector where login=@login and pass=@password";
                DBWrapper.Instance.Command.Parameters.Add("@login", login);
                DBWrapper.Instance.Command.Parameters.Add("@password", password);
                Id = int.Parse(DBWrapper.Instance.Command.ExecuteScalar().ToString());
                if(Id  > 0) {
                    passw = password;
                    user = login;
                    return true;
                }
            } catch (Exception) {
                passw = "";
                user = "";
            }
            return false;
        }
    }
}
