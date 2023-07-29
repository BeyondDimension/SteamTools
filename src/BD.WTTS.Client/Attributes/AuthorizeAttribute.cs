// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// 指定应用此特性的类需要指定的授权
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class AuthorizeAttribute : Attribute
{
    public static bool HasAuthorize(object obj)
        => obj.GetType().GetCustomAttributes<AuthorizeAttribute>(true).Any();
}