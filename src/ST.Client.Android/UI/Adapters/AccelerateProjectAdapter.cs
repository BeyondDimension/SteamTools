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
using System.Linq;
using System.Collections.ObjectModel;
using System.Application.Models;

namespace System.Application.UI.Adapters
{
    internal sealed class AccelerateProjectAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>
    {
        public AccelerateProjectAdapter(ObservableCollection<TViewModel> collection) : base(collection)
        {

        }

        public AccelerateProjectAdapter(AccelerateProjectGroupDTO item) : base(item.ObservableItems!)
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

            binding.tvName.Text = ViewModel!.Name;
            binding.tvDomainName.Text = ViewModel.DomainNamesArray.FirstOrDefault();
            ViewModel.WhenAnyValue(x => x.Enable)
                .SubscribeInMainThread(value =>
                {
                    if (binding.checkbox.Checked != value)
                        binding.checkbox.Checked = value;
                }).AddTo(this);
        }
    }
}
