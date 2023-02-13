// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static class ImageUrlHelper
{
    /// <summary>
    /// 获取图片的 ApiUrl (相对路径)
    /// </summary>
    const string GetImageApiUrl = "{1}/api/image/{0}";

    /// <summary>
    /// 根据图片Id获取APIURL(绝对路径)
    /// </summary>
    /// <param name="imageId"></param>
    /// <returns></returns>
    public static string? GetImageApiUrlById(Guid imageId) => imageId == default ? default : string.Format(GetImageApiUrl, imageId, IMicroServiceClient.Instance.ApiBaseUrl);
}