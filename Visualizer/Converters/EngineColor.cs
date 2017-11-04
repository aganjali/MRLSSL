using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using MRL.SSL.Visualizer.Extentions;

namespace Visualizer.Converters
{
    public class EngineColor : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return 0;
            if ((bool)value)
                return 1;
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.As<int>() == 0) 
                return false;
            return true;
        }

        #endregion
    }
}
