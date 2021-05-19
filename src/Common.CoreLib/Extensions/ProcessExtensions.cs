using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ProcessExtensions
    {
        public static ProcessModule? TryGetMainModule(this Process process)
        {
            try
            {
                return process.MainModule;
            }
            catch
            {
                return null;
            }
        }
    }
}