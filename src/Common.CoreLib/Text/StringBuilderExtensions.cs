namespace System.Text
{
    public static class StringBuilderExtensions
    {
        internal static void AppendFormatLine(
           Action<string, object?[]> appendFormat,
           Action appendLine,
           string format,
           params object?[] args)
        {
            appendFormat(format, args);
            appendLine();
        }

        internal static void AppendDateTimeNowLine(Action<string, object?[]> appendFormatLine, DateTime? now = null)
        {
            now ??= DateTime.Now;
            AppendFormatLine("当前时间：{0}", now.Value.ToString(DateTimeFormat.Complete));
            void AppendFormatLine(string format, params object[] args) => appendFormatLine(format, args);
        }

        public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object?[] args)
        {
            AppendFormatLine((f, a) => sb.AppendFormat(f, a), () => sb.AppendLine(), format, args);
            return sb;
        }

        public static StringBuilder AppendDateTimeNowLine(this StringBuilder sb, DateTime? now = null)
        {
            AppendDateTimeNowLine((format, args) => sb.AppendFormatLine(format, args), now);
            return sb;
        }
    }
}