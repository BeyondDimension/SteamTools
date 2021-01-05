using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace MetroRadiance.Interop.Win32
{
	public static class Shell32
	{
		[DllImport("shell32")]
		internal static extern IntPtr SHAppBarMessage(AppBarMessages dwMessage, ref APPBARDATA pData);

		public static AppBarState SHAppBarGetState(ref APPBARDATA pData)
		{
			return (AppBarState)SHAppBarMessage(AppBarMessages.ABM_GETSTATE, ref pData);
		}
		
		public static AppBarEdges SHAppBarGetTaskbarPos(RECT rc)
		{
			var data = new APPBARDATA()
			{
				cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
				rc = rc,
			};
			if (SHAppBarMessage(AppBarMessages.ABM_GETTASKBARPOS, ref data) == IntPtr.Zero)
			{
				throw new Win32Exception();
			}
			return data.uEdge;
		}

		public static IntPtr SHAppBarGetAutoHideBar(AppBarEdges uEdge)
		{
			var data = new APPBARDATA()
			{
				cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
				uEdge = uEdge,
			};
			return SHAppBarMessage(AppBarMessages.ABM_GETAUTOHIDEBAR, ref data);
		}

		public static IntPtr SHAppBarGetAutoHideBarEx(AppBarEdges uEdge, RECT rc)
		{
			var data = new APPBARDATA()
			{
				cbSize = Marshal.SizeOf(typeof(APPBARDATA)),
				uEdge = uEdge,
				rc = rc,
			};
			return SHAppBarMessage(AppBarMessages.ABM_GETAUTOHIDEBAREX, ref data);
		}
	}
}
