#if NET35
using System.Diagnostics.CodeAnalysis;

namespace System
{
    public static class Version2
    {
        public static bool TryParse([NotNullWhen(true)] string? input, [NotNullWhen(true)] out Version? result)
        {
            try
            {
                result = new Version(input);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
#endif