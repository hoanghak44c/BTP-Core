using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using QLBH.Core.Providers;

namespace QLBH.Core.Services
{
    public partial class ManagerService : ScheduleServiceBase<ServiceScheduleInfo>
    {
        public ManagerService()
            : base(true)
        {
            InitializeComponent();
        }

        protected override void OnStartEx()
        {
            WorkerThread.Name = "SaleTidManagerServiceWorkerThread";
            //System.Diagnostics.EventLog.WriteEntry(ServiceName, ServiceName + " started.", EventLogEntryType.Information);
        }

        protected override void OnStopEx()
        {
            //System.Diagnostics.EventLog.WriteEntry(ServiceName, ServiceName + " stopped.", EventLogEntryType.Information);
        }

        /// <summary>
        /// create list jobs need to do
        /// </summary>
        /// <returns></returns>
        protected override IList<ServiceScheduleInfo> createJobList()
        {
            return SchedulerProvider.Instance.GetListServiceScheduleInfo();
            //List<ServiceScheduleInfo> listScheduleInfo = SchedulerProvider.Instance.GetListServiceScheduleInfo();
            //IList<object> result = new List<object>();
            //// add them mot service de cap nhat dinh ki NextTime Run cua cac service
            //result.Add(null);
            //// add cac service child
            //foreach (ServiceScheduleInfo serviceScheduleInfo in listScheduleInfo)
            //{
            //    result.Add(serviceScheduleInfo);
            //}
            //return result;
        }

        private class TracedService
        {
            public int Pid;
            public string ServiceName;
            public double TotalMinutes;
            public int RetriedTime;
        }
        private List<TracedService> lstStopPending;

        protected override void DoJob(ServiceScheduleInfo serviceScheduleHeaderInfo)
        {
            //System.Diagnostics.EventLog.WriteEntry(ServiceName, ServiceName + " implementing its job", EventLogEntryType.Information);
            // param = null tuc day la thread update state cua cac thread khac
            //if (param == null)
            //{
            //    updateServiceChildState();
            //    return;
            //}
            //ServiceScheduleInfo serviceScheduleInfo = (ServiceScheduleInfo)param;
            try
            {
                var serviceController = new ServiceController(serviceScheduleHeaderInfo.ServiceName);
                
                Process[] processes = Process.GetProcessesByName(String.Format("QLBH.{0}", serviceController.ServiceName));
                
                if (processes.Length == 0) // && serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    //DateTime now = DateTime.Now;
                    //string exDayStr = (now.Day).ToString().PadLeft(2, '0') + now.ToString("HHmm"); // exact day
                    //string evDayStr = String.Format("00{0}", DateTime.Now.ToString("HHmm")); // daily

                    if (// daily
                        //(serviceScheduleHeaderInfo.NextRunTime == evDayStr &&
                        // serviceScheduleHeaderInfo.ScheduleType == Convert.ToInt32(EScheduleType.DAILY)) ||
                        
                        // //hoac la hang tuan, voi dinh dang ngay thu may trong tuan
                        //(serviceScheduleHeaderInfo.NextRunTime == exDayStr &&
                        // (serviceScheduleHeaderInfo.ScheduleType == Convert.ToInt32(EScheduleType.MONTHLY) ||
                        //  serviceScheduleHeaderInfo.ScheduleType == Convert.ToInt32(EScheduleType.WEEKLY))) ||

                        serviceScheduleHeaderInfo.NextRunTime <= DateTime.Now ||

                        serviceScheduleHeaderInfo.ScheduleType == Convert.ToInt32(EScheduleType.CONTINUOUSLY))
                    {
                        serviceController.Start(new[] {serviceScheduleHeaderInfo.Id.ToString()});                        
                    }
                } 
                else
                {
                    if (lstStopPending == null) lstStopPending = new List<TracedService>();

                    lstStopPending.RemoveAll(delegate(TracedService match)
                                                 {
                                                     return match.ServiceName ==

                                                            serviceController.ServiceName &&

                                                            match.Pid != processes[0].Id;

                                                 });

                    TracedService tracedService =
                        lstStopPending.Find(delegate(TracedService match)
                                                {
                                                    return match.ServiceName ==

                                                           serviceController.ServiceName &&

                                                           match.Pid == processes[0].Id;
                                                });

                    if (tracedService == null)
                    {
                        lstStopPending.Add(new TracedService
                        {
                            Pid = processes[0].Id,

                            ServiceName = serviceController.ServiceName,

                            TotalMinutes = processes[0].TotalProcessorTime.TotalMinutes,
                        });
                    }
                    else if (serviceController.Status == ServiceControllerStatus.StopPending)
                    {
                        if (tracedService.RetriedTime < 3)

                            tracedService.RetriedTime += 1;

                        else

                        {
                            lstStopPending.Remove(tracedService);

                            if (processes.Length > 0) processes[0].Kill();
                        }
                    }
                    else if (tracedService.TotalMinutes == processes[0].TotalProcessorTime.TotalMinutes)
                    {
                        if (tracedService.RetriedTime < 3)

                            tracedService.RetriedTime += 1;

                        else

                            serviceController.Stop();
                    }
                    else
                    {
                        tracedService.TotalMinutes = processes[0].TotalProcessorTime.TotalMinutes;

                        tracedService.RetriedTime = 0;
                    }
                    
                    processes[0].Close();
                }
                
                serviceController.Close();

                processes = null;

                serviceController = null;

                GC.Collect();

                if (!String.IsNullOrEmpty(Convert.ToString(serviceController) + Convert.ToString(processes)))
                {
                    //do nothing here
                    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                                       Convert.ToString(serviceController));
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.Instance.WriteOfflineLog(ex.ToString(), "ManagerService");
            }
        }

        private void updateServiceChildState()
        {
            //List<int> serviceIds = CScheduler.getScheduleServiceIds();
            //foreach (int id in serviceIds)
            //{
            //    CScheduler.UpdateNextTime(id);
            //}
            // sau nay xoa dong nay di
            //CExecLog.insertLog("Check update next time", string.Empty, string.Empty);
            Thread.Sleep(10000);
        }
    }
}
