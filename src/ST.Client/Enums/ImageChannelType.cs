using System.Application;
using System.Application.Services;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application
{
    /// <summary>
    /// 图片下载渠道类型
    /// </summary>
    public enum ImageChannelType
    {
        /// <summary>
        /// Steam 头像图片
        /// </summary>
        SteamAvatars,

        /// <summary>
        /// Steam 游戏图片
        /// </summary>
        SteamGames,

        /// <summary>
        /// Steam 市场物品图片
        /// </summary>
        SteamEconomys,

        /// <summary>
        /// Steam 成就图标
        /// </summary>
        SteamAchievementIcon,

        /// <summary>
        /// Steam 加速项目图标
        /// </summary>
        AccelerateGroup,

        /// <summary>
        /// 脚本图标
        /// </summary>
        ScriptIcon,

        /// <summary>
        /// 验证码图片
        /// </summary>
        CodeImage,

        /// <summary>
        /// 通知公告封面图
        /// </summary>
        NoticePicture,

        /// <summary>
        /// 广告图片
        /// </summary>
        Advertisement,
    }
}

namespace System
{
    public static class ImageChannelTypeEnumExtensions
    {
        /// <inheritdoc cref="IHttpService.GetImageAsync(string, string, CancellationToken)"/>
        public static Task<string?> GetImageAsync(this IHttpService httpService,
            string? requestUri,
            ImageChannelType channelType,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(requestUri)) return Task.FromResult((string?)null);
            var channelType_ = channelType.ToString();
            return httpService.GetImageAsync(requestUri, channelType_, cancellationToken);
        }

        /// <inheritdoc cref="IHttpService.GetImageStreamAsync(string, string, CancellationToken)"/>
        public static Task<Stream?> GetImageStreamAsync(this IHttpService httpService,
            string? requestUri,
            ImageChannelType channelType,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(requestUri)) return Task.FromResult((Stream?)null);
            var channelType_ = channelType.ToString();
            return httpService.GetImageStreamAsync(requestUri, channelType_, cancellationToken);
        }
    }
}