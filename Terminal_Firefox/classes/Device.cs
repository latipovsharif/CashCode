using System;
using System.Data.SQLite;
using NLog;

namespace Terminal_Firefox.classes {

    public enum Devices {
        CashCode,
        Printer,
        CashSum,
        CashCount,
        Terminal
    }


    public class Device {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        public static void SetDeviceState(Devices device, string state) {
            try {
                using (SQLiteConnection connection = new SQLiteConnection()) {
                    connection.ConnectionString = SQLiteDatabase.DbConnection;
                    using (SQLiteCommand command = new SQLiteCommand()) {
                        try {
                            command.Connection = connection;
                            command.CommandText = "update perif_state set perif_state = @state where perif = @device";
                            command.Parameters.Add(new SQLiteParameter("@state", state));
                            command.Parameters.Add(new SQLiteParameter("@device", device));
                            connection.Open();
                            command.ExecuteNonQuery();
                        } catch (Exception exception) {
                            Log.Error("Невозможно обновить статус устройства", exception);
                        } finally {
                            command.Parameters.Clear();
                        }
                    }
                }

            } catch (Exception exception) {
                Log.Error(exception);
            }
        }
    }
}
