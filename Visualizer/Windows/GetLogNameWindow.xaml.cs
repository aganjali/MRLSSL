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
    /// Interaction logic for GetLogNameWindow.xaml
    /// </summary>
    public partial class GetLogNameWindow : Window
    {
        public bool Ok { get; set; }
        public string LogName { get; set; }

        public GetLogNameWindow()
        {
            InitializeComponent();
        }
        
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (nameTextBox.Text == "") return;
            Ok = true;
            LogName = nameTextBox.Text;
            this.Close();
        }

        private void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Ok = false;
                this.Close();
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
