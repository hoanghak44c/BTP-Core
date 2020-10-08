using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;
using System.Threading;
using CrystalDecisions.Shared;
using System.Runtime.CompilerServices;
using QLBH.Core.Data;
using QLBH.Core.Exceptions;

namespace QLBH.Core.Form
{
    public partial class frmBCBase : DevExpress.XtraEditors.XtraForm
    {
        //protected Utils ut = new Utils();        
        protected ReportClass rpt;
        private ParameterFields myParams;
        private IComponent source;
        private List<frm_rpt> frmCollection = null;
        private string key = String.Empty;
        private bool isRunning = false;
        private Thread boneThread;

        public frmBCBase()
        {
            InitializeComponent();
        }

        private frm_rpt GetReportViewer(string key){
            if (frmCollection == null) return null;
            int i = frmCollection.Count - 1;
            while (i >= 0 && frmCollection.Count > 0 )
            {
                if (frmCollection[i].IsDisposed) {
                    frmCollection.RemoveAt(i);
                    continue;
                }
                if (!frmCollection[i].IsDisposed && String.Equals(frmCollection[i].Text, key)) return frmCollection[i];
                i--;
            }
            return null;
        }

        protected void ShowReport(string sTitle)
        {
            try {
                ShowReport(sTitle, false);
            }
            catch (System.Exception ex) {                
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(ex.Message);
#endif
            }
        }

        protected void ShowReport(string sTitle, bool isBackground){
            try {
                if (isBackground) {
                    if (this.Modal) {
                        throw new InvalidOperationException("Form dialog không thể chạy dưới dạng background được");
                    }
                    if (boneThread == null) {
                        isRunning = true;
                        boneThread = new Thread(new ThreadStart(CloseOnCompleted));
                        boneThread.IsBackground = true;
                        boneThread.Start();
                    }
                    this.Visible = false;
                }
                frm_rpt frm = GetReportViewer(sTitle);
                if (frm == null) {
                    Thread wt = new Thread(new ParameterizedThreadStart(LoadReport));
                    frm = new frm_rpt(sTitle, wt);
                    wt.IsBackground = isBackground;
                    wt.Start(frm);
                    frm.isAsSyns = true;
                    if (frmCollection == null) frmCollection = new List<frm_rpt>();
                    frmCollection.Add(frm);
                    if (isBackground) {
                        frm.CreateBackgroundHandle();
                        if (MainForm != null) (MainForm as IToolStripItem).AddAppBackgroundStatus(frm.GetHashCode(), frm.Text, new EventHandler(frm.onClick));
                    }
                    else {
                        frm.Show();
                    }
                }
                else {
                    frm.Activate();
                }
            }
            catch (InvalidOperationException ex) {
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(ex.Message);
#endif
                this.DialogResult = DialogResult.Ignore;
            }
            catch (System.Exception ex) {
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(ex.Message);
#endif
            }
        }

        delegate void CloseFormDelegate();
        delegate void ChangeReportStripItemStatusDelegate(int key);
        private void ChangeReportStripItemStatus(int key)
        {
            if (MainForm != null) (MainForm as IToolStripItem).ChangeReportStripItemStatus(key);
        }
        delegate void RemoveReportStripItemDelegate(int key);
        private void RemoveReportStripItem(int key)
        {
            if (MainForm != null) (MainForm as IToolStripItem).RemoveReportStripItem(key);
        }

        virtual protected void OnLoadReport()
        {
            throw new Exception("The override of OnLoadReport function is not implemented.");
        }
        private void OnLoadReport(object frm)
        {
            key = (frm as System.Windows.Forms.Form).Text;
            OnLoadReport();
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        private void LoadReport(object frm)
        {
            try {
                OnLoadReport(frm);
                if (source == null) {
                    throw new Exception("Data source is not set");
                }
                if (source.GetType() == typeof(DataSet)) {
                    if ((source as DataSet).Tables.Count == 0) {
                        throw new Exception("Data source is not set");
                    }
                    if ((source as DataSet).Tables[0].Rows.Count == 0) {
                        throw new Exception("Không có dữ liệu báo cáo");
                    }
                }
                else if (source.GetType() == typeof(DataTable)) {
                    if ((source as DataTable).Rows.Count == 0) {
                        throw new Exception("Không có dữ liệu báo cáo");
                    }
                } else if(source is ICollection) {
                    if ((source as ICollection).Count == 0)
                    {
                        throw new Exception("Không có dữ liệu báo cáo");
                    }
                    
                }
                
                rpt.SetDataSource(source);
                (frm as frm_rpt).LoadReport(rpt, myParams);
                if (Thread.CurrentThread.IsBackground) {
                    this.Invoke(new ChangeReportStripItemStatusDelegate(ChangeReportStripItemStatus), frm.GetHashCode());
                    if (this.frmCollection.IndexOf(frm as frm_rpt) == this.frmCollection.Count - 1) isRunning = false;
                }
            }
            catch (System.Exception ex) {
                if (ex.GetBaseException().GetType() != typeof(System.Threading.ThreadAbortException)) {
#if DEBUG
                    MessageBox.Show(ex.ToString());
#else
                    MessageBox.Show(ex.Message);
#endif
                    if (Thread.CurrentThread.IsBackground) {
                        this.Invoke(new RemoveReportStripItemDelegate(RemoveReportStripItem), frm.GetHashCode());
                        if (this.frmCollection.IndexOf(frm as frm_rpt) == this.frmCollection.Count - 1) isRunning = false;
                    }
                    (frm as frm_rpt).CloseMe();
                }
            }
        }

        private object MainForm{
            get{
                if (this.MdiParent != null && this.MdiParent.GetType() == typeof(IToolStripItem))
                {
                    return this.MdiParent;
                }
                else if (this.Owner != null && this.Owner.GetType() == typeof(IToolStripItem))
                {
                    return this.Owner;
                }
                return null;
            }
        }
        private void CloseOnCompleted()
        {
            while (isRunning) {
                Thread.Sleep(1000);
            }
            if (this.InvokeRequired) {
                this.Invoke(new CloseFormDelegate(Close));
            }
            else {
                this.Close();
            }
        }

        virtual protected void OnSetParameterFields(ParameterFields myParams)
        {
            throw new Exception("The override of OnSetParameterFields function is not implemented.");
        }
        private delegate void SetParameterFieldsDelegate(ParameterFields myParams);
        protected void SetParameterFields()
        {
            myParams = new ParameterFields();
            if (this.InvokeRequired) {
                this.Invoke(new SetParameterFieldsDelegate(OnSetParameterFields), new object[] { myParams });
            }
            else {
                OnSetParameterFields(myParams);
            }
        }

        virtual protected void OnSetSqlParameters(GtidParameterCollection sqlParams)
        {
            throw new Exception("The override of OnSetSqlParameters function with parameter collection is not implemented.");
        }
        virtual protected string OnSetSqlParameters(string cmdTextFormatString)
        {
            throw new Exception("The override of OnSetSqlParameters function with command text format string is not implemented.");
        }
        private delegate void SetSqlParameterCollectionDelegate(GtidParameterCollection sqlParams);
        private delegate string SetSqlParameterStringDelegate(string cmdTextFormatString);
        protected void SetSqlParameters(string cmdText, CommandType cmdType, params object[] srcName)
        {
            GtidCommand sqlcmd = new GtidCommand();

            sqlcmd.CommandType = cmdType;
            switch (cmdType) {
                case CommandType.StoredProcedure:
                    if (this.InvokeRequired){
                        this.Invoke(new SetSqlParameterCollectionDelegate(OnSetSqlParameters), new object[] { sqlcmd.Parameters });
                    }
                    else {
                        OnSetSqlParameters(sqlcmd.Parameters);
                    }
                    break;
                case CommandType.Text:
                    if (this.InvokeRequired) {
                        cmdText = (string)this.Invoke(new SetSqlParameterStringDelegate(OnSetSqlParameters), new object[] { cmdText });
                    }
                    else {
                        cmdText = OnSetSqlParameters(cmdText);
                    }
                    break;
            }

            sqlcmd.CommandText = cmdText;
            source = getData(sqlcmd, (string)srcName[0]);
            if (srcName.Length > 1){
                for(int i = 0; i< srcName.Length; i++){
                    if((source as DataSet).Tables.Count > i){
                        (source as DataSet).Tables[i].TableName = (string)srcName[i];
                    }else{
                        throw new Exception(String.Format("Table {0} does not exist.", srcName[i]));
                    }
                }
            }
        }

        virtual protected object OnSetDataSource(){
            throw new Exception("The override of OnSetDataSource function is not implemented.");
        }
        private delegate object SetDataSourceDelegate();
        protected void SetDataSource(){
            if(this.InvokeRequired){
                source = this.Invoke(new SetDataSourceDelegate(OnSetDataSource)) as IComponent;
            }else{
                source = OnSetDataSource() as IComponent;
            }
        }

        protected string Key
        {
            get
            {
                return key;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (frmCollection != null) {
                foreach (frm_rpt frm in frmCollection) {
                    if (!frm.Completed && !frm.IsDisposed) {
                        if (MessageBox.Show(frm, String.Format("{0} đang chạy, bạn có muốn dừng lại không?", frm.Text), "Xác nhận", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3) == DialogResult.Yes) {
                            frm.CloseMe();
                            continue;
                        }else{
                            e.Cancel = true;
                        }
                    }
                }
            }
            base.OnFormClosing(e);
        }

        public static DataSet getData(GtidCommand sql, string TableName)
        {

            GtidDataAdapter adap = new GtidDataAdapter();

            adap.SelectCommand = sql;

            DataSet ds;
            ds = new DataSet();
            try
            {
                adap.Fill(ds, TableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ds;
        }

        protected void SetParameterReport(ParameterFields myParams, string ParamName, object ParamValue)
        {
            ParameterField myParam = new ParameterField();
            ParameterDiscreteValue myDiscreteValue = new ParameterDiscreteValue();
            // Add trung tam
            myParam.Name = ParamName;
            myDiscreteValue.Value = ParamValue;
            myParam.CurrentValues.Add(myDiscreteValue);
            myParams.Add(myParam);

        } 
    }

    public class StorageInfor1
    {
        public int IdKho { get; set; }

        public int IdTrungTam { get; set; }

        public DateTime RunningDate { get; set; }
    }

    public interface IFilter
    {
        string SoPhieu { get; set; }
        DateTime TuNgay { get; set; }
        DateTime DenNgay { get; set; }
        int IdTrungTam { get; set; }
        string DienThoai { get; set; }
    }

    public interface IBcDataProvider<T>
    {
        List<StorageInfor1> GetListComputedStorage(DateTime date, int idTrungTam);
        List<T> Search(IFilter filter);
    }

    public class StorageInfor2
    {
        public int IdKho { get; set; }
        public string Nganh { get; set; }
        public string Loai { get; set; }
        public string Chung { get; set; }
        public string Nhom { get; set; }
        public string Model { get; set; }
        public int IdSanPham { get; set; }
        public int IdPhieuNhap { get; set; }
        public int Amount { get; set; }
        public int Defined { get; set; }

        public override string ToString()
        {
            return
                String.Format(
                    "IdKho:{0}, Nganh:{1}, Loai:{2}, Chung:{3}, Nhom:{4}, Model:{5}, IdSanPham:{6}, IdPhieuNhap:{7}, Amount:{8}",
                    IdKho, Nganh, Loai, Chung, Nhom, Model, IdSanPham, IdPhieuNhap, Amount);
        }
    }

    public interface IFilter2
    {
        int IdTrungTam { get; set; }
        int IdKho { get; set; }
        int IdSanPham { get; set; }
        string Nganh { get; set; }
        string MaVach { get; set; }
    }

    public interface IBcDataProvider2<T>
    {
        List<StorageInfor2> GetListComputedStorage(StorageInfor2 storageInfo, IFilter2 filter, int defined);
        List<T> Search(IFilter2 filter, StorageInfor2 storageInfo);
    }

    public abstract class FrmBcBase2<T>:DevExpress.XtraEditors.XtraForm
    {
        private List<StorageInfor2> computedStorages, computedStorages2;

        private bool isCompletedLoadComputedStorages;

        protected IBcDataProvider2<T> DataProvider;

        protected IList<T> DataSource;

        private IFilter2 filter;

        protected FrmBcBase2()
        {
            DataSource = new List<T>();
        }

        private void LoadComputedStorages()
        {
            isCompletedLoadComputedStorages = false;

            for (var i = 0; i <= 1; i++)
            {
                computedStorages.AddRange(

                    DataProvider.GetListComputedStorage(null, filter, i));

                while (computedStorages.Count > 0)
                {
                    computedStorages2.AddRange(
                        computedStorages.FindAll(delegate(StorageInfor2 match)
                        {
                            return match.Amount <= 500 ||
                                   !String.IsNullOrEmpty(match.Nganh) &&
                                   !String.IsNullOrEmpty(match.Loai) &&
                                   !String.IsNullOrEmpty(match.Chung) &&
                                   !String.IsNullOrEmpty(match.Nhom) &&
                                   !String.IsNullOrEmpty(match.Model) &&
                                   match.IdSanPham > 0 &&
                                   (match.IdPhieuNhap > 0 || i == 0);
                        }));

                    computedStorages.RemoveAll(delegate(StorageInfor2 match)
                    {
                        return match.Amount <= 500 ||
                               !String.IsNullOrEmpty(match.Nganh) &&
                               !String.IsNullOrEmpty(match.Loai) &&
                               !String.IsNullOrEmpty(match.Chung) &&
                               !String.IsNullOrEmpty(match.Nhom) &&
                               !String.IsNullOrEmpty(match.Model) &&
                               match.IdSanPham > 0 &&
                               (match.IdPhieuNhap > 0 || i == 0);
                    });

                    if (computedStorages.Count <= 0) continue;

                    computedStorages.AddRange(

                        DataProvider.GetListComputedStorage(computedStorages[0], filter, i));

                    computedStorages.RemoveAt(0);
                }
                
            }

            isCompletedLoadComputedStorages = true;
        }

        protected abstract void RefreshDataSource();

        private void RefreshView()
        {
            if (InvokeRequired)

                Invoke((MethodInvoker)RefreshDataSource);

            else

                RefreshDataSource();
        }

        protected abstract IFilter2 GetFilter2();

        protected void TimKiemChungTu()
        {
            try
            {
                filter = GetFilter2();

                DataSource.Clear();

                RefreshView();

                computedStorages = new List<StorageInfor2>();

                if (String.IsNullOrEmpty(filter.MaVach))
                {
                    computedStorages = new List<StorageInfor2>();

                    computedStorages2 = new List<StorageInfor2>();

                    var loadComputedStorages = new Thread(LoadComputedStorages);

                    loadComputedStorages.Start();
                }

                frmProgress.Instance.DoWork(LoadDuLieu);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, true);
            }
        }

        private void LoadDuLieu()
        {
            DataSource.Clear();

            RefreshView();

            frmProgress.Instance.Caption = this.Text;

            frmProgress.Instance.Description = "Đang tổng hợp số liệu ...";

            frmProgress.Instance.Value = 0;

            if (String.IsNullOrEmpty(filter.MaVach))
            {
                while (!isCompletedLoadComputedStorages || computedStorages2.Count > 0)
                {
                    while (computedStorages2.Count == 0 && !isCompletedLoadComputedStorages)
                    {
                        Thread.CurrentThread.Join(500);
                    }

                    if (computedStorages2.Count > 0)
                    {
                        ((List<T>)DataSource).AddRange(DataProvider.Search(filter, computedStorages2[0]));

                        RefreshView();

                        computedStorages2.RemoveAt(0);                        
                    }

                    frmProgress.Instance.MaxValue = frmProgress.Instance.Value + computedStorages2.Count + computedStorages.Count;

                    frmProgress.Instance.Value += 1;
                }
            }
            else
            {
                ((List<T>)DataSource).AddRange(DataProvider.Search(filter, null));

                RefreshView();
            }

            frmProgress.Instance.Description = "Đã xong";

            frmProgress.Instance.Value = frmProgress.Instance.MaxValue;

            frmProgress.Instance.IsCompleted = true;
        }
    }

    public abstract class FrmBcBase1<T>:DevExpress.XtraEditors.XtraForm
    {
        private int filterIdTrungTam;

        private DateTime startDate, endDate;
        
        private List<StorageInfor1> computedStorages;
        
        private bool isCompletedLoadComputedStorages;
        
        protected IBcDataProvider<T> DataProvider;
        
        protected IList<T> DataSource;
        
        private IFilter filter;

        protected FrmBcBase1()
        {
            DataSource = new List<T>();
        }

        private void LoadComputedStorages()
        {
            isCompletedLoadComputedStorages = false;

            var runningDate = startDate;

            while (runningDate <= endDate)
            {
                computedStorages.AddRange(

                    DataProvider.GetListComputedStorage(runningDate, filterIdTrungTam));

                runningDate = runningDate.AddDays(1);
            }

            isCompletedLoadComputedStorages = true;
        }

        protected abstract void RefreshDataSource();

        private void RefreshView()
        {
            if (InvokeRequired)

                Invoke((MethodInvoker)RefreshDataSource);

            else

                RefreshDataSource();
        }

        protected virtual bool DivCondition()
        {
            return String.IsNullOrEmpty(filter.SoPhieu) && String.IsNullOrEmpty(filter.DienThoai);
        }

        private void LoadDuLieu()
        {
            DataSource.Clear();

            RefreshView();

            frmProgress.Instance.Caption = this.Text;

            frmProgress.Instance.Description = "Đang tổng hợp số liệu ...";

            frmProgress.Instance.Value = 0;

            if (DivCondition())
            {
                var totalDay = (int)(endDate - startDate).TotalDays;

                var runningDate = startDate;

                frmProgress.Instance.MaxValue = totalDay + 1;

                if (filterIdTrungTam <= 0)
                {
                    while (runningDate <= endDate)
                    {
                        filter.TuNgay = filter.DenNgay = runningDate;

                        while (computedStorages.Count == 0 && !isCompletedLoadComputedStorages)
                        {
                            Thread.CurrentThread.Join(500);
                        }

                        while (computedStorages.FindAll(
                                delegate(StorageInfor1 match)
                                {
                                    return match.RunningDate.Date == runningDate.Date;
                                }).Count > 0)
                        {
                            filter.IdTrungTam = computedStorages[0].IdTrungTam;

                            ((List<T>)DataSource).AddRange(DataProvider.Search(filter));

                            RefreshView();

                            computedStorages.RemoveAt(0);

                            frmProgress.Instance.MaxValue = frmProgress.Instance.Value + computedStorages.Count;

                            frmProgress.Instance.Value += 1;
                        }

                        runningDate = runningDate.AddDays(1);
                    }
                }
                else
                {
                    while (runningDate <= endDate)
                    {
                        filter.TuNgay = filter.DenNgay = runningDate;

                        ((List<T>)DataSource).AddRange(DataProvider.Search(filter));

                        RefreshView();

                        frmProgress.Instance.Value += 1;

                        runningDate = runningDate.AddDays(1);
                    }
                }
            }
            else
            {
                ((List<T>)DataSource).AddRange(DataProvider.Search(filter));

                RefreshView();
            }

            frmProgress.Instance.Description = "Đã xong";

            frmProgress.Instance.Value = frmProgress.Instance.MaxValue;
            
            frmProgress.Instance.IsCompleted = true;
        }

        protected abstract IFilter GetFilter();

        protected void TimKiemChungTu()
        {
            try
            {
                filter = GetFilter();

                filterIdTrungTam = filter.IdTrungTam;
                
                startDate = filter.TuNgay.Date;
                
                endDate = filter.DenNgay.Date;

                DataSource.Clear();

                RefreshView();

                computedStorages = new List<StorageInfor1>();

                if (DivCondition() && filterIdTrungTam <= 0)
                {
                    var loadComputedStorages = new Thread(LoadComputedStorages);

                    loadComputedStorages.Start();
                }

                frmProgress.Instance.DoWork(LoadDuLieu);
            }
            catch (Exception ex)
            {
                throw new ManagedException(ex.Message, true);
            }
        }
    }
}