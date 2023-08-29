using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;
using System.Reactive;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptPageViewModel : TabItemViewModel
{
    DateTime _initializeTime;

    public ReactiveCommand<Unit, Unit>? EnableScriptAutoUpdateCommand { get; }

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

        EditScriptItemCommand = ReactiveCommand.Create<ScriptDTO>(EditScriptItem);

        RefreshScriptItemCommand = ReactiveCommand.Create<ScriptDTO>(RefreshScriptItem);

        ScriptStoreCommand = ReactiveCommand.Create(OpenScriptStoreWindow);

        DeleteScriptItemCommand = ReactiveCommand.Create<ScriptDTO>(DeleteScriptItemButton);

        DownloadScriptItemCommand = ReactiveCommand.Create<ScriptDTO>(DownloadScriptItem);

        RefreshALLScriptCommand = ReactiveCommand.Create(async () =>
        {
            if (_initializeTime > DateTime.Now.AddSeconds(-2))
            {
                Toast.Show(ToastIcon.Warning, Strings.Warning_DoNotOperateFrequently);
                return;
            }

            _initializeTime = DateTime.Now;

            await ProxyService.Current.RefreshScript();
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
        if (UserService.Current.IsAuthenticated == false)
        {
            Toast.Show(ToastIcon.Error, "脚本商店需要登录账号才可访问");
            return;
        }
        var model = new ScriptStorePageViewModel();
        await IWindowManager.Instance.ShowTaskDialogAsync(model,
                       Strings.ScriptStore,
                       pageContent: new ScriptStorePage(),
                       isOkButton: false);
    }

    public void DownloadScriptItem(ScriptDTO model)
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
