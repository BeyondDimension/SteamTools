using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class PageViewModel
    {
        /// <summary>
        /// 关闭当前 View
        /// </summary>
        public virtual void Close()
        {
            IShowWindowService.Instance.Pop();
        }
    }
}