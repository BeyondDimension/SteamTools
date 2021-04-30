// ReSharper disable once CheckNamespace
namespace System
{
    public static class ObjectExtensions
    {
        public static T ThrowIsNull<T>(this T? t, string parameterName) where T : class
        {
            if (t == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return t;
        }

        public static T ThrowIsNull<T>(this T? t, string parameterName) where T : struct
        {
            if (!t.HasValue)
            {
                throw new ArgumentNullException(parameterName);
            }
            return t.Value;
        }
    }
}