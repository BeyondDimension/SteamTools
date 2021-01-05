using Microsoft.Win32;
using SteamTools.Models;
using SteamTools.Models.Settings;
using SteamTools.Properties;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTool.Core.Common;
using System.Xml;
using System.IO;
using MetroTrilithon.Mvvm;

namespace SteamTools.ViewModels
{
    public class LocalAuthPageViewModel : TabItemViewModel
    {
        public int ProgressBarDefaultValue => 50;

        public override string Name
        {
            get { return Properties.Resources.SteamAuth; }
            protected set { throw new NotImplementedException(); }
        }

        public bool _IsEdit;
        public bool IsEdit
        {
            get => this._IsEdit;
            set
            {
                if (this._IsEdit != value)
                {
                    this._IsEdit = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public void ImageDelete_Click(WinAuthAuthenticator auth)
        {
            AuthService.Current.Authenticators.Remove(auth);
            if (AuthService.Current.Authenticators.Count == 0)
            {
                AuthSettings.Authenticators.Value = null;
                IsEdit = false;
            }
        }

        public void ImageSeek_Click(WinAuthAuthenticator auth)
        {
            if (auth.AutoRefresh == false)
            {
                auth.AutoRefresh = true;
                auth.ProgressBarValue = ProgressBarDefaultValue;
                Task.Run(async () =>
                {
                    while (auth.AutoRefresh)
                    {
                        auth.CurrentCode = string.Empty;
                        auth.ProgressBarValue--;
                        if (auth.ProgressBarValue == 0)
                        {
                            auth.ProgressBarValue = ProgressBarDefaultValue;
                            auth.AutoRefresh = false;
                        }
                        await Task.Delay(100);
                    }
                }).ContinueWith(s => s.Dispose());
            }
        }

        public void ImageCopy_Click(WinAuthAuthenticator auth)
        {
            auth.CopyCodeToClipboard();
            TaskbarService.Current.Notify("已复制令牌");
        }


        public void AddAuth_Click()
        {
            new AddAuthWindow() { DataContext = new AddAuthWindowViewModel() }.Show();
        }

        public void EditAuth_Click()
        {
            if (AuthService.Current.Authenticators == null || !AuthService.Current.Authenticators.Any())
            {
                StatusService.Current.Notify("没有可编辑的令牌");
                return;
            }
            this.IsEdit = !IsEdit;
            if (!IsEdit)
            {
                AuthSettings.Authenticators.Value = AuthService.ConvertJsonAuthenticator(AuthService.Current.Authenticators.ToList()).CompressString();
            }
        }

        public void ImageShowAuth_Click(WinAuthAuthenticator auth)
        {
            new ShowAuthWindow() { DataContext = new ShowAuthWindowViewModel(auth) }.Show();
        }


        public void ImageAuthTrade_Click(WinAuthAuthenticator auth)
        {
            new AuthTradeWindow() { DataContext = new AuthTradeWindowViewModel(auth) }.Show();
        }


        public void ImageAuthExport_Click()
        {
            if (!string.IsNullOrEmpty(AuthSettings.Authenticators.Value))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    AddExtension = true,
                    CheckPathExists = true,
                    Filter = "AuthData Files (*.dat)|*.dat",
                    RestoreDirectory = true,
                    FileName = $"{ProductInfo.Title} Authenticator {DateTime.Now.ToString("yyyy-MM-dd")}",
                    Title = ProductInfo.Title + " | " + Resources.Auth_Export,
                };
                if (saveFileDialog.ShowDialog(App.Current.MainWindow) == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, AuthSettings.Authenticators.Value);
                }
            }
        }

        public void ImageAuthRefresh_Click()
        {
            if (!string.IsNullOrEmpty(AuthSettings.Authenticators.Value))
            {
                AuthService.Current.Initialize();
            }
        }
    }
}
