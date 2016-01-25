using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace ProjectExplorer.Converters
{
    public class TextToColorConverter : MarkupExtension, IValueConverter
    {
        private static TextToColorConverter _converter = null;

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.LightGray;
            return ColorConverter.ConvertFromString(value as string);
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var color = (Color)value;

            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2"); ;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _converter ?? (_converter = new TextToColorConverter());
        }
    }
}
