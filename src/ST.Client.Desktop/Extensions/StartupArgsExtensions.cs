// ReSharper disable once CheckNamespace
namespace System
{
    public static class StartupArgsExtensions
    {
        public static bool ContainsArg(this string[] args, string arg)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], arg, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ContainsArg(this string[] args, string arg, out string value)
        {
            value = "";
            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], arg, StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length > i + 1)
                    {
                        value = args[i + 1];
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ContainsArg(this string[] args, string arg, out int value)
        {
            value = 0;
            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], arg, StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length > i + 1 && int.TryParse(args[i + 1], out int val))
                    {
                        value = val;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}