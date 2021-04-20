using ReactiveUI;
using DynamicData;
using DynamicData.Binding;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class GameListPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.GameList;
            protected set { throw new NotImplementedException(); }
        }

        public GameListPageViewModel()
        {
            IconKey = nameof(GameListPageViewModel).Replace("ViewModel", "Svg");
            AppTypeFiltres = new ObservableCollection<EnumModel<SteamAppType>>(EnumModel.GetEnumModels<SteamAppType>());
            AppTypeFiltres[1].Enable = true;
            AppTypeFiltres[2].Enable = true;
        }

        private readonly Subject<Unit> updateSource = new();
        public bool IsReloading { get; set; }

        private bool _IsOpenFilter;
        public bool IsOpenFilter
        {
            get => _IsOpenFilter;
            set => this.RaiseAndSetIfChanged(ref _IsOpenFilter, value);
        }

        private bool _IsAppInfoOpen;
        public bool IsAppInfoOpen
        {
            get => _IsAppInfoOpen;
            set => this.RaiseAndSetIfChanged(ref _IsAppInfoOpen, value);
        }

        private SteamApp? _SelectApp;
        public SteamApp? SelectApp
        {
            get => _SelectApp;
            set => this.RaiseAndSetIfChanged(ref _SelectApp, value);
        }

        private IReadOnlyCollection<SteamApp>? _SteamApps;
        public IReadOnlyCollection<SteamApp>? SteamApps
        {
            get => _SteamApps;
            set
            {
                if (_SteamApps != value)
                {
                    _SteamApps = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged(nameof(IsSteamAppsEmpty));
                }
            }
        }

        private string? _SerachText;
        public string? SerachText
        {
            get => _SerachText;
            set => this.RaiseAndSetIfChanged(ref _SerachText, value);
        }

        public bool IsSteamAppsEmpty => !SteamApps.Any_Nullable();

        private ObservableCollection<EnumModel<SteamAppType>> _AppTypeFiltres;
        public ObservableCollection<EnumModel<SteamAppType>> AppTypeFiltres
        {
            get => _AppTypeFiltres;
            set => this.RaiseAndSetIfChanged(ref _AppTypeFiltres, value);
        }

        internal override void Initialize()
        {
            this.updateSource
                .Do(_ => this.IsReloading = true)
                .SelectMany(x => this.UpdateAsync())
                .Do(_ => this.IsReloading = false)
                .Subscribe()
                .AddTo(this);

            SteamConnectService.Current
              .WhenAnyValue(x => x.SteamApps)
              .Subscribe(_ => Update());

            this.WhenAnyValue(x => x.SerachText)
                  .Subscribe(_ =>
                  {
                      Update();
                  });

            this.WhenAnyValue(x => x.AppTypeFiltres)
                  .Subscribe(type => type?
                        .ToObservableChangeSet()
                        .AutoRefresh(x => x.Enable)
                        .WhenValueChanged(x => x.Enable, false)
                        .Subscribe(_ =>
                        {
                            Update();
                        }));
        }

        private IObservable<Unit> UpdateAsync()
        {
            var types = AppTypeFiltres.Where(x => x.Enable);
            bool predicateName(SteamApp s)
            {
                if (!string.IsNullOrEmpty(SerachText))
                {
                    if (s.DisplayName?.Contains(SerachText, StringComparison.OrdinalIgnoreCase) == true ||
                        s.AppId.ToString().Contains(SerachText, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                else 
                {
                    return true;
                }
                return false;
            }
            bool predicateType(SteamApp s)
            {
                if (types.Any())
                {
                    if (types.Any(x => x.Value == s.Type))
                    {
                        return true;
                    }
                }
                return false;
            }

            return Observable.Start(() =>
            {
                var list = SteamConnectService.Current.SteamApps?
                .Where(x => predicateType(x))
                .Where(x => predicateName(x))
                .OrderBy(x => x.Name).ToList();
                if (list.Any_Nullable())
                    this.SteamApps = list;
                else
                    this.SteamApps = null;
                this.CalcTypeCount();
            });
        }

        public void Update()
        {
            this.updateSource.OnNext(Unit.Default);
        }

        public void CalcTypeCount()
        {
            if (SteamConnectService.Current.SteamApps.Any_Nullable())
                foreach (var item in AppTypeFiltres)
                {
                    item.Count = SteamConnectService.Current.SteamApps.Count(s => s.Type == item.Value);
                }
        }

        public void AppClick(SteamApp app)
        {
            IsAppInfoOpen = true;
            SelectApp = app;
        }
    }
}