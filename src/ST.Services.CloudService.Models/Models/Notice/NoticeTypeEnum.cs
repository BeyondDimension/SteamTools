using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public enum NoticeTypeEnum : byte
    {
        /// <summary>
        /// URL
        /// </summary>
        URL = 1,

        /// <summary>
        /// 文本
        /// </summary>
        Text = 4,

        /// <summary>
        /// 图片
        /// </summary>
        Picture = 8,
    }
}
