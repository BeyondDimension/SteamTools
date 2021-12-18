using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using Xamarin.Essentials;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class ProxyScriptManagePageViewModel
    {
        //IEnumerable<MenuItemViewModel> GetMenuItems()
        //{
        //    //yield return new MenuItemViewModel(nameof(AppResources.CommunityFix_EnableScriptService));
        //    yield return new MenuItemViewModel(nameof(AppResources.ScriptStore))
        //    {
        //        IconKey = "JavaScriptDrawing",
        //        Command = ScriptStoreCommand
        //    };
        //    yield return new MenuItemViewModel(nameof(AppResources.Script_AllEnable))
        //    {
        //        IconKey = "JavaScriptDrawing",
        //        Command = AllEnableScriptCommand,
        //    };
        //    yield return new MenuItemViewModel();
        //    if (OnlySteamBrowser != null) yield return OnlySteamBrowser;
        //}

        public ProxyScriptManagePageViewModel()
        {
            IconKey = nameof(ProxyScriptManagePageViewModel);

            if (!IApplication.IsDesktopPlatform)
            {
                return;
            }

            ScriptStoreCommand = ReactiveCommand.Create(OpenScriptStoreWindow);
            AllEnableScriptCommand = ReactiveCommand.Create(AllEnableScript);
            AddNewScriptButton_Click = ReactiveCommand.CreateFromTask(async () =>
            {
                var fileTypes = !FilePicker2.IsSupportedFileExtensionFilter ? (FilePickerFileType?)null : new FilePicker2.FilePickerFilter(new (string, IEnumerable<string>)[] {
                    ("JavaScript Files", new[] { "js" }),
                    ("Text Files", new[] { "txt" }),
                    ("All Files", new[] { "*" }),
                });
                await FilePicker2.PickAsync(ProxyService.Current.AddNewScript, fileTypes);
            });

            //EnableScriptAutoUpdateCommand = ReactiveCommand.Create(() =>
            //{
            //    ScriptAutoUpdate?.CheckmarkChange(ProxySettings.IsAutoCheckScriptUpdate.Value = !ProxySettings.IsAutoCheckScriptUpdate.Value);
            //});
            //OnlySteamBrowserCommand = ReactiveCommand.Create(() =>
            //{
            //    OnlySteamBrowser?.CheckmarkChange(ProxySettings.IsOnlyWorkSteamBrowser.Value = !ProxySettings.IsOnlyWorkSteamBrowser.Value);
            //});

            //ScriptAutoUpdate = new MenuItemViewModel(nameof(AppResources.Script_AutoUpdate))
            //{
            //    Command = EnableScriptAutoUpdateCommand,
            //};
            //OnlySteamBrowser = new(nameof(AppResources.CommunityFix_OnlySteamBrowser))
            //{
            //    Command = OnlySteamBrowserCommand,
            //};
            //MenuItems = new ObservableCollection<MenuItemViewModel>(GetMenuItems());

            //ScriptAutoUpdate?.CheckmarkChange(ProxySettings.IsAutoCheckScriptUpdate.Value);
            //OnlySteamBrowser?.CheckmarkChange(ProxySettings.IsOnlyWorkSteamBrowser.Value);

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

        protected readonly ReadOnlyObservableCollection<ScriptDTO>? _ProxyScripts;
        public ReadOnlyObservableCollection<ScriptDTO>? ProxyScripts => _ProxyScripts;

        protected readonly Dictionary<string, string[]> dictPinYinArray = new();
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

        public ReactiveCommand<Unit, Unit>? EnableScriptAutoUpdateCommand { get; }

        //public MenuItemViewModel? ScriptAutoUpdate { get; }

        //public MenuItemViewModel? OnlySteamBrowser { get; }

        //public ReactiveCommand<Unit, Unit>? OnlySteamBrowserCommand { get; }

        public ReactiveCommand<Unit, Unit>? ScriptStoreCommand { get; }

        public ReactiveCommand<Unit, Unit>? AllEnableScriptCommand { get; }

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
            Toast.Show(AppResources.Success_.Format(AppResources.Refresh));
        }

        public void DownloadScriptItemButton(ScriptDTO model)
        {
            ProxyService.Current.DownloadScript(model);
        }

        public void DeleteScriptItemButton(ScriptDTO script)
        {
            MessageBox.ShowAsync(AppResources.Script_DeleteItem, button: MessageBox.Button.OKCancel).ContinueWith(async (s) =>
            {
                if (s.Result == MessageBox.Result.OK)
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
            var result = MessageBox.ShowAsync(AppResources.Script_NoFileDeleteItem, button: MessageBox.Button.OKCancel).ContinueWith(async (s) =>
            {
                if (s.Result == MessageBox.Result.OK)
                {
                    var item = await IScriptManager.Instance.DeleteScriptAsync(script);
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
                var result = MessageBox.ShowAsync(AppResources.Script_EditTxt, button: MessageBox.Button.OKCancel).ContinueWith(async (s) =>
                {
                    if (s.Result == MessageBox.Result.OK)
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
                var item = await IScriptManager.Instance.AddScriptAsync(Path.Combine(IOPath.AppDataDirectory, script.FilePath), script, order: script.Order, build: script.IsBuild, ignoreCache: true);
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
            CustomWindow window = default;
            if (IUserManager.Instance.GetCurrentUser() == null)
            {
                var result = await MessageBox.ShowAsync(AppResources.ScriptShop_NoLogin, button: MessageBox.Button.OKCancel);
                if (result == MessageBox.Result.OK)
                {
                    window = CustomWindow.LoginOrRegister;

                }
            }
            else
            {
                window = CustomWindow.ScriptStore;
            }

            if (window != default)
            {
                await IWindowManager.Instance.Show(window, resizeMode: ResizeMode.CanResize);
            }
        }

        public void AllEnableScript()
        {
            ProxyService.Current.ProxyScripts.Edit(x => x.All(e => e.Enable = true));
            // ProxyService.Current.ProxyScripts= ProxyService.Current.ProxyScripts.Items.Select(x=>x.Enable=true).ToList();
        }

        public ICommand? AddNewScriptButton_Click { get; }
    }
}
