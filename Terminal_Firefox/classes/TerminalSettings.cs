using System;
using System.Data.SqlServerCe;
using NLog;

namespace Terminal_Firefox.classes {
    public class TerminalSettings {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static readonly TerminalSettings _terminalSettings = new TerminalSettings();

        private string AddressRu { get; set; }
        private string AddressEn { get; set; }
        private string AddressTj { get; set; }
        public string TerminalNumber { get; private set; }
        private string CallCenter { get; set; }
        private bool OfflineMode { get; set; }
        private int HeartbeatInterval { get; set; }
        public string CollectionId { get; set; }
        public string TerminalPassword { get; private set; }
        public static readonly string PublicKey = "asdfghjk";

        private TerminalSettings() {
            try {

                DBWrapper.Instance.Command.CommandText = "select variable, value from settings";
                SqlCeDataReader reader = DBWrapper.Instance.Command.ExecuteReader();
                while (reader.Read()) {
                    switch (reader[0].ToString()) {
                        case "current_collect_id":
                            try {
                                CollectionId = reader[1].ToString();
                            } catch (Exception exception) {
                                Log.Error("Could not get collection id", exception);
                            }
                            break;
                        case "terminal_address":
                            AddressRu = reader[1].ToString();
                            break;
                        case "terminal_number":
                            TerminalNumber = reader[1].ToString();
                            break;
                        case "terminal_password":
                            TerminalPassword = reader[1].ToString();
                            break;
                        case "terminal_id": // Todo delete if doesn't need it
                            break;
                        case "call_center_phone":
                            CallCenter = reader[1].ToString();
                            break;
                    }
                }
            } catch (Exception) {
                Log.Fatal("Невозможно считать настройки терминала");
            }
        }

        public static TerminalSettings Instance {
            get { return _terminalSettings; }
        }

        public string GetSettings() {
            //var property = new JProperty("properties", new JObject(
            //    new JProperty("address", AddressRu), 
            //    new JProperty("call-center", CallCenter), 
            //    new JProperty("terminal-number", TerminalNumber)        //Todo make it right way
            //    ), new JsonSerializerSettings() {});
            //return property.ToString();

            return "var properties = { address: '" + AddressRu + "'," +
                                       "call_center: '" + CallCenter + "', " +
                                       "terminal_number: '" + TerminalNumber + "'}";

        }

    }
}
