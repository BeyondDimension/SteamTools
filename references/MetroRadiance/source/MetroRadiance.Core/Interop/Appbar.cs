using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Interop
{
	public static class AppBar
	{
		// Note: This is constant in every DPI.
		private const int _hideAppbarSpace = 2;

		public static bool HasAutoHideAppBar(RECT area, AppBarEdges targetEdge)
		{
			var appbar = Shell32.SHAppBarGetAutoHideBarEx(targetEdge, area);
			return User32.IsWindow(appbar);
		}

		public static void ApplyAppbarSpace(RECT monitorArea, ref RECT workArea)
		{
			if (HasAutoHideAppBar(monitorArea, AppBarEdges.ABE_TOP)) workArea.Top += _hideAppbarSpace;
			if (HasAutoHideAppBar(monitorArea, AppBarEdges.ABE_LEFT)) workArea.Left += _hideAppbarSpace;
			if (HasAutoHideAppBar(monitorArea, AppBarEdges.ABE_RIGHT)) workArea.Right -= _hideAppbarSpace;
			if (HasAutoHideAppBar(monitorArea, AppBarEdges.ABE_BOTTOM)) workArea.Bottom -= _hideAppbarSpace;
		}
	}
}
