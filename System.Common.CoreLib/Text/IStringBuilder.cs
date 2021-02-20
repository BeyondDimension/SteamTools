namespace System.Text
{
    public interface IStringBuilder
    {
        IStringBuilder Append(string? value);

        IStringBuilder AppendLine();

        IStringBuilder AppendLine(string? value);

        IStringBuilder AppendFormat(string format, params object?[] args);

        IStringBuilder AppendFormatLine(string format, params object?[] args);

        IStringBuilder AppendDateTimeNowLine(DateTime? now = null);
    }
}