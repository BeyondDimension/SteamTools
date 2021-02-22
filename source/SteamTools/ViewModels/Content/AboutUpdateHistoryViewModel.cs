using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using SteamTools.Win32;
using MetroRadiance.Interop.Win32;
using System.Diagnostics;
using SteamTool.Core.Common;
using System.Runtime.InteropServices;
using SteamTools.Services;
using SteamTool.Core;
using System.IO;
using SteamTools.Properties;
using SteamTool.Model;
using Newtonsoft.Json;
using System.Threading;

namespace SteamTools.ViewModels
{
    public class AboutUpdateHistoryViewModel : Livet.ViewModel
    {
        private readonly HttpServices httpServices = SteamToolCore.Instance.Get<HttpServices>();

        private string _UpdateHistoryText;
        public string UpdateHistoryText
        {
            get => this._UpdateHistoryText;
            set
            {
                if (this._UpdateHistoryText != value)
                {
                    this._UpdateHistoryText = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public AboutUpdateHistoryViewModel()
        {
            _ = Task.Run(() =>
              {
                  httpServices.Get(Const.UPDATEHISTORY_URL).ContinueWith(s =>
                  {
                      if (!string.IsNullOrEmpty(s.Result))
                      {
                          UpdateHistoryText = s.Result;
                      }
                  });
              }).ContinueWith(s => { Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message); }, TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
