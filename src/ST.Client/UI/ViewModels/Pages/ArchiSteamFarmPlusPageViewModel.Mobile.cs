// ReSharper disable once CheckNamespace
using System.Application.Services;
using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    partial class ArchiSteamFarmPlusPageViewModel
    {
        public enum ActionItem
        {
            StartOrStop = 1,
            AddBot,
            Refresh,
            OpenWebConsole,
        }

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.StartOrStop => ASFService.Current.IsASFRuning ? "停止 ASF" : "启动 ASF",
            ActionItem.AddBot => "新增 Bot",
            ActionItem.Refresh => AppResources.Refresh,
            ActionItem.OpenWebConsole => "打开 Web 控制台",
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
    }
}