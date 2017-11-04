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

namespace MRL.SSL.Visualizer.UserControls
{
    /// <summary>
    /// Interaction logic for VertexUserControl.xaml
    /// </summary>
    public partial class VertexUserControl : UserControl
    {
        private string _nodeName = "";
        public string NodeName
        {
            set
            {
                nodenameTextBlock.Text = value;
                _nodeName = value;
            }
            get
            {
                return _nodeName;
            }
        }
        public bool Active
        {
            set
            {
                if (value)
                    node.Stroke = Brushes.Red;
                else
                    node.Stroke = new SolidColorBrush(Color.FromRgb(0, 137, 255));
            }
        }

        public VertexUserControl(string name, bool active)
        {
            InitializeComponent();
            NodeName = name;
            Active = active;
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void nodenameTextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainGrid.Width = nodenameTextBlock.ActualWidth + 15;
            mainGrid.Height = nodenameTextBlock.ActualWidth + 15;

            node.Width = nodenameTextBlock.ActualWidth + 15;
            node.Height = nodenameTextBlock.ActualWidth + 15;
        }

    }
}
