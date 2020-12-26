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

        private BindingList<WinAuthAuthenticator> _Authenticators;

        public BindingList<WinAuthAuthenticator> Authenticators
        {
            get => this._Authenticators;
            set
            {
                if (this._Authenticators != value)
                {
                    this._Authenticators = value;
                    this.RaisePropertyChanged();
                }
            }
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
            Authenticators.Remove(auth);
            if (Authenticators.Count == 0)
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

        internal async override Task Initialize()
        {
            AuthService.Current.Subscribe(nameof(AuthService.Current.Authenticators),
                () => { Authenticators = AuthService.Current.Authenticators; }).AddTo(this);
            await Task.CompletedTask;
        }

        public void AddAuth_Click()
        {
            new AddAuthWindow() { DataContext = new AddAuthWindowViewModel() }.Show();
        }

        public void EditAuth_Click()
        {
            if (Authenticators == null || !Authenticators.Any())
            {
                StatusService.Current.Notify("没有可编辑的令牌");
                return;
            }
            this.IsEdit = !IsEdit;
            if (!IsEdit)
            {
                AuthSettings.Authenticators.Value = AuthService.ConvertJsonAuthenticator(Authenticators.ToList()).CompressString();
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
    }
}
