using ReactiveUI;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class LocalAuthPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.SteamAuth;
            protected set { throw new NotImplementedException(); }
        }

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
    }
}
