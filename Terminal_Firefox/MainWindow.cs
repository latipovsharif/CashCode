using System;
using System.Configuration;
using System.Threading;
using System.Windows.Forms;
using Gecko;
using Gecko.DOM;
using NLog;
using Terminal_Firefox.classes;
using Terminal_Firefox.peripheral;
using Terminal_Firefox.Utils;

namespace Terminal_Firefox {
    public partial class MainWindow : Form {

        private Payment _payment = new Payment();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly CashCode _cashCode = new CashCode();
        private readonly Thread _thrCashCode;
        private readonly GeckoWebBrowser _browser = new GeckoWebBrowser {Dock = DockStyle.Fill};
        private ushort _currentWindow;
        private short _mainServiceId; // If clicked main service in main window will set the value
        private bool _clickedTopButton; // If service clicked
        private Collector _collector = new Collector();
        private readonly TerminalSettings _terminalSettings = TerminalSettings.Instance;
        private readonly System.Threading.Timer _timer;
        private const ushort TimerDelay = 3000;


        public MainWindow() {
            try {
                InitializeComponent();

                GeckoPreferences.User["extensions.blocklist.enabled"] = false;
                Util.NavigateTo(_browser, CurrentWindow.Main);

                _browser.DomClick += BrowserDomClick;
                _browser.DocumentCompleted += (s, e) => DocumentReady();

                Controls.Add(_browser);

                _timer = new System.Threading.Timer(WaitTimeIsUp);

                _thrCashCode = new Thread(_cashCode.StartPolling) {IsBackground = true, Name = "Poll"};
                _thrCashCode.Start();
                _cashCode.Reset();
                _cashCode.MoneyAcceptedHandler += AcceptedBanknote;
                
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void WaitTimeIsUp(object sender) {
            try {
                switch (_currentWindow) {
                    case (int) CurrentWindow.Main:
                    case (int)CurrentWindow.BlockTerminal:
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        break;
                    case (int) CurrentWindow.Pay:
                        _cashCode.DisableBillTypes();
                        break;
                    default:
                        Action<CurrentWindow> window = NavigateIndex;
                        Invoke(window, CurrentWindow.Main);
                        break;
                }
            } catch (Exception exception) {
                Log.Fatal("Проблемы с таймером", exception);
            }
        }


        private void NavigateIndex(CurrentWindow window) {
            Util.NavigateTo(_browser, window);
        }

        private void AcceptedBanknote(short banknote) {
            try {
                Action<short> action = SetAmount;
                Invoke(action, banknote);
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void SetAmount(short banknote) {
            try {
                Collect.InsertBanknote(banknote);

                switch (banknote) {
                    case 1:
                        _payment.val1 += 1;
                        break;
                    case 3:
                        _payment.val3 += 1;
                        break;
                    case 5:
                        _payment.val5 += 1;
                        break;
                    case 10:
                        _payment.val10 += 1;
                        break;
                    case 20:
                        _payment.val20 += 1;
                        break;
                    case 50:
                        _payment.val50 += 1;
                        break;
                    case 100:
                        _payment.val100 += 1;
                        break;
                    case 200:
                        _payment.val200 += 1;
                        break;
                    case 500:
                        _payment.val500 += 1;
                        break;
                }

                if (_payment.summa == 0) { Util.AddJSToDom(_browser, "enable();"); }

                _payment.summa += banknote;
                _payment.summa_komissia = Rate.GetCommissionAmount(_payment.id_uslugi, _payment.summa);
                _payment.summa_zachis = _payment.summa - _payment.summa_komissia;
                _browser.Document.GetElementById("sum").TextContent =
                    (_payment.summa - _payment.summa_komissia).ToString();
                _browser.Document.GetElementById("banknote").TextContent = banknote.ToString();
                _browser.Document.GetElementById("commission").TextContent = _payment.summa_komissia.ToString();

                _timer.Change(TimerDelay, Timeout.Infinite);
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void DocumentReady() {
            try {
                _currentWindow = ushort.Parse(_browser.Document.Title);

                string property = "";
                string toAppend = "";

                switch (_currentWindow) {
                    case (int) CurrentWindow.Main:
                        toAppend = _terminalSettings.GetSettings();
                        Log.Debug(String.Format("Current window id is {0}", CurrentWindow.Main));
                        break;

                    case (int) CurrentWindow.Dependent:
                        toAppend = Util.GetSubServices(_mainServiceId, Util.ServiceTypes.MainService);
                        Log.Debug(String.Format("Current window id is {0}", CurrentWindow.Dependent));
                        break;
                    case (int) CurrentWindow.EnterNumber:
                        Util.AppendImageElement(_browser, "leftBanner", _payment.id_uslugi);
                        Util.AppendImageElement(_browser, "rightBanner", _payment.id_uslugi);
                        toAppend = Util.GetSubServices(_payment.id_uslugi, Util.ServiceTypes.Service);
                        property = _payment.nomer;
                        Log.Debug(String.Format("Current window id is {0}", CurrentWindow.EnterNumber));
                        break;

                    case (int) CurrentWindow.Pay:
                        var element = (GeckoHtmlElement) _browser.Window.Document.GetElementById("entered-number");
                        string commission = Rate.GetCommissionString(_payment.id_uslugi);
                        Util.AppendText(_browser, commission, "leftBanner");
                        // Ensure that entered number is not technical number for encashment
                        if (_payment.nomer.Equals(ConfigurationManager.AppSettings["encashmentCode"])) {
                            Util.NavigateTo(_browser, CurrentWindow.Encashment);
                            return;
                        }

                        element.TextContent = _payment.nomer;
                        _cashCode.EnableBillTypes();
                        Log.Debug(String.Format("Current window id is {0}", CurrentWindow.Pay));
                        break;
                }
                Util.AddJSToDom(_browser, toAppend);
                Util.AddJSToDom(_browser, "setProperties('" + property + "');");

                _timer.Change(TimerDelay, Timeout.Infinite);

            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void BrowserDomClick(object sender, DomEventArgs e) {
            try {
                if (sender == null || e == null || e.Target == null) return;

                var clicked = e.Target.CastToGeckoElement();

                if (clicked == null) return;

                switch (_currentWindow) {
                    case (int) CurrentWindow.Main:
                        MainClick(clicked);
                        break;
                    case (int) CurrentWindow.Dependent:
                        DependentClick(clicked);
                        break;
                    case (int) CurrentWindow.EnterNumber:
                        EnterNumberClick(clicked);
                        _timer.Change(TimerDelay, Timeout.Infinite);
                        break;
                    case (int) CurrentWindow.Pay:
                        PayClick(clicked);
                        break;
                    case (int) CurrentWindow.Encashment:
                        EncashmentClick(clicked);
                        break;
                    case (int) CurrentWindow.MakeEncashment:
                        MakeEncashmentClick(clicked);
                        break;
                }
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void MainClick(GeckoElement clicked) {
            try {
                if (clicked.HasAttribute("data-type")) {
                    switch (clicked.GetAttribute("data-type")) {
                        case "service":
                            short.TryParse(clicked.GetAttribute("id"), out _mainServiceId);
                            Log.Debug(String.Format("Clicked service with id {0}", clicked.GetAttribute("id")));
                            Util.NavigateTo(_browser, CurrentWindow.Dependent);
                            _clickedTopButton = false;
                            break;
                        case "top-service":
                            try {
                                _payment.id_uslugi = short.Parse(clicked.GetAttribute("id"));
                                Log.Debug(String.Format("Clicked top-service with id {0}", clicked.GetAttribute("id")));
                                Util.NavigateTo(_browser, CurrentWindow.EnterNumber);
                                _clickedTopButton = true;
                            } catch (Exception ex) {
                                Log.Error("Incorrect service id format", ex, clicked);
                            }
                            break;
                        default:
                            Log.Debug("Clicked element was null");
                            return;
                    }
                } else if (clicked.HasAttribute("language")) {

                }
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void DependentClick(GeckoElement clicked) {
            try {
                if (clicked.HasAttribute("id")) {
                    _payment.id_uslugi = short.Parse(clicked.GetAttribute("id"));
                    Log.Debug(String.Format("Clicked service with id {0}", clicked.GetAttribute("id")));
                    Util.NavigateTo(_browser, CurrentWindow.EnterNumber);
                }
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void EnterNumberClick(GeckoElement clicked) {
            try {
                if (clicked.HasAttribute("id")) {
                    if (clicked.GetAttribute("id").Equals("next") && !clicked.HasAttribute("disabled")) {
                        var input = (GeckoInputElement) _browser.Document.GetElementById("number");
                        _payment.nomer = input.Value;
                        Log.Debug(String.Format("Entered number {0}", input.Value));
                        Util.NavigateTo(_browser, CurrentWindow.Pay);

                    } else if (clicked.GetAttribute("id").Equals("back") && !clicked.HasAttribute("disabled")) {
                        _payment.nomer = "";
                        _payment.nomer2 = "";

                        Util.NavigateTo(_browser,
                                        _clickedTopButton
                                            ? CurrentWindow.Main
                                            : CurrentWindow.Dependent);
                    }
                }
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void PayClick(GeckoElement clicked) {
            try {
                if (clicked.HasAttribute("id")) {
                    if (clicked.GetAttribute("id").Equals("next") && !clicked.HasAttribute("disabled")) {
                        _payment.Save();
                        _payment = new Payment();
                        Util.NavigateTo(_browser, CurrentWindow.Main);
                    } else if (clicked.GetAttribute("id").Equals("back") && !clicked.HasAttribute("disabled")) {
                        _cashCode.DisableBillTypes();
                        Util.NavigateTo(_browser, CurrentWindow.EnterNumber);
                    }
                }
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void EncashmentClick(GeckoElement clicked) {
            try {
                if (clicked.HasAttribute("id")) {
                    if (clicked.GetAttribute("id").Equals("next") && !clicked.HasAttribute("disabled")) {
                        var login = (GeckoInputElement) _browser.Document.GetElementById("login");
                        var password = (GeckoInputElement) _browser.Document.GetElementById("password");
                        _collector = Collector.FindCollector(login.Value, password.Value);
                        if (_collector.Id <= 0) {
                            MessageBox.Show("Неправильный логин и/или пароль");
                        } else {
                            Util.NavigateTo(_browser, CurrentWindow.MakeEncashment);
                        }
                    } else if (clicked.GetAttribute("id").Equals("back") && !clicked.HasAttribute("disabled")) {
                        Util.NavigateTo(_browser, CurrentWindow.Main);
                    }
                }
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }


        private void MakeEncashmentClick(GeckoElement clicked) {
            try {
                if (clicked.HasAttribute("id")) {
                    if (clicked.GetAttribute("id").Equals("next") && !clicked.HasAttribute("disabled")) {
                    } else if (clicked.GetAttribute("id").Equals("back") && !clicked.HasAttribute("disabled")) {
                        Util.NavigateTo(_browser, CurrentWindow.Main);
                    }
                }
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }
    }
}
