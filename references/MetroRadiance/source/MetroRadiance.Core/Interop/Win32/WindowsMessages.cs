using System;
// ReSharper disable InconsistentNaming

namespace MetroRadiance.Interop.Win32
{
	/// <summary>
	/// Windows Messages
	/// Defined in winuser.h from Windows SDK v6.1
	/// Documentation pulled from MSDN.
	/// </summary>
	public enum WindowsMessages : uint
	{
		/// <summary>
		/// The WM_NULL message performs no operation. An application sends the WM_NULL message if it wants to post a message that the recipient window will ignore.
		/// </summary>
		WM_NULL = 0x0000,

		/// <summary>
		/// The WM_CREATE message is sent when an application requests that a window be created by calling the CreateWindowEx or CreateWindow function. (The message is sent before the function returns.) The window procedure of the new window receives this message after the window is created, but before the window becomes visible.
		/// </summary>
		WM_CREATE = 0x0001,

		/// <summary>
		/// The WM_DESTROY message is sent when a window is being destroyed. It is sent to the window procedure of the window being destroyed after the window is removed from the screen. 
		/// This message is sent first to the window being destroyed and then to the child windows (if any) as they are destroyed. During the processing of the message, it can be assumed that all child windows still exist.
		/// /// </summary>
		WM_DESTROY = 0x0002,

		/// <summary>
		/// The WM_MOVE message is sent after a window has been moved. 
		/// </summary>
		WM_MOVE = 0x0003,

		/// <summary>
		/// The WM_SIZE message is sent to a window after its size has changed.
		/// </summary>
		WM_SIZE = 0x0005,

		/// <summary>
		/// The WM_ACTIVATE message is sent to both the window being activated and the window being deactivated. If the windows use the same input queue, the message is sent synchronously, first to the window procedure of the top-level window being deactivated, then to the window procedure of the top-level window being activated. If the windows use different input queues, the message is sent asynchronously, so the window is activated immediately. 
		/// </summary>
		WM_ACTIVATE = 0x0006,

		/// <summary>
		/// The WM_SETFOCUS message is sent to a window after it has gained the keyboard focus. 
		/// </summary>
		WM_SETFOCUS = 0x0007,

		/// <summary>
		/// The WM_KILLFOCUS message is sent to a window immediately before it loses the keyboard focus. 
		/// </summary>
		WM_KILLFOCUS = 0x0008,

		/// <summary>
		/// The WM_ENABLE message is sent when an application changes the enabled state of a window. It is sent to the window whose enabled state is changing. This message is sent before the EnableWindow function returns, but after the enabled state (WS_DISABLED style bit) of the window has changed. 
		/// </summary>
		WM_ENABLE = 0x000A,

		/// <summary>
		/// An application sends the WM_SETREDRAW message to a window to allow changes in that window to be redrawn or to prevent changes in that window from being redrawn. 
		/// </summary>
		WM_SETREDRAW = 0x000B,

		/// <summary>
		/// An application sends a WM_SETTEXT message to set the text of a window. 
		/// </summary>
		WM_SETTEXT = 0x000C,

		/// <summary>
		/// An application sends a WM_GETTEXT message to copy the text that corresponds to a window into a buffer provided by the caller. 
		/// </summary>
		WM_GETTEXT = 0x000D,

		/// <summary>
		/// An application sends a WM_GETTEXTLENGTH message to determine the length, in characters, of the text associated with a window. 
		/// </summary>
		WM_GETTEXTLENGTH = 0x000E,

		/// <summary>
		/// The WM_PAINT message is sent when the system or another application makes a request to paint a portion of an application's window. The message is sent when the UpdateWindow or RedrawWindow function is called, or by the DispatchMessage function when the application obtains a WM_PAINT message by using the GetMessage or PeekMessage function. 
		/// </summary>
		WM_PAINT = 0x000F,

		/// <summary>
		/// The WM_CLOSE message is sent as a signal that a window or an application should terminate.
		/// </summary>
		WM_CLOSE = 0x0010,

		/// <summary>
		/// The WM_QUERYENDSESSION message is sent when the user chooses to end the session or when an application calls one of the system shutdown functions. If any application returns zero, the session is not ended. The system stops sending WM_QUERYENDSESSION messages as soon as one application returns zero.
		/// After processing this message, the system sends the WM_ENDSESSION message with the wParam parameter set to the results of the WM_QUERYENDSESSION message.
		/// </summary>
		WM_QUERYENDSESSION = 0x0011,

		/// <summary>
		/// The WM_QUERYOPEN message is sent to an icon when the user requests that the window be restored to its previous size and position.
		/// </summary>
		WM_QUERYOPEN = 0x0013,

		/// <summary>
		/// The WM_ENDSESSION message is sent to an application after the system processes the results of the WM_QUERYENDSESSION message. The WM_ENDSESSION message informs the application whether the session is ending.
		/// </summary>
		WM_ENDSESSION = 0x0016,

		/// <summary>
		/// The WM_QUIT message indicates a request to terminate an application and is generated when the application calls the PostQuitMessage function. It causes the GetMessage function to return zero.
		/// </summary>
		WM_QUIT = 0x0012,

		/// <summary>
		/// The WM_ERASEBKGND message is sent when the window background must be erased (for example, when a window is resized). The message is sent to prepare an invalidated portion of a window for painting. 
		/// </summary>
		WM_ERASEBKGND = 0x0014,

		/// <summary>
		/// This message is sent to all top-level windows when a change is made to a system color setting. 
		/// </summary>
		WM_SYSCOLORCHANGE = 0x0015,

		/// <summary>
		/// The WM_SHOWWINDOW message is sent to a window when the window is about to be hidden or shown.
		/// </summary>
		WM_SHOWWINDOW = 0x0018,

		/// <summary>
		/// An application sends the WM_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.
		/// Note  The WM_WININICHANGE message is provided only for compatibility with earlier versions of the system. Applications should use the WM_SETTINGCHANGE message.
		/// </summary>
		WM_WININICHANGE = 0x001A,

		/// <summary>
		/// An application sends the WM_WININICHANGE message to all top-level windows after making a change to the WIN.INI file. The SystemParametersInfo function sends this message after an application uses the function to change a setting in WIN.INI.
		/// Note  The WM_WININICHANGE message is provided only for compatibility with earlier versions of the system. Applications should use the WM_SETTINGCHANGE message.
		/// </summary>
		WM_SETTINGCHANGE = WM_WININICHANGE,

		/// <summary>
		/// The WM_DEVMODECHANGE message is sent to all top-level windows whenever the user changes device-mode settings. 
		/// </summary>
		WM_DEVMODECHANGE = 0x001B,

		/// <summary>
		/// The WM_ACTIVATEAPP message is sent when a window belonging to a different application than the active window is about to be activated. The message is sent to the application whose window is being activated and to the application whose window is being deactivated.
		/// </summary>
		WM_ACTIVATEAPP = 0x001C,

		/// <summary>
		/// An application sends the WM_FONTCHANGE message to all top-level windows in the system after changing the pool of font resources. 
		/// </summary>
		WM_FONTCHANGE = 0x001D,

		/// <summary>
		/// A message that is sent whenever there is a change in the system time.
		/// </summary>
		WM_TIMECHANGE = 0x001E,

		/// <summary>
		/// The WM_CANCELMODE message is sent to cancel certain modes, such as mouse capture. For example, the system sends this message to the active window when a dialog box or message box is displayed. Certain functions also send this message explicitly to the specified window regardless of whether it is the active window. For example, the EnableWindow function sends this message when disabling the specified window.
		/// </summary>
		WM_CANCELMODE = 0x001F,

		/// <summary>
		/// The WM_SETCURSOR message is sent to a window if the mouse causes the cursor to move within a window and mouse input is not captured. 
		/// </summary>
		WM_SETCURSOR = 0x0020,

		/// <summary>
		/// The WM_MOUSEACTIVATE message is sent when the cursor is in an inactive window and the user presses a mouse button. The parent window receives this message only if the child window passes it to the DefWindowProc function.
		/// </summary>
		WM_MOUSEACTIVATE = 0x0021,

		/// <summary>
		/// The WM_CHILDACTIVATE message is sent to a child window when the user clicks the window's title bar or when the window is activated, moved, or sized.
		/// </summary>
		WM_CHILDACTIVATE = 0x0022,

		/// <summary>
		/// The WM_QUEUESYNC message is sent by a computer-based training (CBT) application to separate user-input messages from other messages sent through the WH_JOURNALPLAYBACK Hook procedure. 
		/// </summary>
		WM_QUEUESYNC = 0x0023,

		/// <summary>
		/// The WM_GETMINMAXINFO message is sent to a window when the size or position of the window is about to change. An application can use this message to override the window's default maximized size and position, or its default minimum or maximum tracking size. 
		/// </summary>
		WM_GETMINMAXINFO = 0x0024,

		/// <summary>
		/// Windows NT 3.51 and earlier: The WM_PAINTICON message is sent to a minimized window when the icon is to be painted. This message is not sent by newer versions of Microsoft Windows, except in unusual circumstances explained in the Remarks.
		/// </summary>
		WM_PAINTICON = 0x0026,

		/// <summary>
		/// Windows NT 3.51 and earlier: The WM_ICONERASEBKGND message is sent to a minimized window when the background of the icon must be filled before painting the icon. A window receives this message only if a class icon is defined for the window; otherwise, WM_ERASEBKGND is sent. This message is not sent by newer versions of Windows.
		/// </summary>
		WM_ICONERASEBKGND = 0x0027,

		/// <summary>
		/// The WM_NEXTDLGCTL message is sent to a dialog box procedure to set the keyboard focus to a different control in the dialog box. 
		/// </summary>
		WM_NEXTDLGCTL = 0x0028,

		/// <summary>
		/// The WM_SPOOLERSTATUS message is sent from Print Manager whenever a job is added to or removed from the Print Manager queue. 
		/// </summary>
		WM_SPOOLERSTATUS = 0x002A,

		/// <summary>
		/// The WM_DRAWITEM message is sent to the parent window of an owner-drawn button, combo box, list box, or menu when a visual aspect of the button, combo box, list box, or menu has changed.
		/// </summary>
		WM_DRAWITEM = 0x002B,

		/// <summary>
		/// The WM_MEASUREITEM message is sent to the owner window of a combo box, list box, list view control, or menu item when the control or menu is created.
		/// </summary>
		WM_MEASUREITEM = 0x002C,

		/// <summary>
		/// Sent to the owner of a list box or combo box when the list box or combo box is destroyed or when items are removed by the LB_DELETESTRING, LB_RESETCONTENT, CB_DELETESTRING, or CB_RESETCONTENT message. The system sends a WM_DELETEITEM message for each deleted item. The system sends the WM_DELETEITEM message for any deleted list box or combo box item with nonzero item data.
		/// </summary>
		WM_DELETEITEM = 0x002D,

		/// <summary>
		/// Sent by a list box with the LBS_WANTKEYBOARDINPUT style to its owner in response to a WM_KEYDOWN message. 
		/// </summary>
		WM_VKEYTOITEM = 0x002E,

		/// <summary>
		/// Sent by a list box with the LBS_WANTKEYBOARDINPUT style to its owner in response to a WM_CHAR message. 
		/// </summary>
		WM_CHARTOITEM = 0x002F,

		/// <summary>
		/// An application sends a WM_SETFONT message to specify the font that a control is to use when drawing text. 
		/// </summary>
		WM_SETFONT = 0x0030,

		/// <summary>
		/// An application sends a WM_GETFONT message to a control to retrieve the font with which the control is currently drawing its text. 
		/// </summary>
		WM_GETFONT = 0x0031,

		/// <summary>
		/// An application sends a WM_SETHOTKEY message to a window to associate a hot key with the window. When the user presses the hot key, the system activates the window. 
		/// </summary>
		WM_SETHOTKEY = 0x0032,

		/// <summary>
		/// An application sends a WM_GETHOTKEY message to determine the hot key associated with a window. 
		/// </summary>
		WM_GETHOTKEY = 0x0033,

		/// <summary>
		/// The WM_QUERYDRAGICON message is sent to a minimized (iconic) window. The window is about to be dragged by the user but does not have an icon defined for its class. An application can return a handle to an icon or cursor. The system displays this cursor or icon while the user drags the icon.
		/// </summary>
		WM_QUERYDRAGICON = 0x0037,

		/// <summary>
		/// The system sends the WM_COMPAREITEM message to determine the relative position of a new item in the sorted list of an owner-drawn combo box or list box. Whenever the application adds a new item, the system sends this message to the owner of a combo box or list box created with the CBS_SORT or LBS_SORT style. 
		/// </summary>
		WM_COMPAREITEM = 0x0039,

		/// <summary>
		/// Active Accessibility sends the WM_GETOBJECT message to obtain information about an accessible object contained in a server application. 
		/// Applications never send this message directly. It is sent only by Active Accessibility in response to calls to AccessibleObjectFromPoint, AccessibleObjectFromEvent, or AccessibleObjectFromWindow. However, server applications handle this message. 
		/// </summary>
		WM_GETOBJECT = 0x003D,

		/// <summary>
		/// The WM_COMPACTING message is sent to all top-level windows when the system detects more than 12.5 percent of system time over a 30- to 60-second interval is being spent compacting memory. This indicates that system memory is low.
		/// </summary>
		WM_COMPACTING = 0x0041,

		/// <summary>
		/// WM_COMMNOTIFY is Obsolete for Win32-Based Applications
		/// </summary>
		[Obsolete]
		WM_COMMNOTIFY = 0x0044,

		/// <summary>
		/// The WM_WINDOWPOSCHANGING message is sent to a window whose size, position, or place in the Z order is about to change as a result of a call to the SetWindowPos function or another window-management function.
		/// </summary>
		WM_WINDOWPOSCHANGING = 0x0046,

		/// <summary>
		/// The WM_WINDOWPOSCHANGED message is sent to a window whose size, position, or place in the Z order has changed as a result of a call to the SetWindowPos function or another window-management function.
		/// </summary>
		WM_WINDOWPOSCHANGED = 0x0047,

		/// <summary>
		/// Notifies applications that the system, typically a battery-powered personal computer, is about to enter a suspended mode.
		/// Use: POWERBROADCAST
		/// </summary>
		[Obsolete]
		WM_POWER = 0x0048,

		/// <summary>
		/// An application sends the WM_COPYDATA message to pass data to another application. 
		/// </summary>
		WM_COPYDATA = 0x004A,

		/// <summary>
		/// The WM_CANCELJOURNAL message is posted to an application when a user cancels the application's journaling activities. The message is posted with a NULL window handle. 
		/// </summary>
		WM_CANCELJOURNAL = 0x004B,

		/// <summary>
		/// Sent by a common control to its parent window when an event has occurred or the control requires some information. 
		/// </summary>
		WM_NOTIFY = 0x004E,

		/// <summary>
		/// The WM_INPUTLANGCHANGEREQUEST message is posted to the window with the focus when the user chooses a new input language, either with the hotkey (specified in the Keyboard control panel application) or from the indicator on the system taskbar. An application can accept the change by passing the message to the DefWindowProc function or reject the change (and prevent it from taking place) by returning immediately. 
		/// </summary>
		WM_INPUTLANGCHANGEREQUEST = 0x0050,

		/// <summary>
		/// The WM_INPUTLANGCHANGE message is sent to the topmost affected window after an application's input language has been changed. You should make any application-specific settings and pass the message to the DefWindowProc function, which passes the message to all first-level child windows. These child windows can pass the message to DefWindowProc to have it pass the message to their child windows, and so on. 
		/// </summary>
		WM_INPUTLANGCHANGE = 0x0051,

		/// <summary>
		/// Sent to an application that has initiated a training card with Microsoft Windows Help. The message informs the application when the user clicks an authorable button. An application initiates a training card by specifying the HELP_TCARD command in a call to the WinHelp function.
		/// </summary>
		WM_TCARD = 0x0052,

		/// <summary>
		/// Indicates that the user pressed the F1 key. If a menu is active when F1 is pressed, WM_HELP is sent to the window associated with the menu; otherwise, WM_HELP is sent to the window that has the keyboard focus. If no window has the keyboard focus, WM_HELP is sent to the currently active window. 
		/// </summary>
		WM_HELP = 0x0053,

		/// <summary>
		/// The WM_USERCHANGED message is sent to all windows after the user has logged on or off. When the user logs on or off, the system updates the user-specific settings. The system sends this message immediately after updating the settings.
		/// </summary>
		WM_USERCHANGED = 0x0054,

		/// <summary>
		/// Determines if a window accepts ANSI or Unicode structures in the WM_NOTIFY notification message. WM_NOTIFYFORMAT messages are sent from a common control to its parent window and from the parent window to the common control.
		/// </summary>
		WM_NOTIFYFORMAT = 0x0055,

		/// <summary>
		/// The WM_CONTEXTMENU message notifies a window that the user clicked the right mouse button (right-clicked) in the window.
		/// </summary>
		WM_CONTEXTMENU = 0x007B,

		/// <summary>
		/// The WM_STYLECHANGING message is sent to a window when the SetWindowLong function is about to change one or more of the window's styles.
		/// </summary>
		WM_STYLECHANGING = 0x007C,

		/// <summary>
		/// The WM_STYLECHANGED message is sent to a window after the SetWindowLong function has changed one or more of the window's styles
		/// </summary>
		WM_STYLECHANGED = 0x007D,

		/// <summary>
		/// The WM_DISPLAYCHANGE message is sent to all windows when the display resolution has changed.
		/// </summary>
		WM_DISPLAYCHANGE = 0x007E,

		/// <summary>
		/// The WM_GETICON message is sent to a window to retrieve a handle to the large or small icon associated with a window. The system displays the large icon in the ALT+TAB dialog, and the small icon in the window caption. 
		/// </summary>
		WM_GETICON = 0x007F,

		/// <summary>
		/// An application sends the WM_SETICON message to associate a new large or small icon with a window. The system displays the large icon in the ALT+TAB dialog box, and the small icon in the window caption. 
		/// </summary>
		WM_SETICON = 0x0080,

		/// <summary>
		/// The WM_NCCREATE message is sent prior to the WM_CREATE message when a window is first created.
		/// </summary>
		WM_NCCREATE = 0x0081,

		/// <summary>
		/// The WM_NCDESTROY message informs a window that its nonclient area is being destroyed. The DestroyWindow function sends the WM_NCDESTROY message to the window following the WM_DESTROY message. WM_DESTROY is used to free the allocated memory object associated with the window. 
		/// The WM_NCDESTROY message is sent after the child windows have been destroyed. In contrast, WM_DESTROY is sent before the child windows are destroyed.
		/// </summary>
		WM_NCDESTROY = 0x0082,

		/// <summary>
		/// The WM_NCCALCSIZE message is sent when the size and position of a window's client area must be calculated. By processing this message, an application can control the content of the window's client area when the size or position of the window changes.
		/// </summary>
		WM_NCCALCSIZE = 0x0083,

		/// <summary>
		/// The WM_NCHITTEST message is sent to a window when the cursor moves, or when a mouse button is pressed or released. If the mouse is not captured, the message is sent to the window beneath the cursor. Otherwise, the message is sent to the window that has captured the mouse.
		/// </summary>
		WM_NCHITTEST = 0x0084,

		/// <summary>
		/// The WM_NCPAINT message is sent to a window when its frame must be painted. 
		/// </summary>
		WM_NCPAINT = 0x0085,

		/// <summary>
		/// The WM_NCACTIVATE message is sent to a window when its nonclient area needs to be changed to indicate an active or inactive state.
		/// </summary>
		WM_NCACTIVATE = 0x0086,

		/// <summary>
		/// The WM_GETDLGCODE message is sent to the window procedure associated with a control. By default, the system handles all keyboard input to the control; the system interprets certain types of keyboard input as dialog box navigation keys. To override this default behavior, the control can respond to the WM_GETDLGCODE message to indicate the types of input it wants to process itself.
		/// </summary>
		WM_GETDLGCODE = 0x0087,

		/// <summary>
		/// The WM_SYNCPAINT message is used to synchronize painting while avoiding linking independent GUI threads.
		/// </summary>
		WM_SYNCPAINT = 0x0088,

		/// <summary>
		/// The WM_NCMOUSEMOVE message is posted to a window when the cursor is moved within the nonclient area of the window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCMOUSEMOVE = 0x00A0,

		/// <summary>
		/// The WM_NCLBUTTONDOWN message is posted when the user presses the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCLBUTTONDOWN = 0x00A1,

		/// <summary>
		/// The WM_NCLBUTTONUP message is posted when the user releases the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCLBUTTONUP = 0x00A2,

		/// <summary>
		/// The WM_NCLBUTTONDBLCLK message is posted when the user double-clicks the left mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCLBUTTONDBLCLK = 0x00A3,

		/// <summary>
		/// The WM_NCRBUTTONDOWN message is posted when the user presses the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCRBUTTONDOWN = 0x00A4,

		/// <summary>
		/// The WM_NCRBUTTONUP message is posted when the user releases the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCRBUTTONUP = 0x00A5,

		/// <summary>
		/// The WM_NCRBUTTONDBLCLK message is posted when the user double-clicks the right mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCRBUTTONDBLCLK = 0x00A6,

		/// <summary>
		/// The WM_NCMBUTTONDOWN message is posted when the user presses the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCMBUTTONDOWN = 0x00A7,

		/// <summary>
		/// The WM_NCMBUTTONUP message is posted when the user releases the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCMBUTTONUP = 0x00A8,

		/// <summary>
		/// The WM_NCMBUTTONDBLCLK message is posted when the user double-clicks the middle mouse button while the cursor is within the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCMBUTTONDBLCLK = 0x00A9,

		/// <summary>
		/// The WM_NCXBUTTONDOWN message is posted when the user presses the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCXBUTTONDOWN = 0x00AB,

		/// <summary>
		/// The WM_NCXBUTTONUP message is posted when the user releases the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCXBUTTONUP = 0x00AC,

		/// <summary>
		/// The WM_NCXBUTTONDBLCLK message is posted when the user double-clicks the first or second X button while the cursor is in the nonclient area of a window. This message is posted to the window that contains the cursor. If a window has captured the mouse, this message is not posted.
		/// </summary>
		WM_NCXBUTTONDBLCLK = 0x00AD,

		/// <summary>
		/// The WM_INPUT_DEVICE_CHANGE message is sent to the window that registered to receive raw input. A window receives this message through its WindowProc function.
		/// </summary>
		WM_INPUT_DEVICE_CHANGE = 0x00FE,

		/// <summary>
		/// The WM_INPUT message is sent to the window that is getting raw input. 
		/// </summary>
		WM_INPUT = 0x00FF,

		/// <summary>
		/// This message filters for keyboard messages.
		/// </summary>
		WM_KEYFIRST = 0x0100,

		/// <summary>
		/// The WM_KEYDOWN message is posted to the window with the keyboard focus when a nonsystem key is pressed. A nonsystem key is a key that is pressed when the ALT key is not pressed. 
		/// </summary>
		WM_KEYDOWN = 0x0100,

		/// <summary>
		/// The WM_KEYUP message is posted to the window with the keyboard focus when a nonsystem key is released. A nonsystem key is a key that is pressed when the ALT key is not pressed, or a keyboard key that is pressed when a window has the keyboard focus. 
		/// </summary>
		WM_KEYUP = 0x0101,

		/// <summary>
		/// The WM_CHAR message is posted to the window with the keyboard focus when a WM_KEYDOWN message is translated by the TranslateMessage function. The WM_CHAR message contains the character code of the key that was pressed. 
		/// </summary>
		WM_CHAR = 0x0102,

		/// <summary>
		/// The WM_DEADCHAR message is posted to the window with the keyboard focus when a WM_KEYUP message is translated by the TranslateMessage function. WM_DEADCHAR specifies a character code generated by a dead key. A dead key is a key that generates a character, such as the umlaut (double-dot), that is combined with another character to form a composite character. For example, the umlaut-O character (Ö) is generated by typing the dead key for the umlaut character, and then typing the O key. 
		/// </summary>
		WM_DEADCHAR = 0x0103,

		/// <summary>
		/// The WM_SYSKEYDOWN message is posted to the window with the keyboard focus when the user presses the F10 key (which activates the menu bar) or holds down the ALT key and then presses another key. It also occurs when no window currently has the keyboard focus; in this case, the WM_SYSKEYDOWN message is sent to the active window. The window that receives the message can distinguish between these two contexts by checking the context code in the lParam parameter. 
		/// </summary>
		WM_SYSKEYDOWN = 0x0104,

		/// <summary>
		/// The WM_SYSKEYUP message is posted to the window with the keyboard focus when the user releases a key that was pressed while the ALT key was held down. It also occurs when no window currently has the keyboard focus; in this case, the WM_SYSKEYUP message is sent to the active window. The window that receives the message can distinguish between these two contexts by checking the context code in the lParam parameter. 
		/// </summary>
		WM_SYSKEYUP = 0x0105,

		/// <summary>
		/// The WM_SYSCHAR message is posted to the window with the keyboard focus when a WM_SYSKEYDOWN message is translated by the TranslateMessage function. It specifies the character code of a system character key — that is, a character key that is pressed while the ALT key is down. 
		/// </summary>
		WM_SYSCHAR = 0x0106,

		/// <summary>
		/// The WM_SYSDEADCHAR message is sent to the window with the keyboard focus when a WM_SYSKEYDOWN message is translated by the TranslateMessage function. WM_SYSDEADCHAR specifies the character code of a system dead key — that is, a dead key that is pressed while holding down the ALT key. 
		/// </summary>
		WM_SYSDEADCHAR = 0x0107,

		/// <summary>
		/// The WM_UNICHAR message is posted to the window with the keyboard focus when a WM_KEYDOWN message is translated by the TranslateMessage function. The WM_UNICHAR message contains the character code of the key that was pressed. 
		/// The WM_UNICHAR message is equivalent to WM_CHAR, but it uses Unicode Transformation Format (UTF)-32, whereas WM_CHAR uses UTF-16. It is designed to send or post Unicode characters to ANSI windows and it can can handle Unicode Supplementary Plane characters.
		/// </summary>
		WM_UNICHAR = 0x0109,

		/// <summary>
		/// This message filters for keyboard messages.
		/// </summary>
		WM_KEYLAST = 0x0109,

		/// <summary>
		/// Sent immediately before the IME generates the composition string as a result of a keystroke. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_STARTCOMPOSITION = 0x010D,

		/// <summary>
		/// Sent to an application when the IME ends composition. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_ENDCOMPOSITION = 0x010E,

		/// <summary>
		/// Sent to an application when the IME changes composition status as a result of a keystroke. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_COMPOSITION = 0x010F,

		WM_IME_KEYLAST = 0x010F,

		/// <summary>
		/// The WM_INITDIALOG message is sent to the dialog box procedure immediately before a dialog box is displayed. Dialog box procedures typically use this message to initialize controls and carry out any other initialization tasks that affect the appearance of the dialog box. 
		/// </summary>
		WM_INITDIALOG = 0x0110,

		/// <summary>
		/// The WM_COMMAND message is sent when the user selects a command item from a menu, when a control sends a notification message to its parent window, or when an accelerator keystroke is translated. 
		/// </summary>
		WM_COMMAND = 0x0111,

		/// <summary>
		/// A window receives this message when the user chooses a command from the Window menu, clicks the maximize button, minimize button, restore button, close button, or moves the form. You can stop the form from moving by filtering this out.
		/// </summary>
		WM_SYSCOMMAND = 0x0112,

		/// <summary>
		/// The WM_TIMER message is posted to the installing thread's message queue when a timer expires. The message is posted by the GetMessage or PeekMessage function. 
		/// </summary>
		WM_TIMER = 0x0113,

		/// <summary>
		/// The WM_HSCROLL message is sent to a window when a scroll event occurs in the window's standard horizontal scroll bar. This message is also sent to the owner of a horizontal scroll bar control when a scroll event occurs in the control. 
		/// </summary>
		WM_HSCROLL = 0x0114,

		/// <summary>
		/// The WM_VSCROLL message is sent to a window when a scroll event occurs in the window's standard vertical scroll bar. This message is also sent to the owner of a vertical scroll bar control when a scroll event occurs in the control. 
		/// </summary>
		WM_VSCROLL = 0x0115,

		/// <summary>
		/// The WM_INITMENU message is sent when a menu is about to become active. It occurs when the user clicks an item on the menu bar or presses a menu key. This allows the application to modify the menu before it is displayed. 
		/// </summary>
		WM_INITMENU = 0x0116,

		/// <summary>
		/// The WM_INITMENUPOPUP message is sent when a drop-down menu or submenu is about to become active. This allows an application to modify the menu before it is displayed, without changing the entire menu. 
		/// </summary>
		WM_INITMENUPOPUP = 0x0117,

		/// <summary>
		/// The WM_MENUSELECT message is sent to a menu's owner window when the user selects a menu item. 
		/// </summary>
		WM_MENUSELECT = 0x011F,

		/// <summary>
		/// The WM_MENUCHAR message is sent when a menu is active and the user presses a key that does not correspond to any mnemonic or accelerator key. This message is sent to the window that owns the menu. 
		/// </summary>
		WM_MENUCHAR = 0x0120,

		/// <summary>
		/// The WM_ENTERIDLE message is sent to the owner window of a modal dialog box or menu that is entering an idle state. A modal dialog box or menu enters an idle state when no messages are waiting in its queue after it has processed one or more previous messages. 
		/// </summary>
		WM_ENTERIDLE = 0x0121,

		/// <summary>
		/// The WM_MENURBUTTONUP message is sent when the user releases the right mouse button while the cursor is on a menu item. 
		/// </summary>
		WM_MENURBUTTONUP = 0x0122,

		/// <summary>
		/// The WM_MENUDRAG message is sent to the owner of a drag-and-drop menu when the user drags a menu item. 
		/// </summary>
		WM_MENUDRAG = 0x0123,

		/// <summary>
		/// The WM_MENUGETOBJECT message is sent to the owner of a drag-and-drop menu when the mouse cursor enters a menu item or moves from the center of the item to the top or bottom of the item. 
		/// </summary>
		WM_MENUGETOBJECT = 0x0124,

		/// <summary>
		/// The WM_UNINITMENUPOPUP message is sent when a drop-down menu or submenu has been destroyed. 
		/// </summary>
		WM_UNINITMENUPOPUP = 0x0125,

		/// <summary>
		/// The WM_MENUCOMMAND message is sent when the user makes a selection from a menu. 
		/// </summary>
		WM_MENUCOMMAND = 0x0126,

		/// <summary>
		/// An application sends the WM_CHANGEUISTATE message to indicate that the user interface (UI) state should be changed.
		/// </summary>
		WM_CHANGEUISTATE = 0x0127,

		/// <summary>
		/// An application sends the WM_UPDATEUISTATE message to change the user interface (UI) state for the specified window and all its child windows.
		/// </summary>
		WM_UPDATEUISTATE = 0x0128,

		/// <summary>
		/// An application sends the WM_QUERYUISTATE message to retrieve the user interface (UI) state for a window.
		/// </summary>
		WM_QUERYUISTATE = 0x0129,

		/// <summary>
		/// The WM_CTLCOLORMSGBOX message is sent to the owner window of a message box before Windows draws the message box. By responding to this message, the owner window can set the text and background colors of the message box by using the given display device context handle. 
		/// </summary>
		WM_CTLCOLORMSGBOX = 0x0132,

		/// <summary>
		/// An edit control that is not read-only or disabled sends the WM_CTLCOLOREDIT message to its parent window when the control is about to be drawn. By responding to this message, the parent window can use the specified device context handle to set the text and background colors of the edit control. 
		/// </summary>
		WM_CTLCOLOREDIT = 0x0133,

		/// <summary>
		/// Sent to the parent window of a list box before the system draws the list box. By responding to this message, the parent window can set the text and background colors of the list box by using the specified display device context handle. 
		/// </summary>
		WM_CTLCOLORLISTBOX = 0x0134,

		/// <summary>
		/// The WM_CTLCOLORBTN message is sent to the parent window of a button before drawing the button. The parent window can change the button's text and background colors. However, only owner-drawn buttons respond to the parent window processing this message. 
		/// </summary>
		WM_CTLCOLORBTN = 0x0135,

		/// <summary>
		/// The WM_CTLCOLORDLG message is sent to a dialog box before the system draws the dialog box. By responding to this message, the dialog box can set its text and background colors using the specified display device context handle. 
		/// </summary>
		WM_CTLCOLORDLG = 0x0136,

		/// <summary>
		/// The WM_CTLCOLORSCROLLBAR message is sent to the parent window of a scroll bar control when the control is about to be drawn. By responding to this message, the parent window can use the display context handle to set the background color of the scroll bar control. 
		/// </summary>
		WM_CTLCOLORSCROLLBAR = 0x0137,

		/// <summary>
		/// A static control, or an edit control that is read-only or disabled, sends the WM_CTLCOLORSTATIC message to its parent window when the control is about to be drawn. By responding to this message, the parent window can use the specified device context handle to set the text and background colors of the static control. 
		/// </summary>
		WM_CTLCOLORSTATIC = 0x0138,

		/// <summary>
		/// Use WM_MOUSEFIRST to specify the first mouse message. Use the PeekMessage() Function.
		/// </summary>
		WM_MOUSEFIRST = 0x0200,

		/// <summary>
		/// The WM_MOUSEMOVE message is posted to a window when the cursor moves. If the mouse is not captured, the message is posted to the window that contains the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_MOUSEMOVE = 0x0200,

		/// <summary>
		/// The WM_LBUTTONDOWN message is posted when the user presses the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_LBUTTONDOWN = 0x0201,

		/// <summary>
		/// The WM_LBUTTONUP message is posted when the user releases the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_LBUTTONUP = 0x0202,

		/// <summary>
		/// The WM_LBUTTONDBLCLK message is posted when the user double-clicks the left mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_LBUTTONDBLCLK = 0x0203,

		/// <summary>
		/// The WM_RBUTTONDOWN message is posted when the user presses the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_RBUTTONDOWN = 0x0204,

		/// <summary>
		/// The WM_RBUTTONUP message is posted when the user releases the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_RBUTTONUP = 0x0205,

		/// <summary>
		/// The WM_RBUTTONDBLCLK message is posted when the user double-clicks the right mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_RBUTTONDBLCLK = 0x0206,

		/// <summary>
		/// The WM_MBUTTONDOWN message is posted when the user presses the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_MBUTTONDOWN = 0x0207,

		/// <summary>
		/// The WM_MBUTTONUP message is posted when the user releases the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_MBUTTONUP = 0x0208,

		/// <summary>
		/// The WM_MBUTTONDBLCLK message is posted when the user double-clicks the middle mouse button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_MBUTTONDBLCLK = 0x0209,

		/// <summary>
		/// The WM_MOUSEWHEEL message is sent to the focus window when the mouse wheel is rotated. The DefWindowProc function propagates the message to the window's parent. There should be no internal forwarding of the message, since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
		/// </summary>
		WM_MOUSEWHEEL = 0x020A,

		/// <summary>
		/// The WM_XBUTTONDOWN message is posted when the user presses the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse. 
		/// </summary>
		WM_XBUTTONDOWN = 0x020B,

		/// <summary>
		/// The WM_XBUTTONUP message is posted when the user releases the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_XBUTTONUP = 0x020C,

		/// <summary>
		/// The WM_XBUTTONDBLCLK message is posted when the user double-clicks the first or second X button while the cursor is in the client area of a window. If the mouse is not captured, the message is posted to the window beneath the cursor. Otherwise, the message is posted to the window that has captured the mouse.
		/// </summary>
		WM_XBUTTONDBLCLK = 0x020D,

		/// <summary>
		/// The WM_MOUSEHWHEEL message is sent to the focus window when the mouse's horizontal scroll wheel is tilted or rotated. The DefWindowProc function propagates the message to the window's parent. There should be no internal forwarding of the message, since DefWindowProc propagates it up the parent chain until it finds a window that processes it.
		/// </summary>
		WM_MOUSEHWHEEL = 0x020E,

		/// <summary>
		/// Use WM_MOUSELAST to specify the last mouse message. Used with PeekMessage() Function.
		/// </summary>
		WM_MOUSELAST = 0x020E,

		/// <summary>
		/// The WM_PARENTNOTIFY message is sent to the parent of a child window when the child window is created or destroyed, or when the user clicks a mouse button while the cursor is over the child window. When the child window is being created, the system sends WM_PARENTNOTIFY just before the CreateWindow or CreateWindowEx function that creates the window returns. When the child window is being destroyed, the system sends the message before any processing to destroy the window takes place.
		/// </summary>
		WM_PARENTNOTIFY = 0x0210,

		/// <summary>
		/// The WM_ENTERMENULOOP message informs an application's main window procedure that a menu modal loop has been entered. 
		/// </summary>
		WM_ENTERMENULOOP = 0x0211,

		/// <summary>
		/// The WM_EXITMENULOOP message informs an application's main window procedure that a menu modal loop has been exited. 
		/// </summary>
		WM_EXITMENULOOP = 0x0212,

		/// <summary>
		/// The WM_NEXTMENU message is sent to an application when the right or left arrow key is used to switch between the menu bar and the system menu. 
		/// </summary>
		WM_NEXTMENU = 0x0213,

		/// <summary>
		/// The WM_SIZING message is sent to a window that the user is resizing. By processing this message, an application can monitor the size and position of the drag rectangle and, if needed, change its size or position. 
		/// </summary>
		WM_SIZING = 0x0214,

		/// <summary>
		/// The WM_CAPTURECHANGED message is sent to the window that is losing the mouse capture.
		/// </summary>
		WM_CAPTURECHANGED = 0x0215,

		/// <summary>
		/// The WM_MOVING message is sent to a window that the user is moving. By processing this message, an application can monitor the position of the drag rectangle and, if needed, change its position.
		/// </summary>
		WM_MOVING = 0x0216,

		/// <summary>
		/// Notifies applications that a power-management event has occurred.
		/// </summary>
		WM_POWERBROADCAST = 0x0218,

		/// <summary>
		/// Notifies an application of a change to the hardware configuration of a device or the computer.
		/// </summary>
		WM_DEVICECHANGE = 0x0219,

		/// <summary>
		/// An application sends the WM_MDICREATE message to a multiple-document interface (MDI) client window to create an MDI child window. 
		/// </summary>
		WM_MDICREATE = 0x0220,

		/// <summary>
		/// An application sends the WM_MDIDESTROY message to a multiple-document interface (MDI) client window to close an MDI child window. 
		/// </summary>
		WM_MDIDESTROY = 0x0221,

		/// <summary>
		/// An application sends the WM_MDIACTIVATE message to a multiple-document interface (MDI) client window to instruct the client window to activate a different MDI child window. 
		/// </summary>
		WM_MDIACTIVATE = 0x0222,

		/// <summary>
		/// An application sends the WM_MDIRESTORE message to a multiple-document interface (MDI) client window to restore an MDI child window from maximized or minimized size. 
		/// </summary>
		WM_MDIRESTORE = 0x0223,

		/// <summary>
		/// An application sends the WM_MDINEXT message to a multiple-document interface (MDI) client window to activate the next or previous child window. 
		/// </summary>
		WM_MDINEXT = 0x0224,

		/// <summary>
		/// An application sends the WM_MDIMAXIMIZE message to a multiple-document interface (MDI) client window to maximize an MDI child window. The system resizes the child window to make its client area fill the client window. The system places the child window's window menu icon in the rightmost position of the frame window's menu bar, and places the child window's restore icon in the leftmost position. The system also appends the title bar text of the child window to that of the frame window. 
		/// </summary>
		WM_MDIMAXIMIZE = 0x0225,

		/// <summary>
		/// An application sends the WM_MDITILE message to a multiple-document interface (MDI) client window to arrange all of its MDI child windows in a tile format. 
		/// </summary>
		WM_MDITILE = 0x0226,

		/// <summary>
		/// An application sends the WM_MDICASCADE message to a multiple-document interface (MDI) client window to arrange all its child windows in a cascade format. 
		/// </summary>
		WM_MDICASCADE = 0x0227,

		/// <summary>
		/// An application sends the WM_MDIICONARRANGE message to a multiple-document interface (MDI) client window to arrange all minimized MDI child windows. It does not affect child windows that are not minimized. 
		/// </summary>
		WM_MDIICONARRANGE = 0x0228,

		/// <summary>
		/// An application sends the WM_MDIGETACTIVE message to a multiple-document interface (MDI) client window to retrieve the handle to the active MDI child window. 
		/// </summary>
		WM_MDIGETACTIVE = 0x0229,

		/// <summary>
		/// An application sends the WM_MDISETMENU message to a multiple-document interface (MDI) client window to replace the entire menu of an MDI frame window, to replace the window menu of the frame window, or both. 
		/// </summary>
		WM_MDISETMENU = 0x0230,

		/// <summary>
		/// The WM_ENTERSIZEMOVE message is sent one time to a window after it enters the moving or sizing modal loop. The window enters the moving or sizing modal loop when the user clicks the window's title bar or sizing border, or when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete when DefWindowProc returns. 
		/// The system sends the WM_ENTERSIZEMOVE message regardless of whether the dragging of full windows is enabled.
		/// </summary>
		WM_ENTERSIZEMOVE = 0x0231,

		/// <summary>
		/// The WM_EXITSIZEMOVE message is sent one time to a window, after it has exited the moving or sizing modal loop. The window enters the moving or sizing modal loop when the user clicks the window's title bar or sizing border, or when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete when DefWindowProc returns. 
		/// </summary>
		WM_EXITSIZEMOVE = 0x0232,

		/// <summary>
		/// Sent when the user drops a file on the window of an application that has registered itself as a recipient of dropped files.
		/// </summary>
		WM_DROPFILES = 0x0233,

		/// <summary>
		/// An application sends the WM_MDIREFRESHMENU message to a multiple-document interface (MDI) client window to refresh the window menu of the MDI frame window. 
		/// </summary>
		WM_MDIREFRESHMENU = 0x0234,

		/// <summary>
		/// Sent to an application when a window is activated. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_SETCONTEXT = 0x0281,

		/// <summary>
		/// Sent to an application to notify it of changes to the IME window. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_NOTIFY = 0x0282,

		/// <summary>
		/// Sent by an application to direct the IME window to carry out the requested command. The application uses this message to control the IME window that it has created. To send this message, the application calls the SendMessage function with the following parameters.
		/// </summary>
		WM_IME_CONTROL = 0x0283,

		/// <summary>
		/// Sent to an application when the IME window finds no space to extend the area for the composition window. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_COMPOSITIONFULL = 0x0284,

		/// <summary>
		/// Sent to an application when the operating system is about to change the current IME. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_SELECT = 0x0285,

		/// <summary>
		/// Sent to an application when the IME gets a character of the conversion result. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_CHAR = 0x0286,

		/// <summary>
		/// Sent to an application to provide commands and request information. A window receives this message through its WindowProc function.
		/// </summary>
		WM_IME_REQUEST = 0x0288,

		/// <summary>
		/// Sent to an application by the IME to notify the application of a key press and to keep message order. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_KEYDOWN = 0x0290,

		/// <summary>
		/// Sent to an application by the IME to notify the application of a key release and to keep message order. A window receives this message through its WindowProc function. 
		/// </summary>
		WM_IME_KEYUP = 0x0291,

		/// <summary>
		/// The WM_MOUSEHOVER message is posted to a window when the cursor hovers over the client area of the window for the period of time specified in a prior call to TrackMouseEvent.
		/// </summary>
		WM_MOUSEHOVER = 0x02A1,

		/// <summary>
		/// The WM_MOUSELEAVE message is posted to a window when the cursor leaves the client area of the window specified in a prior call to TrackMouseEvent.
		/// </summary>
		WM_MOUSELEAVE = 0x02A3,

		/// <summary>
		/// The WM_NCMOUSEHOVER message is posted to a window when the cursor hovers over the nonclient area of the window for the period of time specified in a prior call to TrackMouseEvent.
		/// </summary>
		WM_NCMOUSEHOVER = 0x02A0,

		/// <summary>
		/// The WM_NCMOUSELEAVE message is posted to a window when the cursor leaves the nonclient area of the window specified in a prior call to TrackMouseEvent.
		/// </summary>
		WM_NCMOUSELEAVE = 0x02A2,

		/// <summary>
		/// The WM_WTSSESSION_CHANGE message notifies applications of changes in session state.
		/// </summary>
		WM_WTSSESSION_CHANGE = 0x02B1,

		WM_TABLET_FIRST = 0x02c0,

		WM_TABLET_LAST = 0x02df,

		/// <summary>
		/// An application sends a WM_CUT message to an edit control or combo box to delete (cut) the current selection, if any, in the edit control and copy the deleted text to the clipboard in CF_TEXT format. 
		/// </summary>
		WM_CUT = 0x0300,

		/// <summary>
		/// An application sends the WM_COPY message to an edit control or combo box to copy the current selection to the clipboard in CF_TEXT format. 
		/// </summary>
		WM_COPY = 0x0301,

		/// <summary>
		/// An application sends a WM_PASTE message to an edit control or combo box to copy the current content of the clipboard to the edit control at the current caret position. Data is inserted only if the clipboard contains data in CF_TEXT format. 
		/// </summary>
		WM_PASTE = 0x0302,

		/// <summary>
		/// An application sends a WM_CLEAR message to an edit control or combo box to delete (clear) the current selection, if any, from the edit control. 
		/// </summary>
		WM_CLEAR = 0x0303,

		/// <summary>
		/// An application sends a WM_UNDO message to an edit control to undo the last operation. When this message is sent to an edit control, the previously deleted text is restored or the previously added text is deleted.
		/// </summary>
		WM_UNDO = 0x0304,

		/// <summary>
		/// The WM_RENDERFORMAT message is sent to the clipboard owner if it has delayed rendering a specific clipboard format and if an application has requested data in that format. The clipboard owner must render data in the specified format and place it on the clipboard by calling the SetClipboardData function. 
		/// </summary>
		WM_RENDERFORMAT = 0x0305,

		/// <summary>
		/// The WM_RENDERALLFORMATS message is sent to the clipboard owner before it is destroyed, if the clipboard owner has delayed rendering one or more clipboard formats. For the content of the clipboard to remain available to other applications, the clipboard owner must render data in all the formats it is capable of generating, and place the data on the clipboard by calling the SetClipboardData function. 
		/// </summary>
		WM_RENDERALLFORMATS = 0x0306,

		/// <summary>
		/// The WM_DESTROYCLIPBOARD message is sent to the clipboard owner when a call to the EmptyClipboard function empties the clipboard. 
		/// </summary>
		WM_DESTROYCLIPBOARD = 0x0307,

		/// <summary>
		/// The WM_DRAWCLIPBOARD message is sent to the first window in the clipboard viewer chain when the content of the clipboard changes. This enables a clipboard viewer window to display the new content of the clipboard. 
		/// </summary>
		WM_DRAWCLIPBOARD = 0x0308,

		/// <summary>
		/// The WM_PAINTCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and the clipboard viewer's client area needs repainting. 
		/// </summary>
		WM_PAINTCLIPBOARD = 0x0309,

		/// <summary>
		/// The WM_VSCROLLCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and an event occurs in the clipboard viewer's vertical scroll bar. The owner should scroll the clipboard image and update the scroll bar values. 
		/// </summary>
		WM_VSCROLLCLIPBOARD = 0x030A,

		/// <summary>
		/// The WM_SIZECLIPBOARD message is sent to the clipboard owner by a clipboard viewer window when the clipboard contains data in the CF_OWNERDISPLAY format and the clipboard viewer's client area has changed size. 
		/// </summary>
		WM_SIZECLIPBOARD = 0x030B,

		/// <summary>
		/// The WM_ASKCBFORMATNAME message is sent to the clipboard owner by a clipboard viewer window to request the name of a CF_OWNERDISPLAY clipboard format.
		/// </summary>
		WM_ASKCBFORMATNAME = 0x030C,

		/// <summary>
		/// The WM_CHANGECBCHAIN message is sent to the first window in the clipboard viewer chain when a window is being removed from the chain. 
		/// </summary>
		WM_CHANGECBCHAIN = 0x030D,

		/// <summary>
		/// The WM_HSCROLLCLIPBOARD message is sent to the clipboard owner by a clipboard viewer window. This occurs when the clipboard contains data in the CF_OWNERDISPLAY format and an event occurs in the clipboard viewer's horizontal scroll bar. The owner should scroll the clipboard image and update the scroll bar values. 
		/// </summary>
		WM_HSCROLLCLIPBOARD = 0x030E,

		/// <summary>
		/// This message informs a window that it is about to receive the keyboard focus, giving the window the opportunity to realize its logical palette when it receives the focus. 
		/// </summary>
		WM_QUERYNEWPALETTE = 0x030F,

		/// <summary>
		/// The WM_PALETTEISCHANGING message informs applications that an application is going to realize its logical palette. 
		/// </summary>
		WM_PALETTEISCHANGING = 0x0310,

		/// <summary>
		/// This message is sent by the OS to all top-level and overlapped windows after the window with the keyboard focus realizes its logical palette. 
		/// This message enables windows that do not have the keyboard focus to realize their logical palettes and update their client areas.
		/// </summary>
		WM_PALETTECHANGED = 0x0311,

		/// <summary>
		/// The WM_HOTKEY message is posted when the user presses a hot key registered by the RegisterHotKey function. The message is placed at the top of the message queue associated with the thread that registered the hot key. 
		/// </summary>
		WM_HOTKEY = 0x0312,

		/// <summary>
		/// The WM_PRINT message is sent to a window to request that it draw itself in the specified device context, most commonly in a printer device context.
		/// </summary>
		WM_PRINT = 0x0317,

		/// <summary>
		/// The WM_PRINTCLIENT message is sent to a window to request that it draw its client area in the specified device context, most commonly in a printer device context.
		/// </summary>
		WM_PRINTCLIENT = 0x0318,

		/// <summary>
		/// The WM_APPCOMMAND message notifies a window that the user generated an application command event, for example, by clicking an application command button using the mouse or typing an application command key on the keyboard.
		/// </summary>
		WM_APPCOMMAND = 0x0319,

		/// <summary>
		/// The WM_THEMECHANGED message is broadcast to every window following a theme change event. Examples of theme change events are the activation of a theme, the deactivation of a theme, or a transition from one theme to another.
		/// </summary>
		WM_THEMECHANGED = 0x031A,

		/// <summary>
		/// Sent when the contents of the clipboard have changed.
		/// </summary>
		WM_CLIPBOARDUPDATE = 0x031D,

		/// <summary>
		/// The system will send a window the WM_DWMCOMPOSITIONCHANGED message to indicate that the availability of desktop composition has changed.
		/// </summary>
		WM_DWMCOMPOSITIONCHANGED = 0x031E,

		/// <summary>
		/// WM_DWMNCRENDERINGCHANGED is called when the non-client area rendering status of a window has changed. Only windows that have set the flag DWM_BLURBEHIND.fTransitionOnMaximized to true will get this message. 
		/// </summary>
		WM_DWMNCRENDERINGCHANGED = 0x031F,

		/// <summary>
		/// Sent to all top-level windows when the colorization color has changed. 
		/// </summary>
		WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320,

		/// <summary>
		/// WM_DWMWINDOWMAXIMIZEDCHANGE will let you know when a DWM composed window is maximized. You also have to register for this message as well. You'd have other windowd go opaque when this message is sent.
		/// </summary>
		WM_DWMWINDOWMAXIMIZEDCHANGE = 0x0321,

		/// <summary>
		/// Sent to request extended title bar information. A window receives this message through its WindowProc function.
		/// </summary>
		WM_GETTITLEBARINFOEX = 0x033F,

		WM_HANDHELDFIRST = 0x0358,

		WM_HANDHELDLAST = 0x035F,

		WM_AFXFIRST = 0x0360,

		WM_AFXLAST = 0x037F,

		WM_PENWINFIRST = 0x0380,

		WM_PENWINLAST = 0x038F,

		/// <summary>
		/// The WM_APP constant is used by applications to help define private messages, usually of the form WM_APP+X, where X is an integer value. 
		/// </summary>
		WM_APP = 0x8000,

		/// <summary>
		/// The WM_USER constant is used by applications to help define private messages for use by private window classes, usually of the form WM_USER+X, where X is an integer value. 
		/// </summary>
		WM_USER = 0x0400,


		/// <summary>
		/// An application sends the WM_CPL_LAUNCH message to Windows Control Panel to request that a Control Panel application be started. 
		/// </summary>
		WM_CPL_LAUNCH = WM_USER + 0x1000,

		/// <summary>
		/// The WM_CPL_LAUNCHED message is sent when a Control Panel application, started by the WM_CPL_LAUNCH message, has closed. The WM_CPL_LAUNCHED message is sent to the window identified by the wParam parameter of the WM_CPL_LAUNCH message that started the application.
		/// </summary>
		WM_CPL_LAUNCHED = WM_USER + 0x1001,

		/// <summary>
		/// WM_SYSTIMER is a well-known yet still undocumented message. Windows uses WM_SYSTIMER for internal actions like scrolling.
		/// </summary>
		WM_SYSTIMER = 0x118,



		WM_DPICHANGED = 0x02E0,

	}
}



