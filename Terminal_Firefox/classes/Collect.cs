using System;
using System.Data.SQLite;
using NLog;

namespace Terminal_Firefox.classes {
    public class Collect {
        public Collector collector = new Collector();
        public string date_inkass { get; set; }
        public int summa { get; set; }
        public string inkass_id { get; set; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        public Collect(Collector collector) {
            this.collector = collector;

            try {
                if (!String.IsNullOrWhiteSpace(TerminalSettings.Instance.CollectionId)) {
                    // Set collector and current collection date

                    using (SQLiteConnection connection = new SQLiteConnection(SQLiteDatabase.DbConnection)) {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand()) {
                            command.CommandText = "SELECT " +
                                                  "(n1 * 1 + n3 * 3 + n5 * 5 + n10 * 10 + n20 * 20 + n50 * 50 + n100 * 100 + n200 * 200 + n500 * 500) AS sum " +
                                                  "FROM collection WHERE collect_id = @collectId";
                            command.Parameters.Add(new SQLiteParameter("@collectId",
                                                                       TerminalSettings.Instance.CollectionId));
                            summa = (int) command.ExecuteScalar();
                        }
                    }
                }
            } catch (Exception ex) {
                Log.Error(ex);
            }

            date_inkass = DateTime.Now.ToString("yyyy-MM-dd");
            inkass_id = TerminalSettings.Instance.CollectionId;
        }




        public static void InsertBanknote(int val) {
            try {
                using (SQLiteConnection connection = new SQLiteConnection(SQLiteDatabase.DbConnection)) {
                    connection.Open();
                    using (SQLiteCommand command = new SQLiteCommand()) {
                        command.CommandText = "update collection set n" + val + " = n" + val +
                                              " + 1 where collect_id = " + TerminalSettings.Instance.CollectionId;

                        command.ExecuteNonQuery();
                    }
                }
            } catch (Exception exception) {
                Log.Fatal(String.Format("Невозможно запистать купюру: {0} в таблицу инкассации", val), exception);
            }
        }
    }
}
