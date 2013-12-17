using System;
using System.Windows.Forms;
using Gecko;
using NLog;

namespace Terminal_Firefox
{
    internal static class Program
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {

            Log.Info("Запуск приложения...");

            Xpcom.Initialize(@"xul");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(ApplicationThreadException);
            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomainUnhandledException);

            Application.Run(new Form1());
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal(e);
        }

        private static void ApplicationThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Log.Fatal(e);
        }
    }
}
