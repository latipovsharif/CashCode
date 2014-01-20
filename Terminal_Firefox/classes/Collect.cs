using System;
using NLog;

namespace Terminal_Firefox.classes {
    public class Collect {
        public Collector collector = new Collector();
        public string date_inkass { get; set; }
        public int summa { get; set; }
        public int inkass_id { get; set; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    
        public Collect(Collector collector) {
            this.collector = collector;
            try {
                int currentCollect = GetCurrentCollect();
                if (currentCollect > 0) {
                    // Set collector and current collection date
                    DBWrapper.Instance.Command.CommandText =
                        "SELECT " +
                                "(n1 * 1 + n3 * 3 + n5 * 5 + n10 * 10 + n20 * 20 + n50 * 50 + n100 * 100 + n200 * 200 + n500 * 500) AS sum " +
                        "FROM collection WHERE collect_id = @collectId";
                    DBWrapper.Instance.Command.Parameters.Add("@collectId", currentCollect);
                }
            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }

            summa = (int)DBWrapper.Instance.Command.ExecuteScalar();
            date_inkass = DateTime.Now.ToString("yyyy-MM-dd");
            inkass_id = GetCurrentCollect();
        }

        private static int GetCurrentCollect() {
            int value = 0;
            try {
                DBWrapper.Instance.Command.CommandText =
                    "SELECT top(1) value FROM settings where variable = 'current_collect_id'";
                int.TryParse(DBWrapper.Instance.Command.ExecuteScalar().ToString(), out value);
            } catch (Exception ex) {
                Log.Error("Невозможно получить номер инкассации", ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }
            return value;
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
    }
}
