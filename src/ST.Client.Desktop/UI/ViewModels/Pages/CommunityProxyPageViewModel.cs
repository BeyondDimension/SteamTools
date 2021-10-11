using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class CommunityProxyPageViewModel
    {
        protected readonly IHostsFileService hostsFileService = IHostsFileService.Instance;
        protected readonly IDesktopPlatformService desktopPlatformService = IDesktopPlatformService.Instance;

        public CommunityProxyPageViewModel()
        {

        }

        protected override void OpenCertificateDir()
        {
            var fileName = $"{ThisAssembly.AssemblyProduct}.Certificate.cer";
            desktopPlatformService.OpenFolder(Path.Combine(IOPath.AppDataDirectory, fileName));
        }

        protected override void EditHostsFile()
        {
            hostsFileService.OpenFile();
        }
    }
}
