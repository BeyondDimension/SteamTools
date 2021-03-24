using Foundation;
using System.Application.UI;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace System.Application
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register(Name)]
    public partial class AppDelegate : FormsApplicationDelegate
    {
        public const string Name = "AppDelegate";

        //
        // This method is invoked when the application has loaded and is ready to run. In this
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Forms.Init();
            FormsMaterial.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }
    }
}