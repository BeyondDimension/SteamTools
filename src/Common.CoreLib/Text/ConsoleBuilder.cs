using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Text
{
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
                int indexOf;
                var valueSpan = value.AsSpan();
                while (true)
                {
                    indexOf = valueSpan.IndexOf(newLineSpan);
                    if (indexOf != -1)
                    {
                        var index = indexOf + newLineSpan.Length;
                        lineCountIndex.Add(builder.Length + index);
                        if (MaxLine > 0 && lineCountIndex.Count > MaxLine)
                        {
                            var firstLineIndex = lineCountIndex.First();
                            builder.Remove(0, firstLineIndex);
                            lineCountIndex.RemoveAt(0);
                            for (int i = 0; i < lineCountIndex.Count; i++)
                            {
                                lineCountIndex[i] = lineCountIndex[i] - firstLineIndex;
                            }
                        }
                        valueSpan = valueSpan[index..];
                    }
                    else
                    {
                        break;
                    }
                }
                builder.Append(value);
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
}
