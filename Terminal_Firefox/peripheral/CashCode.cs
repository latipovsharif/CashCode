using System;
using System.IO.Ports;
using System.Threading;
using System.Configuration;
using NLog;

namespace Terminal_Firefox.peripheral {
    
    public delegate void MoneyAccepted(short money);

    public class CashCode {

        public event MoneyAccepted MoneyAcceptedHandler;

        private void OnMoneyAccepted(short money) {
            MoneyAccepted handler = MoneyAcceptedHandler;
            if (handler != null) handler(money);
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly SerialPort _port;
        private const int POLYNOMIAL = 0x08408; // CRC 
        private bool _flag = true; // Enabling and disabling polling
        private static bool _newCommand;
        private static byte[] _command;

        /* CONSTANTS FOR BIL VALIDATOR */

        private const byte Sync = 0x02;
        private const byte DeviceType = 0x03;

        /* CONSTANTS FOR BIL VALIDATOR */


        public CashCode() {
            try {
                Log.Info("Открываю порт");
                _port = new SerialPort(ConfigurationManager.AppSettings["com"], 9600, Parity.None, 8, StopBits.One);
                _port.Open();
                _port.WriteTimeout = 400;
                _port.ReadTimeout = 400;
            }
            catch (Exception ex) {
                Log.Fatal(String.Format("Невозможно открыть порт {0}", ConfigurationManager.AppSettings["com"]), ex);
            }
        }

        private byte[] PrepareCommand(CashCodeCommands command) {
            const byte dataLength = 0x06;
            byte[] preparedCommand = {Sync, DeviceType, dataLength, (byte) command, 0x00, 0x00};
            char crc = CalculateCrc(preparedCommand, 4);

            preparedCommand[4] = (byte) crc;
            crc = CalculateCrc(preparedCommand, 5);
            preparedCommand[5] = (byte) crc;
            return preparedCommand;
        }

        private byte[] PrepareLongCommand(CashCodeCommands command, byte first, byte last) {
            const byte dataLength = 0x0C;

            byte[] preparedCommand = {
                                         Sync, DeviceType, dataLength, (byte) command, first, first, first, last, last,
                                         last, 0x00, 0x00
                                     };
            char crc = CalculateCrc(preparedCommand, 10);

            preparedCommand[10] = (byte) crc;
            crc = CalculateCrc(preparedCommand, 11);
            preparedCommand[11] = (byte) crc;
            return preparedCommand;
        }

        public void StartPolling() {
            if (_port != null && _port.IsOpen) {
                byte[] preparedCommand = PrepareCommand(CashCodeCommands.Poll);
                while (_flag) {
                    WriteToPort(preparedCommand);
                    var bytes = ReadFromPort();
                    if (bytes != null && bytes.Length > 0) {
                        ParseMessage(bytes);
                        if (!_newCommand) continue;
                        WriteToPort(_command);
                        bytes = ReadFromPort();
                        ParseMessage(bytes);
                        _newCommand = false;
                    }
                }
            } else {
                // Todo block terminal
            }
        }

        private char CalculateCrc(byte[] data, int length) {
            char crc = (char) 0x0000;
            if (length < 4) return (char) 0;
            for (int i = 0; i < length; i++) {
                crc ^= (char) data[i];
                for (int j = 0; j < 8; j++) {
                    if ((crc & 0x0001) == 1) {
                        crc >>= 1;
                        crc ^= (char) POLYNOMIAL;
                    }
                    else crc >>= 1;
                }
            }
            return crc;
        }

        private void WriteToPort(byte[] data) {
            _port.WriteTimeout = 30;
            _port.DiscardOutBuffer();
            _port.Write(data, 0, data.Length);
        }

        private byte[] ReadFromPort() {
            try {
                Thread.Sleep(100);
                var answer = new byte[_port.BytesToRead];
                _port.Read(answer, 0, _port.BytesToRead);
                return answer;
            } catch(Exception exception) {
                Log.Error("Не могу прочитать данные из порта", exception);
            }
            return null;
        }

        private void ParseMessage(byte[] data) {

            if (data.Length < 5 || CalculateCrc(data, data.Length - 1) != data[data.Length - 1]) {
                _command = PrepareCommand(CashCodeCommands.NAckResponse);
                _newCommand = true;
                return;
            }

            switch (data[3]) {
                case (byte) CashCodePollResponces.PowerUp:
                    Log.Debug("GenericRejectionCodes.PowerUp", data);
                    break;
                case (byte) CashCodePollResponces.PowerUpWithBillInValidator:
                    Log.Debug("GenericRejectionCodes.PowerUpWithBillInValidator", data);
                    break;
                case (byte) CashCodePollResponces.PowerUpWithBillInStacker:
                    Log.Debug("GenericRejectionCodes.PowerUpWithBillInStacker", data);
                    break;
                case (byte) CashCodePollResponces.Initialize:
                    Log.Debug("GenericRejectionCodes.Initialize", data);
                    break;
                case (byte) CashCodePollResponces.Idling:
                    Log.Debug("GenericRejectionCodes.Idling", data);
                    break;
                case (byte) CashCodePollResponces.Accepting:
                    Log.Debug("GenericRejectionCodes.Accepting", data);
                    break;
                case (byte) CashCodePollResponces.Stacking:
                    Log.Debug("GenericRejectionCodes.Stacking", data);
                    break;
                case (byte) CashCodePollResponces.Returning:
                    Log.Debug("GenericRejectionCodes.Returning", data);
                    break;
                case (byte) CashCodePollResponces.UnitDisabled:
                    Log.Debug("GenericRejectionCodes.UnitDisabled", data);
                    break;
                case (byte) CashCodePollResponces.Holding:
                    Log.Debug("GenericRejectionCodes.Holding", data);
                    break;
                case (byte) CashCodePollResponces.DeviceBusy:
                    Log.Debug("GenericRejectionCodes.DeviceBusy", data);
                    break;
                case (byte) CashCodePollResponces.GenericRejecting:
                    RejectionReason(data[4]);
                    break;
                case (byte) CashCodePollResponces.DropCasseteFull:
                    Log.Debug("GenericRejectionCodes.DropCasseteFull", data);
                    break;
                case (byte) CashCodePollResponces.DropCasseteOutOfPosition:
                    Log.Debug("GenericRejectionCodes.DropCasseteOutOfPosition", data);
                    break;
                case (byte) CashCodePollResponces.ValidatorJammed:
                    Log.Debug("GenericRejectionCodes.ValidatorJammed", data);
                    break;
                case (byte) CashCodePollResponces.DropCasseteJammed:
                    Log.Debug("GenericRejectionCodes.DropCasseteJammed", data);
                    break;
                case (byte) CashCodePollResponces.Cheated:
                    Log.Debug("GenericRejectionCodes.Cheated", data);
                    break;
                case (byte) CashCodePollResponces.Pause:
                    Log.Debug("GenericRejectionCodes.Pause", data);
                    break;
                case (byte) CashCodePollResponces.GenericFailure:
                    FailureReason(data[4]);
                    break;
                case (byte) CashCodePollResponces.BillStacked:
                    FindBillDenomination(data[4], "Принята");
                    break;
                case (byte) CashCodePollResponces.BillReturned:
                    FindBillDenomination(data[4], "Возвращена");
                    break;
            }
        }

        private void RejectionReason(byte data) {
            switch (data) {
                case (byte) GenericRejectionCodes.DueToInsertation:
                    Log.Debug("GenericRejectionCodes.DueToInsertation", data);
                    break;
                case (byte) GenericRejectionCodes.DueToMagnetic:
                    Log.Debug("GenericRejectionCodes.DueToMagnetic", data);
                    break;
                case (byte) GenericRejectionCodes.DueToRemainedBillInHead:
                    Log.Debug("GenericRejectionCodes.DueToRemainedBillInHead", data);
                    break;
                case (byte) GenericRejectionCodes.DueToMultiplying:
                    Log.Debug("GenericRejectionCodes.DueToMultiplying", data);
                    break;
                case (byte) GenericRejectionCodes.DueToConveying:
                    Log.Debug("GenericRejectionCodes.DueToConveying", data);
                    break;
                case (byte) GenericRejectionCodes.DueToIdentification:
                    Log.Debug("GenericRejectionCodes.DueToIdentification", data);
                    break;
                case (byte) GenericRejectionCodes.DueToVerification:
                    Log.Debug("GenericRejectionCodes.DueToVerification", data);
                    break;
                case (byte) GenericRejectionCodes.DueToOptic:
                    Log.Debug("GenericRejectionCodes.DueToOptic", data);
                    break;
                case (byte) GenericRejectionCodes.DueToInhibit:
                    Log.Debug("GenericRejectionCodes.DueToInhibit", data);
                    break;
                case (byte) GenericRejectionCodes.DueToCapacity:
                    Log.Debug("GenericRejectionCodes.DueToCapacity", data);
                    break;
                case (byte) GenericRejectionCodes.DueToOperation:
                    Log.Debug("GenericRejectionCodes.DueToOperation", data);
                    break;
                case (byte) GenericRejectionCodes.DueToLenght:
                    Log.Debug("GenericRejectionCodes.DueToLenght", data);
                    break;
            }
        }

        private void FailureReason(byte data) {
            switch (data) {
                case (byte) GenericFailureCodes.StackMotor:
                    Log.Debug("GenericFailureCodes.StackMotor", data);
                    break;
                case (byte) GenericFailureCodes.TransportMotorSpeed:
                    Log.Debug("GenericFailureCodes.TransportMotorSpeed", data);
                    break;
                case (byte) GenericFailureCodes.TransportMotor:
                    Log.Debug("GenericFailureCodes.TransportMotor", data);
                    break;
                case (byte) GenericFailureCodes.AligningMotor:
                    Log.Debug("GenericFailureCodes.AligningMotor", data);
                    break;
                case (byte) GenericFailureCodes.InitialCassetteStatus:
                    Log.Debug("GenericFailureCodes.InitialCassetteStatus", data);
                    break;
                case (byte) GenericFailureCodes.OpticCanal:
                    Log.Debug("GenericFailureCodes.OpticCanal", data);
                    break;
                case (byte) GenericFailureCodes.MagneticCanal:
                    Log.Debug("GenericFailureCodes.MagneticCanal", data);
                    break;
                case (byte) GenericFailureCodes.CapacitanceCanal:
                    Log.Debug("GenericFailureCodes.CapacitanceCanal", data);
                    break;
            }
        }

        private void FindBillDenomination(byte bill, string action) {
            switch (bill) {
                case (byte) BillTypes.One:
                    Log.Debug(String.Format("{0} купюра 1 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(1);                    
                    break;
                case (byte) BillTypes.Three:
                    Log.Debug(String.Format("{0} купюра 3 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(3);                    
                    break;
                case (byte) BillTypes.Five:
                    Log.Debug(String.Format("{0} купюра 5 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(5);                    
                    break;
                case (byte) BillTypes.Ten:
                    Log.Debug(String.Format("{0} купюра 10 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(10);                    
                    break;
                case (byte) BillTypes.Twenty:
                    Log.Debug(String.Format("{0} купюра 20 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(20);                    
                    break;
                case (byte) BillTypes.Fifty:
                    Log.Debug(String.Format("{0} купюра 50 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(50);                    
                    break;
                case (byte) BillTypes.Hundred:
                    Log.Debug(String.Format("{0} купюра 100 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(100);                    
                    break;
                case (byte) BillTypes.TwoHundred:
                    Log.Debug(String.Format("{0} купюра 200 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(200);                    
                    break;
                case (byte) BillTypes.FiveHundred:
                    Log.Debug(String.Format("{0} купюра 500 сомони", action), bill);
                    _command = PrepareCommand(CashCodeCommands.AckResponse);
                    _newCommand = true;
                    OnMoneyAccepted(500);                    
                    break;
            }
        }

        public void EnableBillTypes() {
            _command = PrepareLongCommand(CashCodeCommands.EnableBillTypes, 0xff, 0x00);
            _newCommand = true;
        }

        public void DisableBillTypes() {
            _command = PrepareLongCommand(CashCodeCommands.EnableBillTypes, 0x00, 0x00);
            _newCommand = true;
        }

        public void Reset() {
            _command = PrepareCommand(CashCodeCommands.Reset);
            _newCommand = true;
        }
    }
}
