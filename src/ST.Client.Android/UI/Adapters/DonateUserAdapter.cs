using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using TViewHolder = System.Application.UI.Adapters.DonateUserViewHolder;
using TViewModel = System.Application.Models.RankingResponse;
using static System.Application.UI.Resx.AppResources;
using System.Application.Services;
using System.Collections.Generic;
using DynamicData;
using System.Linq;
using System.Collections.ObjectModel;
using System.Application.Models;

namespace System.Application.UI.Adapters
{
    sealed class DonateUserAdapter : BaseReactiveRecycleViewAdapter<TViewHolder, TViewModel>
    {
        public DonateUserAdapter(ObservableCollection<TViewModel> collection) : base(collection)
        {

        }

        public DonateUserAdapter(AboutPageViewModel viewModel) : base(viewModel.DonateList!)
        {

        }

        public override int? GetLayoutResource(int viewType)
        {
            return Resource.Layout.layout_donate_user;
        }
    }

    sealed class DonateUserViewHolder : BaseReactiveViewHolder<TViewModel>
    {
        readonly layout_donate_user binding;

        public DonateUserViewHolder(View itemView) : base(itemView)
        {
            binding = new(itemView);
        }

        public override void OnBind()
        {
            base.OnBind();

            binding.ivAvatar.SetImageSource(ViewModel!.Avatar, Resource.Dimension.img_size_donate_user_avatar);
            binding.tvNickName.Text = ViewModel.Name;
            binding.tvValue.Text = ViewModel.Amount.ToString("C", ViewModel.CurrencyCode.GetCultureInfo());
        }
    }
}
