using System;
using System.Data.SqlServerCe;
using NLog;

namespace Terminal_Firefox.classes {
    public class Collect {
        public Collector collector = new Collector();
        public string date_inkass { get; set; }
        public double summa { get; set; }
        public string inkass_id { get; set; }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Collect(Collector collector) {
            this.collector = collector;
        }

        public Collect() {
            try {
                // Set collector and current collection date
                DBWrapper.Instance.Command.CommandText =
                    "SELECT n1 + n3 + n5 + n10 + n20 + n50 + n100 + n200 + n500 AS sum FROM collection WHERE collect_id = @collectId";
                DBWrapper.Instance.Command.Parameters.Add("@collectId", currentCollect);
                DBWrapper.Instance.Command.Parameters.Add("@collector", collector);
                DBWrapper.Instance.Command.ExecuteNonQuery();

            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }
        }

        public static int GetCurrentCollect() {
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




        public double Collection(int currentCollect) {
            double value = 0;

            try {
                // Set collector and current collection date
                DBWrapper.Instance.Command.CommandText = 
                    "update collection set collect_date = GETDATE(), collector_id = @collector where collect_id = @collectId";
                DBWrapper.Instance.Command.Parameters.Add("@collectId", currentCollect);
                DBWrapper.Instance.Command.Parameters.Add("@collector", collector.Id);
                DBWrapper.Instance.Command.ExecuteNonQuery();

                // Set new collection id
                DBWrapper.Instance.Command.CommandText =
                    "update settings set value=@val where variable='current_collect_id'";
                DBWrapper.Instance.Command.Parameters.Add("@val", currentCollect + 1);
                DBWrapper.Instance.Command.ExecuteNonQuery();
                
            } catch (Exception ex) {
                Log.Error(ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }

            return value;
        }
    }
}
