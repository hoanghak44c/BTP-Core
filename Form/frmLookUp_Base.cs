using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace QLBH.Core.Form
{
    public partial class frmLookUp_Base<T> : DevExpress.XtraEditors.XtraForm
    {
        private DataGridViewCheckBoxColumn colCheckSeleted;

        private bool isMultiSelect;

        protected List<T> ListInitInfo;

        protected List<T> listResult;

        private int oldIndex, oldIndexSearch;

        private string sNewSearch, sOldSearch;
        
        private SortedList<int, string> lstNewTerms, lstOldTerms;

        private string[] lookupTerms;

        private T selectedItem;

        private List<T> selectedItems;

        public frmLookUp_Base()
        {
            InitializeComponent();
            dgvLookUp.AutoGenerateColumns = false;
            sOldSearch = sNewSearch = String.Empty;
            selectedItems = new List<T>();
        }

        public frmLookUp_Base(string searchInput)
        {
            InitializeComponent();
            dgvLookUp.AutoGenerateColumns = false;
            sOldSearch = sNewSearch = searchInput;
            selectedItems = new List<T>();
        }

        public frmLookUp_Base(bool isMultiSelect)
        {
            InitializeComponent();
            dgvLookUp.AutoGenerateColumns = false;
            sOldSearch = sNewSearch = String.Empty;
            selectedItems = new List<T>();
            this.isMultiSelect = isMultiSelect;
            if (isMultiSelect)
            {
                colCheckSeleted = new DataGridViewCheckBoxColumn();
                colCheckSeleted.Width = 40;
                dgvLookUp.Columns.Add(colCheckSeleted);
            }
        }

        public frmLookUp_Base(bool isMultiSelect, string searchInput)
        {
            InitializeComponent();
            dgvLookUp.AutoGenerateColumns = false;
            sOldSearch = sNewSearch = searchInput;
            selectedItems = new List<T>();
            this.isMultiSelect = isMultiSelect;
            if (isMultiSelect)
            {
                colCheckSeleted = new DataGridViewCheckBoxColumn();
                colCheckSeleted.Width = 40;
                dgvLookUp.Columns.Add(colCheckSeleted);
            }
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

            foreach (DataGridViewColumn column in dgvLookUp.Columns)
            {
                if (column.Displayed && !String.IsNullOrEmpty(column.DataPropertyName))
                {
                    propertyInfo = typeof(T).GetProperty(column.DataPropertyName);

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
                if (dgvLookUp.CurrentRow != null)
                {
                    selectedItem = (T)dgvLookUp.CurrentRow.DataBoundItem;
                    this.DialogResult = DialogResult.OK;
                    return;
                }
            }
            else
            {
                foreach (DataGridViewRow dataGridViewRow in dgvLookUp.Rows)
                {
                    if (dataGridViewRow.Cells[0].Value != null && Convert.ToBoolean(dataGridViewRow.Cells[0].Value))
                    {
                        selectedItems.Add((T)dataGridViewRow.DataBoundItem);                        
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
            dgvLookUp.DataBindings.Clear();
            dgvLookUp.DataSource = listResult;
            dgvLookUp.ClearSelection();
            sOldSearch = sNewSearch;
            lstOldTerms = lstNewTerms;
            oldIndexSearch = indexSearch;
            tsStatus.Text = String.Format("Có {0} kết quả được tìm thấy", listResult.Count);
        }

        private void dgvLookUp_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            int len = dgvLookUp.Rows.Count.ToString().Length;
            foreach (DataGridViewRow dataGridViewRow in dgvLookUp.Rows)
            {
                dataGridViewRow.HeaderCell.Value = (dgvLookUp.Rows.IndexOf(dataGridViewRow) + 1).ToString().PadLeft(len, ' ');
            }
        }

        private void txtLookUp_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Down)
            {
                dgvLookUp.Focus();
                if (dgvLookUp.Rows.Count > 0) dgvLookUp.CurrentCell = dgvLookUp.Rows[0].Cells[0];
                if (dgvLookUp.CurrentRow != null) dgvLookUp.CurrentRow.Selected = true;
            }
        }

        private void txtLookUp_KeyPress(object sender, KeyPressEventArgs e)
        {
            //keyChar = e.KeyChar;
        }

        private void txtLookUp_Enter(object sender, EventArgs e)
        {
            dgvLookUp.ClearSelection();
            txtLookUp.SelectionStart = oldIndex;
        }

        private void txtLookUp_Leave(object sender, EventArgs e)
        {
            oldIndex = txtLookUp.SelectionStart;
        }

        void dgvLookUp_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(Regex.IsMatch(e.KeyChar.ToString(), @"[0-9a-zA-Z-!#\$%&'\*\+/=\?\^`\{\}\|~\w\s]", RegexOptions.Singleline))
            {
                txtLookUp.Text = txtLookUp.Text.Insert(oldIndex, Convert.ToString(e.KeyChar));
                oldIndex += 1;
                txtLookUp.Focus();
            }
        }

        private void dgvLookUp_KeyDown(object sender, KeyEventArgs e)
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

        private void dgvLookUp_MouseDoubleClick(object sender, MouseEventArgs e)
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

        void dgvLookUp_CurrentCellDirtyStateChanged(object sender, System.EventArgs e)
        {
            if (dgvLookUp.Columns[dgvLookUp.CurrentCell.ColumnIndex] is DataGridViewCheckBoxColumn)
                if (dgvLookUp.IsCurrentCellDirty) dgvLookUp.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void frmLookUp_Base_Load(object sender, EventArgs e)
        {
            if(!this.DesignMode) OnLoad();
            dgvLookUp.DataSource = listResult = ListInitInfo;
            dgvLookUp.ClearSelection();
            txtLookUp.Text = sNewSearch;
            txtLookUp.Focus();
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
    }
}