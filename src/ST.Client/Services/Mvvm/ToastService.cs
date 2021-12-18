using ReactiveUI;
using System;
using System.Application.Services.Implementation;
using System.Application.UI;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    /// <summary>
    /// 提供对显示在主窗口底部的状态栏的访问
    /// </summary>
    public sealed class ToastService : ReactiveObject
    {
        static ToastService? mCurrent;
        public static ToastService Current => mCurrent ?? new();

        static readonly Lazy<bool> mIsSupported = new(() => DI.Get<IToast>() is ToastImpl);
        public static bool IsSupported => mIsSupported.Value;

        private readonly Subject<string> notifier;
        private string persisitentMessage = "";
        private string notificationMessage = "";

        #region Message 变更通知

        /// <summary>
        /// 获取指示当前状态的字符串。
        /// </summary>
        public string Message
        {
            get { return notificationMessage ?? persisitentMessage; }
            set
            {
                notificationMessage = value;
                persisitentMessage = value;
                this.RaisePropertyChanged();
            }
        }

        #endregion

        /// <summary>
        /// 显示状态
        /// </summary>
        private bool _IsVisible;
        public bool IsVisible
        {
            get => _IsVisible;
            set => this.RaiseAndSetIfChanged(ref _IsVisible, value);
        }

        private ToastService()
        {
            mCurrent = this;

            notifier = new Subject<string>();
            notifier
                .Do(x =>
                {
                    notificationMessage = x;
                    RaiseMessagePropertyChanged();
                })
                .Throttle(TimeSpan.FromMilliseconds(5000))
                .Subscribe(_ =>
                {
                    notificationMessage = string.Empty;
                    RaiseMessagePropertyChanged();
                });

            this.WhenAnyValue(x => x.Message)
                     .Subscribe(x => IsVisible = !string.IsNullOrEmpty(x));
        }

        public void Set()
        {
            CloseBtn_Click();
        }

        public void Set(string message)
        {
            MainThread2.BeginInvokeOnMainThread(() => Message = message);
        }

        public void Notify(string message)
        {
            notifier.OnNext(message);
        }

        private void RaiseMessagePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(Message));
        }

        public void CloseBtn_Click()
        {
            Set("");
            IsVisible = false;
        }
    }
}
