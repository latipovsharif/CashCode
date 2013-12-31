using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Gecko;
using NLog;
using Terminal_Firefox.classes;
using Terminal_Firefox.peripheral;


namespace Terminal_Firefox {
    public partial class MainWindow : Form {

        private Payment _payment = new Payment();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private CashCode _cashCode = new CashCode();
        private readonly Thread _thrCashCode;
        private GeckoWebBrowser _browser = new GeckoWebBrowser { Dock = DockStyle.Fill };
        private ushort _currentWindow = 0;

        public MainWindow() {
            InitializeComponent();

            //_thrCashCode = new Thread(_cashCode.StartPolling) {IsBackground = true, Name = "Poll"};
            //_thrCashCode.Start();
            //_cashCode.Reset();


            GeckoPreferences.User["extensions.blocklist.enabled"] = false;

            NavigateIndex();

            _browser.DomClick += BrowserDomClick;
            _browser.DocumentCompleted += (s, e) => DocumentReady();

            Controls.Add(_browser);
        }

        private void NavigateIndex() {
            _browser.Navigate(Directory.GetCurrentDirectory() + @"\html\index.html");
        }

        private void DocumentReady() {
            _currentWindow = ushort.Parse(_browser.Document.Title);

            switch (_currentWindow) {
                case (int)CurrentWindow.MainWindow:
                    break;
                case (int)CurrentWindow.DependentWindow:
                    AddJSToDom(
                        "var dependent = {'dependent': " +
                            "[{ 'image': 'images/service_logos/1.png', 'class': 'top-img', 'id': '1' }," +
                            "{ 'image': 'images/service_logos/4.png', 'class': 'top-img', 'id': '4' }," +
                            "{ 'image': 'images/service_logos/5.png', 'class': 'top-img', 'id': '5' }," +
                            "{ 'image': 'images/service_logos/7.png', 'class': 'top-img', 'id': '7' }," +
                            "{ 'image': 'images/service_logos/8.png', 'class': 'top-img', 'id': '8' }," +
                            "{ 'image': 'images/service_logos/9.png', 'class': 'top-img', 'id': '9' }," +
                            "{ 'image': 'images/service_logos/10.png', 'class': 'top-img', 'id': '10' }," +
                            "{ 'image': 'images/service_logos/11.png', 'class': 'top-img', 'id': '11' }," +
                            "{ 'image': 'images/service_logos/12.png', 'class': 'top-img', 'id': '12' }," +
                            "{ 'image': 'images/service_logos/16.png', 'class': 'top-img', 'id': '16' }," +
                            "{ 'image': 'images/service_logos/17.png', 'class': 'top-img', 'id': '17' }," +
                            "{ 'image': 'images/service_logos/18.png', 'class': 'top-img', 'id': '18' }," +
                            "{ 'image': 'images/service_logos/19.png', 'class': 'top-img', 'id': '19' }]};" +
                            "setProperties();");
                    break;
                case (int)CurrentWindow.EnterNumberWindow:
                    break;
                case (int)CurrentWindow.PayWindow:
                    break;
            }

            Log.Debug(String.Format("Current window id is {0}", _currentWindow));

        }


        private void BrowserDomClick(object sender, DomEventArgs e) {
            if (sender == null || e == null || e.Target == null) return;
            
            var clicked = e.Target.CastToGeckoElement();

            if (clicked != null) {
                Log.Debug(String.Format("Current window id is {0}", _currentWindow));
                if ((int)CurrentWindow.MainWindow == _currentWindow) {
                    switch (clicked.GetAttribute("data-type")) {
                        case "service":
                            Log.Debug(String.Format("Clicked service with id {0}", clicked.GetAttribute("id")));
                            break;
                        case "top-service":
                            try {
                                Log.Debug(String.Format("Clicked top-service with id {0}", clicked.GetAttribute("id")));
                            }
                            catch (Exception ex) {
                                Log.Error("Incorrect service id format", ex, clicked);
                                NavigateIndex();
                            }
                            break;
                        default:
                            Log.Debug("Clicked element was null");
                            return;
                    }
                } else if ((int)CurrentWindow.DependentWindow == _currentWindow) {

                } else if ((int)CurrentWindow.EnterNumberWindow == _currentWindow) {
                    
                } else if ((int)CurrentWindow.PayWindow == _currentWindow) {
                    
                } 
            }
        }

        private void AddJSToDom(string textContent) {
            var innerHtml = _browser.Document.CreateElement("script");
            innerHtml.TextContent = textContent;
            _browser.Document.Head.AppendChild(innerHtml);
        }
    }
}
