// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	public enum DWMWINDOWATTRIBUTE : uint
	{
		DWMWA_NCRENDERING_ENABLED = 1,      // [get] Is non-client rendering enabled/disabled
		DWMWA_NCRENDERING_POLICY,           // [set] Non-client rendering policy
		DWMWA_TRANSITIONS_FORCEDISABLED,    // [set] Potentially enable/forcibly disable transitions
		DWMWA_ALLOW_NCPAINT,                // [set] Allow contents rendered in the non-client area to be visible on the DWM-drawn frame.
		DWMWA_CAPTION_BUTTON_BOUNDS,        // [get] Bounds of the caption button area in window-relative space.
		DWMWA_NONCLIENT_RTL_LAYOUT,         // [set] Is non-client content RTL mirrored
		DWMWA_FORCE_ICONIC_REPRESENTATION,  // [set] Force this window to display iconic thumbnails.
		DWMWA_FLIP3D_POLICY,                // [set] Designates how Flip3D will treat the window.
		DWMWA_EXTENDED_FRAME_BOUNDS,        // [get] Gets the extended frame bounds rectangle in screen space
		DWMWA_HAS_ICONIC_BITMAP,            // [set] Indicates an available bitmap when there is no better thumbnail representation.
		DWMWA_DISALLOW_PEEK,                // [set] Don't invoke Peek on the window.
		DWMWA_EXCLUDED_FROM_PEEK,           // [set] LivePreview exclusion information
		DWMWA_CLOAK,                        // [set] Cloak or uncloak the window
		DWMWA_CLOAKED,                      // [get] Gets the cloaked state of the window
		DWMWA_FREEZE_REPRESENTATION,        // [set] Force this window to freeze the thumbnail without live update
		DWMWA_LAST
	};
}
