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
using MRL.SSL.GameDefinitions;
using System.Reflection;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for ExtractLogData.xaml
    /// </summary>
    public partial class ExtractLogData : Window
    {
        public ExtractLogData()
        {
            InitializeComponent();
            TreeViewItem item = new TreeViewItem();
            fill(item, typeof(AiToVisualizerWrapper));
        }

        void fill(TreeViewItem index, Type classType)
        {
            PropertyInfo[] a_pi = classType.GetProperties();
            foreach (PropertyInfo pi in a_pi)
            {
                if (pi.Module.ToString() == "GameDefinitions.dll" || pi.Module.ToString() == "CommonCLassess.dll")
                {
                    if (pi.GetType().GetProperties().Count() == 0)
                    {
                        index.Items.Add(pi.ToString().Split()[1].ToString());
                    }
                    else
                        fill(new TreeViewItem() { Name = pi.ToString().Split()[1].ToString() }, pi.GetType());
                }
            }
            maintree.Items.Add(index.Name.ToString());
        }
    }
}
