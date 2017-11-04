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
using MRL.SSL.GameDefinitions;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for AminChartWindow.xaml
    /// </summary>
    public partial class AminChartWindow : Window
    {
        public double[] ret;

        PerformanceData data, markers;

        public AminChartWindow()
        {
            InitializeComponent();

            data = new PerformanceData();
            markers = new PerformanceData();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            List<ChartDataInfo> po = new List<ChartDataInfo>();
            int time = 0;
            ret.ToList().ForEach(p => po.Add(new ChartDataInfo() { Time = time++, Value = p }));
            ChartItem ci = new ChartItem();
            ci.Points = po;
            ci.Name = "amin";
            ci.Color = System.Drawing.Color.Black;
            ci.BorderWhidth = 1;
            CreatePerformanceGraph(ci);
        }

        private LineAndMarker<ElementMarkerPointsGraph> CreatePerformanceGraph(ChartItem category)
        {
            category.Points.ForEach(p => data.Add(p));
            data.Name = category.Name;
            markers.Name = category.Name;
            var ds = new EnumerableDataSource<ChartDataInfo>(data);
            ds.SetXMapping(pi => pi.Time);
            ds.SetYMapping(pi => pi.Value);
            ds.AddMapping(ShapeElementPointMarker.ToolTipTextProperty,
                Y => String.Format("Time : {0}\nValue : {1}", Y.Time, Y.Value.ToString()));

            LineAndMarker<ElementMarkerPointsGraph> chart = mainPlotter.AddLineGraph(ds,
                new Pen(new SolidColorBrush(Color.FromRgb(category.Color.R, category.Color.G, category.Color.B)), category.BorderWhidth),
                new CircleElementPointMarker
                {
                    Size = 5,
                    Brush = Brushes.Black,
                    Fill = Brushes.Wheat
                },
                new PenDescription(String.Format("{0}", category.Name)));

            chart.LineGraph.Name = category.Name;
            chart.LineGraph.Tag = "S";

            chart.MarkerGraph.DataSource = ds;
            return chart;

        }
    }
}
