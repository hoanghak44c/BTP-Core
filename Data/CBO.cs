using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Oracle.DataAccess.Client;
using QLBH.Core.Form;
using QLBH.Core.Providers;

namespace QLBH.Core.Data
{
    internal class CBO
    {
        //private static CacheManager dataCahe; //= CacheFactory.GetCacheManager();
        //private static CBO instance;

        private class ManagedCBO
        {
            public ManagedCBO()
            {
                Id = Thread.CurrentThread.ManagedThreadId;
            }

            public CBO CboInstance;
            public CacheManager DataCahe;
            public int Id { get; private set; }
        }

        private static List<ManagedCBO> lstManagedCbo;

        public CBO()
        {
//            if (dataCahe == null)
//            {
//                dataCahe = CacheFactory.GetCacheManager("Memory Cache Manager");
//#if DEBUG
//                dataCahe.Flush();
//#endif
//            }
        }

        internal static CBO Instance
        {
            get {
                
                if(lstManagedCbo == null) lstManagedCbo = new List<ManagedCBO>();
                
                if (!lstManagedCbo.Exists(
                    delegate(ManagedCBO match)
                        {
                            return match.Id == Thread.CurrentThread.ManagedThreadId;
                        }))
                {
                    lstManagedCbo.Add(
                        new ManagedCBO
                            {
                                CboInstance = new CBO(),
                                DataCahe = CacheFactory.GetCacheManager("Memory Cache Manager")
                            });
                }

                return lstManagedCbo.Find(
                    delegate(ManagedCBO match)
                        {
                            return match.Id == Thread.CurrentThread.ManagedThreadId;
                        }).CboInstance;

                //if (instance == null) instance = new CBO();
                //return instance;
            }
        }

        private CacheManager DataCahe
        {
            get
            {
                return lstManagedCbo.Find(
                    delegate(ManagedCBO match)
                    {
                        return match.Id == Thread.CurrentThread.ManagedThreadId;
                    }).DataCahe;                
            }
        }

        //private static Hashtable DataCahe = new Hashtable();//Cache các thuộc tính

        /// <summary>
        /// Trả về các thuộc tính của 1 object có kiểu Type sử dụng cache
        /// </summary>
        /// <param name="objType"></param>
        /// <returns></returns>
        public PropertyInfo[] GetPropertyInfo(Type objType)
        {
            PropertyInfo[] objProperties;

            //hah:xoa cac du lieu cu, chay mot thoi gian roi thoi.
            if (DataCahe.Contains(objType.Name)) DataCahe.Remove(objType.Name);

            if (DataCahe.Contains(objType.FullName))
            {
                //nếu có thì lấy từ cache
                objProperties = new PropertyInfo[((PropertyInfo[])DataCahe[objType.FullName]).Length];
                
                ((PropertyInfo[])DataCahe[objType.FullName]).CopyTo(objProperties, 0);
            }
            else
            {
                //nếu không thì lấy từ Type và lưu vào cache
                objProperties = objType.GetProperties();

                DataCahe.Add(objType.FullName, objProperties);
            }

            return objProperties;
        }

        /// <summary>
        /// Kiểm tra 1 trường có tồn tại trong IDateReader
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static bool ColumnExists(IDataReader dr, string columnName)
        {
            dr.GetSchemaTable().DefaultView.RowFilter = "ColumnName= '" + columnName + "'";
            return (dr.GetSchemaTable().DefaultView.Count > 0);
        }
        /// <summary>
        /// Lấy về danh sách các column của Reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static List<string> GetColumnList(IDataReader dr)
        {
            List<string> lstColumns = new List<string>();
            System.Data.DataTable drSchema = dr.GetSchemaTable();
            for (int i = 0; i < drSchema.Rows.Count; i++)
                lstColumns.Add(drSchema.Rows[i]["ColumnName"].ToString().ToUpper());
            return lstColumns;
        }

        /// <summary>
        /// Lấy về danh sách các column của Reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        internal static List<string> GetColumnList(IDataReader dr, string cacheName)
        {
            List<string> lstColumns;
            //nếu không thì lấy từ Type và lưu vào cache
            lstColumns = new List<string>();
            System.Data.DataTable drSchema = dr.GetSchemaTable();
            for (int i = 0; i < drSchema.Rows.Count; i++)
                lstColumns.Add(drSchema.Rows[i]["ColumnName"].ToString().ToUpper());

            //if (dataCahe.Contains(cacheName))
            //{
            //    //nếu có thì lấy từ cache
            //    lstColumns = (List<string>)dataCahe[cacheName];
            //}
            //else
            //{
            //    //nếu không thì lấy từ Type và lưu vào cache
            //    lstColumns = new List<string>();
            //    System.Data.DataTable drSchema = dr.GetSchemaTable();
            //    for (int i = 0; i < drSchema.Rows.Count; i++)
            //        lstColumns.Add(drSchema.Rows[i]["ColumnName"].ToString().ToUpper());
            //    dataCahe.Add(cacheName, lstColumns);
            //}
            return lstColumns;
        }

        /// <summary>
        /// Tao object co kieu la Type va co du lieu lay tu DataReader
        /// </summary>
        /// <param name="objType">Kieu cua object</param>
        /// <param name="dr">Chua du lieu cho object</param>
        /// <returns></returns>
        private T GetObject<T>(IDataReader dr)
        {
            //Tạo Object
            T objObject = Activator.CreateInstance<T>();

            //Lấy về các thuộc tính
            PropertyInfo[] objProperties = GetPropertyInfo(typeof(T));
            //List<string> lstColumns = GetColumnList(dr);
            List<string> lstColumns = GetColumnList(dr, typeof(T).Name + "Columns");

            //Thiết lập giá trị cho Object
            for (int i = 0; i < objProperties.Length; i++)
            {
                try
                {
                    //Nếu tồn tại columnName và trong reader có dữ liệu => gọi hàm setProperty
                    if (lstColumns.Contains(objProperties[i].Name.ToUpper()) && dr[objProperties[i].Name] != DBNull.Value)
                    {
                        //objProperties[i].SetValue(objObject, Convert.ChangeType(dr[objProperties[i].Name], objProperties[i].PropertyType), null);
                        typeof(T).InvokeMember(objProperties[i].Name, BindingFlags.SetProperty, null, objObject, new Object[] { Convert.ChangeType(dr[objProperties[i].Name], objProperties[i].PropertyType) });
                    }

                }
                catch (Exception ex)
                {
                    Debug.Print(objProperties[i].Name);
                    Debug.Print(ex.ToString());
                }
            }
            return objObject;
        }

        /// <summary>
        /// Tao object co kieu la Type va co du lieu lay tu DataReader
        /// </summary>
        /// <param name="objType">Kieu cua object</param>
        /// <param name="dr">Chua du lieu cho object</param>
        /// <returns></returns>
        private Object GetObject(Type objType, IDataReader dr)
        {
            //Tạo Object
            Object objObject = Activator.CreateInstance(objType);

            //Lấy về các thuộc tính
            PropertyInfo[] objProperties = GetPropertyInfo(objType);
            List<string> lstColumns = GetColumnList(dr);

            //Thiết lập giá trị cho Object
            for (int i = 0; i < objProperties.Length; i++)
            {
                //Nếu tồn tại columnName và trong reader có dữ liệu => gọi hàm setProperty
                if (lstColumns.Contains(objProperties[i].Name.ToUpper()) && dr[objProperties[i].Name] != DBNull.Value)
                {
                    objType.InvokeMember(objProperties[i].Name, BindingFlags.SetProperty, null, objObject, new Object[] { dr[objProperties[i].Name] });
                }
            }
            return objObject;
        }

        private T GetObject<T>(IDataReader dr, PropertyInfo[] props, List<string> lstColumns)
        {

            if (typeof(T).FullName.StartsWith("System."))
            {
                return (T)Convert.ChangeType(dr[0], typeof(T));
            }

            //Tạo Object
            T objObject = Activator.CreateInstance<T>();

            //Thiết lập giá trị cho Object
            for (int i = 0; i < props.Length; i++)
            {
                    try
                    {
                        //Nếu tồn tại columnName và trong reader có dữ liệu => gọi hàm setProperty
                        if (lstColumns.Contains(props[i].Name.ToUpper()) && dr[props[i].Name] != DBNull.Value)
                        {
                                //cac du lieu binary khong dung duoc voi setvalue
                                //props[i].SetValue(objObject, Convert.ChangeType(dr[props[i].Name], props[i].PropertyType), null);
                                typeof (T).InvokeMember(props[i].Name, BindingFlags.SetProperty, null, objObject,
                                                        new Object[]
                                                            {Convert.ChangeType(dr[props[i].Name], props[i].PropertyType)});
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print("Loi : " + ex.ToString());
                        throw;
                    }
            }

            return objObject;
        }

        private static Object GetObject(Type objType, IDataReader dr, PropertyInfo[] props, List<string> lstColumns)
        {
            //Tạo Object
            Object objObject = Activator.CreateInstance(objType);

            //Thiết lập giá trị cho Object
            for (int i = 0; i < props.Length; i++)
            {
                //Nếu tồn tại columnName và trong reader có dữ liệu => gọi hàm setProperty
                if (lstColumns.Contains(props[i].Name.ToUpper()) && dr[props[i].Name] != DBNull.Value)
                {
                    objType.InvokeMember(props[i].Name, BindingFlags.SetProperty, null, objObject, new Object[] { dr[props[i].Name] });
                }
            }
            return objObject;
        }
        /// <summary>
        /// Tao object co kieu la Type va co du lieu lay tu DataReader
        /// </summary>
        /// <param name="objType">Kieu cua object</param>
        /// <param name="dr">Chua du lieu cho object</param>
        /// <returns></returns>
        private Object CreateObject(Type objType, IDataReader dr)
        {
            //Tao doi tuong
            Object objObject = Activator.CreateInstance(objType);

            // Thiet lap gia tri cho cac thuoc tinh cua Object tu cac truong DR
            //PropertyInfo[] objProperties = objType.GetProperties();
            PropertyInfo[] objProperties = GetPropertyInfo(objType);
            for (int i = 0; i <= objProperties.Length - 1; i++)
            {
                //Lay ve Property can thiet lap
                PropertyInfo objPropertyInfo = objProperties[i];

                //Kieu cua Property
                Type objPropertyType = objPropertyInfo.PropertyType;

                //Thiet lap gia tri neu thuoc tinh co ham set
                if (objPropertyInfo.CanWrite)
                {
                    int indx = -1;
                    //Lay vi tri cua thuoc tinh trong DR
                    string propertyName = objPropertyInfo.Name;//.ToUpperInvariant();
                    try
                    {
                        indx = dr.GetOrdinal(propertyName);
                    }
                    catch { }
                    //Neu thuoc tinh co ten nam trong cac truong cua DR thi chuyen doi, nguoc lai thi thoi
                    if (indx != -1)
                    {
                        //Lay gia tri tuong ung cua truong trong DR
                        Object objDataValue = dr.GetValue(indx);

                        if (objDataValue == DBNull.Value)
                        {
                            //gia tri la null => thiet lap null cho Property cua Object
                            objPropertyInfo.SetValue(objObject, null, null);
                        }
                        else if (objPropertyType.Equals(objDataValue.GetType()))
                        {
                            //Cung kieu du lieu => thiet lap gia tri cho Property cua Object
                            if (objPropertyType.IsArray)
                            {
                                int buf = 10000, startInd = 0;
                                long num;
                                byte[] bytes = new byte[buf];
                                while (true)
                                {
                                    num = dr.GetBytes(indx, startInd, bytes, 0, buf);
                                    startInd += (int)num;
                                    if (num < buf) break;
                                }
                                byte[] data = new byte[startInd];
                                dr.GetBytes(indx, 0, data, 0, startInd);
                                objPropertyInfo.SetValue(objObject, data, null);
                            }
                            else
                                objPropertyInfo.SetValue(objObject, objDataValue, null);
                        }
                        else
                        {
                            //Thuoc tinh cua Property khong trung voi thuoc tinh cua cac truong trong DR
                            try
                            {
                                //Chuyen doi kieu enum
                                if (objPropertyType.BaseType.Equals(typeof(System.Enum)))
                                {
                                    // check if value is numeric and if not convert to integer ( supports databases like Oracle )
                                    try
                                    {
                                        objPropertyInfo.SetValue(objObject, System.Enum.ToObject(objPropertyType, Convert.ToInt32(objDataValue)), null);
                                    }
                                    catch
                                    {
                                        objPropertyInfo.SetValue(objObject, System.Enum.ToObject(objPropertyType, objDataValue), null);
                                    }
                                }
                                else if (objPropertyType.FullName.Equals("System.Guid"))//Kieu guid
                                {
                                    objPropertyInfo.SetValue(objObject, Convert.ChangeType(new Guid(objDataValue.ToString()), objPropertyType), null);
                                }
                                else//cac kieu khac
                                {
                                    objPropertyInfo.SetValue(objObject, Convert.ChangeType(objDataValue, objPropertyType), null);
                                }
                            }
                            catch
                            {
                                objPropertyInfo.SetValue(objObject, Convert.ChangeType(objDataValue, objPropertyType), null);
                            }
                        }
                    }
                }
            }

            return objObject;
        }

        /// <summary>
        /// Tra ve ArrayList cac object co kieu la Type va du lieu lay tu DataReader
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        internal List<T> FillCollection<T>(IDataReader dr)
        {
            List<T> objFillCollection = new List<T>();

            try
            {
                ConnectionUtil.CurrentManagedObject.IsRunningQuery = true;
                //Lap qua tap du lieu trong datareader
                if (!dr.IsClosed)
                {
                    //Lấy về các thuộc tính
                    PropertyInfo[] objProperties = GetPropertyInfo(typeof(T));
                    //List<string> lstColumns = GetColumnList(dr);
                    List<string> lstColumns = GetColumnList(dr, typeof(T).Name + "Columns");


                    while (dr.Read())
                    {
                        //Tao object
                        //T objFillObject = GetObject<T>(dr, objProperties, lstColumns);
                        T objFillObject = GetObject<T>(dr, objProperties, lstColumns);
                        //Them vao collection
                        objFillCollection.Add(objFillObject);
                    }
                }

                //Dong datareader
                if ((dr != null))
                {
                    dr.Close();
                }
                
                ConnectionUtil.CurrentManagedObject.IsRunningQuery = false;

                return objFillCollection;

            }
            catch (OracleException oracleException)
            {
                //throw;
                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    EventLogProvider.Instance.WriteOfflineLog(oracleException.ToString(), "Data Reader Exception");
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
                            frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);
                            List<T> tmpResult = FillCollection<T>(dr);
                            var tmpArrResult = new T[tmpResult.Count];
                            tmpResult.CopyTo(tmpArrResult);
                            objFillCollection.AddRange(tmpArrResult);
                            ConnectionUtil.CurrentManagedObject.IsRunningQuery = false;                            
                            return objFillCollection;
                    }
                }
                
                ConnectionUtil.CurrentManagedObject.IsRunningQuery = false;

                throw;
            }
        }

        /// <summary>
        /// Tra ve ArrayList cac object co kieu la Type va du lieu lay tu DataReader
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="objType"></param>
        /// <returns></returns>
        public ArrayList FillCollection(IDataReader dr, Type objType)
        {
            ArrayList objFillCollection = new ArrayList();
            object objFillObject = null;

            //Lap qua tap du lieu trong datareader
            if (!dr.IsClosed)
            {
                //Lấy về các thuộc tính
                PropertyInfo[] objProperties = GetPropertyInfo(objType);
                List<string> lstColumns = GetColumnList(dr);

                while (dr.Read())
                {
                    //Tao object
                    objFillObject = GetObject(objType, dr, objProperties, lstColumns);
                    //Them vao collection
                    objFillCollection.Add(objFillObject);
                }
            }

            //Dong datareader
            if ((dr != null))
            {
                dr.Close();
            }

            return objFillCollection;
        }
        /// <summary>
        /// Tra ve Object kieu T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public T FillObject<T>(IDataReader dr)
        {

            T objFillObject = default(T);

            if (dr.Read())
            {
                objFillObject = GetObject<T>(dr);
            }
            else
            {
                objFillObject = default(T);
            }

            if (!dr.IsClosed)
            {
                dr.Close();
            }

            return objFillObject;

        }
        internal void FillToCsv<T>(List<T> lstObj, bool hasHeader, ref string fileName)
        {
            var filePath = String.Empty;
            
            var propertyInfos = GetPropertyInfo(typeof (T));
            
            if(hasHeader)
            {
                fileName = Path.GetRandomFileName();

                filePath = String.Format("{0}\\Data\\{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);

                while (File.Exists(filePath))
                {
                    fileName = Path.GetRandomFileName();

                    filePath = String.Format("{0}\\Data\\{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);
                }
            }
            else filePath = String.Format("{0}\\Data\\{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);

            var sw = File.AppendText(filePath);

            var sb = new StringBuilder();

            if (hasHeader)
            {
                for (var i = 0; i < propertyInfos.Length; i++)
                {
                    sb.AppendFormat("\"{0}\"{1}", propertyInfos[i].Name, i == propertyInfos.Length - 1 ? String.Empty : ",");
                }
                sb.AppendLine();
            }
            foreach (var obj in lstObj)
            {
                for (int i = 0; i < propertyInfos.Length; i++)
                {
                    sb.AppendFormat("\"{0}\"{1}",

                                    propertyInfos[i].GetValue(obj, null) == null
                                        ? String.Empty
                                        : propertyInfos[i].GetValue(obj, null).ToString().Replace("\"", "\"\""),

                                    i == propertyInfos.Length - 1 ? String.Empty : ",");
                }
                sb.AppendLine();
            }
            sw.Write(sb.ToString().TrimEnd("\r\n".ToCharArray()));

            sw.Flush();

            sw.Close();
        }

        internal void FillToCsv(IDataReader dr, bool hasHeader, ref string fileName)
        {
            try
            {
                var filePath = String.Empty;

                ConnectionUtil.CurrentManagedObject.IsRunningQuery = true;

                if (!dr.IsClosed)
                {
                    //Lấy về các thuộc tính
                    List<string> lstColumns = GetColumnList(dr);

                    if(hasHeader)
                    {
                        fileName = Path.GetRandomFileName();

                        filePath = String.Format("{0}\\Data\\{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);
                        
                        while (File.Exists(filePath))
                        {
                            fileName = Path.GetRandomFileName();
                            
                            filePath = String.Format("{0}\\Data\\{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);
                        }
                    } 
                    else filePath = String.Format("{0}\\Data\\{1}", AppDomain.CurrentDomain.BaseDirectory, fileName);

                    var sw = File.AppendText(filePath);

                    var sb = new StringBuilder();

                    if (hasHeader)
                    {
                        for (var i = 0; i < lstColumns.Count; i++)
                        {
                            sb.AppendFormat("\"{0}\"{1}", lstColumns[i], i == lstColumns.Count - 1 ? String.Empty : ",");
                        }
                        sb.AppendLine();
                    }

                    while (dr.Read())
                    {
                        for (var i = 0; i < lstColumns.Count; i++)
                        {
                            sb.AppendFormat("\"{0}\"{1}", dr[i].ToString().Replace("\"", "\"\""),
                                            i == lstColumns.Count - 1 ? String.Empty : ",");
                        }
                        sb.AppendLine();
                    }
                    
                    sw.Write(sb.ToString().TrimEnd("\r\n".ToCharArray()));

                    sw.Flush();

                    sw.Close();
                }

                //Dong datareader
                if ((dr != null))
                {
                    dr.Close();
                }

                ConnectionUtil.CurrentManagedObject.IsRunningQuery = false;
            }
            catch (OracleException oracleException)
            {
                //throw;
                if (!ConnectionUtil.Instance.IsInTransaction)
                {
                    EventLogProvider.Instance.WriteOfflineLog(oracleException.ToString(), "Data Reader Exception");
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
                            frmProgress.Instance.DoWork(ConnectionUtil.Instance.ReTryConnect);
                            FillToCsv(dr, hasHeader, ref fileName);
                            break;
                    }
                }

                ConnectionUtil.CurrentManagedObject.IsRunningQuery = false;

                throw;
            }
        }
    }
}
