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

        }


        private void BrowserDomClick(object sender, DomEventArgs e) {
            if (sender == null || e == null || e.Target == null) return;
            var browser = (GeckoWebBrowser) sender;

            var clicked = e.Target.CastToGeckoElement();

            if (clicked != null) {
                if (browser.Document.Title.Equals("1")) {
                    switch (clicked.GetAttribute("data-type")) {
                        case "service":
                            
                            Log.Debug(String.Format("Clicked service with id {0}", clicked.GetAttribute("id")));
                            break;
                        case "top-service":
                            try {
                                _payment.id_uslugi = long.Parse(clicked.GetAttribute("id"));
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
                } else if (browser.Document.Title.Equals("2")) {

                } else if (browser.Document.Title.Equals("3")) {
                    
                } 
            }
        }

        private void AddJSToDom(string textContent) {
            var innerHtml = _browser.Document.CreateElement("script");
            innerHtml.TextContent = textContent;
            _browser.Document.Head.AppendChild(innerHtml);
        }

        #region Original version

        //protected void ModifyElements(GeckoElement element, string tagName, Action<GeckoElement> mod) {
        //    while (element != null) {
        //        if (element.TagName == tagName) {
        //            mod(element);
        //        }
        //        ModifyElements(element.FirstChild as GeckoElement, tagName, mod);
        //        element = (element.NextSibling as GeckoElement);
        //    }

        //var geckoDomElement = browser.Document.DocumentElement;
        //var element = geckoDomElement as GeckoHtmlElement;
        //if (element == null) return;
        //var innerHtml = element.OwnerDocument.CreateHtmlElement("script");
        //element.OwnerDocument.GetElementsByTagName("head")[0].AppendChild(innerHtml);

        //}


        //protected void TestModifyingDom(GeckoWebBrowser browser) {

        //GeckoElement g = browser.Document.DocumentElement;
        //ModifyElements(g, "HEAD", e => {
        //                              var newElement = g.OwnerDocument.CreateElement("script");
        //                              newElement.TextContent = "setMask('926-***-**-**');";
        //                              g.InsertBefore(newElement, e);
        //                          });
        //}


        //protected void DisplayElements(GeckoElement g) {
        //    while (g != null) {
        //        Console.WriteLine("tag = {0} value = {1} attr={2}", g.TagName, g.TextContent, g.Attributes);
        //        DisplayElements(g.FirstChild as GeckoElement);
        //        g = (g.NextSibling as GeckoElement);
        //    }
        //}


        //protected void TestQueryingOfDom(GeckoWebBrowser browser) {
        //    GeckoElement g = browser.Document.DocumentElement;
        //    DisplayElements(g);
        //}
        #endregion
    }
}
