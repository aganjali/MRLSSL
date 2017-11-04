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
using MRL.SSL.Visualizer.Extentions;
using MRL.SSL.Visualizer.Classes;
using Enterprise.Wpf;


namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for ControlParameteresWindow.xaml
    /// </summary>
    public partial class ControlParameteresWindow : Window
    {
        public ControlParameteresWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {

            doubleListView.ItemsSource = ControlParameters.GetList();
            positionListView.ItemsSource = ControlParameters.GetList();
        }



        private void accelTextBox_TextChanged(object sender, double value)
        {
            ControlParameters.Accel = value;
            SendData();
        }

        private void decelTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.Decel = value;
            SendData();

        }

        private void afactorTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.aFactor = value;
            SendData();
        }

        private void waFactorTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.WaFactor = value;
            SendData();

        }

        private void accuercyTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.Accuercy = value;
            SendData();

        }

        private void waccuercyTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.Waccuercy = value;
            SendData();

        }

        private void maxSpeedTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.MaxSpeed = value;
            SendData();

        }

        private void wAccelTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.wAccel = value;
            SendData();

        }

        private void wDecelTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.wDecel = value;
            SendData();

        }

        private void wMaxSTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.wMaxS = value;
            SendData();

        }

        private void tunningDistTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.TunningDist = value;
            SendData();

        }

        private void tunningAngleTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.TunningAngle = value;
            SendData();

        }

        private void pathweightTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.PathWeight = value;
            SendData();

        }

        private void meanTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.Mean = value;
            SendData();

        }

        private void varianceTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.Variance = value;
            SendData();

        }
        //---------------
        private void kv0TextBox_TextChanged(object sender, double value)
        {

            ControlParameters.K_v0 = value;
            SendData();

        }

        private void klfTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.K_lf = value;
            SendData();

        }
        private void kangTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.K_ang = value;
            SendData();

        }
        private void ksumangTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.K_sumAng = value;
            SendData();

        }
        private void ktotalTextBox_TextChanged(object sender, double value)
        {

            ControlParameters.K_total = value;
            SendData();

        }
         

        private void p0TextBox_TextChanged(object sender, MRL.SSL.CommonClasses.MathLibrary.Position2D value)
        {

            ControlParameters.P0 = value;
            SendData();
        }

        private void p1TextBox_TextChanged(object sender, MRL.SSL.CommonClasses.MathLibrary.Position2D value)
        {

            ControlParameters.P1 = value;
            SendData();
        }

        private void q0TextBox_TextChanged(object sender, MRL.SSL.CommonClasses.MathLibrary.Position2D value)
        {

            ControlParameters.Q0 = value;
            SendData();
        }

        private void q1TextBox_TextChanged(object sender, MRL.SSL.CommonClasses.MathLibrary.Position2D value)
        {

            ControlParameters.Q1 = value;
            SendData();
        }

        private void wp0TextBox_TextChanged(object sender, MRL.SSL.CommonClasses.MathLibrary.Position2D value)
        {

            ControlParameters.wP0 = value;
            SendData();
        }

        private void wp1TextBox_TextChanged(object sender, MRL.SSL.CommonClasses.MathLibrary.Position2D value)
        {

            ControlParameters.wP1 = value;
            SendData();

        }

        private void wq0TextBox_TextChanged(object sender, MRL.SSL.CommonClasses.MathLibrary.Position2D value)
        {

            ControlParameters.wQ0 = value;
            SendData();
        }

        private void wq1TextBox_TextChanged(object sender, MRL.SSL.CommonClasses.MathLibrary.Position2D value)
        {

            ControlParameters.wQ1 = value;
            SendData();
        }

        private void Refresh()
        {
            //positionListView.ItemsSource = ControlParameters.GetList();
            doubleListView.ItemsSource = ControlParameters.GetList();
        }

        private void SendData()
        {
            if (saveonchangeCheckBox.IsChecked.Value)
                ControlParameters.Save("ControlParameters");
            DataSender.CurrentWrapper.SendData.Add("ControlParameters");
            DataSender.SendOn.Set();
            //positionListView.ItemsSource = ControlParameters.GetList();
            //doubleListView.ItemsSource = ControlParameters.GetList();
        }

        private void doubleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            doubleListView.ItemsSource = ControlParameters.GetList();
        }

        private void positionListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            positionListView.ItemsSource = ControlParameters.GetList();
        }

        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
        
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Filter = "Xml Files|*.xml";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            ControlParameters.Save(sfd.FileName, false);
            
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = "xml files|*.xml";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;
            ControlParameters.Load(ofd.FileName, true);
            ControlParameters.Save("ControlParameters", false);
            Refresh();
        }




    }
}
