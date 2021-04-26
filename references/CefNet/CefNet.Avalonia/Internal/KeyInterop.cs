using Avalonia.Input;

#pragma warning disable CS0219

namespace CefNet.Internal
{
	/// <summary>
	/// Provides static methods to convert between Virtual-Keys and the Key enumeration.
	/// </summary>
	internal static class KeyInterop
	{
		/// <summary>
		/// Convert Key into a Virtual-Key.
		/// </summary>
		public static int VirtualKeyFromKey(Key key)
		{
			const int VK_CANCEL = 0x03;
			const int VK_BACK = 0x08;
			const int VK_CLEAR = 0x0C;
			const int VK_RETURN = 0x0D;
			const int VK_PAUSE = 0x13;
			const int VK_CAPITAL = 0x14;
			const int VK_KANA = 0x15;
			const int VK_HANGEUL = 0x15;
			const int VK_HANGUL = 0x15;
			const int VK_JUNJA = 0x17;
			const int VK_FINAL = 0x18;
			const int VK_HANJA = 0x19;
			const int VK_KANJI = 0x19;
			const int VK_ESCAPE = 0x1B;
			const int VK_CONVERT = 0x1C;
			const int VK_NONCONVERT = 0x1D;
			const int VK_ACCEPT = 0x1E;
			const int VK_MODECHANGE = 0x1F;
			const int VK_SPACE = 0x20;
			const int VK_PRIOR = 0x21;
			const int VK_NEXT = 0x22;
			const int VK_END = 0x23;
			const int VK_HOME = 0x24;
			const int VK_LEFT = 0x25;
			const int VK_UP = 0x26;
			const int VK_RIGHT = 0x27;
			const int VK_DOWN = 0x28;
			const int VK_SELECT = 0x29;
			const int VK_PRINT = 0x2A;
			const int VK_EXECUTE = 0x2B;
			const int VK_SNAPSHOT = 0x2C;
			const int VK_INSERT = 0x2D;
			const int VK_DELETE = 0x2E;
			const int VK_HELP = 0x2F;
			const int VK_0 = 0x30;
			const int VK_1 = 0x31;
			const int VK_2 = 0x32;
			const int VK_3 = 0x33;
			const int VK_4 = 0x34;
			const int VK_5 = 0x35;
			const int VK_6 = 0x36;
			const int VK_7 = 0x37;
			const int VK_8 = 0x38;
			const int VK_9 = 0x39;
			const int VK_A = 0x41;
			const int VK_B = 0x42;
			const int VK_C = 0x43;
			const int VK_D = 0x44;
			const int VK_E = 0x45;
			const int VK_F = 0x46;
			const int VK_G = 0x47;
			const int VK_H = 0x48;
			const int VK_I = 0x49;
			const int VK_J = 0x4A;
			const int VK_K = 0x4B;
			const int VK_L = 0x4C;
			const int VK_M = 0x4D;
			const int VK_N = 0x4E;
			const int VK_O = 0x4F;
			const int VK_P = 0x50;
			const int VK_Q = 0x51;
			const int VK_R = 0x52;
			const int VK_S = 0x53;
			const int VK_T = 0x54;
			const int VK_U = 0x55;
			const int VK_V = 0x56;
			const int VK_W = 0x57;
			const int VK_X = 0x58;
			const int VK_Y = 0x59;
			const int VK_Z = 0x5A;
			const int VK_LWIN = 0x5B;
			const int VK_RWIN = 0x5C;
			const int VK_APPS = 0x5D;
			const int VK_POWER = 0x5E;
			const int VK_SLEEP = 0x5F;
			const int VK_NUMPAD0 = 0x60;
			const int VK_NUMPAD1 = 0x61;
			const int VK_NUMPAD2 = 0x62;
			const int VK_NUMPAD3 = 0x63;
			const int VK_NUMPAD4 = 0x64;
			const int VK_NUMPAD5 = 0x65;
			const int VK_NUMPAD6 = 0x66;
			const int VK_NUMPAD7 = 0x67;
			const int VK_NUMPAD8 = 0x68;
			const int VK_NUMPAD9 = 0x69;
			const int VK_MULTIPLY = 0x6A;
			const int VK_ADD = 0x6B;
			const int VK_SEPARATOR = 0x6C;
			const int VK_SUBTRACT = 0x6D;
			const int VK_DECIMAL = 0x6E;
			const int VK_DIVIDE = 0x6F;
			const int VK_F1 = 0x70;
			const int VK_F2 = 0x71;
			const int VK_F3 = 0x72;
			const int VK_F4 = 0x73;
			const int VK_F5 = 0x74;
			const int VK_F6 = 0x75;
			const int VK_F7 = 0x76;
			const int VK_F8 = 0x77;
			const int VK_F9 = 0x78;
			const int VK_F10 = 0x79;
			const int VK_F11 = 0x7A;
			const int VK_F12 = 0x7B;
			const int VK_F13 = 0x7C;
			const int VK_F14 = 0x7D;
			const int VK_F15 = 0x7E;
			const int VK_F16 = 0x7F;
			const int VK_F17 = 0x80;
			const int VK_F18 = 0x81;
			const int VK_F19 = 0x82;
			const int VK_F20 = 0x83;
			const int VK_F21 = 0x84;
			const int VK_F22 = 0x85;
			const int VK_F23 = 0x86;
			const int VK_F24 = 0x87;
			const int VK_NUMLOCK = 0x90;
			const int VK_SCROLL = 0x91;
			const int VK_RSHIFT = 0xA1;
			const int VK_BROWSER_BACK = 0xA6;
			const int VK_BROWSER_FORWARD = 0xA7;
			const int VK_BROWSER_REFRESH = 0xA8;
			const int VK_BROWSER_STOP = 0xA9;
			const int VK_BROWSER_SEARCH = 0xAA;
			const int VK_BROWSER_FAVORITES = 0xAB;
			const int VK_BROWSER_HOME = 0xAC;
			const int VK_VOLUME_MUTE = 0xAD;
			const int VK_VOLUME_DOWN = 0xAE;
			const int VK_VOLUME_UP = 0xAF;
			const int VK_MEDIA_NEXT_TRACK = 0xB0;
			const int VK_MEDIA_PREV_TRACK = 0xB1;
			const int VK_MEDIA_STOP = 0xB2;
			const int VK_MEDIA_PLAY_PAUSE = 0xB3;
			const int VK_LAUNCH_MAIL = 0xB4;
			const int VK_LAUNCH_MEDIA_SELECT = 0xB5;
			const int VK_LAUNCH_APP1 = 0xB6;
			const int VK_LAUNCH_APP2 = 0xB7;
			const int VK_PROCESSKEY = 0xE5;
			const int VK_PACKET = 0xE7;
			const int VK_ATTN = 0xF6;
			const int VK_CRSEL = 0xF7;
			const int VK_EXSEL = 0xF8;
			const int VK_EREOF = 0xF9;
			const int VK_PLAY = 0xFA;
			const int VK_ZOOM = 0xFB;
			const int VK_NONAME = 0xFC;
			const int VK_PA1 = 0xFD;
			const int VK_OEM_CLEAR = 0xFE;
			const int VK_TAB = 0x09;
			const int VK_SHIFT = 0x10;
			const int VK_CONTROL = 0x11;
			const int VK_MENU = 0x12;
			const int VK_LSHIFT = 0xA0;
			const int VK_RMENU = 0xA5;
			const int VK_LMENU = 0xA4;
			const int VK_LCONTROL = 0xA2;
			const int VK_RCONTROL = 0xA3;
			const int VK_LBUTTON = 0x01;
			const int VK_RBUTTON = 0x02;
			const int VK_MBUTTON = 0x04;
			const int VK_XBUTTON1 = 0x05;
			const int VK_XBUTTON2 = 0x06;
			const int VK_OEM_1 = 0xBA;
			const int VK_OEM_PLUS = 0xBB;
			const int VK_OEM_COMMA = 0xBC;
			const int VK_OEM_MINUS = 0xBD;
			const int VK_OEM_PERIOD = 0xBE;
			const int VK_OEM_2 = 0xBF;
			const int VK_OEM_3 = 0xC0;
			const int VK_C1 = 0xC1; // ABNT_C1 (Brazilian)
			const int VK_C2 = 0xC2; // ABNT_C2 (Brazilian)
			const int VK_OEM_4 = 0xDB;
			const int VK_OEM_5 = 0xDC;
			const int VK_OEM_6 = 0xDD;
			const int VK_OEM_7 = 0xDE;
			const int VK_OEM_8 = 0xDF;
			const int VK_OEM_AX = 0xE1;
			const int VK_OEM_102 = 0xE2;
			const int VK_OEM_RESET = 0xE9;
			const int VK_OEM_JUMP = 0xEA;
			const int VK_OEM_PA1 = 0xEB;
			const int VK_OEM_PA2 = 0xEC;
			const int VK_OEM_PA3 = 0xED;
			const int VK_OEM_WSCTRL = 0xEE;
			const int VK_OEM_CUSEL = 0xEF;
			const int VK_OEM_ATTN = 0xF0;
			const int VK_OEM_FINISH = 0xF1;
			const int VK_OEM_COPY = 0xF2;
			const int VK_OEM_AUTO = 0xF3;
			const int VK_OEM_ENLW = 0xF4;
			const int VK_OEM_BACKTAB = 0xF5;


			switch (key)
			{
				case Key.Cancel:
					return VK_CANCEL;

				case Key.Back:
					return VK_BACK;

				case Key.Tab:
					return VK_TAB;

				case Key.Clear:
					return VK_CLEAR;

				case Key.Return:
					return VK_RETURN;

				case Key.Pause:
					return VK_PAUSE;

				case Key.Capital:
					return VK_CAPITAL;

				case Key.KanaMode:
					return VK_KANA;

				case Key.JunjaMode:
					return VK_JUNJA;

				case Key.FinalMode:
					return VK_FINAL;

				case Key.KanjiMode:
					return VK_KANJI;

				case Key.Escape:
					return VK_ESCAPE;

				case Key.ImeConvert:
					return VK_CONVERT;

				case Key.ImeNonConvert:
					return VK_NONCONVERT;

				case Key.ImeAccept:
					return VK_ACCEPT;

				case Key.ImeModeChange:
					return VK_MODECHANGE;

				case Key.Space:
					return VK_SPACE;

				case Key.Prior:
					return VK_PRIOR;

				case Key.Next:
					return VK_NEXT;

				case Key.End:
					return VK_END;

				case Key.Home:
					return VK_HOME;

				case Key.Left:
					return VK_LEFT;

				case Key.Up:
					return VK_UP;

				case Key.Right:
					return VK_RIGHT;

				case Key.Down:
					return VK_DOWN;

				case Key.Select:
					return VK_SELECT;

				case Key.Print:
					return VK_PRINT;

				case Key.Execute:
					return VK_EXECUTE;

				case Key.Snapshot:
					return VK_SNAPSHOT;

				case Key.Insert:
					return VK_INSERT;

				case Key.Delete:
					return VK_DELETE;

				case Key.Help:
					return VK_HELP;

				case Key.D0:
					return VK_0;

				case Key.D1:
					return VK_1;

				case Key.D2:
					return VK_2;

				case Key.D3:
					return VK_3;

				case Key.D4:
					return VK_4;

				case Key.D5:
					return VK_5;

				case Key.D6:
					return VK_6;

				case Key.D7:
					return VK_7;

				case Key.D8:
					return VK_8;

				case Key.D9:
					return VK_9;

				case Key.A:
					return VK_A;

				case Key.B:
					return VK_B;

				case Key.C:
					return VK_C;

				case Key.D:
					return VK_D;

				case Key.E:
					return VK_E;

				case Key.F:
					return VK_F;

				case Key.G:
					return VK_G;

				case Key.H:
					return VK_H;

				case Key.I:
					return VK_I;

				case Key.J:
					return VK_J;

				case Key.K:
					return VK_K;

				case Key.L:
					return VK_L;

				case Key.M:
					return VK_M;

				case Key.N:
					return VK_N;

				case Key.O:
					return VK_O;

				case Key.P:
					return VK_P;

				case Key.Q:
					return VK_Q;

				case Key.R:
					return VK_R;

				case Key.S:
					return VK_S;

				case Key.T:
					return VK_T;

				case Key.U:
					return VK_U;

				case Key.V:
					return VK_V;

				case Key.W:
					return VK_W;

				case Key.X:
					return VK_X;

				case Key.Y:
					return VK_Y;

				case Key.Z:
					return VK_Z;

				case Key.LWin:
					return VK_LWIN;

				case Key.RWin:
					return VK_RWIN;

				case Key.Apps:
					return VK_APPS;

				case Key.Sleep:
					return VK_SLEEP;

				case Key.NumPad0:
					return VK_NUMPAD0;

				case Key.NumPad1:
					return VK_NUMPAD1;

				case Key.NumPad2:
					return VK_NUMPAD2;

				case Key.NumPad3:
					return VK_NUMPAD3;

				case Key.NumPad4:
					return VK_NUMPAD4;

				case Key.NumPad5:
					return VK_NUMPAD5;

				case Key.NumPad6:
					return VK_NUMPAD6;

				case Key.NumPad7:
					return VK_NUMPAD7;

				case Key.NumPad8:
					return VK_NUMPAD8;

				case Key.NumPad9:
					return VK_NUMPAD9;

				case Key.Multiply:
					return VK_MULTIPLY;

				case Key.Add:
					return VK_ADD;

				case Key.Separator:
					return VK_SEPARATOR;

				case Key.Subtract:
					return VK_SUBTRACT;

				case Key.Decimal:
					return VK_DECIMAL;

				case Key.Divide:
					return VK_DIVIDE;

				case Key.F1:
					return VK_F1;

				case Key.F2:
					return VK_F2;

				case Key.F3:
					return VK_F3;

				case Key.F4:
					return VK_F4;

				case Key.F5:
					return VK_F5;

				case Key.F6:
					return VK_F6;

				case Key.F7:
					return VK_F7;

				case Key.F8:
					return VK_F8;

				case Key.F9:
					return VK_F9;

				case Key.F10:
					return VK_F10;

				case Key.F11:
					return VK_F11;

				case Key.F12:
					return VK_F12;

				case Key.F13:
					return VK_F13;

				case Key.F14:
					return VK_F14;

				case Key.F15:
					return VK_F15;

				case Key.F16:
					return VK_F16;

				case Key.F17:
					return VK_F17;

				case Key.F18:
					return VK_F18;

				case Key.F19:
					return VK_F19;

				case Key.F20:
					return VK_F20;

				case Key.F21:
					return VK_F21;

				case Key.F22:
					return VK_F22;

				case Key.F23:
					return VK_F23;

				case Key.F24:
					return VK_F24;

				case Key.NumLock:
					return VK_NUMLOCK;

				case Key.Scroll:
					return VK_SCROLL;

				case Key.LeftShift:
					return VK_LSHIFT;

				case Key.RightShift:
					return VK_RSHIFT;

				case Key.LeftCtrl:
					return VK_LCONTROL;

				case Key.RightCtrl:
					return VK_RCONTROL;

				case Key.LeftAlt:
					return VK_LMENU;

				case Key.RightAlt:
					return VK_RMENU;

				case Key.BrowserBack:
					return VK_BROWSER_BACK;

				case Key.BrowserForward:
					return VK_BROWSER_FORWARD;

				case Key.BrowserRefresh:
					return VK_BROWSER_REFRESH;

				case Key.BrowserStop:
					return VK_BROWSER_STOP;

				case Key.BrowserSearch:
					return VK_BROWSER_SEARCH;

				case Key.BrowserFavorites:
					return VK_BROWSER_FAVORITES;

				case Key.BrowserHome:
					return VK_BROWSER_HOME;

				case Key.VolumeMute:
					return VK_VOLUME_MUTE;

				case Key.VolumeDown:
					return VK_VOLUME_DOWN;

				case Key.VolumeUp:
					return VK_VOLUME_UP;

				case Key.MediaNextTrack:
					return VK_MEDIA_NEXT_TRACK;

				case Key.MediaPreviousTrack:
					return VK_MEDIA_PREV_TRACK;

				case Key.MediaStop:
					return VK_MEDIA_STOP;

				case Key.MediaPlayPause:
					return VK_MEDIA_PLAY_PAUSE;

				case Key.LaunchMail:
					return VK_LAUNCH_MAIL;

				case Key.SelectMedia:
					return VK_LAUNCH_MEDIA_SELECT;

				case Key.LaunchApplication1:
					return VK_LAUNCH_APP1;

				case Key.LaunchApplication2:
					return VK_LAUNCH_APP2;

				case Key.OemSemicolon:
					return VK_OEM_1;

				case Key.OemPlus:
					return VK_OEM_PLUS;

				case Key.OemComma:
					return VK_OEM_COMMA;

				case Key.OemMinus:
					return VK_OEM_MINUS;

				case Key.OemPeriod:
					return VK_OEM_PERIOD;

				case Key.OemQuestion:
					return VK_OEM_2;

				case Key.OemTilde:
					return VK_OEM_3;

				case Key.AbntC1:
					return VK_C1;

				case Key.AbntC2:
					return VK_C2;

				case Key.OemOpenBrackets:
					return VK_OEM_4;

				case Key.OemPipe:
					return VK_OEM_5;

				case Key.OemCloseBrackets:
					return VK_OEM_6;

				case Key.OemQuotes:
					return VK_OEM_7;

				case Key.Oem8:
					return VK_OEM_8;

				case Key.OemBackslash:
					return VK_OEM_102;

				case Key.ImeProcessed:
					return VK_PROCESSKEY;

				case Key.OemAttn:
					return VK_OEM_ATTN; // VK_DBE_ALPHANUMERIC

				case Key.OemFinish:
					return VK_OEM_FINISH; // VK_DBE_KATAKANA

				case Key.OemCopy:
					return VK_OEM_COPY; // VK_DBE_HIRAGANA

				case Key.OemAuto:
					return VK_OEM_AUTO; // VK_DBE_SBCSCHAR

				case Key.OemEnlw:
					return VK_OEM_ENLW; // VK_DBE_DBCSCHAR

				case Key.OemBackTab:
					return VK_OEM_BACKTAB; // VK_DBE_ROMAN

				case Key.Attn:
					return VK_ATTN; // VK_DBE_NOROMAN

				case Key.CrSel:
					return VK_CRSEL; // VK_DBE_ENTERWORDREGISTERMODE

				case Key.ExSel:
					return VK_EXSEL; // VK_DBE_ENTERIMECONFIGMODE

				case Key.EraseEof:
					return VK_EREOF; // VK_DBE_FLUSHSTRING

				case Key.Play:
					return VK_PLAY; // VK_DBE_CODEINPUT

				case Key.Zoom:
					return VK_ZOOM; // VK_DBE_NOCODEINPUT

				case Key.NoName:
					return VK_NONAME; // VK_DBE_DETERMINESTRING

				case Key.Pa1:
					return VK_PA1; // VK_ENTERDLGCONVERSIONMODE

				case Key.OemClear:
					return VK_OEM_CLEAR;

				case Key.DeadCharProcessed:
					return 0; // There is no Win32 VKey for this.

				default:
					return 0;
			}
		}
	}
}
