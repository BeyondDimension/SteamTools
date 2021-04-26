using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CefNet.Internal
{
	public static class CursorInteropHelper
	{
		private static readonly Dictionary<IntPtr, Cursor> _Cursors = new Dictionary<IntPtr, Cursor>();
		private static readonly Dictionary<StandardCursorType, Cursor> _StdCursors = new Dictionary<StandardCursorType, Cursor>();

		static CursorInteropHelper()
		{
			foreach(StandardCursorType cursorType in Enum.GetValues(typeof(StandardCursorType)))
			{
				var cursor = new Cursor(cursorType);
				if (_Cursors.ContainsKey(cursor.PlatformCursor.Handle))
					continue;

				_Cursors.Add(cursor.PlatformCursor.Handle, cursor);
				_StdCursors.Add(cursorType, cursor);
			}
		}

		public static Cursor Create(IntPtr cursorHandle)
		{
			if (_Cursors.TryGetValue(cursorHandle, out Cursor cursor))
				return cursor;
			return Cursor.Default;
		}

		public static Cursor Create(StandardCursorType cursorType)
		{
			Cursor cursor;
			lock (_StdCursors)
			{
				if (!_StdCursors.TryGetValue(cursorType, out cursor))
				{
					cursor = new Cursor(cursorType);
					_StdCursors[cursorType] = cursor;
				}
			}
			return cursor;
		}

	}
}
