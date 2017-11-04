using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Visualizer.Classes
{
    public class DoubleTableFormat
    {
        public TextBox Value { get; set; }
        public string Name { get; set; }
        public float Opacity { get; set; }
        public DoubleTableFormat()
        {
            Value = new TextBox();
        }
    }
}
