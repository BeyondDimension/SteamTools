using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Properties;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Properties;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Models;

namespace System.Application.Services
{
    public sealed class NotificationService : ReactiveObject
    {
        static NotificationService? mCurrent;
        public static NotificationService Current => mCurrent ?? new();
        public SourceList<NoticeTypeDTO> NoticeTypes { get; }

        private ReadOnlyObservableCollection<NoticeTypeDTO>? _ObservableItems;
        public ReadOnlyObservableCollection<NoticeTypeDTO>? ObservableItems
        {
            get => _ObservableItems;
            set => this.RaiseAndSetIfChanged(ref _ObservableItems, value);
        }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        private NoticeTypeDTO? _SelectGroup;
        public NoticeTypeDTO? SelectGroup
        {
            get => _SelectGroup;
            set => this.RaiseAndSetIfChanged(ref _SelectGroup, value);
        }
        public async Task InitializeNotice()
        {
            var client = ICloudServiceClient.Instance.Notice;
            var result = await client.Types();
            if (result.IsSuccess)
            {
                NoticeTypes.Clear();
                NoticeTypes.AddRange(result.Content!);
            }
        }
        public NotificationService()
        {
            mCurrent = this;
            NoticeTypes = new SourceList<NoticeTypeDTO>();
            NoticeTypes
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<NoticeTypeDTO>.Ascending(x => x.Order).ThenBy(x => x.Name))
                .Bind(out _ObservableItems)
                .Subscribe(_ => SelectGroup = NoticeTypes.Items.FirstOrDefault());
        }
    }
}
