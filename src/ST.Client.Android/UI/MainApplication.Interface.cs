using System;
using System.Application.Services;
using System.Application.UI.Activities;
using System.Collections.Generic;
using System.Text;
using JClass = Java.Lang.Class;

namespace System.Application.UI
{
    partial class MainApplication : IAndroidApplication
    {
        int? IAndroidApplication.NotificationSmallIconResId => Resource.Drawable.ic_stat_notify_msg;

        JClass IAndroidApplication.NotificationEntrance => ((IAndroidApplication)this).MainActivityType.GetJClass();

        Type IAndroidApplication.MainActivityType => typeof(MainActivity2);
    }
}
