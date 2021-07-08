using Android.Widget;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// 对 <see cref="EditText"/> 类的扩展函数集
    /// </summary>
    public static class EditTextExtensions
    {
        public static void SetReadOnly(this EditText editText)
        {
            editText.KeyListener = null;
            editText.SetTextIsSelectable(true);
        }
    }
}