using System;
using System.Application.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Application.Models.Settings
{
    public static class SteamAccountSettings
    {
        /// <summary>
        /// Steam账号备注字典
        /// </summary>
        public static SerializableProperty<IReadOnlyDictionary<long, string?>?> AccountRemarks { get; }
            = new SerializableProperty<IReadOnlyDictionary<long, string?>?>(GetKey(), Providers.Local, null);

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(GameLibrarySettings) + "." + propertyName;
        }
    }
}
