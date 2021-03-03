using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Application.Converters
{
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type) : base(type)
        {
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (null != value)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());

                    if (null != fi)
                    {
                        var attributes =
                            (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                        return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description)))
                            ? attributes[0].Description
                            : value.ToString();
                    }
                }

                return string.Empty;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
