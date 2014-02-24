using System;
using System.Threading;
using System.Windows.Forms;
using Gecko;
using NLog;

namespace Terminal_Firefox {
    internal static class Program {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main() {

            try {
                Log.Info("Запуск приложения...");

                Xpcom.Initialize(@"xul");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Application.ThreadException += new ThreadExceptionEventHandler(ApplicationThreadException);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

                Application.Run(new MainWindow());

            } catch (Exception exception) {
                Log.Fatal(exception);
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            Log.Fatal(e);
        }

        private static void ApplicationThreadException(object sender, System.Threading.ThreadExceptionEventArgs e) {
            Log.Fatal(e);
        }
    }
}
