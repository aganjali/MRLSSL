using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Visualizer.UserControls
{
    /// <summary>
    /// Interaction logic for GetNumberUserControl.xaml
    /// </summary>
    public partial class GetNumberUserControl : UserControl
    {
        int cur = 0;
        public GetNumberUserControl()
        {
            InitializeComponent();
        }




        public static DependencyProperty ValueProperty = DependencyProperty.Register("ParameterValue", typeof(double), typeof(GetNumberUserControl));
        public double ParameterValue
        {
            get { return (double)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);

            }
        }


        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            double d;
            if (double.TryParse(textBox1.Text, out d))
                slider.Value = d;
        }

        public delegate void ValueChange(object sender, double value);
        public event ValueChange ValueChanged;

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            cur = textBox1.SelectionStart;
            textBox1.Text = slider.Value.ToString("f3");
            textBox1.SelectionStart = cur;
            if (ValueChanged != null)
                ValueChanged(this, slider.Value);
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                slider.Value += 1;
            else if (e.Key == Key.Down)
                slider.Value -= 1;
        }

    }
}
