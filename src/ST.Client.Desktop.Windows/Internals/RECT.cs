using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        public RECT(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public int Width => Right - Left;

        public int Height => Bottom - Top;
    }
}