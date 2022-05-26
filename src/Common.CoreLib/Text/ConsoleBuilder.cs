namespace System.Text;

public sealed class ConsoleBuilder : IConsoleBuilder, IStringBuilder
{
    readonly StringBuilder builder;
    readonly List<int> lineCountIndex = new();

    public int MaxLine { get; set; }

    public ConsoleBuilder() : this(new())
    {

    }

    ConsoleBuilder(StringBuilder builder)
    {
        this.builder = builder;
    }

    void Append(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var newLineSpan = Environment.NewLine.AsSpan();
            var valueSpan = value.AsSpan();
            var bLength = builder.Length;
            builder.Append(value);
            int indexOf, indexSub = 0;
            while (true)
            {
                indexOf = valueSpan.IndexOf(newLineSpan); // 查找当前追加文本中的换行符
                if (indexOf != -1)
                {
                    var index = indexOf + newLineSpan.Length;
                    var lineCountIndexValue = bLength + index + indexSub;
                    lineCountIndex.Add(lineCountIndexValue); // 当前换行符末尾下标
                    if (MaxLine > 0 && lineCountIndex.Count > MaxLine) // 超过最大行数限制，移除第一行
                    {
                        var firstLineIndex = lineCountIndex.First();
                        builder.Remove(0, firstLineIndex); // 移除字符串
                        bLength -= firstLineIndex; // 修正字符串长度
                        lineCountIndex.RemoveAt(0); // 移除行号下标
                        for (int i = 0; i < lineCountIndex.Count; i++) // 修正行号下标
                        {
                            lineCountIndex[i] = lineCountIndex[i] - firstLineIndex;
                        }
                    }
                    valueSpan = valueSpan[index..]; // SubString
                    indexSub += index; // 字符串截取后需要将删减的数量下次下标计算时候加上
                }
                else
                {
                    break;
                }
            }
        }
    }

    IConsoleBuilder IStringBuilder<IConsoleBuilder>.Append(string? value)
    {
        Append(value);
        return this;
    }

    IStringBuilder IStringBuilder<IStringBuilder>.Append(string? value)
    {
        Append(value);
        return this;
    }

    int IConsoleBuilder.LineCount => lineCountIndex.Count;

    public override string ToString() => builder.ToString().TrimEnd();
}
