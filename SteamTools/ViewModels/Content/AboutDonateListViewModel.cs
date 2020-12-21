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
    public class AboutDonateListViewModel : Livet.ViewModel
    {
        private readonly HttpServices httpServices = SteamToolCore.Instance.Get<HttpServices>();

        public class DonateRecord
        {
            public long Index { get; set; }
            public string Name { get; set; }
            public decimal Amount { get; set; }
            public string OrderNumber { get; set; }
            public string Remark { get; set; }
            public short PayType { get; set; }
            public DateTime PayTime { get; set; }
        }

        private List<DonateRecord> _DonateRecordList;
        public List<DonateRecord> DonateRecordList
        {
            get => this._DonateRecordList;
            set
            {
                if (this._DonateRecordList != value)
                {
                    this._DonateRecordList = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public AboutDonateListViewModel()
        {
            Task.Run(() =>
            {
                httpServices.Get(Const.REWARDMELIST_URL).ContinueWith(s =>
                {
                    if (!string.IsNullOrEmpty(s.Result))
                    {
                        DonateRecordList = JsonConvert.DeserializeObject<List<DonateRecord>>(s.Result);
                    }
                });
            });
        }
    }
}
