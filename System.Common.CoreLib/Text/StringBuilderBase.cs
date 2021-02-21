namespace System.Text
{
    public abstract class StringBuilderBase : IStringBuilder
    {
        public abstract IStringBuilder Append(string? value);

        public virtual IStringBuilder AppendLine()
        {
            Append(Environment.NewLine);
            return this;
        }

        public virtual IStringBuilder AppendLine(string? value)
        {
            Append(value);
            AppendLine();
            return this;
        }

        public virtual IStringBuilder AppendFormat(string format, params object?[] args)
        {
            Append(string.Format(format, args));
            return this;
        }

        public virtual IStringBuilder AppendFormatLine(string format, params object?[] args)
        {
            StringBuilderExtensions.AppendFormatLine((f, a) => AppendFormat(f, a), () => AppendLine(), format, args);
            return this;
        }

        public virtual IStringBuilder AppendDateTimeNowLine(DateTime? now = null)
        {
            StringBuilderExtensions.AppendDateTimeNowLine((format, args) => AppendFormatLine(format, args), now);
            return this;
        }
    }
}