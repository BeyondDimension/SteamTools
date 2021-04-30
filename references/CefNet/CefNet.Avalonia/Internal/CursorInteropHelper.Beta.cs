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

        private static IntPtr GetPlatformHandle(Cursor cursor)
        {
            if (cursor != null)
            {
                if (cursor.PlatformImpl is IPlatformHandle i)
                {
                    return i.Handle;
                }
            }
            return default;
        }

        static CursorInteropHelper()
        {
            foreach (StandardCursorType cursorType in Enum.GetValues(typeof(StandardCursorType)))
            {
                var cursor = new Cursor(cursorType);
                var handle = GetPlatformHandle(cursor);
                if (handle == default || _Cursors.ContainsKey(handle))
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