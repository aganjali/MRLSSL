using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using MRL.SSL.CommonClasses.MathLibrary;

namespace MRL.SSL.GameDefinitions
{
    public class ChartObject
    {
        /// <summary>
        /// Color Of The Graph
        /// </summary>
        Color _graphColor;
        public Color GraphColor
        {
            get { return _graphColor; }
            set { _graphColor = value; }
        }
        /// <summary>
        /// if be true Data is double else is position2D
        /// </summary>
        public bool HasX;
        /// <summary>
        /// name of the collection
        /// </summary>
        public string CollectionName { get; set; }
        /// <summary>
        /// if be true logged in visualizer
        /// </summary>
        public bool Log { get; set; }
        /// <summary> 
        /// if be true show in charter
        /// </summary>
        public bool ShowInChart { get; set; }
        /// <summary>
        /// Double List for the Charting
        /// </summary>
        private List<Position2D> _chartData;
        public List<Position2D> ChartData
        {
            get { return _chartData; }
            set
            {
                if (_chartData == null)
                    _chartData = new List<Position2D>();
                _chartData = value;
            }
        }
        /// <summary>
        /// Construct class with out any parameter
        /// </summary>
        public ChartObject()
        {
            _chartData = new List<Position2D>();
        }
        /// <summary>
        /// Construct class with name
        /// create a list of Position2D
        /// </summary>
        /// <param name="name">a System.Drawing.Color</param>
        public ChartObject(string name)
        {
            CollectionName = name;
            _chartData = new List<Position2D>();
        }
        /// <summary>
        /// Construct class with name and DataList
        /// create a list of Position2D from DataList
        /// </summary>
        /// <param name="name">name of chart collection</param>
        /// <param name="DataList">list of position2D</param>
        public ChartObject(string name,List<Position2D> DataList)
        {
            CollectionName = name;
            _chartData = new List<Position2D>();
            _chartData = DataList;
        }
        /// <summary>
        /// Construct class with name and DataList
        /// </summary>
        /// <param name="name">name of chart collection</param>
        /// <param name="DataList">list of position2D</param>
        /// <param name="hasX">if be true data is Position2D else is double</param>
        /// <param name="log">if be true data will log in visualizer</param>
        public ChartObject(string name, List<Position2D> DataList,bool log)
        {
            CollectionName = name;
            _chartData = new List<Position2D>();
            _chartData = DataList;
            Log = log;
        }
        /// <summary>
        /// Construct class with name and DataList
        /// </summary>
        /// <param name="name">name of chart collection</param>
        /// <param name="DataList">list of position2D</param>
        /// <param name="hasX">if be true data is Position2D else is double</param>
        /// <param name="log">if be true data will log in visualizer</param>
        /// <param name="showInChart">if be true data will show in chrter</param>
        public ChartObject(string name, List<Position2D> DataList, bool log, bool showInChart)
        {
            CollectionName = name;
            _chartData = new List<Position2D>();
            _chartData = DataList;
            Log = log;
            ShowInChart = showInChart;
        }
        /// <summary>
        /// Adding position2D data to chart data 
        /// </summary>
        /// <param name="val"></param>
        public void AddData(Position2D val)
        {
            _chartData.Add(val);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void AddData(double val)
        {
            _chartData.Add(new Position2D(0, val));
            HasX = false;
        }
        /// <summary>
        /// Removing data from chart data
        /// </summary>
        /// <param name="val">Remove object</param>
        public void RemoveData(Position2D val)
        {
            _chartData.Remove(val);
        }

    }
}
