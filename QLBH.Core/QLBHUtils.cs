using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
//using QLBH.Common.Providers;
using QLBH.Core.Data;
using QLBH.Core.Exceptions;
using QLBH.Core.Form;
using QLBH.Core.Properties;
using QLBH.Core.Providers;
using QLBH.Core.UserControls;
using System.Drawing;
namespace QLBH.Core
{
    public enum ActionState
    {
        ADD = 1,
        DELETE = 2,
        UPDATE = 3
    }

    public class QLBHUtils
    {
        public static bool IsShowNotify = true;

        public static object GetObject(string fullName, params object[] args)
        {
            try
            {
                var module = fullName.Substring(0, fullName.LastIndexOf(".")) +
                    ", Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

                while (true)
                {
                    try
                    {
                        Assembly asm = Assembly.Load(module);

                        return Activator.CreateInstance(asm.GetType(fullName), args);
                    }
                    catch (FileNotFoundException ex)
                    {
                        Assembly asm = Assembly.GetCallingAssembly();

                        return Activator.CreateInstance(asm.GetType(fullName), args);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.ToString(), false, fullName, args);
            }
        }

        public static object GetObject(Type type , params object[] args)
        {
            try
            {
                return Activator.CreateInstance(type, args);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false, type, args);
            }
        }

        public static T GetObject<T>(params object[] args)
        {
            try
            {
                var result = Activator.CreateInstance(typeof (T), args);

                return result is T ? (T) result : default(T);
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        public static string DocSo(ulong so)
        {
            return (new DocSo.DocSo()).DocSo(so);
        }

        private struct ShortCutKeyStruct
        {
            public string Key;
            public Keys ShortKey;
        }

        private static List<KeyValuePair<int, List<ShortCutKeyStruct>>> shortCutKeyManager;

        private static void GetButtonHasShortKey(Control control, ref KeyValuePair<int, List<ShortCutKeyStruct>> form, int hashCode, Keys keyCode)
        {
            foreach (Control ctl in control.Controls)
            {
                if (ctl is GtidSimpleButton)
                {
                    if (form.Value != null)
                        form.Value.Add(new ShortCutKeyStruct { Key = ctl.Name, ShortKey = ((GtidSimpleButton)ctl).ShortCutKey });
                    else
                    {
                        form = new KeyValuePair<int, List<ShortCutKeyStruct>>
                            (hashCode,
                             new List<ShortCutKeyStruct>
                                     {
                                         new ShortCutKeyStruct
                                             {
                                                 Key = ctl.Name,
                                                 ShortKey = ((GtidSimpleButton) ctl).ShortCutKey
                                             }
                                     });
                        shortCutKeyManager.Add(form);
                    }
                }
                else if (ctl.HasChildren)
                {
                    GetButtonHasShortKey(ctl, ref form, hashCode, keyCode);
                }
            }
        }

        private static void CallShortKeyAction(Control control, KeyValuePair<int, List<ShortCutKeyStruct>> form, Keys keyCode)
        {
            if (form.Value == null) return;

            var shortKey = form.Value.Find(delegate(ShortCutKeyStruct match) { return match.ShortKey == keyCode; });
            
            if (!String.IsNullOrEmpty(shortKey.Key))
            {
                Control[] controls = control.Controls.Find(shortKey.Key, true);
                
                if (controls.Length > 0 && controls[0] is GtidSimpleButton)
                {
                    if (controls[0].Enabled && controls[0].Visible)
                        ((GtidSimpleButton)controls[0]).PerformClick();
                
                    return;
                }
            }            
        }

        public static void PerformShortCutKey(Control control, Keys keyCode, params int[] keySalt)
        {
            int hashCode = 0;

            if (keySalt.Length > 0) hashCode = keySalt[0];

            if (control is System.Windows.Forms.Form)
            {
                (control as System.Windows.Forms.Form).Cursor = Cursors.WaitCursor;
                hashCode = control.Name.GetHashCode();
            }
            
            if (shortCutKeyManager == null) shortCutKeyManager = new List<KeyValuePair<int, List<ShortCutKeyStruct>>>();

            var form = shortCutKeyManager.Find(delegate(KeyValuePair<int, List<ShortCutKeyStruct>> match)
                                                   {
                                                       return match.Key == hashCode;
                                                   });
            if (form.Value == null)
                GetButtonHasShortKey(control, ref form, hashCode, keyCode);

            CallShortKeyAction(control, form, keyCode);

            if (control is System.Windows.Forms.Form)
            {
                (control as System.Windows.Forms.Form).ResetCursor();
            }
        }

    }

    public abstract class NotifiyBase
    {
        protected TaskbarNotifier TaskbarNotifier;

        private DateTime notifyDateTime;

        private Thread notifyThread;

        private bool isRunning;

        protected List<string> FunctionCode;

        protected NotifiyBase()
        {
            notifyDateTime = CommonProvider.Instance.GetSysDate();
            TaskbarNotifier = new TaskbarNotifier();
            TaskbarNotifier.SetBackgroundBitmap(Resources.skin2, Color.FromArgb(255, 0, 255));
            TaskbarNotifier.SetCloseBitmap(Resources.close2, Color.FromArgb(255, 0, 255), new Point(300, 74));
            TaskbarNotifier.TitleRectangle = new Rectangle(123, 80, 176, 16);
            TaskbarNotifier.ContentRectangle = new Rectangle(116, 97, 197, 22);
            TaskbarNotifier.KeepVisibleOnMousOver = true;
            TaskbarNotifier.ReShowOnMouseOver = true;
            TaskbarNotifier.TitleClick += new EventHandler(TaskbarNotifier_TitleClick); ;
            TaskbarNotifier.ContentClick += new EventHandler(TaskbarNotifier_ContentClick);
            TaskbarNotifier.CloseClick += new EventHandler(TaskbarNotifier_CloseClick);
            isRunning = true;
        }

        protected virtual void OnCloseClick() { }
        void TaskbarNotifier_CloseClick(object sender, EventArgs e)
        {
            OnCloseClick();
        }

        protected virtual void OnContentClick() { }
        void TaskbarNotifier_ContentClick(object sender, EventArgs e)
        {
            OnContentClick();
        }

        protected virtual void OnTitleClick(){}
        void TaskbarNotifier_TitleClick(object sender, EventArgs e)
        {
            OnTitleClick();
        }

        public virtual int HasChanged(DateTime checkPoint)
        {
            return 0;
        }

        public virtual void Start(object sender)
        {
            //if (!PrivilegedProvider.Instance.IsSupperUser &&
            //    !PrivilegedProvider.Instance.CurrentPrivileges
            //    .Exists(delegate(string match)
            //    {
            //        return FunctionCode.Contains(match);
            //    }))
            //    return;
            try
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = Application.StartupPath + "\\QLBanHang.exe.config";
                Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                if (configuration.AppSettings.Settings["NotifierProcess"] == null ||
    configuration.AppSettings.Settings["NotifierProcess"].Value != Process.GetCurrentProcess().Id.ToString())
                {
                    try
                    {
                        Process.GetProcessById(
                            Convert.ToInt32(configuration.AppSettings.Settings["NotifierProcess"].Value));

                        return;
                    }
                    catch (Exception ex)
                    {
                        if (ex is ArgumentException || ex is FormatException || ex is NullReferenceException)
                        {
                            if (ex is NullReferenceException)
                                configuration.AppSettings.Settings.Add("NotifierProcess", Process.GetCurrentProcess().Id.ToString());
                            else
                                configuration.AppSettings.Settings["NotifierProcess"].Value =
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

                                    if (configuration.AppSettings.Settings["NotifierProcess"] == null)
                                        configuration.AppSettings.Settings.Add("NotifierProcess", Process.GetCurrentProcess().Id.ToString());
                                    else
                                        configuration.AppSettings.Settings["NotifierProcess"].Value = String.Empty;

                                    configuration.Save();
                                }
                            }
                        }
                        else throw;
                    }
                }

                if (PrivilegedProvider.Instance == null)
                {
                    EventLogProvider.Instance.WriteLog("Không có quyền thực hiện.", "Previleged");
                    return;
                }

                bool hasPrivilegde = PrivilegedProvider.Instance.IsSupperUser;
                if (!hasPrivilegde && PrivilegedProvider.Instance.CurrentPrivileges != null)
                {
                    foreach (string fCode in FunctionCode)
                    {
                        hasPrivilegde = PrivilegedProvider.Instance.CurrentPrivileges.Contains(fCode);

                        if (hasPrivilegde) break;
                    }
                }

                if (!hasPrivilegde) return;

                if (sender == null)
                {
                    EventLogProvider.Instance.WriteLog("Form chưa được tạo", "Load plugins");
                    return;
                }

                notifyThread = new Thread(ShowNotify);
                notifyThread.Start(sender);
            }
            catch (Exception ex)
            {
                EventLogProvider.Instance.WriteLog(ex.ToString(), "Load plugins");
            }
        }

        public void Stop()
        {
            isRunning = false;
            //try
            //{
            //    notifyThread.Abort();
            //}
            //catch (ThreadAbortException){ }
            //catch(Exception ex)
            //{
            //    EventLogProvider.Instance.WriteLog(ex.ToString(), "UnLoad plugins");
            //}
        }

        private void ShowNotify(object sender)
        {
            int countChanged;

            while (sender is Control && !((Control)sender).IsDisposed && isRunning && QLBHUtils.IsShowNotify)
            {
                try
                {
                    if (!((Control)sender).IsDisposed && (countChanged = HasChanged(notifyDateTime)) > 0)
                    {

                        ((Control)sender).Invoke((MethodInvoker)
                               delegate()
                               {
                                   if (!TaskbarNotifier.IsDisposed && TaskbarNotifier.IsHandleCreated)
                                   {
                                       TaskbarNotifier.Show(TaskbarNotifier.TitleText,
                                                            String.Format(TaskbarNotifier.ContentText,
                                                                          countChanged), 500, 6000, 500);

                                       notifyDateTime = CommonProvider.Instance.GetSysDate();
                                   }
                               });
                    }

                    //int timeDelay = 0;
                    //while (!((Control)sender).IsDisposed && isRunning && timeDelay < 300000)
                    //{
                    //    timeDelay += 500;
                    //    Thread.Sleep(500);
                    //}

                    Thread.CurrentThread.Join(15000);
                }
                catch (Exception ex)
                {
                    if (!((Control)sender).IsDisposed)
                        EventLogProvider.Instance.WriteLog(ex.ToString(), "ShowNotify");
                }
            }
        }
    }

    abstract class RegInfoBase
    {
        abstract internal string Org { get; }

        abstract internal string Id { get; }
    }

    class RegInfoEx : RegInfoBase
    {
        #region Overrides of RegInfoBase

        internal override string Org
        {
            get { return "0W6gK1BhcZOV8comBOOqBnxu3VnF2nf/9iZXO134lh06fjewAK3dCk1gmHOeJUECfRItpN11SXdbmTViX/k5/bHrm/TCVcgx6s7MWq7waBCNfqSBnAwr7/0OnHXnSBcmg+bxtSNJ3oMM6u/37W6jmlajSchYetJ6uiTFGz+lGRmJKYQawywuFg1VkRMf1I1yJT5FVx0Bb4o="; }
        }

        internal override string Id
        {
            get { return "jTGzRsXNOs1CZugDHHWtsyLPm49EC2tNiZwFm61yvmreV8MmyB/1UP13PH70Zbb9NpUU7+7gvmNgWQIUil0mHz4M2M3XsTo0X4dbO2ECARVjX62xegFBLDM/S50kSaFH8gx8IBUq8/7Gy5/uj67RwoeelQnBl8Z0"; }
        }

        #endregion
    }

    class RegInfo : RegInfoBase
    {
        #region Overrides of RegInfoBase

        internal override string Org
        {
            get { return "0W6gK1BhcZOV8comBOOqBnxu3VnF2nf/9iZXO134lh06fjewAK3dCk1gmHOeJUECfRItpN11SXdbmTViX/k5/bHrm/TCVcgx6s7MWq7waBCNfqSBnAwr7/0OnHXnSBcmg+bxtSNJ3oMM6u/37W6jmlajSchYetJ6uiTFGz+lGRkr/2Lw68L3/w1VkRMf1I1yJT5FVx0Bb4o="; }
        }

        internal override string Id
        {
            get { return "jTGzRsXNOs1CZugDHHWtsyLPm49EC2tNiZwFm61yvmreV8MmyB/1UP13PH70Zbb9NpUU7+7gvmNgWQIUil0mHz4M2M3XsTo0X4dbO2ECARVjX62xegFBLDM/S50kSaFH8gx8IBUq8/7vroQPi+yeu4eelQnBl8Z0"; }
        }

        #endregion
    }

    public class Regmon
    {
        private static Regmon _instance;

        private static RegInfoBase regInfo;

        private Regmon()
        {
            if (ConnectionUtil.Instance.IsUAT == 1)

                regInfo = new RegInfo();

            else

                regInfo = new RegInfoEx();
        }

        public static Regmon Instance
        {
            get { return _instance ?? (_instance = new Regmon()); }
        }

        public delegate void ValidAction<T>(T info);

        public List<T> ValidResult<T>(List<T> result, string code, ValidAction<T> validAction)
        {
            for (var i = 0; i < result.Count; i++)
            {
                if (RegValid(code, result[i]))

                    continue;

                result.RemoveAt(i);

                validAction.Invoke(result[i]);

                i--;
            }

            return result;
        }

        public string ValidSql(string code, string sql)
        {
            try
            {
                //if(CommonProvider.Instance.GetSysDate() < new DateTime(2015, 12, 31)) return sql;

                if (!isValid(code)) return String.Empty;
                    
                return String.Format(@"select * from ({0}) where instr('{1}', {2}) > 0", sql, regOrg, resolve(code));

                
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        private static bool isValid(string code)
        {
            return code == "VAPMDD1kOEnloiFl+DSbWA==";
        }

        public List<T> ValidResult<T>(List<T> result, string code)
        {
            for (var i = 0; i < result.Count; i++)
            {
                if (RegValid(code, result[i]))

                    continue;

                result.RemoveAt(i);

                i--;
            }

            return result;
        }

        private static bool RegValid(string code, object info)
        {
            //if(CommonProvider.Instance.GetSysDate() < new DateTime(2015,12,31)) return true;

            if (info.GetType().GetProperty(resolve(code)).PropertyType == typeof(int))
            {
                return regId.Contains(String.Format(",{0},", info.GetType().GetProperty(resolve(code)).GetValue(info, null)));                
            }

            if (info.GetType().GetProperty(resolve(code)).PropertyType == typeof(string))
            {
                return regOrg.Contains(String.Format(",{0},", info.GetType().GetProperty(resolve(code)).GetValue(info, null)));
            }

            return false;
        }

        private static string resolve(string code)
        {
            return GtidCryptionReg.DecryptEx(code, true);
        }

        private static string regOrg
        {
            get { return GtidCryptionReg.DecryptEx(regInfo.Org, true); }
        }

        private static string regId
        {
            get { return GtidCryptionReg.DecryptEx(regInfo.Id, true); }
        }
    }

    public class TriggerAfter<T>
    {
        public TriggerAfter()
        {
            var sf = new StackFrame(1);

            ownerType = sf.GetMethod().ReflectedType;
        }

        public String RemindAction { get; set; }

        private IWin32Window owner { get; set; }

        private readonly Type ownerType;

        private static readonly List<IWin32Window> lstPending = new List<IWin32Window>();

        public delegate bool TriggerConditionDelegate();

        public delegate void TriggerBodyDelegate();

        public TriggerConditionDelegate PreTrigCond { get; set; }

        public TriggerConditionDelegate PostTrigCond { get; set; }

        public TriggerBodyDelegate Body { get; set; }

        private void Pendding()
        {
            foreach (System.Windows.Forms.Form openForm in Application.OpenForms)
            {
                if (openForm.GetType() == ownerType)
                {
                    if (!lstPending.Contains(openForm))
                    {
                        lstPending.Add(openForm);

                        owner = openForm;
                    }
                }
            }            
        }

        private void PreAction()
        {
            Pendding();

            foreach (System.Windows.Forms.Form openForm in Application.OpenForms)
            {
                if (!(openForm is T)) continue;

                MessageBox.Show(RemindAction);

                //lstPending.Add(Owner);

                openForm.Activate();

                var pending = new Thread(delegate()
                {
                    while (PreTrigCond.Invoke())
                    {
                        Thread.Sleep(500);
                        //Waiting here....
                    }
                });
                pending.Start();

                pending.Join();

                break;
            }            
        }

        private void PostAction()
        {
            if (PostTrigCond.Invoke())
            {
                if (lstPending.Count == 0)
                {
                    Body.Invoke();
                }
                else
                {
                    while (lstPending.Count > 0)
                    {
                        if (lstPending[0] == owner)
                        {
                            Body.Invoke();

                            lstPending.RemoveAt(0);

                            break;
                        }

                        Thread.Sleep(500);
                    }
                }
            }            
        }

        private void Action()
        {
            new Thread(

                delegate()
                {
                    try
                    {
                        PreAction();

                        PostAction();
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(owner, ex.Message);
                    }
                }).Start();            
        }

        public void Start()
        {
            Action();
        }
    }

    internal class CommonProvider
    {
        private static CommonProvider instance;

        private CommonProvider() { }

        public static CommonProvider Instance
        {
            get { return instance ?? (instance = new CommonProvider()); }
        }

        public double GetVersion()
        {
            return CommonDAO.Instance.GetVersion();
        }
      
        public string GetPath()
        {
            return CommonDAO.Instance.GetPath();
        }

        public DateTime GetSysDate()
        {
            return CommonDAO.Instance.GetSysDate();
        }
    }

    internal class CommonDAO : BaseDAO
    {
        private static CommonDAO instance;
        private CommonDAO() { }

        public static CommonDAO Instance
        {
            get { return instance ?? (instance = new CommonDAO()); }
        }

        public double GetVersion()
        {
            ExecuteCommand("sp_GetVersion");
            return Convert.ToDouble(Parameters["p_Version"].Value.ToString());
        }
        
        public string GetPath()
        {
            ExecuteCommand("sp_GetPath");
            return Convert.ToString(Parameters["p_Path"].Value);
        }

        public DateTime GetSysDate()
        {
            ExecuteCommand("sp_GetSysDate");

            if (Parameters["p_SysDate"].Value is DateTime)
            {
                return Convert.ToDateTime(Parameters["p_SysDate"].Value);
            }
            if (Parameters["p_SysDate"].Value is Oracle.DataAccess.Types.OracleDate)
            {
                return ((Oracle.DataAccess.Types.OracleDate)Parameters["p_SysDate"].Value).Value;
            }

            return DateTime.MinValue;

        }
    }
}
