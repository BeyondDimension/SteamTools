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

        public override void OnBind()
        {
            base.OnBind();

            var imageUrl = ImageUrlHelper.GetImageApiUrlById(ViewModel!.ImageId);
            binding.ivImage.SetImageSource(imageUrl);
            binding.tvName.Text = ViewModel.Name;
            ViewModel.WhenAnyValue(x => x.Enable)
                .Subscribe(value => binding.checkbox.Checked = value).AddTo(this);
        }
    }
}
