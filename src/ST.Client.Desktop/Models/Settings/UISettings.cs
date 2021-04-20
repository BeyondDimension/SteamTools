using System;
using System.Application.Serialization;
using System.Application.UI;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Application.Models.Settings
{
    public static class UISettings
    {
        static UISettings()
        {
            //Theme.ValueChanged += Theme_ValueChanged;
        }

        private static void Theme_ValueChanged(object sender, ValueChangedEventArgs<short> e)
        {
            if (e.NewValue != e.OldValue)
            {
                var value = (AppTheme)e.NewValue;
                if (value.IsDefined())
                {
                    AppHelper.Current.Theme = value;
                }
            }
        }

        /// <summary>
        /// Ö÷Ìâ
        /// </summary>
        public static SerializableProperty<short> Theme { get; }
            = new SerializableProperty<short>(GetKey(), Providers.Local, 0) { AutoSave = true };

        /// <summary>
        /// ÓïÑÔ
        /// </summary>
        public static SerializableProperty<string> Language { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, "") { AutoSave = true };

        /// <summary>
        /// ×ÖÌå
        /// </summary>
        public static SerializableProperty<string> FontName { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, "Default") { AutoSave = true };

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(UISettings) + "." + propertyName;
        }
    }
}
