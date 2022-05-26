namespace System.Text;

public static class StringBuilderExtensions
{
    public const string AppendDateTimeNowLineFormat = "当前时间：{0}";

    public static StringBuilder AppendFormatLine(this StringBuilder sb, string format, params object?[] args)
    {
        sb.AppendFormat(format, args);
        sb.AppendLine();
        return sb;
    }

    public static StringBuilder AppendDateTimeNowLine(this StringBuilder sb, DateTime now = default)
    {
        if (now == default) now = DateTime.Now;
        return sb.AppendFormatLine(AppendDateTimeNowLineFormat,
             now.ToString(DateTimeFormat.Complete));
    }

    /// <summary>
    /// 从获取行号所在下标
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="line_num"></param>
    /// <returns></returns>
    public static int GetLineIndex(this StringBuilder sb, int line_num)
    {
        // https://gist.github.com/manigandham/97bf3521ada5624aa8ca82caee0f216f
        // https://stackoverflow.com/questions/15204830/get-index-of-line-from-textbox-c-sharp

        var thisLine = 0;
#if NETCOREAPP3_0_OR_GREATER
        var pos = 0;
        foreach (var chunk in sb.GetChunks())
        {
            var span = chunk.Span;
            for (var i = 0; i < span.Length; i++)
            {
                if (thisLine == line_num)
                {
                    return pos + i;
                }

                if (span[i] == '\n')
                {
                    ++thisLine;
                }
            }

            pos += span.Length;
        }
#else
        for (int i = 0; i < sb.Length; i++)
        {
            if (thisLine == line_num)
                return i;

            if (sb[i] == '\n')
                ++thisLine;
        }
#endif

        return -1;
    }
}