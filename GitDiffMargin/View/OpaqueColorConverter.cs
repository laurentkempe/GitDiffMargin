using System;
using System.Globalization;
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
            // We don't need to convert back. What should we do anyways? We don't know the original opacity, nor we can we get it.
            throw new NotSupportedException();
        }
    }
}
