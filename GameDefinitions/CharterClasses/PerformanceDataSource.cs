using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay.Common;
using System.Collections.Generic;

namespace MRL.SSL.GameDefinitions
{
	public class ChartDataInfo
	{
		public int Time { get; set; }
		public double Value { get; set; }
	}

    public class PerformanceData : RingArray<ChartDataInfo>
    {
        public string Name { get; set; }
        public PerformanceData()
            : base(200)
        {
            
        }

    }
}
