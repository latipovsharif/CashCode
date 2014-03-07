using System;
using System.Data.SQLite;
using NLog;

namespace Terminal_Firefox.classes {
    internal class Payment {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public long id { get; set; }
        public short id_uslugi { get; set; }
        public string nomer { get; set; }
        public string nomer2 { get; set; } //Sms information
        public short summa { get; set; }
        public double summa_zachis { get; set; }
        public long status { get; set; }
        public string date_create { get; set; }
        public string date_send { get; set; }
        public string id_inkas { get; set; }
        public short val1 { get; set; }
        public short val3 { get; set; }
        public short val5 { get; set; }
        public short val10 { get; set; }
        public short val20 { get; set; }
        public short val50 { get; set; }
        public short val100 { get; set; }
        public short val200 { get; set; }
        public short val500 { get; set; }

        public string hesh_id { get; set; }
        public string chekn { get; private set; }
        public double summa_komissia { get; set; }
        public double rate { get; set; }
        public string curr { get { return ""; } set { } }


        public Payment() {
            try {
            date_create = ((Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString(); 
            id_inkas = TerminalSettings.Instance.TerminalNumber + TerminalSettings.Instance.CollectionId;
            chekn = TerminalSettings.Instance.TerminalNumber + date_create;
            hesh_id = Guid.NewGuid().ToString();

            } catch (Exception exception) {
                Log.Error(exception);
            }
        }
        

        public bool Save() {
            try {
                using (SQLiteConnection connection = new SQLiteConnection(SQLiteDatabase.DbConnection)) {
                    using (SQLiteCommand command = new SQLiteCommand()) {
                        command.Connection = connection;
                        connection.Open();
                        command.CommandText = "INSERT INTO payments (service_id, number, sec_number, [check], date_create,  sum, comission, hash, state, n1, n3, n5, n10, n20, n50, n100, n200, n500, rate, curr) " +
                    "VALUES(@service, @number, @sec_number, @check, @date_create, @sum, @comission, @hash, 0, @n1, @n3, @n5, @n10, @n20, @n50, @n100, @n200, @n500, @rate, @curr);";
                        command.Parameters.Add(new SQLiteParameter("@service", id_uslugi));
                        command.Parameters.Add(new SQLiteParameter("@number", nomer));
                        command.Parameters.Add(new SQLiteParameter("@sec_number", nomer2));
                        command.Parameters.Add(new SQLiteParameter("@check", chekn));
                        command.Parameters.Add(new SQLiteParameter("@date_create", date_create));
                        command.Parameters.Add(new SQLiteParameter("@sum", summa));
                        command.Parameters.Add(new SQLiteParameter("@comission", summa_komissia));
                        command.Parameters.Add(new SQLiteParameter("@hash", hesh_id));
                        command.Parameters.Add(new SQLiteParameter("@n1", val1));
                        command.Parameters.Add(new SQLiteParameter("@n3", val3));
                        command.Parameters.Add(new SQLiteParameter("@n5", val5));
                        command.Parameters.Add(new SQLiteParameter("@n10", val10));
                        command.Parameters.Add(new SQLiteParameter("@n20", val20));
                        command.Parameters.Add(new SQLiteParameter("@n50", val50));
                        command.Parameters.Add(new SQLiteParameter("@n100", val100));
                        command.Parameters.Add(new SQLiteParameter("@n200", val200));
                        command.Parameters.Add(new SQLiteParameter("@n500", val500));
                        command.Parameters.Add(new SQLiteParameter("@rate", rate));
                        command.Parameters.Add(new SQLiteParameter("@curr", curr));
                        command.ExecuteNonQuery();

                    }
                }
            } catch (Exception ex) {
                Log.Fatal("Невозможно сохранить данные по платежу", ex);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Возвращает единственный объект Payment
        /// </summary>
        /// <returns>Возвращает объект Payment с незавершенным статусом</returns>
        public static Payment GetSingle() {
            Payment result = null;

            try {
                using (SQLiteConnection connection = new SQLiteConnection()) {
                    using (SQLiteCommand command = new SQLiteCommand()) {

                        connection.ConnectionString = SQLiteDatabase.DbConnection;
                        command.Connection = connection;
                        connection.Open();
                        command.CommandText =
                            "SELECT id, service_id, number, sec_number, [check], date_create,  " +
                            "sum, comission, hash, n1, n3, n5, n10, n20, n50, " +
                            "n100, n200, n500, rate, curr FROM payments WHERE state = 0 LIMIT 1";
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            while (reader.Read()) {
                                result = new Payment {
                                                         id = Int32.Parse(reader[0].ToString()),
                                                         nomer = reader[2].ToString(),
                                                         nomer2 = reader[3].ToString(),
                                                         chekn = reader[4].ToString(),
                                                         hesh_id = reader[8].ToString(),
                                                         curr = reader[19].ToString(),
                                                     };
                                
                                result.date_create = reader[5].ToString();

                                short idUslugi, sum, val1, val3, val5, val10, val20, val50, val100, val200, val500;
                                double commission, rate;

                                Int16.TryParse(reader[1].ToString(), out idUslugi);
                                result.id_uslugi = idUslugi;

                                Int16.TryParse(reader[6].ToString(), out sum);
                                result.summa = sum;

                                double.TryParse(reader[7].ToString(), out commission);
                                result.summa_komissia = commission;

                                Int16.TryParse(reader[9].ToString(), out val1);
                                result.val1 = val1;

                                Int16.TryParse(reader[10].ToString(), out val3);
                                result.val3 = val3;

                                Int16.TryParse(reader[11].ToString(), out val5);
                                result.val5 = val5;

                                Int16.TryParse(reader[12].ToString(), out val10);
                                result.val10 = val10;

                                Int16.TryParse(reader[13].ToString(), out val20);
                                result.val20 = val20;

                                Int16.TryParse(reader[14].ToString(), out val50);
                                result.val50 = val50;

                                Int16.TryParse(reader[15].ToString(), out val100);
                                result.val100 = val100;

                                Int16.TryParse(reader[16].ToString(), out val200);
                                result.val200 = val200;

                                Int16.TryParse(reader[17].ToString(), out val500);
                                result.val500 = val500;

                                double.TryParse(reader[18].ToString(), out rate);
                                result.rate = rate;

                                result.summa_zachis = sum - commission;
                            }
                        }
                    }
                }

            } catch (Exception exception) {
                Log.Error(exception);
            }
            return result;
        }


        /// <summary>
        /// Установить статус платежа на завершенный
        /// </summary>
        /// <param name="id">Номер транзакции</param>
        public static void FinishPayment(string id) {
            try {
                using (SQLiteConnection connection = new SQLiteConnection()) {
                    connection.ConnectionString = SQLiteDatabase.DbConnection;
                    using (SQLiteCommand command = new SQLiteCommand()) {
                        try {
                            command.Connection = connection;
                            command.CommandText = "update payments set state = 1 where hash = @id";
                            command.Parameters.Add(new SQLiteParameter("@id", id));
                            connection.Open();
                            command.ExecuteNonQuery();
                        } catch (Exception exception) {
                            Log.Error("Невозможно обновить статус платежа", exception);
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
