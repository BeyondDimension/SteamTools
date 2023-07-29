using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptPageViewModel : TabItemViewModel
{

    public ReactiveCommand<Unit, Unit>? EnableScriptAutoUpdateCommand { get; }

    //public MenuItemViewModel? ScriptAutoUpdate { get; }

    //public MenuItemViewModel? OnlySteamBrowser { get; }

    //public ReactiveCommand<Unit, Unit>? OnlySteamBrowserCommand { get; }

    //public ReactiveCommand<Unit, Unit>? ScriptStoreCommand { get; }

    //public ReactiveCommand<Unit, Unit>? AllEnableScriptCommand { get; }

    public ScriptPageViewModel()
    {
        //AllEnableScriptCommand = ReactiveCommand.Create(AllEnableScript);
        AddNewScriptCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            FilePickerFileType? fileTypes;
            if (IApplication.IsDesktop())
            {
                fileTypes = new ValueTuple<string, string[]>[]
                {
                        ("JavaScript Files", new[] { $"*{FileEx.JS}", }),
                        ("Text Files", new[] { $"*{FileEx.TXT}", }),
                    //("All Files", new[] { "*", }),
                };
            }
            else if (OperatingSystem2.IsAndroid())
            {
                fileTypes = new[] { MediaTypeNames.TXT, MediaTypeNames.JS };
            }
            else
            {
                fileTypes = null;
            }
            await FilePicker2.PickAsync(ProxyService.Current.AddNewScriptAsync, fileTypes);
        });

        //var scriptFilter = this.WhenAnyValue(x => x.SearchText).Select(ScriptFilter);

        EditScriptItemButton = ReactiveCommand.Create<ScriptDTO>(EditScriptItem);

        RefreshScriptItemButton = ReactiveCommand.Create<ScriptDTO>(RefreshScriptItem);

        OpenHomeScriptItemButton = ReactiveCommand.Create<ScriptDTO>(OpenHomeScriptItem);

        ScriptStoreCommand = ReactiveCommand.Create(OpenScriptStoreWindow);

        RefreshALLScriptCommand = ReactiveCommand.Create(() =>
        {
            ProxyService.Current.RefreshScript();
            Toast.Show(ToastIcon.Success, Strings.RefreshOK);
        });

        ProxyService.Current.ProxyScripts
            .Connect()
            //.Filter(scriptFilter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Sort(SortExpressionComparer<ScriptDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
            .Bind(out _ProxyScripts)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(IsProxyScriptsEmpty)));
    }

    public async void OpenScriptStoreWindow()
    {
        var model = new ScriptStorePageViewModel();
        await IWindowManager.Instance.ShowTaskDialogAsync(model,
                       Strings.ScriptStore,
                       pageContent: new ScriptStorePage(),
                       isOkButton: false);
    }

    public void DownloadScriptItemButton(ScriptDTO model)
    {
        ProxyService.Current.DownloadScript(model);
    }

    public void DeleteScriptItemButton(ScriptDTO script)
    {
        MessageBox.ShowAsync(Strings.Script_DeleteItem, button: MessageBox.Button.OKCancel).ContinueWith(async (s) =>
        {
            if (s.Result == MessageBox.Result.OK)
            {
                var item = await Ioc.Get<IScriptManager>().DeleteScriptAsync(script);
                if (item.IsSuccess)
                {
                    if (ProxyService.Current.ProxyScripts != null)
                    {
                        ProxyService.Current.ProxyScripts.Remove(script);
                    }
                }
                Toast.Show(item.IsSuccess ? ToastIcon.Success : ToastIcon.Error, item.Message);
            }
        });
    }

    public void DeleteNoFileScriptItemButton(ScriptDTO script)
    {
        var result = MessageBox.ShowAsync(Strings.Script_NoFileDeleteItem, button: MessageBox.Button.OKCancel).ContinueWith(async (s) =>
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
                Toast.Show(item.IsSuccess ? ToastIcon.Success : ToastIcon.Error, item.Message);
            }
        });
    }

    public void EditScriptItem(ScriptDTO script)
    {
        var url = Path.Combine(Plugin.Instance.AppDataDirectory, script.FilePath);
        var fileInfo = new FileInfo(url);
        if (fileInfo.Exists)
        {
            IPlatformService.Instance.OpenFileByTextReader(url);
            var result = MessageBox.ShowAsync(Strings.Script_EditTxt, button: MessageBox.Button.OKCancel).ContinueWith(async (s) =>
            {
                if (s.Result == MessageBox.Result.OK)
                {
                    var rsp = await Ioc.Get<IScriptManager>().AddScriptAsync(url, script, isCompile: script.IsCompile, order: script.Order, ignoreCache: true);
                    if (rsp.IsSuccess)
                    {
                        if (ProxyService.Current.ProxyScripts.Items.Any() && rsp.Content != null)
                        {
                            ProxyService.Current.ProxyScripts.Replace(script, rsp.Content);
                            Toast.Show(ToastIcon.Success, Strings.Success_.Format(Strings.Script_Edit));
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

    public async void OpenHomeScriptItem(ScriptDTO script)
    {
        await Browser2.OpenAsync(script.SourceLink);
    }

    public async void RefreshScriptItem(ScriptDTO script)
    {
        if (script?.FilePath != null)
        {
            var item = await IScriptManager.Instance.AddScriptAsync(Path.Combine(Plugin.Instance.AppDataDirectory, script.FilePath), script, order: script.Order, isCompile: script.IsCompile, ignoreCache: true);
            if (item.IsSuccess)
            {
                if (item.Content != null)
                {
                    ProxyService.Current.ProxyScripts.Replace(script, item.Content);
                    Toast.Show(ToastIcon.Success, Strings.RefreshOK);
                }
                else
                {
                    script.Disable = true;
                    Toast.Show(ToastIcon.Error, item.Message);
                }
            }
            else
            {
                DeleteNoFileScriptItemButton(script);
            }
        }
    }
}
