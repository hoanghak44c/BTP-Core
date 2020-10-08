// <summary>
// Mô tả class: Lớp đối tượng thực hiện việc kết nối dữ liệu
// </summary>
// <remarks>
// Người tạo: Nguyen Gia Dang
// Ngày tạo: 03/10/2007
// </remarks>

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
//using Microsoft.ReportingServices.Diagnostics.Utilities;
using Oracle.DataAccess.Client;
//using QLBH.Common;
using QLBH.Core.Business.Calculations;
using QLBH.Core.Exceptions;
using QLBH.Core.Form;
using QLBH.Core.Properties;
using QLBH.Core.Providers;
using ThreadState = System.Threading.ThreadState;

namespace QLBH.Core.Data
{
    public class ConnectionUtil{

        internal class ManagedObject
        {
            internal ManagedObject()
            {
                Id = Thread.CurrentThread.ManagedThreadId;
            }

            public int Id { get; private set; }
            public GtidConnection Connection { get; set; }
            public GtidTransaction Transaction { get; set; }
            public bool IsRunningQuery { get; set; }
            public DateTime LastActionTime { get; set; }
        }

        private readonly Thread checkTimeOutConnThread;

        #region Constructor
        private ConnectionUtil()
        {
            //checkTimeOutConnThread = new Thread(CheckTimeOutConnection);
            //checkTimeOutConnThread.Start();
            Id = Thread.CurrentThread.ManagedThreadId;
        }

        #endregion

        public bool IsTimeOutApp;
        private DateTime lastTimeQuery;

        //Using skeleton parten design
        //private static ConnectionUtil instance;

        //private ManagedObject 
        //private static int i = 0;
        [MethodImpl(MethodImplOptions.Synchronized)]
        private GtidConnection getCurrentConnection()
        {
            if (ManagedObjects != null && ManagedObjects.Count > 0)
            {
                for (int i = 0; i < ManagedObjects.Count; i++)
                {
                    if (ManagedObjects[i].Id == Thread.CurrentThread.ManagedThreadId)
                    {
                        return ManagedObjects[i].Connection;
                    }
                }

                //for (int i = 0; i < ManagedObjects.Count; i++)
                //{
                //    if (ManagedObjects[i].Thread.ThreadState == ThreadState.Stopped &&
                //        ManagedObjects[i].Connection.State == ConnectionState.Open)
                //    {
                //        ManagedObjects[i].Thread = Thread.CurrentThread;

                //        return ManagedObjects[i].Connection;
                //    }
                //}

                //for (int i = 0; i < ManagedObjects.Count; i++)
                //{
                //    if (ManagedObjects[i].Thread.ThreadState == ThreadState.Stopped)
                //    {
                //        ManagedObjects[i].Thread = Thread.CurrentThread;

                //        return ManagedObjects[i].Connection;
                //    }
                //}
            }
            return null;
        }

        private GtidConnection CurrentConnection
        { 
            get { return getCurrentConnection(); }
            set
            {
                if(ManagedObjects == null) ManagedObjects = new List<ManagedObject>();
                if (!ManagedObjects.Exists(delegate(ManagedObject match)
                                               {
                                                   return match.Id == Thread.CurrentThread.ManagedThreadId;
                                               }))
                {
                    ManagedObjects.Add(new ManagedObject {Connection = value, LastActionTime = DateTime.Now});
                } 
                else
                {
                    ManagedObjects.Find(delegate(ManagedObject match) { return match.Id == Thread.CurrentThread.ManagedThreadId; }).
                        Connection = value;
                }
            }
        }

        private static ManagedObject getCurrentManagedObject(int? index)
        {
            if (ManagedObjects == null) return null;

            if (index == null)
                return
                    ManagedObjects.Find(delegate(ManagedObject match) { return match.Id == Thread.CurrentThread.ManagedThreadId; });

            return ManagedObjects[(int)index];
        }

        /// <summary>
        /// Get current object
        /// </summary>
        internal static ManagedObject CurrentManagedObject
        {
            get { return getCurrentManagedObject(null); }
        }

        private delegate void DoForEachManagedObjectDelegate(ManagedObject matchObject);

        private static void DoForEachManagedObject(DoForEachManagedObjectDelegate workForEach)
        {
            for (int? i = 0; i < ManagedObjects.Count; i++ )
            {
                workForEach.Invoke(getCurrentManagedObject(i));
            }
        }

        internal static List<ManagedObject> ManagedObjects;

        //private static IDbTransaction SqlTran = null;
        bool isConnected = false;
        private static List<ConnectionUtil>  lstInstance;
        public int Id { get; private set; }
        public static ConnectionUtil Instance
        {
            get
            {
                if(lstInstance == null) lstInstance = new List<ConnectionUtil>();
                
                if(!lstInstance.Exists(
                    delegate(ConnectionUtil match) { return match.Id == Thread.CurrentThread.ManagedThreadId; }))

                    lstInstance.Add(new ConnectionUtil());

                return lstInstance.Find(
                    delegate(ConnectionUtil match) { return match.Id == Thread.CurrentThread.ManagedThreadId; });
            }
        }

        private static int isUat;
        /// <summary>
        /// 1:UAT; 2:Test1; 3:Test
        /// </summary>
        public int IsUAT
        {
            get { return isUat; }

            set { isUat = value; }
        }

        public DateTime LastTimeQuery
        {
            get
            {
                ManagedObjects.ForEach(
                    delegate(ManagedObject managedObject)
                    {
                        if (managedObject.LastActionTime > lastTimeQuery)
                            lastTimeQuery = managedObject.LastActionTime;
                    });
                
                return lastTimeQuery;
            }
        }

        internal List<string> AutoQueryString = new List<string>
                                                    {
                                                        "sp_DieuChuyen_CheckChanged1", 
                                                        "sp_GetSysDate", 
                                                        "sp_GetVersion",
                                                        "select sysdate from dual",
                                                        "select version from tbl_thamso_chung where type = :pType"
                                                    };

        private static string connectionString = String.Empty;

        internal string GetConnectionString3()
        {
            return GtidCryption.Me.Decrypt(
                "o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScO+wmGH04brmfhVMprg9ILuEdM6XcQT6GmVmvSSG0GyC1EpLe7fjIRn91TuZZ6MbU2JDPRTq6W7mHCd+6dXnSefu1FtESR6hE4KIRBXIo8OGApcSMVbbX06OlRcW00umJ9pR2T0DrmNPm2zbErN02Z2sw0yV2kbg8gJ56LGG7LTZ83WUqv5xUiCJfJEMcuUFkU2U/dN3mJcB0faKjJnARv8gBYNWNsNqeTAOV169Ju2lX+b8yEQ+wEUzvA5IAnJTdN/nnxlNMPEI=",
                true);
        }

        internal string GetConnectionString2()
        {
            return "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "\\Data\\;Extended Properties=\"text;HDR=YES\"";
        }

        internal string GetConnectionString()
        {
            if (IsUAT == 0)
                MessageBox.Show("Bạn chưa chọn ứng dụng.");

            string sConnectionStringWebServiceForDev =
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScTvKudkkiC5pwukAQD/nsK6eaq5eL20Ioz7uDs7D0MUR1JujD8AOMUHm2vjCEg16pzBhfPd0xKMNsbLTSMrMe3K7bUBNKiaHorkmEB4R5dF8tWXi+HEnSkAs132i0X7s3Oq2Ksn3UwLjXYJY8zjPFjPKOXvG/xUAaOeAd6kAqE7XwU3TGugq1UiI/DiAWJH2zqyzFphe+643wU3TGugq1UgybsscpsyE5GRfVPyHNF8KTVZeAtek4hxaKVZlrwVNR"; // may 16
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScJIZJr45bABA4lM18XoETpeE1gFKOkrNYCYFCS8CURK2sSjvKR6M9CaaRkYkejuEvaA3M1B/8IoPIWsQsKVFbFrjHT3zYPxV7xdK+vq3hQkXpNHsvFhoGOUBUzylOssN/r12XMNnSanYBN7HPLhp970zvA5IAnJTdU1ASNqXUmmCDYdvUbTeqAUV4CwaOp2CCU1ASNqXUmmDHvbu3M6tUQ1pi0nby8f7M5HFSSy1tZOB5y8v/6d5hW644mbnZjN6o"; //build rieng cho TrungNT
                "o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8LE9VJ8XaLtfCRcHTCljNtgT6yt3bkAb/uEdM6XcQT6GmVmvSSG0GyC1EpLe7fjIRn91TuZZ6MbU2JDPRTq6W7mHCd+6dXnSeSwqPZvB/Rge+p/ijLs74nnnu7i61zTnJ1ty9jFU/B1qNiPDG2zQjKicv2N3YxcGGehE+1jVPT8hyLG59vEj1Oc5FctBtWNUXWnThYtv4bgMyLG59vEj1OfpTSRcXRBT6EYV4ks2JyNQ8o5e8b/FQBpa4gKQe3XIqw=="; //server test cua Tuan

                //test 1 ip
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScJIZJr45bABB0SQoN891ZzeE1gFKOkrNYCYFCS8CURK2sSjvKR6M9CaaRkYkejuEvaA3M1B/8IoPIWsQsKVFbFrjHT3zYPxV7vzBXlGIDfM2ApcSMVbbX06OlRcW00umJSAYhFEpkaMicGEOnh7/h5/KOXvG/xUAaOeAd6kAqE7XwU3TGugq1UiI/DiAWJH2zqyzFphe+643wU3TGugq1UgybsscpsyE5GRfVPyHNF8KTVZeAtek4hxaKVZlrwVNR"; //build rieng cho TrungNT
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScJIZJr45bABB0SQoN891ZzeE1gFKOkrNYCYFCS8CURK2sSjvKR6M9CaaRkYkejuEvaA3M1B/8IoPIWsQsKVFbFrjHT3zYPxV7xdK+vq3hQkXpNHsvFhoGOUBUzylOssN/r12XMNnSanYBN7HPLhp970zvA5IAnJTdU1ASNqXUmmCDYdvUbTeqAUV4CwaOp2CCU1ASNqXUmmDHvbu3M6tUQ1pi0nby8f7M5HFSSy1tZOB5y8v/6d5hW644mbnZjN6o";
                //test 2 ip
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScJIZJr45bABB0SQoN891ZzeE1gFKOkrNYJL9j7XcixftQOGzPLGwUJKboVsyxQXFNfAqBd7wbNkRwQNuVHYJ7aiSWWyBitIVh5FOBviiRaWoPOR91VzPGliUVOAtLuqIM31qG8NIGz6p/QGONnsa6UfUXS+Sttf1WbiwmnGMI6PveukqYH6SrYCtBxRneMstVJCoHEsi7cS2cv2N3YxcGGehE+1jVPT8hyLG59vEj1Oc5FctBtWNUXWnThYtv4bgMyLG59vEj1OfpTSRcXRBT6EYV4ks2JyNQ8o5e8b/FQBpa4gKQe3XIqw==";

            //test do serial
            string sConnectionStringWebServiceForDev1 =
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8BXYmF5u8sONWJaXf8m2h6oxvjNWBgcDwOEdM6XcQT6GmVmvSSG0GyC1EpLe7fjIRn91TuZZ6MbU2JDPRTq6W7mHCd+6dXnSefu1FtESR6hEWMnE+w4CMW6lap/1SxvWzz9ZDAd21lh9DUHWXbe0GK4rHN+M9URURpy/Y3djFwYZ6ET7WNU9PyHIsbn28SPU5zkVy0G1Y1RdadOFi2/huAzIsbn28SPU5+lNJFxdEFPoRhXiSzYnI1Dyjl7xv8VAGlriApB7dcir";//subserver
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScTvKudkkiC5pwukAQD/nsK6eaq5eL20Ioz7uDs7D0MUR1JujD8AOMUHm2vjCEg16pzBhfPd0xKMNsbLTSMrMe3K7bUBNKiaHorkmEB4R5dF8tWXi+HEnSkAs132i0X7s3Oq2Ksn3UwLjXYJY8zjPFjPKOXvG/xUAaOeAd6kAqE7XwU3TGugq1UiI/DiAWJH2zqyzFphe+643wU3TGugq1UgybsscpsyE5GRfVPyHNF8KTVZeAtek4hxaKVZlrwVNR"; // may 16
                "o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8LE9VJ8XaLtfCRcHTCljNtgT6yt3bkAb/uEdM6XcQT6GmVmvSSG0GyC1EpLe7fjIRn91TuZZ6MbU2JDPRTq6W7mHCd+6dXnSeSwqPZvB/Rge+p/ijLs74nnnu7i61zTnJ1ty9jFU/B1qNiPDG2zQjKicv2N3YxcGGehE+1jVPT8hyLG59vEj1Oc5FctBtWNUXWnThYtv4bgMyLG59vEj1OfpTSRcXRBT6EYV4ks2JyNQ8o5e8b/FQBpa4gKQe3XIqw=="; //server test cua Tuan
            
            string sConnectionStringWebServiceForUAT =
                "o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8LE9VJ8XaLtfCRcHTCljNtgT6yt3bkAb/uEdM6XcQT6GmVmvSSG0GyC1EpLe7fjIRn91TuZZ6MbU2JDPRTq6W7mHCd+6dXnSeSwqPZvB/Rge+p/ijLs74nnnu7i61zTnJ1ty9jFU/B1qNiPDG2zQjKicv2N3YxcGGehE+1jVPT8hyLG59vEj1Oc5FctBtWNUXWnThYtv4bgMyLG59vEj1OfpTSRcXRBT6EYV4ks2JyNQ8o5e8b/FQBpa4gKQe3XIqw=="; //server test cua Tuan
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScO+wmGH04brlwukAQD/nsK6eaq5eL20Ioz7uDs7D0MUR1JujD8AOMUHm2vjCEg16pzBhfPd0xKMNsbLTSMrMe3K7bUBNKiaHorkmEB4R5dF8tWXi+HEnSkAs132i0X7s3Oq2Ksn3UwLjXYJY8zjPFjPKOXvG/xUAaOeAd6kAqE7XwU3TGugq1UiI/DiAWJH2zqyzFphe+643wU3TGugq1UgybsscpsyE5GRfVPyHNF8KTVZeAtek4hxaKVZlrwVNR"; //addr
                //test 1 ip
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScJIZJr45bABA4lM18XoETpeE1gFKOkrNYCYFCS8CURK2sSjvKR6M9CaaRkYkejuEvaA3M1B/8IoPIWsQsKVFbFrjHT3zYPxV7xdK+vq3hQkXpNHsvFhoGOUBUzylOssN/r12XMNnSanYBN7HPLhp970zvA5IAnJTdU1ASNqXUmmCDYdvUbTeqAUV4CwaOp2CCU1ASNqXUmmDHvbu3M6tUQ1pi0nby8f7M5HFSSy1tZOB5y8v/6d5hW644mbnZjN6o"; //build rieng cho TrungNT
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScJIZJr45bABB0SQoN891ZzeE1gFKOkrNYCYFCS8CURK2sSjvKR6M9CaaRkYkejuEvaA3M1B/8IoPIWsQsKVFbFrjHT3zYPxV7xdK+vq3hQkXpNHsvFhoGOUBUzylOssN/r12XMNnSanYBN7HPLhp970zvA5IAnJTdU1ASNqXUmmCDYdvUbTeqAUV4CwaOp2CCU1ASNqXUmmDHvbu3M6tUQ1pi0nby8f7M5HFSSy1tZOB5y8v/6d5hW644mbnZjN6o";
                //test 2 ip
                //"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScJIZJr45bABB0SQoN891ZzeE1gFKOkrNYJL9j7XcixftQOGzPLGwUJKboVsyxQXFNfAqBd7wbNkRwQNuVHYJ7aiSWWyBitIVh5FOBviiRaWoPOR91VzPGliUVOAtLuqIM31qG8NIGz6p/QGONnsa6UfUXS+Sttf1WbiwmnGMI6PveukqYH6SrYCtBxRneMstVJCoHEsi7cS2cv2N3YxcGGehE+1jVPT8hyLG59vEj1Oc5FctBtWNUXWnThYtv4bgMyLG59vEj1OfpTSRcXRBT6EYV4ks2JyNQ8o5e8b/FQBpa4gKQe3XIqw==";

            string sConnectionStringWebServiceTuan =
                "o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74fzX7pyJ12ymxsfRh/JKbebapyl+1zS9VHeH5FLnscMCgV4ySxE8t8MaIl/N8enScO+wmGH04brlwukAQD/nsK6eaq5eL20Ioz7uDs7D0MUR1JujD8AOMUHm2vjCEg16pzBhfPd0xKMNsbLTSMrMe3FRhCk51E7NTeNYBiK4KoedPx6QXYHfaQ8lbpbMLHhfH41CnszeZBRpjYuOC8beqQA==";

            string sConnectionString = sConnectionStringWebServiceForDev;  

            if (IsUAT == 1)
                sConnectionString = sConnectionStringWebServiceForUAT;

            if (IsUAT == 2)
                sConnectionString = sConnectionStringWebServiceForDev1;// sConnectionStringWebServiceLocal;// sConnectionStringWebServiceLocal;
            
            //try
            //{
            //    //return Settings.Default.QLBanHangOracleConnectionString;
            //    return sConnectionString;
            //}
            //catch (Exception ex)
            //{
            //    return sConnectionString;
            //}

            if(String.IsNullOrEmpty(connectionString))
            {
                connectionString = GtidCryption.Me.Decrypt(sConnectionString, true);
            }

            return connectionString;
        }

        internal bool IsConnected
        {
            get { return isConnected; }
        }

        private static bool isReConnecting;

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void ReTryConnect()
        {
            int MAX_INTERVAL = 10;
            int RetryTime = 1;
            int interval = MAX_INTERVAL;
            bool isThreadPending = false;
            isConnected = false;
            Configuration configuration;
            ExeConfigurationFileMap fileMap;
            try
            {
                //if (!frmProgress.Instance.IsRetryingConnect)
                    
                //    frmProgress.Instance.IsRetryingConnect = true;

                //else
                //{
                //    while (frmProgress.Instance.IsRetryingConnect)
                //    {
                //        //Debug.Print("{0} Thread Id: {1} is pending to retried connect...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                //        Thread.CurrentThread.Join(2000);
                //    }

                //    frmProgress.Instance.Value = frmProgress.Instance.MaxValue;

                //    frmProgress.Instance.IsCompleted = true;

                //    return;
                //}

                ////Debug.Print("{0} Thread Id: {1} check retry connecting...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                
                while (isReConnecting)
                {
                    ////Debug.Print("{0} Thread Id: {1} is reconnecting loop...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                    
                    isThreadPending = true;

                    Thread.CurrentThread.Join(2000);
                }

                ////Debug.Print("{0} Thread Id: {1} check pending...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                if (isThreadPending)
                {
                    ////Debug.Print("{0} Thread Id: {1} close pending...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                    frmProgress.Instance.Value = frmProgress.Instance.MaxValue;

                    frmProgress.Instance.IsCompleted = true;

                    return;
                }

                ////Debug.Print("{0} Thread Id: {1} retry connecting...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                ////Debug.Print("{0} Thread Id: {1} set label...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                frmProgress.Instance.Caption = "Kết nối máy chủ";

                frmProgress.Instance.Description = String.Format("Đang kết nối...");

                ////Debug.Print("{0} Thread Id: {1} open system configuration file...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                
                fileMap = new ExeConfigurationFileMap();

                fileMap.ExeConfigFilename = String.Format("{0}\\{1}.config", Application.StartupPath, Process.GetCurrentProcess().MainModule.ModuleName);

                if (!File.Exists(fileMap.ExeConfigFilename))
                {
                    ////Debug.Print("{0} Thread Id: {1} can not find system configuration file...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                    throw new ManagedException("Không tìm thấy file cấu hình hệ thống!", false);
                }
                
                configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            }
            catch (Exception ex)
            {
                //Debug.Print("{0} Thread Id: {1} error on retry connect...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                //Debug.Print(ex.ToString());

                throw;
            }

            while (!isConnected)
            {
                isReConnecting = true;
                if (CurrentConnection == null || CurrentConnection.IsDisposed)
                {
                    if (CurrentConnection == null)
                        CurrentConnection = new GtidOracleConnection(new OracleConnection(GetConnectionString()));
                    else if(CurrentConnection is GtidOracleProdConnection)
                        CurrentConnection = new GtidOracleProdConnection(new OracleConnection(GetConnectionString()));
                    else if (CurrentConnection is GtidOracleStByConnection)
                        CurrentConnection = new GtidOracleStByConnection(new OracleConnection(GetConnectionString3()));
                }
                try
                {
                    frmProgress.Instance.Description = String.Format("Đang kết nối...");

                    try
                    {

                        if (configuration.AppSettings.Settings["PendingProcess"] == null ||
                            configuration.AppSettings.Settings["PendingProcess"].Value != Process.GetCurrentProcess().Id.ToString())
                        {
                            try
                            {
                                if (Process.GetProcessById(Convert.ToInt32(configuration.AppSettings.Settings["PendingProcess"].Value)).
                                    MainModule.ModuleName == Process.GetCurrentProcess().MainModule.ModuleName)

                                    throw new ManagedException(
                                        "Hệ thống vẫn đang thiết lập kết nối đến máy chủ, đề nghị bạn hãy vui lòng chờ ít phút nữa.", false);
                            }
                            catch (Exception ex)
                            {
                                if (ex is ArgumentException || ex is FormatException || 
                                    ex is NullReferenceException || ex is Win32Exception)
                                {
                                    if (ex is NullReferenceException)
                                        configuration.AppSettings.Settings.Add("PendingProcess", Process.GetCurrentProcess().Id.ToString());
                                    else
                                        configuration.AppSettings.Settings["PendingProcess"].Value =
                                            Process.GetCurrentProcess().Id.ToString();

                                    try
                                    {
                                        configuration.Save();
                                    }
                                    catch (ConfigurationErrorsException cfgException)
                                    {
                                        if (cfgException.Message == "The configuration file has been changed by another program")
                                        {
                                            configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                                            
                                            if (configuration.AppSettings.Settings["PendingProcess"] == null)
                                                configuration.AppSettings.Settings.Add("PendingProcess", Process.GetCurrentProcess().Id.ToString());
                                            else
                                                configuration.AppSettings.Settings["PendingProcess"].Value =
                                                    Process.GetCurrentProcess().Id.ToString();

                                            configuration.Save();
                                        }
                                    }
                                } else throw;
                            }
                        }


                        ////Debug.Print("{0} Thread Id: {1} retry connecting...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                        
                        CurrentManagedObject.IsRunningQuery = true;
                        
                        if(!CurrentConnection.IsConnectedToServer)
                        {
                            ////Debug.Print("{0} Thread Id: {1} Không ping được máy chủ...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                            throw new ManagedException("Kết nối máy chủ chưa thông", false);
                        }

                        if (CurrentConnection.State == ConnectionState.Open) CurrentConnection.Close();
                        CurrentConnection.Open();
                        CurrentConnection.Close();

                        try
                        {
                            configuration.AppSettings.Settings["PendingProcess"].Value = String.Empty;

                            configuration.Save();
                        }
                        catch (ConfigurationErrorsException cfgException)
                        {
                            if (cfgException.Message == "The configuration file has been changed by another program")
                            {
                                configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                                configuration.AppSettings.Settings["PendingProcess"].Value = String.Empty;

                                configuration.Save();
                            }
                        }

                        CurrentManagedObject.IsRunningQuery = false;

                        DoForEachManagedObject(delegate(ManagedObject managedObject)
                        {
                            if (managedObject.Id != Thread.CurrentThread.ManagedThreadId)
                                if (managedObject.IsRunningQuery &&
                                    managedObject.Connection.State == ConnectionState.Open &&
                                    (managedObject.Transaction == null ||
                                     managedObject.Transaction.Connection.InnerConnection != null))
                                {
                                    try
                                    {
                                        managedObject.Connection.Close();
                                        Thread.Sleep(2000);
                                        managedObject.Connection.Open();
                                        Thread.Sleep(2000);
                                    }
                                    catch (InvalidOperationException) { }
                                }
                        });
                        
                        ////Debug.Print("{0} Thread Id: {1} connect successfull...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                        Thread.Sleep(3000);
                        isConnected = true;
                        frmProgress.Instance.Value = frmProgress.Instance.MaxValue;
                        frmProgress.Instance.IsCompleted = true;

                        isReConnecting = false;

                    }
                    catch(Exception ex)
                    {
                        if(ex is IOException || ex is ManagedException)
                        {
                            Thread.CurrentThread.Join(5000);
                            
                            ////Debug.Print("{0} Thread Id: {1} Kết nối máy chủ chưa thông...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                            
                            frmProgress.Instance.Description = String.Format("Kết nối máy chủ chưa thông");
                            
                            Thread.CurrentThread.Join(3000);
                            
                            if (RetryTime % 2 == 0)
                            {
                                ////Debug.Print("{0} Thread Id: {1} Xác nhận kết nối lại...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                                if (Environment.UserInteractive && 
                                    MessageBox.Show("Kết nối với máy chủ hiện tại đang bị gián đoạn, bạn có muốn tiếp tục thử lại không?", "Xác nhận", MessageBoxButtons.RetryCancel,
                                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                                {
                                    Environment.Exit(124);
                                }
                                MAX_INTERVAL = Environment.UserInteractive ? 10 : (10 * new Random(1).Next(1, 100));
                                RetryTime = 1;
                            }

                            interval = MAX_INTERVAL;
                            
                            ////Debug.Print("{0} Thread Id: {1} Sẽ thử lại trong vòng {2} giây nữa....", DateTime.Now, Thread.CurrentThread.ManagedThreadId, interval);
                            
                            frmProgress.Instance.Value = 0;
                            
                            frmProgress.Instance.MaxValue = MAX_INTERVAL;
                            
                            while (interval > 0)
                            {
                                ////Debug.Print("{0} Thread Id: {1} Sẽ thử lại trong vòng {2} giây nữa....", DateTime.Now, Thread.CurrentThread.ManagedThreadId, interval);
                                
                                frmProgress.Instance.Description = String.Format("Sẽ thử lại trong vòng {0} giây nữa.", interval);
                                
                                frmProgress.Instance.Value += 1;
                                
                                interval -= 1;
                                
                                Thread.CurrentThread.Join(1000);
                            }
                            MAX_INTERVAL += 5;
                            RetryTime += 1;
                            isConnected = false;
                        } 
                        else
                        {
                            //Debug.Print(ex.ToString());

                            throw;
                        }
                    }
                }
                catch (OracleException oracleException)
                {
                    ////Debug.Print("{0} Thread Id: {1} error on retry connect 1...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                    //EventLogProvider.Instance.WriteOfflineLog(oracleException.ToString(), "Retry connect");
                    switch (oracleException.Number)
                    {
                        case 12571: //TNS:packet writer failure
                        case 12560: //TNS:protocol adapter error
                        case 12543: //TNS:destination host unreachable.
                        case 12514: //TNS:listener does not currently know of service requested in connect descriptor
                        case 12170: //TNS:Connect timeout occurred
                        case 3135: //connection lost contact
                        case 3113: //end-of-file on communication channel
                        case 3114: //not connected to ORACLE
                        case 1033: //ORACLE initialization or shutdown in progress
						case 1034: //ORACLE not available
                        case -1000: //Connection request timed out
                        case -3000: //Data provider internal error
						case 28547: //connection to server failed, probable Oracle Net admin error
                        case 12528: //TNS:listener: all appropriate instances are blocking new connections
                        case 12518: //TNS:listener could not hand off client connection
                        case 12541: //TNS:no listener
                        case 12505: //TNS:listener does not currently know of SID given in connect descriptor
                        case 00600: //internal errors
                            Thread.CurrentThread.Join(5000);

                            ////Debug.Print("{0} Thread Id: {1} Kết nối máy chủ chưa thông...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                            frmProgress.Instance.Description = String.Format("Kết nối máy chủ chưa thông");
                            
                            Thread.CurrentThread.Join(3000);

                            if (RetryTime % 2 == 0)
                            {
                                ////Debug.Print("{0} Thread Id: {1} Xác nhận kết nối lại...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                                if (Environment.UserInteractive &&
                                    MessageBox.Show("Kết nối với máy chủ hiện tại đang bị gián đoạn, bạn có muốn tiếp tục thử lại không?", "Xác nhận", MessageBoxButtons.RetryCancel,
                                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                                {
                                    Environment.Exit(124);
                                }
                                MAX_INTERVAL = 10;
                                RetryTime = 1;
                            }

                            frmProgress.Instance.Value = 0;
                            frmProgress.Instance.MaxValue = MAX_INTERVAL;
                            interval = MAX_INTERVAL;
                            while (interval > 0)
                            {
                                ////Debug.Print("{0} Thread Id: {1} Sẽ thử lại trong vòng {2} giây nữa....", DateTime.Now, Thread.CurrentThread.ManagedThreadId, interval);

                                frmProgress.Instance.Description = String.Format("Sẽ thử lại trong vòng {0} giây nữa.", interval);
                                
                                frmProgress.Instance.Value += 1;
                                
                                interval -= 1;
                                
                                Thread.CurrentThread.Join(1000);
                            }
                            MAX_INTERVAL += 5;
                            RetryTime += 1;
                            isConnected = false;
                            break;
                        default:
                            throw;
                    }
                }
                catch(Exception ex)
                {
                    ////Debug.Print("{0} Thread Id: {1} error on retry connect 2...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                    //Debug.Print(ex.ToString());

                    //EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "Retry connect");

                    if(Environment.UserInteractive) MessageBox.Show(ex.Message);
                }
            }

            frmProgress.Instance.Value = frmProgress.Instance.MaxValue;
            frmProgress.Instance.IsCompleted = true;
            frmProgress.Instance.IsRetryingConnect = false;
            ////Debug.Print("{0} Thread Id: {1} exit connecting...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
        }

        internal GtidConnection GetConnection2()
        {
            if (CurrentConnection == null || CurrentConnection.IsDisposed || !(CurrentConnection is GtidOleDbConnection))
            {
                CurrentConnection = new GtidOleDbConnection(new OleDbConnection(GetConnectionString2()));
            }

            CurrentConnection.OpenIfClosed();

            return CurrentConnection;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>return opened connection</returns>
        internal GtidConnection GetConnection3()
        {
            try
            {
                if (CurrentConnection == null || CurrentConnection.IsDisposed || !(CurrentConnection is GtidOracleStByConnection))
                {
                    CurrentConnection = new GtidOracleStByConnection(new OracleConnection(GetConnectionString3()));
                }

                CurrentConnection.OpenIfClosed();

                return CurrentConnection;
            }
            catch (OracleException)
            {
                //throw;
                if (Environment.UserInteractive)
                {
                    isConnected = false;

                    //Debug.Print("{0} Thread Id: {1} retry connect on GetConnection...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                    frmProgress.Instance.DoWork(ReTryConnect);

                    while (!IsConnected)
                    {
                        Thread.SpinWait(1000);
                    }
                    return CurrentConnection;
                }
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>return opened connection</returns>
        public GtidConnection GetConnection()
        {
            try
            {
                if (CurrentConnection == null || CurrentConnection.IsDisposed || !(CurrentConnection is GtidOracleProdConnection))
                {
                    CurrentConnection = new GtidOracleProdConnection(new OracleConnection(GetConnectionString()));
                }
                
                CurrentConnection.OpenIfClosed();
                                                
                return CurrentConnection;
            }
            catch (OracleException)
            {
                //throw;
                if(Environment.UserInteractive)
                {
                    isConnected = false;
                    
                    //Debug.Print("{0} Thread Id: {1} retry connect on GetConnection...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                    
                    frmProgress.Instance.DoWork(ReTryConnect);
                    
                    while (!IsConnected)
                    {
                        Thread.SpinWait(1000);
                    }
                    return CurrentConnection;                    
                }
                throw;
            }
        }

        public void CloseConnections()
        {
            if (ManagedObjects != null && ManagedObjects.Count > 0)
            {
                DoForEachManagedObject(delegate(ManagedObject managedObject)
                {
                    managedObject.Connection.Close();
                });
            }
        }

        public GtidTransaction BeginTransaction()
        {
            try
            {
                if (CurrentConnection == null || CurrentConnection.IsDisposed)
                {
                    GetConnection();
                    //throw new System.InvalidOperationException("Bạn chưa tạo connection.");
                }
                if (CurrentTransaction != null && (CurrentTransaction.Connection == null || CurrentTransaction.Connection.IsDisposed)) CurrentTransaction.Dispose();
                
                CurrentManagedObject.IsRunningQuery = true;

                CurrentConnection.OpenIfClosed();

                if (!IsInTransaction) CurrentTransaction = CurrentConnection.BeginTransaction(IsolationLevel.ReadCommitted);

                return CurrentTransaction;
            }
            catch (OracleException oracleException)
            {
                //throw;
                switch (oracleException.Number)
                {
                    case 12571:
                    case 12560:
                    case 12543:
                    case 12514:
                    case 12170:
                    case 3135:
                    case 3114:
                    case -1000:
                    case -3000:
                    case 28547:
                    case 12528:
                    case 12518:
                    case 12541:
                    case 12505:
                    case 00600:
                        {
                            //Debug.Print("{0} Thread Id: {1} retry connect on BeginTransaction...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                            
                            frmProgress.Instance.DoWork(ReTryConnect);
                            
                            return BeginTransaction();                            
                        }    
                }
                //EventLogProvider.Instance.WriteOfflineLog(oracleException.ToString(), "Begin Transaction.");
                throw;
            } 
            catch(Exception ex)
            {
                //EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "Begin Transaction.");
                throw;                
            }
        }
        
        public delegate void SerializableWorkDelegate();

        public void DoSerializableWorkInTransaction(SerializableWorkDelegate workTransUnit)
        {
            Exception innerException = null;
            frmProgress.Instance.Description = "Đang thực hiện ...";
            frmProgress.Instance.MaxValue = 100;
            frmProgress.Instance.Value = 0;
            SerializableWorkDelegate workUnit = workTransUnit;
            frmProgress.Instance.DoWork(
                delegate
                    {
                        bool isCompleted = false;

                        while (!isCompleted)
                        {
                            try
                            {
                                frmProgress.Instance.Value = 0;

                                if (CurrentConnection == null || CurrentConnection.IsDisposed) GetConnection();
                                
                                frmProgress.Instance.Value += 1;

                                BeginSerializableTransaction();

                                frmProgress.Instance.Value += 1;

                                workUnit.Invoke();

                                frmProgress.Instance.Value += 1;

                                CommitTransaction();

                                frmProgress.Instance.Value += 1;

                                frmProgress.Instance.Description = "Đã thực hiện thành công!!!";

                                frmProgress.Instance.Value = frmProgress.Instance.MaxValue;

                                Thread.CurrentThread.Join(2500);

                                isCompleted = true;

                                //frmProgress.Instance.IsCompleted = true;

                            }
                            catch (OracleException oracleException)
                            {
                                RollbackTransaction();
                                
                                if (oracleException.Number != 8177)
                                {
                                    innerException = oracleException;
                                    frmProgress.Instance.Description = "Không thực hiện thành công!";
                                    frmProgress.Instance.Value = frmProgress.Instance.MaxValue;
                                    Thread.CurrentThread.Join(2500);
                                    isCompleted = true;
                                    //frmProgress.Instance.IsCompleted = true;
                                } 
                                else
                                {
                                    innerException = null;
                                    frmProgress.Instance.Description = "Không thực hiện thành công!";
                                    frmProgress.Instance.Value = frmProgress.Instance.MaxValue;
                                    Thread.CurrentThread.Join(2500);
                                    frmProgress.Instance.Value = 0;

                                    if (MessageBox.Show("Hiện tại đang có nhiều giao dịch xảy ra đồng thời nên giao dịch của bạn khó thực hiện, bạn có muốn tiếp tục thử lại không?", "Xác nhận", 
                                        MessageBoxButtons.RetryCancel, 
                                        MessageBoxIcon.Question, 
                                        MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                                    {
                                        isCompleted = true;
                                        innerException = new ManagedException("Không thực hiện thành công!", false);
                                    }

                                    frmProgress.Instance.Description = "Đang thử lại...!";
                                    Thread.CurrentThread.Join(2500);   
                                }
                            }
                            catch (Exception exception)
                            {
                                RollbackTransaction();
                                innerException = exception;
                                frmProgress.Instance.Description = "Không thực hiện thành công!";
                                frmProgress.Instance.Value = frmProgress.Instance.MaxValue;
                                Thread.CurrentThread.Join(2500);
                                isCompleted = true;
                                //frmProgress.Instance.IsCompleted = true;
                            }
                        }

                        frmProgress.Instance.Value = frmProgress.Instance.MaxValue;
                        frmProgress.Instance.IsCompleted = true;

                    });

            if (innerException != null)
            {
                //if (!(innerException is ManagedException))
                    EventLogProvider.Instance.WriteOfflineLog(innerException.Message, "Do serializable work.");
                
                throw innerException;
            }
        }

        public GtidTransaction BeginSerializableTransaction()
        {
            try
            {
                if (CurrentConnection == null || CurrentConnection.IsDisposed)
                {
                    GetConnection();
                    //throw new System.InvalidOperationException("Bạn chưa tạo connection.");
                }
                if (CurrentTransaction != null && (CurrentTransaction.Connection == null || CurrentTransaction.Connection.IsDisposed)) CurrentTransaction.Dispose();

                CurrentManagedObject.IsRunningQuery = true;

                CurrentConnection.OpenIfClosed();

                if (!IsInTransaction) CurrentTransaction = CurrentConnection.BeginTransaction(IsolationLevel.ReadCommitted);

                return CurrentTransaction;
            }
            catch (OracleException oracleException)
            {
                //throw;
                switch (oracleException.Number)
                {
                    case 12571:
                    case 12560:
                    case 12543:
                    case 12514:
                    case 12170:
                    case 3135:
                    case 3114:
                    case -1000:
                    case -3000:
					case 28547:
                    case 12528:
                    case 12518:
                    case 12541:
                    case 12505:
                    case 00600:
                        {
                            //Debug.Print("{0} Thread Id: {1} retry connect on BeginSerializableTransaction...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                            frmProgress.Instance.DoWork(ReTryConnect);
                            
                            return BeginSerializableTransaction();
                        }
                }

                EventLogProvider.Instance.WriteOfflineLog(
                    oracleException.ToString(), "Begin Serializable Transaction.");
                throw;
            }
            catch (Exception ex)
            {
                EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "Begin Serializable Transaction.");
                throw;                
            }
        }

        public bool IsInTransaction
        {
            get {
                return CurrentTransaction != null && 
                    CurrentTransaction.Connection != null && 
                    CurrentTransaction.Connection.InnerConnection != null; 
            }
        }

        public bool HasTransaction
        {
            get
            {
                if (ManagedObjects != null && ManagedObjects.Count > 0)
                {
                    for (int i = 0; i < ManagedObjects.Count; i++)
                    {
                        return ManagedObjects[i].Transaction != null &&
                               ManagedObjects[i].Transaction.Connection != null &&
                               ManagedObjects[i].Transaction.Connection.InnerConnection != null;
                    }
                }
                return false;
            }
        }

        internal GtidTransaction CurrentTransaction
        {
            get
            {
                if (ManagedObjects != null && ManagedObjects.Count > 0)
                {
                    for (int i = 0; i < ManagedObjects.Count; i++)
                    {
                        if (ManagedObjects[i].Id == Thread.CurrentThread.ManagedThreadId)
                        {
                            return ManagedObjects[i].Transaction;
                        }
                    }
                    //for (int i = 0; i < ManagedObjects.Count; i++)
                    //{
                    //    if (ManagedObjects[i].Thread.ThreadState == ThreadState.Stopped)
                    //    {
                    //        ManagedObjects[i].Thread = Thread.CurrentThread;
                    //        return ManagedObjects[i].Transaction;
                    //    }
                    //}
                }
                return null;

                //return (GtidTransaction)SqlTran;
            }
            set
            {
                if (ManagedObjects == null) throw new ManagedException("Kết nối chưa được thiết lập!", false);
                
                ManagedObjects.Find(delegate(ManagedObject match)
                                        {
                                            return match.Id == Thread.CurrentThread.ManagedThreadId;
                                        }).Transaction = value;
            }
        }

        public void CommitTransaction() {
            try
            {
                if (CurrentConnection == null || CurrentConnection.IsDisposed) throw new System.InvalidOperationException("Transaction chưa thiết lập.");
                if (IsInTransaction)
                {
                    if (CurrentTransaction.IsNeedRollback) throw new Exception("Không thể hoàn thành giao dịch này.");

                    CurrentTransaction.Commit();
                    CurrentTransaction.Dispose();
                    CurrentManagedObject.IsRunningQuery = false;
                    CurrentManagedObject.LastActionTime = DateTime.Now;
                    CurrentConnection.Close();

                }
            }
            catch (Exception ex)
            {
                EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "Commit Transaction.");
                throw new ManagedException(ex.Message, false);
            }
        }

        public void RollbackTransaction() {
            try
            {
                if (CurrentConnection == null || CurrentConnection.IsDisposed) throw new System.InvalidOperationException("Transaction chưa thiết lập.");
                
                if (!IsInTransaction)
                {
                    EventLogProvider.Instance.WriteOfflineLog("No rollback.", "Rollback Transaction.");
                }
                
                if (IsInTransaction)
                {
                    CurrentTransaction.Rollback();
                    if (CurrentTransaction.IsNeedRollback) CurrentTransaction.IsNeedRollback = false;
                    if (CurrentTransaction != null) CurrentTransaction.Dispose();
                }

                CurrentManagedObject.IsRunningQuery = false;
                CurrentManagedObject.LastActionTime = DateTime.Now;
                CurrentConnection.Close();
            }
            catch (Exception ex)
            {
                EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "Rollback Transaction.");
                throw new ManagedException(ex.Message, false);
            }
        }
    }

    #region Parameter Classes

    public class GtidParameterCollection : DbParameterCollection
    {        
        private readonly IDataParameterCollection parameterCollection;
        private readonly IDbCommand ownerCommand;

        public GtidParameterCollection(IDataParameterCollection parameterCollection, IDbCommand ownerCommand)
        {
            this.parameterCollection = parameterCollection;
            this.ownerCommand = ownerCommand;
        }

        public new GtidParameter this[int index] 
        {
            get
            {
                object param = parameterCollection[index];
                if (param is GtidParameter)
                    return (GtidParameter)param;
                return new GtidParameter((IDbDataParameter)param);
            }

            set { parameterCollection[index] = value; }
        }

        public new GtidParameter this[string parameterName]
        {
            get
            {
                object param = parameterCollection[parameterName];
                if (param is GtidParameter)
                    return (GtidParameter)param;
                return new GtidParameter((IDbDataParameter)param);
            }

            set { parameterCollection[parameterName] = value; }

        }

        #region Implementation of IEnumerable

        /// <summary>
        /// Exposes the <see cref="M:System.Collections.IEnumerable.GetEnumerator"/> method, which supports a simple iteration over a collection by a .NET Framework data provider.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override IEnumerator GetEnumerator()
        {
            return parameterCollection.GetEnumerator();
        }

        /// <summary>
        /// Returns the <see cref="T:System.Data.Common.DbParameter"/> object at the specified index in the collection.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Common.DbParameter"/> object at the specified index in the collection.
        /// </returns>
        /// <param name="index">The index of the <see cref="T:System.Data.Common.DbParameter"/> in the collection.</param>
        protected override DbParameter GetParameter(int index)
        {
            object param = parameterCollection[index];
            if (param is GtidParameter)
                return (DbParameter)param;
            return new GtidParameter((IDbDataParameter)param);
        }

        /// <summary>
        /// Returns <see cref="T:System.Data.Common.DbParameter"/> the object with the specified name.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Common.DbParameter"/> the object with the specified name.
        /// </returns>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> in the collection.</param>
        protected override DbParameter GetParameter(string parameterName)
        {
            object param = parameterCollection[parameterName];
            if (param is GtidParameter)
                return (DbParameter)param;
            return new GtidParameter((IDbDataParameter)param);
        }

        #endregion

        #region Implementation of ICollection

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing. </param><param name="index">The zero-based index in <paramref name="array"/> at which copying begins. </param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null. </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero. </exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or- <paramref name="index"/> is equal to or greater than the length of <paramref name="array"/>.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>. </exception><exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>. </exception><filterpriority>2</filterpriority>
        public override void CopyTo(Array array, int index)
        {
            if (array != null)
            {
                foreach (DbParameter parameter in parameterCollection)
                {
                    if (parameter is OracleParameter)
                        array.SetValue(new GtidOracleParameter((IDbDataParameter)((ICloneable)parameter).Clone()), index);
                    else
                        array.SetValue(new GtidParameter((IDbDataParameter)((ICloneable)parameter).Clone()), index);
                    index += 1;
                }                
            }
        }

        /// <summary>
        /// Sets the <see cref="T:System.Data.Common.DbParameter"/> object with the specified name to a new value.
        /// </summary>
        /// <param name="parameterName">The name of the <see cref="T:System.Data.Common.DbParameter"/> object in the collection.</param><param name="value">The new <see cref="T:System.Data.Common.DbParameter"/> value.</param>
        protected override void SetParameter(string parameterName, DbParameter value)
        {
            parameterCollection[parameterName] = value;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// The number of elements contained in the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int Count
        {
            get { return parameterCollection.Count; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>
        /// An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override object SyncRoot
        {
            get { return parameterCollection.SyncRoot; }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <returns>
        /// true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool IsSynchronized
        {
            get { return parameterCollection.IsSynchronized; }
        }

        #endregion

        #region Implementation of IList

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <returns>
        /// The position into which the new element was inserted.
        /// </returns>
        /// <param name="value">The <see cref="T:System.Object"/> to add to the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception><filterpriority>2</filterpriority>
        public override int Add(object value)
        {
            //if (!Contains(((IDbDataParameter)value).ParameterName))
            //{
                GtidParameter parameter;
                if (value is GtidParameter)
                {
                    //if (((GtidParameter)value).InnerParameter != null)
                    //{
                    //    return parameterCollection.Add(((GtidParameter)value).InnerParameter);
                    //}
                    parameter = (GtidParameter)ownerCommand.CreateParameter();
                    parameter.ParameterName = ((IDbDataParameter)value).ParameterName;
                    parameter.Size = ((IDbDataParameter)value).Size;
                    parameter.Scale = ((IDbDataParameter)value).Scale;
                    parameter.Precision = ((IDbDataParameter)value).Precision;
                    parameter.Value = ((IDbDataParameter)value).Value;
                    parameter.Direction = ((IDbDataParameter)value).Direction;
                    parameter.DbType = ((GtidParameter)value).DbType;
                    parameter.GtidParamType = ((GtidParameter)value).GtidParamType;
                    return parameterCollection.Add(parameter.InnerParameter);
                }
                
                parameter = new GtidParameter((IDbDataParameter)value);
                return parameterCollection.Add(parameter.InnerParameter);
            //}

            //this[((IDbDataParameter)value).ParameterName].Value = ((IDbDataParameter)value).Value;
            //this[((IDbDataParameter)value).ParameterName].Direction = ((IDbDataParameter)value).Direction;
            //return IndexOf(((IDbDataParameter)value).ParameterName);
        }

        /// <summary>
        /// Adds an array of items with the specified values to the <see cref="T:System.Data.Common.DbParameterCollection"/>.
        /// </summary>
        /// <param name="values">An array of values of type <see cref="T:System.Data.Common.DbParameter"/> to add to the collection.</param><filterpriority>2</filterpriority>
        public override void AddRange(Array values)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Object"/> is found in the <see cref="T:System.Collections.IList"/>; otherwise, false.
        /// </returns>
        /// <param name="value">The <see cref="T:System.Object"/> to locate in the <see cref="T:System.Collections.IList"/>. </param><filterpriority>2</filterpriority>
        public override bool Contains(object value)
        {
            return parameterCollection.Contains(value);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only. </exception><filterpriority>2</filterpriority>
        public override void Clear()
        {
            parameterCollection.Clear();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <returns>
        /// The index of <paramref name="value"/> if found in the list; otherwise, -1.
        /// </returns>
        /// <param name="value">The <see cref="T:System.Object"/> to locate in the <see cref="T:System.Collections.IList"/>. </param><filterpriority>2</filterpriority>
        public override int IndexOf(object value)
        {
            return parameterCollection.IndexOf(value);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted. </param><param name="value">The <see cref="T:System.Object"/> to insert into the <see cref="T:System.Collections.IList"/>. </param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception><exception cref="T:System.NullReferenceException"><paramref name="value"/> is null reference in the <see cref="T:System.Collections.IList"/>.</exception><filterpriority>2</filterpriority>
        public override void Insert(int index, object value)
        {
            if (!Contains(((IDbDataParameter)value).ParameterName))
            {
                GtidParameter parameter;
                if (value is GtidParameter)
                    if (((GtidParameter)value).InnerParameter != null)
                    {
                        parameterCollection.Insert(index, ((GtidParameter)value).InnerParameter);
                        return;
                    }
                    else
                    {
                        parameter = (GtidParameter)ownerCommand.CreateParameter();
                        parameter.ParameterName = ((IDbDataParameter)value).ParameterName;
                        parameter.DbType = ((IDbDataParameter)value).DbType;
                        parameter.Value = ((IDbDataParameter)value).Value;
                        parameter.Direction = ((IDbDataParameter)value).Direction;
                        parameterCollection.Insert(index, parameter.InnerParameter);
                        return;
                    }
                parameter = new GtidParameter((IDbDataParameter)value);
                parameterCollection.Insert(index, parameter.InnerParameter);
                return;
            }
            //throw new ItemAlreadyExistsException(((IDbDataParameter)value).ParameterName);
            throw new ManagedException(String.Format("{0} already exists", ((IDbDataParameter)value).ParameterName));
        }

        /// <summary>
        /// Removes the specified <see cref="T:System.Data.Common.DbParameter"/> object from the collection.
        /// </summary>
        /// <param name="value">The <see cref="T:System.Data.Common.DbParameter"/> object to remove.</param><filterpriority>1</filterpriority>
        public override void Remove(object value)
        {
            parameterCollection.Remove(value);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Data.Common.DbParameter"/> object at the specified from the collection.
        /// </summary>
        /// <param name="index">The index where the <see cref="T:System.Data.Common.DbParameter"/> object is located.</param><filterpriority>2</filterpriority>
        public override void RemoveAt(int index)
        {
            parameterCollection.RemoveAt(index);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> is read-only.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IList"/> is read-only; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool IsReadOnly
        {
            get { return parameterCollection.IsReadOnly; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool IsFixedSize
        {
            get { return parameterCollection.IsFixedSize; }
        }

        #endregion

        #region Implementation of IDataParameterCollection

        /// <summary>
        /// Gets a value indicating whether a parameter in the collection has the specified name.
        /// </summary>
        /// <returns>
        /// true if the collection contains the parameter; otherwise, false.
        /// </returns>
        /// <param name="parameterName">The name of the parameter. </param><filterpriority>2</filterpriority>
        public override bool Contains(string parameterName)
        {
            return parameterCollection.Contains(parameterName);
        }

        /// <summary>
        /// Gets the location of the <see cref="T:System.Data.IDataParameter"/> within the collection.
        /// </summary>
        /// <returns>
        /// The zero-based location of the <see cref="T:System.Data.IDataParameter"/> within the collection.
        /// </returns>
        /// <param name="parameterName">The name of the parameter. </param><filterpriority>2</filterpriority>
        public override int IndexOf(string parameterName)
        {
            return parameterCollection.IndexOf(parameterName);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Data.IDataParameter"/> from the collection.
        /// </summary>
        /// <param name="parameterName">The name of the parameter. </param><filterpriority>2</filterpriority>
        public override void RemoveAt(string parameterName)
        {
            parameterCollection.RemoveAt(parameterName);
        }

        /// <summary>
        /// Sets the <see cref="T:System.Data.Common.DbParameter"/> object at the specified index to a new value. 
        /// </summary>
        /// <param name="index">The index where the <see cref="T:System.Data.Common.DbParameter"/> object is located.</param><param name="value">The new <see cref="T:System.Data.Common.DbParameter"/> value.</param>
        protected override void SetParameter(int index, DbParameter value)
        {
            parameterCollection[index] = value;
        }

        #endregion

        public GtidParameter AddWithValue(string parameterName, object value)
        {
            if(!Contains(parameterName))
            {
                IDbDataParameter parameter = ownerCommand.CreateParameter();
                parameter.ParameterName = parameterName;
                parameter.Value = value != null && value.GetType() == typeof (Boolean)
                                      ? Convert.ToBoolean(value) ? 1 : 0
                                      : value;
                Add(parameter);
            }
            else
                this[parameterName].Value = value != null && value.GetType() == typeof (Boolean)
                                                ? Convert.ToBoolean(value) ? 1 : 0
                                                : value;

            return (GtidParameter) this[parameterName];
        }
    }

    internal class GtidOracleParameter : GtidParameter, ICloneable
    {
        public GtidOracleParameter(IDbDataParameter parameter) : base(parameter) { }
        
        public override int GtidParamType
        {
            get
            {
                return Convert.ToInt32(((OracleParameter)InnerParameter).OracleDbType);
            }
            set
            {
                if(Enum.IsDefined(typeof(OracleDbType), value))
                    ((OracleParameter)InnerParameter).OracleDbType = (OracleDbType)value;
                else
                    ((OracleParameter)InnerParameter).DbType = (DbType)value;
            }
        }
        public new object Clone()
        {
            return new GtidOracleParameter(InnerParameter);
        }
    }

    public class GtidParameter : DbParameter, IDbDataParameter, ICloneable
    {
        internal readonly IDbDataParameter InnerParameter;

        public GtidParameter(string parameterName)
        {
            ParameterName = parameterName;
        }
        public GtidParameter(string parameterName, object value)
        {
            ParameterName = parameterName;
            Value = value;
            Direction = ParameterDirection.Input;
        }
        public GtidParameter(IDbDataParameter parameter)
        {
            InnerParameter = parameter;
        }
        
        #region Implementation of IDataParameter

        /// <summary>
        /// Resets the DbType property to its original settings.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void ResetDbType()
        {
            throw new NotImplementedException("ResetDbType is not implemented.");
        }

        public virtual int GtidParamType {  get; set; }

        private DbType dbType;
        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.DbType"/> of the parameter.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.DbType"/> values. The default is <see cref="F:System.Data.DbType.String"/>.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The property is not set to a valid <see cref="T:System.Data.DbType"/>.</exception><filterpriority>1</filterpriority>
        public override DbType DbType
        {
            get { return InnerParameter != null ? InnerParameter.DbType : dbType; }
            set
            {
                dbType = value;
                if(InnerParameter != null) InnerParameter.DbType = value;
            }
        }

        private ParameterDirection direction;
        /// <summary>
        /// Gets or sets a value that indicates whether the parameter is input-only, output-only, bidirectional, or a stored procedure return value parameter.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.ParameterDirection"/> values. The default is Input.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The property is not set to one of the valid <see cref="T:System.Data.ParameterDirection"/> values.</exception><filterpriority>1</filterpriority>
        public override ParameterDirection Direction
        {
            get { return InnerParameter != null ? InnerParameter.Direction : direction; }
            set
            {
                direction = value;
                if (InnerParameter!= null) InnerParameter.Direction = value;
            }
        }

        private bool isNullable;
        /// <summary>
        /// Gets or sets a value that indicates whether the parameter accepts null values.
        /// </summary>
        /// <returns>
        /// true if null values are accepted; otherwise false. The default is false.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool IsNullable
        {
            get { return InnerParameter != null ? InnerParameter.IsNullable : isNullable; }
            set
            {
                isNullable = value;
                try
                {
                    ((DbParameter) InnerParameter).IsNullable = value;
                }
                finally{}
            }
        }

        private string parameterName;
        /// <summary>
        /// Gets or sets the name of the <see cref="T:System.Data.Common.DbParameter"/>.
        /// </summary>
        /// <returns>
        /// The name of the <see cref="T:System.Data.Common.DbParameter"/>. The default is an empty string ("").
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string ParameterName
        {
            get
            {
                return InnerParameter != null ? InnerParameter.ParameterName : parameterName;
            }
            set 
            { 
                parameterName = value; 
                if(InnerParameter != null) InnerParameter.ParameterName = value; 
            }
        }

        /// <summary>
        /// Gets or sets the name of the source column mapped to the <see cref="T:System.Data.DataSet"/> and used for loading or returning the <see cref="P:System.Data.Common.DbParameter.Value"/>.
        /// </summary>
        /// <returns>
        /// The name of the source column mapped to the <see cref="T:System.Data.DataSet"/>. The default is an empty string.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string SourceColumn { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.DataRowVersion"/> to use when you load <see cref="P:System.Data.Common.DbParameter.Value"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.DataRowVersion"/> values. The default is Current.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The property is not set to one of the <see cref="T:System.Data.DataRowVersion"/> values.</exception><filterpriority>1</filterpriority>
        public override DataRowVersion SourceVersion { get; set; }

        private object pvalue;
        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object"/> that is the value of the parameter. The default value is null.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override object Value
        {
            get { return InnerParameter != null ? InnerParameter.Value : pvalue; }
            set
            {
                pvalue = value;
                if(InnerParameter != null) InnerParameter.Value = value;
            }
        }

        /// <summary>
        /// Sets or gets a value which indicates whether the source column is nullable. This allows <see cref="T:System.Data.Common.DbCommandBuilder"/> to correctly generate Update statements for nullable columns.
        /// </summary>
        /// <returns>
        /// true if the source column is nullable; false if it is not.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override bool SourceColumnNullMapping { get; set; }

        /// <summary>
        /// Gets or sets the maximum size, in bytes, of the data within the column.
        /// </summary>
        /// <returns>
        /// The maximum size, in bytes, of the data within the column. The default value is inferred from the parameter value.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override int Size
        {
            get { return InnerParameter.Size; }
            set { InnerParameter.Size = value; }
        }
        
        #endregion

        public byte Precision
        {
            get { return InnerParameter.Precision; } 
            set { InnerParameter.Precision = value; }
        }

        public byte Scale
        {
            get { return InnerParameter.Scale; }
            set { InnerParameter.Scale = value; }
        }


        #region Implementation of ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object Clone()
        {
            return new GtidParameter(InnerParameter);
        }

        #endregion
    }

    internal class GtidPrameterCache //in
    {
        private Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

        private static List<ManagedObject> managedObjects;

        private class ManagedObject
        {
            internal ManagedObject()
            {
                Id = Thread.CurrentThread.ManagedThreadId;
            }

            public object Content { get; set; }
            public int Id { get; private set; }
        }

        public static GtidPrameterCache Instance
        {
            get
            {
                if(managedObjects == null) managedObjects = new List<ManagedObject>();
                
                if(!managedObjects.Exists(
                    
                    delegate(ManagedObject match)
                        {
                            return match.Id == Thread.CurrentThread.ManagedThreadId;
                        }))
                {
                    managedObjects.Add(new ManagedObject { Content = new GtidPrameterCache() });
                }

                return managedObjects.Find(
                    
                    delegate(ManagedObject match)
                        {
                            return match.Id == Thread.CurrentThread.ManagedThreadId;

                        }).Content as GtidPrameterCache;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CacheParameterSet(string commandText, params GtidParameter[] commandParameters)
        {
            string hashKey = ConnectionUtil.Instance.GetConnectionString() + ":" + commandText;

            paramCache[hashKey] = commandParameters;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public GtidParameter[] GetCachedParameterSet(string commandText)
        {
            string hashKey = ConnectionUtil.Instance.GetConnectionString() + ":" + commandText;

            var cachedParameters = (GtidParameter[])paramCache[hashKey];

            if (cachedParameters == null)
            {
                return null;
            }

            return CloneParameters(cachedParameters);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private GtidParameter[] CloneParameters(GtidParameter[] originalParameters)
        {
            var clonedParameters = new GtidParameter[originalParameters.Length];

            for (int i = 0; i < originalParameters.Length; i++)
            {
                clonedParameters[i] = (GtidParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }
    }

    #endregion

    #region Connection Classes

    internal class GtidOracleConnection : GtidConnection //internal
    {
        internal GtidOracleConnection(OracleConnection connection) : base(connection) { }
        
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new GtidOracleTransaction((OracleTransaction)InnerConnection.BeginTransaction(isolationLevel), this);
        }

        internal override void DeriveParametersInstance(GtidCommand command)
        {
            OracleCommandBuilder.DeriveParameters((OracleCommand)command.InnerCommand);
        }

        public override List<string> HostEntries
        {
            get
            {
                int key = ConnectionString.GetHashCode();

                if(HostEntriesCache[key]== null)
                {
                    string sValidConnectionString = InnerConnection.ConnectionString
                        .Replace(" ", String.Empty)
                        .Replace("\t", String.Empty)
                        .Replace("\r", String.Empty)
                        .Replace("\n", String.Empty);

                    HostEntriesCache[key] = TnsNamesReader.LoadHost(sValidConnectionString);
                }

                return (List<string>)HostEntriesCache[key];
            }
        }

        public override string DataSource
        {
            get
            {
                return ((OracleConnection)InnerConnection).DataSource;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return ((OracleConnection)InnerConnection).ServerVersion;
            }
        }

        public override string HostName
        {
            get
            {
                return ((OracleConnection)InnerConnection).HostName;
            }
        }
    }

    internal class GtidSqlConnection : GtidConnection //
    {
        internal GtidSqlConnection(SqlConnection connection) : base(connection) { }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new GtidSqlTransaction((SqlTransaction)InnerConnection.BeginTransaction(isolationLevel), this);
        }

        internal override void DeriveParametersInstance(GtidCommand command)
        {
            SqlCommandBuilder.DeriveParameters((SqlCommand)command.InnerCommand);
        }

        public override string DataSource
        {
            get
            {
                return ((SqlConnection)InnerConnection).DataSource;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return ((SqlConnection)InnerConnection).ServerVersion;
            }
        }

        public override string HostName
        {
            get
            {
                return ((OracleConnection)InnerConnection).DataSource;
            }
        }
    }

    internal class GtidOleDbConnection : GtidConnection
    {
        internal GtidOleDbConnection(OleDbConnection connection) : base(connection) { }

        internal override void DeriveParametersInstance(GtidCommand command)
        {
            OleDbCommandBuilder.DeriveParameters((OleDbCommand)command.InnerCommand);
        }
    }

    internal class GtidOracleProdConnection : GtidOracleConnection
    {
        internal GtidOracleProdConnection(OracleConnection connection) : base(connection) { }
    }

    internal class GtidOracleStByConnection : GtidOracleConnection
    {
        internal GtidOracleStByConnection(OracleConnection connection) : base(connection) { }
    }

    public class GtidConnection : DbConnection
    {
        protected static Hashtable HostEntriesCache = Hashtable.Synchronized(new Hashtable());

        internal readonly IDbConnection InnerConnection;
        private bool isDisposed;
        public GtidConnection(IDbConnection innerConnection)
        {
            isDisposed = false;
            InnerConnection = innerConnection;
        }

        //~GtidConnection()
        //{
        //    HostEntriesCache = null;
        //    isDisposed = true;
        //    InnerConnection.Dispose();
        //    Dispose(false);
        //}

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                HostEntriesCache = null;
                isDisposed = true;
                InnerConnection.Dispose();                
            }
            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal virtual void DeriveParametersInstance(GtidCommand command){}

        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void DeriveParameters(GtidCommand command, params object[] paramValues)
        {
            OpenIfClosed();
            
            if(command.CommandType == CommandType.StoredProcedure)
            {
                DeriveParametersInstance(command);

                if (paramValues != null && paramValues.Length > 0)
                {
                    for (int i = 0; i < paramValues.Length; i++)
                    {
                        if (command.Parameters.Count > i)

                            command.Parameters[i].Value = paramValues[i];
                    }
                }
            }
            else
            {
                if (paramValues != null && paramValues.Length > 0)
                {
                    string cmdText = PreProcess(command.CommandText);
                    
                    string paramName = String.Empty;

                    var splitCharArr = new[] { ' ', ',', '\n', '\r', '\t', '>', '<', '=', '+', '|', '-', '*', '/', ')' };

                    int index = cmdText.IndexOf(":");

                    int indexCompiled = 0;

                    for (int i = 0; i < paramValues.Length; i++)
                    {
                        bool isCompiled = false;

                        do
                        {
                            if (index == -1)

                                throw new ManagedException("Can not bind values to parameters.", false);

                            paramName = cmdText.Substring(index);

                            paramName = paramName.Substring(0,
                                                            paramName.IndexOfAny(splitCharArr) == -1
                                                                ? paramName.Length
                                                                : paramName.IndexOfAny(splitCharArr));

                            if (command.Parameters.Count > 0 && command.Parameters.Contains(paramName))
                            {
                                command.Parameters.Add(command.Parameters[paramName].Clone());

                                indexCompiled = index;
                            }
                            else
                            {
                                command.Parameters.AddWithValue(paramName, paramValues[i]);

                                isCompiled = true;

                                indexCompiled = index;
                            }

                            index = cmdText.IndexOf(":", index + 1);

                        } while (!isCompiled);

                    }

                    while (indexCompiled < index)
                    {
                        if (index == -1)

                            throw new ManagedException("Can not bind values to parameters.", false);

                        paramName = cmdText.Substring(index);

                        paramName = paramName.Substring(0,
                                                        paramName.IndexOfAny(splitCharArr) == -1
                                                            ? paramName.Length
                                                            : paramName.IndexOfAny(splitCharArr));

                        if (command.Parameters.Contains(paramName))
                        {
                            command.Parameters.Add(command.Parameters[paramName].Clone());

                            indexCompiled = index;
                        } 
                        else
                        {
                            throw new ManagedException("Can not bind values to parameters.", false);
                        }

                        index = cmdText.IndexOf(":", index + 1);
                    }

                    command.Prepare();
                }
            }

            if (!ConnectionUtil.Instance.IsInTransaction) Close();
        }

        private static string PreProcess(string commandText)
        {
            string result = commandText;

            var regex = new Regex("'(.*?)'");

            var matches = regex.Matches(commandText);

            foreach (Match match in matches)
            {
                if(match.Success)
                    
                    result = result.Replace(match.Value, "const");
            }

            return result;
        }

        #region Implementation of IDisposable

        public bool IsDisposed
        {
            get { return isDisposed; }
        }

        #endregion

        #region Implementation of IDbConnection

        /// <summary>
        /// Begins a database transaction.
        /// </summary>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public new GtidTransaction BeginTransaction()
        {
            return (GtidTransaction) InnerConnection.BeginTransaction();
        }

        /// <summary>
        /// Begins a database transaction with the specified <see cref="T:System.Data.IsolationLevel"/> value.
        /// </summary>
        /// <returns>
        /// An object representing the new transaction.
        /// </returns>
        /// <param name="il">One of the <see cref="T:System.Data.IsolationLevel"/> values. </param><filterpriority>2</filterpriority>
        public new GtidTransaction BeginTransaction(IsolationLevel il)
        {
            return (GtidTransaction) BeginDbTransaction(il);
        }

        /// <summary>
        /// Closes the connection to the database.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Close()
        {
            InnerConnection.Close();
        }

        /// <summary>
        /// Changes the current database for an open Connection object.
        /// </summary>
        /// <param name="databaseName">The name of the database to use in place of the current database. </param><filterpriority>2</filterpriority>
        public override void ChangeDatabase(string databaseName)
        {
            InnerConnection.ChangeDatabase(databaseName);
        }
        /// <summary>
        /// Creates and returns a Command object associated with the connection.
        /// </summary>
        /// <returns>
        /// A Command object associated with the connection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public new GtidCommand CreateCommand()
        {
            try
            {
                OpenIfClosed();

                GtidCommand cmdResult = (GtidCommand)CreateDbCommand();

                if (!ConnectionUtil.Instance.IsInTransaction) Close();

                return cmdResult;

            }
            catch (OracleException oracleException)
            {
                //EventLogProvider.Instance.WriteOfflineLog(oracleException.ToString(), "Create Command");

                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    switch (oracleException.Number)
                    {
                        case 12571:
                        case 12560:
                        case 12543:
                        case 12514:
                        case 12170:
                        case 3135:
                        case 3113:
                        case 3114:
                        case 1033:
						case 1034: 
                        case -1000:
                        case -3000:
						case 28547:
                        case 12528:
                        case 12518:
                        case 12541:
                        case 12505:
                        case 00600:
                            {
                                //Debug.Print("{0} Thread Id: {1} retry connect on BeginSerializableTransaction...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                                
                                frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);
                                
                                while (!ConnectionUtil.Instance.IsConnected)
                                {
                                    Thread.SpinWait(1000);
                                }
                                
                                return CreateCommand();
                                
                            }
                        case 24309:
                            return CreateCommand();
                    }
                }
                throw new ManagedException(oracleException.Message, false);
            }
        }

        internal bool IsConnectedToServer
        {
            get
            {
                try
                {
                    foreach (string hostEntry in HostEntries)
                    {
                        var p = new Ping();

                        PingReply reply = p.Send(hostEntry, 3000);

                        if (reply == null || reply.Status != IPStatus.Success) return false;

                    }

                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString property of the provider-specific Connection object.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Open()
        {
            if(isDebug()) throw new ManagedException("Không thể kết nối tới máy chủ!", false);

            try
            {
                InnerConnection.Open();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }

        }

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool CheckRemoteDebuggerPresent(IntPtr hProcess, ref bool isDebuggerPresent);
        
        private bool isDebug()
        {
            var isDebuggerPresent = false;
            
            CheckRemoteDebuggerPresent(Process.GetCurrentProcess().Handle, ref isDebuggerPresent);
            
            return isDebuggerPresent;
        }

        internal void OpenIfClosed()
        {
            while (frmProgress.Instance.IsRetryingConnect)
            {
                Thread.CurrentThread.Join(200);
            }

            if (State == ConnectionState.Closed)
            {
                if (!IsConnectedToServer)
                {
                    if (!ConnectionUtil.Instance.IsInTransaction)
                    {
                        //Debug.Print("{0} Thread Id: {1} retry connect on OpenIfClosed...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                        
                        frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);
                    }
                        
                    else
                        
                        throw new ManagedException("Kết nối máy chủ chưa thông!", false);
                }

                Open();
            }
        }

        /// <summary>
        /// Gets or sets the string used to open a database.
        /// </summary>
        /// <returns>
        /// A string containing connection settings.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ConnectionString
        {
            get { return GtidCryption.Me.Encrypt(InnerConnection.ConnectionString, true); }
            set { InnerConnection.ConnectionString = GtidCryption.Me.Decrypt(value, true); }
        }

        /// <summary>
        /// Gets the time to wait while trying to establish a connection before terminating the attempt and generating an error.
        /// </summary>
        /// <returns>
        /// The time (in seconds) to wait for a connection to open. The default value is 15 seconds.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public new int ConnectionTimeout
        {
            get { return InnerConnection.ConnectionTimeout; }
        }

        /// <summary>
        /// Gets the name of the current database or the database to be used after a connection is opened.
        /// </summary>
        /// <returns>
        /// The name of the current database or the name of the database to be used once a connection is open. The default value is an empty string.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string Database
        {
            get { return InnerConnection.Database; }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.ConnectionState"/> values.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override ConnectionState State
        {
            get { return InnerConnection.State; }
        }

        #endregion

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return new GtidTransaction(InnerConnection.BeginTransaction(isolationLevel), this);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new GtidCommand(InnerConnection.CreateCommand(), this);
        }

        public virtual List<string> HostEntries
        {
            get { return null; }
        }

        public virtual string HostName
        {
            get { throw new ManagedException("Can not get HostName value.", false);}
        }

        public override string DataSource
        {
            get { return String.Empty; }
        }

        public override string ServerVersion
        {
            get { return String.Empty; }
        }

    }

    #endregion

    #region Command Classes
    public class GtidCommandBuilder
    {
        private class ManagedBuilder
        {
            internal ManagedBuilder()
            {
                Id = Thread.CurrentThread.ManagedThreadId;
            }

            public GtidCommandBuilder Builder { get; set; }

            public int Id { get; private set; }
        }

        private static List<ManagedBuilder> managedBuilders;

        public static GtidCommandBuilder Instance
        {
            get
            {
                if (managedBuilders == null) managedBuilders = new List<ManagedBuilder>();

                if(!managedBuilders.Exists(
                    
                    delegate(ManagedBuilder match) { return match.Id == Thread.CurrentThread.ManagedThreadId; }))
                {
                    managedBuilders.Add(new ManagedBuilder { Builder = new GtidCommandBuilder() });
                }

                return managedBuilders.Find(
                    
                    delegate(ManagedBuilder match) { return match.Id == Thread.CurrentThread.ManagedThreadId; }).Builder;                
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DeriveParameters(GtidCommand command, params object[] paramValues)
        {
            try
            {
                GtidParameter[] gtidParameters = GtidPrameterCache.Instance.GetCachedParameterSet(command.CommandText);

                if (gtidParameters == null)
                {
                    ConnectionUtil.CurrentManagedObject.IsRunningQuery = true;

                    ConnectionUtil.CurrentManagedObject.Connection.DeriveParameters(command, paramValues);

                    gtidParameters = new GtidParameter[command.Parameters.Count];

                    command.Parameters.CopyTo(gtidParameters, 0);

                    GtidPrameterCache.Instance.CacheParameterSet(command.CommandText, gtidParameters);
                }
                else
                {
                    command.Parameters.AddRange(gtidParameters);

                    if(command.CommandType == CommandType.StoredProcedure)
                    {
                        if (paramValues != null && paramValues.Length > 0)
                        {
                            for (int i = 0; i < paramValues.Length; i++)
                            {
                                if (command.Parameters.Count > i)

                                    command.Parameters[i].Value = paramValues[i];
                            }
                        }
                    } 
                    else
                    {
                        if (paramValues != null && paramValues.Length > 0)
                        {
                            var isCompiled = new int[command.Parameters.Count];
                            
                            int indexValueCompiled = 0;
                            
                            for (int i = 0; i < command.Parameters.Count; i++)
                            {
                                if (isCompiled[i] == 0)
                                {
                                    string paramNameCompiled = command.Parameters[i].ParameterName;

                                    for (int j = i; j < command.Parameters.Count; j++)
                                    {
                                        if (command.Parameters[j].ParameterName == paramNameCompiled)
                                        {
                                            command.Parameters[j].Value = paramValues[indexValueCompiled];

                                            isCompiled[j] = 1;
                                        }
                                    }
                                    if (indexValueCompiled < paramValues.Length) indexValueCompiled += 1;
                                }
                            }
                            command.Prepare();

                            //isCompiled = null;

                            //GC.Collect();

                            //if (!String.IsNullOrEmpty(Convert.ToString(isCompiled)))
                            //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                            //                       Convert.ToString(isCompiled));

                        }                        
                    }
                }

                //gtidParameters = null;

                //GC.Collect();

                //if (!String.IsNullOrEmpty(Convert.ToString(gtidParameters)))
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(gtidParameters));

            }
            catch (OracleException oracleException)
            {
                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    //EventLogProvider.Instance.WriteOfflineLog(oracleException.ToString(), "Derive Parameters");
                    switch (oracleException.Number)
                    {
                        case 12571:
                        case 12560:
                        case 12543:
                        case 12514:
                        case 12170:
                        case 3135:
                        case 3113:
                        case 3114:
                        case 1033: 
						case 1034: 
                        case -1000:
                        case -3000:
						case 28547:
                        case 12528:
                        case 12518:
                        case 12541:
                        case 12505:
                        case 00600:
                            {
                                //Debug.Print("{0} Thread Id: {1} retry connect on DerivedParameters...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                                frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);
                                
                                //while (!ConnectionUtil.Instance.IsConnected)
                                //{
                                //    Thread.SpinWait(1000);
                                //}
                                
                                DeriveParameters(command);
                                
                                return;

                            }    
                    }
                }
                throw new ManagedException(oracleException.Message, false, command, paramValues);
            }
        }
    }

    public class GtidCommand : DbCommand
    {
        internal IDbCommand InnerCommand; 
        internal IDbConnection InnerConnection;
        internal IDbTransaction InnerTransaction;
        internal bool OutputTrace;

        private bool isAutoRun;
        
        public GtidCommand()
        {
            InnerConnection = ConnectionUtil.Instance.GetConnection();
            InnerCommand = InnerConnection.CreateCommand();
        }
        
        public GtidCommand(string commandText)
        {
            if(Environment.UserInteractive)
                isAutoRun = ConnectionUtil.Instance.
                    AutoQueryString.Exists(delegate(string match)
                                               {
                                                   return match.ToLower() ==
                                                          commandText.ToLower();
                                               });

            InnerConnection = ConnectionUtil.Instance.GetConnection();
            InnerCommand = InnerConnection.CreateCommand();
            InnerCommand.CommandText = commandText;
        }

        public GtidCommand(string commandText, GtidConnection connection)
        {
            if(Environment.UserInteractive)
                isAutoRun = ConnectionUtil.Instance.
                    AutoQueryString.Exists(delegate(string match)
                                               {
                                                   return match.ToLower() ==
                                                          commandText.ToLower();
                                               });

            InnerConnection = connection;
            InnerCommand = InnerConnection.CreateCommand();
            InnerCommand.CommandText = commandText;
        }

        public GtidCommand(string commandText, GtidConnection connection, GtidTransaction sqlTran)
        {
            if (Environment.UserInteractive)
                isAutoRun = ConnectionUtil.Instance.
                    AutoQueryString.Exists(delegate(string match)
                                               {
                                                   return match.ToLower() ==
                                                          commandText.ToLower();
                                               });

            InnerConnection = connection;
            InnerCommand = InnerConnection.CreateCommand();
            InnerCommand.CommandText = commandText;
            InnerCommand.Transaction = sqlTran;
        }

        internal GtidCommand(IDbConnection connection)
        {
            InnerConnection = connection;
            InnerCommand = InnerConnection.CreateCommand();
        }

        internal GtidCommand(IDbCommand innerCommand)
        {
            InnerCommand = innerCommand;

            if(!String.IsNullOrEmpty(innerCommand.CommandText))
                if (Environment.UserInteractive)
                    isAutoRun = ConnectionUtil.Instance.
                        AutoQueryString.Exists(delegate(string match)
                        {
                            return match.ToLower() ==
                                   innerCommand.CommandText.ToLower();
                        });
        }

        internal GtidCommand(IDbCommand innerCommand, IDbConnection innerConnection)
        {
            InnerConnection = innerConnection;
            
            InnerCommand = innerCommand;

            if (!String.IsNullOrEmpty(innerCommand.CommandText))
                if (Environment.UserInteractive)
                    isAutoRun = ConnectionUtil.Instance.
                        AutoQueryString.Exists(delegate(string match)
                        {
                            return match.ToLower() ==
                                   innerCommand.CommandText.ToLower();
                        });
        }

        internal GtidCommand(string commandText, IDbConnection connection)
        {
            InnerConnection = connection;
            InnerCommand = InnerConnection.CreateCommand();
            InnerCommand.CommandText = commandText;

            if (Environment.UserInteractive)
                isAutoRun = ConnectionUtil.Instance.
                    AutoQueryString.Exists(delegate(string match)
                    {
                        return match.ToLower() ==
                               commandText.ToLower();
                    });
        }

        //~GtidCommand()
        //{
        //    Dispose(false);    
        //}

        protected override void Dispose(bool disposing)
        {
            if (InnerCommand != null) InnerCommand.Dispose();
            InnerConnection = null;
            InnerCommand = null;
            base.Dispose(disposing);
        }

        #region Implementation of IDbCommand

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored.
        /// </summary>
        /// <returns>
        /// The first column of the first row in the result set.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override object ExecuteScalar()
        {
            try
            {
                ConnectionUtil.CurrentManagedObject.IsRunningQuery = true;

                Connection.OpenIfClosed();

                if (ConnectionUtil.Instance.IsInTransaction)
                    InnerCommand.Transaction = ConnectionUtil.Instance.CurrentTransaction.InnerTransaction;

                if(OutputTrace)
                {
                    //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteScalar", Thread.CurrentThread.ManagedThreadId, this.CommandType, this.CommandText, DateTime.Now);   
                }

                object result = InnerCommand.ExecuteScalar();

                if (!ConnectionUtil.Instance.IsInTransaction) Connection.Close();

                if (!isAutoRun)
                    ConnectionUtil.CurrentManagedObject.LastActionTime = DateTime.Now;

                return result;
            }
            catch (OracleException oracleException)
            {
                //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteScalar go to error", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);

                //Debug.Print(oracleException.ToString());

                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    switch (oracleException.Number)
                    {
                        case 12571:
                        case 12560:
                        case 12543:
                        case 12514:
                        case 12170:
                        case 3135:
                        case 3114:
                        case -1000:
                        case -3000:
                        case 28547:
                        case 12528:
                        case 12518:
                        case 12541:
                        case 12505:
                        case 00600:
                            {
                                //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} Retry ExecuteScalar", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);

                                //Debug.Print("{0} Thread Id: {1} retry connect on ExecuteScalar...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                                
                                frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);

                                return ExecuteScalar();                                
                            }
                    }
                }
                else
                {
                    ConnectionUtil.Instance.CurrentTransaction.IsNeedRollback = true;
                }

                throw new ManagedException(oracleException.Message, false);
            } 
            catch(Exception ex)
            {
                //EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "Execute Scalar");

                throw new ManagedException(ex.Message, false);
            }
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Data.OleDb.OleDbCommand.Connection"/> is not set.-or- The <see cref="P:System.Data.OleDb.OleDbCommand.Connection"/> is not <see cref="M:System.Data.OleDb.OleDbConnection.Open"/>. </exception><filterpriority>2</filterpriority>
        public override void Prepare()
        {
            InnerCommand.Prepare();
        }

        /// <summary>
        /// Attempts to cancels the execution of an <see cref="T:System.Data.IDbCommand"/>.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public override void Cancel()
        {
            InnerCommand.Cancel();
        }

        /// <summary>
        /// Creates a new instance of an <see cref="T:System.Data.IDbDataParameter"/> object.
        /// </summary>
        /// <returns>
        /// An IDbDataParameter object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public new GtidParameter CreateParameter()
        {
            object param = InnerCommand.CreateParameter();
            
            if (param is GtidParameter) return (GtidParameter)param;
            
            if(param is OracleParameter)
                return new GtidOracleParameter((IDbDataParameter)param);

            return new GtidParameter((IDbDataParameter)param);
        }

        /// <summary>
        /// Creates a new instance of a <see cref="T:System.Data.Common.DbParameter"/> object.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbParameter"/> object.
        /// </returns>
        protected override DbParameter CreateDbParameter()
        {
            object param = InnerCommand.CreateParameter();
            //if (param is DbParameter) return (DbParameter)param;
            if (param is GtidParameter) return (GtidParameter)param;

            if (param is OracleParameter)
                return new GtidOracleParameter((IDbDataParameter)param);
            
            return new GtidParameter((IDbDataParameter)param);
        }

        /// <summary>
        /// Executes the command text against the connection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbDataReader"/>.
        /// </returns>
        /// <param name="behavior">An instance of <see cref="T:System.Data.CommandBehavior"/>.</param>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            try
            {
                return (DbDataReader)InnerCommand.ExecuteReader(behavior);
            }
            catch (OracleException oracleException)
            {
                //Debug.Print( "{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteDbDataReader with behavior go to error", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);

                //Debug.Print(oracleException.ToString());

                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    switch (oracleException.Number)
                    {
                        case 12571:
                        case 12560:
                        case 12543:
                        case 12514:
                        case 12170:
                        case 3135:
                        case 3113:
                        case 3114:
                        case 1033:
                        case 1034:
                        case -1000:
                        case -3000:
                        case 28547:
                        case 12528:
                        case 12518:
                        case 12541:
                        case 12505:
                        case 00600:
                            {
                                //Debug.Print( "{4}: ThreadId:{0} CommandType:{1} CommandText:{2} Retry ExecuteReader CommandBehavior:{3}", Thread.CurrentThread.ManagedThreadId, this.CommandType, this.CommandText, behavior, DateTime.Now);

                                //Debug.Print("{0} Thread Id: {1} retry connect on ExecuteReader(CommandBehavior)...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                                frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);

                                return ExecuteDbDataReader(behavior);
                            }
                    }
                }
                else
                {
                    ConnectionUtil.Instance.CurrentTransaction.IsNeedRollback = true;
                }

                throw new ManagedException(oracleException.Message, false);
            }
            catch (ManagedException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                //Debug.Print( "{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteDbDataReader with behavior go to error", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);

                //Debug.Print(ex.ToString());

                var managedException = new ManagedException(ex.Message, false);

                //EventLogProvider.Instance.WriteOfflineLog(managedException.ToString(), "Exceute Db Data Reader");

                throw managedException;
            }
        }

        /// <summary>
        /// Executes an SQL statement against the Connection object of a .NET Framework data provider, and returns the number of rows affected.
        /// </summary>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The connection does not exist.-or- The connection is not open. </exception><filterpriority>2</filterpriority>
        public override int ExecuteNonQuery()
        {
            try
            {
                ConnectionUtil.CurrentManagedObject.IsRunningQuery = true;

                Connection.OpenIfClosed();
                
                if (ConnectionUtil.Instance.IsInTransaction)
                {
                    if (ConnectionUtil.Instance.CurrentTransaction.IsNeedRollback) return 0;

                    InnerCommand.Transaction = ConnectionUtil.Instance.CurrentTransaction.InnerTransaction;
                }

                if (OutputTrace)
                {
                    //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteNonQuery", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);
                }

                int result = InnerCommand.ExecuteNonQuery();

                if (!ConnectionUtil.Instance.IsInTransaction) Connection.Close();

                if (!isAutoRun) 
                    ConnectionUtil.CurrentManagedObject.LastActionTime = DateTime.Now;

                return result;
            }
            catch (OracleException oracleException)
            {
                //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteNonQuery go to error", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);

                //Debug.Print(oracleException.ToString());

                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    switch (oracleException.Number)
                    {
                        case 12571:
                        case 12560:
                        case 12543:
                        case 12514:
                        case 12170:
                        case 3135:
                        case 3113:
                        case 3114:
                        case 1033:
                        case 1034:
                        case -1000:
                        case -3000:
                        case 28547:
                        case 12528:
                        case 12518:
                        case 12541:
                        case 12505:
                        case 00600:
                            {
                                //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} Retry ExecuteNonQuery", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);

                                //Debug.Print("{0} Thread Id: {1} retry connect on ExecuteNonQuery...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                                frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);
                                
                                return ExecuteNonQuery();                                
                            }
                    }
                }
                else
                {
                    ConnectionUtil.Instance.CurrentTransaction.IsNeedRollback = true;
                }

                if (oracleException.Number == 20101)
                {
                    throw new ManagedException(oracleException.Message, false);
                }

                throw new ManagedException(oracleException.Message, false);
            }
            catch (ManagedException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                var managedException = new ManagedException(ex.Message, false);

                //EventLogProvider.Instance.WriteOfflineLog(managedException.ToString(), "ExceuteNoneQuery");

                throw managedException;
            }
        }

        //private IDataReader dataReader;
        //public void ExecuteReaderSync()
        //{
        //    dataReader = InnerCommand.ExecuteReader();
        //    frmProgress.Instance.Value = frmProgress.Instance.MaxValue;
        //    frmProgress.Instance.IsCompleted = true;
        //}

        /// <summary>
        /// Executes the <see cref="P:System.Data.IDbCommand.CommandText"/> against the <see cref="P:System.Data.IDbCommand.Connection"/> and builds an <see cref="T:System.Data.IDataReader"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.IDataReader"/> object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public new IDataReader ExecuteReader()
        {
            try
            {
                ConnectionUtil.CurrentManagedObject.IsRunningQuery = true;

                Connection.OpenIfClosed();

                if (ConnectionUtil.Instance.IsInTransaction)
                    InnerCommand.Transaction = ConnectionUtil.Instance.CurrentTransaction.InnerTransaction;

                if(OutputTrace)
                {
                    //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteReader", Thread.CurrentThread.ManagedThreadId, this.CommandType, this.CommandText, DateTime.Now);
                }

                IDataReader dataReader = new GtidReader(InnerCommand.ExecuteReader());

                if (!isAutoRun)
                    ConnectionUtil.CurrentManagedObject.LastActionTime = DateTime.Now;

                return dataReader;

            }
            catch (OracleException oracleException)
            {
                //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteReader go to error", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);

                //Debug.Print(oracleException.ToString());
                
                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    switch (oracleException.Number)
                    {
                        case 12571:
                        case 12560:
                        case 12543:
                        case 12514:
                        case 12170:
                        case 3135:
                        case 3113:
                        case 3114:
                        case 1033:
                        case 1034:
                        case -1000:
                        case -3000:
                        case 28547:
                        case 12528:
                        case 12518:
                        case 12541:
                        case 12505:
                        case 00600:
                            {
                                //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} Retry ExecuteReader", Thread.CurrentThread.ManagedThreadId, this.CommandType, this.CommandText, DateTime.Now);

                                //Debug.Print("{0} Thread Id: {1} retry connect on ExecuteReader...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                                frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);
                                
                                return ExecuteReader();
                            }
                    }
                }
                else
                {
                    ConnectionUtil.Instance.CurrentTransaction.IsNeedRollback = true;
                }

                throw new ManagedException(oracleException.Message, false);
            }
            catch(InvalidOperationException ex)
            {
                if (ex.Message == "Connection must be open for this operation")
                {
                    if (!ConnectionUtil.Instance.IsInTransaction)
                    {
                        //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} Retry ExecuteReader InvalidOperationException", Thread.CurrentThread.ManagedThreadId, this.CommandType, this.CommandText, DateTime.Now);

                        //Debug.Print("{0} Thread Id: {1} retry connect on ExecuteReader...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                        frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);
                        
                        return ExecuteReader();                        
                    }
                }

                //Debug.Print("{0}: ThreadId {1} {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, ex.ToString());

                var managedException = new ManagedException(ex.Message, false);

                //EventLogProvider.Instance.WriteOfflineLog(managedException.ToString(), "Retry ExecuteReader InvalidOperationException");

                throw managedException;
            }
            catch (ManagedException ex)
            {
                //Debug.Print("{0}: ThreadId {1} {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, ex.ToString());

                throw;
            }
            catch (Exception ex)
            {
                //Debug.Print("{0}: ThreadId {1} {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, ex.ToString());

                var managedException = new ManagedException(ex.Message, false);

                //EventLogProvider.Instance.WriteOfflineLog(managedException.ToString(), "Execute Reader");

                throw managedException;
            }
        }

        /// <summary>
        /// Executes the <see cref="P:System.Data.IDbCommand.CommandText"/> against the <see cref="P:System.Data.IDbCommand.Connection"/>, and builds an <see cref="T:System.Data.IDataReader"/> using one of the <see cref="T:System.Data.CommandBehavior"/> values.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.IDataReader"/> object.
        /// </returns>
        /// <param name="behavior">One of the <see cref="T:System.Data.CommandBehavior"/> values. </param><filterpriority>2</filterpriority>
        public new IDataReader ExecuteReader(CommandBehavior behavior)
        {
            try
            {
                ConnectionUtil.CurrentManagedObject.IsRunningQuery = true;

                Connection.OpenIfClosed();

                if (ConnectionUtil.Instance.IsInTransaction)
                    InnerCommand.Transaction = ConnectionUtil.Instance.CurrentTransaction.InnerTransaction;

                if(OutputTrace)
                {
                    //Debug.Print("{4}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteReader CommandBehavior:{3}", Thread.CurrentThread.ManagedThreadId, this.CommandType, this.CommandText, behavior, DateTime.Now);
                }
                
                IDataReader dataReader = new GtidReader(InnerCommand.ExecuteReader(behavior));

                if (!isAutoRun)
                    ConnectionUtil.CurrentManagedObject.LastActionTime = DateTime.Now;

                return dataReader;

            }
            catch (OracleException oracleException)
            {
                //Debug.Print("{3}: ThreadId:{0} CommandType:{1} CommandText:{2} ExecuteReader with behavior go to error", Thread.CurrentThread.ManagedThreadId, CommandType, CommandText, DateTime.Now);

                //Debug.Print(oracleException.ToString());

                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    switch (oracleException.Number)
                    {
                        case 12571:
                        case 12560:
                        case 12543:
                        case 12514:
                        case 12170:
                        case 3135:
                        case 3113:
                        case 3114:
                        case 1033: 
						case 1034: 
                        case -1000:
                        case -3000:
						case 28547:
                        case 12528:
                        case 12518:
                        case 12541:
                        case 12505:
                        case 00600:
                            {
                                //Debug.Print( "{4}: ThreadId:{0} CommandType:{1} CommandText:{2} Retry ExecuteReader CommandBehavior:{3}", Thread.CurrentThread.ManagedThreadId, this.CommandType, this.CommandText, behavior, DateTime.Now);

                                //Debug.Print("{0} Thread Id: {1} retry connect on ExecuteReader(CommandBehavior)...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                                frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);

                                return ExecuteReader(behavior);                                
                            }
                    }
                }
                else
                {
                    ConnectionUtil.Instance.CurrentTransaction.IsNeedRollback = true;
                }

                throw new ManagedException(oracleException.Message, false);
            }
            catch (ManagedException ex)
            {
                //Debug.Print("{0}: ThreadId {1} {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, ex.ToString());
                
                throw;
            }
            catch (Exception ex)
            {
                //Debug.Print("{0}: ThreadId {1} {2}", DateTime.Now, Thread.CurrentThread.ManagedThreadId, ex.ToString());

                var managedException = new ManagedException(ex.Message, false);

                //EventLogProvider.Instance.WriteOfflineLog(managedException.ToString(), "Execute Reader with behavior");

                throw managedException;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.IDbConnection"/> used by this instance of the <see cref="T:System.Data.IDbCommand"/>.
        /// </summary>
        /// <returns>
        /// The connection to the data source.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public new GtidConnection Connection
        {
            get {
                return (GtidConnection)InnerConnection;
            }

            set
            {                
                if (InnerCommand != null)
                {
                    if (InnerCommand.Connection == null)
                        InnerCommand.Connection = value;
                }
                else
                {
                    InnerCommand = value.CreateCommand();
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:System.Data.Common.DbConnection"/> used by this <see cref="T:System.Data.Common.DbCommand"/>.
        /// </summary>
        /// <returns>
        /// The connection to the data source.
        /// </returns>
        protected override DbConnection DbConnection
        {
            get
            {
                if (InnerCommand.Connection is DbConnection)
                    return (DbConnection)InnerCommand.Connection;
                return new GtidConnection(InnerCommand.Connection);
            }
            set { InnerCommand.Connection = value; }
        }

        /// <summary>
        /// Gets the collection of <see cref="T:System.Data.Common.DbParameter"/> objects.
        /// </summary>
        /// <returns>
        /// The parameters of the SQL statement or stored procedure.
        /// </returns>
        protected override DbParameterCollection DbParameterCollection
        {            
            get
            {
                IDataParameterCollection collection = InnerCommand.Parameters;
                if (collection is DbParameterCollection) return (DbParameterCollection)collection;
                return new GtidParameterCollection(collection, this);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="P:System.Data.Common.DbCommand.DbTransaction"/> within which this <see cref="T:System.Data.Common.DbCommand"/> object executes.
        /// </summary>
        /// <returns>
        /// The transaction within which a Command object of a .NET Framework data provider executes. The default value is a null reference (Nothing in Visual Basic).
        /// </returns>
        protected override DbTransaction DbTransaction
        {
            get
            {
                if (InnerCommand.Transaction is DbTransaction)
                    return (DbTransaction) InnerCommand.Transaction;
                return new GtidTransaction(InnerCommand.Transaction, InnerConnection);
            }
            set
            {
                if (InnerCommand.Transaction is DbTransaction)
                    InnerCommand.Transaction = value;
                else
                    InnerCommand.Transaction = ((GtidTransaction)value).InnerTransaction;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command object should be visible in a customized interface control.
        /// </summary>
        /// <returns>
        /// true, if the command object should be visible in a control; otherwise false. The default is true.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override bool DesignTimeVisible
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the transaction within which the Command object of a .NET Framework data provider executes.
        /// </summary>
        /// <returns>
        /// the Command object of a .NET Framework data provider executes. The default value is null.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public new GtidTransaction Transaction
        {
            get
            {
                if (InnerTransaction is GtidTransaction)
                    return (GtidTransaction)InnerTransaction;
                return new GtidTransaction(InnerCommand.Transaction, InnerConnection);
            }
            set
            {
                InnerTransaction = value;
                InnerCommand.Transaction = value.InnerTransaction;

                //if (InnerCommand.Transaction is GtidTransaction)
                //    InnerCommand.Transaction = value;
                //else
                //    InnerCommand.Transaction = value.InnerTransaction;
            }
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source.
        /// </summary>
        /// <returns>
        /// The text command to execute. The default value is an empty string ("").
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string CommandText
        {
            get { return InnerCommand.CommandText; } 
            set
            {
                InnerCommand.CommandText = value;

                setCommandType(InnerCommand.CommandText);
            }
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void setCommandType(string cmdText)
        {
            string tmp = cmdText.Trim().ToUpper();

            if (tmp.StartsWith("SELECT ") || tmp.StartsWith("INSERT ") || tmp.StartsWith("UPDATE ") || tmp.StartsWith("DELETE "))
                InnerCommand.CommandType = CommandType.Text;
            else
                InnerCommand.CommandType = CommandType.StoredProcedure;

            if (Environment.UserInteractive)
                isAutoRun = ConnectionUtil.Instance.
                    AutoQueryString.Exists(delegate(string match)
                    {
                        return match.ToUpper() == tmp;
                    });            
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error.
        /// </summary>
        /// <returns>
        /// The time (in seconds) to wait for the command to execute. The default value is 30 seconds.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The property value assigned is less than 0. </exception><filterpriority>2</filterpriority>
        public override int CommandTimeout
        {
            get { return InnerCommand.CommandTimeout; }
            set { InnerCommand.CommandTimeout = value; }
        }

        /// <summary>
        /// Indicates or specifies how the <see cref="P:System.Data.IDbCommand.CommandText"/> property is interpreted.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.CommandType"/> values. The default is Text.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override CommandType CommandType
        {
            get { return InnerCommand.CommandType; } 
            set { InnerCommand.CommandType = value; }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Data.IDataParameterCollection"/>.
        /// </summary>
        /// <returns>
        /// The parameters of the SQL statement or stored procedure.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        private GtidParameterCollection parameters;

        public new GtidParameterCollection Parameters
        {
            get
            {
                if (parameters == null)
                {
                    IDataParameterCollection collection = DbParameterCollection;
                    if (collection is GtidParameterCollection)
                        parameters = (GtidParameterCollection)collection;
                    else
                        parameters = new GtidParameterCollection(collection, this);
                }
                return parameters;
            }
        }

        /// <summary>
        /// Gets or sets how command results are applied to the <see cref="T:System.Data.DataRow"/> when used by the <see cref="M:System.Data.IDataAdapter.Update(System.Data.DataSet)"/> method of a <see cref="T:System.Data.Common.DbDataAdapter"/>.
        /// </summary>
        /// <returns>
        /// One of the <see cref="T:System.Data.UpdateRowSource"/> values. The default is Both unless the command is automatically generated. Then the default is None.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The value entered was not one of the <see cref="T:System.Data.UpdateRowSource"/> values. </exception><filterpriority>2</filterpriority>
        public override UpdateRowSource UpdatedRowSource
        {
            get { return InnerCommand.UpdatedRowSource; }
            set { InnerCommand.UpdatedRowSource = value; }
        }

        #endregion

    }

    #endregion

    public class GtidDataAdapter : DbDataAdapter
    {
        private readonly DbDataAdapter innerAdapter;

        public GtidDataAdapter()
        {
            SelectCommand = new GtidCommand();
        }

        public GtidDataAdapter(string selectCommandText, GtidConnection connection)
        {
            SelectCommand = new GtidCommand(selectCommandText, connection);
        }

        public GtidDataAdapter(GtidCommand selectCommand)
        {
            SelectCommand = selectCommand;
        }

        public new int Fill(DataTable dataTable)
        {
            //if (SelectCommand.Connection.InnerConnection is OracleConnection && SelectCommand.CommandType == CommandType.StoredProcedure)
            //{
            //    OracleParameter param = new OracleParameter("p_cursor", OracleDbType.RefCursor);
            //    param.Direction = ParameterDirection.Output;
            //    SelectCommand.Parameters.Insert(0, param);
            //}
            try
            {
                return base.Fill(dataTable);
            }
            catch (OracleException oracleException)
            {
                //Debug.Print("{1}: ThreadId:{0} error on filling data..", Thread.CurrentThread.ManagedThreadId, DateTime.Now);

                //EventLogProvider.Instance.WriteOfflineLog(oracleException.ToString(), "Fill");

                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    switch (oracleException.Number)
                    {
                        case 12571:
                        case 12560:
                        case 12543:
                        case 12514:
                        case 12170:
                        case 3135:
                        case 3113:
                        case 3114:
                        case 1033:
                        case 1034:
                        case -1000:
                        case -3000:
                        case 28547:
                        case 12528:
                        case 12518:
                        case 12541:
                        case 12505:
                        case 00600:
                            {
                                //Debug.Print("{1}: ThreadId:{0} Retry Fill", Thread.CurrentThread.ManagedThreadId, DateTime.Now);

                                //Debug.Print("{0} Thread Id: {1} retry connect on Fill...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                                frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);

                                return Fill(dataTable);
                            }    
                    }
                }
                else
                {
                    ConnectionUtil.Instance.CurrentTransaction.IsNeedRollback = true;
                }

                if (oracleException.Number == 20101)
                {
                    throw new ManagedException(oracleException.Message, false);
                }

                throw new ManagedException(oracleException.Message, false);
            }
            catch (ManagedException ex)
            {
                //EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "Fill");

                throw;
            }
            catch (Exception ex)
            {
                var managedException = new ManagedException(ex.Message, false);

                //EventLogProvider.Instance.WriteOfflineLog(managedException.ToString(), "Fill");

                throw managedException;
            }
        }

        //public override int Fill(DataSet dataSet)
        //{
        //    if (SelectCommand.Connection.InnerConnection is OracleConnection && SelectCommand.CommandType == CommandType.StoredProcedure)
        //    {
        //        OracleParameter param = new OracleParameter("p_cursor", OracleDbType.RefCursor);
        //        param.Direction = ParameterDirection.Output;
        //        SelectCommand.Parameters.Insert(0, param);
        //    }

        //    return base.Fill(dataSet);
        //}

        //public new int Fill(DataSet dataSet, string srcTable)
        //{
        //    if (SelectCommand.Connection.InnerConnection is OracleConnection && SelectCommand.CommandType == CommandType.StoredProcedure)
        //    {
        //        OracleParameter param = new OracleParameter("p_cursor", OracleDbType.RefCursor);
        //        param.Direction = ParameterDirection.Output;
        //        SelectCommand.Parameters.Insert(0, param);
        //    }

        //    return base.Fill(dataSet, srcTable);
        //}

        #region Implementation of IDataAdapter

        public new GtidCommand SelectCommand
        {
            get { return (GtidCommand) base.SelectCommand; }
            set { base.SelectCommand = value; }
        }

        public new GtidCommand InsertCommand
        {
            get { return (GtidCommand)base.InsertCommand; }
            set { base.InsertCommand = value; }
        }

        public new GtidCommand UpdateCommand
        {
            get { return (GtidCommand)base.UpdateCommand; }
            set { base.UpdateCommand = value; }
        }

        public new GtidCommand DeleteCommand
        {
            get { return (GtidCommand)base.DeleteCommand; }
            set { base.DeleteCommand = value; }
        }

        #endregion
    }

    #region Transaction Classes

    internal class GtidSqlTransaction : GtidTransaction
    {
        internal GtidSqlTransaction(SqlTransaction transaction, IDbConnection connection) : base(transaction, connection){}
        public override void Save(string savepointName)
        {
            ((SqlTransaction)InnerTransaction).Save(savepointName);
        }

        public override void Rollback(string savepointName)
        {
            ((SqlTransaction)InnerTransaction).Rollback(savepointName);
        }
    }

    internal class GtidOracleTransaction : GtidTransaction //
    {
        internal GtidOracleTransaction(OracleTransaction transaction, IDbConnection connection) : base(transaction, connection) { }

        public override void Save(string savepointName)
        {
            ((OracleTransaction)InnerTransaction).Save(savepointName);
        }

        public override void Rollback(string savepointName)
        {
            ((OracleTransaction)InnerTransaction).Rollback(savepointName);
        }
    }

    public class GtidTransaction : DbTransaction
    {
        internal IDbTransaction InnerTransaction;
        internal IDbConnection InnerConnection;

        internal GtidTransaction(IDbTransaction innerTransaction, IDbConnection innerConnection)
        {
            InnerTransaction = innerTransaction;
            InnerConnection = innerConnection;
            IsNeedRollback = false;
        }

        //~GtidTransaction()
        //{
        //    Dispose(false);
        //}

        internal bool IsNeedRollback { get; set; }

        public new GtidConnection Connection 
        {
            get {
                if (DbConnection == null) return null;
                if (DbConnection is GtidConnection) return (GtidConnection)DbConnection;
                return (GtidConnection) InnerConnection;
            }
        }
       
        public virtual void Save(string savepointName)
        {
            throw new NotImplementedException("Save method with savepointName is not implemented.");
        }

        public virtual void Rollback(string savepointName)
        {
            throw new NotImplementedException("Rollback method with savepointName is not implemented.");
        }

        #region Overrides of DbTransaction

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Commit()
        {
            InnerTransaction.Commit();
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        /// <filterpriority>1</filterpriority>
        public override void Rollback()
        {
            InnerTransaction.Rollback();
        }

        /// <summary>
        /// Specifies the <see cref="T:System.Data.Common.DbConnection"/> object associated with the transaction.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Common.DbConnection"/> object associated with the transaction.
        /// </returns>
        protected override DbConnection DbConnection
        {
            get {
                if (InnerTransaction == null) return null;
                return (DbConnection) InnerTransaction.Connection;
            }
        }

        /// <summary>
        /// Specifies the <see cref="T:System.Data.IsolationLevel"/> for this transaction.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.IsolationLevel"/> for this transaction.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override IsolationLevel IsolationLevel
        {
            get { return InnerTransaction.IsolationLevel; }
        }

        protected override void Dispose(bool disposing)
        {
            if (InnerTransaction != null) InnerTransaction.Dispose();
            base.Dispose(disposing);
            InnerConnection = null;
            InnerTransaction = null;
        }

        #endregion
    }
    
    internal class GtidReader : IDataReader
    {
        private readonly IDataReader innerDataReader;

        internal GtidReader(IDataReader dr)
        {
            innerDataReader = dr;
        }
        
        //~GtidReader()
        //{
        //    Dispose();
        //}

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            innerDataReader.Dispose();
        }

        #endregion

        #region Implementation of IDataRecord

        /// <summary>
        /// Gets the name for the field to find.
        /// </summary>
        /// <returns>
        /// The name of the field or the empty string (""), if there is no value to return.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public string GetName(int i)
        {
            return innerDataReader.GetName(i);
        }

        /// <summary>
        /// Gets the data type information for the specified field.
        /// </summary>
        /// <returns>
        /// The data type information for the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public string GetDataTypeName(int i)
        {
            return innerDataReader.GetDataTypeName(i);
        }

        /// <summary>
        /// Gets the <see cref="T:System.Type"/> information corresponding to the type of <see cref="T:System.Object"/> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)"/>.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Type"/> information corresponding to the type of <see cref="T:System.Object"/> that would be returned from <see cref="M:System.Data.IDataRecord.GetValue(System.Int32)"/>.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public Type GetFieldType(int i)
        {
            return innerDataReader.GetFieldType(i);
        }

        /// <summary>
        /// Return the value of the specified field.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Object"/> which will contain the field value upon return.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public object GetValue(int i)
        {
            return innerDataReader.GetValue(i);
        }

        /// <summary>
        /// Gets all the attribute fields in the collection for the current record.
        /// </summary>
        /// <returns>
        /// The number of instances of <see cref="T:System.Object"/> in the array.
        /// </returns>
        /// <param name="values">An array of <see cref="T:System.Object"/> to copy the attribute fields into. </param><filterpriority>2</filterpriority>
        public int GetValues(object[] values)
        {
            return innerDataReader.GetValues(values);
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <returns>
        /// The index of the named field.
        /// </returns>
        /// <param name="name">The name of the field to find. </param><filterpriority>2</filterpriority>
        public int GetOrdinal(string name)
        {
            return innerDataReader.GetOrdinal(name);
        }

        /// <summary>
        /// Gets the value of the specified column as a Boolean.
        /// </summary>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public bool GetBoolean(int i)
        {
            return innerDataReader.GetBoolean(i);
        }

        /// <summary>
        /// Gets the 8-bit unsigned integer value of the specified column.
        /// </summary>
        /// <returns>
        /// The 8-bit unsigned integer value of the specified column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public byte GetByte(int i)
        {
            return innerDataReader.GetByte(i);
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <returns>
        /// The actual number of bytes read.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param><param name="fieldOffset">The index within the field from which to start the read operation. </param><param name="buffer">The buffer into which to read the stream of bytes. </param><param name="bufferoffset">The index for <paramref name="buffer"/> to start the read operation. </param><param name="length">The number of bytes to read. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return innerDataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Gets the character value of the specified column.
        /// </summary>
        /// <returns>
        /// The character value of the specified column.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public char GetChar(int i)
        {
            return innerDataReader.GetChar(i);
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// </summary>
        /// <returns>
        /// The actual number of characters read.
        /// </returns>
        /// <param name="i">The zero-based column ordinal. </param><param name="fieldoffset">The index within the row from which to start the read operation. </param><param name="buffer">The buffer into which to read the stream of bytes. </param><param name="bufferoffset">The index for <paramref name="buffer"/> to start the read operation. </param><param name="length">The number of bytes to read. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return innerDataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }

        /// <summary>
        /// Returns the GUID value of the specified field.
        /// </summary>
        /// <returns>
        /// The GUID value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public Guid GetGuid(int i)
        {
            return innerDataReader.GetGuid(i);
        }

        /// <summary>
        /// Gets the 16-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 16-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public short GetInt16(int i)
        {
            return innerDataReader.GetInt16(i);
        }

        /// <summary>
        /// Gets the 32-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 32-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public int GetInt32(int i)
        {
            return innerDataReader.GetInt32(i);
        }

        /// <summary>
        /// Gets the 64-bit signed integer value of the specified field.
        /// </summary>
        /// <returns>
        /// The 64-bit signed integer value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public long GetInt64(int i)
        {
            return innerDataReader.GetInt64(i);
        }

        /// <summary>
        /// Gets the single-precision floating point number of the specified field.
        /// </summary>
        /// <returns>
        /// The single-precision floating point number of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public float GetFloat(int i)
        {
            return innerDataReader.GetFloat(i);
        }

        /// <summary>
        /// Gets the double-precision floating point number of the specified field.
        /// </summary>
        /// <returns>
        /// The double-precision floating point number of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public double GetDouble(int i)
        {
            return innerDataReader.GetDouble(i);
        }

        /// <summary>
        /// Gets the string value of the specified field.
        /// </summary>
        /// <returns>
        /// The string value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public string GetString(int i)
        {
            return innerDataReader.GetString(i);
        }

        /// <summary>
        /// Gets the fixed-position numeric value of the specified field.
        /// </summary>
        /// <returns>
        /// The fixed-position numeric value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public decimal GetDecimal(int i)
        {
            return innerDataReader.GetDecimal(i);
        }

        /// <summary>
        /// Gets the date and time data value of the specified field.
        /// </summary>
        /// <returns>
        /// The date and time data value of the specified field.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public DateTime GetDateTime(int i)
        {
            return innerDataReader.GetDateTime(i);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.IDataReader"/> for the specified column ordinal.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.IDataReader"/>.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public IDataReader GetData(int i)
        {
            return innerDataReader.GetData(i);
        }

        /// <summary>
        /// Return whether the specified field is set to null.
        /// </summary>
        /// <returns>
        /// true if the specified field is set to null; otherwise, false.
        /// </returns>
        /// <param name="i">The index of the field to find. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        public bool IsDBNull(int i)
        {
            return innerDataReader.IsDBNull(i);
        }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        /// <returns>
        /// When not positioned in a valid recordset, 0; otherwise, the number of columns in the current record. The default is -1.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public int FieldCount
        {
            get { return innerDataReader.FieldCount; }
        }

        /// <summary>
        /// Gets the column located at the specified index.
        /// </summary>
        /// <returns>
        /// The column located at the specified index as an <see cref="T:System.Object"/>.
        /// </returns>
        /// <param name="i">The zero-based index of the column to get. </param><exception cref="T:System.IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="P:System.Data.IDataRecord.FieldCount"/>. </exception><filterpriority>2</filterpriority>
        object IDataRecord.this[int i]
        {
            get { return innerDataReader[i]; }
        }

        /// <summary>
        /// Gets the column with the specified name.
        /// </summary>
        /// <returns>
        /// The column with the specified name as an <see cref="T:System.Object"/>.
        /// </returns>
        /// <param name="name">The name of the column to find. </param><exception cref="T:System.IndexOutOfRangeException">No column with the specified name was found. </exception><filterpriority>2</filterpriority>
        object IDataRecord.this[string name]
        {
            get { return innerDataReader[name]; }
        }

        #endregion

        #region Implementation of IDataReader

        /// <summary>
        /// Closes the <see cref="T:System.Data.IDataReader"/> Object.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Close()
        {
            innerDataReader.Close();

            if (!ConnectionUtil.Instance.IsInTransaction)
                if (ConnectionUtil.CurrentManagedObject != null &&
                    ConnectionUtil.CurrentManagedObject.Connection != null &&
                    ConnectionUtil.CurrentManagedObject.Connection.State == ConnectionState.Open)

                ConnectionUtil.CurrentManagedObject.Connection.Close();

        }

        /// <summary>
        /// Returns a <see cref="T:System.Data.DataTable"/> that describes the column metadata of the <see cref="T:System.Data.IDataReader"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.DataTable"/> that describes the column metadata.
        /// </returns>
        /// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Data.IDataReader"/> is closed. </exception><filterpriority>2</filterpriority>
        public DataTable GetSchemaTable()
        {
            return innerDataReader.GetSchemaTable();
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool NextResult()
        {
            return innerDataReader.NextResult();
        }

        /// <summary>
        /// Advances the <see cref="T:System.Data.IDataReader"/> to the next record.
        /// </summary>
        /// <returns>
        /// true if there are more rows; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool Read()
        {
            return innerDataReader.Read();
        }

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row.
        /// </summary>
        /// <returns>
        /// The level of nesting.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public int Depth
        {
            get { return innerDataReader.Depth; }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        /// <returns>
        /// true if the data reader is closed; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool IsClosed
        {
            get { return innerDataReader.IsClosed; }
        }

        /// <summary>
        /// Gets the number of rows changed, inserted, or deleted by execution of the SQL statement.
        /// </summary>
        /// <returns>
        /// The number of rows changed, inserted, or deleted; 0 if no rows were affected or the statement failed; and -1 for SELECT statements.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public int RecordsAffected
        {
            get { return innerDataReader.RecordsAffected; }
        }

        #endregion
    }

    #endregion

    internal class TnsNamesReader
    {
        private const string SUB_PATH = @"network\ADMIN\tnsnames.ora";
        private static readonly Regex EnvironmentPathRegex = new Regex(@"[a-zA-Z]:\\[a-zA-Z0-9\\]*(oracle|app)[a-zA-Z0-9_.\\]*(?=bin)", RegexOptions.Compiled);
        private static readonly Regex TnsNamesEntryRegex = new Regex(@"[\n][\s]*[^\(][a-zA-Z0-9_.]+[\s]*", RegexOptions.Compiled);
        private const string TNS_ADMIN = "TNS_ADMIN";

        public static List<string> LoadHost(string tnsName)
        {
            var hostEntries = new List<string>();

            string tnsNameEntry = String.Empty;

            if (!tnsName.ToLower().Contains("host=") && !tnsName.Contains("/")) //Using the TNS Alias
            {
                var tnsNamesFile = GetPath();

                foreach (string sTnsFile in tnsNamesFile)
                {
                    tnsNameEntry = File.ReadAllText(sTnsFile).ToLower();
                    tnsNameEntry = tnsNameEntry.Replace(" ", String.Empty)
                        .Replace("\r", String.Empty)
                        .Replace("\n", String.Empty)
                        .ToLower();

                    if (tnsNameEntry.Contains(String.Format("{0}=", tnsName.ToLower())))
                    {
                        tnsNameEntry = tnsNameEntry.Substring(tnsNameEntry.IndexOf(String.Format("{0}=", tnsName.ToLower())));
                        tnsNameEntry = tnsNameEntry.Substring(0, tnsNameEntry.IndexOf(")))"));
                        break;
                    }
                }                
            }
            else if (tnsName.Contains("/")) //Using Easy Connect Naming Method
            {
                hostEntries.Add(tnsName.Substring(0, tnsName.LastIndexOf("/")).Replace("/", String.Empty));
            }
            else
            {
                tnsNameEntry = tnsName.ToLower(); //Using the Connect Descriptor
            }

            while (tnsNameEntry.IndexOf("host=") > 0)
            {
                string host = tnsNameEntry.Substring(tnsNameEntry.IndexOf("host="));
                host = host.Substring(0, host.IndexOf(")"));
                host = host.Substring(host.IndexOf("=") + 1);
                hostEntries.Add(host);

                tnsNameEntry = tnsNameEntry.Substring(tnsNameEntry.IndexOf("host=") + 1);
            }

            return hostEntries;

        }

        public static List<string> LoadTnsNamesEntries()
        {
            var tnsNamesFile = GetPath();

            var tnsNamesEntries = new List<String>();

            foreach (string sTnsFile in tnsNamesFile)
            {
                var matches = TnsNamesEntryRegex.Matches(File.ReadAllText(sTnsFile));

                foreach (var match in matches)
                {
                    tnsNamesEntries.Add(match.ToString().Trim());
                }
            }

            return tnsNamesEntries;//.Sort(entries => entries).ToList();                

        }

        private static IEnumerable<string> GetPath()
        {
            var tnsFiles = new List<string>();

            var path = Environment.GetEnvironmentVariable("Path");

            if (path != null)
            {
                var matches = EnvironmentPathRegex.Matches(path);

                foreach (var match in matches)
                {
                    if (File.Exists(Path.Combine(match.ToString(), SUB_PATH)))
                    {
                        tnsFiles.Add(Path.Combine(match.ToString(), SUB_PATH));
                    }
                }
            }
            
            string env = Environment.GetEnvironmentVariable(TNS_ADMIN);

            if (!String.IsNullOrEmpty(env))
            {
                tnsFiles.Add(Path.Combine(env,"tnsnames.ora"));
            }
            
            if (tnsFiles.Count > 0) return tnsFiles;

            throw new FileNotFoundException("File not found.", "tnsnames.ora");
        }

    }

}
