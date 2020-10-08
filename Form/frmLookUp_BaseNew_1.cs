using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using DevExpress.Data;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
//using QLBH.Common;

namespace QLBH.Core.Form
{
    public partial class frmLookUp_BaseNew_1<T> : DevExpress.XtraEditors.XtraForm
    {
        private objGridMarkSelection selector = new objGridMarkSelection();

        private GridColumn colCheckSeleted;

        //private RepositoryItemCheckEdit repItemCheckEdit;

        //bool[] checkData;

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
        
        private bool loading, searching;

        public frmLookUp_BaseNew_1()
        {
            InitializeComponent();

            sOldSearch = sNewSearch = String.Empty;
            selectedItems = new List<T>();
            listDisplay = new List<T>();
        }

        public frmLookUp_BaseNew_1(string searchInput)
        {
            InitializeComponent();

            sOldSearch = sNewSearch = searchInput;
            selectedItems = new List<T>();
            listDisplay = new List<T>();
        }

        public frmLookUp_BaseNew_1(bool isMultiSelect)
        {
            InitializeComponent();
            sOldSearch = sNewSearch = String.Empty;
            selectedItems = new List<T>();
            listDisplay = new List<T>();

            this.isMultiSelect = isMultiSelect;

            if (isMultiSelect)
            {

                selector.View = grvLookUp;
                selector.CheckMarkColumn.Fixed = FixedStyle.Left;
                selector.CheckMarkColumn.VisibleIndex = 0;
                selector.ClearSelection();

                //repItemCheckEdit = new RepositoryItemCheckEdit
                //{
                //    ValueChecked = true,
                //    ValueUnchecked = false,
                //};

                //this.grcLookUp.RepositoryItems.Add(this.repItemCheckEdit);

                //colCheckSeleted = grvLookUp.Columns.AddField("chk");
                //colCheckSeleted.VisibleIndex = 0;
                //colCheckSeleted.Width = 40;
                //colCheckSeleted.UnboundType = UnboundColumnType.Boolean;
                //colCheckSeleted.ColumnEdit = repItemCheckEdit;
                //colCheckSeleted.OptionsColumn.AllowEdit = true;
                //grvLookUp.CustomUnboundColumnData += grvLookUp_CustomUnboundColumnData;
                //grvLookUp.CellValueChanged += grvLookUp_CellValueChanged;
            }
        }

        public frmLookUp_BaseNew_1(bool isMultiSelect, string searchInput)
        {
            InitializeComponent();
            sOldSearch = sNewSearch = searchInput;
            selectedItems = new List<T>();
            listDisplay = new List<T>();

            this.isMultiSelect = isMultiSelect;
            
            if (isMultiSelect)
            {
                selector.View = grvLookUp;
                selector.CheckMarkColumn.Fixed = FixedStyle.Left;
                selector.CheckMarkColumn.VisibleIndex = 0;
                selector.ClearSelection();

                //repItemCheckEdit = new RepositoryItemCheckEdit
                //{
                //    ValueChecked = true,
                //    ValueUnchecked = false,
                //};
                
                //this.grcLookUp.RepositoryItems.Add(this.repItemCheckEdit);

                //colCheckSeleted = grvLookUp.Columns.AddField("chk");
                //colCheckSeleted.VisibleIndex = 0;
                //colCheckSeleted.Width = 40;
                //colCheckSeleted.UnboundType = UnboundColumnType.Boolean;
                //colCheckSeleted.ColumnEdit = repItemCheckEdit;
                //repItemCheckEdit.Click += new EventHandler(repItemCheckEdit_Click);
                //colCheckSeleted.OptionsColumn.AllowEdit = true;
                //grvLookUp.CustomUnboundColumnData += grvLookUp_CustomUnboundColumnData;
                //grvLookUp.CellValueChanged += grvLookUp_CellValueChanged;
            }
        }

        //void repItemCheckEdit_Click(object sender, EventArgs e)
        //{
        //    repItemCheckEdit.BeginUpdate();
        //    grvLookUp.SetRowCellValue(grvLookUp.FocusedRowHandle, colCheckSeleted, !checkData[grvLookUp.FocusedRowHandle]);
        //    repItemCheckEdit.EndUpdate();
        //}

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
                for (int handle = 0; handle < selector.selection.Count; handle++)
                {
                    selectedItems.Add((T)selector.selection[handle]);
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

        private void Search(object expression)
        {
            if (searching) return;

            searching = true;

            //string searchTerm = GetSearchTerm().ToLower();

            string searchTerm = Convert.ToString(expression).ToLower();

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
                if (lstOldTerms != null)
                {
                    if (lstOldTerms.Count > lstNewTerms.Count)
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
                else if (lstNewTerms.Count > 0)
                {
                    if (indexSearch == -1) indexSearch = lstNewTerms.Keys[lstNewTerms.Count - 1];

                    compareOperate = new ContainsOperation(lstNewTerms[indexSearch]);
                    listResult = ContainsOptimizedList.FindAll(Match);
                }
            }

            T[] arrTemp = new T[pageSize];

            listResult.CopyTo(0, arrTemp, 0, listResult.Count < pageSize ? listResult.Count : pageSize);
            pageIndex = 0;
            pageTotal = listResult.Count / pageSize + (listResult.Count % pageSize > 0 ? 1 : 0);

            listDisplay.Clear();

            foreach (var item in arrTemp)
            {
                if (item != null)
                    listDisplay.Add(item);
            }

            Invoke((MethodInvoker)
                   delegate()
                   {
                       grcLookUp.RefreshDataSource();
                       tsStatus.Text = String.Format("Có {0} kết quả được tìm thấy", listResult.Count);
                   });

            sOldSearch = sNewSearch;
            lstOldTerms = lstNewTerms;
            oldIndexSearch = indexSearch;

            searching = false;

            bool isRemain = false;

            Invoke((MethodInvoker)
                   delegate()
                   {
                       if (searchTerm != txtLookUp.Text.ToLower())
                       {
                           isRemain = true;
                           searchTerm = txtLookUp.Text;
                       }
                   });

            if(isRemain)  Search(searchTerm);
        }

        private void txtLookUp_TextChanged(object sender, EventArgs e)
        {
            var searchThread = new Thread(Search);

            searchThread.Start(txtLookUp.Text); 

            //Search(txtLookUp.Text);
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
            txtLookUp.SelectionStart = oldIndex == -1 ? 0 : oldIndex;
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
                //checkData = new bool[listResult.Count];

                T[] arrTemp = new T[pageSize];

                listResult.CopyTo(0, arrTemp, 0, listResult.Count < pageSize ? listResult.Count : pageSize);
                pageIndex = 0;
                pageTotal = listResult.Count / pageSize + (listResult.Count % pageSize > 0 ? 1 : 0);

                foreach (var item in arrTemp)
                {
                    if (item != null)
                        listDisplay.Add(item);
                }

                grcLookUp.DataSource = listDisplay;

                //grcLookUp.DataSource = listResult;
                grvLookUp.BestFitColumns();
                txtLookUp.Text = sNewSearch;
                txtLookUp.Focus();
            }
        }

        //void grvLookUp_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e)
        //{
        //    if (e.IsGetData)
        //        e.Value = checkData[e.ListSourceRowIndex];
        //    else
        //        checkData[e.ListSourceRowIndex] = (bool) e.Value;

        //}

        //void grvLookUp_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        //{
        //    if(e.Column == colCheckSeleted)
        //    {
        //        checkData[e.RowHandle] = (bool) e.Value;
        //    }
        //}

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
            GridViewInfo info = grvLookUp.GetViewInfo() as GridViewInfo;

            rowNums = info.RowsInfo.Count;

            Debug.Print(grvLookUp.TopRowIndex.ToString());

            var workerThread = new Thread(FetchData);

            workerThread.Start(grvLookUp.TopRowIndex);

        }

        void FetchData(object topIndex)
        {
            int step = pageSize;

            if (loading || pageIndex + 1 >= pageTotal) return;

            if (Convert.ToInt32(topIndex) + rowNums + step < (pageIndex+1)*pageSize) return;

            loading = true;

            T[] arrTemp = new T[pageSize];

            int offset = (pageIndex + 1)*pageSize;

            listResult.CopyTo(offset, arrTemp, 0, offset + pageSize > listResult.Count ? listResult.Count - listDisplay.Count : pageSize);

            pageIndex += 1;

            for (int i = 0; i < arrTemp.Length; i++)
            {
                if (arrTemp[i] != null)
                    listDisplay.Add(arrTemp[i]);
            }

            Invoke((MethodInvoker)
                   delegate()
                   {
                       grcLookUp.RefreshDataSource();
                   });

            loading = false;
        }

        private void grvLookUp_ColumnFilterChanged(object sender, EventArgs e)
        {
            string expression = String.Empty;

            foreach (GridColumn column in grvLookUp.Columns)
            {
                if(column.FilterInfo.Value != null)
                {
                    expression += "%" + column.FilterInfo.Value.ToString().TrimStart('%');
                }
            }
            expression += String.IsNullOrEmpty(expression) ? String.Empty : "%";

            if(String.IsNullOrEmpty(expression))
            {
                var searchThread = new Thread(Search);

                searchThread.Start(txtLookUp.Text); 
            }
            else
            {
                var searchThread = new Thread(Search);

                searchThread.Start(expression);
            }
        }
    }
    internal class objGridMarkSelection
    {
        protected GridView view;
        // Use a binding list so we get a live list of selected objects
        public BindingList<object> selection = new BindingList<object>();
        private GridColumn column;
        private RepositoryItemCheckEdit edit;
        private int row;

        public objGridMarkSelection()
            : base()
        {
        }

        public objGridMarkSelection(GridView view)
            : this()
        {
            View = view;
        }
        #region Actions

        protected virtual void Attach(GridView view)
        {
            if (view == null) return;
            selection.Clear();
            this.view = view;
            edit = view.GridControl.RepositoryItems.Add("CheckEdit") as RepositoryItemCheckEdit;
            edit.EditValueChanged += new EventHandler(edit_EditValueChanged);

            column = view.Columns.Add();
            column.OptionsColumn.AllowSort = DevExpress.Utils.DefaultBoolean.False;
            column.VisibleIndex = 0;
            //column.Fixed = FixedStyle.Left;
            column.FieldName = "CheckMarkSelection";
            column.Caption = " ";
            column.OptionsColumn.ShowCaption = true;
            column.OptionsColumn.FixedWidth = true;
            column.UnboundType = DevExpress.Data.UnboundColumnType.Boolean;
            column.ColumnEdit = edit;
            column.Width = 50;

            view.Click += new EventHandler(View_Click);
            //view.CustomDrawColumnHeader += new ColumnHeaderCustomDrawEventHandler(View_CustomDrawColumnHeader);
            view.CustomDrawGroupRow += new RowObjectCustomDrawEventHandler(View_CustomDrawGroupRow);
            view.CustomUnboundColumnData += new CustomColumnDataEventHandler(view_CustomUnboundColumnData);
            view.RowStyle += new RowStyleEventHandler(view_RowStyle);
            view.RowClick += new RowClickEventHandler(view_RowClick);
        }



        protected virtual void Detach()
        {
            if (view == null) return;
            if (column != null)
                column.Dispose();
            if (edit != null)
            {
                try
                {
                    view.GridControl.RepositoryItems.Remove(edit);
                    edit.Dispose();
                }
                catch { }
            }

            view.Click -= new EventHandler(View_Click);
            //view.CustomDrawColumnHeader -= new ColumnHeaderCustomDrawEventHandler(View_CustomDrawColumnHeader);
            view.CustomDrawGroupRow -= new RowObjectCustomDrawEventHandler(View_CustomDrawGroupRow);
            view.CustomUnboundColumnData -= new CustomColumnDataEventHandler(view_CustomUnboundColumnData);
            view.RowStyle -= new RowStyleEventHandler(view_RowStyle);
            view.RowClick += new RowClickEventHandler(view_RowClick);
            view = null;
        }

        protected void DrawCheckBox(Graphics g, Rectangle r, bool Checked)
        {
            DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo info;
            DevExpress.XtraEditors.Drawing.CheckEditPainter painter;
            DevExpress.XtraEditors.Drawing.ControlGraphicsInfoArgs args;
            info = edit.CreateViewInfo() as DevExpress.XtraEditors.ViewInfo.CheckEditViewInfo;
            painter = edit.CreatePainter() as DevExpress.XtraEditors.Drawing.CheckEditPainter;
            info.EditValue = Checked;
            info.Bounds = r;
            info.CalcViewInfo(g);
            args = new DevExpress.XtraEditors.Drawing.ControlGraphicsInfoArgs(info, new DevExpress.Utils.Drawing.GraphicsCache(g), r);
            painter.Draw(args);
            args.Cache.Dispose();
        }



        public GridView View
        {
            get
            {
                return view;
            }
            set
            {
                if (view != value)
                {
                    Detach();
                    Attach(value);
                }
            }
        }

        public GridColumn CheckMarkColumn
        {
            get
            {
                return column;
            }

        }

        public int SelectedCount
        {
            get
            {
                return selection.Count;
            }
        }

        public void RemoveSelect()
        {
            selection.RemoveAt(row);
        }

        public int SelectRowIndex
        {
            get
            {
                return row;
            }
        }

        public object GetSelectedRow(int index)
        {
            return selection[index];
        }

        public int GetSelectedIndex(object row)
        {
            return selection.IndexOf(row);
        }

        public void ClearSelection()
        {
            selection.Clear();
            Invalidate();
        }

        private void Invalidate()
        {
            view.BeginUpdate();
            view.EndUpdate();
        }

        public void SelectAll()
        {
            selection.Clear();
            if (view.DataRowCount > 0)
            {
                for (int i = 0; i < view.DataRowCount; i++)  // slow
                    selection.Add(view.GetRow(i));
                Invalidate();
            }
        }

        public void SelectGroup(int rowHandle, bool select)
        {
            if (IsGroupRowSelected(rowHandle) && select) return;
            for (int i = 0; i < view.GetChildRowCount(rowHandle); i++)
            {
                int childRowHandle = view.GetChildRowHandle(rowHandle, i);
                if (view.IsGroupRow(childRowHandle))
                    SelectGroup(childRowHandle, select);
                else
                    SelectRow(childRowHandle, select, false);
            }
            Invalidate();
        }

        public void SelectRow(int rowHandle, bool select)
        {
            SelectRow(rowHandle, select, true);
        }

        private void SelectRow(int rowHandle, bool select, bool invalidate)
        {
            if (IsRowSelected(rowHandle) == select) return;
            object row = view.GetRow(rowHandle);
            if (select)
                selection.Add(row);
            else
                selection.Remove(row);

            if (invalidate)
            {
                Invalidate();
            }
        }

        public bool IsGroupRowSelected(int rowHandle)
        {
            for (int i = 0; i < view.GetChildRowCount(rowHandle); i++)
            {
                int row = view.GetChildRowHandle(rowHandle, i);
                if (view.IsGroupRow(row))
                {
                    if (!IsGroupRowSelected(row)) return false;
                }
                else
                    if (!IsRowSelected(row)) return false;
            }
            return true;
        }

        public bool IsRowSelected(int rowHandle)
        {
            if (view.IsGroupRow(rowHandle))
                return IsGroupRowSelected(rowHandle);

            object row = view.GetRow(rowHandle);
            return GetSelectedIndex(row) != -1;
        }

        #endregion

        #region Events

        private void view_CustomUnboundColumnData(object sender, CustomColumnDataEventArgs e)
        {
            if (e.Column == CheckMarkColumn)
            {
                if (e.IsGetData)
                    e.Value = IsRowSelected(e.RowHandle);
                else
                    SelectRow(e.RowHandle, (bool)e.Value);
            }
        }

        private void edit_EditValueChanged(object sender, EventArgs e)
        {
            view.PostEditor();
        }

        private void View_Click(object sender, EventArgs e)
        {
            //row = e.RowHandle;
            GridHitInfo info;
            Point pt = view.GridControl.PointToClient(Control.MousePosition);
            info = view.CalcHitInfo(pt);
            if (info.InColumn && info.Column == column)
            {
                if (SelectedCount == view.DataRowCount)
                    ClearSelection();
                else
                    SelectAll();
            }
            if (info.InRow && view.IsGroupRow(info.RowHandle) && info.HitTest != GridHitTest.RowGroupButton)
            {
                bool selected = IsGroupRowSelected(info.RowHandle);
                SelectGroup(info.RowHandle, !selected);
            }
        }

        //private void View_CustomDrawColumnHeader(object sender, ColumnHeaderCustomDrawEventArgs e)
        //{
        //    if (e.Column == column)
        //    {
        //        e.Info.InnerElements.Clear();
        //        e.Painter.DrawObject(e.Info);
        //        DrawCheckBox(e.Graphics, e.Bounds, SelectedCount == view.DataRowCount);
        //        e.Handled = true;
        //    }
        //}

        private void View_CustomDrawGroupRow(object sender, RowObjectCustomDrawEventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.ViewInfo.GridGroupRowInfo info;
            info = e.Info as DevExpress.XtraGrid.Views.Grid.ViewInfo.GridGroupRowInfo;

            info.GroupText = "         " + info.GroupText.TrimStart();
            e.Info.Paint.FillRectangle(e.Graphics, e.Appearance.GetBackBrush(e.Cache), e.Bounds);
            e.Painter.DrawObject(e.Info);

            Rectangle r = info.ButtonBounds;
            r.Offset(r.Width * 2, 0);
            DrawCheckBox(e.Graphics, r, IsGroupRowSelected(e.RowHandle));
            e.Handled = true;
        }

        void view_RowStyle(object sender, RowStyleEventArgs e)
        {

            if (IsRowSelected(e.RowHandle))
            {
                //  e.Appearance.BackColor = Color.SkyBlue;
                // e.Appearance.ForeColor = SystemColors.HighlightText;
            }
        }

        void view_RowClick(object sender, RowClickEventArgs e)
        {
            row = e.RowHandle;
        }

        #endregion
    }
}