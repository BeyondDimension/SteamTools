using System.Application.Columns;
using System.Application.Models;
using System.IO;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public interface IApiConnectionPlatformHelper
    {
        #region Authentication

        IAuthHelper Auth { get; }

        /// <summary>
        /// 保存用户登录凭证
        /// </summary>
        /// <param name="authToken"></param>
        Task SaveAuthTokenAsync(string authToken);

        /// <summary>
        /// 当登录完成时
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="response"></param>
        Task OnLoginedAsync(IReadOnlyPhoneNumber? phoneNumber, ILoginResponse response);

        #endregion

        /// <summary>
        /// 显示响应错误消息
        /// </summary>
        /// <param name="message"></param>
        void ShowResponseErrorMessage(string message);

        /// <summary>
        /// 尝试将需要[上传的文件流]处理(文件压缩，格式转换[heic->png]，由客户端平台原生实现)保存到临时路径中并返回信息
        /// </summary>
        /// <param name="imageStream"></param>
        /// <returns></returns>
        (string filePath, string mime)? TryHandleUploadFile(Stream imageFileStream, UploadFileType uploadFileType);
    }
}
