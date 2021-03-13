using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct PointInt32
    {
        public int X;
        public int Y;

        public PointInt32(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [Serializable]
    public struct PointDouble
    {
        public double X { get; set; }

        public double Y { get; set; }

        public PointDouble(double x, double y)
        {
            X = x;
            Y = y;
        }
    }
}