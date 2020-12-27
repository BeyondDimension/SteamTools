using MetroTrilithon.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SteamTools.Models.Settings
{
    public static class UISettings
    {
        /// <summary>
        /// Ö÷Ìâ
        /// </summary>
        public static SerializableProperty<short> Theme { get; }
            = new SerializableProperty<short>(GetKey(), Providers.Roaming) { AutoSave = true };

        /// <summary>
        /// ÑÕÉ«
        /// </summary>
        public static SerializableProperty<short> Accent { get; }
            = new SerializableProperty<short>(GetKey(), Providers.Roaming) { AutoSave = true };


        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(UISettings) + "." + propertyName;
        }
    }
}
