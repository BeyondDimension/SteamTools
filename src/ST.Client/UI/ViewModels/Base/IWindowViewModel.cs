using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public interface IWindowViewModel : IPageViewModel
    {
        /// <summary>
        /// 是否支持窗口大小与位置
        /// </summary>
        public static bool IsSupportedSizePosition { protected get; set; }
            = IApplication.IsDesktopPlatform;


        public abstract void OnClosing(object? sender, CancelEventArgs e);
    }
}
