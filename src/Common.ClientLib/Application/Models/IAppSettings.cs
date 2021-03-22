namespace System.Application.Models
{
    /// <summary>
    /// 应用设置
    /// </summary>
    public interface IAppSettings
    {
        /// <summary>
        /// https://appcenter.ms
        /// Unfortunately, this only works on [Windows / iOS / Android]
        /// </summary>
        string? AppSecretVisualStudioAppCenter { get; set; }
    }
}