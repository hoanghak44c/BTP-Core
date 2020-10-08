using System;
using System.Collections.Generic;
using System.ServiceProcess;
using QLBH.Core.DAO;
using QLBH.Core.Exceptions;
using QLBH.Core.Services;

namespace QLBH.Core.Providers
{
    public class SchedulerProvider
    {

        private static SchedulerProvider instance;

        private SchedulerProvider()
        {
        }

        public static SchedulerProvider Instance
        {
            get
            {
                if (instance == null) instance = new SchedulerProvider();
                return instance;
            }
        }

        public List<ServiceScheduleInfo> GetListServiceScheduleInfo()
        {
            return SchedulerDAO.Instance.GetListServiceScheduleInfo();
        }

        public ServiceScheduleInfo GetServiceScheduleInfo(string serviceName)
        {
            try
            {
                return SchedulerDAO.Instance.GetServiceScheduleInfo(serviceName);
            }
            catch (Exception ex)
            {
                
                throw new ManagedException(ex.Message, false, serviceName);
            }
        }

        public int Register(string serviceName)
        {
            return SchedulerDAO.Instance.Register(serviceName);
        }

        public void UpdateStatus(int serviceId, ServiceControllerStatus status)
        {
            SchedulerDAO.Instance.UpdateStatus(serviceId, status);
        }

        public void UpdateNextTime(int serviceId, DateTime nextRunTime)
        {
            SchedulerDAO.Instance.UpdateNextTime(serviceId, nextRunTime);
        }
    }
}