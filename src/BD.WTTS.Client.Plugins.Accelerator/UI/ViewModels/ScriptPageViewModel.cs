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

    public ReactiveCommand<Unit, Unit>? ScriptStoreCommand { get; }

    //public ReactiveCommand<Unit, Unit>? AllEnableScriptCommand { get; }

    public ICommand? AddNewScriptButton_Click { get; }

    public ScriptPageViewModel()
    {
        ScriptStoreCommand = ReactiveCommand.Create(OpenScriptStoreWindow);
        //AllEnableScriptCommand = ReactiveCommand.Create(AllEnableScript);
        AddNewScriptButton_Click = ReactiveCommand.CreateFromTask(async () =>
        {
            FilePickerFileType? fileTypes;
            if (IApplication.IsDesktop())
            {
                fileTypes = new ValueTuple<string, string[]>[]
                {
                        ("JavaScript Files", new[] { FileEx.JS, }),
                        ("Text Files", new[] { FileEx.TXT, }),
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
                       isCancelButton: true);
    }
}
