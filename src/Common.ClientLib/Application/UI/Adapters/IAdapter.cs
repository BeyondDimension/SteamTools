using System.Collections.Generic;
using System.Linq;

namespace System.Application.UI.Adapters
{
    /// <summary>
    /// 适配器
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public interface IAdapter<TViewModel> : ICreateViewModels<TViewModel>, IReadOnlyViewModels<TViewModel>
    {
        /// <summary>
        /// 视图模型
        /// </summary>
        new IList<TViewModel> ViewModels { get; set; }

        IReadOnlyList<TViewModel> IReadOnlyViewModels<TViewModel>.ViewModels => AsReadOnly(ViewModels);

        public static IReadOnlyList<TViewModel> AsReadOnly(IList<TViewModel> viewModels)
        {
            if (viewModels is IReadOnlyList<TViewModel> viewModels_) return viewModels_;
            return viewModels.ToArray();
        }

        /// <summary>
        /// 列表项点击事件
        /// </summary>
        event EventHandler<ItemClickEventArgs<TViewModel>>? ItemClick;

        /// <summary>
        /// 列表项【长按】点击事件
        /// </summary>
        event EventHandler<ItemClickEventArgs<TViewModel>>? ItemLongClick;

        void NotifyDataSetChanged();

        /// <summary>
        /// 替换视图模型，默认替换后刷新
        /// </summary>
        /// <param name="newViewModels"></param>
        /// <param name="notify"></param>
        public void Replace(IEnumerable<TViewModel>? newViewModels, bool notify = true)
        {
            if (ViewModels.IsReadOnly)
            {
                if (newViewModels != null && newViewModels.Any())
                {
                    ViewModels = CreateViewModels(newViewModels);
                }
                else
                {
                    ViewModels = CreateViewModels();
                }
            }
            else
            {
                ViewModels.Clear();
                if (newViewModels != null && newViewModels.Any())
                {
                    ViewModels.AddRange(newViewModels);
                }
            }
            if (notify)
            {
                NotifyDataSetChanged();
            }
        }

        /// <summary>
        /// 添加新的视图模型，默认添加后刷新
        /// </summary>
        /// <param name="newViewModels"></param>
        /// <param name="notify"></param>
        public void AddRange(IEnumerable<TViewModel>? newViewModels, bool notify = true)
        {
            if (newViewModels != null && newViewModels.Any())
            {
                ViewModels.AddRange(newViewModels);
                if (notify)
                {
                    NotifyDataSetChanged();
                }
            }
        }

        /// <summary>
        /// 根据下标移除视图模型，默认移除后刷新
        /// </summary>
        /// <param name="index"></param>
        /// <param name="notify"></param>
        public void RemoveAt(int index, bool notify = true)
        {
            if (index >= 0 && index < ViewModels.Count)
            {
                ViewModels.RemoveAt(index);
                if (notify)
                {
                    NotifyDataSetChanged();
                }
            }
        }

        /// <summary>
        /// 根据值移除视图模型，默认移除后刷新
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="notify"></param>
        public void Remove(TViewModel vm, bool notify = true)
        {
            if (ViewModels.Remove(vm))
            {
                if (notify)
                {
                    NotifyDataSetChanged();
                }
            }
        }

        /// <summary>
        /// 根据点击参数移除视图模型，默认移除后刷新
        /// </summary>
        /// <param name="args"></param>
        /// <param name="notify"></param>
        public void Remove(ItemClickEventArgs<TViewModel> args, bool notify = true)
            => RemoveAt(args.Position, notify);
    }
}