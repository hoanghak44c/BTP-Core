using System.Drawing;
using System.Windows.Forms;

namespace QLBH.Core.Interfaces
{
    public delegate void UnitWorkDelegate();

    public delegate object ParameterizedUnitWorkDelegate(params object[] param);

    public delegate void ParameterizedUnitWorkDelegate<T>(T param);

    public interface IBaseViewIndex<T>
    {
        T this[string viewName] { get; }
    }

    public interface IBaseViewA
    {
        void Show();
        void Show(IWin32Window owner);
        string Name { get; set; }
        string ViewName { get; set; }
        DialogResult ShowDialog();
        DialogResult ShowDialog(IWin32Window owner);
        DialogResult DialogResult { get; set; }
        void Close();
        bool Focus();
        void Activate();
        void SafeMode(UnitWorkDelegate method);
        object SafeMode(ParameterizedUnitWorkDelegate method, params object[] args);
        void SafeMode<T>(ParameterizedUnitWorkDelegate<T> method, T args);
        Point Location { get; set; }
        int Height { get; set; }
        int Width { get; set; }
        string Text { get; set; }
        bool IsHandleCreated { get; }
        void ShowMessage(string message);
        DialogResult ShowConfirmMessage(string message);
        DialogResult ShowConfirmMessage(IWin32Window owner, string message);
        FormStartPosition StartPosition { get; set; }
        FormWindowState WindowState { get; set; }
        IBaseViewA MdiParent { get; set; }
        Control.ControlCollection Controls { get; }
        bool IsDisposed { get; }
        void Initialize();
        bool Allow { get; }
    }

    public interface IBaseView<T> : IBaseViewA
    {
        T Controller { get; }
    }
}