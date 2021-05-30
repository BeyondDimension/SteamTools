using ReactiveUI;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Runtime.Serialization;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;

namespace System.Application.UI.ViewModels
{
    [MPObj]
    public class WindowSizePosition : ReactiveObject
    {
        int _X;
        [MPKey(0)]
        public int X
        {
            get => _X;
            set => this.RaiseAndSetIfChanged(ref _X, value);
        }

        int _Y;
        [MPKey(1)]
        public int Y
        {
            get => _Y;
            set => this.RaiseAndSetIfChanged(ref _Y, value);
        }

        double _Height;
        [MPKey(2)]
        public double Height
        {
            get => _Height;
            set => this.RaiseAndSetIfChanged(ref _Height, value);
        }

        double _Width;
        [MPKey(3)]
        public double Width
        {
            get => _Width;
            set => this.RaiseAndSetIfChanged(ref _Width, value);
        }
    }

    partial class WindowViewModel
    {
        //[DataMember]
        WindowSizePosition _SizePosition = new();
        public WindowSizePosition SizePosition
        {
            get => _SizePosition;
            set => this.RaiseAndSetIfChanged(ref _SizePosition, value);
        }

        public WindowViewModel()
        {
            var name = this.GetType().Name;

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
                         UISettings.WindowSizePositions.Value[name] = _SizePosition;
                     else
                         UISettings.WindowSizePositions.Value.Add(name, _SizePosition);
                     UISettings.WindowSizePositions.RaiseValueChanged();
                 });
        }

        [IgnoreDataMember]
        public bool IsInitialized { get; protected set; }

        /// <summary>
        /// 关闭当前viewmodel绑定的窗口
        /// </summary>
        public virtual void Close()
        {
            IShowWindowService.Instance.CloseWindow(this);
        }

        /// <summary>
        /// 显示当前viewmodel绑定的窗口
        /// </summary>
        public virtual void Show()
        {
            IShowWindowService.Instance.ShowWindow(this);
        }

        /// <summary>
        /// 隐藏当前viewmodel绑定的窗口
        /// </summary>
        public virtual void Hide()
        {
            IShowWindowService.Instance.HideWindow(this);
        }
    }
}