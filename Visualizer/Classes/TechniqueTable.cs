using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Visualizer.Classes
{
    public class TechniqueTable
    {
        public TextBox Feasibility { get; set; }
        public string MainName { get; set; }
        public string Name { get; set; }
        public CheckBox Enabled { get; set; }
        public float Opacity { get; set; }
        public TechniqueTable()
        {
            MainName = "";
            Name = "";
            Feasibility = new TextBox();
            Enabled = new CheckBox();
            Opacity = 1;
        }
    }
}
