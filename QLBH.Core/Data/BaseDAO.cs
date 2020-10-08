using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Oracle.DataAccess.Client;
using QLBH.Core.Business.Calculations;
using QLBH.Core.Exceptions;
using QLBH.Core.Providers;
using QLBH.Core.Threads;
using ThreadState = System.Threading.ThreadState;

namespace QLBH.Core.Data
{
    public abstract class BaseDAO
    {
        #region tbl_HangTonKho
        internal protected const string TBL_HANG_TON_KHO = "tbl_HangTonKho";
        internal protected const string spHangTonKhoInsert = "sp_TonKho_Insert";
        internal protected const string spHangTonKhoInsert2 = "sp_TonKho_Insert2";
        internal protected const string spHangTonKhoUpdate = "sp_TonKho_Update";
        internal protected const string spHangTonKhoUpdate2 = "sp_TonKho_Update2";
        internal protected const string spHangTonKhoUpdate3 = "sp_TonKho_Update3";
        internal protected const string spHangTonKhoGetById = "sp_TonKho_GetById";
        internal protected const string spHangTonKhoGetSoTonDauKy = "sp_TonKho_GetTonDau";
        #endregion

        #region tbl_The_Kho
        internal protected const string spTheKhoWriteLog = "sp_TheKho_Log";
        internal protected const string spTheKhoDelete = "sp_TheKho_Delete";
        internal protected const string spTheKhoTonTruoc = "sp_TheKho_TonTruoc";
        internal protected const string spTheKhoGetBy = "sp_TheKho_GetBy";
        #endregion

        #region tbl_HangHoa_ChiTiet
        internal protected const string spHangHoaChiTietInsert = "sp_HangHoa_ChiTiet_Insert";
        internal protected const string spHangHoaChiTietInsert2 = "sp_HangHoa_ChiTiet_Insert2";
        internal protected const string spHangHoaChiTietInsert3 = "sp_HangHoa_ChiTiet_Insert3";
        internal protected const string spHangHoaChiTietInsert4 = "sp_HangHoa_ChiTiet_Insert4";
        internal protected const string spHangHoaChiTietUpdate = "sp_HangHoa_ChiTiet_Update";
        internal protected const string spHangHoaChiTietUpdate2 = "sp_HangHoa_ChiTiet_Update2";
        internal protected const string spHangHoaChiTietUpdate3 = "sp_HangHoa_ChiTiet_Update3";
        internal protected const string spHangHoaChiTietUpdate4 = "sp_HangHoa_ChiTiet_Update4";
        internal protected const string spHangHoaChiTietUpdate5 = "sp_HangHoa_ChiTiet_Update5";
        internal protected const string spHangHoaChiTietUpdate6 = "sp_HangHoa_ChiTiet_Update6";
        internal protected const string spHangHoaChiTietGetByMaVach1 = "sp_HangHoa_ChiTiet_GetByMV1";
        internal protected const string spNgayBaoHanhGetByMaVach = "sp_GetNgayBaoHanhByMaVach";
        internal protected const string spHangHoaChiTietIsUsedForAnother = "sp_HangHoa_ChiTiet_MVDaSuDung";
        protected internal const string spHangHoaChiTietIsUniqueSerial = "sp_HangHoa_ChiTiet_MVUnique";
        protected internal const string spHangHoaChiTietIsNotInUniqueSerial = "sp_HHCT_MVNotInIUnique";
        protected internal const string spHangHoaChiTietDaDungChoGiaoDichKhac = "sp_HangHoa_ChiTiet_IdDaSuDung";
        protected internal const string spHangHoaChiTietSetTuoiTon = "sp_HangHoa_ChiTiet_SetTuoiTon1";
        internal protected const string spXuatKho_SelectbyMaVach = "sp_XuatKho_SelectbyMaVach";

        #endregion

        #region tbl_SanPham
        internal protected const string spSanPhamGetById = "sp_SanPham_GetById";
        internal protected const string spSanPhamGetByIds = "sp_SanPham_GetByIds";
        internal protected const string spSanPhamGetByCodes = "sp_SanPham_GetByCodes";
        #endregion

        #region tbl_Services
        internal protected const string spServiceGetInfoByName = "sp_Services_GetByName";
        internal protected const string spServiceGetListInfo = "sp_Services_GetList";
        internal protected const string spServiceInsert = "sp_Services_Insert";
        internal protected const string spServiceUpdateStatus = "sp_Services_Status_Update";
        #endregion


        protected string CRUDTableName;
        protected bool UseCaching;
        protected string TrackField;

        private GtidCommand sqlCmd;
        private string macAddresses;
        private CacheManager catchManager;
        protected bool OutputTrace;

        private static List<ManagedObject> managedObjects;

        internal class ManagedObject
        {
            public int Id { get; set; }
            public GtidCommand Command { get; set; }
        }

        //private GtidCommand CurrentCommand;
        internal protected static GtidCommand CurrentCommand
        {
            get
            {
                if (managedObjects != null)

                    return managedObjects.Find(

                        delegate(ManagedObject match)
                            {
                                return match.Id == Thread.CurrentThread.ManagedThreadId;
                            }).Command;

                return null;
            }
            set
            {
                if (managedObjects == null) managedObjects = new List<ManagedObject>();
                
                ManagedObject managedObject = managedObjects.Find(
                
                    delegate(ManagedObject match)
                        {
                            return match.Id == Thread.CurrentThread.ManagedThreadId;
                        });
                
                if (managedObject== null)
                    
                    managedObjects.Add(new ManagedObject {Command = value, Id = Thread.CurrentThread.ManagedThreadId});
                
                else
                
                    managedObject.Command = value;
            }
        }

        private CacheManager CacheIsolatedStorage
        {
            get
            {
                try
                {
                    if (catchManager == null)
                    {
                        catchManager = CacheFactory.GetCacheManager();
                    }
                }
                catch (System.Runtime.Serialization.SerializationException)
                {
                    IsolatedStorageFile.Remove(IsolatedStorageScope.User);
                    catchManager = CacheFactory.GetCacheManager();
                }
                catch (System.Reflection.TargetInvocationException)
                {
                    IsolatedStorageFile.Remove(IsolatedStorageScope.User);
                    catchManager = CacheFactory.GetCacheManager();
                }
                catch (System.NullReferenceException)
                {
                    IsolatedStorageFile.Remove(IsolatedStorageScope.User);
                    catchManager = CacheFactory.GetCacheManager();
                }
                catch (System.Exception ex)
                {
                    //EventLogProvider.Instance.WriteLog(ex.ToString(), "Initialize Cache");
                    throw new ManagedException(ex.Message, false);
                }
                return catchManager;
            }
        }

        [Obsolete("Ham nay se khong tiep tuc su dung nua. Hay dung ham CreateCommand de thay the.")]
        internal protected void CreateGetListCommand(string commandText)
        {
            try
            {
                CreateCommonCommand(commandText);
                OracleParameter oracleParameter = new OracleParameter("@Cursor", OracleDbType.RefCursor);
                oracleParameter.Direction = ParameterDirection.Output;
                CurrentCommand.Parameters.Add(oracleParameter);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }
         
        [Obsolete("Ham nay se khong tiep tuc su dung nua. Hay dung ham CreateCommand de thay the.")]
        internal protected void CreateCommonCommand(string commandText)
        {
            try
            {
                CurrentCommand = ConnectionUtil.Instance.GetConnection().CreateCommand();
                CurrentCommand.CommandText = commandText;
                CurrentCommand.CommandType = CommandType.StoredProcedure;
                //GtidCommandBuilder.DeriveParameters(CurrentCommand);
                return;

                //if (CurrentCommand == null)
                //    CurrentCommand = ConnectionUtil.Instance.GetConnection().CreateCommand();
                //else
                //    CurrentCommand.Parameters.Clear();
                //CurrentCommand.CommandText = commandText;
                //CurrentCommand.CommandType = CommandType.StoredProcedure;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }

        internal protected void CreateCommand(string commandText)
        {
            try
            {
                CreateCommand(commandText, null);

                //if (CurrentCommand == null)
                //{
                //    CurrentCommand = ConnectionUtil.Instance.GetConnection().CreateCommand();
                //    CurrentCommand.CommandText = commandText;
                //    CurrentCommand.CommandType = CommandType.StoredProcedure;
                //    GtidCommandBuilder.DeriveParameters(CurrentCommand);
                //    return;
                //}
                //if (CurrentCommand.Connection.State != ConnectionState.Open)
                //{
                //    CurrentCommand.Connection.Open();
                //}
                //CurrentCommand.Parameters.Clear();
                //CurrentCommand.CommandText = commandText;
                //CurrentCommand.CommandType = CommandType.StoredProcedure;
                //GtidCommandBuilder.DeriveParameters(CurrentCommand);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        internal protected virtual void CreateCommand(string commandText, params object[] paramValues)
        {
            try
            {
                CurrentCommand = ConnectionUtil.CurrentManagedObject == null || 
                    ConnectionUtil.CurrentManagedObject.Connection == null ||
                    !(ConnectionUtil.CurrentManagedObject.Connection is GtidOracleProdConnection)

                                     ? ConnectionUtil.Instance.GetConnection().CreateCommand()
                                     : ConnectionUtil.CurrentManagedObject.Connection.CreateCommand();

                CurrentCommand.OutputTrace = false;

                CurrentCommand.CommandText = commandText;

                GtidCommandBuilder.Instance.DeriveParameters(CurrentCommand, paramValues);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        protected GtidParameterCollection Parameters
        {
            get { return CurrentCommand.Parameters; }
        }

        protected int ExecuteCommand(string commandText)
        {
            try
            {
                CreateCommand(commandText);
                return ExecuteNoneQuery();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }

        protected int ExecuteCommand(string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                return ExecuteNoneQuery();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        public int ExecInsertCommand(string commandText, params object[] paramValues)
        {
            try
            {
                //set creation log
                paramValues = SetLogParams(paramValues);

                //set update log
                paramValues = SetLogParams(paramValues);

                return ExecuteCommand(commandText, paramValues);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        public int ExecUpdateCommand(string commandText, params object[] paramValues)
        {
            try
            {
                //set update log
                paramValues = SetLogParams(paramValues);

                return ExecuteCommand(commandText, paramValues);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        protected virtual object[] SetLogParams(params object[] paramValues)
        {
            return paramValues;
        }

        [Obsolete("Ham nay se khong tiep tuc su dung nua. Hay dung ham ExecuteCommand de thay the.")]
        protected int ExecuteNoneQuery()
        {
            try
            {
                int resultAffected = CurrentCommand.ExecuteNonQuery();
                if (UseCaching && !String.IsNullOrEmpty(CRUDTableName) &&
                    (CurrentCommand.CommandText.ToLower().EndsWith("insert") ||
                    CurrentCommand.CommandText.ToLower().EndsWith("update") ||
                    CurrentCommand.CommandText.ToLower().EndsWith("delete")))
                    ClearCg(CRUDTableName.GetHashCode().ToString());

                return resultAffected;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }
        }

        protected object ExecuteScalar(string commandText)
        {
            try
            {
                CreateCommand(commandText);
                return CurrentCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }

        protected object ExecuteScalar(string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                return CurrentCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }


        protected DataSet GetDataSetCommand(string srcTable, string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                GtidDataAdapter da = new GtidDataAdapter(CurrentCommand);
                DataSet dsResult = new DataSet();
                da.Fill(dsResult, srcTable);

                da.Dispose();

                //da = null;

                //GC.Collect();

                //if (!String.IsNullOrEmpty(Convert.ToString(da)))
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(da));

                return dsResult;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, srcTable, commandText, paramValues);
            }
        }

        /// <returns>Trả về dataset với tên bảng mặc định</returns>
        protected DataSet GetDataSetCommand(string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                GtidDataAdapter da = new GtidDataAdapter(CurrentCommand);
                DataSet dsResult = new DataSet();
                da.Fill(dsResult);

                da.Dispose();

                //da = null;

                //GC.Collect();

                //if (!String.IsNullOrEmpty(Convert.ToString(da)))
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(da));
                return dsResult;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        protected DataSet GetDataSetCommand(string srcTable, string commandText)
        {
            try
            {
                CreateCommand(commandText);
                GtidDataAdapter da = new GtidDataAdapter(CurrentCommand);
                DataSet dsResult = new DataSet();
                da.Fill(dsResult, srcTable);

                da.Dispose();

                //da = null;

                //GC.Collect();

                //if (!String.IsNullOrEmpty(Convert.ToString(da)))
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(da));
                return dsResult;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, srcTable, commandText);
            }
        }

        /// <returns>Trả về dataset với tên bảng mặc định</returns>
        protected DataSet GetDataSetCommand(string commandText)
        {
            try
            {
                CreateCommand(commandText);
                GtidDataAdapter da = new GtidDataAdapter(CurrentCommand);
                DataSet dsResult = new DataSet();
                da.Fill(dsResult);

                da.Dispose();

                //da = null;

                //GC.Collect();

                //if (!String.IsNullOrEmpty(Convert.ToString(da)))
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(da));

                return dsResult;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }

        /// <returns>Trả về dataset với tên bảng mặc định</returns>
        protected DataTable GetDataTableCommand(string commandText)
        {
            try
            {
                CreateCommand(commandText);
                GtidDataAdapter da = new GtidDataAdapter(CurrentCommand);
                DataTable dtResult = new DataTable();
                da.Fill(dtResult);

                da.Dispose();

                //da = null;

                //GC.Collect();

                //if (!String.IsNullOrEmpty(Convert.ToString(da)))
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(da));
                return dtResult;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }

        /// <returns>Trả về dataset với tên bảng mặc định</returns>
        protected DataTable GetDataTableCommand(string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                GtidDataAdapter da = new GtidDataAdapter(CurrentCommand);
                DataTable dtResult = new DataTable();
                da.Fill(dtResult);

                da.Dispose();

                //da = null;

                //GC.Collect();

                //if (!String.IsNullOrEmpty(Convert.ToString(da)))
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(da));

                return dtResult;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        /// <returns>Trả về dataset với tên bảng mặc định</returns>
        protected void FillToDataTable(DataTable dataTable, string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);

                var da = new GtidDataAdapter(CurrentCommand);

                if (dataTable == null)

                    dataTable = new DataTable();

                da.Fill(dataTable);

                da.Dispose();

                //da = null;

                //GC.Collect();

                //if (!String.IsNullOrEmpty(Convert.ToString(da)))
                //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
                //                       Convert.ToString(da));
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, dataTable, commandText, paramValues);
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ParseToCsv<T>(ref string tblName, List<T> lstObject)
        {
            CBO.Instance.FillToCsv(lstObject, String.IsNullOrEmpty(tblName), ref tblName);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void GetCsvCommand(ref string tblName, string commandText)
        {
            try
            {
                CreateCommand(commandText);
                FillToCsv(ref tblName);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }            
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected void GetCsvCommand(ref string tblName, string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                FillToCsv(ref tblName);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected List<T> GetListCommand<T>(string commandText)
        {
            try
            {
                CreateCommand(commandText);
                return FillToList<T>();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected List<T> GetListCommand<T>(string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                return FillToList<T>();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        protected List<T> GetListCommandTest<T>(string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                return FillToListTest<T>();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        protected List<T> GetListCommandWithTimeout<T>(string commandText, int commandTimeout, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                CurrentCommand.CommandTimeout = commandTimeout;
                return FillToList<T>();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, commandTimeout, paramValues);
            }
        }

        [Obsolete("Ham nay se khong tiep tuc su dung nua. Hay dung ham GetListCommand de thay the.")]
        protected List<T> FillToListTest<T>()
        {
            DateTime start = DateTime.Now;
            
            IDataReader dataReader = CurrentCommand.ExecuteReader();

            DateTime end1  = DateTime.Now;

            Debug.Print(String.Format("Elapsed: {0} miliseconds.", (end1 - start).TotalMilliseconds));

            List<T> result = CBO.Instance.FillCollection<T>(dataReader);

            DateTime end2 = DateTime.Now;

            Debug.Print(String.Format("Elapsed: {0} miliseconds.", (end2 - start).TotalMilliseconds));

            if (!dataReader.IsClosed) dataReader.Close();

            dataReader.Dispose();

            return result;
        }
        protected void FillToCsv(ref string tableName)
        {
            try
            {
                IDataReader dataReader = CurrentCommand.ExecuteReader();

                CBO.Instance.FillToCsv(dataReader, String.IsNullOrEmpty(tableName), ref tableName);

                if (!dataReader.IsClosed) dataReader.Close();

                dataReader.Dispose();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }
        }

        [Obsolete("Ham nay se khong tiep tuc su dung nua. Hay dung ham GetListCommand de thay the.")]
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected List<T> FillToList<T>()
        {
            try
            {
                IDataReader dataReader = CurrentCommand.ExecuteReader();

                List<T> result = CBO.Instance.FillCollection<T>(dataReader);

                if (!dataReader.IsClosed) dataReader.Close();

                dataReader.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }
        }

        protected T GetObjectCommand<T>(string commandText)
        {
            try
            {
                CreateCommand(commandText);
                return FillToObject<T>();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }

        protected T GetObjectCommand<T>(string commandText, params object[] paramValues)
        {
            try
            {
                CreateCommand(commandText, paramValues);
                return FillToObject<T>();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }

        [Obsolete("Ham nay se khong tiep tuc su dung nua. Hay dung ham GetObjectCommand de thay the.")]
        protected T FillToObject<T>()
        {
            try
            {
                if (typeof(T).FullName.StartsWith("System."))
                {
                    var tmpResult = CurrentCommand.ExecuteScalar();

                    if (tmpResult == null || Convert.IsDBNull(tmpResult)) return default(T);

                    return (T)Convert.ChangeType(tmpResult, typeof(T));
                }

                IDataReader dataReader = CurrentCommand.ExecuteReader();

                var result = CBO.Instance.FillObject<T>(dataReader);

                if (!dataReader.IsClosed) dataReader.Close();

                dataReader.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }
        }
        /// <summary>
        /// Khi dùng phải đảm bảo thứ tự các thuộc tính của info phải đúng thứ tự các tham số của SP.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        protected static object[] ParseToParams<T>(T info)
        {
            PropertyInfo[] propertyInfos = CBO.Instance.GetPropertyInfo(typeof(T));
            object[] result = new object[propertyInfos.Length];
            for (int i = 0; i < propertyInfos.Length; i++ )
            {
                result[i] = propertyInfos[i].GetValue(info, null);
            }
            //propertyInfos = null;

            //GC.Collect();

            //if (!String.IsNullOrEmpty(Convert.ToString(propertyInfos)))
            //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
            //                       Convert.ToString(propertyInfos));

            return result;
        }

        [Obsolete("Ham nay se khong tiep tuc su dung nua. Hay dung ham ParseToParams de thay the.")]
        protected void SetParams<T>(T info)
        {
            PropertyInfo[] propertyInfos = CBO.Instance.GetPropertyInfo(typeof(T));
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                CurrentCommand.Parameters.AddWithValue(String.Format("@{0}", propertyInfo.Name), propertyInfo.GetValue(info, null));
            }

            //propertyInfos = null;

            //GC.Collect();

            //if (!String.IsNullOrEmpty(Convert.ToString(propertyInfos)))
            //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
            //                       Convert.ToString(propertyInfos));

        }

        private object threadCache;
        private string[] referTables;
        private bool? isRefreshedCache;
        private string keyCached;
        private string spCachedName;

        private void GetCache<T>() where T : class
        {
            List<T> result = null;

            if(!String.IsNullOrEmpty(TrackField) &&
                
                spCachedName.ToLower().Trim().StartsWith("select "))
            {
                var lastRefreshed = LastRefreshed(keyCached, macAddresses);

                if (lastRefreshed > DateTime.MinValue)
                {
                    result = CacheIsolatedStorage[keyCached] as List<T>;

                    var tmp =

                        GetListCommand<T>(

                            spCachedName +

                            (spCachedName.ToLower().Contains(" where ") ? " and " : " where ") +

                            String.Format("{0} >= to_date(:lastRereshed, 'dd/mm/rrrr')", TrackField),

                            lastRefreshed.ToString("dd/MM/yyyy"));

                    foreach (var objData in tmp)
                    {
                        if (result != null)
                        {
                            var obj2 = result.Find(delegate(T match) { return match.Equals(objData); });

                            if (obj2 != null)

                                result[result.IndexOf(obj2)] = objData;

                            else

                                result.Add(objData);
                        }
                    }
                }
                else
                {
                    result = GetListCommand<T>(spCachedName);
                }
            }
            else
            {
                result = GetListCommand<T>(spCachedName);
            }

            CacheIsolatedStorage.Add(keyCached, result);

            //result = null;

            //GC.Collect();

            //if (!String.IsNullOrEmpty(Convert.ToString(result)))
            //    File.AppendAllText(AppDomain.CurrentDomain.BaseDirectory + String.Format("\\{0}.log", Path.GetRandomFileName()),
            //                       Convert.ToString(result));

            for (int i = 0; i < referTables.Length; i++)
            {
                referTables[i] = referTables[i].GetHashCode().ToString();
            }

            SaveCg(keyCached, macAddresses, String.Format(",{0},", String.Join(",", referTables)), isRefreshedCache != null);
        }

        protected List<T> GetListAll<T>(string commandText, params string[] referTables) where T : class
        {
            try
            {
                if (UseCaching && referTables.Length > 0)
                {
                    spCachedName = commandText;

                    keyCached =
                        (ConnectionUtil.Instance.GetConnectionString() + ":" + spCachedName + ":" + typeof(T).FullName).
                            GetHashCode().ToString();

                    if (String.IsNullOrEmpty(macAddresses))
                        macAddresses = GetMacAddress();

                    isRefreshedCache = IsRefreshed(keyCached, macAddresses);

                    if (isRefreshedCache == true && CacheIsolatedStorage.Contains(keyCached))
                    {
                        return CacheIsolatedStorage[keyCached] as List<T>;
                    }

                    this.referTables = referTables;

                    if (CacheIsolatedStorage.Contains(keyCached))
                    {
                        var genericThread = (GenericThread<T>)threadCache;

                        if (genericThread == null || genericThread.ThreadState == ThreadState.Stopped ||
                            genericThread.ThreadState == ThreadState.Unstarted ||
                            genericThread.ThreadState == ThreadState.Aborted)
                        {

                            threadCache = new GenericThread<T>(GetCache<T>);

                            genericThread = (GenericThread<T>)threadCache;

                            genericThread.Start();
                        }

                        return CacheIsolatedStorage[keyCached] as List<T>;
                    }

                    var result = GetListAll<T>(spCachedName);

                    CacheIsolatedStorage.Add(keyCached, result);

                    for (var i = 0; i < referTables.Length; i++)
                    {
                        referTables[i] = referTables[i].GetHashCode().ToString();
                    }

                    SaveCg(keyCached, macAddresses, String.Format(",{0},", String.Join(",", referTables)), isRefreshedCache != null);

                    return result;
                }

                return GetListAll<T>(commandText);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, referTables);
            }
        }

        protected List<T> GetListAll<T>(string commandText)
        {
            try
            {
                return GetListCommand<T>(commandText);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText);
            }
        }

        private void SaveCg(string key, string mac, string dep, bool existed)
        {
            SqlHelper.ExecuteNonQuery(ConnectionUtil.Instance.GetConnection(), CommandType.Text, String.Format(!existed
                    ? "INSERT INTO tbl_cg(cdep,cname,identifier,cref,ccre) VALUES('{0}','{1}','{2}',1,SYSDATE)"
                    : "UPDATE tbl_cg SET cdep='{0}',cref=1,ccre=SYSDATE WHERE cname='{1}' AND identifier='{2}'", dep, key, mac));
        }

        private void ClearCg(string dep)
        {
            SqlHelper.ExecuteNonQuery(ConnectionUtil.Instance.GetConnection(),CommandType.Text, 
                
                String.Format("UPDATE tbl_cg SET cref=0 WHERE cdep LIKE '{0}'", String.Format("%,{0},%", dep)));
        }

        private bool? IsRefreshed(string key, string mac)
        {
            var result = SqlHelper.ExecuteScalar(ConnectionUtil.Instance.GetConnection(), CommandType.Text,
                
                String.Format("SELECT cref FROM tbl_cg WHERE cname='{0}' AND identifier='{1}'", key, mac));

            if (result != null) return Convert.ToBoolean(result);

            return null;
        }

        private DateTime LastRefreshed(string key, string mac)
        {
            var result = SqlHelper.ExecuteScalar(ConnectionUtil.Instance.GetConnection(), CommandType.Text,
                
                String.Format("SELECT ccre FROM tbl_cg WHERE cname='{0}' AND identifier='{1}'", key, mac));

            if (result != null) return (DateTime) result;

            return DateTime.MinValue;
        }

        private string GetMacAddress()
        {
            if(String.IsNullOrEmpty(macAddresses))
            {
                macAddresses = String.Empty;
                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus == OperationalStatus.Up)
                    {
                        macAddresses += nic.GetPhysicalAddress().ToString();
                        break;
                    }
                }
                if (String.IsNullOrEmpty(macAddresses))
                {
                    string hostName = Dns.GetHostName();
                    string ipAddress = Dns.GetHostEntry(hostName).AddressList[0].ToString();
                    macAddresses = hostName + "(" + ipAddress + ")"; 
                    
                }
                return macAddresses;
            }
            return macAddresses;
        }

    }

    internal class StreamList<T>
    {
        MemoryStream mStream = new MemoryStream();

        public void Test()
        {
            var t = new List<T>();

            byte[] buffer = ObjectToByteArray(t);

            mStream.BeginWrite(
                buffer, 0, buffer.Length,
                TestASync, null);

        }

        void TestASync(IAsyncResult asyncResult)
        {
            mStream.EndWrite(asyncResult);
        }

        private static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
                return null;
            var bf = new BinaryFormatter();
            var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);
            return obj;
        }
    }

    public abstract class Base2Dao : BaseDAO
    {
        protected internal override void CreateCommand(string commandText, params object[] paramValues)
        {
            try
            {
                CurrentCommand = ConnectionUtil.CurrentManagedObject == null || 
                    ConnectionUtil.CurrentManagedObject.Connection == null ||
                    !(ConnectionUtil.CurrentManagedObject.Connection is GtidOleDbConnection)
                                     ? ConnectionUtil.Instance.GetConnection2().CreateCommand()
                                     : ConnectionUtil.CurrentManagedObject.Connection.CreateCommand();

                CurrentCommand.OutputTrace = false;

                CurrentCommand.CommandText = commandText;

                GtidCommandBuilder.Instance.DeriveParameters(CurrentCommand, paramValues);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }
    }

    public abstract class Base3Dao : BaseDAO
    {
        protected internal override void CreateCommand(string commandText, params object[] paramValues)
        {
            try
            {
                if(ConnectionUtil.Instance.IsUAT > 1)
                {
                    base.CreateCommand(commandText, paramValues);
                
                    return;
                }

                CurrentCommand = ConnectionUtil.CurrentManagedObject == null ||
                    ConnectionUtil.CurrentManagedObject.Connection == null ||
                    !(ConnectionUtil.CurrentManagedObject.Connection is GtidOracleStByConnection)
                                     ? ConnectionUtil.Instance.GetConnection3().CreateCommand()
                                     : ConnectionUtil.CurrentManagedObject.Connection.CreateCommand();

                CurrentCommand.OutputTrace = false;

                CurrentCommand.CommandText = commandText;

                GtidCommandBuilder.Instance.DeriveParameters(CurrentCommand, paramValues);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, commandText, paramValues);
            }
        }
    }
}
