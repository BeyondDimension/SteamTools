using System;
// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	[Flags]
	public enum SetWindowPosFlags
	{
		/// <summary>
		/// Retains the current size (ignores the cx and cy parameters).
		/// </summary>
		SWP_NOSIZE = 0x0001,

		/// <summary>
		/// Retains the current position (ignores X and Y parameters).
		/// </summary>
		SWP_NOMOVE = 0x0002,

		/// <summary>
		/// Retains the current Z order (ignores the hWndInsertAfter parameter).
		/// </summary>
		SWP_NOZORDER = 0x0004,

		SWP_NOREDRAW = 0x0008,
		SWP_NOACTIVATE = 0x0010,
		SWP_FRAMECHANGED = 0x0020,
		SWP_SHOWWINDOW = 0x0040,
		SWP_HIDEWINDOW = 0x0080,
		SWP_NOCOPYBITS = 0x0100,
		
		/// <summary>
		/// Does not change the owner window's position in the Z order.
		/// </summary>
		SWP_NOOWNERZORDER = 0x0200,

		/// <summary>
		/// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
		/// </summary>
		SWP_NOSENDCHANGING = 0x0400,
		
		/// <summary>
		/// Prevents generation of the WM_SYNCPAINT message.
		/// </summary>
		SWP_DEFERERASE = 0x2000,
		
		/// <summary>
		/// If the calling thread and the thread that owns the window are attached to different input queues,
		/// the system posts the request to the thread that owns the window.
		/// This prevents the calling thread from blocking its execution while other threads process the request.
		/// </summary>
		SWP_ASYNCWINDOWPOS = 0x4000,
	}
}
