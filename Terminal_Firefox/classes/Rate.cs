using System;
using NLog;
using System.Data.SqlServerCe;

namespace Terminal_Firefox.classes {
    public class Rate {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static double GetCommissionAmount(int serviceId, int sum) {
            double value = 0;

            try {
                DBWrapper.Instance.Command.CommandText =
                    "select rate, is_percent from comission where service_id = @serviceId and from_sum <= @sum and to_sum >= @sum";
                DBWrapper.Instance.Command.Parameters.Add("@serviceId", serviceId);
                DBWrapper.Instance.Command.Parameters.Add("@sum", sum);
                SqlCeDataReader reader = DBWrapper.Instance.Command.ExecuteReader();
                while(reader.Read()) {
                    if((bool)reader[1]) {
                        value = sum*Convert.ToDouble(reader[0])/100;
                    } else {
                        value = Convert.ToDouble(reader[0]);
                    }
                }
            } catch (Exception ex) {
                Log.Error(String.Format("Невозможно получить комиссию для услуги с id = {0} и суммой в {1}", serviceId, sum), ex);
            } finally {
                DBWrapper.Instance.Command.Parameters.Clear();
            }

            return value;
        }
    }
}
