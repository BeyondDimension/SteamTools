using ArchiSteamFarm.Steam;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.UI.Controls;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Linq;

namespace System.Application.UI.Views.Pages
{
    public partial class ASF_BotPage : ReactiveUserControl<ArchiSteamFarmPlusPageViewModel>
    {
        private ContentDialog keyDialog;
        private TextBox keyText;
        private DataGrid keysDataGrid;

        public ASF_BotPage()
        {
            InitializeComponent();

            keyDialog = this.FindControl<ContentDialog>("RedeemKeyDialog");
            keyText = this.FindControl<TextBox>("keyText");
            keysDataGrid = this.FindControl<DataGrid>("keysDataGrid");
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
                keysDataGrid.Items = await ViewModel.GetUsedAndUnusedKeys(bot);

                if (await keyDialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrEmpty(keyText.Text))
                        return;
                    var keys = ExtractKeysFromString(keyText.Text);
                    if (keys.Count == 0)
                        return;

                    ViewModel.RedeemKeyBot(bot, keys);
                }
            }
        }


        private static IOrderedDictionary ExtractKeysFromString(string source)
        {
            MatchCollection m = Regex.Matches(source, "([0-9A-Z]{5})(?:\\-[0-9A-Z]{5}){2,3}",
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
    }
}
