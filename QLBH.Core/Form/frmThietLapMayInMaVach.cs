using System;
using System.Configuration;
using System.Windows.Forms;

namespace QLBH.Core.Form
{
    public partial class frmThietLapMayInMaVach : DevExpress.XtraEditors.XtraForm
    {
        public frmThietLapMayInMaVach(string printerName)
        {
            InitializeComponent();

            txtConfig.Text = printerName;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //var fileMap = new ExeConfigurationFileMap();

            //fileMap.ExeConfigFilename = Application.StartupPath + String.Format("\\{0}.config", Process.GetCurrentProcess().MainModule.ModuleName);

            //var configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            //configuration.AppSettings.Settings["BarCodePrinter"].Value = String.Empty;

            //configuration.Save();

            if (String.IsNullOrEmpty(txtConfig.Text.Trim())) return;

            ConfigurationManager.AppSettings.Set("BarCodePrinter", txtConfig.Text.Trim());

            this.DialogResult = DialogResult.OK;
        }

        public string ConfigValue
        {
            get { return txtConfig.Text.Trim(); }
        }
    }
}