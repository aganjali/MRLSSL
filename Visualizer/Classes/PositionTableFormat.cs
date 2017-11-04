using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;


namespace Visualizer.Classes
{
    public class PositionTableFormat
    {
        public string Name { get; set; }
        public TextBox X { get; set; }
        public TextBox Y { get; set; }
        public PositionTableFormat()
        {
            X = new TextBox();
            Y = new TextBox();
        }

    }
}
