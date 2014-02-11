﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Configuration;
using NLog;

namespace Terminal_Firefox.Utils {
    public class Communication {

        private readonly TcpClient _clientStream = new TcpClient();
        private NetworkStream _serverStream;
        private string _ip;
        private int _port;
        private string _key;
        private readonly byte[] _iv = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();


        public bool Autorize() {
            try {
                _key = classes.TerminalSettings.TerminalPassword;
                _port = int.Parse(ConfigurationManager.AppSettings["port"]);
                _ip = (ConfigurationManager.AppSettings["ip"]);

                if (_clientStream.Connected == false) {
                    _clientStream.Connect(_ip, _port);
                }
                _serverStream = _clientStream.GetStream();

                //Dim a As auth = New auth()
                //a.set_login(GlobalVars.agent_login)
                //a.set_pass(GlobalVars.agent_pass)
            
                
                byte[] toWrite = Encoding.UTF8.GetBytes(Encrypt("asdf","asdf") + "</d>");


                _serverStream.Write(toWrite, 0, toWrite.Length);
                _serverStream.Flush();
                return true;
            } catch (Exception exception) {
                Log.Error("Невозможно соединиться с сервером '" + _ip + "' на порт '" + _port + "'", exception);
                return false;
            }
        }


        private string Encrypt(string stringToEncrypt, string encryptionKey) {
            try {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider {
                    Key = Encoding.UTF8.GetBytes(encryptionKey)
                };

                byte[] inputByteArray = Encoding.UTF8.GetBytes(stringToEncrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(des.Key, _iv), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();

                string result = Convert.ToBase64String(ms.ToArray());
                return result;
            } catch (Exception ex) {
                Log.Error("Невозможно зашифровать строку", ex);
                return "";
            }
        }


        private string Decrypt(string stringToDecrypt, string encryptionKey) {
            try {
                DESCryptoServiceProvider des = new DESCryptoServiceProvider {
                    Key = Encoding.UTF8.GetBytes(encryptionKey)
                };

                byte[] inputByteArray = Convert.FromBase64String(stringToDecrypt);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(des.Key, _iv), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Encoding.UTF8.GetString(ms.ToArray());
            } catch (Exception ex) {
                Log.Error("Невозможно расшифровать строку", ex);
            }
            return "";
        }


        public string SSend(byte act, string body) {
            string mes = Encrypt(body, _key) + "</d>";

            byte[] outer = Encoding.UTF8.GetBytes(mes);
            _serverStream.Write(outer, 0, outer.Length);
            _serverStream.Flush();

            // Wait before reading data
            // otherwise data Available 
            // data amount will not be available
            Thread.Sleep(1500);

            int data = _clientStream.Available;
            byte[] read = new byte[data];
            _serverStream.Read(read, 0, read.Length);

            string result = Encoding.UTF8.GetString(read);
            result = result.Substring(0, result.Length - 4);
            result = Decrypt(result, _key);

            return result;
        }
    }
}
