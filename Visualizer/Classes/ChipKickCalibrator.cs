using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Visualizer.Windows;
using MRL.SSL.GameDefinitions.General_Settings;
using Enterprise;

namespace MRL.SSL.Visualizer.Classes
{
    
    public class ChipKickCalibrator
    {
        Dictionary<WorldModel,Position2D> _positions;
        List<WorldModel> _models = new List<WorldModel>();
        List<Position2D> _pos = new List<Position2D>();
        public ChipKickCalibrator()
        {
            _positions = new Dictionary<WorldModel, Position2D>();
        }

        public void Add(Position2D pos)
        {
            if (DataReciever.CurrentWrapper != null && DataReciever.CurrentWrapper.Model != null && DataReciever.CurrentWrapper.Model.BallState != null)
            {
                //_models.Add(DataReciever.CurrentWrapper.Model);
                //_pos.Add(pos);
                //_models.Clear();
                //_pos.Clear();

                if (!_positions.ContainsKey(DataReciever.CurrentWrapper.Model))
                    _positions.Add(DataReciever.CurrentWrapper.Model, pos);
            }    
           

        }
        public void Clear()
        {
            _positions.Clear();
        }

        public void Calibrate(int robotid)
        {
            Position2D? p1 = null/*, tmpP = null*/;
            Position2D p2 = new Position2D();
            int count = 0;
            WorldModel m1 = null, tmp = new WorldModel(), m2 = new WorldModel(), tmpM = new WorldModel();
            double dy = 0, d2y = 0, dy0 = 0;
            double? tmpPX = null, tmpPY = null, tmpMX = null, tmpMY = null;
            List<double> DY = new List<double>(), D2Y = new List<double>();
            bool firstP = false;
            TimeSpan ellapsed = new TimeSpan();
            foreach (var item in _positions.Keys)
            {
                if (!firstP)
                {
                    if (!p1.HasValue)
                    {
                        DY.Clear();
                        D2Y.Clear();
                        p1 = _positions[item];
                        //m1 = item;
                        tmp = item;
                        firstP = true;
                    }

                }
                else
                {

                    if (tmpPX.HasValue && tmpPY.HasValue && Math.Abs(_positions[item].X - tmpPX.Value) >= 0.005)
                    {
                        if (m1 == null)
                            m1 = tmp;
                        dy = (_positions[item].Y - tmpPY.Value) / (_positions[item].X - tmpPX.Value);
                        DY.Add(dy);

                        if (DY.Count > 1)
                        {
                            d2y = dy - dy0;
                            D2Y.Add(d2y);
                        }
                        dy0 = dy;
                    }
                   // else
                        tmp = new WorldModel(item);
                    try
                    {
                        if ((DY.Count > 1) && (Math.Sign(DY[DY.Count - 2]) != Math.Sign(dy)) && (Math.Abs(_positions[item].DistanceFrom(p1.Value)) > 0.05))
                            count++;
                    }
                    catch(Exception e)
                    {
                        Logger.Write(LogType.Exception, e.ToString());
                    }
                    if (count == 2)
                    {
                        m2.BallState = new SingleObjectState();
                        m2.BallState.Location = new Position2D(tmpMX.Value, tmpMY.Value);
                        m2.TimeElapsed = ellapsed;
                        p2 = new Position2D(tmpPX.Value, tmpPY.Value);
                        count++;
                    }
                    tmpMX = item.BallState.Location.X;
                    tmpMY = item.BallState.Location.Y;

                    tmpPX = _positions[item].X;
                    tmpPY = _positions[item].Y;

                }
                ellapsed = item.TimeElapsed;
                
            }
            //Line l = new Line(new Position2D(-2.09242080733102, -0.884461523439241), new Position2D(-1.29007681566825, -0.888953631134853), new System.Drawing.Pen(System.Drawing.Brushes.Red, 0.01f));
            //Line ll = new Line(new Position2D(-2.09382183165101, -0.880003303209559), new Position2D(-1.34851237621163, -0.930741439631547), new System.Drawing.Pen(System.Drawing.Brushes.Blue, 0.01f));
            //l.IsShown = true;
            //ll.IsShown = true;
            //DrawingObjects.AddObject(l);
            //DrawingObjects.AddObject(ll);     
     //       DrawingObjects.AddObject(new Line(m1.BallState.Location, m2.BallState.Location, new Pen(Brushes.Red, 0.01f)));
            double dis = 0, time = 0 ;

            if (m1 != null && m1.BallState != null && m1.BallState.Location != null && m2.BallState != null && m2.BallState.Location != null)
            {
                dis = m1.BallState.Location.DistanceFrom(m2.BallState.Location) - 0.0;
                time = (m2.TimeElapsed - m1.TimeElapsed).TotalMilliseconds;
            }
            AddChipkickWindow w = new AddChipkickWindow();
            w.robotidNum.Value = robotid;
            w.timeUpDown.Value = (decimal)(time / 1000);
            w.lenghtTextBox.Value = (decimal)dis;
            w.ShowDialog();
            if (w.MustBeAdd)
            {
                if (LookUpTable.Default.MetricChipKick == null)
                    LookUpTable.Default.MetricChipKick = new SerializableDictionary<int, MRL.SSL.GameDefinitions.Visualizer_Classes.MetricChipKick>();


                if (LookUpTable.Default.MetricChipKick.Any(p => p.Key == w.RobotID && p.Value.KickInfo.Any(a => a.Power == w.Power && a.HasSpin == w.HasSpin)))
                {
                    LookUpTable.Default.MetricChipKick.Single(p => p.Key == w.RobotID).Value.KickInfo.Single(a => a.Power == w.Power && a.HasSpin == w.HasSpin).Length = w.Lenght;
                    LookUpTable.Default.MetricChipKick.Single(p => p.Key == w.RobotID).Value.KickInfo.Single(a => a.Power == w.Power && a.HasSpin == w.HasSpin).SafeRadi = w.Safe;
                }
                else
                {
                    if (LookUpTable.Default.MetricChipKick.ContainsKey(w.RobotID))
                        LookUpTable.Default.MetricChipKick[w.RobotID].KickInfo.Add(new MRL.SSL.GameDefinitions.Visualizer_Classes.ChipKickInfo() { Length = w.Lenght, Power = w.Power, SafeRadi = w.Safe, HasSpin = w.HasSpin, Time = w.Time });
                    else
                    {
                        LookUpTable.Default.MetricChipKick.Add(w.RobotID, new MRL.SSL.GameDefinitions.Visualizer_Classes.MetricChipKick());
                        LookUpTable.Default.MetricChipKick[w.RobotID].KickInfo.Add(new MRL.SSL.GameDefinitions.Visualizer_Classes.ChipKickInfo() { Length = w.Lenght, Power = w.Power, SafeRadi = w.Safe, HasSpin = w.HasSpin, Time = w.Time });
                    }
                    LookUpTable.Default.Save();
                }
            }
//            MessageBox.Show("Dis: " + dis.ToString() + "\tTime: " + time.ToString());
            lock (_positions)
            {
                MemoryStream mstime = new MemoryStream();
                StreamWriter swtime = new StreamWriter(mstime);
                swtime.Write("M_x\t M_y\t\n");
                foreach (var item in _positions.Keys)
                {
                    swtime.Write(item.BallState.Location.X.ToString() + "\t");
                    swtime.Flush();
                    swtime.Write(item.BallState.Location.Y.ToString() + "\t\n");
                    swtime.Flush();
                }
                swtime.Write("P_x\t P_y\t\n");
                foreach (var item in _positions.Values)
                {
                    swtime.Write(item.X.ToString() + "\t");
                    swtime.Flush();
                    swtime.Write(item.Y.ToString() + "\t\n");
                    swtime.Flush();
                }
                swtime.Write("D\t\n");
                foreach (var item in DY)
                {
                    swtime.Write(item.ToString() + "\t\n");
                    swtime.Flush();
                }
                swtime.Write("D2\t\n");
                foreach (var item in D2Y)
                {
                    swtime.Write(item.ToString() + "\t\n");
                    swtime.Flush();
                }
                if (m1 != null && m1.BallState != null && m1.BallState.Location != null & m2.BallState != null && m2.BallState.Location != null)
                {
                    swtime.Write("M1: " + m1.BallState.Location.ToString() + "\n");
                    swtime.Flush();
                    swtime.Write("M2: " + m2.BallState.Location.ToString() + "\n");
                    swtime.Flush();
                }
                FileStream fstime = new FileStream(@"d:\ChipData2.txt", FileMode.Create, FileAccess.Write);
                fstime.Write(mstime.ToArray(), 0, (int)mstime.Length);
                
                //fstime.Flush();
                fstime.Close();
               // mstime.Close();
                swtime.Close();
            }
        }
    }
}
