using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.AccelerateProjectGroupViewHolder;
using TViewModel = System.Application.Models.AccelerateProjectGroupDTO;
using static System.Application.UI.Resx.AppResources;
using System.Application.Services;
using System.Collections.Generic;
using DynamicData;
using DynamicData.Binding;
using AndroidX.VectorDrawable.Graphics.Drawable;
using Android.Graphics.Drawables;

namespace System.Application.UI.Adapters
{
    internal sealed class AccelerateProjectGroupAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>
    {
        public AccelerateProjectGroupAdapter() : this(ProxyService.Current)
        {

        }

        public AccelerateProjectGroupAdapter(ProxyService proxyService) : base(proxyService.ProxyDomainsList, proxyService.ProxyDomains)
        {

        }

        public AccelerateProjectGroupAdapter(IList<TViewModel> viewModels, ISourceList<TViewModel> sourceList) : base(viewModels, sourceList)
        {

        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_card_accelerate_project_group;
        }
    }

    internal sealed class AccelerateProjectGroupViewHolder : BaseReactiveViewHolder<TViewModel>
    {
        readonly layout_card_accelerate_project_group binding;

        public AccelerateProjectGroupViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        bool mIsOpen;
        /// <summary>
        /// 设置或获取折叠块展开状态，设置展开会使用动画
        /// </summary>
        public bool IsOpen
        {
            get => mIsOpen;
            set => SetIsOpen(value, animation: true);
        }

        /// <summary>
        /// 设置折叠块是否打开，默认禁用动画
        /// </summary>
        /// <param name="value"></param>
        /// <param name="animation"></param>
        void SetIsOpen(bool value, bool animation = false)
        {
            mIsOpen = value;
            var value2 = mIsOpen ? ViewStates.Visible : ViewStates.Gone;
            binding.divider.Visibility = value2;
            binding.rvAccelerateProject.Visibility = value2;

            var arrow_drawable = value ?
                         Resource.Drawable.ic_baseline_keyboard_arrow_up_24 :
                         Resource.Drawable.ic_baseline_keyboard_arrow_down_24;
            if (animation)
            {
                #region 箭头动画

                var drawable = AnimatedVectorDrawableCompat.Create(binding.ivArrow.Context!, value ?
                     Resource.Drawable.ic_animated_keyboard_arrow_down_24 :
                     Resource.Drawable.ic_animated_keyboard_arrow_up_24);
                binding.ivArrow.SetImageDrawable(drawable);
                drawable.RegisterAnimationCallback(new Animatable2CompatAnimationCallbackDelegate(onAnimationEnd: () =>
                {
                    drawable.ClearAnimationCallbacks();
                    binding.ivArrow.SetImageResource(arrow_drawable);
                }));
                drawable.Start();

                #endregion
            }
            else
            {
                binding.ivArrow.SetImageResource(arrow_drawable);
            }
        }

        public override void OnBind()
        {
            base.OnBind();

            var imageUrl = ImageUrlHelper.GetImageApiUrlById(ViewModel!.ImageId);
            binding.ivImage.SetImageSource(imageUrl, Resource.Dimension.accelerate_project_group_img_size);
            binding.tvName.Text = ViewModel.Name;
            ViewModel.WhenAnyValue(x => x.ThreeStateEnable)
                .Subscribe(value =>
                {
                    binding.checkbox.SetChecked(value);
                }).AddTo(this);
            binding.checkbox.CheckedChange += (_, e) =>
            {
                var isChecked = e.IsChecked;
                var value = ViewModel.ThreeStateEnable;
                if (isChecked != value)
                {
                    ViewModel.ThreeStateEnable = isChecked;
                }
            };

            SetIsOpen(false);

            binding.rvAccelerateProject.SetLinearLayoutManager();
            binding.rvAccelerateProject.AddDividerItemDecorationRes(drawableResId: Resource.Drawable.bg_divider_with_padding);

            var adapter = new AccelerateProjectAdapter(ViewModel);
            binding.rvAccelerateProject.SetAdapter(adapter);
            adapter.ItemClick += (_, e) =>
            {
                e.Current.Enable = !e.Current.Enable;
            };
        }

        sealed class Animatable2CompatAnimationCallbackDelegate : Animatable2CompatAnimationCallback
        {
            readonly Action onAnimationEnd;

            public Animatable2CompatAnimationCallbackDelegate(Action onAnimationEnd)
            {
                this.onAnimationEnd = onAnimationEnd;
            }

            public override void OnAnimationEnd(Drawable _) => onAnimationEnd();
        }

        public override View? ItemClickView => binding.itemClickView;
    }
}
