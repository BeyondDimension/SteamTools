using System.Application.UI.Views.Controls;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(TextBox), typeof(TextBoxRenderer))]
namespace System.Application.UI.Views.Controls
{
    internal partial class TextBoxRenderer
    {
        /// <summary>
        /// 默认左右内边距
        /// </summary>
        const int DefaultPaddingLR = 10;
    }
}