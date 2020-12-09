using MetroTrilithon.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SteamTools.Models.Settings
{
    public static class AuthSettings
    {
        /// <summary>
        /// ÁîÅÆÊý¾Ý(Ñ¹Ëõ´æ´¢£©
        /// </summary>
        public static SerializableProperty<string> Authenticators { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local) { AutoSave = true };


        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(GeneralSettings) + "." + propertyName;
        }
    }
}
