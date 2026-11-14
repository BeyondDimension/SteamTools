// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class HttpReverseProxyMiddleware
{
    static readonly Utf8StringComparerOrdinalIgnoreCase comparer = new();

    /// <summary>
    /// 查找脚本注入位置
    /// </summary>
    /// <param name="buffer_">Response.Body written bytes</param>
    /// <param name="encoding">Response.Body Encoding</param>
    /// <param name="buffer">Response.Body written bytes</param>
    /// <param name="insertPosition">Insert Script Xml Position</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static bool FindScriptInjectInsertPosition(ReadOnlyMemory<byte> buffer_, Encoding encoding, out ReadOnlyMemory<byte> buffer, out int insertPosition)
    {
        buffer = buffer_;
        var span = buffer_.Span;

        // 匹配 </...> 60 47 ... 62
        ReadOnlySpan<byte> mark_start = "</"u8;
        ReadOnlySpan<byte> mark_end = ">"u8;

        int index_name_end = 0;
        int match_mark_end_index = 0;
        int match_mark_start_index = 0;

        for (int i = span.Length - 1; i >= 0; i--) // 倒序匹配，对应之前的 LastIndexOf(string
        {
            var item = span[i];
            if (index_name_end == 0)
            {
                var index = mark_end.Length - 1 - match_mark_end_index;
                if (index >= 0 && index < mark_end.Length && item == mark_end[index]) // 匹配末尾
                {
                    if (item == mark_end[index])
                    {
                        match_mark_end_index++;
                        if (match_mark_end_index >= mark_end.Length)
                        {
                            if (index_name_end == 0)
                            {
                                index_name_end = i;
                                continue;
                            }
                        }
                    }
                }
            }
            else
            {
                var index = mark_start.Length - 1 - match_mark_start_index;
                if (index >= 0 && index < mark_start.Length && item == mark_start[index]) // 匹配开头
                {
                    match_mark_start_index++;
                    if (match_mark_start_index >= mark_start.Length)
                    {
                        const int matchCharCount = 4;
                        var index_name_start = i + mark_start.Length;
                        //if (encoding.GetMaxCharCount(index_name_end - index_name_start) >= matchCharCount)
                        //{
                        var bytes = span[index_name_start..index_name_end];
                        var charCount = encoding.GetCharCount(bytes);
                        if (charCount == matchCharCount)
                        {
                            var body = "BODY"u8;
                            var head = "HEAD"u8;
                            if ((bytes.Length == body.Length &&
                                bytes.SequenceEqual(body, comparer)) ||
                                (bytes.Length == head.Length &&
                                bytes.SequenceEqual(head, comparer)))
                            {
                                insertPosition = index_name_start - mark_start.Length;
                                return true;
                            }
                        }
                        //}
                        goto reset;
                    }
                }
            }

            continue;

        reset: index_name_end = match_mark_end_index = match_mark_start_index = 0;
        }

        insertPosition = -1;
        return false;
    }

    internal static bool FindScriptInjectInsertPositionForGithub(ReadOnlyMemory<byte> buffer_, Encoding encoding, out ReadOnlyMemory<byte> buffer, out int insertPosition)
    {
        buffer = buffer_;

        ReadOnlySpan<byte> mark = "<script"u8;
        var lastScriptWithSrcStart = -1;
        var span = buffer_.Span;

        if (span.Length >= mark.Length)
        {
            for (var i = 0; i <= span.Length - mark.Length; i++)
            {
                if (!EqualsAsciiIgnoreCase(span.Slice(i, mark.Length), mark))
                {
                    continue;
                }

                var afterMarkIndex = i + mark.Length;
                if (afterMarkIndex < span.Length && IsHtmlAttributeNameChar(span[afterMarkIndex]))
                {
                    continue;
                }

                var tagEndOffset = span.Slice(afterMarkIndex).IndexOf((byte)'>');
                if (tagEndOffset < 0)
                {
                    break;
                }

                var tagEndIndex = afterMarkIndex + tagEndOffset;
                var scriptTag = span.Slice(i, tagEndIndex - i + 1);
                if (ScriptTagHasSrcAttribute(scriptTag))
                {
                    lastScriptWithSrcStart = i;
                }

                i = tagEndIndex;
            }
        }

        if (lastScriptWithSrcStart >= 0)
        {
            insertPosition = lastScriptWithSrcStart;
            return true;
        }

        return FindScriptInjectInsertPosition(buffer_, encoding, out buffer, out insertPosition);
    }

    static bool ScriptTagHasSrcAttribute(ReadOnlySpan<byte> scriptTag)
    {
        ReadOnlySpan<byte> src = "src"u8;

        if (scriptTag.Length < src.Length)
            return false;

        for (var i = 0; i <= scriptTag.Length - src.Length; i++)
        {
            if (!EqualsAsciiIgnoreCase(scriptTag.Slice(i, src.Length), src))
                continue;

            if (i > 0 && IsHtmlAttributeNameChar(scriptTag[i - 1]))
                continue;

            var j = i + src.Length;
            while (j < scriptTag.Length && IsAsciiWhitespace(scriptTag[j]))
            {
                j++;
            }

            if (j < scriptTag.Length && scriptTag[j] == (byte)'=')
            {
                return true;
            }
        }

        return false;
    }

    static bool EqualsAsciiIgnoreCase(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        if (left.Length != right.Length)
            return false;

        for (var i = 0; i < left.Length; i++)
        {
            var l = ToUpperAscii(left[i]);
            var r = ToUpperAscii(right[i]);
            if (l != r)
                return false;
        }

        return true;
    }

    static byte ToUpperAscii(byte value)
    {
        if (value is >= (byte)'a' and <= (byte)'z')
            return (byte)(value - 32);
        return value;
    }

    static bool IsAsciiWhitespace(byte value) =>
        value == (byte)' ' ||
        value == (byte)'\t' ||
        value == (byte)'\r' ||
        value == (byte)'\n' ||
        value == (byte)'\f';

    static bool IsHtmlAttributeNameChar(byte value) =>
        (value is >= (byte)'a' and <= (byte)'z') ||
        (value is >= (byte)'A' and <= (byte)'Z') ||
        (value is >= (byte)'0' and <= (byte)'9') ||
        value == (byte)'-' ||
        value == (byte)'_' ||
        value == (byte)':' ||
        value == (byte)'.';
}