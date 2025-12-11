namespace BD.WTTS.Helpers;

internal sealed partial class AuthenticatorRegexHelper
{
    [GeneratedRegex("https?://.*")]
    internal static partial Regex SecretCodeHttpRegex();

    [GeneratedRegex(@"data:image/([^;]+);base64,(.*)", RegexOptions.IgnoreCase)]
    internal static partial Regex SecretCodeDataImageRegex();

    [GeneratedRegex(@"otpauth://([^/]+)/([^?]+)\?(.*)", RegexOptions.IgnoreCase)]
    internal static partial Regex SecretCodeOptAuthRegex();

    [GeneratedRegex(@"[^0-9a-z]", RegexOptions.IgnoreCase)]
    internal static partial Regex SecretHexCodeAuthRegex();
}
