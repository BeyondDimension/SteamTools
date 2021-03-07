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
                switch (e.NewValue)
                {
                    case 0:
                        AppHelper.Current.Theme = AppTheme.Light;
                        break;
                    case 1:
                        AppHelper.Current.Theme = AppTheme.Dark;
                        break;
                }
            }
        }

        /// <summary>
        /// ÷˜Ã‚
        /// </summary>
        public static SerializableProperty<short> Theme { get; }
            = new SerializableProperty<short>(GetKey(), Providers.Local, 1) { AutoSave = true };

        /// <summary>
        /// ”Ô—‘
        /// </summary>
        public static SerializableProperty<string> Language { get; }
            = new SerializableProperty<string>(GetKey(), Providers.Local, "") { AutoSave = true };

        private static string GetKey([CallerMemberName] string propertyName = "")
        {
            return nameof(UISettings) + "." + propertyName;
        }
    }
}
