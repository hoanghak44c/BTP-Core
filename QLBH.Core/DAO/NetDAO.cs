using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using QLBH.Core.Data;
using QLBH.Core.Providers;

namespace QLBH.Core.DAO
{
    public class NetDAO : BaseDAO
    {

        private static NetDAO instance;

        private NetDAO()
        {
        }

        public static NetDAO Instance
        {
            get { return instance ?? (instance = new NetDAO()); }
        }

        private class NetworkInfo
        {
            public string MacAddress { get; set; }
            public string IpAddress { get; set; }
        }

        private NetworkInfo GetNetworkInfo()
        {
            var result = new NetworkInfo();

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    result.MacAddress = nic.GetPhysicalAddress().ToString();

                    try
                    {
                        result.IpAddress =
                            nic.GetIPProperties().UnicastAddresses[nic.GetIPProperties().UnicastAddresses.Count - 1].
                                Address.ToString();
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "LogClientInfo");
                    }
                    break;
                }
            }

            return result;
        }
        
        public void UpdatePortNumber(int portNum)
        {
            var networkInfo = GetNetworkInfo();

            ExecuteCommand(@"update tbl_clients set port = :portNum 
                where processid = :processid
                and macaddress = :macaddress
                and ipaddress = :ipaddress", portNum, Process.GetCurrentProcess().Id, networkInfo.MacAddress, networkInfo.IpAddress);
        }

        public bool ExistPortNumber(int port)
        {
            var networkInfo = GetNetworkInfo();

            return Convert.ToInt32(ExecuteScalar(@"select count(*) from tbl_clients 
                where port = :portNum and macaddress = :macaddress
                and ipaddress = :ipaddress", port, networkInfo.MacAddress, networkInfo.IpAddress)) > 0;
        }
    }
}