using System.Diagnostics.CodeAnalysis;

namespace System.Application.Services;

/// <summary>
/// 模型验证
/// </summary>
public interface IModelValidator
{
    /// <summary>
    /// 验证模型，返回结果以及错误消息
    /// </summary>
    /// <param name="model"></param>
    /// <param name="errorMessage"></param>
    /// <param name="ignores"></param>
    /// <returns></returns>
    bool Validate(object model, [NotNullWhen(false)] out string? errorMessage, params Type[] ignores);

    /// <inheritdoc cref="Validate(object, out string?, Type[])"/>
    public bool Validate(object model, params Type[] ignores) => Validate(model, out var _, ignores);
}