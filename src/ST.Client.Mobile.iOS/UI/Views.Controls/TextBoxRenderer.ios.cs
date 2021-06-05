using Foundation;
using UIKit;
using Xamarin.Forms.Platform.iOS;

namespace System.Application.UI.Views.Controls
{
    partial class TextBoxRenderer : EntryRenderer
    {
        public TextBoxRenderer() : base()
        {
        }

        protected override UITextField CreateNativeControl()
        {
            var textBox = base.CreateNativeControl();
            //textBox.SetValueForKey(NSNumber.FromInt32(DefaultPaddingLR), new NSString("_paddingLeft"));
            //textBox.SetValueForKey(NSNumber.FromInt32(DefaultPaddingLR), new NSString("_paddingRight"));
            return textBox;
        }
    }
}