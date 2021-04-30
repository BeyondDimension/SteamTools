namespace System.Application.UI.Views
{
    public interface INotifyIconWindow
    {
        IntPtr Handle { get; }
    }

    public interface INotifyIconWindow<TContextMenu> : INotifyIconWindow
    {
        void Initialize(INotifyIcon<TContextMenu> notifyIcon);
    }
}