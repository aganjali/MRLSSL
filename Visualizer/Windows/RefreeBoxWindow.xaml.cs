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

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for RefreeBoxWindow.xaml
    /// </summary>
    public partial class RefreeBoxWindow : Window
    {
        public RefreeBoxWindow()
        {
            InitializeComponent();
        }
        public bool ballPlacementBlue;
        public bool ballPlacementYellow;
        private void bluekikckoffButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "K";
            DataSender.SendOn.Set();
        }

        private void bluedirectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "F";
            DataSender.SendOn.Set();
        }

        private void blueindirectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");

            DataSender.CurrentWrapper.RefreeCommand = "I"; 
            DataSender.SendOn.Set();
        }

        private void bluepenaltyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "P";
            DataSender.SendOn.Set();
        }

        private void bluetimeoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "T";
            DataSender.SendOn.Set();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "S";
            DataSender.SendOn.Set();
        }

        private void normalstartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = " ";
            DataSender.SendOn.Set();
        }

        private void forcestartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "s";
            DataSender.SendOn.Set();
        }

        private void comhereButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "v";
            DataSender.SendOn.Set();
        }

        private void haltButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "H";
            DataSender.SendOn.Set();
        }

        private void yellowkickoffButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "k";
            DataSender.SendOn.Set();
        }

        private void yellowdirectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "f";
            DataSender.SendOn.Set();
        }

        private void yellowindirectbutton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "i";
            DataSender.SendOn.Set();
        }

        private void yellowpenaltyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "p";
            DataSender.SendOn.Set();
        }

        private void yellowtimeoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "t";
            DataSender.SendOn.Set();
        }

        private void robotcontrollerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "a";
            DataSender.SendOn.Set();
        }

        private void moveRobotButton_Click(object sender, RoutedEventArgs e)
        {

            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "m";
            DataSender.SendOn.Set();
        }

        private void radButton1_Click(object sender, RoutedEventArgs e)
        {
            if (!DataSender.CurrentWrapper.SendData.Contains("RefreeCommand"))
                DataSender.CurrentWrapper.SendData.Add("RefreeCommand");
            
            DataSender.CurrentWrapper.RefreeCommand = "z";
            DataSender.SendOn.Set();
        }

        private void blueBallPlacement_Click(object sender, RoutedEventArgs e)
        {
            ballPlacementBlue = true;
            MessageBox.Show("Click on ball target on the field. ");
        }

        private void YellowBallPlacement_Click(object sender, RoutedEventArgs e)
        {
            ballPlacementYellow = true;
            MessageBox.Show("Click on ball target on the field. ");
        }
    }
}
