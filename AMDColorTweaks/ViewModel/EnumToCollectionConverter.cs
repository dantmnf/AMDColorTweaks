using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace AMDColorTweaks.ViewModel
{
    //[ValueConversion(typeof(Enum), typeof(IEnumerable<EnumWithDescription<Enum>>))]
    //public class EnumToCollectionConverter : MarkupExtension, IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        var type = value.GetType();
    //        if (!type.IsEnum)
    //            throw new ArgumentException("Value must be an enum", "value");
    //        var name = value.ToString();
    //        var attr = type.GetField(name).GetCustomAttributes(typeof(DescriptionAttribute), false);
    //        var desc = (attr.FirstOrDefault() as DescriptionAttribute)?.Description;
            
    //        return EnumHelper.GetAllValuesAndDescriptions(value.GetType());
    //    }
    //    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        return null;
    //    }
    //    public override object ProvideValue(IServiceProvider serviceProvider)
    //    {
    //        return this;
    //    }
    //}
}
