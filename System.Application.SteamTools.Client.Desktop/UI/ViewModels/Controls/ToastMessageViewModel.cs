using ReactiveUI;
using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class ToastMessageViewModel : ViewModelBase
    {
        /// <summary>
        /// 吐司消息标题
        /// </summary>
        private string? _Title;
        public string? Title
        {
            get => _Title;
            set => this.RaiseAndSetIfChanged(ref _Title, value);
        }

        /// <summary>
        /// 消息
        /// </summary>
        private string? _Message;
        public string? Message
        {
            get => _Message;
            set => this.RaiseAndSetIfChanged(ref _Message, value);
        }

        /// <summary>
        /// 显示状态
        /// </summary>
        private bool _IsVisible;
        public bool IsVisible
        {
            get => _IsVisible;
            set => this.RaiseAndSetIfChanged(ref _IsVisible, value);
        }




    }
}
