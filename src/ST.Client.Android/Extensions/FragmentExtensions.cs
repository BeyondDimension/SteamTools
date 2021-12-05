using System;
using System.Application.UI.Activities;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Platform.Android;
using Fragment = AndroidX.Fragment.App.Fragment;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// <see cref="Fragment"/> 扩展
    /// </summary>
    public static partial class FragmentExtensions
    {
        public static bool IsEmbeddedForms(this Fragment? fragment)
        {
            return fragment?.Activity is FormsAppCompatActivity;
        }
    }
}
