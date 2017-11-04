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

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for AddDirectkickWindow.xaml
    /// </summary>
    public partial class AddDirectkickWindow : Window
    {
        public AddDirectkickWindow()
        {
            InitializeComponent();
        }
        public int Power { get; set; }
        public double Speed { get; set; }
        public int RobotID { get; set; }
        public bool MustBeAdd { get; set; }
        private void addbutton_Click(object sender, RoutedEventArgs e)
        {
            RobotID = (int)robotidNum.Value;
            Power = (int)powerTextBox.Value;
            Speed = (double)speedTextBox.Value;
            MustBeAdd = true;
            this.Close();
        }
    }
}
