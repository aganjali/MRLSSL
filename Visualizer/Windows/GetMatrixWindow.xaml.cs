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

namespace Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for GetMatrixWindow.xaml
    /// </summary>
    public partial class GetMatrixWindow : Window
    {
        public GetMatrixWindow()
        {
            InitializeComponent();
        }
        public MathMatrix E = new MathMatrix(3, 4);
        public MathMatrix D = new MathMatrix(4, 1);
        public bool IsFilled { get; set; }
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            //--------------------------------
            E = MathMatrix.IdentityMatrix(3, 4);
            E[0, 0] = float.Parse(e00.Text);
            E[0, 1] = float.Parse(e01.Text);
            E[0, 2] = float.Parse(e02.Text);

            E[1, 0] = float.Parse(e10.Text);
            E[1, 1] = float.Parse(e11.Text);
            E[1, 2] = float.Parse(e12.Text);

            E[2, 0] = float.Parse(e20.Text);
            E[2, 1] = float.Parse(e21.Text);
            E[2, 2] = float.Parse(e22.Text);
            //--------------------------------
            D = MathMatrix.IdentityMatrix(4, 1);
            D[0, 0] = double.Parse(d00.Text);
            D[1, 0] = double.Parse(d10.Text);
            D[2, 0] = double.Parse(d20.Text);
            D[3, 0] = double.Parse(d30.Text);
            IsFilled = true;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
