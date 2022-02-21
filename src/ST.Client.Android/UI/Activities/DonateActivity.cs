using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using System.Linq;
using System.Collections.Specialized;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;
using System.Application.UI.Resx;
using Google.Android.Material.DatePicker;
using Android.Flexbox;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(DonateActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
          LaunchMode = LaunchMode.SingleTask,
          ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class DonateActivity : BaseActivity<activity_donate, AboutPageViewModel>, DatePickerHelper.IOnPositiveButtonClickListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_donate;

        protected override AboutPageViewModel? OnCreateViewModel() => AboutPageViewModel.Instance;

        NotifyCollectionChangedEventHandler? handlerDonateList;
        DonateUserAdapter? adapter;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            R.Subscribe(() =>
            {
                Title = AppResources.About_Donate;
                if (binding == null) return;
                binding.tvTitle.Text = AppResources.About_Donate_Title;
                binding.btnUID.Text = AppResources.ClickHereCopyUIDToClipboard;
                binding.tvDesc.Text = AppResources.About_Donate_Desc;
                binding.tvDonateType.Text = AppResources.About_DonateType;
                binding.tvDonateRecord.Text = AppResources.About_DonateRecord;
                binding.tvDonateRecordPrompt.Text = AppResources.About_DonateRecord_Prompt;
                binding.tvDonateRecordNoData.Text = AppResources.About_DonateRecord_NoData;
            }).AddTo(this);

            binding.btn_afdian.Text = "爱发电";
            binding.btn_ko_fi.Text = "Ko-fi";
            binding.btn_patreon.Text = "Patreon";

            if (!ViewModel!.DonateList.Any_Nullable())
            {
                ViewModel.LoadDonateRankListData();
            }

            handlerDonateList = (_, _) =>
            {
                var hasValue = ViewModel.DonateList.Any();
                binding.rvDonateList.Visibility = hasValue ? ViewStates.Visible : ViewStates.Gone;
                binding.tvDonateRecordNoData.Visibility = !hasValue ? ViewStates.Visible : ViewStates.Gone;
            };
            ViewModel.DonateList!.CollectionChanged += handlerDonateList;

            SetDateText();

            adapter = new DonateUserAdapter(ViewModel);
            binding.rvDonateList.SetFlexboxLayoutManager(justifyContent: JustifyContent.Center);
            binding.rvDonateList.SetAdapter(adapter);

            SetOnClickListener(
                binding.btnUID,
                binding.clickDate,
                binding.btn_afdian,
                binding.btn_ko_fi,
                binding.btn_patreon);
        }

        void SetDateText() => binding!.tbDate.Text = ViewModel!.DonateFliterDate.ToString(DateTimeFormat.YearMonth);

        protected override void OnClick(View view)
        {
            base.OnClick(view);

            if (view.Id == Resource.Id.btnUID)
            {
                ViewModel!.UIDCommand.Invoke();
                return;
            }
            else if (view.Id == Resource.Id.clickDate)
            {
                this.ShowBirthDatePicker(ViewModel!.DonateFliterDate.Date);
                return;
            }
            else if (view.Id == Resource.Id.btn_afdian)
            {
                ViewModel!.OpenBrowserCommand.Invoke(UrlConstants.DonateUrl_afdian);
                return;
            }
            else if (view.Id == Resource.Id.btn_ko_fi)
            {
                ViewModel!.OpenBrowserCommand.Invoke(UrlConstants.DonateUrl_ko_fi);
                return;
            }
            else if (view.Id == Resource.Id.btn_patreon)
            {
                ViewModel!.OpenBrowserCommand.Invoke(UrlConstants.DonateUrl_patreon);
                return;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            handlerDonateList = null;
            if (binding != null)
            {
                binding.rvDonateList.SetAdapter(null);
            }
            adapter = null;
            if (ViewModel != null)
            {
                if (ViewModel.DonateList != null)
                {
                    ViewModel.DonateList.CollectionChanged -= handlerDonateList;
                    ViewModel.DonateList = null;
                }
            }
        }

        void DatePickerHelper.IOnPositiveButtonClickListener.OnPositiveButtonClick(DateTimeOffset selection)
        {
            ViewModel!.DonateFliterDate = selection.GetCurrentMonth();
            SetDateText();
        }

        void IMaterialPickerOnPositiveButtonClickListener.OnPositiveButtonClick(Java.Lang.Object p0) => this.OnPositiveButtonClick(p0);
    }
}
