// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class HttpReverseProxyMiddleware
{
    /// <summary>
    /// 查找脚本注入位置
    /// </summary>
    /// <param name="buffer_">Response.Body ByteArray</param>
    /// <param name="encoding">Response.Body Encoding</param>
    /// <param name="buffer">Response.Body Byte[]</param>
    /// <param name="insertPosition">Insert Script Xml Position</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    internal static bool FindScriptInjectInsertPosition(byte[] buffer_, Encoding encoding, out ReadOnlyMemory<byte> buffer, out int insertPosition)
    {
        buffer = buffer_.AsMemory();

        // 匹配 </...> 60 47 ... 62
        var mark_start = "</"u8.ToArray();
        var mark_end = ">"u8.ToArray();
        if (mark_start.Length <= 0 || mark_end.Length <= 0) goto notfound;

        int index_name_end = 0;
        int match_mark_end_index = 0;
        int match_mark_start_index = 0;

        for (int i = buffer_.Length - 1; i >= 0; i--) // 倒序匹配，对应之前的 LastIndexOf(string
        {
            var item = buffer_[i];
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
                        var bytes = buffer.Span[index_name_start..index_name_end];
                        var charCount = encoding.GetCharCount(bytes);
                        if (charCount == matchCharCount)
                        {
                            var body = "BODY"u8;
                            var head = "HEAD"u8;
                            if ((bytes.Length == body.Length && bytes.SequenceEqual(body, Utf8StringComparerOrdinalIgnoreCase.Instance)) ||
                                (bytes.Length == head.Length && bytes.SequenceEqual(head, Utf8StringComparerOrdinalIgnoreCase.Instance)))
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

    notfound: insertPosition = -1;
        return false;
    }

    sealed class Utf8StringComparerOrdinalIgnoreCase : IEqualityComparer<byte>
    {
        Utf8StringComparerOrdinalIgnoreCase() { }

        public static readonly Utf8StringComparerOrdinalIgnoreCase Instance = new();

        // https://www.geeksforgeeks.org/lower-case-upper-case-interesting-fact/

        static byte Convert(byte b)
        {
            int i = b;
            i &= ~32;
            return (byte)i;
        }

        bool IEqualityComparer<byte>.Equals(byte x, byte y)
            => Convert(x) == Convert(y);

        int IEqualityComparer<byte>.GetHashCode(byte obj)
            => EqualityComparer<byte>.Default.GetHashCode(Convert(obj));
    }
}