using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Services.Implementation
{
    partial class
#if __MOBILE__
        MobileAppUpdateServiceImpl
#else
        DesktopAppUpdateServiceImpl
#endif
    {
        protected sealed override AppVersionDTO.Download? GetByDownloadChannelSettings(IEnumerable<AppVersionDTO.Download> downloads)
        {
            var channel = GeneralSettings.UpdateChannel.Value;
            switch (channel)
            {
                case UpdateChannelType.GitHub:
                case UpdateChannelType.Gitee:
                    break;
                default:
                    channel = R.Language.StartsWith("zh") ? UpdateChannelType.Gitee : UpdateChannelType.GitHub;
                    break;
            }
            return downloads.FirstOrDefault(x => x.DownloadChannelType == channel)
                ?? downloads.FirstOrDefault();
        }
    }
}