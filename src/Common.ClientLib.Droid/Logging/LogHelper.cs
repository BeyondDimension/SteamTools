using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Logging
{
    internal static class LogHelper
    {
        /// <summary>
        /// Android 的 Log Tag 名称最大长度
        /// <para>如果超出此长度，会引发异常：</para>
        /// <para>Java.Lang.IllegalArgumentException: Log tag "Microsoft.EntityFrameworkCore.Infrastructure" exceeds limit of 23 characters</para>
        /// </summary>
        public const int droid_tag_max_len = 23;
        public const string Droid = "Droid";

        public static string CutDroidTag(string name)
        {
            var array = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var lastItem = array.LastOrDefault(x => !string.IsNullOrWhiteSpace(x));
            if (lastItem != null)
            {
                if (lastItem.StartsWith(Droid, StringComparison.OrdinalIgnoreCase))
                {
                    lastItem = lastItem[Droid.Length..];
                }
                if (lastItem.Length > droid_tag_max_len)
                {
                    return lastItem.Substring(0, droid_tag_max_len);
                }
                else
                {
                    return lastItem;
                }
            }
            else
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// 根据 Name 获取 Android Log Tag
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public static string GetDroidTag(string name)
        {
            if (name.Length > droid_tag_max_len)
            {
                return CutDroidTag(name);
            }
            return name;
        }
    }
}