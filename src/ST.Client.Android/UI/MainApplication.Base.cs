#if !NET6_0_OR_GREATER
using AndroidApplication = Android.App.Application;

namespace System.Application.UI
{
    partial class MainApplication : AndroidApplication
    {

    }
}
#endif