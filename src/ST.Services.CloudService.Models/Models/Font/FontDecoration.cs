using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public enum FontDecoration : byte
    {
        /// <summary>
        /// 无
        /// </summary>
        None=1,

        /// <summary>
        /// 下划线
        /// </summary>
        Underline=4,
        /// <summary>
        /// 删除线
        /// </summary>
        LineThrough=8
    }
}
