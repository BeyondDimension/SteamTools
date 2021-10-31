// ReSharper disable once CheckNamespace
using System.Application.Services;
using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    partial class ArchiSteamFarmPlusPageViewModel : IActionItem<ArchiSteamFarmPlusPageViewModel.ActionItem>
    {
        public enum ActionItem
        {
            StartOrStop = 1,
            AddBot,
            Refresh,
            OpenWebConsole,
        }

        string IActionItem<ActionItem>.ToString2(ActionItem action) => ToString2(action);

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.StartOrStop => ASFService.Current.IsASFRuning ? "停止 ASF" : "启动 ASF",
            ActionItem.AddBot => "新增 Bot",
            ActionItem.Refresh => AppResources.Refresh,
            ActionItem.OpenWebConsole => "打开 Web 控制台",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        string IActionItem<ActionItem>.GetIcon(ActionItem action) => GetIcon(action);

        public static string GetIcon(ActionItem action) => action switch
        {
            ActionItem.StartOrStop => ASFService.Current.IsASFRuning ? "round_pause_circle_outline_black_24" : "round_play_circle_outline_black_24",
            ActionItem.AddBot => "baseline_add_black_24",
            ActionItem.OpenWebConsole => "round_open_in_browser_black_24",
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        public void MenuItemClick(ActionItem id)
        {
            switch (id)
            {
                case ActionItem.StartOrStop:
                    RunOrStopASF();
                    break;
                case ActionItem.AddBot:
                    ShowAddBotWindow();
                    break;
                case ActionItem.Refresh:
                    break;
                case ActionItem.OpenWebConsole:
                    OpenBrowser(null);
                    break;
            }
        }

        bool IActionItem<ActionItem>.IsPrimary(ActionItem action) => action switch
        {
            ActionItem.StartOrStop => true,
            _ => false,
        };
    }
}