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
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.Visualizer.Classes;

namespace Visualizer.UserControls
{
    /// <summary>
    /// Interaction logic for RobotViewerUserControl.xaml
    /// </summary>
    public partial class RobotViewerUserControl : UserControl
    {
        GradientBrush g1, g2, g3;

        public int BatteryHeart
        {
            set
            {
                batteryProgressBar.Value = value;
                if (value < 30)
                    batteryProgressBar.Foreground = g3;
                else if (value < 55)
                    batteryProgressBar.Foreground = g2;
                else
                    batteryProgressBar.Foreground = g1;
                batteryLabel.Content = value+"%";
            }
        }

        public bool Sensor
        {
            set
            {
                if (value)
                    sensorBorder.Background = Brushes.Red;
                else
                    sensorBorder.Background = Brushes.White;
            }
        }

        int _number;
        public int Number
        {
            set
            {
                _number = value;
                numberLabel.Content = value;
            }
            get
            {
                return _number;
            }
        }

        public bool Kick
        {
            set
            {
                if (value)
                    kickBorder.Background = Brushes.Red;
                else
                    kickBorder.Background = Brushes.Gray;
               
            }
        }

        public bool ChipKick
        {
            set
            {
                if (value)
                    chipBorder.Background = Brushes.Red;
                else
                    chipBorder.Background = Brushes.Gray;
               
            }
        }

        public RobotViewerUserControl()
        {
            InitializeComponent();
            
            g1 = new LinearGradientBrush(Colors.Green, Colors.YellowGreen, new Point(0, 0), new Point(1, 1));
            g2 = new LinearGradientBrush(Colors.Gold, Colors.Yellow, 10);
            g3 = new LinearGradientBrush(Colors.Maroon, Colors.Red, 10);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void namesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (namesComboBox.SelectedItem == null) return;
            valueTextBox.Text = ActiveRoleSettings.Default.Parameters[_number].propeties[(string)namesComboBox.SelectedItem].ToString();
        }

        private void valueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            double res;
            if (double.TryParse(valueTextBox.Text, out res) && namesComboBox.SelectedItem!=null)
            {
                ActiveRoleSettings.Default.Parameters[_number].propeties[(string)namesComboBox.SelectedItem] = res;
                ActiveRoleSettings.Default.Save();
                DataSender.CurrentWrapper.SendData.Add("ActiveSettings");
                DataSender.SendOn.Set();
            }
        }

        private void addSettings_Click(object sender, RoutedEventArgs e)
        {
            StateAddPanel(Visibility.Visible);
        }

        void StateAddPanel(Visibility v)
        {
            nameLabel.Visibility = v;
            ValueNum.Visibility = v;
            okButton.Visibility = v;
            cancelButton.Visibility = v;
            mainRect.Visibility = v;
            valueLabel.Visibility = v;
            nameTextBox.Visibility = v;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            StateAddPanel(Visibility.Hidden);
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            string name = nameTextBox.Text;
            double val = (double)ValueNum.Value;
            if (ActiveRoleSettings.Default.Parameters == null)
                ActiveRoleSettings.Default.Parameters = new MRL.SSL.CommonClasses.MathLibrary.SerializableDictionary<int, MRL.SSL.GameDefinitions.ActiveRoleSettingTemplate>();
            if (!ActiveRoleSettings.Default.Parameters.ContainsKey(_number))
                ActiveRoleSettings.Default.Parameters.Add(_number, new MRL.SSL.GameDefinitions.ActiveRoleSettingTemplate());
            ActiveRoleSettings.Default.Parameters[_number].propeties[name] = val;
            ActiveRoleSettings.Default.Save();
            DataSender.CurrentWrapper.SendData.Add("ActiveSettings");
            DataSender.SendOn.Set();
            StateAddPanel(Visibility.Hidden);
            Refresh();
        }

        private void Refresh()
        {
            if (ActiveRoleSettings.Default.Parameters != null && ActiveRoleSettings.Default.Parameters.ContainsKey(_number))
                namesComboBox.ItemsSource = ActiveRoleSettings.Default.Parameters[_number].propeties.Keys.ToList();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (namesComboBox.SelectedItem == null) return;
            if (MessageBox.Show("Are you sure?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) == MessageBoxResult.No)
                return;
            string key=namesComboBox.SelectedItem.ToString();
            if (ActiveRoleSettings.Default.Parameters[_number].propeties.ContainsKey(key))
                ActiveRoleSettings.Default.Parameters[_number].propeties.Remove(key);
            ActiveRoleSettings.Default.Save();
            DataSender.CurrentWrapper.SendData.Add("ActiveSettings");
            DataSender.SendOn.Set();
            Refresh();
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                StateAddPanel(Visibility.Hidden);
        }
    }
}
