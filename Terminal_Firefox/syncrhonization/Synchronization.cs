using System;
using System.Threading;
using NLog;
using Terminal_Firefox.Utils;
using Terminal_Firefox.classes;

namespace Terminal_Firefox.syncrhonization {
    public class Synchronization {
        private Payment _payment;
        private readonly Communication _communication = new Communication();
        private CommandTypes _lastCommand;
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        public void Synchronize() {
            try {
                _communication.Autorize();

                while (true) {
                    Thread.Sleep(15000);
                    _payment = Payment.GetSingle();

                    string preparedCommand = Command.Prepare(CommandTypes.Link, new Link());
                    _communication.SSend(preparedCommand);
                    _lastCommand = CommandTypes.Link;

                    if (_payment != null) {
                        preparedCommand = Command.Prepare(CommandTypes.Payment, _payment);
                        Command.HandleAnswer(_communication.SSend(preparedCommand));
                        _lastCommand = CommandTypes.Payment;
                    }
                }
            } catch (Exception exception) {
                Log.Error(exception);
            }
        }
    }
}
