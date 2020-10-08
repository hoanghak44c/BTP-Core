using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using QLBH.Core.Data;
using QLBH.Core.Exceptions;
using QLBH.Core.Form;
using QLBH.Core.Providers;

namespace QLBH.Core.Net
{
    public interface ITelListener
    {
        void Start();
        void Stop();
        string CallOut(string telNumber);
    }

    public abstract class Common : ITelListener
    {
        private bool isStarted;

        private Socket serverSck, clientSck, clientSck2;

        protected int CurrentPort;

        protected abstract void ReceiptData(string data);

        protected abstract void ShowMPClient();

        protected abstract bool Authenticated(string auth);

        protected abstract string GetMPClientProcessName();

        private static bool ValidNumber(string number)
        {
            if (String.IsNullOrEmpty(number)) return false;

            foreach (var c in number.ToCharArray())
            {
                if("0123456789".Contains(c.ToString())) continue;

                return false;
            }

            return true;
        }

        private string chucNang;

        public string CallOut(string number)
        {
            try
            {
                stopPending = false;

                if (!ValidNumber(number)) 
                    
                    throw new ManagedException("Số điện thoại bạn đang gọi không đúng", false);

                var stackTrace = new StackTrace();

                var callerType = stackTrace.GetFrame(1).GetMethod().ReflectedType;

                foreach (System.Windows.Forms.Form openForm in Application.OpenForms)
                {
                    if(openForm.GetType() == callerType)
                    {
                        chucNang = openForm.Text;

                        if (String.IsNullOrEmpty(chucNang)) chucNang = openForm.GetType().Name;

                        break;
                    }
                }

                var data = Encoding.ASCII.GetBytes(number);

                if (Process.GetProcessesByName(GetMPClientProcessName()).Length == 0)

                    Start();

                if (clientSck != null && clientSck.Connected)
                {
                    callId = String.Empty;

                    clientSck.Send(data, data.Length, SocketFlags.None);

                    //data = new byte[1024];

                    //var recv = clientSck.Receive(data);

                    //var callId = Encoding.ASCII.GetString(data, 0, recv);

                    //return callId;

                    frmPending = new frmPendingCallOut();

                    frmPending.DoWork(PendingCallOut);

                    return callId;
                }

                throw new ManagedException("Không thể thực hiện cuộc gọi");
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }
        }

        private bool stopPending;

        private string callId;

        private frmPendingCallOut frmPending;

        private void CancelPending()
        {
            stopPending = true;

            callId = "0";
        }

        private void PendingCallOut()
        {
            try
            {
                frmPending.OnCancel = CancelPending;

                var i = 0;
                
                while (true)
                {
                    if (stopPending) break;

                    Thread.CurrentThread.Join(500);

                    i++;

                    if (i > 2 * 15 && !frmPending.Enabled) frmPending.Enabled = true;
                }

                frmPending.Activate();

                frmPending.DlgResult = callId == "0" ? DialogResult.Cancel : DialogResult.OK;

            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }
        }

        private int GetRandomPort()
        {
            return 5051;

            var random = new Random(1);

            int port = random.Next(5000, 10000);

            bool isAvailable = false;

            while(!isAvailable)
            {
                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                isAvailable = true;

                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == port)
                    {
                        isAvailable = false;

                        port = random.Next(5000, 10000);
                        
                        break;
                    }
                }

                if (isAvailable && NetProvider.Instance.ExistPortNumber(port))
                {
                    isAvailable = false;

                    port = random.Next(5000, 10000);
                }
            }

            CurrentPort = port;

            NetProvider.Instance.UpdatePortNumber(CurrentPort);
            
            return CurrentPort;
        }

        private void StopListen()
        {
            try
            {
                while (isStarted && serverSck != null)
                {
                    Thread.CurrentThread.Join(500);

                    if (Process.GetProcessesByName(GetMPClientProcessName()).Length == 0)
                    {
                        isStarted = false;
                    }
                }

                Thread.CurrentThread.Join(500);

                if (clientSck2 != null)
                {
                    clientSck2.Close();
                }

                if (serverSck == null) return;

                if (Process.GetProcessesByName(GetMPClientProcessName()).Length == 0 &&
                    
                    clientSck != null)
                {
                    if(!goodbye)
                    {
                        clientSck.Close();

                        clientSck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        clientSck.Connect("127.0.0.1", 5051);

                        var data = Encoding.ASCII.GetBytes(String.Format("Hello, I am {0},{1}", EventLogProvider.Instance.UserName, 1555));

                        clientSck.Send(data, data.Length, SocketFlags.None);

                        data = new byte[1024];

                        clientSck.Receive(data);

                        data = Encoding.ASCII.GetBytes("Bye!");

                        clientSck.Send(data, data.Length, SocketFlags.None);

                        clientSck.Close();

                        clientSck = null;

                        //Start();
                    }

                    return;
                }

                if (serverSck.Connected)
                    
                    serverSck.Shutdown(SocketShutdown.Both);

                serverSck.Close();

                serverSck = null;

                WriteLog("Stop listener");
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }

        private EndPoint BindSock()
        {
            try
            {
                EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Convert.ToInt32(GetRandomPort()));

                serverSck.Bind(localEndPoint);

                return localEndPoint;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Thread stopThread, listenThread;

        public void Start()
        {
            //while (listenThread != null && listenThread.IsAlive)
            //{
            //    WriteLog("creating start thread ...");

            //    listenThread.Join();
            //}

            if (!isStarted)
            {
                isStarted = true;

                listenThread = new Thread(StartListen);

                listenThread.Start();                
            }
        }

        public void Stop()
        {
            try
            {
                var data = Encoding.ASCII.GetBytes("Close");

                if (isStarted && clientSck != null && clientSck.Connected)
                {
                    clientSck.Send(data, data.Length, SocketFlags.None);

                    clientSck.Close();

                    clientSck = null;
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.ToString());
            }
        }

        protected virtual bool ResetConfirmed()
        {
            return false;
        }
        
        private bool goodbye = false;

        private void StartListen()
        {
            try
            {
                //TCP protocol

                var recv = 0;

                byte[] data;

                if (serverSck == null)
                {
                    serverSck = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    BindSock();
                }

                serverSck.Listen(1);

                WriteLog("Started listening...");

                if (Process.GetProcessesByName(GetMPClientProcessName()).Length == 0)
                    
                    ShowMPClient();

                while (stopThread != null && stopThread.IsAlive)
                {
                    WriteLog("creating stop thread ...");

                    stopThread.Join();
                }

                stopThread = new Thread(StopListen);

                stopThread.Start();

                if (Process.GetProcessesByName(GetMPClientProcessName()).Length == 0) return;

                clientSck = serverSck.Accept();

                data = new byte[1024];

                recv = clientSck.Receive(data);

                var sAuthentication = Encoding.ASCII.GetString(data, 0, recv);

                WriteLog(sAuthentication);

                WriteLog(String.Format("clientSck Connected with {0} at port {1}", ((IPEndPoint)clientSck.RemoteEndPoint).Address,
                    ((IPEndPoint)clientSck.RemoteEndPoint).Port));

                data = Encoding.ASCII.GetBytes("WELCOME_CRM");

                clientSck.Send(data, data.Length, SocketFlags.None);

                goodbye = false;

                if (!Authenticated(sAuthentication))
                {
                    if(ResetConfirmed())
                    {
                        data = Encoding.ASCII.GetBytes("Close");

                        clientSck.Send(data, data.Length, SocketFlags.None);

                        WriteLog("Reset mpcc client");

                        goodbye = true;
                    } 
                    else
                    {
                        clientSck.Close();

                        clientSck = null;

                        return;
                    }
                }

                while (isStarted)
                {
                    serverSck.Listen(1);

                    WriteLog("Waiting for a client about call number...");

                    do
                    {
                        try
                        {
                            clientSck2 = serverSck.Accept();

                            data = new byte[1024];

                            recv = clientSck2.Receive(data);

                            sAuthentication = Encoding.ASCII.GetString(data, 0, recv);

                            WriteLog(sAuthentication);

                            if (!Authenticated(sAuthentication))
                            {
                                clientSck2.Close();

                                clientSck2 = null;

                                isStarted = false;
                            } 
                            else
                            {
                                data = Encoding.ASCII.GetBytes("WELCOME_CRM");

                                clientSck2.Send(data, data.Length, SocketFlags.None);
                            }
                        }
                        catch (SocketException ex)
                        {
                            WriteLog(ex.ToString());
                        }
                    } while (isStarted && clientSck2 == null);

                    if(clientSck2 == null) continue;

                    var clientEp = (IPEndPoint)clientSck2.RemoteEndPoint;

                    WriteLog(String.Format("clientSck2 Connected with {0} at port {1}", clientEp.Address, clientEp.Port));

                    while (isStarted)
                    {
                        data = new byte[1024];

                        recv = clientSck2.Receive(data);

                        if (recv == 0) break;

                        var spacket = Encoding.ASCII.GetString(data, 0, recv);

                        WriteLog(spacket);

                        if(spacket == "Bye!")
                        {
                            if (clientSck != null)
                            {
                                clientSck.Close();

                                clientSck = null;
                            }

                            isStarted = false;

                            goodbye = true;

                            break;
                        }

                        var chieugoi = spacket.EndsWith(",i") ? CHIEUGOI.ABCFECS
                                                
                            : spacket.EndsWith(",o") ? CHIEUGOI.USDFKEA : CHIEUGOI.KOGSKDT;

                        if (chieugoi == CHIEUGOI.ABCFECS || chieugoi == CHIEUGOI.USDFKEA)
                        {
                            callId = spacket.Split(",".ToCharArray())[0];
                        }

                        var soDienThoai = spacket.Split(",".ToCharArray())[1];

                        LogCuocGoiProvider.Instance.LogCuocGoi(soDienThoai, Convert.ToInt32(chieugoi),

                            Convert.ToDouble(callId), chucNang);

                        chucNang = String.Empty;

                        if (chieugoi == CHIEUGOI.USDFKEA)
                        {
                            stopPending = true;

                            break;
                        }

                        //clientSck2.Send(data, recv, SocketFlags.None);

                        var showPopUpThread = new Thread(

                            delegate()
                                {
                                    ReceiptData(spacket);
                                });

                        showPopUpThread.Start();

                    }

                    if(recv == 0)
                    {
                        WriteLog(String.Format("clientSck2 Disconnected from {0}", clientEp.Address));

                        clientSck2.Close();

                        clientSck2 = null;
                    }

                    if (isStarted) continue;

                    if (clientSck2 != null) clientSck2.Close();

                    clientSck2 = null;

                }

                //serverSck.Close();

                ////UDP protocol

                //if (serverSck == null)

                //    serverSck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                //var localEndPoint = BindSock();

                //var stopThread = new Thread(StopListen);

                //stopThread.Start();

                //while (isStarted)
                //{
                //    var buffer = new byte[1024];

                //    int recv = serverSck.ReceiveFrom(buffer, ref localEndPoint);

                //    string message = Encoding.ASCII.GetString(buffer, 0, recv);

                //    ReceiptData(message);

                //    //if (lblCallerId.InvokeRequired)
                //    //    Invoke((MethodInvoker)
                //    //           delegate
                //    //           {
                //    //               lblCallerId.Text = message;
                //    //           });

                //    ////message = "OK";

                //    //byte[] data = Encoding.ASCII.GetBytes("OK");

                //    //var ipEndPoint = new IPEndPoint(IPAddress.Parse(txtRemoteIp.Text), Convert.ToInt32(txtRemotePort.Text));

                //    //serverSck.SendTo(data, data.Length, SocketFlags.None, ipEndPoint);
                //}
            }
            catch (SocketException ex)
            {
                isStarted = false;

                WriteLog(ex.ToString());
            }
            catch (Exception ex)
            {
                isStarted = false;

                WriteLog(ex.ToString());

                //throw new ManagedException(ex.ToString());
            }
        }

        private void WriteLog(string data)
        {
            try
            {
                //var wr = File.AppendText("TcpServer.Log");
                //wr.WriteLine(data);
                //wr.Flush();
                //wr.Close();
            }
            catch (Exception)
            {
                
                //throw;
            }
        }
    }

    internal enum CHIEUGOI
    {
        /// <summary>
        /// Gọi vào
        /// </summary>
        ABCFECS = 0,
        /// <summary>
        /// Gọi ra
        /// </summary>
        USDFKEA = 1,
        /// <summary>
        /// Không xác định
        /// </summary>
        KOGSKDT
    }

    internal class LogCuocGoiProvider
    {
        private static LogCuocGoiProvider instance;

        private LogCuocGoiProvider() { }

        internal static LogCuocGoiProvider Instance
        {
            get { return instance ?? (instance = new LogCuocGoiProvider()); }
        }
        internal void LogCuocGoi(string sodienthoai, int chieugoi, double callid, string chucNang)
        {
            LogCuocGoiDAO.Instance.LogThongTinCuocGoi(sodienthoai, chieugoi, callid, chucNang);
        }
    }

    internal class LogCuocGoiDAO : BaseDAO
    {
        private static LogCuocGoiDAO instance;

        private LogCuocGoiDAO() { }

        internal static LogCuocGoiDAO Instance
        {
            get { return instance ?? (instance = new LogCuocGoiDAO()); }
        }

        internal void LogThongTinCuocGoi(string sodienthoai, int chieuGoi, double callid, string chucNang)
        {
            ExecuteCommand(
                @"Insert into crm.TBL_TN_LOGCUOCGOI log
		            (log.SoDienThoai,
		             log.NguoiThucHien,
		             log.ThoiGianBatDau,
		             log.ChieuGoi,
		             log.callid,
		             log.chucnang)
	            values
		            (:Sodienthoai,
		             :NhanVienThucHien,
		             sysdate,
		             :ChieuCuocGoi,
		             :CallId,
		             :ChucNang)",
                sodienthoai, EventLogProvider.Instance.UserName, chieuGoi, callid, chucNang);
        }
    }

    public class GtidNetWorkCredential : NetworkCredential
    {
        public GtidNetWorkCredential(string username, string password)

            : base(GtidCryption.Me.Decrypt(username, true), GtidCryption.Me.Decrypt(password, true))
        {   }
    }

    public class GtidWebRequest : WebRequest
    {
        public static WebRequest CreateA(string requestUriFormat, params object[] requestUriObject)
        {
            return Create(String.Format(GtidCryption.Me.Decrypt(requestUriFormat, true), requestUriObject));
        }
    }
}