using System;
using System.IO;
using Gecko;
using NLog;

namespace Terminal_Firefox.Utils {
    public static class Util {



        public enum ServiceTypes {
            MainService = 0,
            Service = 1
        }



       

        //int a;
        //_browser.AddMessageEventListener("previous", s => a = int.Parse(s));
        //(GeckoHtmlElement)_browser.Window.Document.GetElementById("number").InnerHtml = myString;


        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static string GetSubServices(short serviceId, ServiceTypes serviceType) {
            try {
                string path = (serviceType == ServiceTypes.MainService) ? 
                    "/html/js/data/main_buttons/" : "/html/js/data/services/";
                return File.ReadAllText(Directory.GetCurrentDirectory() + path + serviceId + ".js");
            } catch (Exception ex) {
                Log.Error(String.Format("Service file with id {0} does not found", serviceId), ex);
            }
            return "";
        }

        public static void AddJSToDom(GeckoWebBrowser browser, string textContent) {
            var innerHtml = browser.Document.CreateElement("script");
            innerHtml.TextContent = textContent;
            browser.Document.Head.AppendChild(innerHtml);
        }

        public static double GetCommission(int amount, int serviceId) {
            return 1;
        }
    }
}
