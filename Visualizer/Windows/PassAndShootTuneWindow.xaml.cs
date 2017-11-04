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
using MRL.SSL.Visualizer.Classes;
using MRL.SSL.GameDefinitions;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for PassAndSHootTune.xaml
    /// </summary>
    public partial class PassAndSHootTuneWindow : Window
    {
        public PassAndSHootTuneWindow()
        {
            InitializeComponent();

        }

        private void PasserDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {


            if (PasserDistance != null)
                PassShootParameter.shooterDistance = PasserDistance.Value;
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void ShootSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (ShootSpeed != null)
                PassShootParameter.shootSpeed = ShootSpeed.Value;
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void PassSpeed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ShootSpeed != null)
                PassShootParameter.passSpeed = PassSpeed.Value;
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }
        private void SendData()
        {
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void button1_Click(object sender, RoutedEventArgs e) //Halt
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            PassShootParameter.AcceptData = false;
            DataSender.CurrentWrapper.RefreeCommand = "H";
            DataSender.SendOn.Set();
        }

        private void button2_Click(object sender, RoutedEventArgs e)//positioning
        {
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");



            PassShootParameter.start = false;
            PassShootParameter.test = false;
            PassShootParameter.AcceptData = false;
            DataSender.CurrentWrapper.RefreeCommand = "X";
            DataSender.SendOn.Set();
        }

        private void button3_Click(object sender, RoutedEventArgs e)//start
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            DataSender.CurrentWrapper.RefreeCommand = "X";
            PassShootParameter.start = true;
            PassShootParameter.test = false;
            PassShootParameter.AcceptData = false;
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)//passerID
        {
            if (textBox1 != null)
                PassShootParameter.PasserID = int.Parse(textBox1.Text);
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)//shooterID
        {
            if (textBox2 != null)
                PassShootParameter.ShooterID = int.Parse(textBox2.Text);
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            DataSender.CurrentWrapper.RefreeCommand = "X";
            PassShootParameter.start = false;
            PassShootParameter.test = true;
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void passerYDistance_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PasserDistance != null)
                PassShootParameter.YDistance = passerYDistance.Value;
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void button5_Click(object sender, RoutedEventArgs e)//Accept
        {
            PassShootParameter.AcceptData = true;
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void button6_Click(object sender, RoutedEventArgs e)
        {
            PassShootParameter.clear = true;
            DataSender.CurrentWrapper.SendData.Add("PassShootTuneTool");
            DataSender.SendOn.Set();
        }

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void SpinBack_Unchecked(object sender, RoutedEventArgs e)
        {

        }





    }
}
