using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class LocalAuthPageViewModel : TabItemViewModel
    {
        public LocalAuthPageViewModel()
        {
            AddAuthCommand = ReactiveCommand.Create(AddAuthMenu_Click);
            MenuItems = new[]
            {
                new MenuItemViewModel
                {
                    Header = AppResources.LocalAuth_EditAuth,
                    Items = new[]
                    {
                        new MenuItemViewModel { Header = AppResources.Add,IconKey="AddDrawing",
                            Command= AddAuthCommand },
                        new MenuItemViewModel { Header = AppResources.Edit,IconKey="EditDrawing" },
                        new MenuItemViewModel { Header = AppResources.Export,IconKey="ExportDrawing" },
                        new MenuItemViewModel { Header = "-" },
                        new MenuItemViewModel { Header = AppResources.Refresh,IconKey="RefreshDrawing" },
                        new MenuItemViewModel { Header = "-" },
                        new MenuItemViewModel { Header = AppResources.Encrypt,IconKey="LockDrawing" },
                        new MenuItemViewModel { Header = AppResources.CloudSync,IconKey="CloudDrawing" },
                    }
                },
            };
        }

        public override string Name
        {
            get => AppResources.LocalAuth;
            protected set { throw new NotImplementedException(); }
        }

        public ReactiveCommand<Unit, Unit> AddAuthCommand { get; }


        public override IList<MenuItemViewModel> MenuItems { get; protected set; }

        /// <summary>
        /// 令牌列表
        /// </summary>
        private IList<string>? _Authenticators;
        public IList<string>? Authenticators
        {
            get => _Authenticators;
            set => this.RaiseAndSetIfChanged(ref _Authenticators, value);
        }

        internal async override Task Initialize()
        {
#if DEBUG
            Authenticators = new ObservableCollection<string>();
            for (var i = 0; i < 10; i++)
            {
                Authenticators.Add("test");
            }
#endif
            await Task.CompletedTask;
        }

        void AddAuthMenu_Click()
        {
            if (!AppHelper.IsOfficialChannelPackage)
            {
                return;
            }
            DI.Get<IShowWindowService>().Show(CustomWindow.AddAuth, new AddAuthWindowViewModel());
        }
    }
}
