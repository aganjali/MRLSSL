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
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Visualizer.Classes;
using MRL.SSL.GameDefinitions;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using FormulaEvaluator;
using System.Text.RegularExpressions;
using MRL.SSL.GameDefinitions.General_Settings;

namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for ScoreWindow.xaml
    /// </summary>
    public partial class ScoreWindow : Window
    {
        enum Export
        {
            excel,
            text,
            binary
        }


        List<Score> Data = new List<Score>();
        int? robotID = null;
        Position2D? ballPos = null;
        WorldModel model;
        Score state = null;
        float[,] _coefs = new float[6, 6];

        public float[,] Coefs
        {
            get { return _coefs; }
            set { _coefs = value; }
        }

        public ScoreWindow()
        {
            InitializeComponent();
            DataReciever.DataRecieved += new DataReciever.DataRecievedEventHandler(DataReciever_DataRecieved);
        }

        void DataReciever_DataRecieved(object sender, System.IO.MemoryStream Data)
        {
            model = DataReciever.CurrentWrapper.Model;
            if (model != null)
            {
                var list = model.OurRobots.OrderBy(o => o.Value.Location.DistanceFrom(GameParameters.OurGoalCenter));
                if (list != null && list.Count() > 0)
                {
                    robotID = list.First().Key;
                    Dispatcher.Invoke((Action)(() =>
                    {
                        robotxTextBox.Text = list.First().Value.Location.X.ToString("f4");
                        robotyTextBox.Text = list.First().Value.Location.Y.ToString("f4");
                    }));
                    ballPos = model.BallState.Location;
                }
                else
                {
                    ballPos = model.BallState.Location;
                    robotID = null;

                }
            }
        }

        private void exporttoexcelMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog()
            {
                Filter = "Comma Seperated Values File (*.csv)|*.csv",
                DefaultExt = "csv"
            };
            if (dlg.ShowDialog() == true)
            {
                if (isInUse(dlg.FileName))
                {
                    MessageBox.Show("This file is in use by another program. Please close the program and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    
                    ExportTo(Data, dlg.FileName, Export.excel);

                    if (MessageBox.Show("Data exported successfully. Press \"OK\" to view the exported data.", "Export", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        try
                        {
                            Process.Start(dlg.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }

            }
        }

        private void ExportTo(List<Score> data, string p, Export mode)
        {
            double sigma;
            double.TryParse(sigmaxTextBox.Text, out sigma);
            RegionScore.Default.SigmaX = sigma;
            double.TryParse(sigmayTextBox.Text, out sigma);
            RegionScore.Default.SigmaY = sigma;
            double.TryParse(sigmaxtTextBox.Text, out sigma);
            RegionScore.Default.SigmaXt = sigma;
            double.TryParse(sigmaytTextBox.Text, out sigma);
            RegionScore.Default.SigmaYt = sigma;
            if (mode == Export.binary)
            {
                RegionScore.Save(p);
            }
            else
            {
                string listSeparator = "";
                if (mode == Export.excel)
                    listSeparator = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator;
                else
                    listSeparator = "\t";
                StringBuilder cvsStringBuilder = new StringBuilder();

                cvsStringBuilder.AppendFormat("RX{0}RY{0}Score{0}Region\r\n", listSeparator);
                foreach (var item in data)
                {
                    cvsStringBuilder.AppendFormat("{1}{0}{2}{0}{3}{0}{4}", listSeparator,
                        item.Robot.X.ToString("f3"),
                        item.Robot.Y.ToString("f3"),
                        item.PosScore.ToString(),
                        item.Region.ToString());
                    cvsStringBuilder.Append("\r\n");
                }

                var writer = new StreamWriter(p);
                writer.Write(cvsStringBuilder.ToString());
                writer.Close();
            }
        }

        private bool isInUse(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    return false;
                }
                catch (IOException ex)
                {
                    Enterprise.Logger.Write(Enterprise.LogType.Exception, ex.ToString());
                    return true;
                }
            }
            return false;
        }

        private void exporttotextMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog()
            {
                Filter = "Text File (*.txt)|*.txt",
                DefaultExt = "txt"
            };
            if (dlg.ShowDialog() == true)
            {
                if (isInUse(dlg.FileName))
                {
                    MessageBox.Show("This file is in use by another program. Please close the program and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ExportTo(Data, dlg.FileName, Export.text);

                    if (MessageBox.Show("Data exported successfully. Press \"OK\" to view the exported data.", "Export", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                    {
                        try
                        {
                            Process.Start(dlg.FileName);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }

        }

        private void addToTable_Click(object sender, RoutedEventArgs e)
        {
            double d; int r;
            double sigmax
                   , sigmay
                   , sigmaxt
                   , sigmayt;
            if (state != null && scoreTextBox.Text != "" && double.TryParse(scoreTextBox.Text, out d) && int.TryParse(regionTextBox.Text, out r)
                && double.TryParse(sigmaxTextBox.Text, out sigmax) && double.TryParse(sigmayTextBox.Text, out sigmay) && double.TryParse(sigmaxtTextBox.Text, out sigmaxt) && double.TryParse(sigmaytTextBox.Text, out sigmayt))
            {
                state.PosScore = d;
                state.Robot = new Position2D(double.Parse(robotxTextBox.Text), double.Parse(robotyTextBox.Text));
                state.Region = r;
                Data.Add(state.Clone());
                dataListView.ItemsSource = Data.ToList();
                RegionScore.Default.Data = new List<Score>();
                RegionScore.Default.Data = Data.ToList();
                RegionScore.Default.SigmaX = sigmax;
                RegionScore.Default.SigmaY = sigmay;
                RegionScore.Default.SigmaXt = sigmaxt;
                RegionScore.Default.SigmaYt = sigmayt;
            }
            else
            {
                MessageBox.Show("Please compelete the form!");
            }
        }

        private void catchdataButton_Click(object sender, RoutedEventArgs e)
        {
            if (robotID.HasValue)
            {
                state = new Score();
                Score ds = new Score();
                ds.Robot = model.OurRobots[robotID.Value].Location;
                robotxTextBox.Text = ds.Robot.X.ToString("f3");
                robotyTextBox.Text = ds.Robot.Y.ToString("f3");
                state = ds;
            }
        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Data = new List<Score>();
            dataListView.ItemsSource = Data.ToList();
        }

        private void fitButton_Click(object sender, RoutedEventArgs e)
        {
            int x, y, r;
            if (int.TryParse(degxTextBox.Text, out  x) && int.TryParse(degyTextBox.Text, out  y) && int.TryParse(regionTextBox.Text, out r))
            {
                List<Score> data = Data.Where(w => w.Region == r).ToList();
                string f = Formula(x, y);
                string[] parts = f.Split(new char[] { '+' });
                IParser par = new ExpParser();
                ExpEvaluator eu = new ExpEvaluator(par);
                MathMatrix A = new MathMatrix(data.Count, parts.Length);
                MathMatrix B = new MathMatrix(data.Count, 1);
                MathMatrix B_bar = new MathMatrix(data.Count, 1);
                double[] res = new double[data.Count];
                MathMatrix C = new MathMatrix(parts.Length, 1);
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < parts.Length; j++)
                    {
                        par = new ExpParser();
                        eu = new ExpEvaluator(par);
                        eu.SetExpression(parts[j]);
                        eu["x"] = (double)data[i].Robot.X;
                        eu["y"] = (double)data[i].Robot.Y;
                        double ff = eu.Evaluate(); ;
                        A[i, j] = ff;

                    }
                    B[i, 0] = data[i].PosScore;
                }
                C = A.PsuedoInverse * B;
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < parts.Length; j++)
                    {
                        eu.SetExpression(parts[j]);
                        eu["x"] = data[i].Robot.X;
                        eu["y"] = data[i].Robot.Y;
                        B_bar[i, 0] += C[j, 0] * eu.Evaluate();
                    }

                    res[i] = B[i, 0] - B_bar[i, 0];
                }
                _coefs = CoefFormat(x, y, C, f);


                RegionScore.Default.Scores[r] = _coefs;

                AminChartWindow a = new AminChartWindow();
                a.ret = res;
                a.Show();
            }
        }

        private string Formula(int degx, int degy)
        {
            string f = "";
            int i = (degx - 1) * 5 + degy;
            switch (i)
            {
                case 1:
                    {
                        f = "1 + x + y";
                        break;
                    }
                case 2:
                    {
                        f = "1 + x + y + x*y + y^2";

                        break;
                    }
                case 3:
                    {
                        f = "1 + x + y + x*y + y^2 + x*y^2 + y^3";
                        break;
                    }
                case 4:
                    {
                        f = "1 + x + y + x*y + y^2 + x*y^2 + y^3 + x*y^3 + y^4";
                        break;
                    }
                case 5:
                    {
                        f = "1 + x + y + x*y + y^2 + x*y^2 + y^3 + x*y^3 + y^4 + x*y^4 + y^5";
                        break;
                    }
                case 6:
                    {
                        f = "1 + x + y + x^2 + x*y";
                        break;
                    }
                case 7:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2";
                        break;
                    }
                case 8:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^2*y + x*y^2 + y^3";
                        break;
                    }
                case 9:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^2*y + x*y^2 + y^3 + x^2*y^2 + x*y^3 + y^4";
                        break;
                    }
                case 10:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^2*y + x*y^2 + y^3 + x^2*y^2 + x*y^3 + y^4 + x^2*y^3 + x*y^4 + y^5";
                        break;
                    }
                case 11:
                    {
                        f = "1 + x + y + x^2 + x*y + x^3 + x^2*y";
                        break;
                    }
                case 12:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2";
                        break;
                    }
                case 13:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3";
                        break;
                    }
                case 14:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3 + x^3*y + x^2*y^2 + x*y^3 + y^4";
                        break;
                    }
                case 15:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3 + x^3*y + x^2*y^2 + x*y^3 + y^4 + x^3*y^2 + x^2*y^3 + x*y^4 + y^5";
                        break;
                    }
                case 16:
                    {
                        f = " 1 + x + y + x^2 + x*y + x^3 + x^2*y + x^4 + x^3*y";
                        break;
                    }
                case 17:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + x^4 + x^3*y + x^2*y^2";
                        break;
                    }
                case 18:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3 + x^4 + x^3*y + x^2*y^2 + x*y^3";
                        break;
                    }
                case 19:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3 + x^4 + x^3*y + x^2*y^2 + x*y^3 + y^4";
                        break;
                    }
                case 20:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3 + x^4 + x^3*y + x^2*y^2 + x*y^3 + y^4 + x^4*y + x^3*y^2 + x^2*y^3 + x*y^4 + y^5";
                        break;
                    }
                case 21:
                    {
                        f = "1 + x + y + x^2 + x*y + x^3 + x^2*y +x^4 + x^3*y + x^5 + x^4*y";
                        break;
                    }
                case 22:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + x^4 + x^3*y + x^2*y^2 + x^5 + x^4*y + x^3*y^2";
                        break;
                    }
                case 23:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3 + x^4 + x^3*y + x^2*y^2 + x*y^3 + x^5 + x^4*y + x^3*y^2 + x^2*y^3";
                        break;
                    }
                case 24:
                    {
                        f = " 1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3 + x^4 + x^3*y + x^2*y^2 + x*y^3 + y^4 + x^5 + x^4*y + x^3*y^2 + x^2*y^3 + x*y^4";
                        break;
                    }
                case 25:
                    {
                        f = "1 + x + y + x^2 + x*y + y^2 + x^3 + x^2*y + x*y^2 + y^3 + x^4 + x^3*y + x^2*y^2 + x*y^3 + y^4 + x^5 + x^4*y + x^3*y^2 + x^2*y^3 + x*y^4 + y^5";
                        break;
                    }
            }

            return f.Trim();
        }

        private float[,] CoefFormat(int degx, int degy, MathMatrix C, string f)
        {
            float[,] c = new float[6, 6];
            int idx = (degx - 1) * 5 + degy;
            switch (idx)
            {
                case 1:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        break;
                    }
                case 2:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[1, 1] = (float)C[3, 0];
                        c[0, 2] = (float)C[4, 0];
                        break;
                    }
                case 3:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[1, 1] = (float)C[3, 0];
                        c[0, 2] = (float)C[4, 0];
                        c[1, 2] = (float)C[5, 0];
                        c[0, 3] = (float)C[6, 0];
                        break;
                    }
                case 4:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[1, 1] = (float)C[3, 0];
                        c[0, 2] = (float)C[4, 0];
                        c[1, 2] = (float)C[5, 0];
                        c[0, 3] = (float)C[6, 0];
                        c[1, 3] = (float)C[7, 0];
                        c[0, 4] = (float)C[8, 0];
                        break;
                    }
                case 5:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[1, 1] = (float)C[3, 0];
                        c[0, 2] = (float)C[4, 0];
                        c[1, 2] = (float)C[5, 0];
                        c[0, 3] = (float)C[6, 0];
                        c[1, 3] = (float)C[7, 0];
                        c[0, 4] = (float)C[8, 0];
                        c[1, 4] = (float)C[9, 0];
                        c[0, 5] = (float)C[10, 0];
                        break;
                    }
                case 6:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        break;
                    }
                case 7:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        break;
                    }
                case 8:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[2, 1] = (float)C[6, 0];
                        c[1, 2] = (float)C[7, 0];
                        c[0, 3] = (float)C[8, 0];
                        break;
                    }
                case 9:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[2, 1] = (float)C[6, 0];
                        c[1, 2] = (float)C[7, 0];
                        c[0, 3] = (float)C[8, 0];
                        c[2, 2] = (float)C[9, 0];
                        c[1, 3] = (float)C[10, 0];
                        c[0, 4] = (float)C[11, 0];
                        break;
                    }
                case 10:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[2, 1] = (float)C[6, 0];
                        c[1, 2] = (float)C[7, 0];
                        c[0, 3] = (float)C[8, 0];
                        c[2, 2] = (float)C[9, 0];
                        c[1, 3] = (float)C[10, 0];
                        c[0, 4] = (float)C[11, 0];
                        c[2, 3] = (float)C[12, 0];
                        c[1, 4] = (float)C[13, 0];
                        c[0, 5] = (float)C[14, 0];
                        break;
                    }
                case 11:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[3, 0] = (float)C[5, 0];
                        c[2, 1] = (float)C[6, 0];
                        break;
                    }
                case 12:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        break;
                    }
                case 13:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        break;
                    }
                case 14:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        c[3, 1] = (float)C[10, 0];
                        c[2, 2] = (float)C[11, 0];
                        c[1, 3] = (float)C[12, 0];
                        c[0, 4] = (float)C[13, 0];
                        break;
                    }
                case 15:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        c[3, 1] = (float)C[10, 0];
                        c[2, 2] = (float)C[11, 0];
                        c[1, 3] = (float)C[12, 0];
                        c[0, 4] = (float)C[13, 0];
                        c[3, 2] = (float)C[14, 0];
                        c[2, 3] = (float)C[15, 0];
                        c[1, 4] = (float)C[16, 0];
                        c[0, 5] = (float)C[17, 0];
                        break;
                    }
                case 16:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[3, 0] = (float)C[5, 0];
                        c[2, 1] = (float)C[6, 0];
                        c[4, 0] = (float)C[7, 0];
                        c[3, 1] = (float)C[8, 0];
                        break;
                    }
                case 17:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[4, 0] = (float)C[9, 0];
                        c[3, 1] = (float)C[10, 0];
                        c[2, 2] = (float)C[11, 0];
                        break;
                    }
                case 18:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        c[4, 0] = (float)C[10, 0];
                        c[3, 1] = (float)C[11, 0];
                        c[2, 2] = (float)C[12, 0];
                        c[1, 3] = (float)C[13, 0];
                        break;
                    }
                case 19:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        c[4, 0] = (float)C[10, 0];
                        c[3, 1] = (float)C[11, 0];
                        c[2, 2] = (float)C[12, 0];
                        c[1, 3] = (float)C[13, 0];
                        c[0, 4] = (float)C[14, 0];
                        break;
                    }
                case 20:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        c[4, 0] = (float)C[10, 0];
                        c[3, 1] = (float)C[11, 0];
                        c[2, 2] = (float)C[12, 0];
                        c[1, 3] = (float)C[13, 0];
                        c[0, 4] = (float)C[14, 0];
                        c[4, 1] = (float)C[15, 0];
                        c[3, 2] = (float)C[16, 0];
                        c[2, 3] = (float)C[17, 0];
                        c[1, 4] = (float)C[18, 0];
                        c[0, 5] = (float)C[19, 0];
                        break;
                    }
                case 21:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[3, 0] = (float)C[5, 0];
                        c[2, 1] = (float)C[6, 0];
                        c[4, 0] = (float)C[7, 0];
                        c[3, 1] = (float)C[8, 0];
                        c[5, 0] = (float)C[9, 0];
                        c[4, 1] = (float)C[10, 0];
                        break;
                    }
                case 22:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[4, 0] = (float)C[9, 0];
                        c[3, 1] = (float)C[10, 0];
                        c[2, 2] = (float)C[11, 0];
                        c[5, 0] = (float)C[12, 0];
                        c[4, 1] = (float)C[13, 0];
                        c[3, 2] = (float)C[14, 0];
                        break;
                    }
                case 23:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        c[4, 0] = (float)C[10, 0];
                        c[3, 1] = (float)C[11, 0];
                        c[2, 2] = (float)C[12, 0];
                        c[1, 3] = (float)C[13, 0];
                        c[5, 0] = (float)C[14, 0];
                        c[4, 1] = (float)C[15, 0];
                        c[3, 2] = (float)C[16, 0];
                        c[2, 3] = (float)C[17, 0];
                        break;
                    }
                case 24:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        c[4, 0] = (float)C[10, 0];
                        c[3, 1] = (float)C[11, 0];
                        c[2, 2] = (float)C[12, 0];
                        c[1, 3] = (float)C[13, 0];
                        c[0, 4] = (float)C[14, 0];
                        c[5, 0] = (float)C[15, 0];
                        c[4, 1] = (float)C[16, 0];
                        c[3, 2] = (float)C[17, 0];
                        c[2, 3] = (float)C[18, 0];
                        c[1, 4] = (float)C[19, 0];
                        break;
                    }
                case 25:
                    {
                        c[0, 0] = (float)C[0, 0];
                        c[1, 0] = (float)C[1, 0];
                        c[0, 1] = (float)C[2, 0];
                        c[2, 0] = (float)C[3, 0];
                        c[1, 1] = (float)C[4, 0];
                        c[0, 2] = (float)C[5, 0];
                        c[3, 0] = (float)C[6, 0];
                        c[2, 1] = (float)C[7, 0];
                        c[1, 2] = (float)C[8, 0];
                        c[0, 3] = (float)C[9, 0];
                        c[4, 0] = (float)C[10, 0];
                        c[3, 1] = (float)C[11, 0];
                        c[2, 2] = (float)C[12, 0];
                        c[1, 3] = (float)C[13, 0];
                        c[0, 4] = (float)C[14, 0];
                        c[5, 0] = (float)C[15, 0];
                        c[4, 1] = (float)C[16, 0];
                        c[3, 2] = (float)C[17, 0];
                        c[2, 3] = (float)C[18, 0];
                        c[1, 4] = (float)C[19, 0];
                        c[0, 5] = (float)C[20, 0];
                        break;
                    }

            }
            return c;
        }

        private void exporttobinaryMenuItem_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog()
            {
                Filter = "Text File (*.txt)|*.txt",
                DefaultExt = "txt"
            };
            if (dlg.ShowDialog() == true)
            {
                if (isInUse(dlg.FileName))
                {
                    MessageBox.Show("This file is in use by another program. Please close the program and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    ExportTo(Data, dlg.FileName, Export.binary);
                }
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                Filter = "Text File (*.txt)|*.txt",
                DefaultExt = "txt"
            };
            if (dlg.ShowDialog() == true)
            {

                RegionScore.Load(dlg.FileName);
                dataListView.ItemsSource = RegionScore.Default.Data.ToList();
                Data = RegionScore.Default.Data;
                sigmaxTextBox.Text = RegionScore.Default.SigmaX.ToString();
                sigmayTextBox.Text = RegionScore.Default.SigmaY.ToString();
                sigmaxtTextBox.Text = RegionScore.Default.SigmaXt.ToString();
                sigmaytTextBox.Text = RegionScore.Default.SigmaYt.ToString();
            }
        }

        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (dataListView.SelectedItem == null) return;

            Data.Remove(dataListView.SelectedItem as Score);
            dataListView.ItemsSource = Data.ToList();
            RegionScore.Default.Data = new List<Score>();
            RegionScore.Default.Data = Data.ToList();
        }
    }
}
