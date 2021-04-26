#if AvaloniaBeta
using Avalonia.Input;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace CefNet.Internal
{
	public static class CursorInteropHelper
	{
		private static readonly Dictionary<IntPtr, Cursor> _Cursors = new Dictionary<IntPtr, Cursor>();
		private static readonly Dictionary<StandardCursorType, Cursor> _StdCursors = new Dictionary<StandardCursorType, Cursor>();

#region Avalonia API Change see https://github.com/AvaloniaUI/Avalonia/pull/5763
		static readonly object lock_GetPlatformHandle = new object();
		static readonly Dictionary<Type, Func<ICursorImpl, IntPtr>> _GetPlatformHandle = new Dictionary<Type, Func<ICursorImpl, IntPtr>>();
		static IntPtr GetDefault(ICursorImpl _) => default;
		static IntPtr GetPlatformHandle(Cursor cursor)
		{
			if (cursor != null)
			{
				if (cursor.PlatformImpl is IPlatformHandle i)
				{
					return i.Handle;
				}
				else if (cursor.PlatformImpl != null)
				{
					var cursorType = cursor.PlatformImpl.GetType();
					if (_GetPlatformHandle.ContainsKey(cursorType))
					{
						return _GetPlatformHandle[cursorType](cursor.PlatformImpl);
					}
					else
					{
						lock (lock_GetPlatformHandle)
						{
							Func<ICursorImpl, IntPtr> value;
							var p = cursorType.GetProperty(nameof(IPlatformHandle.Handle), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeof(IntPtr), Array.Empty<Type>(), Array.Empty<ParameterModifier>());
							if (p != null)
							{
								value = c => (IntPtr)p.GetValue(c);
							}
							else
							{
								value = GetDefault;
							}
							_GetPlatformHandle.Add(cursorType, value);
							return value(cursor.PlatformImpl);
						}
					}
				}
			}

			return default;
		}
#endregion

		static CursorInteropHelper()
		{
			foreach (StandardCursorType cursorType in Enum.GetValues(typeof(StandardCursorType)))
			{
				var cursor = new Cursor(cursorType);
				var handle = GetPlatformHandle(cursor);
				if (_Cursors.ContainsKey(handle))
					continue;

				_Cursors.Add(handle, cursor);
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
#endif