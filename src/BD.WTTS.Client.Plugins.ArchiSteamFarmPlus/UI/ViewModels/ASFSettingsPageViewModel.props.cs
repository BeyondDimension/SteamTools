namespace BD.WTTS.UI.ViewModels;

partial class ASFSettingsPageViewModel
{
    /// <summary>
    /// 通过配置文件导入 ASF 全局配置
    /// </summary>
    public ICommand SelectGlobalFiles { get; }

    /// <summary>
    /// 设置 ASF 加密密钥
    /// </summary>
    public ICommand SetEncryptionKey { get; }

    /// <summary>
    /// 打开 ASF 文件夹
    /// </summary>
    public ICommand OpenASFFolder { get; }

    /// <summary>
    /// 刷新全局配置
    /// </summary>
    public ICommand RefreshGlobalConfig { get; }
}
