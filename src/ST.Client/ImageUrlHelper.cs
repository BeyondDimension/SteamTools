using System.Application.Services;

namespace System.Application
{
    public static class ImageUrlHelper
    {
        /// <summary>
        /// 获取图片的APIURL(相对路径)
        /// </summary>
        const string GetImageApiUrl = "{1}/api/image/{0}";

        /// <summary>
        /// 根据图片Id获取APIURL(绝对路径)
        /// </summary>
        /// <param name="imageId"></param>
        /// <returns></returns>
        public static string GetImageApiUrlById(Guid imageId) => string.Format(GetImageApiUrl, imageId, ICloudServiceClient.Instance.ApiBaseUrl);
    }
}