using System;
using System.Application.Services;
using System.Collections.Generic;
using System.Text;
using JClass = Java.Lang.Class;

namespace System.Application.UI
{
    public interface IAndroidApplication : IApplication
    {
        static new IAndroidApplication Instance => DI.Get<IAndroidApplication>();

        int? NotificationSmallIconResId { get; }

        Type MainActivityType { get; }
    }
}
