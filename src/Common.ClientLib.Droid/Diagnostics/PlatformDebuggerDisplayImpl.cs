using JEnum = Java.Lang.Enum;

// ReSharper disable once CheckNamespace
namespace System.Diagnostics
{
    internal sealed class PlatformDebuggerDisplayImpl : IDebuggerDisplay
    {
        string IDebuggerDisplay.GetDebuggerDisplayValue(object obj)
        {
            if (obj is JEnum @enum)
            {
                return @enum.GetJavaEnumName();
            }
            return obj.ToString();
        }
    }
}