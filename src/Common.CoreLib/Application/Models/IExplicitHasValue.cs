using System.Application.Models;

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
        public static bool HasValue(this IExplicitHasValue? obj)
        {
            return obj != null && obj.ExplicitHasValue();
        }
    }
}