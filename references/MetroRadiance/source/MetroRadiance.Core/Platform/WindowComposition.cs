using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using MetroRadiance.Interop.Win32;
using MetroRadiance.Media;

namespace MetroRadiance.Platform
{
	public static class WindowComposition
	{
		public static void Disable(Window window)
		{
			var accentPolicy = new AccentPolicy
			{
				AccentState = AccentState.ACCENT_DISABLED,
			};
			SetAccentPolicy(window, accentPolicy);
		}

		public static void EnableBlur(Window window, AccentFlags accentFlags)
		{
			var accentPolicy = new AccentPolicy
			{
				AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND,
				AccentFlags = accentFlags,
			};
			SetAccentPolicy(window, accentPolicy);
		}

		public static void EnableAcrylicBlur(Window window, Color backgroundColor, AccentFlags accentFlags)
		{
			var accentPolicy = new AccentPolicy
			{
				AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
				AccentFlags = accentFlags,
				GradientColor = ColorHelper.GetColorAsUInt32(backgroundColor),
			};
			SetAccentPolicy(window, accentPolicy);
		}

		public static void SetAccentPolicy(Window window, AccentPolicy accentPolicy)
		{
			var windowInteropHelper = new WindowInteropHelper(window);
			var hWnd = windowInteropHelper.EnsureHandle();
			SetAccentPolicy(hWnd, accentPolicy);
		}

		public static void SetAccentPolicy(IntPtr hWnd, AccentPolicy accentPolicy)
		{
			var accentStructSize = Marshal.SizeOf(typeof(AccentPolicy));
			var accentPtr = IntPtr.Zero;
			try
			{
				accentPtr = Marshal.AllocCoTaskMem(accentStructSize);
				Marshal.StructureToPtr(accentPolicy, accentPtr, false);

				var data = new WindowCompositionAttributeData
				{
					Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
					SizeOfData = accentStructSize,
					Data = accentPtr,
				};
				User32.SetWindowCompositionAttribute(hWnd, data);
			}
			finally
			{
				if (accentPtr != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(accentPtr);
				}
			}
		}
	}
}
