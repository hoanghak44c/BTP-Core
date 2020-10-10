using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using QLBH.Core.Data;
using QLBH.Core.Exceptions;
using QLBH.Core.Providers;

namespace QLBH.Core.Version
{
    public interface IUpdateVersion
    {
        bool CheckUpdate();

        double CommingNewVersion { get; }

        void Login();

        void Run();
        
        void UpToVersion(string commingVersion);
    }

    public abstract class VerBase : IUpdateVersion
    {
		public static readonly double CurrentVersion = 20201010111500;

        internal protected IUpVer Provider;

        protected VerBase()
        {
            CreateProvider();
        }

        internal abstract void CreateProvider();

        private bool checkUpdate()
        {
            try
            {
                var appLivUp = String.Format("{0}\\LiveUpdate.exe", Application.StartupPath);

                var appLivUp2 = String.Format("{0}\\LiveUpdate2.exe", Application.StartupPath);

                if (File.Exists(appLivUp2))
                {
                    try
                    {
                        File.Copy(appLivUp2, appLivUp, true);

                        File.Delete(appLivUp2);
                    }
                    catch (Exception ex) { }
                }

                var commingNewVersion = CommingNewVersion;

                if (commingNewVersion > 0)
                {
                    var sPath = Provider.GetPath();

                    var processStartInfo =

                        new ProcessStartInfo(appLivUp, sPath + " " + Convert.ToString(commingNewVersion) + " " +

                            Process.GetCurrentProcess().MainModule.ModuleName) { WorkingDirectory = Application.StartupPath };

                    if (Environment.OSVersion.Version.Major >= 6)
                    {
                        processStartInfo.Verb = "runas";
                    }
                    Process.Start(processStartInfo);

                    return true;
                }
                
                var d = DateTime.ParseExact(CurrentVersion.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

                return d.AddDays(45) < Provider.GetSysDate();
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }
        }

        private double commingNewVersion()
        {
            try
            {
                var lastVersion = Provider.GetVersion();

                if (ConnectionUtil.Instance.IsUAT == 3) return 0;

                return CurrentVersion < lastVersion ? lastVersion : 0;
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, false);
            }
        }

        private void login()
        {
            run("QLBanHang.Modules.HeThong.frmLogin");
        }

        private void run(string param)
        {
            Application.Run((System.Windows.Forms.Form)QLBHUtils.GetObject(param));
        }

        protected internal abstract string getParam();

        #region Implementation of IUpdateVersion

        public bool CheckUpdate()
        {
            return checkUpdate();
        }

        public double CommingNewVersion
        {
            get { return commingNewVersion(); }
        }

        public void Login()
        {
            login();
        }

        public void Run()
        {
            run(getParam());
        }

        private void upToVersion(string version)
        {
            Provider.UpVer(version);
        }

        public void UpToVersion(string commingVersion)
        {
            upToVersion(commingVersion);    
        }

        #endregion
    }

    public class SaleTidVer: VerBase
    {
        private static IUpdateVersion instance;

        private SaleTidVer() { }

        public static IUpdateVersion Instance
        {
            get { return instance ?? (instance = new SaleTidVer()); }
        }

        #region Overrides of VerBase

        internal override void CreateProvider()
        {
            Provider = SaleTidUpVerProvider.Instance;
        }

        protected internal override string getParam()
        {
            return "QLBanHang.Modules.frmMain";
        }
        
        #endregion
    }

    public class CrmTidVer : VerBase
    {
        private static IUpdateVersion instance;

        private CrmTidVer() { }

        public static IUpdateVersion Instance
        {
            get { return instance ?? (instance = new CrmTidVer()); }
        }

        #region Overrides of VerBase

        internal override void CreateProvider()
        {
            Provider = CrmTidUpVerProvider.Instance;
        }

        protected internal override string getParam()
        {
            return "QLBanHangCRM.Modules.frmMainCRM";
        }

        #endregion
    }
}
