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
using System.Linq;
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

			this.WhenAnyValue(x => x.SearchText)
				.Subscribe(x =>
				{
					//x
					InitializeScriptList();
				});
		}

		private readonly ReadOnlyObservableCollection<ScriptDTO> _Scripts;
		public ReadOnlyObservableCollection<ScriptDTO> Scripts => _Scripts;

		private readonly SourceList<ScriptDTO> _ScriptsSourceList;

		public bool IsScriptsEmpty => !Scripts.Any_Nullable();

		private string? _SearchText;
		public string? SearchText
		{
			get => _SearchText;
			set => this.RaiseAndSetIfChanged(ref _SearchText, value);
		}
		public void RefreshScriptButton()
		{
			InitializeScriptList();
		}
		public async void DownloadScriptItemButton(ScriptDTO model)
		{

			var jspath = await DI.Get<IScriptManagerService>().DownloadScript(model.UpdateLink);
			if (jspath.state)
			{
				var build = await DI.Get<IScriptManagerService>().AddScriptAsync(jspath.path, build: true, order: 10, deleteFile: true, pid: model.Id);
				if (build.state)
				{
					if (build.model != null)
					{
						var basicsItem = ProxyService.Current.ProxyScripts.Items.FirstOrDefault(x => x.Id == model.Id);
						if (basicsItem != null)
						{
							var index = ProxyService.Current.ProxyScripts.Items.IndexOf(basicsItem);
							ProxyService.Current.ProxyScripts.ReplaceAt(index, basicsItem);
						}
						else
						{
							ProxyService.Current.ProxyScripts.Add(build.model);
						}
						Toast.Show(AppResources.Download_ScriptOk);
					}
				}
				else
					Toast.Show(build.msg);
			}
			else
				Toast.Show(AppResources.Download_ScriptError);
		}
		private async void InitializeScriptList()
		{
			var client = ICloudServiceClient.Instance.Script;
			var response = await client.ScriptTable(name: SearchText);
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