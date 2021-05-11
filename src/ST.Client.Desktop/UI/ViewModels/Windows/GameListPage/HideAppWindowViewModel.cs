using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Models.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class HideAppWindowViewModel : WindowViewModel
    {
        public HideAppWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.GameList_EditAppInfo;
            //_SteamHideApp = new ObservableCollection<SteamHideApps>();
            //_SteamHideApp.Subscribe(_ => this.RaisePropertyChanged(nameof(IsHideAppEmpty)));

            SteamHideApp =new ReadOnlyObservableCollection<SteamHideApps>(ProxySettings.HideGameList.Value??new ObservableCollection<SteamHideApps>());


            this.WhenAnyValue(v => v.SteamHideApp)
                       .Subscribe(apps => apps?
                       .ToObservableChangeSet()
                       .AutoRefresh(x => x.Enable)
                       .ToCollection()
                       .Select<IReadOnlyCollection<SteamHideApps>, bool?>(x =>
                       {
                           var count = x.Count(s => s.Enable);
                           if (x == null || count == 0)
                               return false;
                           if (count == x.Count)
                               return true;
                           return null;
                       })
                       .Subscribe(s =>
                       {
                           if (ThreeStateEnable != s)
                           {
                               _ThreeStateEnable = s;
                               this.RaisePropertyChanged(nameof(ThreeStateEnable));
                           }
                       })
                       );

            this.WhenAnyValue(x => x.SearchText)
                .Subscribe(x =>
                {
                    //InitializeScriptList();
                });

        }
        public bool? _ThreeStateEnable;
        public bool? ThreeStateEnable
        {
            get => _ThreeStateEnable;
            set
            {
                if (SteamHideApp != null)
                    foreach (var item in SteamHideApp)
                    {
                        item.Enable = value ?? false;
                    }
                this.RaiseAndSetIfChanged(ref _ThreeStateEnable, value);
            }
        }
        private string? _SearchText;
        public string? SearchText
        {
            get => _SearchText;
            set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        }
        private ReadOnlyObservableCollection<SteamHideApps>? _SteamHideApp;
        public ReadOnlyObservableCollection<SteamHideApps>? SteamHideApp
        {
            get => _SteamHideApp;
            set
            {
                if (_SteamHideApp != value)
                {
                    _SteamHideApp = value;
                    this.RaisePropertyChanged();
                }
            }
        } 


    }
}