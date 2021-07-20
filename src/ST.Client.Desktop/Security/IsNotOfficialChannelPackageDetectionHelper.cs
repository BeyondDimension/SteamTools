using System.Application.Models;
using System.Application.UI.Resx;
using System.Windows;
using APIConst = System.Application.Services.CloudService.Constants;

namespace System.Application.Security
{
    public static class IsNotOfficialChannelPackageDetectionHelper
    {
        public static bool Check()
        {
            var value = AppSettings.IsOfficialChannelPackage;
            if (!value)
            {
                static void IsNotOfficialChannelPackageWarning()
                {
                    var text = APIConst.IsNotOfficialChannelPackageWarning;
                    var title = AppResources.Warning;
                    MessageBoxCompat.Show(text, title, MessageBoxButtonCompat.OK, MessageBoxImageCompat.Warning);
                }
                IsNotOfficialChannelPackageWarning();
            }
            return value;
        }
    }
}