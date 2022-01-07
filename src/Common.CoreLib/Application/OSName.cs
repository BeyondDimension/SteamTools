namespace System.Application
{
    public static class OSNames
    {
        const string WindowsDesktopBridge = "Windows Desktop Bridge";

        public enum Value : byte
        {
            UWP = 1,
            WindowsDesktopBridge,
            Windows,
            WSA,
            Android,
            iPadOS,
            iOS,
            macOS,
            tvOS,
            watchOS,
            Linux,
        }

        public static string ToString2(this Value value) => value == default ? string.Empty : value switch
        {
            Value.WindowsDesktopBridge => WindowsDesktopBridge,
            _ => value.ToString(),
        };

        public static Value Parse(string value)
        {
            if (Enum.TryParse<Value>(value, true, out var valueEnum)) return valueEnum;
            if (string.Equals(value, WindowsDesktopBridge, StringComparison.OrdinalIgnoreCase))
                return Value.WindowsDesktopBridge;
            return default;
        }
    }
}
