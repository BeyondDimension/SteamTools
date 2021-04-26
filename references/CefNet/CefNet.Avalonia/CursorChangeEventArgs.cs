using Avalonia.Input;
using System;
using System.ComponentModel;

namespace CefNet.Avalonia
{
	public class CursorChangeEventArgs : HandledEventArgs
	{
		public CursorChangeEventArgs(Cursor cursor, CefCursorType cursorType)
		{
			this.Cursor = cursor;
			this.CursorType = cursorType;
		}

		public Cursor Cursor { get; }

		public CefCursorType CursorType { get; }
	}
}
