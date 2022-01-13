using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.AccelerateProjectViewHolder;
using TViewModel = System.Application.Models.AccelerateProjectDTO;
using static System.Application.UI.Resx.AppResources;
using System.Application.Services;
using System.Collections.Generic;
using DynamicData;

namespace System.Application.UI.Adapters
{
    internal sealed class AccelerateProjectAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>
    {
        public AccelerateProjectAdapter(IList<TViewModel> viewModels, ISourceList<TViewModel> sourceList) : base(viewModels, sourceList)
        {

        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_card_accelerate_project;
        }
    }

    internal sealed class AccelerateProjectViewHolder : BaseReactiveViewHolder<TViewModel>
    {
        readonly layout_card_accelerate_project binding;

        public AccelerateProjectViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();
        }
    }
}
