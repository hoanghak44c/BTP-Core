using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using QLBH.Core.Data;

namespace QLBH.Core.DAO
{
    public class EventLogDAO : BaseDAO
    {

        private static EventLogDAO instance;

        private EventLogDAO()
        {
        }

        public static EventLogDAO Instance
        {
            get
            {
                if (instance == null) instance = new EventLogDAO();
                return instance;
            }
        }

        public void WriteLog(string description, string type, int errCode)
        {
            try
            {
                if (!ConnectionUtil.Instance.IsInTransaction)
                    ExecuteCommand("sp_EventLog_Write1", description, type, errCode);
                else 
                    WriteOfflineLog(description, type);
            }
            catch (Exception)
            {
                WriteOfflineLog(description, type);
            }
        }

        public void WriteOfflineLog(string description, string type)
        {
            try
            {
                string fileName = AppDomain.CurrentDomain.BaseDirectory +
                                  String.Format("\\QLBH_Log_{0}_{1}.txt", Process.GetCurrentProcess().Id,
                                                Thread.CurrentThread.ManagedThreadId);

                File.AppendAllText(fileName, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss tt") + ": Description: " + description + "\nType: " + type + "\n");
            }
            catch(System.UnauthorizedAccessException)
            {
                if (Environment.UserInteractive)
                {
                    MessageBox.Show("Bạn chưa được phân quyền đầy đủ để thực hiện chương trình này");

                    Environment.Exit(124);
                }
            }
            catch(System.Security.SecurityException)
            {
                if (Environment.UserInteractive)
                {
                    MessageBox.Show("Bạn chưa được phân quyền đầy đủ để thực hiện chương trình này");

                    Environment.Exit(124);
                }
            }
            catch (Exception ex)
            {
                if(Environment.UserInteractive)
                {
                    //MessageBox.Show(ex.Message);

                    //MessageBox.Show(description, type);
                }

                Debug.Print("{0} Thread Id: {1} WriteOfflineLog go to on error...", DateTime.Now, Thread.CurrentThread.ManagedThreadId);

                Debug.Print(ex.ToString());

                Debug.Print(type);
                
                Debug.Print(description);
            }
        }
    }
}