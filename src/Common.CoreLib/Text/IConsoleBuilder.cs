using System;
using System.Collections.Generic;
using System.Text;

namespace System.Text
{
    public interface IConsoleBuilder : IStringBuilder<IConsoleBuilder>
    {
        IConsoleBuilder IStringBuilder<IConsoleBuilder>.@this => this;

        /// <summary>
        /// 当前行数 
        /// </summary>
        int LineCount { get; }

        /// <summary>
        /// 保留最大行数
        /// </summary>
        int MaxLine { get; set; }
    }
}
