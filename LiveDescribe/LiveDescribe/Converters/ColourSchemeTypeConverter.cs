using LiveDescribe.Model;
using System;
using System.ComponentModel;
using System.Globalization;

namespace LiveDescribe.Converters
{
    public class ColourSchemeTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var stringValue = value as string;
            if (stringValue != null && stringValue == "DefaultColourScheme")
            {
                return ColourScheme.DefaultColourScheme.DeepCopy();
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
