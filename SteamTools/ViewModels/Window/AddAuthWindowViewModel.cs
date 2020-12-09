using Microsoft.Win32;
using SteamTools.Models;
using SteamTools.Models.Settings;
using SteamTools.Properties;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using SteamTool.Core.Common;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class AddAuthWindowViewModel : MainWindowViewModelBase
    {
        public AddAuthWindowViewModel()
        {
            this.Title = ProductInfo.Title + " | " + Resources.Auth_Add;
        }

        public bool IsWinAuth { get; set; }
        public bool IsSteamApp { get; set; }
        public bool IsSDA { get; set; }

        private string _WinAuthFileName;
        public string WinAuthFileName
        {
            get => this._WinAuthFileName;
            set
            {
                if (this._WinAuthFileName != value)
                {
                    this._WinAuthFileName = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string _SDAFile;
        public string SDAFile
        {
            get => this._SDAFile;
            set
            {
                if (this._SDAFile != value)
                {
                    this._SDAFile = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        private string _SDAPassword;
        public string SDAPassword
        {
            get => this._SDAPassword;
            set
            {
                if (this._SDAPassword != value)
                {
                    this._SDAPassword = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private (string Name, string UUID, string Guard) _SteamGuard;
        public (string Name, string UUID, string Guard) SteamGuard
        {
            get => this._SteamGuard;
            set
            {
                if (this._SteamGuard != value)
                {
                    this._SteamGuard = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public void OpenWinAuthFile()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                RestoreDirectory = true,
                Title = ProductInfo.Title
            };
            if (ofd.ShowDialog(App.Current.MainWindow) == true)
            {
                WinAuthFileName = ofd.FileName;
                this.Topmost = true;
            }
        }

        public void OpenSDAFile()
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Json Files (*.json)|*.json|All Files (*.*)|*.*",
                RestoreDirectory = true,
                Title = ProductInfo.Title
            };
            if (ofd.ShowDialog(App.Current.MainWindow) == true)
            {
                SDAFile = ofd.FileName;
                this.Topmost = true;
            }
        }

        public void ImportWinAuthFile()
        {
            if (File.Exists(WinAuthFileName))
            {
                StatusService.Current.Set("导入WinAuth令牌中...");
                //Task.Run(() =>
                //{

                //});
                AuthService.Current.ImportAuthenticators(WinAuthFileName);
                AuthSettings.Authenticators.Value = AuthService.ConvertJsonAuthenticator(AuthService.Current.Authenticators).CompressString();
                //(WindowService.Current.MainWindow as MainWindowViewModel).LocalAuthPage
                StatusService.Current.Set(Resources.Ready);
            }
        }

        public void ImportSteamGuard()
        {
            StatusService.Current.Set("导入Steam App令牌中...");
            AuthService.Current.ImportSteamGuard(SteamGuard.Name, SteamGuard.UUID, SteamGuard.Guard);
            AuthSettings.Authenticators.Value = AuthService.ConvertJsonAuthenticator(AuthService.Current.Authenticators).CompressString();
            //(WindowService.Current.MainWindow as MainWindowViewModel).LocalAuthPage
            StatusService.Current.Set(Resources.Ready);
        }

        public void ImportSDA()
        {
            StatusService.Current.Set("导入Steam App令牌中...");
            AuthService.Current.ImportSDAFile(SDAFile, SDAPassword);
            AuthSettings.Authenticators.Value = AuthService.ConvertJsonAuthenticator(AuthService.Current.Authenticators).CompressString();
            //(WindowService.Current.MainWindow as MainWindowViewModel).LocalAuthPage
            StatusService.Current.Set(Resources.Ready);
        }

        public void Apply() 
        {
            if (IsWinAuth)
                ImportWinAuthFile();
            else if (IsSteamApp)
                ImportSteamGuard();
            else if(IsSDA)
                ImportSDA();
        }
    }
}
