using System.Runtime.InteropServices;

namespace System
{
    [StructLayout(LayoutKind.Sequential)]
    internal class RECT
    {
        public int left;
        public int top;
        public int width;
        public int height;
    }
}