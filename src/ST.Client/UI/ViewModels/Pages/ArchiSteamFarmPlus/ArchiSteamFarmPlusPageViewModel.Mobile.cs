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
            Wiki,
            Repo,
            ConfigGenerator,
            WebAddBot,
            WebConfig,
        }

        string IActionItem<ActionItem>.ToString2(ActionItem action) => ToString2(action);

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.StartOrStop => ASFService.Current.IsASFRuning ? AppResources.ASF_Stop : AppResources.ASF_Start,
            ActionItem.AddBot or ActionItem.WebAddBot => AppResources.ASF_AddBot,
            ActionItem.Refresh => AppResources.ASF_RefreshBot,
            ActionItem.OpenWebConsole => AppResources.ASF_OpenWebUIConsole,
            ActionItem.Wiki => "ArchiSteamFarm Wiki",
            ActionItem.Repo => "ArchiSteamFarm Github",
            ActionItem.ConfigGenerator => AppResources.ASF_WebConfigGenerator,
            _ => string.Empty,
        };

        string IActionItem<ActionItem>.GetIcon(ActionItem action) => GetIcon(action);

        public static string GetIcon(ActionItem action) => action switch
        {
            ActionItem.StartOrStop => ASFService.Current.IsASFRuning ? "round_pause_circle_outline_black_24" : "round_play_circle_outline_black_24",
            ActionItem.AddBot => "baseline_add_black_24",
            ActionItem.Wiki => "baseline_language_black_24",
            ActionItem.Repo => "baseline_language_black_24",
            ActionItem.OpenWebConsole or _ => "round_open_in_browser_black_24",
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
                case ActionItem.WebAddBot:
                case ActionItem.OpenWebConsole:
                case ActionItem.Wiki:
                case ActionItem.Repo:
                case ActionItem.ConfigGenerator:
                    OpenBrowserCore(id);
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