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
            //this.Topmost = true;
        }

        public bool IsWinAuth { get; set; }
        public bool IsSteamApp { get; set; } = true;
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

        private string _AuthName;
        public string AuthName
        {
            get => this._AuthName;
            set
            {
                if (this._AuthName != value)
                {
                    this._AuthName = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        private string _UUID;
        public string UUID
        {
            get => this._UUID;
            set
            {
                if (this._UUID != value)
                {
                    this._UUID = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private string _SteamGuard;
        public string SteamGuard
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
                Filter = "MaFile Files (*.maFile)|*.maFile|Json Files (*.json)|*.json|All Files (*.*)|*.*",
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
                AuthService.Current.ImportAuthenticators(WinAuthFileName);
                AuthSettings.Authenticators.Value = AuthService.ConvertJsonAuthenticator(AuthService.Current.Authenticators).CompressString();
                //(WindowService.Current.MainWindow as MainWindowViewModel).LocalAuthPage
                StatusService.Current.Set(Resources.Ready);
            }
        }

        public void ImportSteamGuard()
        {
            StatusService.Current.Notify("导入Steam App令牌中...");
            if (AuthService.Current.ImportSteamGuard(AuthName, UUID, SteamGuard))
            {
                AuthSettings.Authenticators.Value = AuthService.ConvertJsonAuthenticator(AuthService.Current.Authenticators).CompressString();
            }
            else
            {
                StatusService.Current.Notify("导入失败，确认数据是否正确");
            }
        }

        public void ImportSDA()
        {
            StatusService.Current.Set("导入Steam Desktop Auth令牌中...");
            if (AuthService.Current.ImportSDAFile(SDAFile, SDAPassword))
            {
                AuthSettings.Authenticators.Value = AuthService.ConvertJsonAuthenticator(AuthService.Current.Authenticators).CompressString();
                StatusService.Current.Set(Resources.Ready);
            }
            else
            {
                StatusService.Current.Set(Resources.Ready);
                StatusService.Current.Notify("导入失败，确认数据是否正确");
            }
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
