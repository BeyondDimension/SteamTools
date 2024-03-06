namespace BD.WTTS;

/// <summary>
/// <see cref="ApiRspImpl"/> 的扩展函数
/// </summary>
public static class ApiRspImplHandleUIExtensions
{
    const string TAG = "HandleUI";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void LogError(this ApiRspBase apiRsp, string message)
    {
        var url = string.IsNullOrWhiteSpace(apiRsp.Url) ? null : apiRsp.Url.Split('?', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        Log.Error(TAG, apiRsp.ClientException, "message: {message}, url: {url}", message, url);
    }

    /// <summary>
    /// UI 上处理 <see cref="ApiRspImpl"/>，失败时显示错误消息
    /// </summary>
    /// <param name="apiRsp"></param>
    /// <param name="icon"></param>
    /// <param name="errorAppendText"></param>
    /// <param name="errorFormat"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HandleUI(this ApiRspBase apiRsp, ToastIcon icon = ToastIcon.Error, string? errorAppendText = null, string? errorFormat = null)
    {
        if (!apiRsp.IsSuccess)
        {
            //if (!apiRsp.IsDisplayed)
            //{
            //    apiRsp.IsDisplayed = true;
            IApiRsp apiRsp1 = apiRsp;
            var message = apiRsp1.GetMessageByFormat(errorFormat!, errorAppendText);
            Toast.Show(icon, message);
            apiRsp.LogError(message);
            //}
            return false;
        }
        return true;
    }

    /// <summary>
    /// UI 上处理 <see cref="ApiRspImpl{TContent}"/>，失败时显示错误消息
    /// <para>Content 为 <see langword="null"/> 时，显示 <see cref="ApiRspCode.NoResponseContent"/></para>
    /// <para>Content 为 <see cref="IExplicitHasValue"/> 且验证失败时，显示 <see cref="ApiRspCode.NoResponseContentValue"/></para>
    /// </summary>
    /// <typeparam name="TContent"></typeparam>
    /// <param name="apiRsp"></param>
    /// <param name="content"></param>
    /// <param name="icon"></param>
    /// <param name="errorAppendText"></param>
    /// <param name="errorFormat"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HandleUI<TContent>(this ApiRsp<TContent> apiRsp,
       [NotNullWhen(true)] out TContent? content, ToastIcon icon = ToastIcon.Error, string? errorAppendText = null, string? errorFormat = null)
    {
        //var isDisplayed = apiRsp.IsDisplayed;
        //apiRsp.IsDisplayed = true;
        if (!apiRsp.IsSuccess)
        {
            content = default;
            //if (!isDisplayed)
            //{
            IApiRsp apiRsp1 = apiRsp;
            var message = apiRsp1.GetMessageByFormat(errorFormat!, errorAppendText);
            Toast.Show(icon, message);
            apiRsp.LogError(message);
            //}
            return false;
        }
        content = apiRsp.Content;
        if (content is null)
        {
            //if (!isDisplayed)
            //{
            var message = ApiRspCode.NoResponseContent.GetMessage(errorAppendText, errorFormat);
            Toast.Show(icon, message);
            apiRsp.LogError(message);
            //}
            return false;
        }
        if (content is IExplicitHasValue explicitHasValue)
        {
            if (!explicitHasValue.HasValue())
            {
                //if (!isDisplayed)
                //{
                var message = ApiRspCode.NoResponseContentValue.GetMessage(errorAppendText, errorFormat);
                Toast.Show(icon, message);
                apiRsp.LogError(message);
                //}
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// UI 上处理 <see cref="ApiRspImpl{TContent}"/>，失败时显示错误消息，Content 允许为 <see langword="null"/>
    /// </summary>
    /// <typeparam name="TContent"></typeparam>
    /// <param name="apiRsp"></param>
    /// <param name="content"></param>
    /// <param name="icon"></param>
    /// <param name="errorAppendText"></param>
    /// <param name="errorFormat"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HandleUIAllowNull<TContent>(this ApiRsp<TContent> apiRsp,
       out TContent? content, ToastIcon icon = ToastIcon.Error, string? errorAppendText = null, string? errorFormat = null)
    {
        //var isDisplayed = apiRsp.IsDisplayed;
        //apiRsp.IsDisplayed = true;
        if (!apiRsp.IsSuccess)
        {
            content = default;
            //if (!isDisplayed)
            //{
            IApiRsp apiRsp1 = apiRsp;
            var message = apiRsp1.GetMessageByFormat(errorFormat!, errorAppendText);
            Toast.Show(icon, message);
            apiRsp.LogError(message);
            //}
            return false;
        }
        content = apiRsp.Content;
        return true;
    }
}
