// Converters/InverseBoolConverter.cs
using System.Globalization; // Add this if not present

namespace newRestaurant.Converters // Ensure this namespace matches your folder structure
{
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value; // Return original value if not a bool
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            return value; // Return original value if not a bool
        }
    }
}