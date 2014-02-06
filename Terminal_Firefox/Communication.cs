using System;
using System.Configuration;
using System.Net.Sockets;
using NLog;

namespace Terminal_Firefox {
    class Communication {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private TcpClient _clientStream = new TcpClient();
        private NetworkStream _serverStream;
        private string _ip = "109.74.68.45";
        private int _port = 7443;

        public bool Authorize() {
            if(_clientStream.Client == null) {
                _clientStream.Close();
                _clientStream = new TcpClient();
            }

            try {
                _port = int.Parse(ConfigurationManager.AppSettings["port"]);
                _ip = ConfigurationManager.AppSettings["ip"];
            } catch (Exception ex) {
                Log.Error("Невозможно прочитать данные из конфигурационного файла");
            }

            _clientStream.Connect(_ip, _port);
            _serverStream = _clientStream.GetStream();

        }

    }
}




Imports Newtonsoft.Json
Imports System.Threading
Imports System.Security
Imports System.IO
Imports System.Net.Security
Imports System.Net
Imports System.Security.Cryptography

Public Class Client
    Public Function Autorize() As Boolean
        Try
            
            clientSocket.Connect(GlobalVars.server_adr, GlobalVars.server_port)
            serverStream = clientSocket.GetStream()
            Dim a As auth = New auth()
            a.set_login(GlobalVars.agent_login)
            a.set_pass(GlobalVars.agent_pass)
            Dim j_auth As String
            j_auth = JsonConvert.SerializeObject(a)
            Dim mes As String = Encrypt(j_auth, public_key) & "</d>"
            Dim outStream As Byte() = System.Text.Encoding.UTF8.GetBytes(mes) 'str2zip(mes)
            serverStream.Write(outStream, 0, outStream.Length)
            serverStream.Flush()
            Return True
        Catch ex As Exception
            msg("Error: 002 " & ex.Message)
            Return False
        End Try
    End Function


    Public Function sSend(ByRef act As String, ByVal ms_body As String) As String
        'функция получает act и строку в формате json, шифрует и отправляет на сервер 
        'полученную строку дешифрует и возвращает
        If GlobalVars.connected Then
            Try
                Dim msg1 As Message = New Message()
                msg1.setLogin(GlobalVars.agent_login)
                msg1.setPassw(GlobalVars.agent_pass)

                Dim str_send As String
                msg1.setAct(act)
                msg1.setBody(ms_body)
                str_send = JsonConvert.SerializeObject(msg1)
                str_send = str_send.Replace("date1", "date")

                Dim mes As String = Encrypt(str_send, GlobalVars.agent_pass) & "</d>"
                Dim out_stream As Byte() = System.Text.Encoding.UTF8.GetBytes(mes) 'str2zip(mes)  '

                serverStream.Write(out_stream, 0, out_stream.Length)
                serverStream.Flush()

                Dim inStream(1024000) As Byte
                Dim ends As Integer = 0
                Dim desindex As Integer = 0
                Dim return_data As String = ""
                Dim colIter As Integer = 0

                Dim instream2(6096) As Byte
                return_data = ""
                While True
                    Array.Clear(instream2, 0, instream2.Length)
                    colIter += 1
                    serverStream.Read(instream2, 0, 6096)
                    Array.Copy(instream2, 0, inStream, desindex, instream2.Length)
                    desindex = desindex + instream2.Length - 1
                    return_data = System.Text.Encoding.UTF8.GetString(inStream)
                    ends = return_data.IndexOf("</d>")
                    If ends > -1 Or colIter > 300 Then Exit While
                End While

                return_data = return_data.Substring(0, ends)
                return_data = return_data.Replace(Chr(0), "")
                return_data = Decrypt(return_data, GlobalVars.agent_pass)
                If Not return_data.Equals("status:-1") And return_data <> "-1" Then
                    return_data = proc_repl(return_data) ' ответ получен. Обрабатываем ответ.
                End If
                Return return_data
            Catch ex As Exception
                GlobalVars.connected = False
                msg("ERROR: 003 " & ex.Message & " Error occured in: " & ex.TargetSite.Name)
                My.Application.Log.WriteException(ex, TraceEventType.Critical, "Unhandled Exception.")
                Return "-1"
            End Try
        End If
        Return "-1"
    End Function


    Public Function SendCommand(ByRef act As String, ByVal ms_body As String) As String
        Try
            Dim msg As New Message()
            Dim getCommand As New GetCommand()
            getCommand.setAct(act)
            getCommand.setBody("")
            getCommand.setDt(ms_body)
            getCommand.setLogin(GlobalVars.agent_login)
            getCommand.setVer(msg.ver)

            Dim [hash] As String = getCommand.getHash("sy5NvO7RqR6FW3z")
            Dim md5Hash = MD5.Create()

            Dim mes As String = GetMd5Hash(md5Hash, hash)
            getCommand.setHash(mes)

            Dim str As String = JsonConvert.SerializeObject(getCommand)
            Dim result As String = str

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
            ServicePointManager.ServerCertificateValidationCallback =
               New RemoteCertificateValidationCallback(AddressOf AcceptAllCertifications)

            Dim request As HttpWebRequest = _
                CType(WebRequest.Create("https://" + GlobalVars.server_adr + ":7443/api/nlisten/" + str), HttpWebRequest)

            
            Dim response As HttpWebResponse = _
               CType(request.GetResponse(), HttpWebResponse)

            Dim dataStream As Stream = response.GetResponseStream()
            Dim reader As StreamReader = New StreamReader(dataStream)

            Dim responseFromServer As String = reader.ReadToEnd()
            
            reader.Close()
            dataStream.Close()
            response.Close()

            proc_repl(responseFromServer)

        Catch ex As Exception
            msg(ex.ToString())
        End Try
    End Function

    Public Function AcceptAllCertifications(ByVal sender As Object,
                                            ByVal certification As Cryptography.X509Certificates.X509Certificate,
                                            ByVal chain As Cryptography.X509Certificates.X509Chain,
                                            ByVal sslPolicyErrors As SslPolicyErrors) As Boolean
        Return True
    End Function


    Private Function SplitArray(ByVal arr1() As Byte, ByVal arr2() As Byte)
        Dim col As Integer = arr1.Count + arr2.Count
        Dim arr3(col) As Byte, i As Integer = 0
        For j As Integer = 0 To arr1.Count - 1
            arr3(i) = arr1(j)
            i = i + 1
        Next
        For j As Integer = 0 To arr2.Count - 1
            arr3(i) = arr2(j)
            i = i + 1
        Next
        Return arr3
    End Function

    ''' <summary>
    ''' Обрабатывает ответ от сервера
    ''' </summary>
    ''' <param name="return_data">Ответ от сервера в формате Json</param>
    ''' <returns>-1 При возникновении исключительной ситуации</returns>
    ''' <remarks>1 В случае успешной обработки</remarks>
    Public Function proc_repl(ByVal return_data As String) As Integer
        Try
            If Not return_data.Equals("") Then 'если имеются данные
                Dim msg_in As MessageInput = New MessageInput()
                msg_in = JsonConvert.DeserializeObject(Of MessageInput)(return_data)

                Dim act As Integer = msg_in.getAct()
                Dim status As Integer = msg_in.getStatus()
                Dim hesh_id As String = msg_in.getHesh_id()
                Dim Body2 As String = msg_in.getBody()
                Dim sqlUpdate As String = ""
                If status = 0 Then
                    Select Case act
                        Case 0 'состояние транзакции
                            If Not hesh_id.Equals("") Then
                                sqlUpdate = "UPDATE plateji SET status=1 WHERE hesh_id='" & hesh_id & "'"
                                Execute(sqlUpdate)
                            End If
                        Case 1 'состояние терминала
                            If Not hesh_id.Equals("") Then
                                sqlUpdate = "UPDATE perif_status SET status=1 WHERE id='" & hesh_id & "'"
                                Execute(sqlUpdate)
                            End If
                        Case 2 'синхронизация даты врмени
                            msg("Время на сервере: " & msg_in.getBody())

                            Dim date_time As String = msg_in.getBody()
                            '2011-09-19 11:29:00.236
                            Dim a As Date = DateTime.Parse(date_time)
                            Dim c As SetDateTime = New SetDateTime()
                            Dim d As Boolean = c.UpdateSystemTime(a)
                            If d Then
                                GlobalVars.syncDateTime = True
                                Dim sql1 As String = "UPDATE commands SET status=1, date_execute=UNIX_TIMESTAMP() WHERE id_on_server=" & GlobalVars.doCommand(0).ToString()
                                Execute(sql1)
                                msg("Синхронизированно системное время с сервером")
                                Dim comm1 As New command
                                comm1.setStatus(6)
                                comm1.setId(GlobalVars.doCommand(0))
                                Dim strSend As String = JsonConvert.SerializeObject(comm1)
                                sSend("12", strSend)

                                GlobalVars.doCommand(0) = 0
                                GlobalVars.doCommand(1) = 0
                            End If
                        Case 5 'Список групп услуг

                            Dim OpG As List(Of OpGroup) = New List(Of OpGroup)
                            OpG = JsonConvert.DeserializeObject(Of List(Of OpGroup))(Body2)
                            Dim cnt_json As Integer = OpG.Count
                            If cnt_json > 0 Then
                                Dim sqlDelete As String = "DELETE FROM OpGroup"
                                Execute(sqlDelete)
                            End If

                            Dim sql_insert1 As String = "INSERT INTO OpGroup (id,code,name,order_n,parent,show_child) VALUES"
                            Dim sql_insert2 As String = ""
                            For i As Integer = 0 To cnt_json - 1
                                sql_insert2 = sql_insert1 & " (" & OpG(i).getId() & ", '" & OpG(i).getCode() & "','" & OpG(i).getName() & "','" & OpG(i).getOrder() & "','" & OpG(i).getParent() & "','" & OpG(i).getShowChild() & "')"
                                Execute(sql_insert2)
                                sql_insert2 = ""
                            Next

                            GlobalVars.getGroupServises = True
                            GlobalVars.getExistsServices = False
                            sql_insert1 = "UPDATE commands SET status=1, date_execute=UNIX_TIMESTAMP() WHERE id_on_server=" & GlobalVars.doCommand(0).ToString()
                            Execute(sql_insert1)

                            Dim comm1 As New command
                            comm1.setStatus(6)
                            comm1.setId(GlobalVars.doCommand(0))
                            Dim strSend As String = JsonConvert.SerializeObject(comm1)
                            sSend("12", strSend)

                            GlobalVars.doCommand(0) = 0
                            GlobalVars.doCommand(1) = 0
                            GlobalVars.doBlock = False '
                            msg("Обновлен список групп операторов")
                        Case 6 'список операторов

                            Dim lag As List(Of OpService) = New List(Of OpService)
                            lag = JsonConvert.DeserializeObject(Of List(Of OpService))(Body2)
                            Dim cnt_json As Integer = lag.Count
                            If cnt_json > 0 Then
                                Dim sqlDelete As String = "DELETE FROM OpService;"
                                Execute(sqlDelete)
                            End If
                            Dim sql_insert1 As String = "INSERT INTO OpService (code,name,type,state,need_chek, show_sec_num, sec_mask, order_n,mask,id_operator, mask_length, keyboard) VALUES"
                            Dim sql_insert2 As String = ""
                            For i As Integer = 0 To cnt_json - 1
                                sql_insert2 = sql_insert1 & " ('" & lag(i).getCode() & "','" & lag(i).getName() & "','" & lag(i).getType1() & "','" & lag(i).getState() &
                                   "','" & lag(i).getNeed_check() & "'," & lag(i).getShow_num2() & ",'" & lag(i).getMask_num2() & "','" & lag(i).getOrder() & "','" & lag(i).getMask() & "','" & lag(i).getId() & "','" & lag(i).getMaskLength() & "','" & lag(i).getKey_Type() & "');"
                                Execute(sql_insert2)
                                sql_insert2 = ""
                            Next
                            Dim dt2 As DataTable = GetTable("SELECT * FROM last_use_service")
                            For j As Integer = 0 To dt2.Rows.Count - 1
                                Dim del As Boolean = True
                                Dim idSer As Integer = dt2.Rows(j).Item("service")
                                For i As Integer = 0 To cnt_json - 1
                                    If idSer = lag(i).getId() Then del = False
                                Next
                                'If del Then Execute("DELETE FROM last_use_service WHERE service=" & idSer)
                            Next

                            GlobalVars.getServises = True
                            GlobalVars.getExistsServices = False
                            sql_insert2 = "UPDATE commands SET status=1, date_execute=UNIX_TIMESTAMP() WHERE id_on_server=" & GlobalVars.doCommand(0).ToString()
                            Execute(sql_insert2)

                            Dim comm1 As New command
                            comm1.setStatus(6)
                            comm1.setId(GlobalVars.doCommand(0))
                            Dim strSend As String = JsonConvert.SerializeObject(comm1)
                            sSend("12", strSend)

                            GlobalVars.doBlock = False
                            GlobalVars.doCommand(0) = 0
                            GlobalVars.doCommand(1) = 0
                            msg("Обновлен список операторов")
                        Case 7 'тарифы
                            Dim tarif_plan As List(Of tarifPlan) = New List(Of tarifPlan)
                            tarif_plan = JsonConvert.DeserializeObject(Of List(Of tarifPlan))(Body2)
                            Dim cnt_json As Integer = tarif_plan.Count
                            Dim cnt_json1 As Integer = 0
                            Dim cnt_json2 As Integer = 0
                            If cnt_json > 0 Then
                                Dim sql As String = "DELETE FROM proc_tarifplan"
                                Execute(sql)
                                sql = "DELETE FROM proc_tarif"
                                Execute(sql)
                                sql = "DELETE FROM proc_tarifarr"
                                Execute(sql)
                            End If

                            Dim sql_insert As String
                            Dim sql_insert_tarif As String = ""
                            Dim sql_insert_tarif_arr As String = ""
                            For i As Integer = 0 To cnt_json - 1
                                sql_insert = "INSERT INTO proc_tarifplan (id,code,name,date_begin,date_end) VALUES (" &
                                     tarif_plan(i).getId() & ", '" & tarif_plan(i).getCode() & "', '" & tarif_plan(i).getName() & "', '" & tarif_plan(i).getDate_begin() & "', '" & tarif_plan(i).getDate_end() & "')"
                                Execute(sql_insert)
                                Thread.Sleep(10)
                                Dim tarif As List(Of Tarif) = New List(Of Tarif)
                                tarif = JsonConvert.DeserializeObject(Of List(Of Tarif))(tarif_plan(i).getTarif)
                                cnt_json1 = tarif.Count


                                For j As Integer = 0 To cnt_json1 - 1
                                    sql_insert_tarif = "INSERT INTO proc_tarif (id,code,name,op_service_id,prc,summa,summa_own,min,max,tarif_plan_id,rus,taj,eng) VALUES (" &
                                                                  tarif(j).getId() & ", '" &
                                                                  tarif(j).getCode() & "', '" &
                                                                  tarif(j).getName() & "', " &
                                                                  tarif(j).getOp_service_id() & ", '" &
                                                                  tarif(j).isPrc() & "', " &
                                                                  tarif(j).getSumma() & ", " &
                                                                  tarif(j).getSumma_own() & ", " &
                                                                  tarif(j).getMin() & ", " &
                                                                  tarif(j).getMax() & ", " &
                                                                  tarif_plan(i).getId() & ", '" &
                                                                  tarif(j).getRu_text() & "', '" &
                                                                  tarif(j).getTj_text() & "', '" &
                                                                  tarif(j).getEn_text() & "')"
                                    Execute(sql_insert_tarif)
                                    Thread.Sleep(10)
                                    Dim tarif_arr As List(Of TarifArr) = New List(Of TarifArr)
                                    tarif_arr = JsonConvert.DeserializeObject(Of List(Of TarifArr))(tarif(j).getArr)
                                    cnt_json2 = tarif_arr.Count


                                    For k As Integer = 0 To cnt_json2 - 1
                                        sql_insert_tarif_arr = "INSERT INTO proc_tarifarr (id,parent,prc,summa,min,max,tarif_id,beg_time,end_time) VALUES ( " &
                                                                   tarif_arr(k).getId() & ", '" &
                                                                   tarif_arr(k).isParent() & "', '" &
                                                                   tarif_arr(k).isPrc() & "', " &
                                                                   tarif_arr(k).getSumma().ToString.Replace(",", ".") & ", " &
                                                                   tarif_arr(k).getMin() & ", " &
                                                                   tarif_arr(k).getMax() & ", " &
                                                                   tarif(j).getId() & ", '" &
                                                                   tarif_arr(k).getDate_begin() & "', '" &
                                                                   tarif_arr(k).getDate_end() & "')"

                                        Execute(sql_insert_tarif_arr)
                                        Thread.Sleep(10)
                                    Next k
                                Next j
                            Next i
                            GlobalVars.getTarifServises = True
                            Dim sql_insert2 As String = "UPDATE commands SET status=1, date_execute=UNIX_TIMESTAMP() WHERE id_on_server=" & GlobalVars.doCommand(0).ToString()
                            Execute(sql_insert2)

                            Dim comm1 As New command
                            comm1.setStatus(6)
                            comm1.setId(GlobalVars.doCommand(0))
                            Dim strSend As String = JsonConvert.SerializeObject(comm1)
                            sSend("12", strSend)

                            GlobalVars.doBlock = False
                            GlobalVars.doCommand(0) = 0
                            GlobalVars.doCommand(1) = 0
                            msg("Обновлены тарифные планы")
                        Case 8
                            sqlUpdate = "UPDATE inkasso SET status=1 WHERE inkass_id='" & hesh_id & "';"
                            Execute(sqlUpdate)
                        Case 9 'настройки терминала
                            Dim ter_par As TerminalParam = New TerminalParam
                            ter_par = JsonConvert.DeserializeObject(Of TerminalParam)(Body2)

                            GlobalVars.TerminalAddress(0) = ter_par.getAddress_rus()
                            GlobalVars.TerminalAddress(1) = ter_par.getAddress_eng()
                            GlobalVars.TerminalAddress(2) = ter_par.getAddress_taj()

                            StatusMessage(0) = "Терминал №" & GlobalVars.terminal_id & ", Адрес: " & GlobalVars.TerminalAddress(0) & ", Телефон службы поддержки: " & GlobalVars.callcenter
                            StatusMessage(1) = "Terminal #" & GlobalVars.terminal_id & ", Address: " & GlobalVars.TerminalAddress(1) & ", Call center: " & GlobalVars.callcenter
                            StatusMessage(2) = "Терминал №" & GlobalVars.terminal_id & ", Суроға: " & GlobalVars.TerminalAddress(2) & ", Телефон: " & GlobalVars.callcenter

                            sqlUpdate = "UPDATE settings set value='" & GlobalVars.TerminalAddress(0) & "' WHERE variable='adress_rus';"
                            sqlUpdate = sqlUpdate & "UPDATE settings set value='" & GlobalVars.TerminalAddress(1) & "' WHERE variable='adress_eng';"
                            sqlUpdate = sqlUpdate & "UPDATE settings set value='" & GlobalVars.TerminalAddress(2) & "' WHERE variable='adress_taj';"
                            Execute(sqlUpdate)

                            sqlUpdate = "UPDATE settings set value='" & ter_par.getTerminal_n() & "' WHERE variable='terminal_id'"
                            GlobalVars.terminal_id = ter_par.getTerminal_n()
                            Execute(sqlUpdate)

                            GlobalVars.inkass_n = GlobalVars.terminal_id & GlobalVars.inkass_id
                            sqlUpdate = "UPDATE settings set value='" & GlobalVars.inkass_n & "' WHERE variable='inkass_n'"
                            Execute(sqlUpdate)

                            sqlUpdate = "UPDATE settings set value='" & ter_par.getCall_center() & "' WHERE variable='call_center'"
                            GlobalVars.callcenter = ter_par.getCall_center()
                            Execute(sqlUpdate)

                            sqlUpdate = "UPDATE settings set value='" & ter_par.getBlockTerminal() & "' WHERE variable='BlockTerminal'"
                            GlobalVars.BlockTerminal = ter_par.getBlockTerminal()
                            Execute(sqlUpdate)

                            sqlUpdate = "UPDATE settings set value='" & ter_par.getPing_time() & "' WHERE variable='ping_time'"
                            GlobalVars.PingTime = ter_par.getPing_time()
                            Execute(sqlUpdate)

                            GlobalVars.getSettings = True
                            Dim sql_insert2 As String = "UPDATE commands SET status=1, date_execute=UNIX_TIMESTAMP() WHERE id_on_server=" & GlobalVars.doCommand(0).ToString()
                            Execute(sql_insert2)

                            Dim comm1 As New command
                            comm1.setStatus(6)
                            comm1.setId(GlobalVars.doCommand(0))
                            Dim strSend As String = JsonConvert.SerializeObject(comm1)
                            sSend("12", strSend)

                            GlobalVars.doBlock = False
                            GlobalVars.doCommand(0) = 0
                            GlobalVars.doCommand(1) = 0
                            msg("Получены настройки терминала")
                        Case 10 'выбор пользователей инкассаторов
                            Dim inkas_user As List(Of InkasUser) = New List(Of InkasUser)
                            inkas_user = JsonConvert.DeserializeObject(Of List(Of InkasUser))(Body2)
                            Dim cnt_json As Integer = inkas_user.Count

                            If cnt_json > 0 Then
                                sqlUpdate = "DELETE FROM user"
                                Execute(sqlUpdate)
                                For i As Integer = 0 To cnt_json - 1
                                    sqlUpdate = "INSERT INTO user (id,login,passw) VALUES (" &
                                        inkas_user(i).getId() & ", '" & inkas_user(i).getLogin() & "', '" & inkas_user(i).getPassw() & "');"
                                    Execute(sqlUpdate)
                                Next i
                            End If
                            GlobalVars.getInkassUsers = True
                            Dim sql_insert2 As String = "UPDATE commands SET status=1, date_execute=UNIX_TIMESTAMP() WHERE id_on_server=" & GlobalVars.doCommand(0).ToString()
                            Execute(sql_insert2)

                            Dim comm1 As New command
                            comm1.setStatus(6)
                            comm1.setId(GlobalVars.doCommand(0))
                            Dim strSend As String = JsonConvert.SerializeObject(comm1)
                            sSend("12", strSend)
                            GlobalVars.doBlock = False
                            GlobalVars.doCommand(0) = 0
                            GlobalVars.doCommand(1) = 0
                            msg("Обновлен список инкассаторов")
                        Case 11
                            msg("Удаление всех комиссий на услуги...")
                            Dim sql As String = "DELETE FROM proc_tarifplan"
                            Execute(sql)
                            sql = "DELETE FROM proc_tarif"
                            Execute(sql)
                            sql = "DELETE FROM proc_tarifarr"
                            Execute(sql)
                            msg("Удалены все комиссии на услуги!!!!")
                        Case 12
                            If (Not Body2.Equals("")) Then
                                Try
                                    Dim comm1 As New command
                                    comm1 = JsonConvert.DeserializeObject(Of command)(Body2)

                                    Dim sqlInsert As String = "INSERT INTO commands (act,date_create,id_on_server,date_from,date_to,script) VALUES (" & comm1.getAct() & ",UNIX_TIMESTAMP()," & comm1.getId() & ",'" & comm1.getDateFrom & "','" & comm1.getDateTo & "','" & comm1.getScript & "');"
                                    Execute(sqlInsert)
                                    comm1.setStatus(5)
                                    Dim strSend As String = JsonConvert.SerializeObject(comm1)
                                    sSend("12", strSend)
                                    msg("С сервера получена команда: " & comm1.getAct())
                                Catch ex As Exception
                                    msg("Произошла ОШИБКА; ACT=12; Не возможно преобразовать полученную команду с сервера.")
                                End Try
                            End If
                    End Select
                End If
            End If
            Return "1"
        Catch ex As Exception
            msg("ERROR: 004 Ошибка при обработке принятого от сервера сообщения: /" & return_data & "/ " & ex.Message)
            Return "-1"
            ' В случаи ошибки нужно авторизаваться
        End Try
    End Function
End Class