using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public interface IWindowViewModel : IPageViewModel
    {
        /// <summary>
        /// 是否支持窗口大小与位置
        /// </summary>
        public static bool IsSupportedSizePosition { protected get; set; } = true;
    }
}
