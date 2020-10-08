using System;
using System.Diagnostics;
using System.Net;
using QLBH.Core.DAO;
using QLBH.Core.Version;

namespace QLBH.Core.Providers
{
    public class EventLogProvider
    {

        private static EventLogProvider instance;

        private EventLogProvider()
        {
        }

        //public double CurrentVersion { get; set; }

        public string UserName { get; set; }

        public static EventLogProvider Instance
        {
            get
            {
                if (instance == null) instance = new EventLogProvider();
                return instance;
            }
        }

        public void WriteLog(string description, string type)
        {
            if(String.IsNullOrEmpty(description)) return;

            Process currentProcess = Process.GetCurrentProcess();
            string hostName = Dns.GetHostName();
            string ipAddress = String.Empty;
            
            IPAddress[] ipAddresses = Dns.GetHostEntry(hostName).AddressList;
            foreach (IPAddress address in ipAddresses)
            {
                ipAddress += address + ",";
            }

            EventLogDAO.Instance.WriteLog(description + "\nCurrent Version: " + VerBase.CurrentVersion
                + "\nUserName:" + UserName
                + "\n" + hostName + "(" + ipAddress + ")"
                + "\nProcessName:" + currentProcess.ProcessName
                + "\nMachineName:" + currentProcess.MachineName
                + "\nPID:" + currentProcess.Id
                + "\nSID:" + currentProcess.SessionId
                + "\n" + Environment.OSVersion, type, description.GetHashCode());
        }

        public void WriteOfflineLog(string description, string type)
        {
            Process currentProcess = Process.GetCurrentProcess();
            string hostName = Dns.GetHostName();
            string ipAddress = String.Empty;
            IPAddress[] ipAddresses = Dns.GetHostEntry(hostName).AddressList;
            foreach (IPAddress address in ipAddresses)
            {
                ipAddress += address + ",";
            }

            if (type == "DBLOG")
                EventLogDAO.Instance.WriteOfflineLog(description, type);
            else
                EventLogDAO.Instance.WriteOfflineLog(description + "\nCurrent Version: " + VerBase.CurrentVersion
                                                     + "\nUserName:" + UserName
                                                     + "\n" + hostName + "(" + ipAddress + ")"
                                                     + "\nProcessName:" + currentProcess.ProcessName
                                                     + "\nMachineName:" + currentProcess.MachineName
                                                     + "\nPID:" + currentProcess.Id
                                                     + "\nSID:" + currentProcess.SessionId
                                                     + "\n" + Environment.OSVersion, type);
        }
    }
}