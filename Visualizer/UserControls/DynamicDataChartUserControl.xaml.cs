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
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using MRL.SSL.Visualizer.Classes;
using Visualizer.Classes;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.CommonClasses.Extentions;
using System.Diagnostics;
using Visualizer.Windows;
using Microsoft.Research.DynamicDataDisplay.Common;



namespace Visualizer.UserControls
{
    /// <summary>
    /// Interaction logic for DynamicDataChartUserControl.xaml
    /// </summary>
    public partial class DynamicDataChartUserControl : UserControl
    {
        List<PerformanceData> data, data2, markers, markers2;
        List<LineAndMarker<ElementMarkerPointsGraph>> lineGraphs, lineGraphs2;
        public bool Log { get; set; }
        LogCharterData _currentChartData;
        public LogCharterData CurrentChartData
        {
            get
            {
                return _currentChartData;
            }
            set
            {
                _currentChartData = value;
                refreshChartData();
            }

        }
        public DynamicDataChartUserControl()
        {
            InitializeComponent();
            //stackpan.Children.Add(new ChartPlotter() { Width = stackpan.Width / stackpan.Children.Count });
            data = new List<PerformanceData>();
            data2 = new List<PerformanceData>();
            markers2 = new List<PerformanceData>();
            lineGraphs2 = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            markers = new List<PerformanceData>();
            lineGraphs = new List<LineAndMarker<ElementMarkerPointsGraph>>();
            CharterThread.ShowData += new CharterThread.ChartData(CharterThread_ShowData);
            GoogleSerializer.SerieChanged += new GoogleSerializer.SerieseChanged(GoogleSerializer_SerieChanged);
            CharterThread.InitialThread();
        }

        void GoogleSerializer_SerieChanged(List<string> newSeries)
        {
            Dispatcher.Invoke((Action)(() => serieChang(newSeries)));
        }

        void serieChang(List<string> newSeries)
        {
            if (newSeries.Count > 0)
                foreach (var item in CharterData.Series.ToList())
                    if (newSeries.Contains(item.Key))
                        lineGraphs.Add(CreatePerformanceGraph(new ChartItem(item.Value)));
            seriesLixtBox.ItemsSource = lineGraphs.ToList();
        }

        int j = 0;

        void CharterThread_ShowData(object sender)
        {
            if (data.Count > 0)
                Dispatcher.Invoke((Action)(() =>
                {
                    int count = data.Count;
                    int ms = (int)DateTime.Now.TimeOfDay.TotalMilliseconds;
                    for (int i = 0; i < count; i++)
                    {
                        if (seriesLixtBox.Items.Count > 0)
                        {
                            LineAndMarker<ElementMarkerPointsGraph> f = seriesLixtBox.Items.Cast<LineAndMarker<ElementMarkerPointsGraph>>().ToList().SingleOrDefault(s => s.LineGraph.Name == data[i].Name);
                            if (f.LineGraph.Tag.ToString() == "H")
                            {
                                data[i].Clear();
                                mainPlotter.InvalidateVisual();
                            }
                            else
                            {
                                ChartDataInfo newInfo;
                                if (!Log && DataReciever.ChartRun)
                                {
                                    if (CharterData.Series.Count > i)
                                    {
                                        newInfo = new ChartDataInfo { Time = ms, Value = CharterData.Series.ElementAt(i).Value.Points.Last().Value };
                                        data[i].Add(newInfo);
                                    }

                                }
                            }
                        }
                    }
                }));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void changstateButton_Checked(object sender, RoutedEventArgs e)
        {
            lineGraphs.ForEach(p =>
            {
                p.MarkerGraph.Marker = null;
            });
            CharterThread.ChangState();
        }

        private void changstateButton_Unchecked(object sender, RoutedEventArgs e)
        {
            lineGraphs.ForEach(p =>
            {
                p.MarkerGraph.DataSource = null;
                p.MarkerGraph.Marker = new CircleElementPointMarker
                {
                    Size = 5,
                    Brush = Brushes.Black,
                    Fill = Brushes.Wheat
                };
                p.MarkerGraph.DataSource = p.LineGraph.DataSource;
            });
            CharterThread.ChangState();
        }

        private LineAndMarker<ElementMarkerPointsGraph> CreatePerformanceGraph(ChartItem category)
        {
            data.Add(new PerformanceData() { Name = category.Name });
            markers.Add(new PerformanceData() { Name = category.Name });
            var ds = new EnumerableDataSource<ChartDataInfo>(data.Last());
            ds.SetXMapping(pi => pi.Time);
            ds.SetYMapping(pi => pi.Value);
            ds.AddMapping(ShapeElementPointMarker.ToolTipTextProperty,
                Y => String.Format("Time : {0}\nValue : {1}", Y.Time, Y.Value.ToString("f3")));



            LineAndMarker<ElementMarkerPointsGraph> chart = mainPlotter.AddLineGraph(ds,
                new Pen(new SolidColorBrush(Color.FromRgb(category.Color.R, category.Color.G, category.Color.B)), category.BorderWhidth),
                new CircleElementPointMarker
                {
                    Size = 2,
                    Brush = Brushes.Black,
                    Fill = Brushes.Wheat
                },
                new PenDescription(String.Format("{0}", category.Name)));

            chart.LineGraph.Name = category.Name;
            chart.LineGraph.Tag = "S";
            chart.MarkerGraph.DataSource = null;
            return chart;

        }

        private LineAndMarker<ElementMarkerPointsGraph> LogCreatePerformanceGraph(ChartItem category)
        {
            data2.Add(new PerformanceData() { Name = category.Name });
            markers2.Add(new PerformanceData() { Name = category.Name });
            var ds = new EnumerableDataSource<ChartDataInfo>(data2.Last());
            ds.SetXMapping(pi => pi.Time);
            ds.SetYMapping(pi => pi.Value);
            ds.AddMapping(ShapeElementPointMarker.ToolTipTextProperty,
                Y => String.Format("Time : {0}\nValue : {1}", Y.Time, Y.Value.ToString("f3")));



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
            chart.MarkerGraph.DataSource = null;
            return chart;

        }

        private void changecolorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (seriesLixtBox.SelectedItem == null) return;
            //System.Windows.Forms.ColorDialog cd=new System.Windows.Forms.ColorDialog();
            //if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            ChartItemSettingWindow w = new ChartItemSettingWindow();
            w.graphcolorBorder.Background = seriesLixtBox.SelectedItem.As<LineAndMarker<ElementMarkerPointsGraph>>().LineGraph.LinePen.Brush;
            w.nameTextBox.Text = seriesLixtBox.SelectedItem.As<LineAndMarker<ElementMarkerPointsGraph>>().LineGraph.Name;
            w.ShowDialog();
            if (w.IsOk)
            {
                //seriesLixtBox.SelectedItem.As<LineAndMarker<ElementMarkerPointsGraph>>().LineGraph.LinePen.Brush = new SolidColorBrush(Color.FromRgb(cd.Color.R, cd.Color.G, cd.Color.B));
                seriesLixtBox.SelectedItem.As<LineAndMarker<ElementMarkerPointsGraph>>().LineGraph.LinePen.Brush = w.graphcolorBorder.Background;
                seriesLixtBox.SelectedItem.As<LineAndMarker<ElementMarkerPointsGraph>>().LineGraph.Name = w.nameTextBox.Text.Replace(" ", "_");
                seriesLixtBox.SelectedItem.As<LineAndMarker<ElementMarkerPointsGraph>>().LineGraph.Description = new PenDescription(w.nameTextBox.Text);
            }
        }

        private void changeShown_Click(object sender, RoutedEventArgs e)
        {
            if (seriesLixtBox.SelectedItem == null) return;
            LineAndMarker<ElementMarkerPointsGraph> ci = seriesLixtBox.SelectedItem.As<LineAndMarker<ElementMarkerPointsGraph>>();
            if (ci.LineGraph.Tag.ToString() == "S")
            {
               // changeShown.Header = "Show";
                ci.LineGraph.Tag = "H";
            }
            else
            {
                //changeShown.Header = "Hide";
                ci.LineGraph.Tag = "S";
            }
        }

        private void refreshChartData()
        {
            //data2.ForEach(p => p.Clear());

            foreach (var item in _currentChartData.Series.ToList())
                if (_currentChartData.Series.ContainsKey(item.Key))
                {
                    if (seriesLixtBox.Items.Cast<LineAndMarker<ElementMarkerPointsGraph>>().ToList().SingleOrDefault(s => s.LineGraph.Name == item.Key) == null)
                        lineGraphs2.Add(LogCreatePerformanceGraph(new ChartItem(item.Value)));

                }
            seriesLixtBox.ItemsSource = lineGraphs2.ToList();
            if (data2.Count > 0)
                Dispatcher.Invoke((Action)(() =>
                {
                    int count = data2.Count;
                    int ms = (int)DateTime.Now.TimeOfDay.TotalMilliseconds;

                    for (int i = 0; i < count; i++)
                    {
                        if (seriesLixtBox.Items.Count > 0)
                        {
                            LineAndMarker<ElementMarkerPointsGraph> f = seriesLixtBox.Items.Cast<LineAndMarker<ElementMarkerPointsGraph>>().ToList().SingleOrDefault(s => s.LineGraph.Name == data2[i].Name);
                            if (f.LineGraph.Tag.ToString() == "H")
                            {
                                data2[i].Clear();
                                mainPlotter.InvalidateVisual();
                            }
                            else
                            {
                                ChartDataInfo newInfo;
                                newInfo = new ChartDataInfo { Time = ms, Value = _currentChartData.Series.ElementAt(i).Value.Points.Last().Value };
                                data2[i].Add(newInfo);
                            }
                        }
                    }
                }));
        }
        bool rightDown = false;
        private void seriesLixtBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released)// && rightDown)
            {
                var selected = seriesLixtBox.SelectedItem;

                rightDown = false;
                if (selected != null)
                {
                    var ci = selected.As<LineAndMarker<ElementMarkerPointsGraph>>();
                    ContextMenu m = new ContextMenu();
                    MenuItem settings = new MenuItem();
                    settings.Header = "Settings";
                    settings.Click += changecolorMenuItem_Click;

                    MenuItem showHide = new MenuItem();
                    showHide.Header = (ci.LineGraph.Tag.ToString() == "S") ? "Hide" : "Show";
                    showHide.Click += (s, ee) =>
                        {
                            var selct = seriesLixtBox.SelectedItem;
                            if (selct == null) return;
                            var cj = selct.As<LineAndMarker<ElementMarkerPointsGraph>>();
                            if (cj.LineGraph.Tag.ToString() == "S")
                                cj.LineGraph.Tag = "H";
                            else
                                cj.LineGraph.Tag = "S";
                        };
                    m.Items.Add(settings);
                    m.Items.Add(showHide);
                    seriesLixtBox.ContextMenu = m;
                }

            }
        }

        private void seriesLixtBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //if (e.RightButton == MouseButtonState.Pressed)
            //    rightDown = true;
        }
    }
}
