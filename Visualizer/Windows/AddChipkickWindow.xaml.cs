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
    /// Interaction logic for AddChipkickWindow.xaml
    /// </summary>
    public partial class AddChipkickWindow : Window
    {
        public AddChipkickWindow()
        {
            InitializeComponent();
        }
        public int Power { get; set; }
        public double Lenght { get; set; }
        public double Safe { get; set; }
        public int RobotID { get; set; }
        public bool MustBeAdd { get; set; }
        public bool HasSpin { get; set; }
        public double Time { get; set; }
        public bool BackSensore { get; set; }
        private void addbutton_Click(object sender, RoutedEventArgs e)
        {
            RobotID = (int)robotidNum.Value;
            Power = (int)powerTextBox.Value;
            Lenght = (double)lenghtTextBox.Value;
            Safe = (double)safeTextBox.Value;
            HasSpin = hasspineCheckBox.IsChecked.Value;
            BackSensore = backsenseCheckBox.IsChecked.Value;
            Time = (double)timeUpDown.Value;
            MustBeAdd = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            MustBeAdd = false;
            this.Close();
        }

        private void backsenseCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
