using ArchiSteamFarm.Steam;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace System.Application.UI.Views.Pages
{
    public partial class ASF_BotPage : ReactiveUserControl<ArchiSteamFarmPlusPageViewModel>
    {
        private readonly ContentDialog keyDialog;
        private readonly TextBox keyText;
        private readonly DataGrid UsedKeysDataGrid;
        private readonly DataGrid UnusedKeysDataGrid;

        public ASF_BotPage()
        {
            InitializeComponent();

            keyDialog = this.FindControl<ContentDialog>("RedeemKeyDialog");
            keyText = this.FindControl<TextBox>("keyText");
            UsedKeysDataGrid = this.FindControl<DataGrid>("UsedKeysDataGrid");
            UnusedKeysDataGrid = this.FindControl<DataGrid>("UnusedKeysDataGrid");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void RedeemKeyBot_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (ViewModel != null &&
                keyDialog != null &&
                (sender as Avalonia.Controls.Button)?.Tag is Bot bot)
            {
                var k = await ViewModel.GetUsedAndUnusedKeys(bot);

                UsedKeysDataGrid.Items = k.UsedKeys;
                UnusedKeysDataGrid.Items = k.UnusedKeys;

                var r = await keyDialog.ShowAsync();
                if (r == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrEmpty(keyText.Text))
                        return;
                    var keys = ExtractKeysFromString(keyText.Text);
                    if (keys.Count == 0)
                    {
                        Toast.Show(AppResources.ASF_RedeemKey_NoValidkey);
                        return;
                    }

                    ViewModel.RedeemKeyBot(bot, keys);
                }
                else if (r == ContentDialogResult.Secondary)
                {
                    ViewModel.ResetbotRedeemedKeysRecord(bot);
                }
            }
        }

        private static IOrderedDictionary ExtractKeysFromString(string source)
        {
            MatchCollection m = Regex.Matches(source, "([0-9A-Z]{5})(?:\\-[0-9A-Z]{5}){2,4}",
                  RegexOptions.IgnoreCase | RegexOptions.Singleline);
            var dictKeys = new OrderedDictionary();
            if (m.Count > 0)
            {
                foreach (Match v in m)
                {
                    dictKeys.Add(v.Value, v.Value);
                }
            }
            return dictKeys;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            ASFService.Current.RefreshBots();
            base.OnAttachedToVisualTree(e);
        }
    }
}
