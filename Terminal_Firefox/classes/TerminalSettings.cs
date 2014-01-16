using System;
using NLog;

namespace Terminal_Firefox.classes {
    public class TerminalSettings {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        public string AddressRu { get; set; }
        public string AddressEn { get; set; }
        public string AddressTj { get; set; }
        public string TerminalNumber { get; set; }
        public string CallCenter { get; set; }
        public bool OfflineMode { get; set; }
        public int HeartbeatInterval { get; set; }

    }
}
