using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class ProxyScriptManagePageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.ScriptConfig;
            protected set { throw new NotImplementedException(); }
        }

        public override void Activation()
        {
            if (IsFirstActivation)
                if (ProxySettings.IsAutoCheckScriptUpdate)
                    ProxyService.Current.CheckUpdate();
            base.Activation();
        }


        private readonly ReadOnlyObservableCollection<ScriptDTO> _ProxyScripts;
        public ReadOnlyObservableCollection<ScriptDTO> ProxyScripts => _ProxyScripts;

        private Func<ScriptDTO, bool> ScriptFilter(string? serachText)
        {
            return s =>
            {
                if (s == null)
                    return false;
                if (string.IsNullOrEmpty(serachText))
                    return true;
                if (s.Name.Contains(serachText, StringComparison.OrdinalIgnoreCase) ||
                       s.Author.Contains(serachText, StringComparison.OrdinalIgnoreCase) ||
                       s.Description.Contains(serachText, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            };
        }

        public ReactiveCommand<Unit, Unit> EnableScriptAutoUpdateCommand { get; }

        public MenuItemViewModel? ScriptAutoUpdate { get; }

        public ReactiveCommand<Unit, Unit> OnlySteamBrowserCommand { get; }
        public MenuItemViewModel? OnlySteamBrowser { get; }
        public ProxyScriptManagePageViewModel()
        {
            IconKey = nameof(ProxyScriptManagePageViewModel).Replace("ViewModel", "Svg");

            ScriptStoreCommand = ReactiveCommand.Create(OpenScriptStoreWindow);
            EnableScriptAutoUpdateCommand = ReactiveCommand.Create(() =>
            {
                ScriptAutoUpdate?.CheckmarkChange(ProxySettings.IsAutoCheckScriptUpdate.Value = !ProxySettings.IsAutoCheckScriptUpdate.Value);
            });
            OnlySteamBrowserCommand = ReactiveCommand.Create(() =>
            {
                OnlySteamBrowser?.CheckmarkChange(ProxyService.Current.IsOnlyWorkSteamBrowser = !ProxyService.Current.IsOnlyWorkSteamBrowser);
            });
            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
				   //new MenuItemViewModel (nameof(AppResources.CommunityFix_EnableScriptService)),
				   new MenuItemViewModel (nameof(AppResources.ScriptStore)){
                       IconKey ="JavaScriptDrawing",Command=ScriptStoreCommand},
                   new MenuItemViewModel (),
                   (ScriptAutoUpdate=new MenuItemViewModel (nameof(AppResources.Script_AutoUpdate))
                   { Command=EnableScriptAutoUpdateCommand }),
                   (OnlySteamBrowser = new MenuItemViewModel (nameof(AppResources.CommunityFix_OnlySteamBrowser)){ Command=OnlySteamBrowserCommand})
            };

            ScriptAutoUpdate?.CheckmarkChange(ProxySettings.IsAutoCheckScriptUpdate.Value);
            OnlySteamBrowser?.CheckmarkChange(ProxyService.Current.IsOnlyWorkSteamBrowser);

            var scriptFilter = this.WhenAnyValue(x => x.SearchText).Select(ScriptFilter);

            ProxyService.Current.ProxyScripts
                .Connect()
                .Filter(scriptFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<ScriptDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
                .Bind(out _ProxyScripts)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsProxyScriptsEmpty)));
        }

        public ReactiveCommand<Unit, Unit> ScriptStoreCommand { get; }


        private string? _SearchText;
        public string? SearchText
        {
            get => _SearchText;
            set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        }

        public bool IsProxyScriptsEmpty => !ProxyScripts.Any_Nullable();

        public void RefreshScriptButton()
        {
            ProxyService.Current.RefreshScript();
            Toast.Show(@AppResources.Success_.Format(@AppResources.Refresh));
        }
        public void DownloadScriptItemButton(ScriptDTO model)
        {
            ProxyService.Current.DownloadScript(model);
        }

        public void DeleteScriptItemButton(ScriptDTO script)
        {

            var result = MessageBoxCompat.ShowAsync(@AppResources.Script_DeleteItem, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    var item = await DI.Get<IScriptManagerService>().DeleteScriptAsync(script);
                    if (item.state)
                    {
                        if (ProxyService.Current.ProxyScripts != null)
                            ProxyService.Current.ProxyScripts.Remove(script);

                    }
                    Toast.Show(item.msg);
                }
            });
        }
        public void DeleteNoFileScriptItemButton(ScriptDTO script)
        {

            var result = MessageBoxCompat.ShowAsync(@AppResources.Script_NoFileDeleteItem, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
           {
               if (s.Result == MessageBoxResultCompat.OK)
               {
                   var item = await DI.Get<IScriptManagerService>().DeleteScriptAsync(script);
                   if (item.state)
                   {
                       if (ProxyService.Current.ProxyScripts != null)
                           ProxyService.Current.ProxyScripts.Remove(script);

                   }
                   Toast.Show(item.msg);
               }
           });
        }

        public void EditScriptItemButton(ScriptDTO script)
        {

            var url = Path.Combine(IOPath.AppDataDirectory, script.FilePath);
            var fileInfo = new FileInfo(url);
            if (fileInfo.Exists)
            {
                DI.Get<IDesktopPlatformService>().OpenFileByTextReader(url);
                var result = MessageBoxCompat.ShowAsync(@AppResources.Script_EditTxt, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
                {
                    if (s.Result == MessageBoxResultCompat.OK)
                    {
                        var item = await DI.Get<IScriptManagerService>().AddScriptAsync(url, script, build: script.IsBuild, order: script.Order, ignoreCache: true);
                        if (item.state)
                        {
                            if (ProxyService.Current.ProxyScripts.Items.Any() && item.model != null)
                            {
                                ProxyService.Current.ProxyScripts.Replace(script, item.model);
                                Toast.Show(AppResources.Success_.Format(AppResources.Script_Edit));
                            }
                        }
                    }
                });
            }
            else
            {
                DeleteNoFileScriptItemButton(script);
            }

        }

        public void OpenHomeScriptItemButton(ScriptDTO script)
        {
            Services.CloudService.Constants.BrowserOpen(script.SourceLink);
        }

        public async void RefreshScriptItemButton(ScriptDTO script)
        {
            if (script?.FilePath != null)
            {
                var item = await DI.Get<IScriptManagerService>().AddScriptAsync(Path.Combine(IOPath.AppDataDirectory, script.FilePath), script, order: script.Order, build: script.IsBuild, ignoreCache: true);
                if (item.state)
                {
                    if (item.model != null)
                    {
                        ProxyService.Current.ProxyScripts.Replace(script, item.model);
                        Toast.Show(AppResources.RefreshOK);
                    }
                    else
                    {
                        script.Enable = false;
                        Toast.Show(item.msg);
                    }
                }
                else
                {
                    DeleteNoFileScriptItemButton(script);
                }
            }
        }

        public void OpenScriptStoreWindow()
        {
            if (IUserManager.Instance.GetCurrentUser() == null)
            {
                var result = MessageBoxCompat.ShowAsync(@AppResources.ScriptShop_NoLogin, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith((s) =>
                {
                    if (s.Result == MessageBoxResultCompat.OK)
                    {
                        IShowWindowService.Instance.Show(CustomWindow.LoginOrRegister, new LoginOrRegisterWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
                    }
                });
            }
            else
            {
                IShowWindowService.Instance.Show(CustomWindow.ScriptStore, new ScriptStoreWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
            }
        }
    }
}