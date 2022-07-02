using NUnit.Framework;
using System.Text;

namespace System.Application;

[TestFixture]
public class BytesTest
{
    [Test]
    public async Task ScriptInject()
    {
        //var htmlString = await new HttpClient().GetStringAsync("https://store.steampowered.com/");
        const string htmlString = "<html><head></head><body><img src=\"https://store.st.dl.eccdnx.com/public/shared/images/header/logo_steam.svg?t=962016\" width=\"176\" height=\"44\"><div></div></body></html>";

        var encoding = Encoding.UTF8;
        var buffer_ = encoding.GetBytes(htmlString);

        if (FindScriptInjectInsertPosition(buffer_, encoding, out var buffer, out var position))
        {
            using var bodyCoreWriter = new MemoryStream();
            await bodyCoreWriter.WriteAsync(buffer[..position]);
            const string scriptXmlStart = "<script type=\"text/javascript\" src=\"https://local.steampp.net/";
            const string scriptXmlEnd = "\"></script>";
            var scriptXmlStartBytes = encoding.GetBytes(scriptXmlStart);
            var scriptXmlEndBytes = encoding.GetBytes(scriptXmlEnd);

            await bodyCoreWriter.WriteAsync(scriptXmlStartBytes);
            await bodyCoreWriter.WriteAsync(encoding.GetBytes("2"));
            await bodyCoreWriter.WriteAsync(scriptXmlEndBytes);

            await bodyCoreWriter.WriteAsync(buffer[position..]);

            var data = bodyCoreWriter.ToArray();
            var htmlString2 = encoding.GetString(data);
            TestContext.WriteLine(htmlString2);
        }
        else
        {
            throw new Exception("FindScriptInjectInsertPosition fail.");
        }
    }

    static readonly object marksLock = new();
    static readonly Dictionary<int, (byte[] mark_start, byte[] mark_end)> marksDict = new();

    static (byte[] mark_start, byte[] mark_end) GetMarks(Encoding encoding)
    {
        var codePage = encoding.CodePage;
        if (marksDict.ContainsKey(codePage)) return marksDict[codePage];
        lock (marksLock)
        {
            var mark_start = encoding.GetBytes("</");
            var mark_end = encoding.GetBytes(">");
            var value = (mark_start, mark_end);
            marksDict.Add(codePage, value);
            return value;
        }
    }

    /// <summary>
    /// 查找脚本注入位置
    /// </summary>
    /// <param name="buffer_">Response.Body ByteArray</param>
    /// <param name="encoding">Response.Body Encoding</param>
    /// <param name="buffer">Response.Body Byte[]</param>
    /// <param name="insertPosition">Insert Script Xml Position</param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    static bool FindScriptInjectInsertPosition(byte[] buffer_, Encoding encoding, out ReadOnlyMemory<byte> buffer, out int insertPosition)
    {
        buffer = buffer_.AsMemory();

        // 匹配 </...> 60 47 ... 62
        (var mark_start, var mark_end) = GetMarks(encoding);
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
                        var index_name_start = i + mark_start.Length;
                        var name = encoding.GetString(buffer.Span[index_name_start..index_name_end]);
                        if (name.Equals("body", StringComparison.OrdinalIgnoreCase) ||
                            name.Equals("head", StringComparison.OrdinalIgnoreCase))
                        {
                            insertPosition = index_name_start - mark_start.Length;
                            return true;
                        }
                        else
                        {
                            goto reset;
                        }
                    }
                }
            }

            continue;


        reset: index_name_end = match_mark_end_index = match_mark_start_index = 0;
        }

    notfound: insertPosition = -1;
        return false;
    }
}
