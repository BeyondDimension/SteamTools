using System;
// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	[Flags]
	public enum WindowExStyles : uint
	{
		/// <summary>
		/// Specifies that a window created with this style accepts drag-drop files.
		/// </summary>
		WS_EX_ACCEPTFILES = 0x00000010,

		/// <summary>
		/// Forces a top-level window onto the taskbar when the window is visible.
		/// </summary>
		WS_EX_APPWINDOW = 0x00040000,

		/// <summary>
		/// Specifies that a window has a border with a sunken edge.
		/// </summary>
		WS_EX_CLIENTEDGE = 0x00000200,

		/// <summary>
		/// Windows XP: Paints all descendants of a window in bottom-to-top painting order using double-buffering. For more information, see Remarks. This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. 
		/// </summary>
		WS_EX_COMPOSITED = 0x02000000,

		/// <summary>
		/// Includes a question mark in the title bar of the window. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message. The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command. The Help application displays a pop-up window that typically contains help for the child window.
		/// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
		/// </summary>
		WS_EX_CONTEXTHELP = 0x00000400,

		/// <summary>
		/// The window itself contains child windows that should take part in dialog box navigation. If this style is specified, the dialog manager recurses into children of this window when performing navigation operations such as handling the TAB key, an arrow key, or a keyboard mnemonic.
		/// </summary>
		WS_EX_CONTROLPARENT = 0x00010000,

		/// <summary>
		/// Creates a window that has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
		/// </summary>
		WS_EX_LGMODALFRAME = 0x00000001,

		/// <summary>
		/// Windows 2000/XP: Creates a layered window. Note that this cannot be used for child windows. Also, this cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. 
		/// </summary>
		WS_EX_LAYERED = 0x00080000,

		/// <summary>
		/// Arabic and Hebrew versions of Windows 98/Me, Windows 2000/XP: Creates a window whose horizontal origin is on the right edge. Increasing horizontal values advance to the left. 
		/// </summary>
		WS_EX_LAYOUTRTL = 0x00400000,

		/// <summary>
		/// Creates a window that has generic left-aligned properties. This is the default.
		/// </summary>
		WS_EX_LEFT = 0x00000000,

		/// <summary>
		/// If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present) is to the left of the client area. For other languages, the style is ignored.
		/// </summary>
		WS_EX_LEFTSCROLLBAR = 0x00004000,

		/// <summary>
		/// The window text is displayed using left-to-right reading-order properties. This is the default.
		/// </summary>
		WS_EX_LTRREADING = 0x00000000,

		/// <summary>
		/// Creates a multiple-document interface (MDI) child window.
		/// </summary>
		WS_EX_MDICHILD = 0x00000040,

		/// <summary>
		/// Windows 2000/XP: A top-level window created with this style does not become the foreground window when the user clicks it. The system does not bring this window to the foreground when the user minimizes or closes the foreground window. 
		/// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
		/// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
		/// </summary>
		WS_EX_NOACTIVATE = 0x08000000,

		/// <summary>
		/// Windows 2000/XP: A window created with this style does not pass its window layout to its child windows.
		/// </summary>
		NOINHERITLAYOUT = 0x00100000,

		/// <summary>
		/// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
		/// </summary>
		WS_EX_NOPARENTNOTIFY = 0x00000004,

		/// <summary>
		/// Combines the WS_EX_CLIENTEDGE and WS_EX_WINDOWEDGE styles.
		/// </summary>
		WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

		/// <summary>
		/// Combines the WS_EX_WINDOWEDGE, WS_EX_TOOLWINDOW, and WS_EX_TOPMOST styles.
		/// </summary>
		WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

		/// <summary>
		/// The window has generic "right-aligned" properties. This depends on the window class. This style has an effect only if the shell language is Hebrew, Arabic, or another language that supports reading-order alignment; otherwise, the style is ignored.
		/// Using the WS_EX_RIGHT style for static or edit controls has the same effect as using the SS_RIGHT or ES_RIGHT style, respectively. Using this style with button controls has the same effect as using BS_RIGHT and BS_RIGHTBUTTON styles.
		/// </summary>
		WS_EX_RIGHT = 0x00001000,

		/// <summary>
		/// Vertical scroll bar (if present) is to the right of the client area. This is the default.
		/// </summary>
		WS_EX_RIGHTSCROLLBAR = 0x00000000,

		/// <summary>
		/// If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties. For other languages, the style is ignored.
		/// </summary>
		WS_EX_RTLREADING = 0x00002000,

		/// <summary>
		/// Creates a window with a three-dimensional border style intended to be used for items that do not accept user input.
		/// </summary>
		WS_EX_STATICEDGE = 0x00020000,

		/// <summary>
		/// Creates a tool window; that is, a window intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font. A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB. If a tool window has a system menu, its icon is not displayed on the title bar. However, you can display the system menu by right-clicking or by typing ALT+SPACE. 
		/// </summary>
		WS_EX_TOOLWINDOW = 0x00000080,

		/// <summary>
		/// Specifies that a window created with this style should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
		/// </summary>
		WS_EX_TOPMOST = 0x00000008,

		/// <summary>
		/// Specifies that a window created with this style should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
		/// To achieve transparency without these restrictions, use the SetWindowRgn function.
		/// </summary>
		WS_EX_TRANSPARENT = 0x00000020,

		/// <summary>
		/// Specifies that a window has a border with a raised edge.
		/// </summary>
		WS_EX_WINDOWEDGE = 0x00000100,
	}
}
