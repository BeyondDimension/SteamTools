using System;
using System.Application.UI.Activities;
using System.Collections.Generic;
using System.Text;
using JClass = Java.Lang.Class;

namespace System.Application.UI
{
    partial class MainApplication : IAndroidApplication
    {
        int? IAndroidApplication.NotificationSmallIconResId => Resource.Drawable.ic_stat_notify_msg;

        JClass IAndroidApplication.NotificationEntrance => typeof(MainActivity).GetJClass();

        Type IAndroidApplication.MainActivityType => typeof(MainActivity);
    }
}
