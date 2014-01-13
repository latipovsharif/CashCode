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
        private short _mainServiceId;

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
                    string dependentServices = Utils.Util.GetSubServices(_mainServiceId);
                    AddJSToDom(dependentServices);
                    AddJSToDom("setProperties();");
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
                            short.TryParse(clicked.GetAttribute("id"), out _mainServiceId);

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
