using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using MRL.SSL.CommonClasses.Extentions;

namespace Visualizer.Converters
{
    public class ColorToBrush : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            
            if (value == null) return Brushes.Black;
            Brush ColorBrush;
            System.Drawing.Color _color = value.As<System.Drawing.Color>();
            System.Windows.Media.BrushConverter bc = new System.Windows.Media.BrushConverter();
            if (_color.IsNamedColor)
                ColorBrush = (System.Windows.Media.Brush)bc.ConvertFromString(_color.Name);
            else
                ColorBrush = (System.Windows.Media.Brush)bc.ConvertFromString("#" + _color.Name);            
            return ColorBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
