using ReactiveUI;

namespace System.Application.UI.ViewModels
{
    public abstract class IdTitleIconViewModel<TId, TIcon> : ReactiveObject
    {
        protected virtual bool IgnoreIcon { get; }

        protected TId? id;
        /// <summary>
        /// 唯一键
        /// </summary>
        public TId? Id
        {
            get => id;
            set
            {
                if (this.RaiseAndSetIfChanged2(ref id, value)) return;
                Title = GetTitleById(value);
                if (!IgnoreIcon) Icon = GetIconById(value);
                OnIdChanged(value);
            }
        }

        protected virtual void OnIdChanged(TId? value)
        {

        }

        protected string title = string.Empty;
        /// <summary>
        /// 标题文本
        /// </summary>
        public virtual string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        protected TIcon? icon;
        /// <summary>
        /// 图标
        /// </summary>
        public TIcon? Icon
        {
            get => icon;
            set => this.RaiseAndSetIfChanged(ref icon, value);
        }

        /// <summary>
        /// 根据键获取标题文本
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected abstract string GetTitleById(TId? id);

        /// <summary>
        /// 根据键获取图标
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected abstract TIcon GetIconById(TId? id);
    }
}