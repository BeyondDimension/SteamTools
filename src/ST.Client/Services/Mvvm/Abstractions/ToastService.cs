using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Interface = System.Application.Services.ToastService;
using BaseClass = System.Application.Services.Abstractions.ToastService;

namespace System.Application.Services
{
#pragma warning disable IDE1006 // 命名样式
    /// <inheritdoc cref="BaseClass"/>
    internal interface ToastService
#pragma warning restore IDE1006 // 命名样式
    {
        static BaseClass Current => mCurrent is BaseClass i ? i : throw new NullReferenceException("AuthService is null.");

        static BaseClass? mCurrent;
    }
}

namespace System.Application.Services.Abstractions
{
    /// <summary>
    /// 提供对显示在主窗口底部的状态栏的访问
    /// </summary>
    public abstract class ToastService : ReactiveObject, Interface
    {
        public static BaseClass Current => Interface.Current;

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

        protected ToastService()
        {
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

    /// <inheritdoc cref="BaseClass"/>
    public abstract class ToastService<T> : BaseClass, Interface where T : ToastService<T>, new()
    {
        public new static T Current
        {
            get => ((T)Interface.Current)!;
            protected set => Interface.mCurrent = value;
        }

        static ToastService()
        {
            Current = new();
        }
    }
}
