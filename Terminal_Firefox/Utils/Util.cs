using System;
using System.IO;
using Gecko;
using NLog;

namespace Terminal_Firefox.Utils {

    /// <summary>
    /// Типы команд
    /// </summary>
    public enum CommandTypes {
        Payment = 0, Link = 1, Settings
    }


    public static class Util {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Типы сервисов
        /// 1. Главные кнопки
        /// 2. Кнонки быстрого набора
        /// </summary>
        public enum ServiceTypes {
            MainService = 0,
            Service = 1
        }

        
        public static string GetSubServices(short serviceId, ServiceTypes serviceType) {
            try {
                string path = (serviceType == ServiceTypes.MainService)
                                  ? "/html/js/data/main_buttons/"
                                  : "/html/js/data/services/";
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

        public static void NavigateTo(GeckoWebBrowser browser, CurrentWindow window) {
            string location = @"\html\index.html";
            switch (window) {
                case CurrentWindow.Dependent:
                    location = @"\html\dependent.html";
                    break;
                case CurrentWindow.EnterNumber:
                    location = @"\html\enter_number.html";
                    break;
                case CurrentWindow.Pay:
                    location = @"\html\pay.html";
                    break;
                case CurrentWindow.Encashment:
                    location = @"\html\encashment.html";
                    break;
                case CurrentWindow.MakeEncashment:
                    location = @"\html\make_encashment.html";
                    break;
                case CurrentWindow.BlockTerminal:
                    location = @"\html\terminal_blocked.html";
                    break;
            }
            browser.Navigate(Directory.GetCurrentDirectory() + location);
        }

        public static void AppendImageElement(GeckoWebBrowser browser, string elementId, int path) {
            var innerHtml = browser.Document.CreateElement("img");
            innerHtml.SetAttribute("src", "images/service_logos/" + path + "_m.png");
            innerHtml.SetAttribute("width", "100%");
            innerHtml.SetAttribute("height", "100%");
            browser.Document.GetElementById(elementId).AppendChild(innerHtml);
        }

        public static void AppendText(GeckoWebBrowser browser, string commission, string elementId) {
            browser.Document.GetElementById(elementId).TextContent = commission;
        }
    }
}
