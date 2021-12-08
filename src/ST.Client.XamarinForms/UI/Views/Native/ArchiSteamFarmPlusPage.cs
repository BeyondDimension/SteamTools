using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using ReactiveUI;
using VM = System.Application.UI.ViewModels.ArchiSteamFarmPlusPageViewModel;
using Interface = System.Application.UI.Views.IArchiSteamFarmPlusPage;

namespace System.Application.UI.Views.Native
{
    public partial class ArchiSteamFarmPlusPage : BaseContentPage<VM>, Interface
    {
        public IReadOnlyDictionary<VM.ActionItem, ToolbarItem> Actions { get; }

        public ArchiSteamFarmPlusPage()
        {
            ViewModel = IViewModelManager.Instance.GetMainPageViewModel<VM>();

            Actions = Interface.InitToolbarItems(this);

            ASFService.Current.WhenAnyValue(x => x.IsASFRuning).Subscribe(value =>
            {
                const VM.ActionItem actionItem = VM.ActionItem.StartOrStop;
                if (Actions.ContainsKey(actionItem))
                {
                    var item = Actions[actionItem];
                    IActionItem<VM.ActionItem> itemHost = ViewModel;
                    item.Text = itemHost.ToString2(actionItem);
                    item.IconImageSource = itemHost.GetIcon(actionItem);
                }
            });
        }
    }
}
