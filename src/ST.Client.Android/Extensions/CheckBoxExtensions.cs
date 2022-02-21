using Android.Graphics.Drawables;
using Android.Widget;
using R = System.Application.Resource;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// <see cref="CheckBox"/> 扩展
    /// </summary>
    public static partial class CheckBoxExtensions
    {
        public static void SetChecked(this CheckBox checkBox, bool? value)
        {
            if (checkBox.GetChecked() == value) return;

            Drawable? drawable = null;
            if (value.HasValue)
            {
                checkBox.Checked = value.Value;
                if (checkBox.GetTag(R.Id.checkbox_indeterminate_state) is Drawable d && checkBox.ButtonDrawable != d) drawable = d;
                checkBox.SetTag(R.Id.checkbox_indeterminate_state, null);
            }
            else
            {
                if (checkBox.GetTag(R.Id.checkbox_indeterminate_state) == null)
                    checkBox.SetTag(R.Id.checkbox_indeterminate_state, checkBox.ButtonDrawable);
                drawable = checkBox.Context!.GetDrawableCompat(R.Drawable.bg_checkbox_indeterminate);
            }
            if (drawable != null) checkBox.SetButtonDrawable(drawable);
        }

        public static bool? GetChecked(this CheckBox checkBox)
        {
            var state = checkBox.GetTag(R.Id.checkbox_indeterminate_state);
            if (state != null) return null;
            return checkBox.Checked;
        }
    }
}
