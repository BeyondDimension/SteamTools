using Android.Content;
using Android.Util;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// UI尺寸单位转换
    /// </summary>
    public static class DimenUnitConverterExtensions
    {
        const NumberToInt32Format Format = NumberToInt32Format.RoundAwayFromZero;

        /// <summary>
        /// 将[传入的单位值]与[传入的单位类型]转换为[px(像素)值]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        static float ToPx(this Context context, float value, ComplexUnitType unit)
        {
            var result = TypedValue.ApplyDimension(
                unit,
                value,
                context.Resources?.DisplayMetrics);
            return result;
        }

        /// <inheritdoc cref="ToPx(Context,float, ComplexUnitType)"/>
        static int ToPxInt32(this Context context, float value, ComplexUnitType unit, NumberToInt32Format format = Format)
        {
            var result = ToPx(context, value, unit).ToInt32(format);
            return result;
        }

        /// <summary>
        /// 将[px(像素)值]转换为[dp值]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float PxToDp(this Context context, float value)
        {
            var density = context.Resources?.DisplayMetrics?.Density;
            if (!density.HasValue) throw new ArgumentNullException(nameof(density));
            return value / density.Value;
        }

        /// <summary>
        /// 将[dp值]转换为[px(像素)值]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float DpToPx(this Context context, float value)
        {
            var result = ToPx(context, value, ComplexUnitType.Dip);
            return result;
        }

        /// <inheritdoc cref="DpToPx(Context, float)"/>
        public static int DpToPxInt32(this Context context, float value, NumberToInt32Format format = Format)
        {
            var result = ToPxInt32(context, value, ComplexUnitType.Dip, format);
            return result;
        }

        /// <summary>
        /// 将[sp值]转换为[px(像素)值]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float SpToPx(this Context context, float value)
        {
            var result = ToPx(context, value, ComplexUnitType.Sp);
            return result;
        }

        /// <inheritdoc cref="SpToPx(Context, float)"/>
        public static int SpToPxInt32(this Context context, float value, NumberToInt32Format format = Format)
        {
            var result = ToPxInt32(context, value, ComplexUnitType.Sp, format);
            return result;
        }
    }
}