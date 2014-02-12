using System;
using System.Data.SqlServerCe;
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

        public string date_create {
            get { return DateTime.Now.ToString("yyyy-MM-dd"); }
            set { }
        }

        public string date_send {
            get { return DateTime.Now.ToString("yyyy-MM-dd"); }
            set {}
        }

        public int id_inkas {
            get { return TerminalSettings.Instance.CollectionId; }
            set {}
        }

        public short val1 { get; set; }
        public short val3 { get; set; }
        public short val5 { get; set; }
        public short val10 { get; set; }
        public short val20 { get; set; }
        public short val50 { get; set; }
        public short val100 { get; set; }
        public short val200 { get; set; }
        public short val500 { get; set; }

        public string hash_id {
            get { return Guid.NewGuid().ToString(); }
            set { }
        }

        public string checkn { get; set; }
        public double summa_komissia { get; set; }
        public double rate { get; set; }

        public string curr {
            get { return "TJS"; }
            set { }
        }

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


        /// <summary>
        /// Возвращает единственный объект Payment
        /// </summary>
        /// <returns>Возвращает объект Payment с незавершенным статусом</returns>
        public static Payment GetSingle() {
            Payment result = null;
            using (SqlCeConnection connection = new SqlCeConnection()) {
                using (SqlCeCommand command = new SqlCeCommand()) {

                    connection.ConnectionString = DBWrapper.ConnectionString;
                    command.Connection = connection;
                    connection.Open();
                    command.CommandText = "SELECT TOP(1) id, service_id, number, sec_number, [check], date_create,  " +
                                          "sum, comission, hash, n1, n3, n5, n10, n20, n50, " +
                                          "n100, n200, n500, rate, curr FROM payments WHERE state = 0";
                    using (SqlCeDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            result = new Payment {
                                                     id = Int32.Parse(reader[0].ToString()),
                                                     nomer = reader[2].ToString(),
                                                     nomer2 = reader[3].ToString(),
                                                     checkn = reader[4].ToString(),
                                                     date_create = reader[5].ToString(),
                                                     hash_id = reader[8].ToString(),
                                                     curr = reader[19].ToString(),
                                                 };
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
                        }
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Установить статус платежа на завершенный
        /// </summary>
        /// <param name="id">Номер транзакции</param>
        public static void FinishPayment(int id) {
            using (SqlCeConnection connection = new SqlCeConnection()) {
                connection.ConnectionString = DBWrapper.ConnectionString;
                using (SqlCeCommand command = new SqlCeCommand()) {
                    try {
                        command.Connection = connection;
                        command.CommandText = "update payments set status = 1 where id = @id";
                        command.Parameters.Add("@id", id);
                        connection.Open();
                        command.ExecuteNonQuery();
                    } catch (Exception exception) {
                        Log.Error("Невозможно обновить статус платежа", exception);
                    } finally { command.Parameters.Clear();}

                }
            }
        }
    }
}
