using System.Linq;

namespace System
{
    public static class String2
    {
        public static bool IsNullOrWhiteSpace(this string? value)
        {
            // https://referencesource.microsoft.com/#mscorlib/system/string.cs,55e241b6143365ef

            if (value == null) return true;

            for (int i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i])) return false;
            }

            return true;
        }

        public static string TryFormat(string format, params string[] args)
        {
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                args = new[] { format }.Concat(args).ToArray();
                return string.Join(", ", args);
            }
        }
    }
}
