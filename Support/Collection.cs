using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Windows;

namespace Support {
    public class Collection {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Date { get; set; }
        public string Collector { get; set; }
        public string State { get; set; }
        public string Sum { get; set; }
        public string Notes { get; set; }

        private const string ConnectionString =
            //@"Data Source=|DataDirectory|terminal.sdf;Password=sdfsafd7897&^^*&;Persist Security Info=True";
            @"Data Source=|DataDirectory|\db\terminal.sdf;Password=sdfsafd7897&^^*&;Persist Security Info=True";

        public void MakeCollection() {
            string collectionId;
            string timestamp = ((Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds).ToString();
            string terminal_number;


            using (SqlCeConnection connection = new SqlCeConnection(ConnectionString)) {
                using (SqlCeCommand command = new SqlCeCommand()) {
                    command.Connection = connection;
                    connection.Open();

                    command.CommandText = "select value from settings where variable='current_collect_id'";
                    collectionId =  command.ExecuteScalar().ToString();

                    command.CommandText =
                        "SELECT n1 + n3 + n5 + n10 + n20 + n50 + n100 + n200 + n500 AS Expr1 FROM collection where collect_id = @collectId";
                    command.Parameters.Add("@collectId", collectionId);
                    int count;
                    Int32.TryParse(command.ExecuteScalar().ToString(), out count);

                    if (count > 0) {
                        command.CommandText = "select value from settings where variable='terminal_number'";
                        terminal_number = command.ExecuteScalar().ToString();

                        string nextCollectionNumber = terminal_number + timestamp;


                        command.CommandText = "update " +
                                                "collection " +
                                              "set " +
                                                "collect_date = GETDATE(), " +
                                                "state = 'False', " +
                                                "collector_id = @collector " +
                                              "where " +
                                                "collect_id = @collectId";

                        command.Parameters.Add("@collector", LoginForm.CollectorId);
                        command.ExecuteNonQuery();

                        command.CommandText = "update settings set value=@nextCollectId where variable='current_collect_id'";
                        command.Parameters.Add("@nextCollectId", nextCollectionNumber);
                        command.ExecuteNonQuery();

                        command.CommandText = "INSERT INTO collection (collect_id) VALUES (@nextCollectId)";
                        command.ExecuteNonQuery();
                    } else {
                        MessageBox.Show("Нет данных для проведения инкассации");
                    }
                }
            }
        }


        public static IEnumerable<Collection> GetCollections() {

            List<Collection> collections = new List<Collection>();

            using (SqlCeConnection connection = new SqlCeConnection(ConnectionString)) {
                using (SqlCeCommand command = new SqlCeCommand()) {
                    command.CommandText =
                        "SELECT id, collect_id, collect_date, collector_id, state, (((((((n1 * 1 + n3 * 3) + n5 * 5) + n10 * 10) + n20 * 20) + n50 * 50) + n100 * 100) + n200 * 200) + n500 * 500 AS sum, n1 + n3 + n5 + n10 + n20 + n50 + n100 + n200 + n500 AS count FROM collection";
                    command.Connection = connection;
                    connection.Open();
                    SqlCeDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        Collection collection = new Collection();
                        collection.Id = int.Parse(reader[0].ToString());
                        collection.Number = reader[1].ToString();
                        collection.Date = reader[2].ToString();
                        collection.Collector = reader[3].ToString();
                        collection.State = reader[4].ToString();
                        collection.Sum = reader[5].ToString();
                        collection.Notes = reader[6].ToString();
                        collections.Add(collection);
                    }
                }
            }
            return collections;
        }

        public static int FindCollector(string login, string password) {
            int id; 
            using (SqlCeConnection connection = new SqlCeConnection(ConnectionString)) {
                using (SqlCeCommand command = new SqlCeCommand()) {
                    command.CommandText = "select id from collector where login=@login and pass=@password";
                    command.Connection = connection;
                    connection.Open();
                    command.Parameters.Add("@login", login);
                    command.Parameters.Add("@password", password);
                    try {
                        int.TryParse(command.ExecuteScalar().ToString(), out id);
                    } catch (Exception) {
                        id = 0;
                    }
                }
            }
            return id;
        }


        private static string set_50(string str1, string str2, string str3) {
            int len2 = str2.Length;
            for (int i = str1.Length; i < 40 - len2; i++) {
                str1 = str1 + str3;
            }

            return str1 + str2;
        }

        private static string set_table50(string str1, string str2, string str3) {

            string str = "|" + str1;

            for (int i = 1; i < 14 - str1.Length; i++) {
                str += " ";
            }
            str += "|" + str2;

            for (int i = 1; i < 13 - str2.Length; i++) {
                str += " ";
            }
            str += "|" + str3;

            for (int i = 1; i < 14 - str3.Length; i++) {
                str += " ";
            }
            str += "|";
            return str;
        }

        private static string set_table_r(string str1, string str2, string str3) {
            string str = "";
            str = "+" + str1;

            for (int i = 1; i < 14 - str1.Length; i++) {
                str += "-";
            }

            str += "+" + str2;

            for (int i = 1; i < 13 - str2.Length; i++) {
                str += "-";
            }

            str += "+" + str3;

            for (int i = 1; i < 14 - str3.Length; i++) {
                str += "-";
            }

            str += "+";
            return str;
        }

        public static string[] get_stat(Collection collection) {
            string[] result = new string[36];

            using (SqlCeConnection connection = new SqlCeConnection(ConnectionString)) {
                using (SqlCeCommand command = new SqlCeCommand()) {
                    command.CommandText =
                        "select variable, value from settings where variable = 'terminal_number' or variable = 'terminal_address'";
                    command.Connection = connection;
                    connection.Open();
                    SqlCeDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        switch (reader[0].ToString()) {
                            case "terminal_number":
                                result[1] = set_50("Терминал №", reader[1].ToString(), ".");
                                break;
                            default:
                                result[2] = set_50("Адрес:", reader[1].ToString(), ".");
                                break;
                        }
                    }
                }
            }

            result[0] = "ОАО \"Банк Эсхата\"";
            
            result[3] = set_50("Дата время:", collection.Date, ".");
            result[4] = set_50("Номер инкасации:", collection.Number, ".");
            result[5] = set_50("Инкассатор:", collection.Collector, ".");
            result[6] = set_50(" ", " ", " ");
            result[7] = set_table_r("", "", "");
            result[8] = set_table50("Купюра", "Количество", "Сумма");
            result[9] = set_table_r("", "", "");

            using (SqlCeConnection connection = new SqlCeConnection(ConnectionString)) {
                using (SqlCeCommand command = new SqlCeCommand()) {
                    command.CommandText =
                        "select n1, n1 * 1, n3, n3 * 3, n5, n5 * 5, n10, n10 * 10, n20, n20 * 20, n50, n50 *50, n100, n100 * 100, n200, n200 * 200, n500, n500 *500 from collection where id = @id";
                    command.Parameters.Add("@id", collection.Id);
                    command.Connection = connection;
                    connection.Open();
                    SqlCeDataReader reader = command.ExecuteReader();
                    while (reader.Read()) {
                        result[10] = set_table50("1 сомони", " " + reader[0], " " + reader[1]);
                        result[11] = set_table_r("", "", "");
                        result[12] = set_table50("3 сомони", " " + reader[2], " " + reader[3]);
                        result[13] = set_table_r("", "", "");
                        result[14] = set_table50("5 сомони", " " + reader[4], " " + reader[5]);
                        result[15] = set_table_r("", "", "");
                        result[16] = set_table50("10 сомони", " " + reader[6], " " + reader[7]);
                        result[17] = set_table_r("", "", "");
                        result[18] = set_table50("20 сомони", " " + reader[8], " " + reader[9]);
                        result[19] = set_table_r("", "", "");
                        result[20] = set_table50("50 сомони", " " + reader[10], " " + reader[11]);
                        result[21] = set_table_r("", "", "");
                        result[22] = set_table50("100 сомони", " " + reader[12], " " + reader[13]);
                        result[23] = set_table_r("", "", "");
                        result[24] = set_table50("200 сомони", " " + reader[14], " " + reader[15]);
                        result[25] = set_table_r("", "", "");
                        result[26] = set_table50("500 сомони", " " + reader[16], " " + reader[17]);
                        result[27] = set_table_r("", "", "");
                    }
                }
            }

            

            result[28] = set_table50("Общ. сумма:", " " + collection.Notes, " " + collection.Sum);
            result[29] = set_table_r("", "", "");
            result[30] = set_table50("Необ. сум:", " ", " ");
            result[31] = set_table_r("", "", "");
            result[32] = set_50(" ", " ", " ");
            result[33] = set_50("Подпись кассира:", "_________", " ");
            result[34] = set_50(" ", " ", " ");
            result[35] = set_50("Подпись техник-инкассатора:", "_________", " ");

            return result;

        }

    }
}
