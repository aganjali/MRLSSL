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
using System.Windows.Shapes;
using Microsoft.Research.DynamicDataDisplay;

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for ChartItemSettingWindow.xaml
    /// </summary>
    public partial class ChartItemSettingWindow : Window
    {
        public ChartItemSettingWindow()
        {
            InitializeComponent();
        }
        public LineAndMarker<ElementMarkerPointsGraph> Graph = new LineAndMarker<ElementMarkerPointsGraph>();
        public bool IsOk;
        private void graphcolorselectButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                graphcolorBorder.Background = new SolidColorBrush(Color.FromRgb(cd.Color.R, cd.Color.G, cd.Color.B));
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            IsOk = true;
           
            this.Close();
        }
    }
}
