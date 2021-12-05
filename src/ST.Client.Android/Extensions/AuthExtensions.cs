using Android.App;
using System;
using System.Collections.Generic;
using System.Text;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class AuthExtensions
    {
        public static bool IsAuthenticated(this Activity activity)
        {
            var value = UserService.Current.IsAuthenticated;
            if (!value)
            {
                activity.Finish();
            }
            return value;
        }
    }
}
