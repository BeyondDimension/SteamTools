using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
namespace System.Application.UI.ViewModels
{
    public class ScriptStoreWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.ScriptStore;

        public ScriptStoreWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
            _ScriptsSourceList = new SourceList<ScriptDTO>();

            _ScriptsSourceList
                .Connect()
                //.Filter(scriptFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<ScriptDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
                .Bind(out _Scripts)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsScriptsEmpty)));

            this.WhenAnyValue(x => x.SearchText)
                .Subscribe(x =>
                {
                    //x
                    InitializeScriptList();
                });
        }

        readonly ReadOnlyObservableCollection<ScriptDTO> _Scripts;
        public ReadOnlyObservableCollection<ScriptDTO> Scripts => _Scripts;

        readonly SourceList<ScriptDTO> _ScriptsSourceList;

        public bool IsScriptsEmpty => !Scripts.Any_Nullable();

        string? _SearchText;
        public string? SearchText
        {
            get => _SearchText;
            set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        }

        public void RefreshScriptButton()
        {
            InitializeScriptList();
        }

        public void DownloadScriptItemButton(ScriptDTO model)
        {
            ProxyService.Current.DownloadScript(model);
        }

        public async void OpenHomeScriptItemButton(ScriptDTO script)
        {
            await Browser2.OpenAsync(script.SourceLink);
        }

        async void InitializeScriptList()
        {
            var client = ICloudServiceClient.Instance.Script;
            var response = await client.ScriptTable(name: SearchText, pageSize: 100, errormsg: AppResources.ScriptShop_GetTableError);
            if (!response.IsSuccess || response.Content == null)
            {
                return;
            }
            response.Content.DataSource.ForEach(item =>
            {
                var old = ProxyService.Current.ProxyScripts.Items.FirstOrDefault(x => x.Id == item.Id);
                if (old != null)
                {
                    item.IsExist = true;
                    if (old.Version != item.Version)
                    {
                        item.IsUpdate = true;
                        item.NewVersion = item.Version;
                        item.Version = old.Version;
                    }
                }
            });
            _ScriptsSourceList.Clear();
            _ScriptsSourceList.AddRange(response.Content.DataSource);
        }
    }
}