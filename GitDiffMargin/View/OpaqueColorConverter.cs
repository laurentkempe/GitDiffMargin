using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GitDiffMargin.View
{
    [ValueConversion(typeof(Brush), typeof(Brush))]
    class OpaqueColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType != typeof(Brush))
            {
                throw new ArgumentException("Target type is not a brush.");
            }

            var newBrush = ((Brush)value).Clone();
            newBrush.Opacity = 1.0; // Remove transparency

            return newBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // It's not possible to convert back: we don't know the original opacity nor can we get it in someway.
            return DependencyProperty.UnsetValue;
        }
    }
}
