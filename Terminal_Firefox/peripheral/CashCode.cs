using System;
using System.IO.Ports;
using System.Threading;

namespace Terminal_Firefox.peripheral {
    public class CashCode {
        //готовые покеты
        private readonly byte[] _conReAck =  { 0x02, 0x03, 0x06, 0x00, 0xC2, 0x82 };
        private readonly byte[] _conReset =  { 0x02, 0x03, 0x06, 0x30, 0x41, 0xB3 };
        //private readonly byte[] _conGetSt =  { 0x02, 0x03, 0x06, 0x31, 0xC8, 0xA2 };
        private readonly byte[] _conStPoll = { 0x02, 0x03, 0x06, 0x33, 0xDA, 0x81 };
        //private readonly byte[] _conIdent =  { 0x02, 0x03, 0x06, 0x37, 0xFE, 0xC7 };
        //private readonly byte[] _conGetBt =  { 0x02, 0x03, 0x06, 0x41, 0x4F, 0xD1 };
        //private readonly byte[] _conStack =  { 0x02, 0x03, 0x06, 0x35, 0xEC, 0xE4 };
        //private readonly byte[] _conReturn = { 0x02, 0x03, 0x06, 0x36, 0x77, 0xD6 };
        //private readonly byte[] _conHold =   { 0x02, 0x03, 0x06, 0x38, 0x09, 0x3F };
        //private readonly byte[] _conExtBd =  { 0x02, 0x03, 0x06, 0x3A, 0x1B, 0x1C };
        //private readonly byte[] _conReqSt =  { 0x02, 0x03, 0x06, 0x60, 0xC4, 0xE1 };
        private readonly byte[] _conEnBt =   { 0x02, 0x03, 0x0C, 0x34, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0xB5, 0xC1 };
        //private readonly byte[] _conDiBt =   { 0x02, 0x03, 0x0C, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x17, 0x0C };
        
        //интервалы ожидания
        //private int _readIntervalTimeout = 400;
        //private int _readTotalTimeoutMultiplier = 0;
        //private int _readTotalTimeoutConstant = 400;
        //private int _writeTotalTimeoutMultiplier = 0;
        private const int WriteTotalTimeoutConstant = 30;

        //номиналы купюр
        public const byte B1 = 1;     // 0 0 0 0 0 0 0 1
        public const byte B3 = 2;     // 0 0 0 0 0 0 1 0
        public const byte B5 = 4;     // 0 0 0 0 0 1 0 0
        public const byte B10 = 8;    // 0 0 0 0 1 0 0 0
        public const byte B20 = 16;   // 0 0 0 1 0 0 0 0 
        public const byte B50 = 32;   // 0 0 1 0 0 0 0 0
        public const byte B100 = 64;  // 0 1 0 0 0 0 0 0
        public const byte B200 = 128; // 1 0 0 0 0 0 0 0 
        
        //переменная для ответа 
        private byte[] _aAnswer;
        
        //идентификатор запуска
        private int _sum;
        
        //хендлер сом порта
        private readonly SerialPort _port;
        private const int POLYNOMIAL = 0x08408;
        private string _msg;

        public CashCode() {
            _port = new SerialPort("com1", 9600, Parity.None, 8, StopBits.One);
            _port.Open();
            _port.WriteTimeout = 400;
            _port.ReadTimeout = 400;
        }

        public void Reset() {
            SendPacketInPort(_conReset);
        }

        public void ClosePort() {
            _port.Close();
        }

        public void OpenPort() {
            if (_port.IsOpen) return;
            _port.Open();
            _port.WriteTimeout = 400;
            _port.ReadTimeout = 400;
        }

        public void ConEnBTMethod() {
            SendPacketInPort(_conEnBt);
        }

        public String[] MoneyPoll() {
            _sum = 0;
            _msg = "";

            SendPacketInPort(_conStPoll);
            if (ReadPort(out _aAnswer)) {
                if (_aAnswer[2] == 7) {
                    Status(_aAnswer[3], _aAnswer[4], out _sum, out _msg);
                    if (_sum > 0) {
                        SendPacketInPort(_conReAck);

                        return new String[2] { _sum.ToString(), _msg };
                    }
                }
            } else {
                Reset();
            }

            return new String[2] { "", _msg };
        }


        //#############################################################\\
        protected bool SendPacketInPort(byte[] aPacket) {
            try {
                _port.WriteTimeout = WriteTotalTimeoutConstant;
                _port.DiscardOutBuffer();
                _port.Write(aPacket, 0, aPacket.Length);
            }
            catch (Exception) {
                return false;
            }
            return true;
        }

        //#############################################################\\
        protected bool ReadPort(out byte[] aAnswer) {
            Thread.Sleep(300);
            aAnswer = new byte[_port.BytesToRead];
            try {
                _port.Read(aAnswer, 0, aAnswer.Length);
            }
            catch (Exception) {
                return false;
            }

            return true;
        }


        public static char CalculateCrc16(byte[] data) {
            char crc = (char) 0x0000;
            int count = (data[2] - 2);
            if (count < 4) return (char) 0;
            for (int i = 0; i < count; i++) {
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


        public bool Status(byte first, byte second, out int money, out string msg) {
            switch (first) {
                case 0x10:
                case 0x11:
                case 0x12:
                    msg = "Включение питания после команд";
                    money = 0;
                    return false;
                case 0x13:
                    msg = "Инициализация";
                    money = 0;
                    return false;
                case 0x14:
                    msg = "Ожидание приема купюры";
                    money = 0;
                    return false;
                case 0x15:
                    msg = "Акцепт";
                    money = 0;
                    return false;
                case 0x19:
                    msg = "Недоступен, ожидаю инициализации";
                    money = 0;
                    return false;
                case 0x41:
                    msg = "Полная кассета";
                    money = 0;
                    return false;
                case 0x42:
                    msg = "Кассета отсутствует";
                    money = 0;
                    return false;
                case 0x43:
                    msg = "Замяло купюру";
                    //reset
                    money = 0;
                    return false;
                case 0x44:
                    msg = "Замяло касету 0_o";
                    //reset
                    money = 0;
                    return false;
                case 0x45:
                    msg = "КАРАУЛ !!!! ЖУЛИКИ !!!";
                    //reset
                    money = 0;
                    return false;
                case 0x47:
                    switch (second) {
                        case 0x50:
                            msg = "Stack_motor_falure";
                            money = 0;
                            return false;
                        case 0x51:
                            msg = "Transport_speed_motor_falure";
                            money = 0;
                            return false;
                        case 0x52:
                            msg = "Transport-motor_falure";
                            money = 0;
                            return false;
                        case 0x53:
                            msg = "Aligning_motor_falure";
                            money = 0;
                            return false;
                        case 0x54:
                            msg = "Initial_cassete_falure";
                            money = 0;
                            return false;
                        case 0x55:
                            msg = "Optical_canal_falure";
                            money = 0;
                            return false;
                        case 0x56:
                            msg = "Magnetical_canal_falure";
                            money = 0;
                            return false;
                        case 0x5F:
                            msg = "Capacitance_canal_falure";
                            money = 0;
                            return false;

                    }
                    money = 0;
                    msg = "";
                    return false;
                case 0x1C:
                    switch (second) {
                        case 0x60:
                            msg = "Insertion_error";
                            money = 0;
                            return false;
                        case 0x61:
                            msg = "Dielectric_error";
                            money = 0;
                            return false;
                        case 0x62:
                            msg = "Previously_inserted_bill_remains_in_head";
                            money = 0;
                            return false;
                        case 0x63:
                            msg = "Compensation__factor_error";
                            money = 0;
                            return false;
                        case 0x64:
                            msg = "Bill_transport_error";
                            money = 0;
                            return false;
                        case 0x65:
                            msg = "Identification_error";
                            money = 0;
                            return false;
                        case 0x66:
                            msg = "Verification_error";
                            money = 0;
                            return false;
                        case 0x67:
                            msg = "Optic_sensor_error";
                            money = 0;
                            return false;
                        case 0x68:
                            msg = "Return_by_inhibit_error";
                            money = 0;
                            return false;
                        case 0x69:
                            msg = "Capacistance_error";
                            money = 0;
                            return false;
                        case 0x6A:
                            msg = "Operation_error";
                            money = 0;
                            return false;
                        case 0x6C:
                            msg = "Length_error";
                            money = 0;
                            return false;
                    }
                    money = 0;
                    msg = "";
                    return false;

                case 0x80:
                    msg = "Депонет";
                    money = 0;
                    return false;

                case 0x81:
                    switch (second) {
                        case 0:
                            msg = "Укладка";
                            money = 1;
                            return false;
                        case 1:
                            msg = "Укладка";
                            money = 3;
                            return false;
                        case 2:
                            msg = "Укладка";
                            money = 5;
                            return false;
                        case 3:
                            msg = "Укладка";
                            money = 10;
                            return false;
                        case 4:
                            msg = "Укладка";
                            money = 20;
                            return false;
                        case 5:
                            msg = "Укладка";
                            money = 50;
                            return false;
                        case 6:
                            msg = "Укладка";
                            money = 100;
                            return false;
                        case 7:
                            msg = "Укладка";
                            money = 200;
                            return false;
                    }
                    money = 0;
                    msg = "";
                    return false;

                case 0x82:
                    msg = "Возврат купюры";
                    money = 0;
                    return false;
                default:
                    msg = "";
                    money = 0;
                    return false;
            }

        }

        //#############################################################\\
    }
}
