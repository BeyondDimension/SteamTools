using ReactiveUI;
using System;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public class WindowViewModel : PageViewModel
    {
        public static bool IsSupportedSizePosition { get; set; } = true;

        protected SizePosition? _SizePosition;
        public SizePosition SizePosition
        {
            get
            {
                if (_SizePosition == null) _SizePosition = new();
                return _SizePosition;
            }
            set => this.RaiseAndSetIfChanged(ref _SizePosition, value);
        }

        public WindowViewModel()
        {
            var name = GetType().Name;

            if (IsSupportedSizePosition)
            {
                if (UISettings.WindowSizePositions.Value!.ContainsKey(name))
                {
                    _SizePosition = UISettings.WindowSizePositions.Value[name];
                }

                this.WhenAnyValue(x => x.SizePosition.X, c => c.SizePosition.Y, v => v.SizePosition.Width, b => b.SizePosition.Height)
                    .Subscribe(x =>
                    {
                        if (x.Item1 == 0 && x.Item2 == 0 && x.Item3 == 0 && x.Item4 == 0)
                            return;
                        else if (UISettings.WindowSizePositions.Value!.ContainsKey(name))
                            UISettings.WindowSizePositions.Value[name] = SizePosition;
                        else
                            UISettings.WindowSizePositions.Value.TryAdd(name, SizePosition);
                        UISettings.WindowSizePositions.RaiseValueChanged();
                    });
            }
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
