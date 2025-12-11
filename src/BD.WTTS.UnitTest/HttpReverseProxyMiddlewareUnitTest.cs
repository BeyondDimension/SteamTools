namespace BD.WTTS.UnitTest;

public sealed class HttpReverseProxyMiddlewareUnitTest
{
    [SetUp]
    public void Setup()
    {
    }

    /// <summary>
    /// 测试查找脚本注入位置
    /// </summary>
    [Test]
    public void TestFindScriptInjectInsertPosition()
    {
        var buffer = """
            <!DOCTYPE html>
            <html>
            <head>
            <meta charset="utf-8">
            <title></title>
            </head>
            <body>
            <h1>xxxx</h1>
            <p>yyyy</p>
            </body>
            </html>
            """u8;

        var encoding = Encoding.UTF8;
        HttpReverseProxyMiddleware.FindScriptInjectInsertPosition(buffer.ToArray(), encoding, out var _, out var position);
        Assert.That(position, Is.GreaterThan(0));

        var html_start = buffer[..position];
        var script_xml_start = "<script type=\"text/javascript\" src=\"https://local.steampp.net/"u8.ToArray();
        var script_xml_end = "\"></script>"u8.ToArray();

        using var s = new MemoryStream();
        s.Write(html_start);
        s.Write(script_xml_start);
        s.Write(encoding.GetBytes("1"));
        s.Write(script_xml_end);
        var html_end = buffer[position..];
        s.Write(html_end);

        var new_html = encoding.GetString(s.ToArray());
        TestContext.WriteLine(new_html);
    }
}