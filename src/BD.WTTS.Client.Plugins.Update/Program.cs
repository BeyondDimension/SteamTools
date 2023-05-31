using Microsoft.AspNetCore.WebUtilities;

if (args.Length != 2)
{
    //Directory.Move()
    var sourcePath = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(args[0]));
    var destPath = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(args[1]));
}

var a = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode("QzpcUHJvZ3JhbSBGaWxlc1xBU1VT"));
Console.WriteLine(a);