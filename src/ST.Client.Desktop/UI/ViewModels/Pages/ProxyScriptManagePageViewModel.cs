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

namespace System.Application.UI.ViewModels
{
    public partial class ProxyScriptManagePageViewModel : TabItemViewModel
    {
        public ProxyScriptManagePageViewModel()
        {
            IconKey = nameof(ProxyScriptManagePageViewModel);

            ScriptStoreCommand = ReactiveCommand.Create(OpenScriptStoreWindow);
            AllEnableScriptCommand = ReactiveCommand.Create(AllEnableScript);
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
                   new MenuItemViewModel (nameof(AppResources.Script_AllEnable)){
                       IconKey ="JavaScriptDrawing",Command=AllEnableScriptCommand},
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

        public override void Activation()
        {
            if (IsFirstActivation)
                if (ProxySettings.IsAutoCheckScriptUpdate)
                    ProxyService.Current.CheckUpdate();
            base.Activation();
        }

        readonly ReadOnlyObservableCollection<ScriptDTO> _ProxyScripts;
        public ReadOnlyObservableCollection<ScriptDTO> ProxyScripts => _ProxyScripts;

        readonly Dictionary<string, string[]> dictPinYinArray = new();
        Func<ScriptDTO, bool> ScriptFilter(string? serachText)
        {
            return s =>
            {
                if (s == null)
                    return false;
                if (string.IsNullOrEmpty(serachText))
                    return true;
                if (s.Author.Contains(serachText, StringComparison.OrdinalIgnoreCase) ||
                    s.Description.Contains(serachText, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var pinyinArray = Pinyin.GetPinyin(s.Name, dictPinYinArray);
                if (Pinyin.SearchCompare(serachText, s.Name, pinyinArray))
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

        public ReactiveCommand<Unit, Unit> ScriptStoreCommand { get; }
        public ReactiveCommand<Unit, Unit> AllEnableScriptCommand { get; }
        
        string? _SearchText;
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
            var result = MessageBoxCompat.ShowAsync(AppResources.Script_DeleteItem, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    var item = await DI.Get<IScriptManager>().DeleteScriptAsync(script);
                    if (item.IsSuccess)
                    {
                        if (ProxyService.Current.ProxyScripts != null)
                        {
                            ProxyService.Current.ProxyScripts.Remove(script);
                        }
                    }
                    Toast.Show(item.Message);
                }
            });
        }

        public void DeleteNoFileScriptItemButton(ScriptDTO script)
        {
            var result = MessageBoxCompat.ShowAsync(AppResources.Script_NoFileDeleteItem, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
            {
                if (s.Result == MessageBoxResultCompat.OK)
                {
                    var item = await DI.Get<IScriptManager>().DeleteScriptAsync(script);
                    if (item.IsSuccess)
                    {
                        if (ProxyService.Current.ProxyScripts != null)
                        {
                            ProxyService.Current.ProxyScripts.Remove(script);
                        }
                    }
                    Toast.Show(item.Message);
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
                        var rsp = await DI.Get<IScriptManager>().AddScriptAsync(url, script, build: script.IsBuild, order: script.Order, ignoreCache: true);
                        if (rsp.IsSuccess)
                        {
                            if (ProxyService.Current.ProxyScripts.Items.Any() && rsp.Content != null)
                            {
                                ProxyService.Current.ProxyScripts.Replace(script, rsp.Content);
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

        public async void OpenHomeScriptItemButton(ScriptDTO script)
        {
            await Browser2.OpenAsync(script.SourceLink);
        }

        public async void RefreshScriptItemButton(ScriptDTO script)
        {
            if (script?.FilePath != null)
            {
                var item = await DI.Get<IScriptManager>().AddScriptAsync(Path.Combine(IOPath.AppDataDirectory, script.FilePath), script, order: script.Order, build: script.IsBuild, ignoreCache: true);
                if (item.IsSuccess)
                {
                    if (item.Content != null)
                    {
                        ProxyService.Current.ProxyScripts.Replace(script, item.Content);
                        Toast.Show(AppResources.RefreshOK);
                    }
                    else
                    {
                        script.Enable = false;
                        Toast.Show(item.Message);
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

        public void AllEnableScript()
        {
            ProxyService.Current.ProxyScripts.Edit(x=>x.All(e=>e.Enable=true));
           // ProxyService.Current.ProxyScripts= ProxyService.Current.ProxyScripts.Items.Select(x=>x.Enable=true).ToList();
        }

        public ICommand AddNewScriptButton_Click { get; }
    }
}