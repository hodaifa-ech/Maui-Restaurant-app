// Converters/InverseBoolToFontAttributesConverter.cs
using System.Globalization;
using Microsoft.Maui.Controls;

namespace newRestaurant.Converters // <--- MAKE SURE THIS NAMESPACE IS CORRECT
{
    public class InverseBoolToFontAttributesConverter : IValueConverter
    {
        public FontAttributes TrueAttribute { get; set; } = FontAttributes.None;
        public FontAttributes FalseAttribute { get; set; } = FontAttributes.Bold;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If IsRead is true (value=true), return TrueAttribute (None)
                // If IsRead is false (value=false), return FalseAttribute (Bold)
                return boolValue ? TrueAttribute : FalseAttribute;
            }
            return TrueAttribute;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}