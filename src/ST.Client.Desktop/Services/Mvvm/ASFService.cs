using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public class ASFService : ReactiveObject
    {
        #region static members
        public static ASFService Current { get; } = new();
        #endregion

        bool _IPCUrl;
        public bool IPCUrl
        {
            get => _IPCUrl;
            set => this.RaiseAndSetIfChanged(ref _IPCUrl, value);
        }

        string? _CommandText;
        public string? CommandText
        {
            get => _CommandText;
            set => this.RaiseAndSetIfChanged(ref _CommandText, value);
        }

    }
}
