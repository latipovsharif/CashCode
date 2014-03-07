using System;
using System.Data.SQLite;
using NLog;

namespace Terminal_Firefox.classes {
    public static class Rate {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static double GetCommissionAmount(int serviceId, int sum) {
            double value = 0;

            try {
                using (SQLiteConnection connection = new SQLiteConnection()) {
                    connection.ConnectionString = SQLiteDatabase.DbConnection;
                    using (SQLiteCommand command = new SQLiteCommand()) {
                        try {
                            command.Connection = connection;
                            command.CommandText = "select rate, is_percent from comission where service_id = @serviceId and from_sum <= @sum and to_sum >= @sum";

                            command.Parameters.Add(new SQLiteParameter("@serviceId", serviceId));
                            command.Parameters.Add(new SQLiteParameter("@sum", sum));
                            
                            connection.Open();

                            SQLiteDataReader reader = command.ExecuteReader();

                            while (reader.Read()) {
                                if ((bool)reader[1]) {
                                    value = sum * Convert.ToDouble(reader[0]) / 100;
                                } else {
                                    value = Convert.ToDouble(reader[0]);
                                }
                            }

                        } catch (Exception exception) {
                            Log.Error("Невозможно обновить статус платежа", exception);
                        } finally {
                            command.Parameters.Clear();
                        }
                    }
                }
                
            } catch (Exception ex) {
                Log.Error(
                    String.Format("Невозможно получить комиссию для услуги с id = {0} и суммой в {1}", serviceId, sum),
                    ex);
            } 

            return value;
        }

        public static string GetCommissionString(int serviceId) {
            string result = "";
            try {

                using (SQLiteConnection connection = new SQLiteConnection()) {
                    connection.ConnectionString = SQLiteDatabase.DbConnection;
                    using (SQLiteCommand command = new SQLiteCommand()) {
                        try {
                            command.Connection = connection;
                            command.CommandText = "select from_sum, to_sum, rate, is_percent from comission where service_id = @serviceId";


                            command.Parameters.Add(new SQLiteParameter("@serviceId", serviceId));

                            connection.Open();

                            SQLiteDataReader reader = command.ExecuteReader();
                            while (reader.Read()) {
                                result += "От " + reader[0] + " до " + reader[1] + " комиссия " + reader[2] +
                                          ((bool)reader[3] ? "%" : "") + "\r\n";
                            }
                        } catch (Exception exception) {
                            Log.Error("Невозможно обновить статус платежа", exception);
                        } finally {
                            command.Parameters.Clear();
                        }
                    }
                }

                
            } catch (Exception ex) {
                Log.Error(
                    String.Format("Невозможно получить комиссию для услуги с id = {0}", serviceId),
                    ex);
            } 
            return result;
        }
    }
}
