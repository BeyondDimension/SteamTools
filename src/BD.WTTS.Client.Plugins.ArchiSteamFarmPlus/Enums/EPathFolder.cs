namespace BD.WTTS.Enums;

public enum EPathFolder : byte
{
    /// <inheritdoc cref="SharedInfo.HomeDirectory"/>
    ASF,

    /// <inheritdoc cref="SharedInfo.ConfigDirectory"/>
    Config,

    /// <inheritdoc cref="SharedInfo.PluginsDirectory"/>
    Plugin,

    /// <inheritdoc cref="SharedInfo.WebsiteDirectory"/>
    WWW,

    /// <inheritdoc cref="SharedInfo.LogFileDirectory"/>
    Logs,
}
