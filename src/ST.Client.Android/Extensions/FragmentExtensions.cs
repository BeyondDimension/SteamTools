using System.Application.UI.Activities;
using System.Collections.Generic;
using System.Text;
#if __XAMARIN_FORMS__
using Xamarin.Forms.Platform.Android;
#endif
using Fragment = AndroidX.Fragment.App.Fragment;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// <see cref="Fragment"/> 扩展
    /// </summary>
    public static partial class FragmentExtensions
    {
#if __XAMARIN_FORMS__
        public static bool IsEmbeddedForms(this Fragment? fragment)
        {
            return fragment?.Activity is FormsAppCompatActivity;
        }
#endif
    }
}
