using AndroidX.Fragment.App;
using Google.Android.Material.DatePicker;
using System.Application.Columns;
using System.Application.UI.Resx;
using JObject = Java.Lang.Object;

namespace System.Application.UI
{
    /// <summary>
    /// https://material.io/components/date-pickers
    /// <para>https://developer.android.google.cn/reference/com/google/android/material/datepicker/MaterialDatePicker?hl=zh-cn</para>
    /// </summary>
    public static class DatePickerHelper
    {
        static DateTimeOffset GetDateTimeOffset(object value)
        {
            var valueInt64 = Convert.ToInt64(value);
            var valueDateTimeOffset = valueInt64.ToDateTimeOffset();
            return valueDateTimeOffset.ToLocalTime();
        }

        /// <summary>
        /// 构建一个日期选择器
        /// </summary>
        /// <param name="listener">点击事件监听</param>
        /// <param name="title">标题</param>
        /// <param name="selected">默认选中时间</param>
        /// <param name="selectionRange">可选的时间范围</param>
        /// <returns></returns>
        static MaterialDatePicker GetDatePicker(object listener, string? title = null, DateTime selected = default, (DateTime min, DateTime max) selectionRange = default)
        {
            var dataPicker = MaterialDatePicker.Builder.DatePicker();

            if (title != null) dataPicker.SetTitleText(title);

            var builder = new CalendarConstraints.Builder();
            // 设置默认选中时间
            if (selected != default)
            {
                dataPicker.SetSelection(selected.ToUnixTimeMilliseconds());

                // 打开弹窗显示的月份
                builder.SetOpenAt(selected.GetCurrentMonth().ToUnixTimeMilliseconds());
            }

            if (selectionRange != default)
            {
                (DateTime min, DateTime max) = selectionRange;

                // 可选择的月历范围
                builder.SetStart(min.GetCurrentMonth().ToUnixTimeMilliseconds());
                builder.SetEnd(max.GetCurrentMonth().ToUnixTimeMilliseconds());

                // 可选择的日期范围
                builder.SetValidator(CompositeDateValidator.AllOf(new[] {
                    DateValidatorPointForward.From(min.ToUnixTimeMilliseconds()),
                    DateValidatorPointForward.From(max.ToUnixTimeMilliseconds()),
                }));
                dataPicker.SetCalendarConstraints(builder.Build());
            }

            var dialogFragment = dataPicker.Build();
            if (listener is IMaterialPickerOnPositiveButtonClickListener onPositiveButtonClickListener)
            {
                dialogFragment.AddOnPositiveButtonClickListener(onPositiveButtonClickListener);
            }
            return dialogFragment;
        }

        static MaterialDatePicker GetBirthDatePicker(object listener)
            => GetDatePicker(listener,
                AppResources.UserProfile_BirthDate,
                IBirthDate.DefaultSelected,
                IBirthDate.SelectionRange);

        static void ShowBirthDatePicker(this FragmentManager fragmentManager, object listener)
        {
            var dialogFragment = GetBirthDatePicker(listener);
            dialogFragment.Show(fragmentManager, dialogFragment.ToString());
        }

        /// <summary>
        /// 显示生日日期选择
        /// </summary>
        /// <param name="fragment"></param>
        /// <param name="listener"></param>
        public static void ShowBirthDatePicker(this Fragment fragment, object? listener = null)
            => ShowBirthDatePicker(fragment.Activity, listener ?? fragment);

        /// <summary>
        /// 显示生日日期选择
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="listener"></param>
        public static void ShowBirthDatePicker(this FragmentActivity activity, object? listener = null)
            => ShowBirthDatePicker(activity.SupportFragmentManager, listener ?? activity);

        /// <summary>
        /// 日期选择确定按钮点击时监听
        /// <para>https://developer.android.google.cn/reference/com/google/android/material/datepicker/MaterialPickerOnPositiveButtonClickListener?hl=en#onPositiveButtonClick(S)</para>
        /// </summary>
        public interface IOnPositiveButtonClickListener : IMaterialPickerOnPositiveButtonClickListener
        {
            void IMaterialPickerOnPositiveButtonClickListener.OnPositiveButtonClick(JObject selection) => OnPositiveButtonClick(GetDateTimeOffset(selection));

            /// <inheritdoc cref="IOnPositiveButtonClickListener"/>
            void OnPositiveButtonClick(DateTimeOffset selection);
        }
    }
}
