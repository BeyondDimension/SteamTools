using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Properties;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class ScriptStoreWindowViewModel : WindowViewModel
    {
        public ScriptStoreWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.ScriptStore;
            _ScriptsSourceList = new SourceList<ScriptDTO>();

            _ScriptsSourceList
                .Connect()
                //.Filter(scriptFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<ScriptDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
                .Bind(out _Scripts)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(IsScriptsEmpty)));

            InitializeScriptList();
        }

        private readonly ReadOnlyObservableCollection<ScriptDTO> _Scripts;
        public ReadOnlyObservableCollection<ScriptDTO> Scripts => _Scripts;

        private readonly SourceList<ScriptDTO> _ScriptsSourceList;

        public bool IsScriptsEmpty => !Scripts.Any_Nullable();

        private string? _SerachText;
        public string? SerachText
        {
            get => _SerachText;
            set => this.RaiseAndSetIfChanged(ref _SerachText, value);
        }

        private async void InitializeScriptList()
        {
            var client = ICloudServiceClient.Instance.Accelerate;
            var response = await client.Scripts();
            if (!response.IsSuccess || response.Content == null)
            {
                return;
            }
            _ScriptsSourceList.Clear();
            _ScriptsSourceList.AddRange(response.Content);
        }
    }
}