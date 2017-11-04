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
using MRL.SSL.Visualizer.Extentions;
using Enterprise.Wpf;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Visualizer.Classes;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for RotateParametersWindow.xaml
    /// </summary>
    public partial class RotateParametersWindow : Window
    {
        public RotateParametersWindow()
        {
            InitializeComponent();
            RotateParameters.Load();
            Refresh();
            init = true;
        }
        public static bool RotateTune = false;
        bool init = false;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            RobotPic.SetImageSource("robot3A (3).png");
            RotateParameters.RoboID = 0;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (4).png");
            RotateParameters.RoboID = 1;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (2).png");
            RotateParameters.RoboID = 2;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (1).png");
            RotateParameters.RoboID = 3;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (5).png");
            RotateParameters.RoboID = 4;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (6).png");
            RotateParameters.RoboID = 5;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (7).png");
            RotateParameters.RoboID = 6;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (8).png");
            RotateParameters.RoboID = 7;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (9).png");
            RotateParameters.RoboID = 8;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (10).png");
            RotateParameters.RoboID = 9;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {

            RobotPic.SetImageSource("robot3A (11).png");
            RotateParameters.RoboID = 10;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void Button_Click_12(object sender, RoutedEventArgs e)
        {
            RobotPic.SetImageSource("robot3A (12).png");
            RotateParameters.RoboID = 11;
            Refresh(RotateParameters.RoboID);
            SendData();
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            RotateParameters.VyValues[RotateParameters.RoboID][RotateParameters.angle] = RotateParameters.Vycoeff;
            RotateParameters.OmegaValues[RotateParameters.RoboID][RotateParameters.angle] = RotateParameters.Omegacoeff;
            Refresh(RotateParameters.RoboID);
            RotateParameters.Save(); 
            SendData();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            RotateParameters.Save();
            SendData();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            RotateParameters.Load();
            Refresh();
            SendData();
        }

        private void RotateEnable_Checked(object sender, RoutedEventArgs e)
        {
            RotateParameters.TuneFlag = true;
            Save.IsEnabled = true;
            Load.IsEnabled = true;
            add.IsEnabled = true;
            Button4.IsEnabled = true;
            Rbtbtn10.IsEnabled = true;
            Rbtbtn11.IsEnabled = true;
            Rbtbtn12.IsEnabled = true;
            Rbtbtn13.IsEnabled = true;
            Rbtbtn14.IsEnabled = true;
            Rbtbtn3.IsEnabled = true;
            Rbtbtn8.IsEnabled = true;
            Rbtbtn5.IsEnabled = true;
            Rbtbtn6.IsEnabled = true;
            Rbtbtn7.IsEnabled = true;
            Rbtbtn9.IsEnabled = true;
           

        }


        private void Refresh(int robotID)
        {
            if (RotateParameters.VyValues[robotID].ContainsKey(30))
            {
                v30.Content = RotateParameters.VyValues[robotID][30].ToString();
                o30.Content = RotateParameters.OmegaValues[robotID][30].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(40))
            {
                v40.Content = RotateParameters.VyValues[robotID][40].ToString();
                o40.Content = RotateParameters.OmegaValues[robotID][40].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(50))
            {
                v50.Content = RotateParameters.VyValues[robotID][50].ToString();
                o50.Content = RotateParameters.OmegaValues[robotID][50].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(60))
            {
                v60.Content = RotateParameters.VyValues[robotID][60].ToString();
                o60.Content = RotateParameters.OmegaValues[robotID][60].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(70))
            {
                v70.Content = RotateParameters.VyValues[robotID][70].ToString();
                O70.Content = RotateParameters.OmegaValues[robotID][70].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(80))
            {
                v80.Content = RotateParameters.VyValues[robotID][80].ToString();
                o80.Content = RotateParameters.OmegaValues[robotID][80].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(90))
            {
                v90.Content = RotateParameters.VyValues[robotID][90].ToString();
                o90.Content = RotateParameters.OmegaValues[robotID][90].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(100))
            {
                v100.Content = RotateParameters.VyValues[robotID][100].ToString();
                o100.Content = RotateParameters.OmegaValues[robotID][100].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(110))
            {
                v110.Content = RotateParameters.VyValues[robotID][110].ToString();
                o110.Content = RotateParameters.OmegaValues[robotID][110].ToString();
            }
            if (RotateParameters.VyValues[robotID].ContainsKey(120))
            {
                v120.Content = RotateParameters.VyValues[robotID][120].ToString();
                o120.Content = RotateParameters.OmegaValues[robotID][120].ToString();
            }
        }
        private void Refresh()
        {
            for (int i = 0; i < 12; i++)
            {
                Refresh(i);
            }
            RotateAngle.Text = RotateParameters.angle.ToString();
            omegacoef.Text = RotateParameters.Omegacoeff.ToString();
            vycoef.Text = RotateParameters.Vycoeff.ToString();

            
        }
        private void SendData()
        {
            //if (RotateEnable.IsChecked.Value)
            DataSender.CurrentWrapper.SendData.Add("RotateSetting");
            DataSender.SendOn.Set();
        }

        private void RotateAngle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            int tmp;
            if (int.TryParse(RotateAngle.Text, out tmp))
            {
                if (tmp <= 30)
                    tmp = 30;
                else if (tmp <= 40)
                    tmp = 40;
                else if (tmp <= 50)
                    tmp = 50;
                else if (tmp <= 60)
                    tmp = 60;
                else if (tmp <= 70)
                    tmp = 70;
                else if (tmp <= 80)
                    tmp = 80;
                else if (tmp <= 90)
                    tmp = 90;
                else if (tmp <=100)
                    tmp = 100;
                else if (tmp <= 110)
                    tmp = 110;
                else if (tmp <= 120)
                    tmp = 120;
                else if (tmp >=150)
                    tmp = 180;
                
                RotateParameters.angle = tmp;
                SendData();
            }
        }

        private void omegacoef_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            double tmp;
            if (double.TryParse(omegacoef.Text, out tmp))
            {
                RotateParameters.Omegacoeff = tmp;
                SendData();
            }
        }

        private void vycoef_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!init || sender.As<TextBox>().Text == "") return;
            double tmp;
            if (double.TryParse(vycoef.Text, out tmp))
            {
                RotateParameters.Vycoeff = tmp;
                SendData();
            }
        }
    }
}
