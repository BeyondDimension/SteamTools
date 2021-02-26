using System.Application.UI.ViewModels;
using System.Globalization;
using System.Linq;

namespace System.Application.Services.Implementation
{
    [Obsolete("Languages")]
    internal sealed class LocalizationServiceImpl : ILocalizationService
    {
        readonly IViewModelCollectionService vmss;

        public LocalizationServiceImpl(IViewModelCollectionService vmss)
        {
            this.vmss = vmss;
            DefaultCurrentUICulture = CultureInfo.CurrentUICulture;
        }

        public CultureInfo DefaultCurrentUICulture { get; }

        ILocalizationViewModel[]? viewModels;

        public ILocalizationViewModel[] ViewModels
        {
            get
            {
                if (viewModels == null)
                {
                    viewModels = vmss.GetViewModels<ILocalizationViewModel>().ToArray();
                }
                return viewModels;
            }
        }

        static bool IsMatch(CultureInfo cultureInfo, string cultureName)
        {
            if (string.IsNullOrWhiteSpace(cultureInfo.Name))
            {
                return false;
            }
            if (cultureInfo.Name == cultureName)
            {
                return true;
            }
            else
            {
                return IsMatch(cultureInfo.Parent, cultureName);
            }
        }

        public void ChangeLanguage(string cultureName)
        {
            if (IsMatch(CultureInfo.CurrentUICulture, cultureName)) return;
            if (!ViewModels.Any()) return;
            void ChangeLanguage()
            {
                CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
                foreach (var viewModel in ViewModels)
                {
                    viewModel.OnChangeLanguage();
                }
            }
            MainThreadDesktop.BeginInvokeOnMainThread(ChangeLanguage);
        }
    }
}