using System.Application.Models;
using System.Diagnostics.CodeAnalysis;

namespace System.Application.Models
{
    /// <summary>
    /// HasValue 接口定义，通常显示实现此接口
    /// </summary>
    public interface IExplicitHasValue
    {
        bool ExplicitHasValue();
    }
}

namespace System
{
    public static class ExplicitHasValueExtensions
    {
        public static bool HasValue([NotNullWhen(true)] this IExplicitHasValue? obj)
        {
            return obj != null && obj.ExplicitHasValue();
        }
    }
}