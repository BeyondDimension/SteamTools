using System.Runtime.InteropServices;

namespace System
{
    internal static class NativeMethods
    {
        [DllImport("Avrt.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr AvSetMmThreadCharacteristics(string taskName, ref uint taskIndex);
    }
}