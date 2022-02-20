using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Binding;
using ReactiveUI;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;
using System.Application.UI.Resx;
using Google.Android.Material.DatePicker;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(ExplorerActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
          LaunchMode = LaunchMode.SingleTask,
          ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class DonateActivity : BaseActivity<activity_donate, AboutPageViewModel>, DatePickerHelper.IOnPositiveButtonClickListener
    {
        protected override int? LayoutResource => Resource.Layout.activity_donate;

        protected override AboutPageViewModel? OnCreateViewModel() => AboutPageViewModel.Instance;

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
            }).AddTo(this);

            binding.btn_afdian.Text = "爱发电";
            binding.btn_ko_fi.Text = "Ko-fi";
            binding.btn_patreon.Text = "Patreon";

            if (!ViewModel!.DonateList.Any_Nullable())
            {
                ViewModel.LoadDonateRankListData();
            }

            binding.tbDate.Text = ViewModel!.DonateFliterDate.ToString(DateTimeFormat.YearMonth);

            var adapter = new DonateUserAdapter(ViewModel);
            binding.rvDonateList.SetAdapter(adapter);

            SetOnClickListener(
                binding.btnUID,
                binding.layoutDate);
        }

        protected override void OnClick(View view)
        {
            base.OnClick(view);

            if (view.Id == Resource.Id.btnUID)
            {
                ViewModel!.UIDCommand.Invoke();
                return;
            }
            else if (view.Id == Resource.Id.layoutDate)
            {
                this.ShowBirthDatePicker();
                return;
            }
        }

        void DatePickerHelper.IOnPositiveButtonClickListener.OnPositiveButtonClick(DateTimeOffset selection)
        {
            ViewModel!.DonateFliterDate = selection.GetCurrentMonth();
        }

        void IMaterialPickerOnPositiveButtonClickListener.OnPositiveButtonClick(Java.Lang.Object p0) => this.OnPositiveButtonClick(p0);
    }
}
