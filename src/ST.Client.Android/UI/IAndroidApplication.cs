using System;
using System.Application.Services;
using System.Collections.Generic;
using System.Text;
using JClass = Java.Lang.Class;

namespace System.Application.UI
{
    public interface IAndroidApplication : IService<IAndroidApplication>, IApplication
    {
        static new IAndroidApplication Instance => IService<IAndroidApplication>.Instance;

        int? NotificationSmallIconResId { get; }

        JClass NotificationEntrance { get; }

        Type MainActivityType { get; }
    }
}
