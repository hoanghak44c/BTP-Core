using System.Diagnostics;
using QLBH.Core.Data;
using QLBH.Core.Form;
using QLBH.Core.UserControls;

namespace QLBH.Core.TestUnits
{
    //[Serializable]
    //internal class TblDmDoiTuong : DataObject
    //{
    //    #region Overrides of DataObject

    //    public override string GetDataObjectName()
    //    {
    //        return "tbl_DM_DoiTuong";
    //    }

    //    #endregion
    //}

    //[Serializable]
    //internal class DoiTuongInfo : TblDmDoiTuong
    //{
    //    public DoiTuongInfo()
    //    {
    //        SetNullValues(this);
    //    }
    //    [IsKey(true)]
    //    [NullValue(-1)]
    //    public int IdDoiTuong { get; set; }
    //    [NullValue(-1)]
    //    public int IdCha { get; set; }
    //    [NullValue("")]
    //    public string MaDoiTuong { get; set; }
    //    [NullValue("")]
    //    public string TenDoiTuong { get; set; }
    //    [NullValue("")]
    //    public string NguoiLienLac { get; set; }
    //    [NullValue("")]
    //    public string DienThoai { get; set; }
    //    [NullValue("")]
    //    public string Fax { get; set; }
    //    [NullValue("")]
    //    public string Email { get; set; }
    //    [NullValue("")]
    //    public string MaSoThue { get; set; }
    //    [NullValue(-1)]
    //    public int Type { get; set; }
    //    [NullValue(-1)]
    //    public int SuDung { get; set; }
    //    [NullValue("")]
    //    public string GhiChu { get; set; }
    //    [NullValue(-1)]
    //    public int GioiTinh { get; set; }
    //    [NullValue(typeof(DateTime), "01/01/0001 12:00:00 AM")]
    //    public DateTime NgaySinh { get; set; }
    //    [NullValue(-1)]
    //    public int IdOrderType { get; set; }
    //}

    //public interface ITest
    //{
    //    void TestA();
    //}

    //public class Test1
    //{
    //    public void TestA()
    //    {
    //        Debug.Print("TestA");
    //    }
    //}

    //[TestClass]
    public class TestUnits
    {
        public TaskbarNotifier TaskbarNotifier1;

        public TestUnits()
        {
            //TaskbarNotifier1 = new TaskbarNotifier();
            //TaskbarNotifier1.SetBackgroundBitmap(new Bitmap(@"E:\Projects\TaskbarNotifierDemo\skin2.bmp"), Color.FromArgb(255, 0, 255));
            //TaskbarNotifier1.SetCloseBitmap(new Bitmap(@"E:\Projects\TaskbarNotifierDemo\close2.bmp"), Color.FromArgb(255, 0, 255), new Point(300, 74));
            //TaskbarNotifier1.TitleRectangle = new Rectangle(123, 80, 176, 16);
            //TaskbarNotifier1.ContentRectangle = new Rectangle(116, 97, 197, 22);
            //TaskbarNotifier1.EnableSelectionRectangle = true;
            //TaskbarNotifier1.KeepVisibleOnMousOver = true;
            //TaskbarNotifier1.ReShowOnMouseOver = true;
            ConnectionUtil.Instance.IsUAT = 1;
        }
        //[TestMethod]
        public void Test()
        {
            //SchedulerDAO.Instance.GetServiceScheduleInfo("RecentService");
        }
        //[TestMethod]
        //public void TestA()
        //{
        //    Test1 test = new Test1();
        //    ((ITest)test).TestA();
        //}

        //[TestMethod]
        //public void Test()
        //{
        //    Insert(new DoiTuongInfo { IdDoiTuong = 12, DienThoai = "1213123", GioiTinh = 1, GhiChu = "GhiChu1"});
        //}

        //[TestMethod]
        public void TestEncrypt()
        {
            string connection =
                @"Data Source=(DESCRIPTION =
                    (ADDRESS = (PROTOCOL = TCP)(HOST = pos.trananh.com.vn)(PORT = 1521))
                    (CONNECT_DATA =
                      (SERVICE_NAME = POS)
                    )
                  );User Id=uat;Password=eX2Oobztakjx;";

            Debug.Print(GtidCryption.Me.Encrypt(connection, true));
        }

        //[TestMethod]
        public void TestDecrypt()
        {
            string connection =
                @"o8zZJDL2nJcry4Yog5GHEXMI+OA3VY74kHw4vcZSB4dJ8wRrxsaaS0nzBGvGxppLJL9j7XcixftHQB9nL7E6fU0VzDzcArWoISZ8hYUQyvUKqXl9M9BlXzJ5riYXvdhLTkmu5SppHH+54EqN78H8+bUgyehJowHZSfMEa8bGmktJ8wRrxsaaSyywOy4zCMITeY6pqIukSiRceqzUMCt+iknzBGvGxppLSfMEa8bGmkuX3NU5MVNhGcuQ5W1r4kzBYwn//u3n+uxJ8wRrxsaaS0nzBGvGxppLnXSSfdmqPFJJ8wRrxsaaS0nzBGvGxppLbiwmnGMI6PvNbeV6IaA2giW9vBvdK/zaeBDAhkdVq015+Zawachvow==";

            Debug.Print(GtidCryption.Me.Decrypt(connection, true));
        }

        //[TestMethod]
        public void TestTnsReader()
        {
            //Assert.AreEqual(TnsNamesReader.LoadHost("WEBSERICE_TA"), "SUBSERVER");
        }

        //[TestMethod]
        public void TestNotify()
        {
            FormTest frm1 = new FormTest();
            frm1.ShowDialog();
            //TaskbarNotifier1.Show("Test Title", "Test Content", 500, 30000, 500);
        }
        //public int Insert<T>(T insObject)
        //{
        //    PropertyInfo[] objProperties = CBO.Instance.GetPropertyInfo(typeof(T));
        //    PropertyInfo objPropertyInfo;
        //    string sFields = string.Empty;
        //    string sValues = string.Empty;
        //    for (int intProperty = 0; intProperty < objProperties.Length; intProperty++)
        //    {
        //        objPropertyInfo = objProperties[intProperty] as PropertyInfo;
        //        object[] attIsKeys = objPropertyInfo.GetCustomAttributes(typeof(IsKey), false);
        //        object[] attDefaultValues = objPropertyInfo.GetCustomAttributes(typeof(NullValueAttribute), false);
        //        if ((attIsKeys.Length == 0 || (attIsKeys[0] is IsKey && !(attIsKeys[0] as IsKey).IsIdentity)) &&
        //            (attDefaultValues.Length == 0 || (attDefaultValues[0] is NullValueAttribute &&
        //                !Equals((attDefaultValues[0] as NullValueAttribute).Value, objPropertyInfo.GetValue(insObject, null)))))
        //            {
        //                sFields += objPropertyInfo.Name + ",";
        //                sValues += String.Format(objPropertyInfo.PropertyType == typeof(String) ||
        //                    objPropertyInfo.PropertyType == typeof(DateTime) ? "'{0}'" : "{0}", objPropertyInfo.GetValue(insObject, null)) + ",";
        //            }
        //    }
        //    sFields = sFields.Substring(0, sFields.Length - 1);
        //    sValues = sValues.Substring(0, sValues.Length - 1);

        //    string tblName = getDataObjectName(typeof(T));

        //    string commandText = String.Format("INSERT INTO {0}({1}) VALUES({2})", tblName, sFields, sValues);
        //    Debug.Print(commandText);
        //    return 1;
        //    //return SqlHelper.ExecuteNonQuery(ConnectionUtil.Instance.GetConnection(), CommandType.Text, commandText);

        //}

        //private static string getDataObjectName(Type objType)
        //{
        //    try
        //    {
        //        return (Activator.CreateInstance(objType) as DataObject).GetDataObjectName();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new NotSupportedException("Không phải kiểu DataObject", ex);
        //    }
        //}
    }

}
