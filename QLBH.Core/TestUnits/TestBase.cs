using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using QLBH.Core.Form;

namespace QLBH.Core.TestUnits
{
    public class TestBase
    {
        public TestBase(bool isGenPerTest)
        {
            if (isGenPerTest)
                frmProgress.Instance.DoWork(buildData);
        }

        private void buildData()
        {
            frmProgress.Instance.MaxValue = 10;
            frmProgress.Instance.Text = "Đồng bộ dữ liệu test";
            frmProgress.Instance.Description = "Đang kiểm tra thư mục...";
            DirectoryInfo directoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            string moduleName = String.Empty;
            string[] fileNames;
            string result;

            while (!directoryInfo.Parent.Name.Equals("Modules"))
            {
                directoryInfo = directoryInfo.Parent;
            }
            moduleName = directoryInfo.Name;

            while (!directoryInfo.Name.StartsWith("QLBH"))
            {
                directoryInfo = directoryInfo.Parent;
            }
            directoryInfo = directoryInfo.Parent;
            directoryInfo = directoryInfo.GetDirectories("QLBH.Database", SearchOption.TopDirectoryOnly)[0];
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Bắt đầu chuẩn bị dữ liệu...";
            StreamWriter streamWriter = File.CreateText(directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\build.sql");
            streamWriter.WriteLine("SET FEEDBACK OFF");
            streamWriter.WriteLine("SET VERIFY OFF");
            streamWriter.WriteLine("SPO x.log");
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Nạp cấu trúc tables...";
            fileNames = Directory.GetFiles(directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\Create Scripts\\Tables", "*.sql");
            result = GetExecuteFiles(fileNames, moduleName);
            streamWriter.WriteLine(result);
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Nạp cấu trúc store procedures...";
            fileNames = Directory.GetFiles(directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\Create Scripts\\Store Procedures", "*.sql");
            result = GetExecuteFiles(fileNames, moduleName);
            streamWriter.WriteLine(result);
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Nạp cấu trúc sequences...";
            fileNames = Directory.GetFiles(directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\Create Scripts\\Sequences", "*.sql");
            result = GetExecuteFiles(fileNames, moduleName);
            streamWriter.WriteLine(result);
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Nạp dữ liệu test...";
            fileNames = Directory.GetFiles(directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\Generate Data Scripts", "*.sql");
            result = GetExecuteFiles(fileNames, moduleName);
            streamWriter.WriteLine(result);
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Nạp cấu trúc triggers...";
            fileNames = Directory.GetFiles(directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\Create Scripts\\Triggers", "*.sql");
            result = GetExecuteFiles(fileNames, moduleName);
            streamWriter.WriteLine(result);
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Nạp cấu trúc triggers...";
            fileNames = Directory.GetFiles(directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\Change Scripts", "*.sql");
            result = GetExecuteFiles(fileNames, moduleName);
            streamWriter.WriteLine(result);
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Chuẩn bị xong dữ liệu";
            streamWriter.WriteLine("SPO OFF");
            streamWriter.WriteLine("COMMIT;");
            streamWriter.WriteLine("QUIT;");

            streamWriter.Flush();
            streamWriter.Close();
            frmProgress.Instance.Value += 1;
            Thread.Sleep(1000);

            frmProgress.Instance.Description = "Đang đồng bộ dữ liệu test...";

            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.FileName = "SQLPLUS";

            startInfo.Arguments =
                String.Format("qlbh/qlbh@qlbh_local {0}", GetExecuteFiles(new[] { directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\build.sql" }, moduleName));
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process process = new Process();

            process.StartInfo = startInfo;

            process.Start();

            while (!process.HasExited)
            {
                Thread.Sleep(3000);
            }
            frmProgress.Instance.Value += 1;
            File.Delete(directoryInfo.FullName + "\\Oracle\\" + moduleName + "\\build.sql");
            Thread.Sleep(1000);
            frmProgress.Instance.Close();
        }

        private string GetSortName(string path, bool isDir)
        {
            if (path.IndexOf(" ") > 0)
            {
                path = path.Replace(" ", String.Empty);
                if (isDir && path.Length > 8) path = path.Substring(0, 6) + "~1";
            }
            return path;
        }

        private string GetExecuteDir(string path, bool isDir)
        {
            string[] dirNames;
            dirNames = path.Split('\\');
            for (int j = 0; j < dirNames.Length; j++)
            {
                dirNames[j] = GetSortName(dirNames[j], isDir || j < dirNames.Length - 1);
            }
            return String.Join("/", dirNames);
        }

        private string GetExecuteFiles(string[] fileNames, string moduleName)
        {
            string result;
            for (int i = 0; i < fileNames.Length; i++)
            {
                //fileNames[i] = "@../" + GetExecuteDir(fileNames[i].Substring(fileNames[i].IndexOf(moduleName)), false);
                fileNames[i] = "@" + GetExecuteDir(fileNames[i], false);
            }
            result = String.Join("\n", fileNames);
            return result;
        }
    }

    public class TestController
    {
        private System.Windows.Forms.Form testView;
        public TestController(System.Windows.Forms.Form testView)
        {
            this.testView = testView;
            this.testView.ShowInTaskbar = false;
            this.testView.Opacity = 0;
            this.testView.Show();
        }

        public System.Windows.Forms.Form TestView
        {
            get { return testView; }
        }
    }
}
