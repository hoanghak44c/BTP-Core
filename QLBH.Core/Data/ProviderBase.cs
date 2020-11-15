using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Data;
using System.Globalization;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using Microsoft.Practices.EnterpriseLibrary.Caching;

namespace QLBH.Core.Data
{
    /// <summary>
    /// Thuộc tính này xác định caption của cột trên XtraGrid
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CaptionColumn : Attribute
    {
        private readonly string caption;
        public CaptionColumn(string caption)
        {
            this.caption = caption;
        }

        public string Caption
        {
            get { return caption; }
        }
    }

    /// <summary>
    /// Thuộc tính này xác định có hiển thị mặc định trên XtraGrid hay không.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DefaultDisplay : Attribute
    {
        private readonly bool isDefaultDisplay;
        public DefaultDisplay(bool isDefaultDisplay)
        {
            this.isDefaultDisplay = isDefaultDisplay;
        }

        public bool IsDefaultDisplay
        {
            get { return isDefaultDisplay; }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class DisplayFormat : Attribute
    {
        private readonly string format;
        private readonly FormatType formatType;

        public DisplayFormat(string format, FormatType formatType)
        {
            this.format = format;
            this.formatType = formatType;
        }

        public DisplayFormat(string format)
        {
            this.format = format;
        }

        public String FormatString
        {
            get { return format; }
        }

        public FormatType FormatType
        {
            get { return formatType; }
        }
    }

    /// <summary>
    /// Thuộc tính này xác định kiểu editor của một cột trên XtraGrid.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class XtraGridEditor : Attribute
    {
        private readonly Type repositoryItem;
        private readonly String name;
        public XtraGridEditor(Type repositoryItem)
        {
            this.repositoryItem = repositoryItem;
        }
        
        public XtraGridEditor(Type repositoryItem, String name)
        {
            this.repositoryItem = repositoryItem;
            this.name = name;
        }

        public Type RepositoryItem
        {
            get { return repositoryItem; }
        }

        public String Name
        {
            get { return name; }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class IsKey : Attribute
    {
        private readonly bool isIdentity;
        public IsKey(bool isIdentity)
        {
            this.isIdentity = isIdentity;
        }

        public bool IsIdentity
        {
            get { return isIdentity; }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class Owner : Attribute
    {
        private string ownerName;
        public Owner(string value)
        {
            ownerName = value;
        }

        public string Name
        {
            get { return ownerName; }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class AutoGen : Attribute
    {
        private readonly bool isAutoGen;
        public AutoGen(bool isAutoGen)
        {
            this.isAutoGen = isAutoGen;
        }

        public bool IsAutoGen
        {
            get { return isAutoGen; }
        }
    }

    public enum JoinType
    {
        INNER,
        LEFT,
        RIGHT,
        CROSS
    }
    [AttributeUsage(AttributeTargets.Class, AllowMultiple= true)]
    public class Join : Attribute
    {
        private string tableName;
        private string[] onFieldNames;
        private JoinType joinType;
        public Join(JoinType joinType, string tableName, params string[] onFieldNames)
        {
            this.onFieldNames = onFieldNames;
            this.tableName = tableName;
            this.joinType = joinType;
        }

        public string QueryFormat(string callerTable)
        {
            string partQuery = String.Format(" {0} JOIN {1} ON ", joinType, tableName);
            for (int i = 0; i < onFieldNames.Length; i++)
            {
                partQuery += String.Format("{0}.{1} = {2}.{1} AND ", callerTable, onFieldNames[i], tableName);
            }
            partQuery = partQuery.Substring(0, partQuery.Length - 5);
            return partQuery;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public class NullValueAttribute : DefaultValueAttribute
    {
        public NullValueAttribute(Type type, string value) : base(type, value) { }
        public NullValueAttribute(char value) : base(value) { }
        public NullValueAttribute(byte value) : base(value) { }
        public NullValueAttribute(short value) : base(value) { }
        public NullValueAttribute(int value) : base(value) { }
        public NullValueAttribute(long value) : base(value) { }
        public NullValueAttribute(float value) : base(value) { }
        public NullValueAttribute(double value) : base(value) { }
        public NullValueAttribute(bool value) : base(value) { }
        public NullValueAttribute(string value) : base(value) { }
        public NullValueAttribute(object value) : base(value) { }
    }

    [Serializable]
    public abstract class DataObject
    {
        public abstract string GetDataObjectName();

        public void SetNullValues<T>(T defaultObject)
        {
            PropertyInfo[] objProperties = CBO.Instance.GetPropertyInfo(typeof(T));
            PropertyInfo objPropertyInfo;
            for (int intProperty = 0; intProperty < objProperties.Length; intProperty++)
            {
                objPropertyInfo = objProperties[intProperty];
                object[] attDefaultValues = objPropertyInfo.GetCustomAttributes(typeof(NullValueAttribute), false);
                if (attDefaultValues.Length > 0 && attDefaultValues[0] is NullValueAttribute)
                {
                    objPropertyInfo.SetValue(defaultObject, (attDefaultValues[0] as NullValueAttribute).Value, null);
                }
            }
        }
    }

    public interface ISuDung
    {
        int SuDung { get; set; }
    }

    public abstract class ProviderBase
    {
        private static CacheManager catchManager;

        private static CacheManager CacheIsolatedStorage
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
                    throw ex;
                }
                return catchManager;
            }
        }

        public static int Delete<T>(T delObject)
        {
            PropertyInfo[] objProperties = CBO.Instance.GetPropertyInfo(typeof(T));
            PropertyInfo objPropertyInfo;
            string sWhere = string.Empty;
            for (int intProperty = 0; intProperty < objProperties.Length; intProperty++)
            {
                objPropertyInfo = objProperties[intProperty] as PropertyInfo;
                object[] attIsKeys = objPropertyInfo.GetCustomAttributes(typeof(IsKey), false);
                object[] attNullValues = objPropertyInfo.GetCustomAttributes(typeof(NullValueAttribute), false);
                if (attIsKeys.Length > 0 &&
                    (attNullValues.Length == 0 || (attNullValues[0] is NullValueAttribute &&
                        !Equals((attNullValues[0] as NullValueAttribute).Value, objPropertyInfo.GetValue(delObject, null)))))
                {
                    sWhere += objPropertyInfo.Name + "=";
                    sWhere += Quote(objPropertyInfo.PropertyType, objPropertyInfo.GetValue(delObject, null)) + " AND ";
                }
            }
            if(String.IsNullOrEmpty(sWhere)) throw new MissingFieldException("Không có dữ liệu để tạo điều kiện xóa");
            sWhere = sWhere.Substring(0, sWhere.Length - 5);

            if (!(delObject is DataObject)) throw new NotSupportedException("Không phải kiểu DataObject");
            string tblName = (delObject as DataObject).GetDataObjectName();

            string commandText = String.Format("DELETE {0} WHERE {1}", tblName, sWhere);
#if DEBUG
            Debug.Print(commandText);
#endif
            int result = SqlHelper.ExecuteNonQuery(ConnectionUtil.Instance.GetConnection(), CommandType.Text, commandText);

            //cache hien moi chi ap dung voi cac bang danh muc
            if (tblName.ToLower().StartsWith("tbl_dm_")) ClearCg(tblName);

            return result;

        }

        public static int Update<T>(T upObject)
        {
            PropertyInfo[] objProperties = CBO.Instance.GetPropertyInfo(typeof(T));
            PropertyInfo objPropertyInfo;
            string sSet = string.Empty;
            string sWhere = string.Empty;
            for (int intProperty = 0; intProperty < objProperties.Length; intProperty++)
            {
                objPropertyInfo = objProperties[intProperty] as PropertyInfo;
                object[] attIsKeys = objPropertyInfo.GetCustomAttributes(typeof(IsKey), false);
                object[] attNullValues = objPropertyInfo.GetCustomAttributes(typeof(NullValueAttribute), false);
                if (attIsKeys.Length == 0 &&
                    (attNullValues.Length == 0 || (attNullValues[0] is NullValueAttribute &&
                        !Equals((attNullValues[0] as NullValueAttribute).Value, objPropertyInfo.GetValue(upObject, null)))))
                {
                    sSet += objPropertyInfo.Name + "=";
                    sSet += Quote(objPropertyInfo.PropertyType, objPropertyInfo.GetValue(upObject, null)) + ",";
                }
                else if(attIsKeys.Length == 0)
                {
                    sSet += objPropertyInfo.Name + "=NULL,";
                }
                else
                {
                    sWhere += objPropertyInfo.Name + "=";
                    sWhere += Quote(objPropertyInfo.PropertyType, objPropertyInfo.GetValue(upObject, null)) + " AND ";
                }
            }
            sSet = sSet.Substring(0, sSet.Length - 1);
            sWhere = sWhere.Substring(0, sWhere.Length - 5);

            if (!(upObject is DataObject)) throw new NotSupportedException("Không phải kiểu DataObject");
            string tblName = (upObject as DataObject).GetDataObjectName();

            string commandText = String.Format("UPDATE {0} SET {1} WHERE {2}", tblName, sSet, sWhere);
#if DEBUG
            Debug.Print(commandText);
#endif
            int result = SqlHelper.ExecuteNonQuery(ConnectionUtil.Instance.GetConnection(), CommandType.Text, commandText);

            //cache hien moi chi ap dung voi cac bang danh muc
            if (tblName.ToLower().StartsWith("tbl_dm_")) ClearCg(tblName);

            return result;

        }

        public static int Insert<T>(T insObject)
        {
            PropertyInfo[] objProperties = CBO.Instance.GetPropertyInfo(typeof(T));
            PropertyInfo objPropertyInfo;
            string sFields = string.Empty;
            string sValues = string.Empty;
            bool hasIdentityKey = false;
            for (int intProperty = 0; intProperty < objProperties.Length; intProperty++)
            {
                objPropertyInfo = objProperties[intProperty] as PropertyInfo;
                object[] attIsKeys = objPropertyInfo.GetCustomAttributes(typeof(IsKey), false);
                object[] attNullValues = objPropertyInfo.GetCustomAttributes(typeof(NullValueAttribute), false);
                if ((attIsKeys.Length == 0 || (attIsKeys[0] is IsKey && !(attIsKeys[0] as IsKey).IsIdentity)) &&
                    (attNullValues.Length == 0 || (attNullValues[0] is NullValueAttribute &&
                        !Equals((attNullValues[0] as NullValueAttribute).Value, objPropertyInfo.GetValue(insObject, null)))))
                {
                    sFields += objPropertyInfo.Name + ",";
                    sValues += Quote(objPropertyInfo.PropertyType, objPropertyInfo.GetValue(insObject, null)) + ",";                        
                }
                else
                {
                    hasIdentityKey = true;
                }
            }
            sFields = sFields.Substring(0, sFields.Length - 1);
            sValues = sValues.Substring(0, sValues.Length - 1);
            
            if (!(insObject is DataObject)) throw new NotSupportedException("Không phải kiểu DataObject");
            string tblName = (insObject as DataObject).GetDataObjectName();

            string commandText = String.Format("INSERT INTO {0}({1}) VALUES({2})", tblName, sFields, sValues);
#if DEBUG
            Debug.Print(commandText);
#endif
            //return number rows affected.
            int result = SqlHelper.ExecuteNonQuery(ConnectionUtil.Instance.GetConnection(), CommandType.Text, commandText);
#if DEBUG
            Debug.Print(String.Format("{0} rows affected.", result));
#endif
            if (hasIdentityKey)
            {
                commandText = String.Format("SELECT {0}_seq.currval FROM DUAL", tblName.ToLower().Replace("tbl_", String.Empty));
                result = Convert.ToInt32(SqlHelper.ExecuteScalar(ConnectionUtil.Instance.GetConnection(), CommandType.Text, commandText));
#if DEBUG
                Debug.Print(String.Format("Seq currval: {0}", result));
#endif
            }

            //cache hien moi chi ap dung voi cac bang danh muc
            if (tblName.ToLower().StartsWith("tbl_dm_")) ClearCg(tblName);

            return result;

        }
        /// <summary>
        /// Sử dụng khi select từ một bảng danh mục
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="match"></param>
        /// <returns></returns>
        public static List<T> SelectWhere<T>(Predicate<T> match)
        {
            return SelectAll<T>().FindAll(match);
        }

        public static List<T> SelectWhere<T>(T condObject)
        {
            PropertyInfo[] objProperties = CBO.Instance.GetPropertyInfo(typeof(T));
            PropertyInfo objPropertyInfo;
            string sFields = string.Empty;
            string sWheres = string.Empty;
            for (int intProperty = 0; intProperty < objProperties.Length; intProperty++)
            {
                objPropertyInfo = objProperties[intProperty];
                object[] attOwners = objPropertyInfo.GetCustomAttributes(typeof(Owner), false);
                object[] attNullValues = objPropertyInfo.GetCustomAttributes(typeof(NullValueAttribute), false);
                object[] attAutoGens = objPropertyInfo.GetCustomAttributes(typeof(AutoGen), false);
                if(attAutoGens.Length == 0 || (attAutoGens.Length > 0 && (attAutoGens[0] as AutoGen).IsAutoGen))
                {   
                    sFields += getFieldName(attOwners, objPropertyInfo.Name) + ",";                    
                }
                if (attNullValues.Length > 0 && attNullValues[0] is NullValueAttribute &&
                        !Equals((attNullValues[0] as NullValueAttribute).Value, objPropertyInfo.GetValue(condObject, null)))
                {
                    sWheres += getFieldName(attOwners, objPropertyInfo.Name) + "=" + Quote(objPropertyInfo.PropertyType, objPropertyInfo.GetValue(condObject, null)) + " AND ";
                }
            }
            sFields = sFields.Substring(0, sFields.Length - 1);
            sWheres = sWheres.Substring(0, sWheres.Length - 5);

            if (!(condObject is DataObject)) throw new NotSupportedException("Không phải kiểu DataObject");
            string tblName = (condObject as DataObject).GetDataObjectName();

            string commandText = String.Format("SELECT {0} FROM {1} WHERE {2}", sFields, tblName + getJoinPart<T>(tblName), sWheres);

            IDataReader dr = SqlHelper.ExecuteReader(ConnectionUtil.Instance.GetConnection(), CommandType.Text , commandText);
            return CBO.Instance.FillCollection<T>(dr);

        }

        /// <summary>
        /// Sử dụng khi select từ một bảng danh mục
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="match"></param>
        /// <returns></returns>
        public static T SelectTop<T>(Predicate<T> match)
        {
            return SelectAll<T>().Find(match);
        }

        public static bool Exists<T>(Predicate<T> match)
        {
            return SelectAll<T>().Exists(match);
        }

        public static List<T> SelectAll<T>()
        {
            string tblName = getDataObjectName(typeof(T));
            string sFields = string.Empty;
            string sOrders = string.Empty;

            PropertyInfo[] objProperties = CBO.Instance.GetPropertyInfo(typeof(T));
            PropertyInfo objPropertyInfo;

            for (int intProperty = 0; intProperty < objProperties.Length; intProperty++)
            {
                objPropertyInfo = objProperties[intProperty];
                object[] attIsKeys = objPropertyInfo.GetCustomAttributes(typeof(IsKey), false);
                object[] attAutoGens = objPropertyInfo.GetCustomAttributes(typeof(AutoGen), false);
                object[] attOwners = objPropertyInfo.GetCustomAttributes(typeof(Owner), false);
                if (attAutoGens.Length == 0 || (attAutoGens.Length > 0 && (attAutoGens[0] as AutoGen).IsAutoGen))
                {
                    if (attIsKeys.Length > 0 && attIsKeys[0] is IsKey)
                    {
                        sOrders += getFieldName(attOwners, objPropertyInfo.Name) + ",";
                    }
                    sFields += getFieldName(attOwners, objPropertyInfo.Name) + ",";
                }
            }

            sFields = sFields.Substring(0, sFields.Length - 1);

            string commandText = string.Empty;
            if (!String.IsNullOrEmpty(sOrders))
            {
                sOrders = sOrders.Substring(0, sOrders.Length - 1);
                commandText = String.Format("SELECT {0} FROM {1} ORDER BY {2}", sFields, tblName + getJoinPart<T>(tblName), sOrders);                
            }
            else
            {
                commandText = String.Format("SELECT {0} FROM {1}", sFields, tblName + getJoinPart<T>(tblName));
            }
            return GetList<T>(CommandType.Text, commandText);
        }

        //hàm này để hỗ trợ cho các danh mục có thuộc tính SuDung
        protected static bool IsSuDung<T>(T isUsedObject)
        {
            try
            {
                return (isUsedObject as ISuDung).SuDung == 1;
            }
            catch (Exception ex)
            {
                throw new NotSupportedException("Không hỗ trợ ISuDung", ex);
            }
        }

        private static string getJoinPart<T>(string callerTblName)
        {
            string sJoins = string.Empty;
            object[] attJoins = typeof(T).GetCustomAttributes(typeof(Join), false);
            if (attJoins.Length > 0)
            {
                foreach (Join attJoin in attJoins)
                {
                    sJoins += attJoin.QueryFormat(callerTblName);
                }
            }
            return sJoins;
        }

        private static string getFieldName(object[] attOwners, string fieldName)
        {
            if (attOwners.Length > 0 && attOwners[0] is Owner)
            {
                return String.Format("{0}.{1}", (attOwners[0] as Owner).Name, fieldName);
            }
            return fieldName;
        }

        private static List<T> GetList<T>(CommandType commandType, string commandText)
        {
#if DEBUG
            Debug.Print(commandText);
#endif
            string key = commandText.GetHashCode().ToString();
            string mac = GetMacAddress();
            bool? isRefreshed = IsRefreshed(key, mac);
            List<T> result;

            if (isRefreshed == true && CacheIsolatedStorage.Contains(key))
            {
                result = CacheIsolatedStorage[key] as List<T>;
            }
            else
            {
                string dependTables = GetTblName(commandText);

                IDataReader dr = SqlHelper.ExecuteReader(ConnectionUtil.Instance.GetConnection(), commandType, commandText);
                result = CBO.Instance.FillCollection<T>(dr);
                
                //hien chi set cache voi cac bang danh muc
                if (result.Count > 0 && dependTables.Split(',').Length == 1 && dependTables.StartsWith("tbl_dm_"))
                {
                    CacheIsolatedStorage.Add(key, result);
                    SaveCg(key, mac, dependTables, isRefreshed != null);
                }

            }
            return result;
        }

        private static string getDataObjectName(Type objType)
        {
            try
            {
                return (Activator.CreateInstance(objType) as DataObject).GetDataObjectName();
            }
            catch (Exception ex)
            {
                throw new NotSupportedException("Không phải kiểu DataObject", ex);
            }
        }

        private static void SaveCg(string key, string mac, string dep, bool existed)
        {
            string commandText;
            if (!existed)
                commandText = "INSERT INTO tbl_cg(cname,identifier,cdep,cref,ccre) VALUES('{0}','{1}','{2}',1,SYSDATE)";
            else
                commandText = "UPDATE tbl_cg SET cdep='{2}',cref=1,ccre=SYSDATE WHERE cname='{0}' AND identifier='{1}'";
            
            commandText = String.Format(commandText, key, mac, dep);

            SqlHelper.ExecuteNonQuery(ConnectionUtil.Instance.GetConnection(), CommandType.Text, commandText);
        }

        private static void ClearCg(string dep)
        {
            string commandText = "UPDATE tbl_cg SET cref=0 WHERE cdep LIKE '%{0}%'";
            
            commandText = String.Format(commandText, dep.ToLower());

            SqlHelper.ExecuteNonQuery(ConnectionUtil.Instance.GetConnection(), CommandType.Text,
                "UPDATE tbl_cg SET cref=0 WHERE cdep LIKE '%'|:dep|'%'", dep.ToLower());
        }

        private static bool? IsRefreshed(string key, string mac)
        {
            object result = SqlHelper.ExecuteScalar(ConnectionUtil.Instance.GetConnection(), CommandType.Text,
                "SELECT cref FROM tbl_cg WHERE cname=:cname AND identifier=:identifier", key, mac);
            
            if (result != null) return Convert.ToBoolean(result);
            return null;
        }

        private static string GetMacAddress()
        {
            string macAddresses = String.Empty;
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddresses;
        }

        private static string GetTblName(string input)
        {
            int startIndex = 0, postIndex = 0;
            string result = String.Empty, tblUnit = String.Empty;
            string tblPrefix = "tbl_";
            char[] chrs = new char[] { ' ', ',', '.' };
            input = input.ToLower();
            startIndex = input.IndexOf(tblPrefix);
            postIndex = input.IndexOfAny(chrs, startIndex);
            while (startIndex > 0 && startIndex < input.Length)
            {
                if (postIndex > startIndex)
                {
                    tblUnit = input.Substring(startIndex, postIndex - startIndex);
                    if (result.IndexOf(tblUnit) < 0) result += tblUnit + ",";
                    startIndex = input.IndexOf(tblPrefix, postIndex);
                    if (startIndex > 0) postIndex = input.IndexOfAny(chrs, startIndex);
                }
                else
                {
                    tblUnit = input.Substring(startIndex);
                    if (result.IndexOf(tblUnit) < 0) result += tblUnit + ",";
                    startIndex = input.IndexOf(tblPrefix, startIndex + 1);
                }
            }
            if (result.Length > 0)
                return result.Substring(0, result.Length - 1);
            return result;
        }

        private static string Quote(Type type, object value)
        {
            if (type == typeof(String))
            {
                if (value == null) return "NULL";
                return String.Format("'{0}'", Escapes(value.ToString()));
            }
            return type == typeof(DateTime) ? String.Format("to_date('{0}', 'MM/DD/YYYY HH:MI:SS AM')", ((DateTime)value).ToString(new CultureInfo("en-US"))) : String.Format("{0}", value);
        }
        
        private static string Escapes(string input)
        {
            return input;
            //todo: @HaH xu ly escapes string input
            //Regex.Replace(input,"'|\"", Match)
            //return input.Replace("'", "\'");
        }

        //private static string Match(string input)
        //{
        //    return "\\" + input;
        //}
    }
}
