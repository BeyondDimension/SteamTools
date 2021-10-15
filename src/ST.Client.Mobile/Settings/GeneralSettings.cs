using System;
using System.Collections.Generic;
using System.Text;
using System.Application.Settings.Abstractions;

namespace System.Application.Settings
{
    public sealed class GeneralSettings : GeneralSettings<GeneralSettings>
    {
        /// <summary>
        /// 屏幕捕获
        /// </summary>
        public static SerializableProperty<bool> CaptureScreen { get; }
            = GetProperty(defaultValue: false, autoSave: true);
    }
}
