using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System
{
    internal static class NativeMethods
    {
        [DllImport("Avrt.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr AvSetMmThreadCharacteristics(string taskName, ref uint taskIndex);
    }
}