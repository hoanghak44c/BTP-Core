using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.ServiceProcess;
using System.Threading;
using QLBH.Core.Data;
using QLBH.Core.Exceptions;
using QLBH.Core.Providers;
using QLBH.Core.Threads;
using ThreadState = System.Threading.ThreadState;

namespace QLBH.Core.Services
{
    public enum EScheduleType
    {
        CONTINUOUSLY = 1,
        /// <summary>
        /// on the first day of year
        /// </summary>
        YEARLY = 2,
        /// <summary>
        /// on the first day of month
        /// </summary>
        MONTHLY = 3,
        /// <summary>
        /// on MO0000
        /// </summary>
        WEEKLY = 4,
        /// <summary>
        /// at 23:59:59
        /// </summary>
        DAILY = 5,
        /// <summary>
        /// at hh:59:59
        /// </summary>
        HOURLY = 6,
        /// <summary>
        /// at hh:mm:ss
        /// </summary>
        TIMELY = 7
    }

    public class ServiceScheduleInfo
    {
        public int ScheduleType { get; set; }

        public DateTime NextRunTime { get; set; }

        public int Id { get; set; }

        private string serviceName;

        public string ServiceName
        {
            get { return serviceName; }
            set { serviceName = GtidCryption.Me.Decrypt(value, true); }
        }

    }

    /// <summary>
    /// Cac service base tren class nay, khi chay lan dau se duoc dang ky voi he thong quan ly.
    /// He thong se quan ly, theo doi trang thai va lich hoat dong cua cac service da dang ky voi no
    /// </summary>
    public abstract class ScheduleServiceBase<T> : ServiceBase where T : class
    {
        /// <summary>
        /// This only true when process is schedule manager
        /// </summary>
        protected readonly bool IsManager;

        protected ScheduleServiceBase()
        {
            ConnectionUtil.Instance.IsUAT = 1;
            hasRun = 0;
        }

        protected ScheduleServiceBase(bool isMan)
        {
            ConnectionUtil.Instance.IsUAT = 1;
            IsManager = isMan;
        }

        /// <summary>
        /// WorkerThread is the service's main thread
        /// </summary>
        protected Thread WorkerThread, StopThread;

        /// <summary>
        /// Determine you want to stop this service
        /// </summary>
        protected bool IsStopOnDemand;
        /// <summary>
        /// The service's jobs need to do
        /// </summary>
        protected IList<T> JobList;

        protected int MaxWorker = 1;

        protected int Interval = 60000;

        protected ServiceScheduleInfo Info { get; private set; }

        /// <summary>
        /// the hien so thread da hoan thanh
        /// </summary>
        private int hasRun;

        private int serviceId;

        protected abstract void OnStartEx();

        public void ForTestUnit(string[] args)
        {
            OnStart(args);
        }

        sealed protected override void OnStart(string[] args)
        {
            try
            {
                if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
                    Thread.CurrentThread.Name = "Main Thread";

                //EventLog.WriteEntry("ScheduleServiceBase", "ScheduleServiceBase check infor service " + ServiceName, EventLogEntryType.Information);

                Info = !IsManager ? SchedulerProvider.Instance.GetServiceScheduleInfo(ServiceName)
                    : new ServiceScheduleInfo()
                    {
                        ScheduleType = Convert.ToInt32(EScheduleType.CONTINUOUSLY)
                    };

                //EventLog.WriteEntry("ScheduleServiceBase", "ScheduleServiceBase register infor service " + ServiceName, EventLogEntryType.Information);

                //kiem tra xem da duoc dang ky chua
                if (Info == null)
                {
                    EventLog.WriteEntry("ScheduleServiceBase", "ScheduleServiceBase register infor service " + ServiceName, EventLogEntryType.Information);
                    //thuc hien dang ky
                    serviceId = SchedulerProvider.Instance.Register(ServiceName);
                }

                //EventLog.WriteEntry("ScheduleServiceBase", "ScheduleServiceBase start service " + ServiceName, EventLogEntryType.Information);

                //He thong quan ly se su dung Id nhu la tham so nhan dien khi khoi dong mot process
                if (Info != null && (Info.ScheduleType == Convert.ToInt32(EScheduleType.CONTINUOUSLY) ||

                    //hoac la hang tuan, voi dinh dang ngay thu may trong tuan
                    //info.NextRunTime == (DateTime.Now.DayOfWeek).ToString().PadLeft(2, '0') + DateTime.Now.ToString("HHmm") ||

                    //hoac la hang ngay
                    //info.NextRunTime == String.Format("00{0}", DateTime.Now.ToString("HHmm"))

                    Info.NextRunTime <= DateTime.Now) &&
                    
                    ((args.Length != 0 && args[0] == Info.Id.ToString()) || IsManager))
                {
                    serviceId = Info.Id;

                    //kich hoat service on time
                    IsStopOnDemand = false;
                    WorkerThread = new Thread(ScheduleWorker);
                    OnStartEx();
                    if (String.IsNullOrEmpty(WorkerThread.Name)) WorkerThread.Name = String.Format("{0}WorkerThread", ServiceName);
                    WorkerThread.Start(Info);
                    
                    if (!IsManager) SchedulerProvider.Instance.UpdateStatus(serviceId, ServiceControllerStatus.Running);

                    new Thread(StopWaiting).Start();

                }
                else
                {
                    StopThread = new Thread(StopMyself);
                    StopThread.Start();
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.Source, "ServiceName: " + ServiceName + ": " + new ManagedException(ex.Message, false, args).Message, EventLogEntryType.Error);
                //throw;
            }
        }

        private void StopWaiting()
        {
            while (!IsStopOnDemand)
            {
                Thread.CurrentThread.Join(20000);
            }

            Stop();
        }

        protected abstract void OnStopEx();

        sealed protected override void OnStop()
        {
            IsStopOnDemand = true;
            
            OnStopEx();

            try
            {
                if (!IsManager) SchedulerProvider.Instance.UpdateStatus(serviceId, ServiceControllerStatus.Stopped);

                UpdateNextRuntime();
            }
            catch (Exception) { }

            if (WorkerThread != null)
            {
                int i = 0;
                while ((WorkerThread.ThreadState == ThreadState.Running ||
                    WorkerThread.ThreadState == ThreadState.Background ||
                    WorkerThread.ThreadState == ThreadState.WaitSleepJoin) && i < 60)
                {
                    Thread.CurrentThread.Join(1000);
                    i++;
                }
            }
        }

        private void StopMyself()
        {
            IsStopOnDemand = true;

            if (WorkerThread != null)
            {
                int i = 0;
                while ((WorkerThread.ThreadState == ThreadState.Running ||
                    WorkerThread.ThreadState == ThreadState.Background ||
                    WorkerThread.ThreadState == ThreadState.WaitSleepJoin) && i < 60)
                {
                    Thread.CurrentThread.Join(1000);
                    i++;
                }
            }

            Stop();
        }

        /// <summary>
        /// Lap lai cv cua schedule tien hanh chay
        /// </summary>
        /// <param name="param"></param>
        private void ScheduleWorker(object param)
        {
            while (!IsStopOnDemand)
            {
                //if(ServiceName == "KhuyenMaiSyncService")
                //{
                //    EventLog.WriteEntry("Running 1", EventLogEntryType.Information);
                //}

                IList<GenericThread<T>> threads = new List<GenericThread<T>>();
                //old try
                JobList = createJobList();
                if (JobList != null)
                {
                    // init threads
                    int hasStarted = 0;
                    try
                    {
                        while (threads.Count < MaxWorker && threads.Count < JobList.Count && !IsStopOnDemand)
                        {
                            threads.Add(new GenericThread<T>(ScheduleJob) { Name = threads.Count.ToString() });
                            threads[threads.Count - 1].Start(JobList[threads.Count - 1]);
                            hasStarted++;
                            Debug.Print(String.Format("{0}, {1}, {2}", hasRun, hasStarted, JobList.Count));
                            //Trace(JobList[threads.Count - 1]);
                            Delay(5000);
                            //if (ServiceName == "KhuyenMaiSyncService")
                            //{
                            //    EventLog.WriteEntry("Running 2", EventLogEntryType.Information);
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry(ex.Source, ex.Message + "\n" + ex.StackTrace, EventLogEntryType.Error);
                    }
                    // --------------
                    try
                    {
                        hasRun = 0;
                        // check state and feed working for threads
                        while (hasRun < JobList.Count && hasStarted < JobList.Count && !IsStopOnDemand && threads.Count > 0)
                        {
                            //string sState = String.Empty;

                            for (int i = 0; i < threads.Count; i++)
                            {
                                //sState += threads[i].Name + ":" + threads[i].ThreadState;

                                if (IsStopOnDemand) break;

                                if (threads[i].ThreadState != ThreadState.Running)
                                {
                                    if (threads[i].ThreadState == ThreadState.Stopped)
                                    {
                                        hasRun += 1;
                                    } 
                                    //else
                                    //{
                                    //    try
                                    //    {
                                    //        threads[i].Thread.Abort();
                                    //        hasStarted--;
                                    //    }
                                    //    catch (Exception ex)
                                    //    {
                                    //        EventLog.WriteEntry(ex.Source, ex.Message + "\n" + ex.StackTrace, EventLogEntryType.Error);
                                    //    }
                                    //}
                                    
                                    if (hasStarted < JobList.Count)
                                    {
                                        threads[i] = new GenericThread<T>(ScheduleJob) { Name = i.ToString() };
                                        threads[i].Start(JobList[hasStarted]);
                                        hasStarted++;
                                        //Trace(JobList[threads.Count - 1]);
                                    }
                                }

                                Delay(5000);
                            }

                            //if (ServiceName == "KhuyenMaiSyncService")
                            //{
                            //    EventLog.WriteEntry("State: " + sState, EventLogEntryType.Information);
                            //}

                        } //end of while has run
                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry(ex.Source, ex.Message + "\n" + ex.StackTrace, EventLogEntryType.Error);
                    }
                    // ---------------------
                    //ensure that threads has finished their works.
                    try
                    {
                        bool isCompleted = false;
                        int retryIndex = 0;
                        while (!isCompleted && !IsStopOnDemand)
                        {
                            //if (ServiceName == "KhuyenMaiSyncService")
                            //{
                            //    EventLog.WriteEntry("Running 4", EventLogEntryType.Information);
                            //}
                            isCompleted = true;
                            
                            for (int i = 0; i < threads.Count; i++)
                            {
                                Thread.CurrentThread.Join(2000);

                                if (threads[i].ThreadState == ThreadState.Running)
                                {
                                    isCompleted = false;

                                    break;
                                }
                            }

                            if (retryIndex < threads.Count && isCompleted)
                            {
                                retryIndex += 1;
                                
                                isCompleted = false;
                            }
                        } // end of while complete


                    }
                    catch (Exception ex)
                    {
                        EventLog.WriteEntry(ex.Source, ex.Message + "\n" + ex.StackTrace, EventLogEntryType.Error);
                    }

                } // end of if joblist

                //old catch


                if (!IsManager 
                    //&& ((ServiceScheduleInfo)param).ScheduleType != Convert.ToInt32(EScheduleType.CONTINUOUSLY)
                    )
                {// neu ko phai kieu continuously thi stop
                    //EventLog.WriteEntry(ServiceName, "Stop on demand.", EventLogEntryType.Information);
                    //StopThread = new Thread(StopMyself);
                    //StopThread.Start();
                    IsStopOnDemand = true;
                    //break;
                }

                //threads = null;

                if (JobList != null) JobList.Clear();

                JobList = null;

                GC.Collect();

                // da chay het joblist, wait in interval timestamp.
                //EventLog.WriteEntry(ServiceName, String.Format("Waiting {0} seconds ...", Interval/1000), EventLogEntryType.Information);

                Delay(Interval);

                //if(!String.IsNullOrEmpty(Convert.ToString(threads)))
                //{
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(threads));

                //    //do nothing here

                //    //EventLog.WriteEntry(ServiceName, "Release all resource", EventLogEntryType.Information);
                //}

            } // end of while big
            //EventLog.WriteEntry(ServiceName, "Finish schedule worker", EventLogEntryType.Information);
        }

        protected virtual void Trace(T param) { }

        private void Delay(int interval)
        {
            int iDelay = interval / 1000;

            while (iDelay > 0 && !IsStopOnDemand)
            {
                Thread.CurrentThread.Join(1000);
                iDelay -= 1;
            }            
        }

        /// <summary>
        /// create list jobs need to do
        /// </summary>
        /// <returns>list parameters of jobs</returns>
        protected abstract IList<T> createJobList();

        private void ScheduleJob(T param)
        {
            try
            {
                //EventLog.WriteEntry(ServiceName, "Call DoJob with param ...", EventLogEntryType.Information);
                DoJob(param);
                //CountCompleted();
            }
            catch(SynchronizationLockException ex)
            {
                EventLog.WriteEntry("Thread " + Thread.CurrentThread.Name + "-" + ex.Source,
                            ex.Message + "\n" + ex.StackTrace, EventLogEntryType.Error);

                IsStopOnDemand = true;
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Thread " + Thread.CurrentThread.Name + "-" + ex.Source,
                            ex.Message + "\n" + ex.StackTrace, EventLogEntryType.Error);

                IsStopOnDemand = true;
            }
        }

        /// <summary>
        /// Do a job in the list jobs created.
        /// </summary>
        /// <param name="param">parameter need to do this job</param>
        protected abstract void DoJob(T param);

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void CountCompleted()
        {
            hasRun += 1;
        }

        protected virtual void UpdateNextRuntime()
        {
            SchedulerProvider.Instance.UpdateNextTime(serviceId, Info.NextRunTime);
        }
    }
}
