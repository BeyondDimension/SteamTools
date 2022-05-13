#if NET35
namespace System
{
    public static class AppContext
    {
        public static bool TryGetSwitch(string switchName, out bool isEnabled)
        {
            isEnabled = default;
            return default;
        }
    }
}
#endif
