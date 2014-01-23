using System;
using NLog;
using Newtonsoft.Json.Linq;

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
                string currentCollect = GetCurrentCollect();
                if (!string.IsNullOrEmpty(currentCollect)) {
                    // Set collector and current collection date
                    DBWrapper.Instance.Command.CommandText =
                        "SELECT " +
                                "(n1 * 1 + n3 * 3 + n5 * 5 + n10 * 10 + n20 * 20 + n50 * 50 + n100 * 100 + n200 * 200 + n500 * 500) AS sum " +
                        "FROM collection WHERE collect_id = @collectId";
                    DBWrapper.Instance.Command.Parameters.Add("@collectId", currentCollect);
                    summa = (int)DBWrapper.Instance.Command.ExecuteScalar();
                }
            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }

            date_inkass = DateTime.Now.ToString("yyyy-MM-dd");
            inkass_id = GetCurrentCollect();
        }


        public static string GetCurrentCollect() {
            string value = null;
            try {
                DBWrapper.Instance.Command.CommandText =
                    "SELECT top(1) value FROM settings where variable = 'current_collect_id'";
                value = DBWrapper.Instance.Command.ExecuteScalar().ToString();
            } catch (Exception ex) {
                Log.Error("Невозможно получить номер инкассации", ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }
            return value;
        }

        
        public JObject GetCollectionInfo(string collectId) {

        //        properties = {
        //        date: '2013-01-01', terminal: '001001', address: 'г. Худжанд ул. Гагарина 135', collectNumber: '001001',
        //        collector: '22222', q1: '10', q3: '5', q5: '4', q10: '9', q20: '2', q50: '8', q100: '8', q200: '3', q500: '9'
        //    }

            try {
                if (!string.IsNullOrEmpty(collectId)) {
                    // Set collector and current collection date
                    DBWrapper.Instance.Command.CommandText =
                        "SELECT n1, n3, n5, n10, n20, n50, n100, n200, n500 FROM collection WHERE collect_id = @collectId";
                    DBWrapper.Instance.Command.Parameters.Add("@collectId", collectId);
                }
            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }
            return
                JObject.Parse(
                    @"{date: '2013-01-01', terminal: '001001', address: 'г. Худжанд ул. Гагарина 135', collectNumber: '001001',
                    collector: '22222', q1: '10', q3: '5', q5: '4', q10: '9', q20: '2', q50: '8', q100: '8', q200: '3', q500: '9'
                }");
        }


        public void MakeCollection() {
            try {
                // Set collector and current collection date
                DBWrapper.Instance.Command.CommandText = 
                    "update collection set collect_date = GETDATE(), collector_id = @collector where collect_id = @collectId";
                DBWrapper.Instance.Command.Parameters.Add("@collectId", inkass_id);
                DBWrapper.Instance.Command.Parameters.Add("@collector", collector.Id);
                DBWrapper.Instance.Command.ExecuteNonQuery();

                // Set new collection id
                DBWrapper.Instance.Command.CommandText =
                    "update settings set value=@collectId where variable='current_collect_id'";
                DBWrapper.Instance.Command.Parameters.Add("@collectId", inkass_id + 1);
                DBWrapper.Instance.Command.ExecuteNonQuery();
                
                // Insert new row into collection table
                DBWrapper.Instance.Command.CommandText =
                    "INSERT INTO collection (collect_id) VALUES (@collectId)";
                DBWrapper.Instance.Command.ExecuteNonQuery();

            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }
        }

        public static void InsertBanknote (int val) {
            try {
                DBWrapper.Instance.Command.CommandText = "update collection set n" + val + " = n" + val +
                                                                    " + 1 where collect_id = " + GetCurrentCollect();
                DBWrapper.Instance.Command.ExecuteNonQuery();
            } catch (Exception exception) {
                Log.Fatal(String.Format("Невозможно запистать купюру: {0} в таблицу инкассации", val), exception);            
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }
        }
    }
}
