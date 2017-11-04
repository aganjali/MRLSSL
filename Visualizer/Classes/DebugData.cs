using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.DynamicDataDisplay.Common;

namespace Visualizer.Classes
{
    public class DebugData
    {
        public DateTime Time { get; set; }
        public Double Value { get; set; }
    }
    public class DebagDataArray : RingArray<DebugData>
    {
        public DebagDataArray()
            : base(100)
        {
            //MRL.SSL.GameDefinitions.settings
            
        }
    } 
}
