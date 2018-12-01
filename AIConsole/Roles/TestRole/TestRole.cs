using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.IO;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public class TestRole : RoleBase
    {
        const bool debug = true;
        Random rand = new Random();
        const double FieldLength = 5, FieldWidth = 3;
        const int haltCount = 60;
        bool first = true;
        int count = 0;
        Position2D center = new Position2D(-3, 0);
        Position2D randomPoint = Position2D.Zero;
        int haltCounter = haltCount;
        bool newRand = false;
        HiPerfTimer ht = new HiPerfTimer();
        Dictionary<int, ActiveData> data = new Dictionary<int, ActiveData>();
        Vector2D r = new Vector2D();
        bool saved = false;
        int tetaCount = 0;
        int LCount = 0;
        int tetaCounter = 0;
        bool back = true;
        bool getTime = true;
        int Totalcounter = 0;
        int rCount = 0;
        bool Clear = true;
        bool detected = true;
        bool GetRand = true;
        Dictionary<int, ReflectData> rdatas = new Dictionary<int, ReflectData>();
        List<BallPathData> ballpath = new List<BallPathData>();
        Position2D pos2go = new Position2D();
        Position2D firstBallLoc = new Position2D();
        private bool getData = true;
        bool newData = false;
        ReflectData RData = new ReflectData();
        private bool getFirstLoc = true;
        public void GetData(WorldModel Model, int RobotID, int dataCount)
        {
            if (count < dataCount)
            {
                if (first || Model.OurRobots[RobotID].Location.DistanceFrom(randomPoint) < 0.01)
                {
                    if (!newRand)
                    {
                        haltCounter++;
                        if (haltCounter >= haltCount)
                        {
                            if (!first)
                            {
                                ht.Stop();
                                data[count] = new ActiveData()
                                {
                                    time = ht.Duration * 1000,
                                    R = r
                                };
                            }
                            randomPoint = new Position2D(FieldLength / 2 - FieldLength * rand.NextDouble(), FieldWidth / 2 - FieldWidth * rand.NextDouble());
                            first = false;
                            r = randomPoint - Model.OurRobots[RobotID].Location;
                            haltCounter = 0;
                            newRand = true;
                            count++;
                            ht = new HiPerfTimer();
                            ht.Start();
                        }
                    }
                }
                else
                    newRand = false;
                Planner.Add(RobotID, randomPoint, 0, false);
            }
            else if (!saved)
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write("RobotID\tIndex\tRSize\tRAngle\tTime\n");
                foreach (var item in data)
                {
                    sw.Write(RobotID.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.Key.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.Value.R.Size.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.Value.R.AngleInDegrees.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.Value.time.ToString() + "\t\n");
                    sw.Flush();
                }
                FileStream fs = new FileStream(@"d:\ActiveData.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();
                saved = true;
            }
        }
        int motionTimer = 0;
        bool startMotion = false;
        public void GetData(WorldModel Model, int RobotID, double Lstep, double TetaStep)
        {
            tetaCount = (int)(180 / TetaStep) + 1;
            LCount = (int)(5 / Lstep);
            if (count < LCount)
            {
                if (tetaCounter <= tetaCount)
                {
                    if (Model.OurRobots[RobotID].Location.DistanceFrom(randomPoint) < 0.03)
                    {

                        if (!back && getTime)
                        {
                            //ht.Stop();
                            startMotion = false;
                            data[Totalcounter] = new ActiveData()
                            {
                                time = motionTimer,
                                R = r
                            };
                            Totalcounter++;
                            getTime = false;
                            motionTimer = 0;
                        }
                        haltCounter++;
                        if (haltCounter >= haltCount)
                        {
                            getTime = true;
                            if (back)
                            {
                                r = Vector2D.FromAngleSize(((tetaCounter) * TetaStep) * Math.PI / 180, (count + 1) * Lstep);
                                //if (r.Size > 3)
                                //{
                                //    center = (Position2D.Zero + r.GetNormalizeToCopy(3)) - r;
                                //}
                                randomPoint = center + r;
                                haltCounter = 0;
                                tetaCounter++;
                                //ht = new HiPerfTimer();
                                //ht.Start();
                                startMotion = true;
                                back = false;
                            }
                            else
                            {

                                haltCounter = 0;
                                back = true;
                                if (tetaCounter >= tetaCount)
                                {
                                    count++;
                                    //r = Vector2D.FromAngleSize(((tetaCounter) * TetaStep) * Math.PI / 180, (count + 1) * Lstep);
                                    //if (r.Size > 3)
                                    //{
                                    //    center = (Position2D.Zero + r.GetNormalizeToCopy(3)) - r;
                                    //    randomPoint = center;
                                    //}
                                    tetaCounter = 0;
                                }
                                var tmpR = Vector2D.FromAngleSize(((tetaCounter) * TetaStep) * Math.PI / 180, (count + 1) * Lstep);
                                if (tmpR.Size > 3)
                                {
                                    center = (Position2D.Zero + tmpR.GetNormalizeToCopy(3)) - tmpR;
                                }
                                randomPoint = center;
                            }

                        }

                    }

                }
                if (startMotion)
                    motionTimer++;
                if (debug)
                {
                    DrawingObjects.AddObject(new StringDraw("startMotion " + startMotion.ToString(), new Position2D(2, 2)), "dfsadf");
                    DrawingObjects.AddObject(new StringDraw("back " + back.ToString(), new Position2D(2 + 0.2, 2)), "dfsadfsdfsd");
                    DrawingObjects.AddObject(new StringDraw("Time " + motionTimer.ToString(), new Position2D(2 + 0.4, 2)), "dfsdfdfsadf");
                }
                Planner.Add(RobotID, randomPoint, 0, false);
            }
            else if (!saved)
            {
                MemoryStream ms = new MemoryStream();
                StreamWriter sw = new StreamWriter(ms);
                sw.Write("RobotID\tIndex\tRSize\tRAngle\tTime\n");
                foreach (var item in data)
                {
                    sw.Write(RobotID.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.Key.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.Value.R.Size.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.Value.R.AngleInDegrees.ToString() + "\t");
                    sw.Flush();
                    sw.Write(item.Value.time.ToString() + "\t\n");
                    sw.Flush();
                }
                FileStream fs = new FileStream(@"d:\MotionData.txt", FileMode.Create);
                fs.Write(ms.ToArray(), 0, (int)ms.Length);
                fs.Close();
                sw.Close();
                saved = true;
            }
        }
        double Kick = 0, pass = 0;
        public void GetReflectData(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, int dataCount, ReflectStatus status, double passSpeed)
        {
            //status = ReflectStatus.Waiting;

            double kickSpeed = 8;
            if (GetRand)
            {
                pos2go = new Position2D(-(rand.NextDouble() * (2.5 - 1.7) + 1.7), rand.NextDouble() * (2 - 0.7) + 0.7);
                GetRand = false;
            }
            if (status == ReflectStatus.Waiting)
            {
                Clear = true;
                GetRand = true;
                getData = true;
                if (!detected)
                {
                    saved = false;
                    detected = true;
                    RData = Ransac(10, ballpath, Target, firstBallLoc);
                    RData.passSpeed = pass;
                    RData.KickSpeed = Kick;
                    newData = true;
                }
                Line I = new Line(RData.Intersect, RData.Intersect - RData.input)
                {
                    DrawPen = new Pen(Color.Red, 0.01f)
                };
                Line O = new Line(RData.Intersect, RData.Intersect + RData.output)
                {
                    DrawPen = new Pen(Color.Blue, 0.01f)
                };
                Line T = new Line(RData.Intersect, RData.Intersect + RData.Target)
                {
                    DrawPen = new Pen(Color.YellowGreen, 0.01f)
                };

                DrawingObjects.AddObject(I, "ReflectInput");
                DrawingObjects.AddObject(O, "ReflectOutput");
                DrawingObjects.AddObject(T, "ReflectTarget");

                Planner.Add(RobotID, new SingleWirelessCommand());
            }
            else if (status == ReflectStatus.GotoPoint)
            {
                Clear = true;
                getData = true;
                double ang = (Target - pos2go).AngleInDegrees;
                Planner.Add(RobotID, pos2go, ang, true);
            }
            else if (status == ReflectStatus.OneTouch)
            {
                if (Model.BallState.Speed.Size > 0.3)
                {
                    GetRand = true;
                    detected = false;
                    if (getData)
                    {
                        ballpath.Add(new BallPathData(Model.TimeElapsed.TotalMilliseconds, Model.BallState.Location));
                        //GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, null, false, Target, 6, false);
                    }
                    else
                        Planner.Add(RobotID, new SingleWirelessCommand());
                    if (Model.BallState.Location.X < -2.9)
                        getData = false;
                }
                else if (getFirstLoc)
                {
                    firstBallLoc = Model.BallState.Location;
                    getFirstLoc = false;
                    pass = passSpeed;
                    Kick = kickSpeed;
                }
                if (Model.BallState.Speed.Size < 0.3)
                {
                    count++;
                    if (!getData)
                    {
                    }
                }

            }
            else if (status == ReflectStatus.Add)
            {
                if (newData)
                {
                    rdatas[rdatas.Count] = RData;
                    newData = false;
                }
            }
            else if (status == ReflectStatus.Save)
            {
                if (!saved)
                {
                    MemoryStream ms = new MemoryStream();
                    StreamWriter sw = new StreamWriter(ms);
                    sw.Write("dist\tPass\tKick\tTeta\tAlfa\n");
                    foreach (var item in rdatas)
                    {
                        sw.Write(item.Value.distance + "\t");
                        sw.Flush();
                        sw.Write(item.Value.passSpeed + "\t");
                        sw.Flush();
                        sw.Write(item.Value.KickSpeed + "\t");
                        sw.Flush();
                        sw.Write(item.Value.Teta + "\t");
                        sw.Flush();
                        sw.Write(item.Value.Alfa + "\n");
                        sw.Flush();
                    }
                    FileStream fs = new FileStream(@"d:\OneTouchData.txt", FileMode.Create);
                    fs.Write(ms.ToArray(), 0, (int)ms.Length);
                    fs.Close();
                    sw.Close();
                    saved = true;
                }
            }
            if (Clear)
            {
                getFirstLoc = true;
                ballpath = new List<BallPathData>();
                Clear = false;
            }
        }

        #region Mamad Change
        public void CutBall(WorldModel Model, int RobotID, Position2D Target)
        {
            if (Model.BallState.Speed.Size > 0.3)
            {
                GetRand = true;
                if (getData)
                {
                    ballpath.Add(new BallPathData(Model.TimeElapsed.TotalMilliseconds, Model.BallState.Location));
                    //GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, null, false, Target, 6, false);
                }
                else
                    Planner.Add(RobotID, new SingleWirelessCommand());
                if (Model.BallState.Location.X < -2.9)
                    getData = false;

                if (ballpath.Count >= 3)
                {
                    detected = false;
                }
            }
            else if (getFirstLoc)
            {
                firstBallLoc = Model.BallState.Location;
                getFirstLoc = false;
            }
            if (!detected)
            {
                RData = Ransac(10, ballpath, Target, firstBallLoc);
                RData.passSpeed = pass;
                RData.KickSpeed = Kick;
                detected = true;

                Line I = new Line(RData.Intersect, RData.Intersect - RData.input)
                {
                    DrawPen = new Pen(Color.Red, 0.01f)
                };
                Line O = new Line(RData.Intersect, RData.Intersect + RData.output)
                {
                    DrawPen = new Pen(Color.Blue, 0.01f)
                };
                Line T = new Line(RData.Intersect, RData.Intersect + RData.Target)
                {
                    DrawPen = new Pen(Color.YellowGreen, 0.01f)
                };

                DrawingObjects.AddObject(I, "ReflectInput");
                DrawingObjects.AddObject(O, "ReflectOutput");
                DrawingObjects.AddObject(T, "ReflectTarget");
            }
        }

        #endregion

        int syncCounter = 0;
        Position2D syncLastTarget = Position2D.Zero;
        int execTimer = 0;
        bool syncFirst = true;
        double syncMotionTime = 0;
        double syncPassTime = 0;
        double syncLastMotionTime = 0;
        Vector2D syncRef = Vector2D.Zero;
        Position2D syncLastLoc = new Position2D();
        double syncIntegralErr = 0;
        double syncRefIntegral = 0;
        double syncErrRef = 0;
        bool syncCalcIntegral = false;
        double syncV = 0;
        double syncTa = 0;
        double syncTd = 0;
        bool syncPrint = false;
        int lastExecTimer = 0;
        bool syncFirstTimer = true;
        double KI = 1;
        Position2D syncFirstPos = new Position2D();
        public void TestSync(GameStrategyEngine engine, WorldModel Model, int RobotID, int PasserID, Position2D Target)
        {
            Position2D gotoTarget = new Position2D(-2.5, 1.5);

            double passSpeed = 3;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(Target) > 0.03)
            {
                syncCounter++;
            }
            else if (!syncPrint)
            {
                Console.WriteLine("RealMotionTime: " + syncIntegralTimer);
                syncPrint = true;
            }
            double Vmax = 3.5;
            double Amax = 5;
            int rotateDelay = 60;
            if (syncFirst)
            {
                // syncMotionTime = 0;
                // syncPassTime = 0;
                syncMotionTime = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, Target, ActiveParameters.RobotMotionCoefs);
                syncPassTime = (Model.OurRobots[PasserID].Location.DistanceFrom(Target) / passSpeed) / 0.016;
                syncPassTime += rotateDelay;
                Vector2D rotateInitVec = (Model.BallState.Location - Target).GetNormalizeToCopy(0.13);
                syncPassTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location, ActiveParameters.RobotMotionCoefs));
                //syncPassTime += Planner.GetMotionTime(Model, PasserID, rotateInitVec + Model.BallState.Location, rotateInitVec.GetNormalizeToCopy(0.13) + Model.BallState.Location);
                syncV = 0;
                Console.WriteLine("PassTime: " + syncPassTime);
                Console.WriteLine("MotionTime: " + syncMotionTime);
                syncRef = Target - Model.OurRobots[RobotID].Location;
                //execTimer = 0;
                syncFirst = false;
                syncLastLoc = Model.OurRobots[RobotID].Location;
                //    syncLastMotionTime = syncMotionTime;
                syncErrRef = Model.OurRobots[RobotID].Location.DistanceFrom(Target);
                syncCalcIntegral = false;
                double D = Vmax * Vmax / (2 * Amax);
                if (2 * D <= Model.OurRobots[RobotID].Location.DistanceFrom(Target))
                {
                    syncTa = Vmax / Amax;
                    double Tcruise = (Model.OurRobots[RobotID].Location.DistanceFrom(Target) - 2 * D) / Vmax;
                    syncTd = syncTa + Tcruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[RobotID].Location.DistanceFrom(Target) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    syncTa = vhalf / Amax;
                    syncTd = syncTa;
                }
                syncTa /= 0.016;
                syncTd /= 0.016;
                syncIntegralTimer = 0;
                KI = ((Model.OurRobots[RobotID].Location.DistanceFrom(Target)) / 5.0) * 5.5;


                syncFirstPos = Model.OurRobots[RobotID].Location;
            }
            double PathTime = 0;
            //    double robotSpeed = Math.Max(Model.OurRobots[RobotID].Speed.Size, 0.1);
            PathTime = (syncIntegralErr - syncRefIntegral) / KI;
            //syncLastMotionTime = syncTimeCurrentFrame;
            //execTimer++;
            //else if(execTimer < 180)
            //{
            //    execTimer++;
            //    //Console.WriteLine(syncCounter);
            //}
            //else if (execTimer < 360)
            //{
            //    Target = gotoTarget;
            //    if (Model.OurRobots[RobotID].Location.DistanceFrom(gotoTarget) <= 0.03)
            //        execTimer++;
            //}
            //else
            //{
            //    execTimer = 0;
            //    double time = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, Target);
            //    Console.WriteLine("Motion Time: " + time);
            //    Console.WriteLine("Sync Time: " + syncCounter);

            //    syncCounter = 0;
            //}


            if (syncPassTime > syncMotionTime)
            {
                Planner.AddRotate(Model, PasserID, Target, 0, kickPowerType.Speed, passSpeed, false, rotateDelay + (int)PathTime);
                if (rotateDelay >= syncPassTime - execTimer)
                {
                    syncCalcIntegral = true;
                }
                if (execTimer >= (syncPassTime - syncMotionTime))
                {
                    if (syncFirstTimer)
                        lastExecTimer = execTimer;
                    if (!syncFirstTimer && (execTimer - lastExecTimer) >= 55)
                    {
                        ;
                    }
                    syncFirstTimer = false;
                    lastExecTimer = execTimer;

                    Planner.Add(RobotID, Target, 0, PathType.UnSafe, false, true, false, false);
                    syncCalcIntegral = true;
                }
            }
            else
            {
                Planner.Add(RobotID, Target, 0, PathType.UnSafe, false, true, false, false);
                syncCalcIntegral = true;
                if (execTimer >= (syncMotionTime - syncPassTime))
                    Planner.AddRotate(Model, PasserID, Target, 0, kickPowerType.Speed, passSpeed, false, rotateDelay + (int)PathTime);
            }
            if (syncCalcIntegral)
            {
                Vector2D dLoc = Model.OurRobots[RobotID].Location - syncFirstPos;
                double dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, syncRef));
                syncIntegralTimer++;
                if (syncIntegralTimer < syncTa)
                    syncV += (Amax * 0.016);
                else if (syncIntegralTimer >= syncTd)
                    syncV -= (Amax * 0.016);

                if (syncV > Vmax)
                    syncV = Vmax;
                else if (syncV < 0)
                {
                    syncV = 0;
                    syncErrRef = 0;
                }
                syncErrRef -= syncV * 0.016;
                syncRefIntegral += syncErrRef;
                syncIntegralErr += (syncFirstPos.DistanceFrom(Target) - dy);


                syncLastLoc = Model.OurRobots[RobotID].Location;
                //  double syncTimeCurrentFrame = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, Target);
                //ouble dt = syncTimeCurrentFrame - syncLastMotionTime;
                //CharterData.AddData("syncTime", Color.DarkOrchid, syncIntegralErr);
                CharterData.AddData("syncRef", Color.DarkOrange, PathTime);
            }
            execTimer++;

            syncLastTarget = Target;
        }
        double extendTime = 0;
        public void TestSync2(GameStrategyEngine engine, WorldModel Model, int RobotID, int PasserID, Position2D Target)
        {
            Position2D gotoTarget = new Position2D(-2.5, 1.5);

            double passSpeed = 3;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(Target) > 0.03)
            {
                syncCounter++;
            }
            else if (!syncPrint)
            {
                Console.WriteLine("RealMotionTime: " + syncIntegralTimer);
                syncPrint = true;
            }
            double Vmax = 3.5;
            double Amax = 5;
            int rotateDelay = 60;
            if (syncFirst)
            {
                // syncMotionTime = 0;
                // syncPassTime = 0;
                syncMotionTime = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, Target, ActiveParameters.RobotMotionCoefs);
                syncPassTime = (Model.OurRobots[PasserID].Location.DistanceFrom(Target) / passSpeed) / 0.016;
                syncPassTime += rotateDelay;
                Vector2D rotateInitVec = (Model.BallState.Location - Target).GetNormalizeToCopy(0.13);
                syncPassTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location, ActiveParameters.RobotMotionCoefs));
                //syncPassTime += Planner.GetMotionTime(Model, PasserID, rotateInitVec + Model.BallState.Location, rotateInitVec.GetNormalizeToCopy(0.13) + Model.BallState.Location);
                syncV = 0;
                Console.WriteLine("PassTime: " + syncPassTime);
                Console.WriteLine("MotionTime: " + syncMotionTime);
                syncRef = Target - Model.OurRobots[RobotID].Location;
                //execTimer = 0;
                syncFirst = false;
                syncLastLoc = Model.OurRobots[RobotID].Location;
                //    syncLastMotionTime = syncMotionTime;
                syncErrRef = Model.OurRobots[RobotID].Location.DistanceFrom(Target);
                syncCalcIntegral = false;
                double D = Vmax * Vmax / (2 * Amax);
                if (2 * D <= Model.OurRobots[RobotID].Location.DistanceFrom(Target))
                {
                    syncTa = Vmax / Amax;
                    double Tcruise = (Model.OurRobots[RobotID].Location.DistanceFrom(Target) - 2 * D) / Vmax;
                    syncTd = syncTa + Tcruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[RobotID].Location.DistanceFrom(Target) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    syncTa = vhalf / Amax;
                    syncTd = syncTa;
                }
                syncTa /= 0.016;
                syncTd /= 0.016;
                syncIntegralTimer = 0;
                KI = ((Model.OurRobots[RobotID].Location.DistanceFrom(Target)) / 5.0) * 5.5;
                syncErrRef = (Model.OurRobots[RobotID].Location.DistanceFrom(Target) / syncMotionTime);

                syncFirstPos = Model.OurRobots[RobotID].Location;
            }
            //    double robotSpeed = Math.Max(Model.OurRobots[RobotID].Speed.Size, 0.1);
            // PathTime = (syncIntegralErr - syncRefIntegral) / KI;


            if (syncPassTime > syncMotionTime)
            {
                int exT = (int)(0.8 * Math.Ceiling(extendTime));
                Planner.AddRotate(Model, PasserID, Target, 0, kickPowerType.Speed, passSpeed, false, rotateDelay + exT);
                //if (rotateDelay >= syncPassTime - execTimer)
                //{
                //    syncCalcIntegral = true;
                //}
                if (execTimer >= (syncPassTime - syncMotionTime))
                {
                    if (syncFirstTimer)
                        lastExecTimer = execTimer;
                    if (!syncFirstTimer && (execTimer - lastExecTimer) >= 55)
                    {
                        ;
                    }
                    syncFirstTimer = false;
                    lastExecTimer = execTimer;

                    Planner.Add(RobotID, Target, 0, PathType.UnSafe, false, true, false, false);
                    syncCalcIntegral = true;
                }
            }
            else
            {
                int exT = (int)Math.Ceiling(extendTime);
                Planner.Add(RobotID, Target, 0, PathType.UnSafe, false, true, false, false);
                syncCalcIntegral = true;
                if (execTimer >= (syncMotionTime - syncPassTime))
                    Planner.AddRotate(Model, PasserID, Target, 0, kickPowerType.Speed, passSpeed, false, rotateDelay + exT);
            }
            if (syncCalcIntegral)
            {
                Vector2D dLoc = Model.OurRobots[RobotID].Location - syncLastLoc;
                double dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, syncRef));
                syncIntegralTimer++;
                if (syncIntegralTimer < syncTa)
                    syncV += (Amax * 0.016);
                else if (syncIntegralTimer >= syncTd)
                    syncV -= (Amax * 0.016);

                if (syncV > Vmax)
                    syncV = Vmax;
                //else if (syncV < 0)
                //{
                //    syncV = 0;
                //    syncErrRef = 0;
                //    syncIntegralTimer = 0;
                //}
                //syncErrRef = syncV * 0.016;

                //if (syncIntegralTimer < syncTd + syncTa)
                if (Model.OurRobots[RobotID].Location.DistanceFrom(Target) > 0.3)
                {
                    extendTime += (1 - (dy / Math.Max(syncErrRef, 0.0001)));
                }
                //else
                //{
                //    extendTime = extendTime + 1;
                //}

                syncLastLoc = Model.OurRobots[RobotID].Location;

                //  double syncTimeCurrentFrame = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, Target);
                //ouble dt = syncTimeCurrentFrame - syncLastMotionTime;
                //CharterData.AddData("syncTime", Color.DarkOrchid, syncIntegralErr);
                CharterData.AddData("syncRef", Color.DarkOrange, extendTime);
            }
            execTimer++;

            syncLastTarget = Target;
        }
        CutBallRole cutRole = new CutBallRole();
        Line l1 = new Line();
        Line l2 = new Line();
        Position2D cutTar = new Position2D();
        double passTime = 0;
        Vector2D sVec = Vector2D.Zero;
        public void TestSyncCut(GameStrategyEngine engine, WorldModel Model, int RobotID, int PasserID, Position2D Target)
        {
            Position2D gotoTarget = new Position2D(-2.0, 1.5);
            Target = new Position2D(0.5, -1.5);
            double passSpeed = 3;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(Target) > 0.03)
            {
                syncCounter++;
            }
            else if (!syncPrint)
            {
                Console.WriteLine("RealMotionTime: " + syncIntegralTimer);
                syncPrint = true;
            }
            double Vmax = 3.5;
            double Amax = 5;
            double rotateDelay = 60;

            if (syncFirst)
            {
                l1 = new Line(Model.OurRobots[RobotID].Location, GameParameters.OppGoalCenter)
                {
                    DrawPen = new Pen(Color.DarkMagenta, 0.01f)
                };
                l2 = new Line(Model.BallState.Location, Target)
                {
                    DrawPen = new Pen(Color.Cyan, 0.01f)
                };
                cutTar = l1.IntersectWithLine(l2).Value;
                cutTar.DrawColor = Color.DarkViolet;

                //double dx = Model.OurRobots[RobotID].Location.DistanceFrom(cutTar);
                //double tmpTacc = Vmax / Amax;
                //double tmpDxAcc = Vmax * Vmax / (2 * Amax);
                //if (dx - tmpDxAcc > 0)
                //    syncMotionTime = tmpTacc + (dx - tmpDxAcc) / Vmax;
                //else
                //    syncMotionTime = Math.Sqrt(2 * Amax * dx) / Amax;
                //syncMotionTime /= 0.016;

                passTime = 1 * ((Model.BallState.Location.DistanceFrom(cutTar) / passSpeed) / 0.016);
                syncMotionTime = Math.Max(Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, cutTar, ActiveParameters.RobotMotionCoefs) - passTime, 0);

                int searchCounter = 0;
                sVec = (cutTar - Model.OurRobots[RobotID].Location);
                double d = -sVec.Size;// Model.OurRobots[RobotID].Location.DistanceFrom(cutTar) - Vmax * passTime * 0.016;
                double ssize = sVec.Size;
                double stime = -1000;
                while (searchCounter < 10 && Math.Abs(stime - syncMotionTime) > 1)
                {
                    d *= 0.5;
                    sVec = Vector2D.FromAngleSize(sVec.AngleInRadians, ssize + d);
                    stime = Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + sVec, ActiveParameters.RobotMotionCoefs);
                    if (stime - syncMotionTime > 0)
                        ssize = sVec.Size;
                    searchCounter++;
                }

                //if (syncMotionTime > rotateDelay)
                //{
                //    rotateDelay = syncMotionTime ;
                //}

                syncPassTime = passTime;
                syncPassTime += rotateDelay;
                //Vector2D rotateInitVec = (Model.BallState.Location - Target).GetNormalizeToCopy(0.13);
                //syncPassTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                syncV = 0;
                Console.WriteLine("PassTime: " + syncPassTime);
                Console.WriteLine("MotionTime: " + syncMotionTime);
                syncRef = Target - Model.OurRobots[RobotID].Location;
                syncFirst = false;
                syncLastLoc = Model.OurRobots[RobotID].Location;

                syncCalcIntegral = false;
                double D = Vmax * Vmax / (2 * Amax);
                if (2 * D <= Model.OurRobots[RobotID].Location.DistanceFrom(cutTar))
                {
                    syncTa = Vmax / Amax;
                    double Tcruise = (Model.OurRobots[RobotID].Location.DistanceFrom(cutTar) - 2 * D) / Vmax;
                    syncTd = syncTa + Tcruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[RobotID].Location.DistanceFrom(cutTar) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    syncTa = vhalf / Amax;
                    syncTd = syncTa;
                }
                syncTa /= 0.016;
                syncTd /= 0.016;
                syncIntegralTimer = 0;
                syncErrRef = (Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OppGoalCenter) / ((double)Planner.GetMotionTime(Model, RobotID, Model.OurRobots[RobotID].Location, GameParameters.OppGoalCenter, ActiveParameters.RobotMotionCoefs)));

                syncFirstPos = Model.OurRobots[RobotID].Location;
                KI = 0.9;
            }
            DrawingObjects.AddObject("L1Sync", l1);
            DrawingObjects.AddObject("L2Sync", l2);
            DrawingObjects.AddObject("CutPosSync", cutTar);

            //int exT = (int)(KI * Math.Ceiling(extendTime));
            double exT = 0;
            Vector2D robotS = Model.OurRobots[RobotID].Location - (syncFirstPos + sVec);
            Vector2D NSvec = -sVec;

            exT = NSvec.InnerProduct(robotS) / Math.Max(0.00001, NSvec.Size);
            if (exT < 0)
                exT = 0;
            DrawingObjects.AddObject(new StringDraw("exT = " + exT.ToString(), new Position2D(0, 0)));
            DrawingObjects.AddObject(syncFirstPos + sVec);
            double rotDel = Math.Max((execTimer + exT * 10), rotateDelay);
            DrawingObjects.AddObject(new StringDraw("rotDel = " + rotDel.ToString(), new Position2D(-0.25, 0)));
            bool rotDelay = Planner.AddRotate(Model, PasserID, Target, 0, kickPowerType.Speed, passSpeed, false, (int)rotDel).IsInRotateDelay;
            if (/*execTimer >= (rotateDelay - syncMotionTime) &&*/ rotDelay && execTimer >= rotateDelay)
            {
                if (syncFirstTimer)
                    lastExecTimer = execTimer;

                syncFirstTimer = false;
                lastExecTimer = execTimer;
                cutRole.CutIt(engine, Model, RobotID, Target, 5, false);
                syncCalcIntegral = true;
            }


            if (syncCalcIntegral)
            {
                Vector2D dLoc = Model.OurRobots[RobotID].Location - syncLastLoc;
                double dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, syncRef));
                syncIntegralTimer++;
                if (syncIntegralTimer < syncTa)
                    syncV += (Amax * 0.016);
                else if (syncIntegralTimer >= syncTd)
                    syncV -= (Amax * 0.016);

                if (syncV > Vmax)
                    syncV = Vmax;
                if (Model.OurRobots[RobotID].Location.DistanceFrom(Target) > 0.3)
                {
                    extendTime += (1 - (dy / syncErrRef));
                }
                //  extendTime = 0;
                syncLastLoc = Model.OurRobots[RobotID].Location;
                CharterData.AddData("syncRef", Color.DarkOrange, extendTime);
            }
            if (rotDelay)
                execTimer++;

            syncLastTarget = Target;
        }
        double syncIntegralTimer = 0;
        private ReflectData Ransac(int randCount, List<BallPathData> ballpath, Position2D Target, Position2D FirstBallLoc)
        {
            double distTresh = 0.1;
            List<ReflectLine> datas = new List<ReflectLine>();

            while (datas.Count < 2 && ballpath.Count > 1)
            {
                Dictionary<int, Line> randLines = new Dictionary<int, Line>();
                Dictionary<int, int> linevotes = new Dictionary<int, int>();
                Dictionary<int, double> timeHead = new Dictionary<int, double>();
                Dictionary<int, double> timeTail = new Dictionary<int, double>();
                for (int j = 0; j < randCount; j++)
                {
                    List<Position2D> points = new List<Position2D>();
                    double thead = 0, ttail = 0;
                    while (points.Count < 2)
                    {
                        Position2D randp = new Position2D();
                        int idx = rand.Next(0, ballpath.Count);
                        randp = ballpath[idx].pos;
                        if (points.Count > 0 && randp != points[0])
                        {
                            points.Add(randp);
                            thead = ballpath[idx].time;
                        }
                        else if (points.Count == 0)
                        {
                            points.Add(randp);
                            ttail = ballpath[idx].time;
                        }
                    }
                    Line randLine = new Line(points[1], points[0]);
                    int vote = 0;
                    foreach (var item in ballpath)
                    {
                        Line tmpp = randLine.PerpenducilarLineToPoint(item.pos);
                        Position2D? tmpi = tmpp.IntersectWithLine(randLine);
                        if (item.pos.DistanceFrom(tmpi.Value) < distTresh)
                            vote++;
                    }
                    linevotes[j] = vote;
                    randLines[j] = randLine;
                    timeHead[j] = thead;
                    timeTail[j] = ttail;
                }
                int maxVote = int.MinValue;
                Line bestLine = new Line();
                double bestTimehead = 0, bestTimetail = 0;
                foreach (var item in linevotes)
                {
                    if (item.Value > maxVote)
                        maxVote = item.Value;
                    bestLine = randLines[item.Key];
                    bestTimehead = timeHead[item.Key];
                    bestTimetail = timeTail[item.Key];
                }
                List<int> keys2Remove = new List<int>();
                List<BallPathData> ballPathTem = ballpath.ToList();
                for (int i = 0; i < ballpath.Count; i++)
                {
                    Line tmpp = bestLine.PerpenducilarLineToPoint(ballpath[i].pos);
                    Position2D? tmpi = tmpp.IntersectWithLine(bestLine);
                    if (ballpath[i].pos.DistanceFrom(tmpi.Value) < distTresh)
                        ballPathTem.Remove(ballpath[i]);
                }

                ballpath = ballPathTem.ToList();
                datas.Add(new ReflectLine(bestLine, bestTimehead, bestTimetail));
            }
            double maxTime = double.MinValue;
            int idxx = 0;
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i].timeHead > maxTime)
                {
                    maxTime = datas[i].timeHead;
                    idxx = i;
                }
                if (datas[i].timeTail > maxTime)
                {
                    maxTime = datas[i].timeTail;
                    idxx = i;
                }
            }
            if (datas.Count > 1)
            {
                Position2D? intersect = datas[0].L.IntersectWithLine(datas[1].L);
                if (intersect.HasValue)
                {
                    Vector2D o = ((datas[idxx].L.Head - intersect.Value).Size > (datas[idxx].L.Tail - intersect.Value).Size) ? (datas[idxx].L.Head - intersect.Value) : (datas[idxx].L.Tail - intersect.Value);
                    idxx = Math.Abs(idxx - 1);
                    Vector2D i = ((datas[idxx].L.Head - intersect.Value).Size > (datas[idxx].L.Tail - intersect.Value).Size) ? (intersect.Value - datas[idxx].L.Head) : (intersect.Value - datas[idxx].L.Tail);
                    Vector2D t = Target - intersect.Value;
                    double d = firstBallLoc.DistanceFrom(intersect.Value);
                    o.NormalizeTo(t.Size);
                    i.NormalizeTo(d);
                    ReflectData rd = new ReflectData(d, 0, 0, i, o, t, intersect.Value);
                    return rd;
                }
            }
            return new ReflectData();
        }

        int inInitPosCounter = 0;
        public void RotateTest(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, double RotateAngle, int delay)
        {
            bool Debug = true;
            bool isInInitPos = false;
            double initPosTresh = 0.02;
            double RotateAngleInRadian = RotateAngle * Math.PI / 180;
            double distanceFromBall = 0.13;

            SingleObjectState robot = new SingleObjectState(Model.OurRobots[RobotID]);
            SingleObjectState ball = Model.BallState;
            Vector2D targetBallVec = Target - ball.Location;
            Position2D initPos = ball.Location + Vector2D.FromAngleSize(RotateAngleInRadian + targetBallVec.AngleInRadians, distanceFromBall);
            initPos.DrawColor = Color.RosyBrown;

            if (robot.Location.DistanceFrom(initPos) < initPosTresh)
                inInitPosCounter++;
            if (inInitPosCounter < delay)
            {
                GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, ball.Location + (ball.Location - initPos));
                isInInitPos = true;
            }
            else
            {

            }

            if (Debug)
            {
                DrawingObjects.AddObject(initPos, "initPosRotate");
                DrawingObjects.AddObject(new StringDraw("isInInitPos: " + isInInitPos.ToString(), "isInInitPos", robot.Location + new Vector2D(0.5, 0.5)), "isInInitPosString");
            }
        }

        public void CatchTest(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            GetSkill<CatchBallSkill>().Catch(engine, Model, RobotID, false, Model.OurRobots[RobotID], true);
        }

        public void RotateSpin(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D target, double passSpeed, bool isLeft)
        {
            GetSkill<ActiveRotateSkill>().Rotate(Model, RobotID, target, passSpeed, isLeft);
        }

        public void ActiveTest(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target)
        {
            bool isChip = false;
            double kick = 4;
            GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, Target);
            Planner.AddKick(RobotID, kickPowerType.Speed, isChip, kick);
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 1;
        }
        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>();
            return res;
        }
        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }



    }
    public struct ActiveData
    {
        public double time;
        public Vector2D R;
    }
    public struct ReflectData
    {
        public ReflectData(double dist, double pass, double shoot, Vector2D Input, Vector2D Output, Vector2D target, Position2D intersect)
        {
            distance = dist;
            KickSpeed = shoot;
            passSpeed = pass;
            input = Input;
            output = Output;
            Target = target;
            Intersect = intersect;
            Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(Target, -input));
            Alfa = Math.Abs(Vector2D.AngleBetweenInDegrees(output, -input));
        }
        public double distance;
        public double passSpeed;
        public double KickSpeed;
        public Vector2D input;
        public Vector2D output;
        public Vector2D Target;
        public double Teta;
        public double Alfa;
        public Position2D Intersect;
    }
    public struct BallPathData
    {
        public BallPathData(double t, Position2D P)
        {
            time = t;
            pos = P;
        }
        public double time;
        public Position2D pos;
    }
    public struct ReflectLine
    {
        public ReflectLine(Line line, double tHead, double tTail)
        {
            L = line;
            timeHead = tHead;
            timeTail = tTail;
        }
        public Line L;
        public double timeHead;
        public double timeTail;
    }
    public enum ReflectStatus
    {
        Waiting,
        GotoPoint,
        OneTouch,
        Add,
        Save
    }

}
