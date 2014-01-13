using System;
using System.Globalization;
using System.IO;
using NLog;

namespace Terminal_Firefox.Utils {
    public static class Util {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static string GetSubServices(short serviceId) {
            try {
                return File.ReadAllText(Directory.GetCurrentDirectory() +
                    "/html/js/data/" + serviceId.ToString(CultureInfo.InvariantCulture) + ".js");
            }
            catch (Exception ex) {
                Log.Error(String.Format("Service file with id {0} does not found", serviceId), ex);
            }
            return "";
        }

        public static short GetServiceId() {
            return 0;
        }
    }
}
