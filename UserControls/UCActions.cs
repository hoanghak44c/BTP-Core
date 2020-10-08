using System;
using System.Windows.Forms;

namespace QLBH.Core.UserControls
{
    public delegate void ActionHandler<T>(T obj);
    public delegate void ActionValidateHandler<T>(T obj, ActionState actionMode);
    public delegate void ActionHandler();

    public partial class UCActions : UserControl
    {
        private bool isAddMode, isEditMode;

        private object editItem;

        public bool IsSynchronizable;

        /// <summary>
        /// Hien thi du lieu 
        /// </summary>
        public event ActionHandler<object> OnLoadEditor;
        public event ActionHandler OnDisableEditor;
        public event ActionHandler OnEnableEditor;
        public event ActionHandler OnClose;
        public event ActionHandler OnSynchronize;
        /// <summary>
        /// Kiem tra du lieu 
        /// </summary>
        public event ActionValidateHandler<object> OnValidate;
        /// <summary>
        /// Thuc hien them moi du lieu 
        /// </summary>
        public event ActionHandler<object> OnAdd;
        /// <summary>
        /// Thuc hien update du lieu 
        /// </summary>
        public event ActionHandler<object> OnUpdate;

        public event ActionHandler<object> OnDelete;

        public UCActions()
        {
            InitializeComponent();
        }

        private string ActionText
        {
            get { return IsSynchronizable ? "&Đồng bộ" : "&Thêm mới"; }
        }

        public virtual void LoadEditor(object editObject)
        {
            try
            {
                this.editItem = editObject;

                btnThem.Text = ActionText;
                btnCapNhat.Text = "&Sửa đổi";
                isEditMode = false;
                isAddMode = false;
                // disable editor
                if (OnDisableEditor != null)
                    OnDisableEditor();

                btnCapNhat.Enabled = editItem != null;
                btnDel.Enabled = editItem != null && !IsSynchronizable;

                if (OnLoadEditor != null)
                    OnLoadEditor(editItem);

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

        private void btnThem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isAddMode && !isEditMode && !IsSynchronizable)
                {
                    isAddMode = true;
                    isEditMode = false;
                    //clean editor
                    editItem = null;
                    if (OnLoadEditor != null)
                        OnLoadEditor(editItem);
                    //enable editor
                    if (OnEnableEditor != null)
                        OnEnableEditor();

                    btnThem.Text = "&Lưu";
                    btnCapNhat.Text = "&Hủy bỏ";
                    btnCapNhat.Enabled = true;
                    return;
                }

                if(isAddMode || isEditMode)
                {
                    //check validate
                    if (OnValidate != null)
                        OnValidate(editItem, isEditMode ? ActionState.UPDATE : ActionState.ADD);

                    if (isAddMode)
                    {
                        //insert data
                        if (OnAdd != null)
                            OnAdd(editItem);
                    }
                    else
                    {
                        //update data
                        if (OnUpdate != null)
                            OnUpdate(editItem);
                    }
                }
                else if (IsSynchronizable)
                {
                    if (OnSynchronize != null)
                    {
                        OnSynchronize();
                        return;
                    }
                }

                btnThem.Text = ActionText;
                btnCapNhat.Text = "&Sửa đổi";
                isEditMode = false;
                isAddMode = false;
                // disable editor
                if (OnDisableEditor != null)
                    OnDisableEditor();
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

        private void btnCapNhat_Click(object sender, EventArgs e)
        {
            try
            {
                if (!isAddMode && !isEditMode)
                {
                    isAddMode = false;
                    isEditMode = true;
                    //enable editor
                    if (OnEnableEditor != null)
                        OnEnableEditor();
                    btnThem.Text = "&Lưu";
                    btnCapNhat.Text = "&Hủy bỏ";
                    return;
                }
                // raise cancel event

                // reset editor to before state
                if (OnLoadEditor != null)
                    OnLoadEditor(editItem);
                btnThem.Text = ActionText;
                btnCapNhat.Text = "&Sửa đổi";
                btnCapNhat.Enabled = editItem != null;
                isEditMode = false;
                isAddMode = false;

                // disable editor
                if (OnDisableEditor != null)
                    OnDisableEditor();
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

        private void btnDel_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsSynchronizable) throw new InvalidOperationException("Không thể xóa dữ liệu");
                //check validate
                if (OnValidate != null)
                    OnValidate(editItem, ActionState.DELETE);

                if (MessageBox.Show("Bạn có thực sự muốn xóa không?", "Thông báo", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    if (OnDelete != null)
                        OnDelete(editItem);
                    //clean editor
                    editItem = null;
                    LoadEditor(editItem);
                }
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

        private void btnDong_Click(object sender, EventArgs e)
        {
            try
            {
                if (OnClose != null)
                    OnClose();
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

        private void UCActions_Load(object sender, EventArgs e)
        {
            btnThem.Enabled = true;
            this.btnThem.Click += btnThem_Click;
            
            btnThem.Text = ActionText;
            
            //lúc form hiện lên có thể chưa có edit item nào được chọn
            btnCapNhat.Enabled = false;
            btnDel.Enabled = false;

            //disable editor
            if (OnDisableEditor != null)
                OnDisableEditor();
        }
    }
}
