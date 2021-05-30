using System.ComponentModel;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// A native Win32 window encapsulation for receiving window messages.
    /// </summary>
    internal class NativeWindow
    {
        readonly UnmanagedMethods.WndProc _wndProc;
        readonly string _className = "NativeHelperWindow" + Guid.NewGuid();

        /// <summary>
        /// The window handle of the underlying native window.
        /// </summary>
        public IntPtr Handle { get; set; }

        /// <summary>
        /// Creates a new native (Win32) helper window for receiving window messages.
        /// </summary>
        public NativeWindow()
        {
            // We need to store the window proc as a field so that
            // it doesn't get garbage collected away.
            _wndProc = new UnmanagedMethods.WndProc(WndProc);

            UnmanagedMethods.WNDCLASSEX wndClassEx = new UnmanagedMethods.WNDCLASSEX
            {
                cbSize = Marshal.SizeOf<UnmanagedMethods.WNDCLASSEX>(),
                lpfnWndProc = _wndProc,
                hInstance = UnmanagedMethods.GetModuleHandle(null),
                lpszClassName = _className
            };

            ushort atom = UnmanagedMethods.RegisterClassEx(ref wndClassEx);

            if (atom == 0)
            {
                throw new Win32Exception();
            }

            Handle = UnmanagedMethods.CreateWindowEx(0, atom, null, UnmanagedMethods.WS_POPUP, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            if (Handle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        /// <summary>
        /// Destructs the object and destroys the native window.
        /// </summary>
        ~NativeWindow()
        {
            if (Handle != IntPtr.Zero)
            {
                UnmanagedMethods.PostMessage(Handle, (uint)UnmanagedMethods.WindowsMessage.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
            }
        }

        /// <summary>
        /// This function will receive all the system window messages relevant to our window.
        /// </summary>
        protected virtual IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case (uint)UnmanagedMethods.WindowsMessage.WM_CLOSE:
                    UnmanagedMethods.DestroyWindow(hWnd);
                    break;
                case (uint)UnmanagedMethods.WindowsMessage.WM_DESTROY:
                    UnmanagedMethods.PostQuitMessage(0);
                    break;
                default:
                    return UnmanagedMethods.DefWindowProc(hWnd, msg, wParam, lParam);
            }
            return IntPtr.Zero;
        }
    }
}