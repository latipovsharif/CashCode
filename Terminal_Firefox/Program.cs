using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gecko;
using NLog;

namespace Terminal_Firefox {
    static class Program {

        private static Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Xpcom.Initialize(@"xul");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
