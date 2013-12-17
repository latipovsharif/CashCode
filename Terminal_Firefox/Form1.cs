using System;
using System.Windows.Forms;
using Gecko;
using NLog;


namespace Terminal_Firefox {
    public partial class Form1 : Form {

        private static Logger Log = LogManager.GetCurrentClassLogger();
        
        public Form1() {
            InitializeComponent();

            var browser = new GeckoWebBrowser {Dock = DockStyle.Fill};

            GeckoPreferences.User["extensions.blocklist.enabled"] = false;
            browser.Navigate(@"D:\development\vs\Terminal_Firefox\Terminal_Firefox\bin\Debug\html\index.html");

            //// add a handler showing how to view the DOM
            // browser.DocumentCompleted += (s, e) => TestQueryingOfDom(browser);

            browser.DomClick += new EventHandler<DomEventArgs>(BrowserDomClick);
            //// add a handler showing how to modify the DOM.
            // browser.DocumentCompleted += (s, e) => TestModifyingDom(browser);

            //logger.Trace("test");
            Controls.Add(browser);
        }

        private static void BrowserDomClick(object sender, DomEventArgs e)
        {
            
            if (sender == null || e == null || e.Target == null) return;
            
            var clicked = e.Target.CastToGeckoElement();
            
            if (clicked == null || clicked.GetAttribute("id") == null) return;
            
            try {
                Log.Trace(clicked);
            } catch (Exception ex){
                Log.Error(ex);
            }
        }

        //protected void ModifyElements(GeckoElement element, string tagName, Action<GeckoElement> mod) {
        //    while (element != null) {
        //        if (element.TagName == tagName) {
        //            mod(element);
        //        }
        //        ModifyElements(element.FirstChild as GeckoElement, tagName, mod);
        //        element = (element.NextSibling as GeckoElement);
        //    }
        //}

        //protected void TestModifyingDom(GeckoWebBrowser browser) {
        //    GeckoElement g = browser.Document.DocumentElement;
        //    ModifyElements(g, "BODY", e => {
        //                        for (int i = 1; i < 4; ++i) {
        //                            var newElement = g.OwnerDocument.CreateElement(String.Format("h{0}", i));
        //                            newElement.TextContent = "Geckofx added this text.";
        //                            g.InsertBefore(newElement, e);
        //                        }
        //                    });
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
        

    }
}
