using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace System.Application.UI
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    public abstract class AppDelegate : FormsApplicationDelegate
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
            VisualStudioAppCenterSDK.Init();

            DILevel level;
            /*if (isMainProcess)*/
            level = DILevel.MainProcess;
            //level = DILevel.Min;
            Startup.Init(level);

            Forms.Init();
            FormsMaterial.Init();
            XF.Material.iOS.Material.Init();
            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        public static bool IsMainProcess { get; private set; }
    }
}