using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Runtime.InteropServices;

namespace terminal
{
    public class CashCode
    {   //готовые покеты
        byte[] ConReACK = { 0x02, 0x03, 0x06, 0x00, 0xC2, 0x82 };
        byte[] ConReset = { 0x02, 0x03, 0x06, 0x30, 0x41, 0xB3 };
        byte[] ConGetSt = { 0x02, 0x03, 0x06, 0x31, 0xC8, 0xA2 };
        byte[] ConStPoll = { 0x02, 0x03, 0x06, 0x33, 0xDA, 0x81 };
        byte[] ConIdent = { 0x02, 0x03, 0x06, 0x37, 0xFE, 0xC7 };
        byte[] ConGetBT = { 0x02, 0x03, 0x06, 0x41, 0x4F, 0xD1 };
        byte[] ConStack = { 0x02, 0x03, 0x06, 0x35, 0xEC, 0xE4 };
        byte[] ConReturn = { 0x02, 0x03, 0x06, 0x36, 0x77, 0xD6 };
        byte[] ConHold = { 0x02, 0x03, 0x06, 0x38, 0x09, 0x3F };
        byte[] ConExtBD = { 0x02, 0x03, 0x06, 0x3A, 0x1B, 0x1C };
        byte[] ConReqSt = { 0x02, 0x03, 0x06, 0x60, 0xC4, 0xE1 };
        byte[] ConEnBT = { 0x02, 0x03, 0x0C, 0x34, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0xB5, 0xC1 };
        byte[] ConDiBT = { 0x02, 0x03, 0x0C, 0x34, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x17, 0x0C };
        
        //интервалы ожидания
        int ReadIntervalTimeout = 400;
        int ReadTotalTimeoutMultiplier = 0;
        int ReadTotalTimeoutConstant = 400;
        int WriteTotalTimeoutMultiplier = 0;
        int WriteTotalTimeoutConstant = 30;
        
        //номинали купюр
        public const byte B1 = 1;      // 0 0 0 0 0 0 0 1
        public const byte B3 = 2;      // 0 0 0 0 0 0 1 0
        public const byte B10 = 4;     // 0 0 0 0 0 1 0 0
        public const byte B50 = 8;     // 0 0 0 0 1 0 0 0
        public const byte B100 = 16;   // 0 0 0 1 0 0 0 0 
        public const byte B500 = 32;   // 0 0 1 0 0 0 0 0
        public const byte B1000 = 64;  // 0 1 0 0 0 0 0 0
        public const byte B5000 = 128; // 1 0 0 0 0 0 0 0 
        //переменная для ответа 
        byte[] aAnswer;
        //идентификатор запуска
        bool start = false;
        private int sum = 0;
        //хендлер сомпорта
        SerialPort port;

        const int POLYNOMIAL = 0x08408;

        private string msg;
        
        public CashCode()
        {
            port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
            //port.ReadBufferSize = 255;
            //port.WriteBufferSize = 255;
            port.Open();
            port.WriteTimeout = 400;
            port.ReadTimeout = 400;

        }

        public void Reset()
        {
            SendPacketInPort(ConReset);
        }

        public void ClosePort()
        {
            port.Close();
        }

        public void OpenPort()
        {
            if (port.IsOpen == false)
            {
                port.Open();
                port.WriteTimeout = 400;
                port.ReadTimeout = 400;
            }

        }

        //private void button3_Click(object sender, EventArgs e)
        //{
        //    int msgsum;
        //    start = true;
        //    sum = 0;
        //    msgsum = 0;
        //    SendPacketInPort(ConEnBT);


        //    while (start)
        //    {
        //        msg = "";
        //        SendPacketInPort(ConStPoll);
        //        ReadPort(out aAnswer, true);
        //        if (aAnswer[2] == 7)
        //        {
        //            status(aAnswer[3], aAnswer[4], out msgsum, out msg);
        //            if (msgsum > 0)
        //            {
        //                SendPacketInPort(ConReACK);
        //                this.sum += msgsum;
        //                msgsum = 0;
        //            }
        //        }
        //    }
        //}

        public void ConEnBTMethod()
        {
            SendPacketInPort(ConEnBT);
        }

        public String[] MoneyPoll()
        {
            int msgsum = 0;
            sum = 0;
            msg = "";

            SendPacketInPort(ConStPoll);
            if (ReadPort(out aAnswer, true) == true)
            {
                if (aAnswer[2] == 7)
                {
                    status(aAnswer[3], aAnswer[4], out msgsum, out msg);
                    if (msgsum > 0)
                    {
                        SendPacketInPort(ConReACK);
                        this.sum += msgsum;
                        msgsum = 0;

                        return new String[2] { sum.ToString(), msg };
                    }
                }
            }
            else
                Reset();
            return new String[2] { "", msg };
        }

        
        //#############################################################\\
        protected bool SendPacketInPort(byte[] aPacket)
        {
            try
            {
                port.WriteTimeout = WriteTotalTimeoutConstant;
                port.DiscardOutBuffer();
                port.Write(aPacket, 0, aPacket.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        //#############################################################\\
        protected bool ReadPort(out byte[] aAnswer, bool aIsTimeOut)
        {
            Thread.Sleep(300);
            aAnswer = new byte[port.BytesToRead];
            try
            {
                port.Read(aAnswer, 0, aAnswer.Length);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        
        public static char calculateCrc16(byte[] data)
        {
            char crc = (char)0x0000;
            int j;
            int count = (data[2] - 2);
            if (count < 4) return (char)0;
            for (int i = 0; i < count; i++)
            {
                crc ^= (char)data[i];
                for (j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) == 1) { crc >>= 1; crc ^= (char)POLYNOMIAL; }
                    else crc >>= 1;
                }
            }
            return crc;
        }

        
        public bool status(byte first, byte second, out int money, out string msg)
        {
            switch (first)
            {
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
                    start = false;
                    return false;
                case 0x42:
                    msg = "Кассета отсутствует";
                    money = 0;
                    start = false;
                    return false;
                case 0x43:
                    msg = "Замяло купюру";
                    //reset
                    money = 0;
                    start = false;
                    return false;
                case 0x44:
                    msg = "Замяло касету 0_o";
                    //reset
                    money = 0;
                    start = false;
                    return false;
                case 0x45:
                    msg = "КАРАУЛ !!!! ЖУЛИКИ !!!";
                    //reset
                    money = 0;
                    start = false;
                    return false;
                case 0x47:
                    switch (second)
                    {
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
                    switch (second)
                    {
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
                    switch (second)
                    {
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
