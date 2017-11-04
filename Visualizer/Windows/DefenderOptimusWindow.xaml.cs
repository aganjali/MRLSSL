using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Media.Imaging;

using MRL.SSL.Visualizer.Classes;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.CommonClasses.Extentions;
using System.IO;
using FormulaEvaluator;
namespace MRL.SSL.Visualizer.Windows
{
    /// <summary>
    /// Interaction logic for DefenderOptimusWindow.xaml
    /// </summary>
    public partial class DefenderOptimusWindow : Window
    {

        int? DefenderID = null;
        int? GoalieID = null;
        List<DefendingState> Data = new List<DefendingState>();
        WorldModel model;
        
        public DefenderOptimusWindow()
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
                if (list != null && list.Count() > 1)
                {
                    GoalieID = list.ElementAt(0).Key;
                    DefenderID = list.ElementAt(1).Key;

                }
                else
                {
                    DefenderID = null;
                    GoalieID = null;
                }
            }
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            if (DefenderID.HasValue && GoalieID.HasValue)
            {
                DefendingState ds = new DefendingState();

                ds.dD = model.BallState.Location.DistanceFrom(model.OurRobots[DefenderID.Value].Location);
                ds.dG = model.BallState.Location.DistanceFrom(model.OurRobots[GoalieID.Value].Location);
                ds.d = model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
                Line l = new Line(model.BallState.Location, model.OurRobots[GoalieID.Value].Location);
                Line l1 = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                Position2D? inter = l.IntersectWithLine(l1);
                ds.GD = (inter.HasValue) ? inter.Value.DistanceFrom(GameParameters.OurGoalCenter) : 0;
                
                Vector2D v1 = (GameParameters.OurGoalLeft - model.BallState.Location);
                Vector2D v2 = (GameParameters.OurGoalRight - model.BallState.Location);


                ds.alfa = Vector2D.AngleBetweenInRadians(v1, v2);

                ds.Ball = model.BallState.Location;
                ds.Goali = model.OurRobots[GoalieID.Value].Location;
                ds.Defender = model.OurRobots[DefenderID.Value].Location;

                Data.Add(ds);
                list.ItemsSource = Data.ToList(); ;
            }
        }

        private void exportFileButton_Click(object sender, RoutedEventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);
            sw.Write("BallX\tBallY\tGoaliX\tGoaliY\tDefenderX\tDefenderY\n");
            foreach (var item in Data)
            {
                sw.Write(item.Ball.X + "\t");
                sw.Flush();

                sw.Write(item.Ball.Y + "\t");
                sw.Flush();

                sw.Write(item.Goali.X + "\t");
                sw.Flush();

                sw.Write(item.Goali.Y + "\t");
                sw.Flush();

                sw.Write(item.Defender.X + "\t");
                sw.Flush();

                sw.Write(item.Defender.Y + "\t\n");
                sw.Flush();

            }
            string goaliX = Fitiing(Data, 0);
            string goaliY = Fitiing(Data, 1);
            string defenderX = Fitiing(Data, 2);
            string defenderY = Fitiing(Data, 3);
            sw.Write("\n");
            sw.Flush();
            sw.Write("GoaliX: "+goaliX + "\n");
            sw.Flush();
            sw.Write("GoaliY: "+goaliY + "\n");
            sw.Flush();
            sw.Write("DefenderX: " + defenderX + "\n");
            sw.Flush();
            sw.Write("DefenderY: " + defenderY + "\n");
            sw.Flush();
            FileStream fs = new FileStream(@"d:\defending.txt", FileMode.Create);
            fs.Write(ms.ToArray(), 0, (int)ms.Length);
            fs.Close();
            sw.Close();
           

            MessageBox.Show("Successfull!");
        }
        private string Fitiing(List<DefendingState> Data, int DataMode)
        {
            float[,] _coefs;
            string f = Formula(2, 2);
            string[] parts = f.Split(new char[] { '+' });
            IParser par = new ExpParser();
            ExpEvaluator eu = new ExpEvaluator(par);
            MathMatrix A = new MathMatrix(Data.Count, parts.Length);
            MathMatrix B = new MathMatrix(Data.Count, 1);
            MathMatrix B_bar = new MathMatrix(Data.Count, 1);
            double[] res = new double[Data.Count];
            MathMatrix C = new MathMatrix(parts.Length, 1);
            for (int i = 0; i < Data.Count; i++)
            {
                for (int j = 0; j < parts.Length; j++)
                {
                    par = new ExpParser();
                    eu = new ExpEvaluator(par);
                    eu.SetExpression(parts[j]);
                    eu["x"] = (double)Data[i].Ball.X;
                    eu["y"] = (double)Data[i].Ball.Y;
                    double ff = eu.Evaluate(); ;
                    A[i, j] = ff;

                }
                if(DataMode == 0)
                    B[i, 0] = Data[i].Goali.X; ;
                if(DataMode == 1)
                    B[i, 0] = Data[i].Goali.Y;
                if (DataMode == 2)
                    B[i, 0] = Data[i].Defender.X;
                if (DataMode == 3)
                    B[i, 0] = Data[i].Defender.Y; 
            }
            C = A.PsuedoInverse * B;

            for (int i = 0; i < parts.Length; i++)
            {
                if (C[i, 0] >= 0)
                    parts[i] = C[i, 0].ToString() + "*" + parts[i];
                else
                    parts[i] = "0-" + Math.Abs(C[i, 0]).ToString() + "*" + parts[i];
            }
            string f2 = string.Join("+", parts);
            eu.SetExpression(f2);
            for (int i = 0; i < Data.Count; i++)
            {
                eu["x"] = Data[i].Ball.X;
                eu["y"] = Data[i].Ball.Y;
                B_bar[i, 0] = eu.Evaluate();
                res[i] = B[i, 0] - B_bar[i, 0];
            }
            _coefs = CoefFormat(2, 2, C, f);
            AminChartWindow a = new AminChartWindow();
            a.ret = res;
            a.Show();
            return f2;
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

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            Data.Clear();
            list.ItemsSource = Data.ToList();
        }

        private void delButton_Click(object sender, RoutedEventArgs e)
        {
            if (list.SelectedItem == null) return;
            var a = list.SelectedItem.As<DefendingState>();
            Data.Remove(a);
            list.ItemsSource = Data.ToList();
        }

       
    }
}
