// Converters/InverseBoolConverter.cs
using System.Globalization;
using Microsoft.Maui.Controls; // Needed for IValueConverter

namespace newRestaurant.Converters // <--- MAKE SURE THIS NAMESPACE IS CORRECT
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value;
        }
    }
}