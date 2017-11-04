using System;
using System.Collections.Generic;
using System.Linq;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.Planning.GPUDirect;

namespace MRL.SSL.Planning.GamePlanner
{
    public partial class Scoring : IDisposable
    {

        int counter = 0;
        int? lastOppBallOwner = null;
        private static float alpha = 0.2f;

        public Scoring()
        {
            AllocateVariables();
        }

        double sigmax;
        double sigmay;
        double sigmaxt;
        double sigmayt;
        Dictionary<int, List<Score>> data;
        GVector2D Field;
        float[] Phi, Kdx, Kdy;
        float[] opps = new float[GPUParams.maxRobotCount * 2];
        int maxCount = int.MinValue;
        float[] eachDataCount;
        bool FLAG = true;
        
        private void AllocateVariables()
        {
            sigmax = GPPlanner.SigmaX;
            sigmay = GPPlanner.SigmaY;
            data = GPPlanner.Data;
            foreach (var item in data)
            {
                if (maxCount < item.Value.Count)
                    maxCount = item.Value.Count;
            }
            maxCount = Math.Max(maxCount, 1);

            Phi = new float[data.Count * maxCount * GPUParams.maxRobotCount];
            Kdx = new float[data.Count * maxCount * GPUParams.maxRobotCount];
            Kdy = new float[data.Count * maxCount * GPUParams.maxRobotCount];

            eachDataCount = new float[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                eachDataCount[i] = data[i + 1].Count;
            }
            sigmaxt = GPPlanner.SigmaXt;
            sigmayt = GPPlanner.SigmaYt;
            Field = new GVector2D(2 * (float)GameParameters.OurGoalCenter.X, 2 * (float)GameParameters.OurLeftCorner.Y);
        }
        public void CalculateRobotsScores(List<WorldModel> model, GVector2D GlobalBallSpeed, ref GamePlannerInfo GPInfo, GamePlannerInfo LastGPInfo, bool logViewer)
        {
            double k = 0.02;
            WorldModel Model = model.LastOrDefault();

            GPInfo.OppTeam.GoaliID = GetOppGoaliID(Model);
            GPInfo.OurTeam.BallOwner = GetOurBallOwnerID(Model, GPInfo);
            if (GPInfo.OurTeam.BallOwner.HasValue)
                DrawingObjects.AddObject(new StringDraw(GPInfo.OurTeam.BallOwner.Value.ToString(), new Position2D(-1, 0)),"ballowner"); 
            GPInfo.OppTeam.BallOwner = GetOppBallOwnerID(Model, GPInfo);
            var Opponents = Model.Opponents;//.ToDictionary(kk => kk.Key, v => v.Value);
            if (Opponents.Count > GPUParams.maxRobotCount)
            {
                Opponents = Model.Opponents.ToDictionary(kk => kk.Key, v => v.Value);
                int c = 0;
                if (GPInfo.OppTeam.GoaliID.HasValue)
                {
                    Opponents.Remove(GPInfo.OppTeam.GoaliID.Value);
                    c++;
                }
                if (GPInfo.OppTeam.BallOwner.HasValue && Opponents.ContainsKey(GPInfo.OppTeam.BallOwner.Value))
                {
                    Opponents.Remove(GPInfo.OppTeam.BallOwner.Value);
                    c++;
                }
                Opponents = Opponents.Take(GPUParams.maxRobotCount - c).ToDictionary(kk => kk.Key, v => v.Value);
                if (GPInfo.OppTeam.GoaliID.HasValue)
                    Opponents[GPInfo.OppTeam.GoaliID.Value] = new SingleObjectState(Model.Opponents[GPInfo.OppTeam.GoaliID.Value]);
                if (GPInfo.OppTeam.BallOwner.HasValue)
                    Opponents[GPInfo.OppTeam.BallOwner.Value] = new SingleObjectState(Model.Opponents[GPInfo.OppTeam.BallOwner.Value]);
            }
            if (GPInfo.OppTeam.BallOwner.HasValue)
                GPInfo.OppTeam.Scores = CalculateWithBallOwner(model, Opponents, GlobalBallSpeed, Model.BallState.Location, GPInfo, LastGPInfo);
            else
                CalculateWithOutBallOwner(model, Model, Opponents, GlobalBallSpeed, ref GPInfo, LastGPInfo);

            foreach (var item in Model.Opponents)
            {
                if (!GPInfo.OppTeam.Scores.ContainsKey(item.Key))
                    GPInfo.OppTeam.Scores[item.Key] = 0.65f;
                if (GPInfo.OppTeam.Scores[item.Key] > 1)
                    GPInfo.OppTeam.Scores[item.Key] = 1;
                else if (GPInfo.OppTeam.Scores[item.Key] < 0)
                    GPInfo.OppTeam.Scores[item.Key] = 0;
                if (GPInfo.OppTeam.Scores[item.Key] > 0.989 && ((GPInfo.OppTeam.GoaliID.HasValue && item.Key != GPInfo.OppTeam.GoaliID.Value) || !GPInfo.OppTeam.GoaliID.HasValue))
                    GPInfo.OppTeam.Scores[item.Key] = 0.989f;
                else if (GPInfo.OppTeam.BallOwner.HasValue && item.Key == GPInfo.OppTeam.BallOwner.Value)
                    GPInfo.OppTeam.Scores[item.Key] = 1;
                if (LastGPInfo.OppTeam.Scores.ContainsKey(item.Key) && Math.Abs(GPInfo.OppTeam.Scores[item.Key] - LastGPInfo.OppTeam.Scores[item.Key]) < k)
                    GPInfo.OppTeam.Scores[item.Key] = LastGPInfo.OppTeam.Scores[item.Key];
                if (!logViewer)
                {
                    DrawingObjects.AddObject(new StringDraw(GPInfo.OppTeam.Scores[item.Key].ToString(), item.Value.Location + new Vector2D(-0.3, 0.2)), "score" + item.Key.ToString());
                    if (GPInfo.OppTeam.Scores[item.Key] >= 0.75)
                        DrawingObjects.AddObject(new Circle(item.Value.Location, 0.2, new System.Drawing.Pen(System.Drawing.Color.Blue, 0.01f)));
                }

            }
            var dic = GPInfo.OppTeam.Scores.OrderByDescending(o => o.Value).ToDictionary(d => d.Key, v => v.Value);
            GPInfo.OppTeam.Scores = dic;
        }

        private int? GetOppBallOwnerID(WorldModel Model, GamePlannerInfo GPInfo)
        {
            int? oppBallOwner = null;
            Circle c = new Circle(Position2D.Zero, 0.5);
            List<int> listID = new List<int>();
            double minDist = 10;
            double dist = 0;
            foreach (var item in Model.Opponents)
            {
                if (!GPInfo.OppTeam.GoaliID.HasValue || (GPInfo.OppTeam.GoaliID.HasValue && GPInfo.OppTeam.GoaliID.Value != item.Key))
                {
                    c = new Circle(item.Value.Location, 0.5);
                    if (c.Center.DistanceFrom(Model.BallState.Location) <= c.Radious)
                    {
                        if (GPInfo.OppTeam.CatchBallLines.ContainsKey(item.Key) && GPInfo.OppTeam.CatchBallLines[item.Key].Count > 0)
                        {
                            dist = c.Center.DistanceFrom(Model.BallState.Location);
                            if (dist < c.Radious)
                            {
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    oppBallOwner = item.Key;
                                }
                            }
                        }
                    }
                }
            }
            lastOppBallOwner = oppBallOwner;
            return oppBallOwner;
        }
        private int? GetOurBallOwnerID(WorldModel Model, GamePlannerInfo GPInfo)
        {
            double margRD = 0.02, margD = 0.5, margR = 0.2;
            if (counter > 20)
            {
                margRD = 0.025;
                margD = 0.55;
                margR = 0.22;
            }
            var robotStatus = Model.OurRobots.Where(w => w.Value.Location.DistanceFrom(Model.BallState.Location) < margD).Select(s => new
            {
                Id = s.Key,
                left = (s.Value.Location - Vector2D.FromAngleSize(Model.OurRobots[s.Key].Angle.Value * Math.PI / 180 + Math.PI / 2, RobotParameters.OurRobotParams.Diameter / 2)),
                right = (s.Value.Location - Vector2D.FromAngleSize(Model.OurRobots[s.Key].Angle.Value * Math.PI / 180 + -Math.PI / 2, RobotParameters.OurRobotParams.Diameter / 2))
            });
            if (robotStatus.Count() > 0)
            {

                var robot_Ang = robotStatus.Select(s => new
                {
                    Id = s.Id,
                    leftLine = new Line(s.left, s.left + Vector2D.FromAngleSize(Model.OurRobots[s.Id].Angle.Value * Math.PI / 180, 1)),
                    rightLine = new Line(s.right, s.right + Vector2D.FromAngleSize(Model.OurRobots[s.Id].Angle.Value * Math.PI / 180, 1)),
                    inner = Vector2D.InnerProduct(Vector2D.FromAngleSize(Model.OurRobots[s.Id].Angle.Value * Math.PI / 180, 1), Model.BallState.Location - Model.OurRobots[s.Id].Location),
                    inter = new Circle(Model.OurRobots[s.Id].Location, margR).Intersect(new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(1)))
                });
                double min = double.MaxValue;
                if (Model.Opponents.Count > 0)
                    min = Model.Opponents.Min(o => o.Value.Location.DistanceFrom(Model.BallState.Location));

                if (robot_Ang.Count() > 0 &&
                    robot_Ang.Any(a => a.leftLine.Distance(Model.BallState.Location) + a.rightLine.Distance(Model.BallState.Location)
                        <= RobotParameters.OurRobotParams.Diameter + margRD && a.inner > 0 && (a.inter.Count > 0 || Model.BallState.Speed.Size < 0.5) && Model.OurRobots[a.Id].Location.DistanceFrom(Model.BallState.Location) < margD))
                    counter++;
                else
                    counter = 0;

                if (counter >= 20)
                {
                    //  counter = 0;
                    var firstId = robot_Ang.FirstOrDefault(s => (s.leftLine.Distance(Model.BallState.Location) + s.rightLine.Distance(Model.BallState.Location)) <= RobotParameters.OurRobotParams.Diameter + margRD
                        && s.inner > 0 && (s.inter.Count > 0 || Model.BallState.Speed.Size < 0.5) && Model.OurRobots[s.Id].Location.DistanceFrom(Model.BallState.Location) < margD);
                    int? i = null;
                    if (firstId != null)
                        i = firstId.Id;
                    else
                        counter = 0;
                    return i;
                }
            }
            else
                counter = 0;
            return null;
            #region Last
            //int? ourBallOwner = null;
            //Circle c = new Circle(Position2D.Zero, 0.5);
            //List<int> listID = new List<int>();
            //double minDist = double.MaxValue;
            //double dist = 0;
            //double d = 0.5;
            //foreach (var item in Model.OurRobots)
            //{
            //    if (!Model.GoalieID.HasValue || (Model.GoalieID.HasValue && Model.GoalieID.Value != item.Key))
            //    {
            //        c = new Circle(item.Value.Location, 0.75);
            //        if (c.Center.DistanceFrom(Model.BallState.Location) < d)
            //        {
            //            if (GPInfo.OurTeam.CatchBallLines.ContainsKey(item.Key) && GPInfo.OurTeam.CatchBallLines[item.Key].Count > 0)
            //            {
            //                dist = GPInfo.OurTeam.CatchBallLines[item.Key].First().Head.DistanceFrom(Model.BallState.Location);
            //                if (dist < c.Radious)
            //                {
            //                    Vector2D robotBallVec = Model.BallState.Location - item.Value.Location;
            //                    Vector2D robot = Vector2D.FromAngleSize(item.Value.Angle.Value * Math.PI / 180, 1);
            //                    double ang = Math.Abs(Vector2D.AngleBetweenInDegrees(robotBallVec, robot));

            //                    if (ang <= 45)
            //                    {

            //                        if (dist < minDist)
            //                        {
            //                            minDist = dist;
            //                            ourBallOwner = item.Key;
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            //lastOurBallOwner = ourBallOwner;
            //return ourBallOwner;
            #endregion
        }
        Dictionary<int, int> exteraIDs = new Dictionary<int, int>();
        List<int> lastIds = new List<int>();
        int? lastOppGoaliId = null, lastOppGoalieIdTemp = null;
        int oppGoaliCounter = 0;
        bool goaliemode = false;
        private int? GetOppGoaliID(WorldModel Model)
        {
            goaliemode = true;
            double goalidist = 7;
            int? goaliID = null;
            double dist, dist2;


            foreach (var item in Model.Opponents)
            {
                if (GameParameters.IsInDangerousZone(item.Value.Location, true, 0, out dist, out dist2))
                {
                    if (dist < goalidist)
                    {
                        goalidist = dist;
                        goaliID = item.Key;
                    }
                }
            }
          
            if (!lastOppGoalieIdTemp.HasValue || !Model.Opponents.ContainsKey(lastOppGoalieIdTemp.Value) || (goaliID.HasValue && goaliID.Value != lastOppGoalieIdTemp.Value))
            {
                oppGoaliCounter = 0;
                lastOppGoalieIdTemp = goaliID;
            }

            if (lastOppGoalieIdTemp.HasValue && Model.Opponents.ContainsKey(lastOppGoalieIdTemp.Value)
                && GameParameters.IsInDangerousZone(Model.Opponents[lastOppGoalieIdTemp.Value].Location, true, 0, out dist, out dist2))
            {
                if (oppGoaliCounter++ >= 60)
                    lastOppGoaliId = lastOppGoalieIdTemp;
            }
            else
            {
                lastOppGoalieIdTemp = null;
                oppGoaliCounter = 0;
            }
            if (goaliemode)
            {

                if (lastOppGoaliId.HasValue && goaliID.HasValue && lastOppGoaliId.Value != goaliID.Value)
                {
                    return lastOppGoaliId.Value;
                }
                return goaliID;
            }
            if (lastOppGoaliId.HasValue)
                return lastOppGoaliId;
            
            if (lastOppGoalieIdTemp.HasValue)
                return lastOppGoalieIdTemp;

            return goaliID;
        }
        private Dictionary<int, float> CalculateWithBallOwner(List<WorldModel> model, Dictionary<int,SingleObjectState> Opponents, GVector2D GlobalBallSpeed, GPosition2D BallOwner, GamePlannerInfo GPInfo, GamePlannerInfo LastGPInfo)
        {
            int tmpCount = 0;
            

            foreach (var item in Opponents)
            {
               
                opps[tmpCount] = (float)item.Value.Location.X;
                opps[tmpCount + GPUParams.maxRobotCount] = (float)item.Value.Location.Y;
                tmpCount++;
            }

            GPPlanner.GPlannerScore(opps, Opponents.Count, Phi, Kdx, Kdy);
            Dictionary<int, float> scr = new Dictionary<int, float>();
            if (Opponents.Count > 0)
            {
                double[,] phiT = new double[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        phiT[i, j] = Math.Exp((-(BallOwner.X - (i - 1) * Field.X / 3) * (BallOwner.X - (i - 1) * Field.X / 3) / sigmaxt) - ((BallOwner.Y + (j - 1) * Field.Y / 3) * (BallOwner.Y + (j - 1) * Field.Y / 3) / sigmayt));
                int robotidx = 0;
                double phi;
                double kdx;
                double kdy;
                foreach (var item in Opponents)
                {
                    if (!GPInfo.OppTeam.GoaliID.HasValue || (GPInfo.OppTeam.GoaliID.HasValue && GPInfo.OppTeam.GoaliID.Value != item.Key))
                    {
                        double sumphiT = 0, sumphi = 0, sumkdx = 0, sumkdy = 0;
                        double[,] tmp = new double[3, 3], dtmpx = new double[3, 3], dtmpy = new double[3, 3];
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                int idx = i * 3 + j + 1;
                                double f_un = 0, dfx_un = 0, dfy_un = 0;

                                for (int m = 0; m < data[idx].Count; m++)
                                {

                                    if (FLAG)
                                    {
                                        phi = Phi[robotidx * maxCount * data.Count + (idx - 1) * maxCount + m];
                                        kdx = Kdx[robotidx * maxCount * data.Count + (idx - 1) * maxCount + m];
                                        kdy = Kdy[robotidx * maxCount * data.Count + (idx - 1) * maxCount + m];
                                    }
                                    else
                                    {
                                        phi = Math.Exp(-(item.Value.Location.X - data[idx][m].Robot.X) * (item.Value.Location.X - data[idx][m].Robot.X) / sigmax
                                        - (item.Value.Location.Y - data[idx][m].Robot.Y) * (item.Value.Location.Y - data[idx][m].Robot.Y) / sigmay);

                                        kdx = (-2 * ((item.Value.Location.X - data[idx][m].Robot.X) / sigmax) * phi);
                                        kdy = (-2 * ((item.Value.Location.Y - data[idx][m].Robot.Y) / sigmay) * phi);
                                    }
                                    f_un += (phi * data[idx][m].PosScore);
                                    dfx_un += (kdx * data[idx][m].PosScore);
                                    dfy_un += (kdy * data[idx][m].PosScore);

                                    sumphi += phi;
                                    sumkdx += kdx;
                                    sumkdy += kdy;

                                }
                                tmp[i, j] = f_un / sumphi;
                                dtmpx[i, j] = (dfx_un * sumphi - f_un * sumkdx) / (sumphi * sumphi);
                                dtmpy[i, j] = (dfy_un * sumphi - f_un * sumkdy) / (sumphi * sumphi);
                                //DrawingObjects.AddObject("dscorex" + item.Key+i.ToString()+j.ToString(), new StringDraw("dtmpx["+i+", "+j+"]"+Math.Round(dtmpx[i, j], 3).ToString(), item.Value.Location + new Vector2D(iii, -.7)));
                                //iii -= 0.1;
                                //DrawingObjects.AddObject("dscorey" + item.Key + i.ToString() + j.ToString(), new StringDraw("dtmpy[" + i + ", " + j + "]" + Math.Round(dtmpy[i, j],3).ToString(), item.Value.Location + new Vector2D(iii, -.7)));
                                //iii -= 0.1;
                                //DrawingObjects.AddObject("dscore" + item.Key + i.ToString() + j.ToString(), new StringDraw("d[" + i + ", " + j + "]" + Math.Round((dtmpx[i, j] * item.Value.Speed.X + dtmpy[i, j] * item.Value.Speed.Y),3).ToString(), item.Value.Location + new Vector2D(iii, -.7)));
                                //iii -= 0.2;
                                tmp[i, j] += (alpha * (dtmpx[i, j] * item.Value.Speed.X + dtmpy[i, j] * item.Value.Speed.Y));
                                
                                sumphi = 0;
                                sumkdx = 0;
                                sumkdy = 0;
                            }
                        }

                        scr[item.Key] = 0;

                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                scr[item.Key] += (float)(phiT[i, j] * tmp[i, j]);
                                sumphiT += phiT[i, j];
                            }
                        }
                        scr[item.Key] /= (float)sumphiT;
                    }
                    else if (GPInfo.OppTeam.GoaliID.HasValue && GPInfo.OppTeam.GoaliID.Value == item.Key)
                        scr[item.Key] = 0;
                    if (scr[item.Key] > 0.985f)
                        scr[item.Key] = 0.985f;
                    #region comment
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    for (int j = 0; j < 6; j++)
                    //    {

                    //          //tmp[0, 0] += (float)(Coef.OppField.P1[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));
                    //          //tmp[0, 1] += (float)(Coef.OppField.P2[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));
                    //          //tmp[0, 2] += (float)(Coef.OppField.P3[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));

                    //          //tmp[1, 0] += (float)(Coef.MiddleField.P1[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));
                    //          //tmp[1, 1] += (float)(Coef.MiddleField.P2[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));
                    //          //tmp[1, 2] += (float)(Coef.MiddleField.P3[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));

                    //          //tmp[2, 0] += (float)(Coef.OurField.P1[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));
                    //          //tmp[2, 1] += (float)(Coef.OurField.P2[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));
                    //          //tmp[2, 2] += (float)(Coef.OurField.P3[i, j] * Math.Pow(item.Value.Location.X, i) * Math.Pow(item.Value.Location.Y, j));

                    //          //dtmpx[0, 0] += (float)(Coef.OppField.P1[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));
                    //          //dtmpx[0, 1] += (float)(Coef.OppField.P2[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));
                    //          //dtmpx[0, 2] += (float)(Coef.OppField.P3[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));

                    //          //dtmpx[1, 0] += (float)(Coef.MiddleField.P1[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));
                    //          //dtmpx[1, 1] += (float)(Coef.MiddleField.P2[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));
                    //          //dtmpx[1, 2] += (float)(Coef.MiddleField.P3[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));

                    //          //dtmpx[2, 0] += (float)(Coef.OurField.P1[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));
                    //          //dtmpx[2, 1] += (float)(Coef.OurField.P2[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));
                    //          //dtmpx[2, 2] += (float)(Coef.OurField.P3[i, j] * i * ((i > 0) ? Math.Pow(item.Value.Location.X, i - 1) : 0) * Math.Pow(item.Value.Location.Y, j));

                    //          //dtmpy[0, 0] += (float)(Coef.OppField.P1[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));
                    //          //dtmpy[0, 1] += (float)(Coef.OppField.P2[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));
                    //          //dtmpy[0, 2] += (float)(Coef.OppField.P3[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));

                    //          //dtmpy[1, 0] += (float)(Coef.MiddleField.P1[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));
                    //          //dtmpy[1, 1] += (float)(Coef.MiddleField.P2[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));
                    //          //dtmpy[1, 2] += (float)(Coef.MiddleField.P3[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));

                    //          //dtmpy[2, 0] += (float)(Coef.OurField.P1[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));
                    //          //dtmpy[2, 1] += (float)(Coef.OurField.P2[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));
                    //          //dtmpy[2, 2] += (float)(Coef.OurField.P3[i, j] * j * (Math.Pow(item.Value.Location.X, i)) * ((j > 0) ? Math.Pow(item.Value.Location.Y, j - 1) : 0));
                    //     }

                    //}
                    #endregion
                    robotidx++;
                }

            }
            return scr;
        }
        private void CalculateWithOutBallOwner(List<WorldModel> model,WorldModel Model, Dictionary<int,SingleObjectState> Opponents, GVector2D GlobalBallSpeed, ref GamePlannerInfo GPInfo, GamePlannerInfo LastGPInfo)
        {
            WorldModel lastModel = (model.Count > 1) ? model[model.Count - 2] : null;
            float min = float.MaxValue;
            float max = float.MinValue;
            float k1 = 0.3f;
            float k2 = 0.3f;
            float k3 = 0.4f;
            float k4 = 0;
            float s = 0;
            int? minID = null;
            int? maxID = null;
            float dx = 0;
            Dictionary<int, float> scr1 = new Dictionary<int, float>(), scr2 = new Dictionary<int, float>();
            foreach (var item in Opponents.ToList())
            {
                // Console.WriteLine("Robot ID " + item.Key + ": " + GPInfo.OppTeam.TimeHeads[item.Key].First());
                dx = 0;
                float t = 4;
                if (!GPInfo.OppTeam.GoaliID.HasValue || (GPInfo.OppTeam.GoaliID.HasValue && GPInfo.OppTeam.GoaliID.Value != item.Key))
                {
                    if (GPInfo.OppTeam.TimeHeads.ContainsKey(item.Key))
                    {
                        if (GPInfo.OppTeam.TimeHeads[item.Key].Count > 0)
                        {
                            t = GPInfo.OppTeam.TimeHeads[item.Key].FirstOrDefault();
                            if (t < min)
                            {
                                min = t;
                                minID = item.Key;
                            }
                        }
                    }
                    else
                    {
                        t = 4;
                    }
                    t = (t > 4) ? 4 : t;
                    t = 1 - t / 4;

                    Vector2D v1 = item.Value.Location - Model.BallState.Location;
                    float d = (float)Math.Abs(Vector2D.AngleBetweenInDegrees(v1, GlobalBallSpeed));
                    Position2D p = ((Vector2D)GlobalBallSpeed).PrependecularPoint(Model.BallState.Location, item.Value.Location);
                    float d2 = (float)p.DistanceFrom(item.Value.Location);

                    d2 = (d > 90) ? 4 : d2;
                    d2 = (d2 > 4) ? 4 : d2;
                    d2 = 1 - d2 / 4;
                    float d3 = 0;
                    k3 = 0;


                    if (GlobalBallSpeed.Size() >= 0.5)
                    {
                        k1 = 0.5f;
                        k2 = 0.5f;
                        k4 = 0.15f;
                    }
                    else
                    {
                        k1 = 1;
                        k2 = 0;
                        k3 = 0;
                        k4 = 0.15f;

                    }

                    int c = Math.Min(4, model.Count);

                    dx = 0;
                    int count = 0;
                    for (int i = 1; i < c; i++)
                    {
                        WorldModel tmp1 = model[model.Count - i];
                        Vector2D vec;
                        if (tmp1.Opponents.ContainsKey(item.Key))
                        {
                            vec = tmp1.Opponents[item.Key].Speed;
                            double aa = -Vector2D.AngleBetweenInRadians(vec, tmp1.BallState.Location - tmp1.Opponents[item.Key].Location);
                            double size = vec.Size;
                            vec.X = size * Math.Cos(aa);
                            vec.Y = size * Math.Sin(aa);
                            dx += (float)vec.X;
                            count++;
                        }
                    }
                    if (count != 0)
                        dx /= (float)count;
                    s = k1 * t + k2 * d2 + k3 * d3 + k4 * dx;

                    if (s < 0)
                        s = 0;
                    if (s > 1)
                        s = 1;

                    scr1[item.Key] = s;
                    if (s > max)
                    {
                        max = s;
                        maxID = item.Key;
                    }
                }
                else
                    scr1[item.Key] = 0;
                GPInfo.OppTeam.Scores[item.Key] = 0;
            }
            float db = 0;
            if (maxID.HasValue)
            {
                scr2 = CalculateWithBallOwner(model, Opponents, GlobalBallSpeed, Model.Opponents[maxID.Value].Location, GPInfo, LastGPInfo);
                db = (float)(Model.BallState.Location.DistanceFrom(Model.Opponents[maxID.Value].Location));
            }
            if (db > 5)
                db = 5;
            float maxs = 0;
            if (scr1.Count > 0)
                maxs = scr1.Max(m => m.Value);
            if (maxs > 0.001)
                scr1.ToList().ForEach(f => scr1[f.Key] = f.Value / maxs);
            foreach (var item in scr2)
            {
                GPInfo.OppTeam.Scores[item.Key] = (db / 5) * scr1[item.Key] + (1 - db / 5) * scr2[item.Key];
                if (GPInfo.OppTeam.Scores[item.Key] > 0.989f)
                    GPInfo.OppTeam.Scores[item.Key] = 0.989f;
                if (maxID.HasValue && maxID.Value == item.Key)
                    GPInfo.OppTeam.Scores[item.Key] = 1;
            }
            //        DrawingObjects.AddObject(new StringDraw(Model.BallState.Speed.Size.ToString(), System.Drawing.Color.Black, Model.BallState.Location + new Vector2D(0.15, 0.15)));
        }


        public void Dispose()
        {
        }
    }
}

