namespace System.Application.Models.NetEaseCloud;

/// <summary>
/// http://dev.netease.im/docs/product/IM%E5%8D%B3%E6%97%B6%E9%80%9A%E8%AE%AF/%E6%9C%8D%E5%8A%A1%E7%AB%AFAPI%E6%96%87%E6%A1%A3/code%E7%8A%B6%E6%80%81%E8%A1%A8
/// </summary>
public enum SendSmsResponseCode
{
    操作成功 = 200,
    被封禁 = 301,

    // ReSharper disable once InconsistentNaming
    IP限制 = 315,

    非法操作或没有权限 = 403,
    对象不存在 = 404,

    /// <summary>
    /// 验证失败(短信服务)
    /// </summary>
    验证失败 = 413,

    参数错误 = 414,
    频率控制 = 416,
    服务器内部错误 = 500
}