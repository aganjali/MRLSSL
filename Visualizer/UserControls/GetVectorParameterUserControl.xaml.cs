using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MRL.SSL.CommonClasses.MathLibrary;

namespace Visualizer.UserControls
{
    /// <summary>
    /// Interaction logic for GetVectorParameterUserControl.xaml
    /// </summary>
    public partial class GetVectorParameterUserControl : UserControl
    {
        int lastcurx = 0, lastcury = 0;

        public GetVectorParameterUserControl()
        {
            InitializeComponent();
        }

        public static DependencyProperty ValueProperty = DependencyProperty.Register("ParameterValue", typeof(Position2D), typeof(GetVectorParameterUserControl));
        public Position2D ParameterValue
        {
            get { return (Position2D)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);

            }
        }

        public delegate void ValueChange(object sender, Position2D value);
        public event ValueChange ValueChanged;

        private void xradSlider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lastcurx = xtextBox1.SelectionStart;
            xtextBox1.Text = xradSlider1.Value.ToString("f3");
            xtextBox1.SelectionStart = lastcurx;
            if (ValueChanged != null)
                ValueChanged(this, new Position2D(xradSlider1.Value, yradSlider2.Value));
        }

        private void yradSlider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lastcury = ytextBox2.SelectionStart;
            ytextBox2.Text = yradSlider2.Value.ToString("f3");
            ytextBox2.SelectionStart = lastcury;
            if (ValueChanged != null)
                ValueChanged(this, new Position2D(xradSlider1.Value, yradSlider2.Value));
        }

        private void ytextBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            double d;
            if (double.TryParse(ytextBox2.Text, out d) && yradSlider2 != null)
                yradSlider2.Value = d;
        }

        private void xtextBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            double d;
            if (double.TryParse(xtextBox1.Text, out d) && xradSlider1 != null)
                xradSlider1.Value = d;
        }

        private void ytextBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                yradSlider2.Value += 1;
            else if (e.Key == Key.Down)
                yradSlider2.Value -= 1;
        }

        private void xtextBox1_KeyDown(object sender, KeyEventArgs e)
        {
             if (e.Key == Key.Up)
                xradSlider1.Value += 1;
            else if (e.Key == Key.Down)
                 xradSlider1.Value -= 1;
        }

    }
}
