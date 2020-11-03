using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Core.Common
{
    public static class Common
    {
        /// <summary>
        /// 返回当前时间戳
        /// </summary>
        /// <returns></returns>
        public static long CurrentMillis(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// 返回当前最后相对URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetCurrentPath(this string url)
        {
            int index = url.LastIndexOf("/");

            if (index != -1)
            {
                return url.Substring(0, index) + "/";
            }
            else
            {
                return "";
            }
        }
    }
}
