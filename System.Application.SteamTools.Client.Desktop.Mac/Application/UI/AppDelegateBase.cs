using MonoMac.AppKit;
using MonoMac.Foundation;

namespace System.Application.UI
{
    /// <summary>
    /// This class is an AppDelegate helper specifically for Mac OSX
    /// Int it's infinite wisdom and unlike Linux and or Windows Mac does not pass in the URL from a sqrl:// invokation
    /// directly as a startup app paramter, instead it uses a System Event to do this which has to be registered
    /// and listed to.
    /// This requires us to use MonoMac to make it work with .net core
    /// </summary>
    [Register("AppDelegate")]
    public sealed class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
            Init();
        }

        /// <summary>
        /// Registers an event for handling URL Invokation
        /// </summary>
        void Init()
        {
            //Register this Apple Delegate globablly with Avalonia for Later Use
            NSAppleEventManager.SharedAppleEventManager.SetEventHandler(this, new MonoMac.ObjCRuntime.Selector("handleGetURLEvent:withReplyEvent:"), AEEventClass.Internet, AEEventID.GetUrl);
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
            NSRunningApplication.CurrentApplication.Activate(NSApplicationActivationOptions.ActivateIgnoringOtherWindows);
        }
    }
}