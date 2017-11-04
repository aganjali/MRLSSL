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
using Visualizer.Classes;

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for MotorCurrentChartWindow.xaml
    /// </summary>
    public partial class MotorCurrentChartWindow : Window
    {
        List<PerformanceData> data;
        List<LineAndMarker<ElementMarkerPointsGraph>>
           linegraphsM1,
           linegraphsM2,
           linegraphsM3,
           linegraphsM4,
           linegraphsSpin;
        bool ShowChart = false;
        public MotorCurrentChartWindow()
        {
            InitializeComponent();

            data = new List<PerformanceData>();

            linegraphsM1 = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            linegraphsM2 = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            linegraphsM3 = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            linegraphsM4 = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            linegraphsSpin = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            
            linegraphsM1.Add(CreatePerformanceGraph(motor1Plotter, new ChartItem() { Name = "Current" }, Brushes.Blue, "Current.1"));
            linegraphsM2.Add(CreatePerformanceGraph(motor2Plotter, new ChartItem() { Name = "Current" }, Brushes.Blue, "Current.2"));
            linegraphsM3.Add(CreatePerformanceGraph(motor3Plotter, new ChartItem() { Name = "Current" }, Brushes.Blue, "Current.3"));
            linegraphsM4.Add(CreatePerformanceGraph(motor4Plotter, new ChartItem() { Name = "Current" }, Brushes.Blue, "Current.4"));
            linegraphsSpin.Add(CreatePerformanceGraph(spinPlotter, new ChartItem() { Name = "Current" }, Brushes.YellowGreen, "Current.Spin"));
            WirelessReciever.MotorCurrentRecieved += new WirelessReciever.RecievedCurrent(WirelessReciever_MotorCurrentRecieved);
        }

        void WirelessReciever_MotorCurrentRecieved(MotorCurrentData sender)
        {
            if (true)
            {
                int ms = (int)DateTime.Now.TimeOfDay.TotalMilliseconds;

                PerformanceData m1 = data.Single(s => s.Name == "Current.1");
                PerformanceData m2 = data.Single(s => s.Name == "Current.2");
                PerformanceData m3 = data.Single(s => s.Name == "Current.3");
                PerformanceData m4 = data.Single(s => s.Name == "Current.4");
                PerformanceData mSpin = data.Single(s => s.Name == "Current.Spin");
                Dispatcher.Invoke((Action)(() =>
                {
                    m1.Add(new ChartDataInfo() { Value = sender.Motor1, Time = ms });
                    m2.Add(new ChartDataInfo() { Value = sender.Motor2, Time = ms });
                    m3.Add(new ChartDataInfo() { Value = sender.Motor3, Time = ms });
                    m4.Add(new ChartDataInfo() { Value = sender.Motor4, Time = ms });
                    mSpin.Add(new ChartDataInfo() { Value = sender.Motor5, Time = ms });
                }));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private LineAndMarker<ElementMarkerPointsGraph> CreatePerformanceGraph(ChartPlotter chartplot, ChartItem category, Brush br, string key)
        {
            data.Add(new PerformanceData() { Name = key });
            var ds = new EnumerableDataSource<ChartDataInfo>(data.Last());
            ds.SetXMapping(pi => pi.Time);
            ds.SetYMapping(pi => pi.Value);
            ds.AddMapping(ShapeElementPointMarker.ToolTipTextProperty,
                Y => String.Format("Time : {0}\nValue : {1}", Y.Time, Y.Value.ToString("f3")));
            LineAndMarker<ElementMarkerPointsGraph> chart = chartplot.AddLineGraph(ds,
                new Pen(br, 2),
                new CircleElementPointMarker
                {
                    Size = 7,
                    Brush = Brushes.Black,
                    Fill = Brushes.Wheat
                },
                new PenDescription(String.Format("{0}-{1}", category.Name, key)));

            chart.LineGraph.Name = category.Name;
            chart.MarkerGraph.DataSource = null;
            return chart;

        }
    }
}
