using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using MRL.SSL.CommonClasses.Extentions;
using System.Windows.Shapes;
using System.Drawing;


namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for ChangeObjectPenWindow.xaml
    /// </summary>
    public partial class ChangeObjectPenWindow : Window
    {
        public ChangeObjectPenWindow()
        {
            InitializeComponent();
        }
        System.Drawing.Color SelectedColor = Color.Black;
        public Pen pen = new Pen(Color.Black);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            objectColorBorder.Background = new System.Windows.Media.SolidColorBrush
                    (System.Windows.Media.Color.FromRgb(pen.Color.R, pen.Color.G, pen.Color.B));
            startcapComboBox.SelectedIndex = (int)pen.StartCap;
            endcapComboBox.SelectedIndex = (int)pen.EndCap;
            dashstyleComboBox.SelectedIndex = (((int)pen.DashStyle) - 2);
            
            witdhTextBox.Text = pen.Width.ToString();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            pen.Color = SelectedColor;
            pen.StartCap = getLieStyle(startcapComboBox.SelectedIndex);
            pen.EndCap = getLieStyle(endcapComboBox.SelectedIndex);
            pen.DashStyle = getDashCap(dashstyleComboBox.SelectedIndex);
            pen.Width = float.Parse(witdhTextBox.Text);
            this.Close();
        }

        private System.Drawing.Drawing2D.LineCap getLieStyle(int Index)
        {
            System.Drawing.Drawing2D.LineCap ret = System.Drawing.Drawing2D.LineCap.Flat;
            switch (Index)
            {
                case 0: ret = System.Drawing.Drawing2D.LineCap.Flat; break;
                case 1: ret = System.Drawing.Drawing2D.LineCap.Round; break;
                case 2: ret = System.Drawing.Drawing2D.LineCap.Square; break;
                case 3: ret = System.Drawing.Drawing2D.LineCap.Triangle; break;
            }
            return ret;
        }

        private System.Drawing.Drawing2D.DashStyle getDashCap(int Index)
        {
            
            System.Drawing.Drawing2D.DashStyle ret = System.Drawing.Drawing2D.DashStyle.Solid;
            switch (Index)
            {
                case 0: ret = System.Drawing.Drawing2D.DashStyle.Dash; break;
                case 1: ret = System.Drawing.Drawing2D.DashStyle.DashDot; break;
                case 2: ret = System.Drawing.Drawing2D.DashStyle.DashDotDot; break;
                case 3: ret = System.Drawing.Drawing2D.DashStyle.Dot; break;
                case 4: ret = System.Drawing.Drawing2D.DashStyle.Solid; break;
            }
            
            return ret;

        }

        private void objectcolorselectButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                objectColorBorder.Background = new System.Windows.Media.SolidColorBrush
                    (System.Windows.Media.Color.FromRgb(cd.Color.R, cd.Color.G, cd.Color.B));
                SelectedColor = cd.Color;
            }

        }
    }
}
