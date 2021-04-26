using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
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

		public ProxyScriptManagePageViewModel()
		{
			IconKey = nameof(ProxyScriptManagePageViewModel).Replace("ViewModel", "Svg");

			ScriptStoreCommand = ReactiveCommand.Create(OpenScriptStoreWindow);

			MenuItems = new ObservableCollection<MenuItemViewModel>()
			{
				   new MenuItemViewModel (nameof(AppResources.CommunityFix_EnableScriptService)),
				   new MenuItemViewModel (nameof(AppResources.ScriptStore)){
					   IconKey ="JavaScriptDrawing",Command=ScriptStoreCommand},
			};

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
			Toast.Show(string.Format(@AppResources.Success_, @AppResources.Refresh));
		}
		public  void DownloadScriptItemButton(ScriptDTO model)
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

		public void EditScriptItemButton(ScriptDTO script)
		{

			var url = Path.Combine(IOPath.AppDataDirectory, script.FilePath);
			DI.Get<IDesktopPlatformService>().OpenFileByTextReader(url);
			var result = MessageBoxCompat.ShowAsync(@AppResources.Script_EditTxt, ThisAssembly.AssemblyTrademark, MessageBoxButtonCompat.OKCancel).ContinueWith(async (s) =>
			{
				if (s.Result == MessageBoxResultCompat.OK)
				{
					var item = await DI.Get<IScriptManagerService>().AddScriptAsync(url, script);
					if (item.state)
					{
						if (ProxyService.Current.ProxyScripts.Items.Any() && item.model != null)
						{
							ProxyService.Current.ProxyScripts.Replace(script, item.model);
						}
					}
				}
			});
		}

		public void OpenHomeScriptItemButton(ScriptDTO script)
		{
			Services.CloudService.Constants.BrowserOpen(script.SourceLink);
		}

		public async void RefreshScriptItemButton(ScriptDTO script)
		{
			if (script?.FilePath != null)
			{
				var item = await DI.Get<IScriptManagerService>().AddScriptAsync(script.FilePath);
				if (item.state)
					if (item.model != null)
						script = item.model;
					else
					{
						script.Enable = false;
						Toast.Show(item.msg);
					}
			}
		}

		public void OpenScriptStoreWindow()
		{
			IShowWindowService.Instance.Show(CustomWindow.ScriptStore, new ScriptStoreWindowViewModel(), string.Empty, ResizeModeCompat.CanResize);
		}
	}
}