using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

namespace QLBH.Core.Form
{
    public partial class frmLookUp_BaseNew<T> : DevExpress.XtraEditors.XtraForm
    {
        private GridColumn colCheckSeleted;

        private RepositoryItemCheckEdit repItemCheckEdit;

        bool[] checkData;

        private bool isMultiSelect;

        protected List<T> ListInitInfo;

        protected List<T> listResult;

        private List<T> listDisplay;

        private int oldIndex, oldIndexSearch;

        private string sNewSearch, sOldSearch;
        
        private SortedList<int, string> lstNewTerms, lstOldTerms;

        private string[] lookupTerms;

        private T selectedItem;

        private List<T> selectedItems;

        private int rowNums, pageTotal, pageIndex;

        private int pageSize = 50;
        
        private bool loading;



        public frmLookUp_BaseNew()
        {
            InitializeComponent();
            sOldSearch = sNewSearch = String.Empty;
            selectedItems = new List<T>();
        }

        public frmLookUp_BaseNew(string searchInput)
        {
            InitializeComponent();
            sOldSearch = sNewSearch = searchInput;
            selectedItems = new List<T>();
        }

        public frmLookUp_BaseNew(bool isMultiSelect)
        {
            InitializeComponent();
            sOldSearch = sNewSearch = String.Empty;
            selectedItems = new List<T>();
            this.isMultiSelect = isMultiSelect;

            if (isMultiSelect)
            {
                repItemCheckEdit = new RepositoryItemCheckEdit
                {
                    ValueChecked = true,
                    ValueUnchecked = false,
                };

                this.grcLookUp.RepositoryItems.Add(this.repItemCheckEdit);

                colCheckSeleted = grvLookUp.Columns.AddField("chk");
                colCheckSeleted.VisibleIndex = 0;
                colCheckSeleted.Width = 40;
                colCheckSeleted.UnboundType = UnboundColumnType.Boolean;
                colCheckSeleted.ColumnEdit = repItemCheckEdit;
                colCheckSeleted.OptionsColumn.AllowEdit = true;
                grvLookUp.CustomUnboundColumnData += grvLookUp_CustomUnboundColumnData;
                grvLookUp.CellValueChanged += grvLookUp_CellValueChanged;
            }
        }

        public frmLookUp_BaseNew(bool isMultiSelect, string searchInput)
        {
            InitializeComponent();
            sOldSearch = sNewSearch = searchInput;
            selectedItems = new List<T>();
            this.isMultiSelect = isMultiSelect;
            
            if (isMultiSelect)
            {
                repItemCheckEdit = new RepositoryItemCheckEdit
                {
                    ValueChecked = true,
                    ValueUnchecked = false,
                };
                
                this.grcLookUp.RepositoryItems.Add(this.repItemCheckEdit);

                colCheckSeleted = grvLookUp.Columns.AddField("chk");
                colCheckSeleted.VisibleIndex = 0;
                colCheckSeleted.Width = 40;
                colCheckSeleted.UnboundType = UnboundColumnType.Boolean;
                colCheckSeleted.ColumnEdit = repItemCheckEdit;
                repItemCheckEdit.Click += new EventHandler(repItemCheckEdit_Click);
                colCheckSeleted.OptionsColumn.AllowEdit = true;
                grvLookUp.CustomUnboundColumnData += grvLookUp_CustomUnboundColumnData;
                grvLookUp.CellValueChanged += grvLookUp_CellValueChanged;
            }
        }

        void repItemCheckEdit_Click(object sender, EventArgs e)
        {
            repItemCheckEdit.BeginUpdate();
            grvLookUp.SetRowCellValue(grvLookUp.FocusedRowHandle, colCheckSeleted, !checkData[grvLookUp.FocusedRowHandle]);
            repItemCheckEdit.EndUpdate();
        }

        public T SelectedItem
        {
            get { return selectedItem; }
        }

        public List<T> SelectedItems
        {
            get { return selectedItems; }
        }

        private List<T> ContainsOptimizedList
        {
            get
            {
                if (!sNewSearch.Equals(sOldSearch) && sNewSearch.Contains(sOldSearch)) return listResult;
                return ReSearch();
            }
        }

        private List<T> EndsOptimizedList
        {
            get
            {
                if (!sNewSearch.Equals(sOldSearch) && sNewSearch.EndsWith(sOldSearch)) return listResult;
                return ReSearch();
            }
        }

        private List<T> StartsOptimizedList
        {
            get
            {
                if (!sNewSearch.Equals(sOldSearch) && sNewSearch.StartsWith(sOldSearch)) return listResult;
                return ReSearch();
            }
        }

        private List<T> ReSearch()
        {
            listResult = ListInitInfo;
            foreach (var lstNewTerm in lstNewTerms)
            {
                if (lstNewTerm.Key == 0)
                {
                    compareOperate = new StartsOperation(lstNewTerm.Value);
                    listResult = listResult.FindAll(Match);
                }
                else if (lstNewTerm.Key == lookupTerms.Length - 1)
                {
                    compareOperate = new EndsOperation(lstNewTerm.Value);
                    listResult = listResult.FindAll(Match);
                }
                else
                {
                    compareOperate = new ContainsOperation(lstNewTerm.Value);
                    listResult = listResult.FindAll(Match);
                }
            }

            return listResult;
        }

        private abstract class CompOpBase
        {
            protected readonly string Spec;
            protected CompOpBase(string spec)
            {
                this.Spec = spec;
            }

            public abstract bool isSatisfy(string input);

        }
        
        private class ContainsOperation : CompOpBase
        {
            public ContainsOperation(string spec) : base(spec){}
            public override bool isSatisfy(string input)
            {
                return input.Contains(Spec);
            }
        }

        private class StartsOperation : CompOpBase
        {
            public StartsOperation(string spec) : base(spec) { }
            public override bool isSatisfy(string input)
            {
                return input.StartsWith(Spec);
            }
        }

        private class EndsOperation : CompOpBase
        {
            public EndsOperation(string spec) : base(spec) { }
            public override bool isSatisfy(string input)
            {
                return input.EndsWith(Spec);
            }
        }

        private class EqualsOperation : CompOpBase
        {
            public EqualsOperation(string spec) : base(spec) { }
            public override bool isSatisfy(string input)
            {
                return input.Equals(Spec);
            }
        }

        private bool lookUp(T match, CompOpBase lookUpOperation)
        {
            PropertyInfo propertyInfo;

            foreach (GridColumn column in grvLookUp.Columns)
            {
                if (column.Visible && !String.IsNullOrEmpty(column.FieldName))
                {
                    propertyInfo = typeof(T).GetProperty(column.FieldName);

                    if(propertyInfo != null)
                    {
                        object value = propertyInfo.GetValue(match, null);

                        if (value != null)
                        {
                            if (lookUpOperation.isSatisfy(value.ToString().ToLower())) return true;
                        }                        
                    }
                }
            }

            foreach (var lookUpPropertyName in LookUpPropertyNames())
            {
                propertyInfo = typeof (T).GetProperty(lookUpPropertyName);
                
                if(propertyInfo == null) throw new ArgumentException(String.Format("Không có thuộc tính {0}", lookUpPropertyName));
                
                object value = propertyInfo.GetValue(match, null);
                if (value != null)
                {
                    if (lookUpOperation.isSatisfy(value.ToString().ToLower())) return true;
                }                
            }

            return false;
        }

        /// <summary>
        /// Dùng để look up các thuộc tính khác ngoài các thuộc tính được hiển thị
        /// </summary>
        /// <returns></returns>
        protected virtual string[] LookUpPropertyNames()
        {
            return new string[]{};
        }

        protected virtual void OnLoad()
        {
            throw new NotImplementedException("Chức năng này chưa được thực hiện.");
        }

        private void SelectItem()
        {
            selectedItem = default(T);
            selectedItems.Clear();
            if (!isMultiSelect)
            {
                if (grvLookUp.GetRow(grvLookUp.FocusedRowHandle) != null)
                {
                    selectedItem = (T)grvLookUp.GetRow(grvLookUp.FocusedRowHandle);
                    this.DialogResult = DialogResult.OK;
                    return;
                }
            }
            else
            {
                for (int handle = 0; handle < grvLookUp.RowCount; handle++ )
                {
                    if(Convert.ToBoolean(grvLookUp.GetRowCellValue(handle, colCheckSeleted)))
                    {
                        selectedItems.Add((T)grvLookUp.GetRow(handle));
                    }
                }

                this.DialogResult = DialogResult.OK;
                
                return;
            }
            
            DialogResult = DialogResult.Cancel;
        }
        
        private CompOpBase compareOperate = null;
        
        private bool Match(T match)
        {
            return lookUp(match, compareOperate);
        }

        protected virtual string GetSearchTerm()
        {
            return txtLookUp.Text ?? String.Empty;
        }

        private void txtLookUp_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = GetSearchTerm().ToLower();
            
            string sTemp = String.Empty;
            
            lstNewTerms = new SortedList<int, string>();
            
            lookupTerms = searchTerm.Split("%".ToCharArray()[0]);
            
            int indexSearch = -1;

            if (listResult == null) listResult = ListInitInfo;

            for (int i = 0; i < lookupTerms.Length; i++)
            {
                if (!String.IsNullOrEmpty(lookupTerms[i]))
                {
                    if (!lstNewTerms.ContainsValue(lookupTerms[i])) lstNewTerms.Add(i, lookupTerms[i]);
                }
            }

            if (lookupTerms.Length == 1)
            {
                if (!String.IsNullOrEmpty(lookupTerms[0]))
                {
                    compareOperate = new EqualsOperation(lookupTerms[0]);
                    sNewSearch = lookupTerms[0];
                    listResult = ListInitInfo.FindAll(Match);                    
                } 
                else
                {
                    listResult = ListInitInfo;
                }
                indexSearch = 0;
            }
            else
            {
                if(lstOldTerms != null)
                {
                    if(lstOldTerms.Count > lstNewTerms.Count)
                    {
                        listResult = ReSearch();
                        indexSearch = -1;
                    }
                    else
                    {
                        foreach (var lstNewTerm in lstNewTerms)
                        {
                            if (!lstOldTerms.TryGetValue(lstNewTerm.Key, out sTemp) || sTemp != lstNewTerm.Value)
                            {
                                sOldSearch = sTemp ?? String.Empty;
                                sNewSearch = lstNewTerm.Value;
                                indexSearch = lstNewTerm.Key;
                                break;
                            }
                        }                        
                    }
                }
                else
                {
                    listResult = ReSearch();
                    indexSearch = -1;
                }

                if (indexSearch == 0)
                {
                    compareOperate = new StartsOperation(lstNewTerms[indexSearch]);
                    listResult = StartsOptimizedList.FindAll(Match);
                }
                else if (indexSearch == lookupTerms.Length - 1)
                {
                    compareOperate = new EndsOperation(lstNewTerms[indexSearch]);
                    listResult = EndsOptimizedList.FindAll(Match);
                }
                else if (indexSearch > -1)
                {
                    compareOperate = new ContainsOperation(lstNewTerms[indexSearch]);
                    listResult = ContainsOptimizedList.FindAll(Match);
                }
            }

            //T[] arrTemp = new T[pageSize];

            //listResult.CopyTo(0, arrTemp, 0, pageSize);

            //listDisplay = new List<T>();
            //listDisplay.AddRange(arrTemp);

            grcLookUp.DataSource = listResult;
            sOldSearch = sNewSearch;
            lstOldTerms = lstNewTerms;
            oldIndexSearch = indexSearch;
            tsStatus.Text = String.Format("Có {0} kết quả được tìm thấy", listResult.Count);
        }

        private void txtLookUp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                grvLookUp.Focus();
            }
        }

        private void txtLookUp_KeyPress(object sender, KeyPressEventArgs e)
        {
            //keyChar = e.KeyChar;
        }

        private void txtLookUp_Enter(object sender, EventArgs e)
        {
            txtLookUp.SelectionStart = oldIndex;
        }

        private void txtLookUp_Leave(object sender, EventArgs e)
        {
            oldIndex = txtLookUp.SelectionStart;
        }

        private void frmLookUp_Base_Load(object sender, EventArgs e)
        {
            if(!this.DesignMode)
            {
                OnLoad();
                listResult = ListInitInfo;
                checkData = new bool[listResult.Count];
                
                //T[] arrTemp = new T[pageSize];

                //listResult.CopyTo( 0, arrTemp, 0, pageSize);

                //listDisplay = new List<T>();
                //listDisplay.AddRange(arrTemp);

                //grcLookUp.DataSource = listDisplay;

                grcLookUp.DataSource = listResult;
                grvLookUp.BestFitColumns();
                txtLookUp.Text = sNewSearch;
                txtLookUp.Focus();
            }
        }

        void grvLookUp_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        {
            if (e.IsGetData)
                e.Value = checkData[e.ListSourceRowIndex];
            else
                checkData[e.ListSourceRowIndex] = (bool) e.Value;

        }

        void grvLookUp_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if(e.Column == colCheckSeleted)
            {
                checkData[e.RowHandle] = (bool) e.Value;
            }
        }

        void frmLookUp_Base_Activated(object sender, EventArgs e)
        {
            if(String.IsNullOrEmpty(txtLookUp.Text)) txtLookUp.Text = sNewSearch;
            txtLookUp.Focus();
            if (!String.IsNullOrEmpty(txtLookUp.Text)) txtLookUp.SelectionStart = txtLookUp.Text.Length - 1;
        }

        private void frmLookUp_Base_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
                selectedItem = default(T);
                selectedItems.Clear();
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void frmLookUp_Base_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Debug.Print(((int)e.KeyChar).ToString());
            ////SendKeys.Send(e.KeyChar.ToString());
        }

        private void grvLookUp_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Regex.IsMatch(e.KeyChar.ToString(), @"[0-9a-zA-Z-!#\$%&'\*\+/=\?\^`\{\}\|~\w\s]", RegexOptions.Singleline))
            {
                txtLookUp.Text = txtLookUp.Text.Insert(oldIndex, Convert.ToString(e.KeyChar));
                oldIndex += 1;
                txtLookUp.Focus();
            }
        }

        private void grvLookUp_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    SelectItem();
                    e.Handled = true;
                    return;
                case Keys.Delete:
                    txtLookUp.Text = txtLookUp.Text.Remove(oldIndex, 1);
                    txtLookUp.Focus();
                    return;
                case Keys.Back:
                    if (oldIndex > 0) txtLookUp.Text = txtLookUp.Text.Remove(oldIndex - 1, 1);
                    oldIndex -= 1;
                    txtLookUp.Focus();
                    return;
            }
        }

        private void grvLookUp_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                SelectItem();
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString(), "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
#else
                MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
#endif
            }
        }

        private void grvLookUp_TopRowChanged(object sender, EventArgs e)
        {
            //GridViewInfo info = grvLookUp.GetViewInfo() as GridViewInfo;

            //rowNums = info.RowsInfo.Count;

            //Debug.Print(grvLookUp.TopRowIndex.ToString());

            //var workerThread = new Thread(FetchData);

            //workerThread.Start(grvLookUp.TopRowIndex);

        }

        void FetchData(object topIndex)
        {
            //int max = 0, step = pageSize;

            //if (loading || pageIndex >= pageTotal) return;

            //if (Convert.ToInt32(topIndex) + rowNums + step < (list.Count / (pageIndex + 1))) return;

            //pageIndex += 1;

            //loading = true;

            //List<KhoThongKeHangTonInfo> listTmp =
            //    KhoThongKeHangTonDataProvider.Instance.GetListThongKeHangTonKho3(
            //        bteKho.Text, String.Empty, fromDate, toDate, pageIndex, pageSize, Declare.UserId);

            //for (int i = 0; i < listTmp.Count; i++)
            //{
            //    list.Add(listTmp[i]);
            //}

            //Invoke((MethodInvoker)
            //       delegate()
            //       {
            //           grcLookUp.RefreshDataSource();
            //       });

            //loading = false;
        }
    }
}