using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using Xamarin.Essentials;

namespace System.Application.UI.ViewModels
{
    public partial class ProxyScriptManagePageViewModel : TabItemViewModel
    {
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
            IconKey = nameof(ProxyScriptManagePageViewModel);

            ScriptStoreCommand = ReactiveCommand.Create(OpenScriptStoreWindow);
            EnableScriptAutoUpdateCommand = ReactiveCommand.Create(() =>
            {
                ScriptAutoUpdate?.CheckmarkChange(ProxySettings.IsAutoCheckScriptUpdate.Value = !ProxySettings.IsAutoCheckScriptUpdate.Value);
            });
            OnlySteamBrowserCommand = ReactiveCommand.Create(() =>
            {
                OnlySteamBrowser?.CheckmarkChange(ProxyService.Current.IsOnlyWorkSteamBrowser = !ProxyService.Current.IsOnlyWorkSteamBrowser);
            });
            AddNewScriptButton_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                var fileTypes =
#if __MOBILE__
                    (FilePickerFileType?)null;
#else
                    new FilePicker2.FilePickerFilter(new (string, IEnumerable<string>)[] {
                        ("JavaScript Files", new[] { "js" }),
                        ("Text Files", new[] { "txt" }),
                        ("All Files", new[] { "*" }),
                    });
#endif
                await FilePicker2.PickAsync(ProxyService.Current.AddNewScript, fileTypes);
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
                IPlatformService.Instance.OpenFileByTextReader(url);
                var result = MessageBoxCompat.ShowAsync(@AppResources.Script_EditTxt, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
                {
                    if (s.Result == MessageBoxResultCompat.OK)
                    {
                        var (state, model, msg) = await DI.Get<IScriptManagerService>().AddScriptAsync(url, script, build: script.IsBuild, order: script.Order, ignoreCache: true);
                        if (state)
                        {
                            if (ProxyService.Current.ProxyScripts.Items.Any() && model != null)
                            {
                                ProxyService.Current.ProxyScripts.Replace(script, model);
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

        public async void OpenScriptStoreWindow()
        {
            if (IUserManager.Instance.GetCurrentUser() == null)
            {
                var result = await MessageBoxCompat.ShowAsync(@AppResources.ScriptShop_NoLogin, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel);
                if (result == MessageBoxResultCompat.OK)
                {
                    await IShowWindowService.Instance.Show(CustomWindow.LoginOrRegister, new LoginOrRegisterWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
                }
            }
            else
            {
                await IShowWindowService.Instance.Show(CustomWindow.ScriptStore, new ScriptStoreWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
            }
        }

        public ICommand AddNewScriptButton_Click { get; }
    }
}