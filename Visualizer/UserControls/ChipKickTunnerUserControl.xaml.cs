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
    /// Interaction logic for ChipKickTunnerUserControl.xaml
    /// </summary>
    public partial class ChipKickTunnerUserControl : UserControl
    {

   

        public ChipKickTunnerUserControl()
        {
            InitializeComponent();
       
        }
        //private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    double d;
        //    if (double.TryParse(textBox1.Text, out d))
        //        slider.Value = d;
        //}

        //public delegate void ValueChange(object sender, double value);
        //public event ValueChange ValueChanged;

        //private void radSlider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{

        //}
        //protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.Property.Name == "Tag")
        //        textBox1.Text = Tag.ToString();
        //}

        //private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    if (ValueChanged != null)
        //        ValueChanged(this, slider.Value);
        //}

    }
}
