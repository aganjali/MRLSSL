using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.Extentions;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using MRL.SSL.CommonClasses.MathLibrary;
namespace MRL.SSL.GameDefinitions
{
    public class ChartItem
    {
        public float Opacity { get; set; }
        public ChartItem(string name, System.Drawing.Color color, double borderSize, XType xvalueType)
        {
            Color = color;
            BorderWhidth = borderSize;
            Xtype = xvalueType;
            Name = name;
        }
        public ChartItem(ChartItem chartItem)
        {
            Color = chartItem.Color;
            BorderWhidth = chartItem.BorderWhidth;
            Xtype = chartItem.Xtype;
            Name = chartItem.Name;
        }
        public ChartItem()
        {

        }
        public List<ChartDataInfo> Points = new List<ChartDataInfo>();

        public System.Drawing.Color Color = System.Drawing.Color.FromArgb(new Random().Next(0, 255), new Random().Next(0, 255), new Random().Next(0, 255));
        public double BorderWhidth = 1;
        public string Name { get; set; }
        public XType Xtype = XType.Fram;
        public enum XType
        {
            Time,
            Fram,
        }
    }

    public static class CharterData
    {
        #region lastcharter
        //public static List<ChartArea> ChartAreas { get; set; }

        //public static List<Series> series { get; set; }

        //public static void AddChartArea(string Name, bool is3D)
        //{
        //    if (ChartAreas == null) ChartAreas = new List<ChartArea>();
        //    if (ChartAreas.Any(a => a.Name == Name)) return;
        //    ChartArea ch = new ChartArea();

        //    ch.Name = Name;
        //    ch.Area3DStyle.Enable3D = is3D;
        //    ChartAreas.Add(ch);
        //}

        //public static void AddSerie(string Name,string chartArea)
        //{
        //    if (series == null) series=new List<Series>();
        //    if (series.Any(a => a.Name == Name)) return;
        //    series.Add(new Series(Name) { ChartArea = chartArea});
        //}

        //public static void AddSerie(string Name, string chartArea,SeriesChartType type)
        //{
        //    if (series == null) series = new List<Series>();
        //    if (series.Any(a => a.Name == Name)) return;

        //    series.Add(new Series(Name) { ChartArea = chartArea, ChartType = type, ToolTip = "Value:#VALY" });
        //}

        //public static void AddSerie(string Name, string chartArea, SeriesChartType type,bool hasMarker)
        //{
        //    if (series == null) series = new List<Series>();
        //    if (series.Any(a => a.Name == Name)) return;
        //    Series s = new Series(Name) { ChartArea = chartArea, ChartType = type };
        //    if (hasMarker)
        //    {
        //        s.MarkerBorderColor = System.Drawing.Color.Black;
        //        s.MarkerColor = System.Drawing.Color.White;
        //        s.MarkerBorderWidth = 1;
        //        s.MarkerSize = 5;
        //        s.MarkerStyle = MarkerStyle.Circle;
        //        s.ToolTip = "value : #VALY"; //string.Format("{0}\nfram: #VALX\nvalue : #VALY", s.Name);
        //    }
        //    series.Add(s);

        //}

        //public static void AddSerie(Series serie)
        //{
        //    if (series == null) series = new List<Series>();
        //    if (series.Any(a => a.Name == serie.Name)) return;
        //    series.Add(serie);
        //}

        //public static void AddData(string seriseName, double data)
        //{
        //    if (series==null || !series.Any(a => a.Name == seriseName)) return;
        //    series.Single(s => s.Name == seriseName).Points.AddY(data);
        //}

        //public static void AddData(double data)
        //{
        //    if (ChartAreas == null)
        //    {
        //        ChartAreas = new List<ChartArea>();
        //        AddChartArea("chartarea1", false);
        //    }
        //    else if (series == null && ChartAreas == null)
        //    {
        //        series = new List<Series>();
        //        ChartAreas = new List<ChartArea>();
        //        AddChartArea("chartarea1", false);
        //        AddSerie("series1", "chartarea1");
        //    }
        //    else if (series == null)
        //    {
        //        series = new List<Series>();
        //        AddSerie("series1", ChartAreas[0].Name);
        //    }
        //    AddData("series1", data);
        //}

        //public static void AddSerie(string Name, string chartArea, SeriesChartType type, System.Drawing.Color color, bool hasMarker)
        //{
        //    if (series == null) series = new List<Series>();
        //    if (series.Any(a => a.Name == Name)) return;
        //    Series s = new Series(Name) { ChartArea = chartArea, ChartType = type };
        //    s.Color = color;
        //    if (hasMarker)
        //    {

        //        s.MarkerBorderColor = System.Drawing.Color.Black;
        //        s.MarkerColor = System.Drawing.Color.White;
        //        s.MarkerBorderWidth = 1;
        //        s.MarkerSize = 5;
        //        s.MarkerStyle = MarkerStyle.Circle;
        //        s.ToolTip = "value : #VALY";//string.Format("{0}\nvalue : #VALY", s.Name);
        //    }
        //    series.Add(s);
        //}
        #endregion
        public static Dictionary<string, ChartItem> Series = new Dictionary<string, ChartItem>();

        public static void AddSeries(string Name, System.Drawing.Color color, double borderWidth, ChartItem.XType xVlueType)
        {
            lock (Series)
            {
                if (!Series.ContainsKey(Name))
                    Series.Add(Name, new ChartItem(Name, color, borderWidth, xVlueType));
            }
        }

        public static void AddSeries(string Name, System.Drawing.Color color, double borderWidth, ChartItem.XType xVlueType, List<ChartDataInfo> Data)
        {
            if (!Series.ContainsKey(Name))
                Series.Add(Name, new ChartItem(Name, color, borderWidth, xVlueType) { Points = Data });
        }

        public static void AddSeries(string Name, ChartItem chartItem)
        {
            if (!Series.ContainsKey(Name))
                Series.Add(Name, chartItem);
        }

        public static void AddData(string serieseName, double data)
        {
            if (Series.ContainsKey(serieseName))
                Series[serieseName].Points.Add(new ChartDataInfo() { Time = 0, Value = data });
            else
            {
                Series.Add(serieseName, new ChartItem() { Name = serieseName, Color = System.Drawing.Color.Black, BorderWhidth = 2 });
                Series[serieseName].Points.Add(new ChartDataInfo() { Time = 0, Value = data });

            }
        }
        public static void AddData(string serieseName, System.Drawing.Color color, double data)
        {
            if (Series.ContainsKey(serieseName))
                Series[serieseName].Points.Add(new ChartDataInfo() { Time = 0, Value = data });
            else
            {
                Series.Add(serieseName, new ChartItem() { Name = serieseName, Color = System.Drawing.Color.Black, BorderWhidth = 2 });
                Series[serieseName].Points.Add(new ChartDataInfo() { Time = 0, Value = data });

            }
            Series[serieseName].Color = color;
        }

    }

    public class LogCharterData
    {
        public LogCharterData()
        {
        }

        public Dictionary<string, ChartItem> Series = new Dictionary<string, ChartItem>();

        public void AddSeries(string Name, System.Drawing.Color color, double borderWidth, ChartItem.XType xVlueType)
        {
            if (!Series.ContainsKey(Name))
                Series.Add(Name, new ChartItem(Name, color, borderWidth, xVlueType));
        }

        public void AddSeries(string Name, System.Drawing.Color color, double borderWidth, ChartItem.XType xVlueType, List<ChartDataInfo> Data)
        {
            if (!Series.ContainsKey(Name))
                Series.Add(Name, new ChartItem(Name, color, borderWidth, xVlueType) { Points = Data });
        }

        public void AddSeries(string Name, ChartItem chartItem)
        {
            if (!Series.ContainsKey(Name))
                Series.Add(Name, chartItem);
        }

        public void AddData(string serieseName, double data)
        {
            if (Series.ContainsKey(serieseName))
                Series[serieseName].Points.Add(new ChartDataInfo() { Time = 0, Value = data });
            else
            {
                Series.Add(serieseName, new ChartItem() { Name = serieseName, Color = System.Drawing.Color.Black, BorderWhidth = 2 });
                Series[serieseName].Points.Add(new ChartDataInfo() { Time = 0, Value = data });

            }
        }

    }


}
