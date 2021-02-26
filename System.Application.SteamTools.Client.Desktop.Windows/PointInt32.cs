using System.Runtime.InteropServices;

namespace System
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    internal struct PointInt32
    {
        public int X;
        public int Y;

        public PointInt32(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}