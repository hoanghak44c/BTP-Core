using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using DevExpress.Utils;
using DevExpress.Utils.Design;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Registrator;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.Drawing;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using QLBH.Core.Data;
using DevExpress.XtraGrid.Views.Base;

namespace QLBH.Core.Form
{

    [Description("Raises an event when a user clicks it.")]
    [ToolboxTabName("GTID: Common Controls")]
    [ToolboxItem(true)]
    [Designer(typeof(BaseControlDesigner))]
    [ToolboxBitmap(typeof(BaseEdit), "Bitmaps256.SimpleButton.bmp")]
    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    public class GtidSimpleButton : SimpleButton
    {
        private Keys shortCutKey;
        public Keys ShortCutKey
        {
            get { return shortCutKey; }
            set { shortCutKey = value; }
        }

        protected override void OnClick(System.EventArgs e)
        {
            try
            {
                base.OnClick(e);
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(ex.Message);
#endif
            }
        }

        //protected override void WndProc(ref Message m)
        //{
        //    const int WM_NCHITTEST = 0x0084;
        //    const int HTTRANSPARENT = (-1);

        //    if (m.Msg == WM_NCHITTEST)
        //    {
        //        m.Result = (IntPtr)HTTRANSPARENT;
        //    }
        //    else
        //    {
        //        base.WndProc(ref m);
        //    }
        //}
    }

    [Description("Raises an event when a user clicks it.")]
    [ToolboxTabName("GTID: Common Controls")]
    [ToolboxItem(true)]
    [Designer(typeof(BaseControlDesigner))]
    [ToolboxBitmap(typeof(BaseEdit), "Bitmaps256.SimpleButton.bmp")]
    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    public class GtidButton : GtidSimpleButton
    {
        public bool UseVisualStyleBackColor;
        public System.Drawing.ContentAlignment ImageAlign;
        public System.Drawing.ContentAlignment TextAlign;
        protected override void OnClick(System.EventArgs e)
        {
            try
            {
                base.OnClick(e);
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(ex.Message);
#endif
            }
        }

        //protected override void WndProc(ref Message m)
        //{
        //    const int WM_NCHITTEST = 0x0084;
        //    const int HTTRANSPARENT = (-1);

        //    if (m.Msg == WM_NCHITTEST)
        //    {
        //        m.Result = (IntPtr)HTTRANSPARENT;
        //    }
        //    else
        //    {
        //        base.WndProc(ref m);
        //    }
        //}
    }

    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    public class GtidTextBox : TextBox
    {
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            try
            {
                base.OnKeyPress(e);
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(ex.Message);
#endif
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            try
            {
                base.OnTextChanged(e);
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(ex.Message);
#endif
            }
        }

        protected override void OnLeave(EventArgs e)
        {
            try
            {
                base.OnLeave(e);
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#else
                MessageBox.Show(ex.Message);
#endif
            }
        }
    }

    [ToolboxItem(true)]
    [DesignTimeVisible(true)]
    [Designer("DevExpress.XtraGrid.Design.BaseViewDesigner, DevExpress.XtraGrid.v9.2.Design")]
    public class GtidXtraGridView : DevExpress.XtraGrid.Views.Grid.GridView
    {
        public GtidXtraGridView(){}
        public GtidXtraGridView(DevExpress.XtraGrid.GridControl gridControl) : base(gridControl) { }
    }


    //public class MEGridView : GridView
    //{
    //    public MEGridView() : this(null) { }

    //    public MEGridView(GridControl gc)
    //        : base(gc)
    //    {
    //        this.OptionsBehavior.KeepFocusedRowOnUpdate = false;
    //        this.OptionsNavigation.AutoFocusNewRow = true;
    //        this.OptionsNavigation.EnterMoveNextColumn = true;
    //        this.OptionsSelection.MultiSelect = true;
    //        this.OptionsView.NewItemRowPosition = NewItemRowPosition.Bottom;
    //        this.OptionsView.ShowAutoFilterRow = true;
    //    }

    //    protected override string ViewName { get { return "MEGridView"; } }
    //}

    //public class MEGridViewInfoRegistrator : GridInfoRegistrator
    //{
    //    public override string ViewName { get { return "MEGridView"; } }
    //    public override BaseView CreateView(GridControl grid)
    //    {
    //        return new MEGridView(grid as GridControl);
    //    }
    //    public override BaseViewPainter CreatePainter(BaseView view)
    //    {
    //        return new MEGridPainter((GridView)view);
    //    }
    //}

    //public class MEGridPainter : GridPainter
    //{
    //    public MEGridPainter(GridView view) : base(view) { }

    //    //protected override bool DrawNewItemRow(GridViewDrawArgs e, GridRowInfo ri)
    //    //{
    //    //    return base.DrawNewItemRow(e, ri);
    //    //}

    //    protected override bool DrawNewItemRow(GridViewDrawArgs e, GridRowInfo ri)
    //    {
    //        if ((ri.RowState & GridRowCellState.Focused) != GridRowCellState.Dirty)
    //        {
    //            return false;
    //        }
    //        Rectangle r = ri.DataBounds;
    //        View.NewItemRowText = "Hier klicken, um einen neuen Datensatz anzulegen!";
    //        if (r.X < e.ViewInfo.ViewRects.ColumnPanelLeft)
    //        {
    //            r.X = e.ViewInfo.ViewRects.ColumnPanelLeft;
    //        }
    //        if (r.Right > e.ViewInfo.ViewRects.Rows.Right)
    //        {
    //            r.Width = e.ViewInfo.ViewRects.Rows.Right - r.X;
    //        }
    //        ri.Appearance.BackColor = Color.AliceBlue;
    //        this.StyleFillRectangle(e.Cache, ri.Appearance, r);
    //        ri.Appearance.DrawString(e.Cache, e.ViewInfo.GetNewItemRowText(), r);
    //        this.DrawRowIndent(e, ri);
    //        return true;
    //    }
    //}

    //public class GtidXtraGridControl1 : DevExpress.XtraGrid.GridControl
    //{
    //    protected override void RegisterAvailableViewsCore(InfoCollection collection)
    //    {
    //        //GridView grid = new GridView();
    //        base.RegisterAvailableViewsCore(collection);
    //        collection.Add(new MEGridViewInfoRegistrator());
    //    }
    //    protected override BaseView CreateDefaultView()
    //    {
    //        return CreateView("MEGridView");
    //    }
    //}
    
    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    public class GtidToolStrip : System.Windows.Forms.ToolStrip
    {
        
    }

    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    public class GtidXtraGridControl : DevExpress.XtraGrid.GridControl
    {
        public GtidXtraGridControl()
        {
            ContextMenuStrip = new ContextMenuStrip();
        }

        protected override void RegisterView(BaseView gv)
        {
            base.RegisterView(gv);
            ((GridView)gv).ShowGridMenu += new GridMenuEventHandler(GtidXtraGridControl_ShowGridMenu);
        }

        void GtidXtraGridControl_ShowGridMenu(object sender, GridMenuEventArgs e)
        {
            GridView view = sender as GridView;

            GridHitInfo hitInfo = view.CalcHitInfo(e.Point);

            if (hitInfo.InRow)
            {
                view.FocusedRowHandle = hitInfo.RowHandle;
                ContextMenuStrip.Show(view.GridControl, e.Point);
            }
        }

        
        public override object DataSource
        {
            get
            {
                return base.DataSource;
            }
            set
            {
                if ((MainView != null)&& base.DataSource == null && value != null)
                {
                    if(((ColumnView)MainView).Columns.Count > 0) ((ColumnView)MainView).Columns.Clear();

                    Type typeValue = value.GetType();
                    if (typeValue.IsGenericType && 
                        (typeValue.GetGenericTypeDefinition() == typeof(List<>) ||
                        typeValue.GetGenericTypeDefinition() == typeof(IList<>)))
                    {
                        Type typeItem = typeValue.GetGenericArguments()[0];
                        PropertyInfo[] objProperties = CBO.Instance.GetPropertyInfo(typeItem);
                        PropertyInfo objPropertyInfo;
                        for (int intProperty = 0; intProperty < objProperties.Length; intProperty++)
                        {
                            objPropertyInfo = objProperties[intProperty];
                            object[] attValues = objPropertyInfo.GetCustomAttributes(typeof(DefaultDisplay), false);
                            GridColumn column = null;
                            if (attValues.Length > 0 && attValues[0] is DefaultDisplay)
                            {
                                if((attValues[0] as DefaultDisplay).IsDefaultDisplay)
                                {
                                    column = ((ColumnView)MainView).Columns.AddField(objPropertyInfo.Name);
                                }
                            } else
                            {
                                column = ((ColumnView)MainView).Columns.AddField(objPropertyInfo.Name);
                            }
                            ToolStripMenuItem menuItem = null;
                            if (column != null)
                            {
                                column.Width = column.GetBestWidth();
                                column.Visible = true;
                                attValues = objPropertyInfo.GetCustomAttributes(typeof(CaptionColumn), false);
                                if (attValues.Length > 0 && attValues[0] is CaptionColumn)
                                {
                                    column.Caption = (attValues[0] as CaptionColumn).Caption;
                                }
                                
                                attValues = objPropertyInfo.GetCustomAttributes(typeof(XtraGridEditor), false);
                                if (attValues.Length > 0 && attValues[0] is XtraGridEditor)
                                {
                                    if ((attValues[0] as XtraGridEditor).RepositoryItem == typeof(RepositoryItemCheckEdit))
                                    {
                                        column.ColumnEdit = CheckBoxEditor;
                                    }
                                }

                                attValues = objPropertyInfo.GetCustomAttributes(typeof(DisplayFormat), false);
                                if (attValues.Length > 0 && attValues[0] is DisplayFormat)
                                {
                                    column.DisplayFormat.FormatType = (attValues[0] as DisplayFormat).FormatType;
                                    column.DisplayFormat.FormatString = (attValues[0] as DisplayFormat).FormatString;
                                }
                                //if(objPropertyInfo.PropertyType == typeof(DateTime))
                                //{
                                    
                                //}

                                menuItem = new ToolStripMenuItem(column.GetCaption());
                                menuItem.Checked = true;
                            }
                            else
                            {
                                menuItem = new ToolStripMenuItem(objPropertyInfo.Name);
                                menuItem.Checked = false;
                            }

                            menuItem.Click += new EventHandler(menuItem_Click);
                            menuItem.Tag = objPropertyInfo;
                            menuItem.Name = objPropertyInfo.Name;
                            ContextMenuStrip.Items.Add(menuItem);
                        }
                    }
                }
                base.DataSource = value;
            }
        }

        void menuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            menuItem.Checked = !menuItem.Checked;
            foreach(GridColumn column in ((ColumnView)MainView).Columns)
            {
                if(column.GetCaption() == menuItem.Text)
                {
                    column.Visible = menuItem.Checked;
                    if (column.Visible) column.VisibleIndex = menuItem.Owner.Items.IndexOfKey(menuItem.Name);
                    this.Refresh();
                    return;
                }
            }
            PropertyInfo objPropertyInfo = (PropertyInfo) menuItem.Tag;
            GridColumn col = ((ColumnView)MainView).Columns.AddField(objPropertyInfo.Name);
            object[] attValues = objPropertyInfo.GetCustomAttributes(typeof(CaptionColumn), false);
            if (attValues.Length > 0 && attValues[0] is CaptionColumn)
                col.Caption = (attValues[0] as CaptionColumn).Caption;
            else
                col.Caption = col.GetCaption();

            attValues = objPropertyInfo.GetCustomAttributes(typeof(XtraGridEditor), false);
            if (attValues.Length > 0 && attValues[0] is XtraGridEditor)
            {
                if ((attValues[0] as XtraGridEditor).RepositoryItem == typeof(RepositoryItemCheckEdit))
                {
                    CheckBoxEditor.Name = (attValues[0] as XtraGridEditor).Name;
                    col.ColumnEdit = CheckBoxEditor;
                } 
                else if ((attValues[0] as XtraGridEditor).RepositoryItem == typeof(RepositoryItemLookUpEdit))
                {
                    LookUpEditor.Name = (attValues[0] as XtraGridEditor).Name;
                    col.ColumnEdit = LookUpEditor;
                }
            }

            col.Width = col.GetBestWidth();
            col.Visible = true;
            col.VisibleIndex = ContextMenuStrip.Items.IndexOf(menuItem);
            menuItem.Text = col.GetCaption();
        }

        protected RepositoryItem LookUpEditor
        {
            get
            {
                foreach (RepositoryItem repositoryItem in RepositoryItems)
                {
                    if (repositoryItem is RepositoryItemLookUpEdit)
                    {
                        return repositoryItem;
                    }
                }

                var riLookUp = new RepositoryItemLookUpEdit();

                RepositoryItems.Add(riLookUp);

                return riLookUp;
            }
        }

        protected RepositoryItem CheckBoxEditor
        {
            get
            {
                foreach (RepositoryItem repositoryItem in RepositoryItems)
                {
                    if (repositoryItem is RepositoryItemCheckEdit)
                    {
                        return repositoryItem;
                    }
                }

                var riCheckBox = new RepositoryItemCheckEdit { ValueChecked = 1, ValueUnchecked = 0 };

                RepositoryItems.Add(riCheckBox);

                return riCheckBox;
            }
        }
    }


    [Designer(typeof(System.Windows.Forms.Design.ControlDesigner))]
    public class GtidDataGridView : DataGridView
    {
        public GtidDataGridView()
        {
            this.MouseUp += new MouseEventHandler(GtidDataGridView_MouseUp);
        }

        protected void GtidDataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DataGridView.HitTestInfo hit = this.HitTest(e.X, e.Y);
                if (hit.Type == DataGridViewHitTestType.None)
                {
                    this.ClearSelection();
                    this.CurrentCell = null;
                }
            }
        }
    }

    partial class frmBCBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "frmBCBase";
        }

        #endregion
    }
}