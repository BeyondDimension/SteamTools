#if MONO_MAC
using MonoMac.AppKit;
using MonoMac.Foundation;
#elif XAMARIN_MAC
using AppKit;
using Foundation;
#endif

namespace System.Application.UI
{
    /// <summary>
    /// This class is an AppDelegate helper specifically for Mac OSX
    /// Int it's infinite wisdom and unlike Linux and or Windows Mac does not pass in the URL from a sqrl:// invokation
    /// directly as a startup app paramter, instead it uses a System Event to do this which has to be registered
    /// and listed to.
    /// This requires us to use Xamarin.Mac to make it work with .net standard
    /// </summary>
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        /// <summary>
        /// Because we are creating our own mac application delegate we are removing / overriding
        /// the one that Avalonia creates. This causes the application to not be handled as it should.
        /// This is the Avalonia Implementation: https://github.com/AvaloniaUI/Avalonia/blob/5a2ef35dacbce0438b66d9f012e5f629045beb3d/native/Avalonia.Native/src/OSX/app.mm
        /// So what we are doing here is re-creating this implementation to mimick their behavior.
        /// </summary>
        /// <param name="notification"></param>
        public override void WillFinishLaunching(NSNotification notification)
        {
            if (NSApplication.SharedApplication.ActivationPolicy != NSApplicationActivationPolicy.Regular)
            {
                foreach (var x in NSRunningApplication.GetRunningApplications(@"com.apple.dock"))
                {
                    x.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
                    break;
                }
                NSApplication.SharedApplication.ActivationPolicy = NSApplicationActivationPolicy.Regular;
            }
        }

        /// <summary>
        /// Because we are creating our own mac application delegate we are removing / overriding
        /// the one that Avalonia creates. This causes the application to not be handled as it should.
        /// This is the Avalonia Implementation: https://github.com/AvaloniaUI/Avalonia/blob/5a2ef35dacbce0438b66d9f012e5f629045beb3d/native/Avalonia.Native/src/OSX/app.mm
        /// So what we are doing here is re-creating this implementation to mimick their behavior.
        /// </summary>
        /// <param name="notification"></param>
        public override void DidFinishLaunching(NSNotification notification)
        {
#if !NETSTANDARD && !NET5_0 && !NET6_0
            VisualStudioAppCenterSDK.Init();
#endif

            NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
        }
    }

    public static class AppDelegateHelper
    {
        static bool isInitialized;
        internal static AppDelegate? Instance { get; private set; }

        public static void Init(string[] args)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                NSApplication.Init();
                //NSApplication.Main(args);
                Instance = new();
                NSApplication.SharedApplication.Delegate = Instance;
            }
        }
    }
}