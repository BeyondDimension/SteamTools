using ReactiveUI;
using System;
using System.Application.Services;
using System.Collections.Generic;
using System.Properties;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public class PageViewModel : ViewModelBase
    {
        protected string title = string.Empty;
        [IgnoreDataMember]
        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        protected string GetTitleByDisplayName(string displayName)
        {
            if (IsMobile)
            {
                return displayName;
            }
            else
            {
                return ThisAssembly.AssemblyTrademark + " | " + displayName;
            }
        }

        [IgnoreDataMember]
        public bool IsInitialized { get; protected set; }

        public virtual void Initialize()
        {
        }

        protected readonly IWindowManager windowManager = IWindowManager.Instance;

        [IgnoreDataMember]
        public bool IsVisible => windowManager.IsVisibleWindow(this);

        /// <summary>
        /// 关闭当前 ViewModel 绑定的窗口
        /// </summary>
        public virtual void Close() => windowManager.CloseWindow(this);

        /// <summary>
        /// 显示当前 ViewModel 绑定的窗口
        /// </summary>
        public virtual void Show() => windowManager.ShowWindow(this);

        /// <summary>
        /// 隐藏当前 ViewModel 绑定的窗口
        /// </summary>
        public virtual void Hide() => windowManager.HideWindow(this);
    }
}
