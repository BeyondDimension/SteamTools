using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
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