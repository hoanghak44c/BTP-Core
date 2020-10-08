using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceProcess;
using QLBH.Core.Data;
using QLBH.Core.Exceptions;
using QLBH.Core.Services;

namespace QLBH.Core.DAO
{
    public class SchedulerDAO : BaseDAO
    {

        private static SchedulerDAO instance;

        private SchedulerDAO()
        {
        }

        public static SchedulerDAO Instance
        {
            get
            {
                if (instance == null) instance = new SchedulerDAO();
                return instance;
            }
        }

        public List<ServiceScheduleInfo> GetListServiceScheduleInfo()
        {
            return GetListCommand<ServiceScheduleInfo>(spServiceGetListInfo);
        }

        public ServiceScheduleInfo GetServiceScheduleInfo(string serviceName)
        {
            try
            {
                return GetObjectCommand<ServiceScheduleInfo>(spServiceGetInfoByName, GtidCryption.Me.Encrypt(serviceName, true));
            }
            catch (Exception ex)
            {

                throw new ManagedException(ex.Message, false, spServiceGetInfoByName, serviceName);
            }
        }

        public int Register(string serviceName)
        {
            ExecuteCommand(spServiceInsert, GtidCryption.Me.Encrypt(serviceName, true));

            return Convert.ToInt32(Parameters["p_ServiceId"].Value.ToString());
        }

        public void UpdateStatus(int serviceId, ServiceControllerStatus status)
        {
            ExecuteCommand(spServiceUpdateStatus, serviceId, (int)status);
        }

        public void UpdateNextTime(int serviceId, DateTime nextRunTime)
        {
            ExecuteCommand("update tbl_services set nextruntime = :nextRunTime where id = :serviceId",
                           nextRunTime, serviceId);
        }
    }
}