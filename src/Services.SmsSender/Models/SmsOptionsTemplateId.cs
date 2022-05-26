namespace System.Application.Models;

public class SmsOptionsTemplateId<T>
{
    /// <summary>
    /// 开发者平台分配的模板标志
    /// </summary>
    public T? Template { get; set; }

    /// <summary>
    /// 用于发送验证码的用途
    /// </summary>
    public int Type { get; set; }
}