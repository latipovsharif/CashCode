using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Gecko;
using NLog;


namespace Terminal_Firefox {
    public partial class Form1 : Form {

        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public Form1() {
            InitializeComponent();

            var browser = new GeckoWebBrowser();

            browser.Dock = DockStyle.Fill;
            GeckoPreferences.User["extensions.blocklist.enabled"] = false;
            //browser.Navigate("http://www.macromedia.com/support/documentation/en/flashplayer/help/settings_manager04.html");
            browser.Navigate(@"C:\Users\Sh_Latipov\Documents\Visual Studio 2010\Projects\Terminal_Firefox\Terminal_Firefox\bin\Debug\html\test.html");

            //// add a handler showing how to view the DOM
            // browser.DocumentCompleted += (s, e) => TestQueryingOfDom(browser);

            browser.DomClick += new EventHandler<DomEventArgs>(browser_DomClick);
            //// add a handler showing how to modify the DOM.
            // browser.DocumentCompleted += (s, e) => TestModifyingDom(browser);

            logger.Trace("test");
            this.Controls.Add(browser);
        }

        void browser_DomClick(object sender, DomEventArgs e) {
            if (sender != null && e != null && e.Target != null) {
                Gecko.GeckoElement clicked = e.Target.CastToGeckoElement();
                if (clicked != null && clicked.GetAttribute("id") != null) {
                    try {
                        logger.Trace(clicked);
                    } catch (Exception ex){
                        
                        logger.Error(ex);
                    }
                    //MessageBox.Show( clicked.GetAttribute("id"));
                }
            }
        }

        protected void ModifyElements(GeckoElement element, string tagName, Action<GeckoElement> mod) {
            while (element != null) {
                if (element.TagName == tagName) {
                    mod(element);
                }
                ModifyElements(element.FirstChild as GeckoElement, tagName, mod);
                element = (element.NextSibling as GeckoElement);
            }
        }

        protected void TestModifyingDom(GeckoWebBrowser browser) {
            GeckoElement g = browser.Document.DocumentElement;
            ModifyElements(g, "BODY", e => {
                                for (int i = 1; i < 4; ++i) {
                                    var newElement = g.OwnerDocument.CreateElement(String.Format("h{0}", i));
                                    newElement.TextContent = "Geckofx added this text.";
                                    g.InsertBefore(newElement, e);
                                }
                            });
        }

        protected void DisplayElements(GeckoElement g) {
            while (g != null) {
                Console.WriteLine("tag = {0} value = {1} attr={2}", g.TagName, g.TextContent, g.Attributes);
                DisplayElements(g.FirstChild as GeckoElement);
                g = (g.NextSibling as GeckoElement);
            }

        }

        protected void TestQueryingOfDom(GeckoWebBrowser browser) {
            GeckoElement g = browser.Document.DocumentElement;
            DisplayElements(g);
        }
        

    }
}
