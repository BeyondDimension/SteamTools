namespace BD.WTTS;

#if WINDOWS
public static partial class HandleWindowExtensions
{
    public static bool IsHasProcessExits(this NativeWindowModel window)
    {
        if (window?.Process?.HasExited == false && window.Name != Process.GetCurrentProcess().ProcessName)
        {
            return false;
        }
        return true;
    }

    public static void Kill(this NativeWindowModel window)
    {
        if (!window.IsHasProcessExits())
        {
            window.Process?.Kill();
        }
    }
}
#endif
