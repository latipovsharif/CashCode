using System;
using Newtonsoft.Json;
using Terminal_Firefox.Utils;

namespace Terminal_Firefox.classes {
    class Command {
        public static string Prepare(CommandTypes act, object obj) {
            Message msg = new Message {
                                          login = TerminalSettings.Instance.TerminalNumber,
                                          passw = TerminalSettings.Instance.TerminalPassword,
                                          act = ((int) act).ToString()
                                      };

            switch ((int)act) {
                case 1:
                    msg.body = JsonConvert.SerializeObject(new Link());
                    break;
            }

            return JsonConvert.SerializeObject(msg);
        }

        internal static void HandleAnswer(string p) {
            
        }
    }
}
