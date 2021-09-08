#pragma warning disable IDE1006 // 命名样式

namespace System
{
    class CommandArguments
    {
        public string resx { get; set; } = string.Empty;

        public string lang { get; set; } = string.Empty;

        public string resxFilePath { get; set; } = string.Empty;

        protected static TCommandArguments Get<TCommandArguments>(string resxOrresxFilePath, string lang) where TCommandArguments : CommandArguments, new() => new()
        {
            resx = resxOrresxFilePath,
            lang = lang,
            resxFilePath = resxOrresxFilePath,
        };

        public static implicit operator CommandArguments(ValueTuple<string, string> value) => Get<CommandArguments>(value.Item1, value.Item2);
    }
}
#pragma warning restore IDE1006 // 命名样式