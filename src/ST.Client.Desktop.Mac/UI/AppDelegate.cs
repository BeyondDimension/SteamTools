#if MONO_MAC
using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Application.UI.ViewModels;
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
            Console.WriteLine("开始注册通知");
            try
            {
                var manager = NSAppleEventManager.SharedAppleEventManager;
                manager.SetEventHandler(this,
                                       new MonoMac.ObjCRuntime.Selector("handleGetURLEvent:withReplyEvent:"),
                                       AEEventClass.Internet,
                                       AEEventID.GetUrl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

#if MONO_MAC 

        [Export("handleGetURLEvent:withReplyEvent:")]
        private void HandleGetURLEvent(NSAppleEventDescriptor descriptor, NSAppleEventDescriptor replyEvent)
        {
            try
            {
                // stackoverflow.com/questions/1945
                // https://forums.xamarin.com/discussion/9774/custom-url-schema-handling
                var keyDirectObject = "----";
                var keyword =
                    ((uint)keyDirectObject[0]) << 24 |
                    ((uint)keyDirectObject[1]) << 16 |
                    ((uint)keyDirectObject[2]) << 8 |
                    ((uint)keyDirectObject[3]);

                var urlString = descriptor.ParamDescriptorForKeyword(keyword).StringValue;

                using (var alert = new NSAlert())
                {
                    alert.MessageText = urlString;
                    alert.RunSheetModal(null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
#elif XAMARIN_MAC
        public override void OpenUrls(NSApplication application, NSUrl[] urls)
        {
            throw new System.Exception("opened with: " + urls[0].AbsoluteString);
        }

#endif

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

        public override bool ApplicationShouldHandleReopen(NSApplication sender, bool hasVisibleWindows)
        {
            if (hasVisibleWindows)
            {
                return false;
            }
            else
            {
                // 点击 Dock 重新打开已关闭的窗口
                IApplication.Instance.RestoreMainWindow();
                return true;
            }
        }

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