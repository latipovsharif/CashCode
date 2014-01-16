﻿using System;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using Gecko;
using Gecko.DOM;
using NLog;
using Terminal_Firefox.Utils;
using Terminal_Firefox.classes;
using Terminal_Firefox.peripheral;


namespace Terminal_Firefox {
    public partial class MainWindow : Form {


        // Database password 'sdfsafd7897&^^*&'

        private Payment _payment = new Payment();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly CashCode _cashCode = new CashCode();
        private readonly Thread _thrCashCode;
        private readonly GeckoWebBrowser _browser = new GeckoWebBrowser {Dock = DockStyle.Fill};
        private ushort _currentWindow;
        private ushort _previousWindow;
        private short _mainServiceId;
        private short _sum;

        public MainWindow() {
            InitializeComponent();

            _thrCashCode = new Thread(_cashCode.StartPolling) { IsBackground = true, Name = "Poll" };
            _thrCashCode.Start();
            _cashCode.Reset();
            _cashCode.MoneyAcceptedHandler += AcceptedBanknote;

            GeckoPreferences.User["extensions.blocklist.enabled"] = false;
            NavigateIndex();

            _browser.DomClick += BrowserDomClick;
            _browser.DocumentCompleted += (s, e) => DocumentReady();

            Controls.Add(_browser);
        }


        private void AcceptedBanknote(short banknote) {
            Action<short> action = SetAmount;
            Invoke(action, banknote);
        }


        private void SetAmount(short banknote) {
            _sum += banknote;
            _browser.Document.GetElementById("sum").TextContent = _sum.ToString();
            _browser.Document.GetElementById("banknote").TextContent = banknote.ToString();
        }


        private void DocumentReady() {
            _currentWindow = ushort.Parse(_browser.Document.Title);
            bool isBack = _currentWindow < _previousWindow;
            _previousWindow = _currentWindow;
            string property = "";
            string toAppend="";

            switch (_currentWindow) {
                case (int) CurrentWindow.MainWindow:
                    Log.Debug(String.Format("Current window id is {0}", CurrentWindow.MainWindow));
                    break;
                case (int) CurrentWindow.DependentWindow:
                    toAppend = Util.GetSubServices(_mainServiceId, Util.ServiceTypes.MainService);
                    Log.Debug(String.Format("Current window id is {0}", CurrentWindow.DependentWindow));
                    break;
                case (int) CurrentWindow.EnterNumberWindow:
                    toAppend = Util.GetSubServices(_payment.id_uslugi, Util.ServiceTypes.Service);
                    property = _payment.nomer;
                    Log.Debug(String.Format("Current window id is {0}", CurrentWindow.EnterNumberWindow));
                    break;
                case (int) CurrentWindow.PayWindow:
                    var element = (GeckoHtmlElement)_browser.Window.Document.GetElementById("entered-number");
                    element.TextContent = _payment.nomer;
                    _cashCode.EnableBillTypes();
                    Log.Debug(String.Format("Current window id is {0}", CurrentWindow.PayWindow));
                    break;
            }
            Util.AddJSToDom(_browser, toAppend);
            Util.AddJSToDom(_browser, "setProperties('" + property + "');");
        }


        private void BrowserDomClick(object sender, DomEventArgs e) {
            if (sender == null || e == null || e.Target == null) return;

            var clicked = e.Target.CastToGeckoElement();

            if (clicked == null) return;

            switch (_currentWindow) {
                case (int) CurrentWindow.MainWindow:
                    switch (clicked.GetAttribute("data-type")) {
                        case "service":
                            short.TryParse(clicked.GetAttribute("id"), out _mainServiceId);
                            Log.Debug(String.Format("Clicked service with id {0}", clicked.GetAttribute("id")));
                            break;
                        case "top-service":
                            try {
                                _payment.id_uslugi = short.Parse(clicked.GetAttribute("id"));
                                Log.Debug(String.Format("Clicked top-service with id {0}", clicked.GetAttribute("id")));
                            } catch (Exception ex) {
                                Log.Error("Incorrect service id format", ex, clicked);
                                NavigateIndex();
                            }
                            break;
                        default:
                            Log.Debug("Clicked element was null");
                            return;
                    }
                    break;
                case (int) CurrentWindow.DependentWindow:
                    break;
                case (int) CurrentWindow.EnterNumberWindow:
                    if (clicked.GetAttribute("id") == null) return;
                    if(clicked.GetAttribute("id").Equals("next")) {
                        var input = (GeckoInputElement)_browser.Document.GetElementById("number");
                        _payment.nomer = input.Value;
                        Log.Debug(String.Format("Entered number {0}", input.Value));
                    }
                    break;
                case (int) CurrentWindow.PayWindow:
                    break;
            }
        }


        private void NavigateIndex() {
            _browser.Navigate(Directory.GetCurrentDirectory() + @"\html\index.html");
        }
    }
}