using static System.Text.StringBuilderExtensions;

namespace System.Text
{
    public interface IStringBuilder<TBuilder>
    {
#pragma warning disable IDE1006 // 命名样式
        TBuilder @this { get; }
#pragma warning restore IDE1006 // 命名样式

        TBuilder Append(string? value);

        TBuilder AppendLine()
        {
            Append(Environment.NewLine);
            return @this;
        }

        TBuilder AppendLine(string? value)
        {
            Append(value);
            AppendLine();
            return @this;
        }

        TBuilder AppendFormat(string format, params object?[] args)
        {
            Append(string.Format(format, args));
            return @this;
        }

        TBuilder AppendFormatLine(string format, params object?[] args)
        {
            AppendFormat(format, args);
            AppendLine();
            return @this;
        }

        TBuilder AppendDateTimeNowLine(DateTime now = default)
        {
            if (now == default) now = DateTime.Now;
            AppendFormatLine(AppendDateTimeNowLineFormat,
                now.ToString(DateTimeFormat.Complete));
            return @this;
        }

        string ToString();
    }

    public interface IStringBuilder : IStringBuilder<IStringBuilder>
    {
        IStringBuilder IStringBuilder<IStringBuilder>.@this => this;
    }
}