using System;
using NLog;

namespace Terminal_Firefox.classes {
    class Payment {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        public long id { get; set; }
        public short id_uslugi { get; set; }
        public string nomer { get; set; }
        public string nomer2 {
            get { return ""; }
            set { }
        } //Sms information
        public int summa { get; set; }
        public double summa_zachis { get; set; }
        public long status { get; set; }
        public string date_create { get { return DateTime.Now.ToString("yyyy-MM-dd"); } }
        public string date_send { get { return DateTime.Now.ToString("yyyy-MM-dd"); } }
        public int id_inkas { get { return TerminalSettings.Instance.CollectionId; } }
        public int val1 { get; set; }
        public int val3 { get; set; }
        public int val5 { get; set; }
        public int val10 { get; set; }
        public int val20 { get; set; }
        public int val50 { get; set; }
        public int val100 { get; set; }
        public int val200 { get; set; }
        public int val500 { get; set; }
        public string hash_id { get { return Guid.NewGuid().ToString(); } }
        public string checkn { get { return ""; } }
        public double summa_komissia { get; set; }
        public double rate { get; set; }
        public string curr { get { return "TJS"; } }

        public bool Save() {
            try {
                DBWrapper.Instance.Command.CommandText =
                    "INSERT INTO payments (service_id, number, sec_number, [check], date_create,  sum, comission, hash, state, n1, n3, n5, n10, n20, n50, n100, n200, n500, rate, curr) " +
                                   "VALUES(@service, @number, @sec_number, @check, @date_create, @sum, @comission, @hash, 0, @n1, @n3, @n5, @n10, @n20, @n50, @n100, @n200, @n500, @rate, @curr);";
                DBWrapper.Instance.Command.Parameters.Add("@service", id_uslugi);
                DBWrapper.Instance.Command.Parameters.Add("@number", nomer);
                DBWrapper.Instance.Command.Parameters.Add("@sec_number", nomer2);
                DBWrapper.Instance.Command.Parameters.Add("@check", checkn);
                DBWrapper.Instance.Command.Parameters.Add("@date_create", date_create);
                DBWrapper.Instance.Command.Parameters.Add("@sum", summa);
                DBWrapper.Instance.Command.Parameters.Add("@comission", summa_komissia);
                DBWrapper.Instance.Command.Parameters.Add("@hash", hash_id);
                DBWrapper.Instance.Command.Parameters.Add("@n1", val1);
                DBWrapper.Instance.Command.Parameters.Add("@n3", val3);
                DBWrapper.Instance.Command.Parameters.Add("@n5", val5);
                DBWrapper.Instance.Command.Parameters.Add("@n10", val10);
                DBWrapper.Instance.Command.Parameters.Add("@n20", val20);
                DBWrapper.Instance.Command.Parameters.Add("@n50", val50);
                DBWrapper.Instance.Command.Parameters.Add("@n100", val100);
                DBWrapper.Instance.Command.Parameters.Add("@n200", val200);
                DBWrapper.Instance.Command.Parameters.Add("@n500", val500);
                DBWrapper.Instance.Command.Parameters.Add("@rate", rate);
                DBWrapper.Instance.Command.Parameters.Add("@curr", curr);
                DBWrapper.Instance.Command.ExecuteNonQuery();
            } catch (Exception ex) {
                Log.Fatal("Невозможно сохранить данные по платежу", ex);
                return false;
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }
            return true;
        }
    }
}
