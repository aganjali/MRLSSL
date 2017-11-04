using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Visualizer.Classes
{
    public class BoolTableFormat
    {
        public CheckBox Value { get; set; }
        public string Name { get; set; }
        public BoolTableFormat()
        {
            Value = new CheckBox();
        }
    }
}
