using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using Enterprise;

namespace MRL.SSL.AIConsole.Roles
{
    public class CutTestRole : RoleBase
    {

        MotionLearning ML;
        Position2D goBackPos = Position2D.Zero;
        Position2D target = Position2D.Zero;
        Position2D initState = Position2D.Zero;
        Vector2D error = Vector2D.Zero;
        Position2D ballInit = Position2D.Zero;
        bool go = false, turnBack = false, halt = false, load = false, save = false;
        bool motionFinished = false, saved = false, learned = false;

       
        int counter = 0, counter2 = 0, counter3 = 0, counter4 = 0;
        double errThresh = 0.06, errThreshX = 0.08;
        public double extendSize = 0.12;
        double closeLoopTresh = -1;
        double deviationRate = 0, deviation = 0, sumErr = 0;
        Position2D passTarget = Position2D.Zero;
        bool test = false, goCut = false;
        double passSpeed = 2;
        bool passed = false;
        int passCounter = 0;

        static string filename = "CutData.ct";

        public bool Load
        {
            get { return load; }
            set { load = value; }
        }
        public bool Save
        {
            get { return save; }
            set { save = value; }
        }
        public bool Halt
        {
            get { return halt; }
            set { halt = value; }
        }
        public bool TurnBack
        {
            get { return turnBack; }
            set { turnBack = value; }
        }
        public bool Go
        {
            get { return go; }
            set { go = value; }
        }
        public bool Saved
        {
            get { return saved; }
            set { saved = value; }
        }

        public static string Filename
        {
            get { return CutTestRole.filename; }
            set { CutTestRole.filename = value; }
        }

        public void Stop(WorldModel Model, int RobotID)
        {
            SingleWirelessCommand s = new SingleWirelessCommand();
            s.W = 0;
            Vector2D prev = Vector2D.Zero;
            if (Model.lastVelocity.ContainsKey(RobotID))
                prev = GameParameters.RotateCoordinates(Model.lastVelocity[RobotID], Model.OurRobots[RobotID].Angle.Value);
            s.Vx = prev.X / 1.07;
            s.Vy = prev.Y / 1.07;
            Planner.Add(RobotID, s, false);
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Test;
        }
      
        public double GetMaximumDistance(WorldModel Model, int RobotID, double aMax, double vMax, Position2D ballIni, Position2D Tar, double passS)
        {
            double dx = ballIni.DistanceFrom(Tar);
            double time = dx / passS;

            double ta = vMax / aMax;
            double res = 0;

            if (ta < time)
            {
                res = vMax * vMax / (2 * aMax);
                res += (time - ta) * vMax;
            }
            else
            {
                double v = aMax * time;
                res = v * v / (2 * aMax);
            }

            return res * 0.9;
        }
        
        public bool CheckContain(double passS, double dist, double cutDist)
        { 
            int passKey = (int)(passS * 100);
            int distKey = (int)(dist * 100);
            int cutDistKey = (int)(cutDist * 100);
            bool res = false;
            if (ML == null || ML.MotionDatas == null)
                return res;
            for (int i = passKey - 1; i <= passKey + 1; i++)
			{
                for (int j = distKey - 1; j <= distKey + 1; j++)
                {
                    for (int k = cutDistKey - 1; k <= cutDistKey + 1; k++)
                    {
                        res = ML.MotionDatas.ContainsKey(i) && ML.MotionDatas[i].ContainsKey(j) && ML.MotionDatas[i][j].ContainsKey(k);
                        if (res)
                            return res;
                    }
                }
			}
            return res;
        }
        public bool CheckContain(double passS, double dist,ref double bestPassS, ref double bestPassD)
        {
            int passKey = (int)(passS * 100);
            int distKey = (int)(dist * 100);
            bool res = false;
            if (ML == null || ML.MotionDatas == null)
                return res;
            if (ML.MotionDatas.ContainsKey(passKey) && ML.MotionDatas[passKey].ContainsKey(distKey))
                return true;
            else
            {
                SearchResult sr = FindBestMotiondata(passKey, distKey,false);
                bestPassS = sr.passSpeedKey / 100.0;
                bestPassD = sr.passDistKey / 100.0;
                return false;
            }
        }
        public bool CheckLoad()
        {
            return (ML != null && ML.Loaded);
        }
        bool saveLastMotion = false;
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (test)
                return;
            if (passed)
                passCounter++;
            DrawingObjects.AddObject(new StringDraw(passCounter.ToString(), target + new Vector2D(-0.55, 0.5)));
            if (CurrentState == (int)LearnState.Stop)
            {
                passCounter = 0;
                passed = false;
                saved = false;
                if (save)
                {
                    CurrentState = (int)LearnState.Save;
                    saveLastMotion = true;
                }
                else if (go)
                    CurrentState = (int)LearnState.Go;
                else if (turnBack)
                    CurrentState = (int)LearnState.Back;
            }
            else if (CurrentState == (int)LearnState.Go)
            {
                passed = true;     
                if (motionFinished)
                {

                    if (!test && Math.Abs(error.X) <= errThreshX && Math.Abs(error.Y) <= errThresh && !saved)
                    {
                        CurrentState = (int)LearnState.Save;
                        saveLastMotion = false;
                    }
                    else
                    {
                        CurrentState = (int)LearnState.Learn;
                    }
                    ML.ResetTimers();
                }
                else if (halt)
                {
                    CurrentState = (int)LearnState.Stop;
                    ML.ResetTimers();
                }

            }
            else if (CurrentState == (int)LearnState.Learn)
            {
               
                counter2++;
                if (halt)
                    CurrentState = (int)LearnState.Stop;
                else if (counter2 >= 120)
                {
                    CurrentState = (int)LearnState.Back;
                    counter2 = 0;
                }

            }
            else if (CurrentState == (int)LearnState.Back)
            {
                passCounter = 0;
                passed = false;
                if (Model.OurRobots[RobotID].Location.DistanceFrom(initState) < 0.02)
                    counter3++;

                if (halt || counter3 > 60)
                {
                    CurrentState = (int)LearnState.Stop;
                    counter3 = 0;
                }
                
            }
            else if (CurrentState == (int)LearnState.Save)
            {
                counter4++;
                if (counter4 > 540)
                    CurrentState = (int)LearnState.Back;
            }

            
            if (counter > 0 && CurrentState != (int)LearnState.Go)
                counter = 0;
            if (counter2 > 0 && CurrentState != (int)LearnState.Learn)
                counter2 = 0;
            if (counter3 > 0 && CurrentState != (int)LearnState.Back)
                counter3 = 0;
            if (counter4 > 0 && CurrentState != (int)LearnState.Save)
                counter4 = 0;
            if (learned && CurrentState != (int)LearnState.Learn)
                learned = false;
            if (motionFinished && CurrentState != (int)LearnState.Go)
                motionFinished = false;
            if (test && goCut && CurrentState != (int)LearnState.Go)
                goCut = false;
            passTarget = target + (target - initState).GetNormalizeToCopy(extendSize);
            if (ML != null)
                ML.PassTarget = passTarget;
            DrawingObjects.AddObject(new StringDraw(((LearnState)CurrentState).ToString(), initState + new Vector2D(0.5, 0.5)), "CurrentState");
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 1;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public void Initilize(WorldModel Model, int RobotID, Position2D Target, Position2D InitState,Position2D ballinit, double step, double PassSpeed, bool Test)
        {
            initState = InitState;
            target = initState + (Target - InitState).GetNormalizeToCopy((Target - InitState).Size - extendSize);
            test = Test;
            //if (test)
                load = true;
            passSpeed = PassSpeed;
            ballInit = ballinit;
            ML = new MotionLearning(Model, RobotID, InitState, target, ballinit, step, PassSpeed, load, CutTestRole.filename, Target,test);
        }
     
        public void Run(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
           
            DrawingObjects.AddObject(new Circle(initState, RobotParameters.OurRobotParams.Diameter / 2) { DrawPen = new Pen(Color.Red, 0.01f) }, "SyncInitState");
            DrawingObjects.AddObject(new Circle(target, RobotParameters.OurRobotParams.Diameter / 2) { DrawPen = new Pen(Color.Red, 0.01f) }, "SyncTargetState");
            DrawingObjects.AddObject(new Circle(passTarget, RobotParameters.OurRobotParams.Diameter / 2) { DrawPen = new Pen(Color.Beige, 0.01f) }, "SyncTargetStateExtend");
            DrawingObjects.AddObject(new StringDraw(error.ToString(), target + new Vector2D(0.5, 0.5)), "SyncErrorVec");
            DrawingObjects.AddObject(new StringDraw(error.Size.ToString(), target + new Vector2D(0.7, 0.5)), "SyncErrorVecSize");
            DrawingObjects.AddObject(new StringDraw(Model.BallState.Speed.Size.ToString(), GameParameters.OurGoalCenter + new Vector2D(-0.7, 0)), "BAllSpeedSize");
            DrawingObjects.AddObject(new Line(initState, passTarget) { IsShown = true, DrawPen = new Pen(Color.DarkOrchid, 0.01f) }, "syncinitTargetLine");
            DrawingObjects.AddObject(new Line(Model.BallState.Location, passTarget) { IsShown = true, DrawPen = new Pen(Color.Red, 0.01f) }, "SyncErrorLine");

            if (CurrentState == (int)LearnState.Stop)
            {
                Stop(Model, RobotID);

            }
            else if (CurrentState == (int)LearnState.Go)
            {
                if (!test)
                {
                    SingleWirelessCommand SWC = new SingleWirelessCommand();
                    motionFinished = ML.GetCommand(Model, RobotID, ref SWC);
                    if (motionFinished)
                    {
                        error = Model.BallState.Location - ballInit;
                        Vector2D refvec = (passTarget - ballInit);
                        error = GameParameters.InRefrence(error, refvec);
                        error.Y = error.Y - refvec.Size;
                        sumErr = ML.SumError * 180 / Math.PI;
                        ML.SumError = 0;
                        deviation = Vector2D.AngleBetweenInDegrees((target - initState), (Model.OurRobots[RobotID].Location - initState));
                        deviationRate = (sumErr / deviation);
                    }
                    Planner.Add(RobotID, SWC, false);
                }
                else
                {
                    motionFinished = false;
                    Vector2D robotInit = Model.OurRobots[RobotID].Location - initState;
                    double imgRobotInit = Math.Max(robotInit.InnerProduct((target - initState)) / (target - initState).Size, 0);
                    Vector2D targetVec = passTarget - Model.OurRobots[RobotID].Location;
                    Line targetLine = new Line(passTarget, Model.OurRobots[RobotID].Location);
                    Line ballLine = new Line((Model.BallState.Location + Model.BallState.Speed), Model.BallState.Location);
                    Position2D? intersectTemp = targetLine.IntersectWithLine(ballLine);
                    Position2D intersect = new Position2D();
                    bool hasInter = true;
                    if (!intersectTemp.HasValue)
                        hasInter = false;
                    else
                        intersect = intersectTemp.Value;
                    if (hasInter && Model.OurRobots[RobotID].Location.DistanceFrom(initState) > 0.6 && Model.OurRobots[RobotID].Location.DistanceFrom(intersect) < closeLoopTresh)
                        goCut = true;

                    intersect = intersect - targetVec.GetNormalizeToCopy(0.08);
                    intersect.DrawColor = Color.BlanchedAlmond;
                    if (goCut)
                    {
                        ML.go(Model, RobotID);
                        DrawingObjects.AddObject(new StringDraw("GOOOO!", target + new Vector2D(0.5, -0.5)), "GOOOO!");
                    }
                    else
                    {
                        SingleWirelessCommand SWC = new SingleWirelessCommand();
                        motionFinished = ML.GetCommand(Model, RobotID, ref SWC);
                        Planner.Add(RobotID, SWC, false);
                        DrawingObjects.AddObject(new StringDraw("GetCommand!", target + new Vector2D(0.5, -0.5)), "GOOOO!");
                    }
                    Vector2D vec = Model.OurRobots[RobotID].Location - target;
                    vec = GameParameters.InRefrence(vec, (target - initState));
                    if (vec.Y > 1)
                        motionFinished = true;
                    
                }
            }
            else if (CurrentState == (int)LearnState.Learn)
            {
                if (!learned)
                {

                    if (!test && Math.Abs(error.X) <= 0.1)
                    {
                        ML.LastMotion = (MotionData)ML.ThisMotion.Clone();
                        learned = ML.ThisMotion.Learn(Model, RobotID, error, initState, target);
                    }
                    else
                        learned = true;
                }
                Stop(Model, RobotID);
            }
            else if (CurrentState == (int)LearnState.Back)
            {
                Planner.Add(RobotID, initState, (target - initState).AngleInDegrees + 0, false);
            }
            else if (CurrentState == (int)LearnState.Save)
            {
                if (!saved)
                    saved = ML.Save(filename,saveLastMotion);

                Stop(Model, RobotID);
                DrawingObjects.AddObject(new StringDraw("SAVED!", Position2D.Zero), "SaveSyncData");
            }
        }
        bool firstCut = true;
        Position2D CutTarget = Position2D.Zero;
        int time2Pass = 0, time2Move = 0;
        MotionData CutMotionData = new MotionData();
        int timer = 0;
        bool isCut = false;
        public void CutIt(GameStrategyEngine engine, WorldModel Model, int PasserID, int CutterID, int RotateDelay, double RotateAngle, Position2D passInit,Position2D PassTarget, Position2D Target, Position2D cutInit, double passSpeed, double kickPower, kickPowerType kickType, bool isChip)
        {
            double ballSpeedTresh = 0.05;
  
            bool containPasser = Model.OurRobots.ContainsKey(PasserID);
            bool containPositioner = Model.OurRobots.ContainsKey(CutterID);
            SingleObjectState passerRobot = (containPasser) ? Model.OurRobots[PasserID] : null;
            SingleObjectState positionerRobot = (containPositioner) ? Model.OurRobots[CutterID] : null;
            timer++;
            if (!containPasser || !containPositioner)
                return;
            if (firstCut)
            {
                timer = 0;
                ML.Load(filename);
                Line RobotTargetLine = new Line(positionerRobot.Location, Target);
                Line PassLine = new Line(passInit, PassTarget);
                Position2D inter = Position2D.Zero;
                
                if (!PassLine.IntersectWithLine(RobotTargetLine, ref inter))
                    inter = PassTarget + (PassTarget - Target).GetNormalizeToCopy(0.1);
                CutTarget = inter;
                Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(0.3);
                
                time2Pass = Planner.GetRotateTime(RotateAngle);
                time2Pass += (int)(Planner.GetMotionTime(Model, PasserID, passerRobot.Location, rotateInitVec + Model.BallState.Location, ActiveParameters.RobotMotionCoefs));
                time2Pass += Planner.GetMotionTime(Model, PasserID, rotateInitVec + Model.BallState.Location, rotateInitVec.GetNormalizeToCopy(0.13) + Model.BallState.Location, ActiveParameters.RobotMotionCoefs);
                time2Pass += RotateDelay;
                time2Pass += (int)((Model.BallState.Location.DistanceFrom(CutTarget) / passSpeed) / StaticVariables.FRAME_PERIOD);
                
                double cutD = CutTarget.DistanceFrom(cutInit);
                double passD = CutTarget.DistanceFrom(passInit);

                int cutKey = (int)(cutD * 100);
                int distKey = (int)(passD * 100);
                int speedKey = (int)(passSpeed * 100);

                CutMotionData = FindBestMotiondata(cutKey, distKey, speedKey);
                
                if (CutMotionData == null)
                    return;

                double a = CutMotionData.lastA;
                double v = CutMotionData.lastVmax;
                double vf = Math.Sqrt(2 * a * cutD);

                double t = Math.Min(vf, v) / a + Math.Max(cutD - v * v / (2 * a), 0) / v;
                time2Move = (int)(t * StaticVariables.FRAME_RATE);
                isCut = false;
                firstCut = false;
            }
            int dt = Math.Abs(time2Move - time2Pass);
            bool rotate = false, go = false;
            if (time2Pass > time2Move)
            {
                rotate = true;
                if (timer > dt)
                    go = true;
            }
            else
            {
                go = true;
                if (timer > dt)
                    rotate = true;
            }
            if (Model.BallState.Speed.Size > ballSpeedTresh)
            {
                isCut = true;
            }

            if(rotate)
                Planner.AddRotate(Model, PasserID, PassTarget, RotateAngle, kickPowerType.Speed, passSpeed, false, RotateDelay);

            if (isCut)
            {
                SingleWirelessCommand SWC = new SingleWirelessCommand();
                MotionData md = new MotionData();
                //ML.GetCommand(Model, CutterID, md, (Target - cutInit), (Target - cutInit).AngleInRadians, isCut, ref SWC);
                Planner.Add(CutterID, SWC, false);
            }
            else if (go)
            {
                Planner.ChangeDefaulteParams(CutterID, false);
                Planner.SetParameter(CutterID, 3, 3);
                Planner.Add(CutterID, PassTarget, (PassTarget - cutInit).AngleInDegrees, false);
            }
        }
        double cutTreshDist = 0;
        int RotateDelay = 0;
        bool calcErr = false;
        MotionData md = new MotionData();
        bool gogocut = false;
        int tmpCutKey = 0;
        int tmpDistKey = 0;
        int tmpSpeedKey = 0;
        int tmpCutSpeedKey = 0;
        int stepIdx = 0;
        bool initialGo2point = true;
        int initCounter = 0;
        SearchResult sr = new SearchResult(0,0,0,0);
        List<SearchResult> srList = new List<SearchResult>();
        Vector2D refvec = Vector2D.Zero;
        public void CutIt(GameStrategyEngine engine, WorldModel Model, int PasserID, int CutterID, double RotateAngle, Position2D passInit, Position2D PassTarget, Position2D Target, Position2D cutInit, double passSpeed, double kickPower, kickPowerType kickType, bool isChip,bool closeloop)
        {
            double ballSpeedTresh = 0.05;

            bool containPasser = Model.OurRobots.ContainsKey(PasserID);
            bool containPositioner = Model.OurRobots.ContainsKey(CutterID);
            SingleObjectState passerRobot = (containPasser) ? Model.OurRobots[PasserID] : null;
            SingleObjectState positionerRobot = (containPositioner) ? Model.OurRobots[CutterID] : null;
           
        
            
            
            if (firstCut)
            {
               
                double bestPassS = 0, bestPassD = 0;
                Initilize(Model, CutterID, PassTarget, cutInit, passInit, 0.1, passSpeed, true);
                Line RobotTargetLine = new Line(cutInit, Target);
                Line PassLine = new Line(passInit, PassTarget);
                Position2D inter = Position2D.Zero;

                if (!PassLine.IntersectWithLine(RobotTargetLine, ref inter))
                    inter = PassTarget + (PassTarget - Target).GetNormalizeToCopy(0.1);
                CutTarget = inter;

                if (!CheckContain(passSpeed, CutTarget.DistanceFrom(passInit), ref bestPassS, ref bestPassD))
                {
                    passSpeed = bestPassS;
                    Circle tmpC = new Circle(passInit, bestPassD);
                    Line tmpL = new Line(Target, cutInit, new Pen(Color.Red, 0.01f));
                    List<Position2D> inters = tmpC.Intersect(tmpL);
                    DrawingObjects.AddObject(new Circle(CutTarget, 0.1, new Pen(Color.OrangeRed, 0.01f)));
                    DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.OrangeRed, 0.01f)));
                    Position2D tmpIntersect = PassTarget;
                    if (inters.Count == 1)
                        tmpIntersect = inters[0];
                    else if (inters.Count == 2)
                    {
                        tmpIntersect = (cutInit.DistanceFrom(inters[0]) > cutInit.DistanceFrom(inters[1])) ? inters[1] : inters[0];
                    }
                    else
                    {

                        Position2D cuttar = passInit + Vector2D.FromAngleSize((CutTarget - passInit).AngleInRadians, bestPassD);
                        cutInit = Target + (cuttar - Target).GetNormalizeToCopy((Target - cutInit).Size);
                        tmpIntersect = cuttar;
                    }
                    PassTarget = tmpIntersect;
                    DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Purple, 0.01f)));
                     refvec = (Target - cutInit);
                }
               
                double cutD = (CutTarget.DistanceFrom(cutInit) * 100);
                double passD = (CutTarget.DistanceFrom(passInit) * 100);

                tmpCutKey = (int)Math.Round(cutD);
                tmpDistKey = (int)Math.Round(passD);
                tmpSpeedKey = (int)(passSpeed * 100);
                cutTreshDist = GetMaximumDistance(Model, CutterID, 4, 3, passInit, CutTarget, passSpeed);
                isCut = false;
                firstCut = false;
                calcErr = true;
                gogocut = false;
                RotateDelay = 1;
                initCounter = 0;
                md = new MotionData();
            }
            DrawingObjects.AddObject(new Circle(CutTarget, 0.1, new Pen(Color.Plum, 0.01f)));
            if (!initialGo2point && !isCut && positionerRobot.Location.DistanceFrom(CutTarget) < 1.5 * cutTreshDist)
            {
                isCut = true;
            }
            
            if (isCut && gogocut)
            {
                SingleWirelessCommand SWC = new SingleWirelessCommand();
                try
                {
                    if (md != null && ML.GetCommand(Model, CutterID, md, stepIdx, refvec, refvec.AngleInRadians, ref SWC,closeloop))
                    {
                        if (calcErr)
                        {
                            error = Model.BallState.Location - ballInit;
                            error = GameParameters.InRefrence(error, refvec);
                            //error.Y = error.Y - refvec.Size;
                            calcErr = false;
                            Reset();
                        }
                        Stop(Model, CutterID);
                    }
                    else
                        Planner.Add(CutterID, SWC, false);
                }

                catch (Exception ex)
                {
                    Logger.Write(LogType.Exception, ex.ToString());
                }
            }
            else
            {

                if (Model.BallState.Speed.Size > ballSpeedTresh)
                {
                    gogocut = true;
                    Vector2D robotInit = CutTarget - Model.OurRobots[CutterID].Location;
                    double imgRobotInit = Math.Max(robotInit.InnerProduct(refvec) / refvec.Size, 0);
                    tmpCutKey = (int)(100 * imgRobotInit);
                    Vector2D tmp = Vector2D.FromAngleSize(Model.OurRobots[CutterID].Speed.AngleInRadians - (refvec.AngleInRadians - Math.PI / 2), Model.OurRobots[CutterID].Speed.Size);
                    tmpCutSpeedKey = (int)(tmp.Y * 100);
                    sr = FindBestMotiondata(tmpSpeedKey, tmpDistKey, tmpCutKey, tmpCutSpeedKey,closeloop);
                    md = (MotionData)ML.MotionDatas[sr.passSpeedKey][sr.passDistKey][sr.cutDistKey].Clone();
                    stepIdx = sr.stepKey;
                }
                if (!isCut)
                    RotateDelay++;
                else
                    RotateDelay = 0;
                Planner.ChangeDefaulteParams(CutterID, false);
                Planner.SetParameter(CutterID, 3, 2);
                if (initialGo2point)
                    Planner.Add(CutterID, cutInit, (Target - cutInit).AngleInDegrees, false);
                else
                    Planner.Add(CutterID, CutTarget, (Target - cutInit).AngleInDegrees, false);
                if (initialGo2point && positionerRobot.Location.DistanceFrom(cutInit) < 0.01)
                    initCounter++;
                if (initCounter > 30)
                    initialGo2point = false;
            }

            Planner.AddRotate(Model, PasserID, CutTarget, RotateAngle, kickPowerType.Speed, passSpeed+0.3, false, RotateDelay);

            DrawingObjects.AddObject(new Line(cutInit, cutInit + refvec, new Pen(Color.RoyalBlue, 0.01f)), "RefLine");
            DrawingObjects.AddObject(new Circle(CutTarget, 0.07, new Pen(Color.SteelBlue, 0.01f)), "CutTargetCircle");
            DrawingObjects.AddObject(new Circle(Target, 0.07, new Pen(Color.SteelBlue, 0.01f)), "TargetCircle");
            DrawingObjects.AddObject(new Circle(cutInit, 0.07, new Pen(Color.SteelBlue, 0.01f)), "cutInitCircle");
            DrawingObjects.AddObject(new StringDraw("gogoCut: " + gogocut.ToString(), "gogoCut", CutTarget + new Vector2D(-0.1, 0)));
            DrawingObjects.AddObject(new StringDraw("initialGo2Point: " + initialGo2point.ToString(), "initialGo2point", CutTarget + new Vector2D(-0.2, 0)));
            DrawingObjects.AddObject(new StringDraw("isCut: " + isCut.ToString(), "isCut", CutTarget + new Vector2D(-0.3, 0)));
            DrawingObjects.AddObject(new StringDraw("dist: " + positionerRobot.Location.DistanceFrom(CutTarget).ToString(), "distFromCutTarget", CutTarget + new Vector2D(-0.4, 0)));
            DrawingObjects.AddObject(new StringDraw("RotateDelay: " + RotateDelay.ToString(), "rotateDelay", CutTarget + new Vector2D(-0.5, 0)));
            DrawingObjects.AddObject(new StringDraw("SpeedKey: " + tmpSpeedKey.ToString(), "SpeedKey", CutTarget + new Vector2D(-0.6, 0)));
            DrawingObjects.AddObject(new StringDraw("DistKey: " + tmpDistKey.ToString(), "distKey", CutTarget + new Vector2D(-0.7, 0)));
            DrawingObjects.AddObject(new StringDraw("CutKey: " + tmpCutKey.ToString(), "cutKey", CutTarget + new Vector2D(-0.8, 0)));
            DrawingObjects.AddObject(new StringDraw("CutSpeedKey: " + tmpCutSpeedKey.ToString(), "cutspeedkey", CutTarget + new Vector2D(-0.9, 0)));

            DrawingObjects.AddObject(new StringDraw("srPassSpeedKey: " + sr.passSpeedKey.ToString(), "srpassspeedkey", CutTarget + new Vector2D(-1, 0)));
            DrawingObjects.AddObject(new StringDraw("srPassDistKey: " + sr.passDistKey.ToString(), "srpassDistkey", CutTarget + new Vector2D(-1.1, 0)));
            DrawingObjects.AddObject(new StringDraw("srCutDistKey: " + sr.cutDistKey.ToString(), "srcutDistkey", CutTarget + new Vector2D(-1.2, 0)));
            DrawingObjects.AddObject(new StringDraw("srStepIdx: " + stepIdx.ToString(), "srstepidx", CutTarget + new Vector2D(-1.3, 0)));
            DrawingObjects.AddObject(new StringDraw("stepCounts: " + md.stepsData.Count.ToString(), "stepcounts", CutTarget + new Vector2D(-1.4, 0)));
            DrawingObjects.AddObject(new StringDraw("maxDistCut: " + cutTreshDist.ToString(), "maxDistCut", CutTarget + new Vector2D(-1.5, 0)));
            DrawingObjects.AddObject(new StringDraw("calcError: " + calcErr.ToString(), "calcError", CutTarget + new Vector2D(-1.6, 0)));
            DrawingObjects.AddObject(new StringDraw("firstCut: " + firstCut.ToString(), "firstCut", CutTarget + new Vector2D(-1.7, 0)));
            DrawingObjects.AddObject(new StringDraw("error: " + error.ToString(), "errrror", CutTarget + new Vector2D(-1.8, 0)));
        }
        double a4cut = 0;
        double v4cut = 0;
        
        
        public void CutIt(GameStrategyEngine engine, WorldModel Model, int PasserID, int CutterID, double RotateAngle, Position2D passInit, Position2D PassTarget, Position2D Target, Position2D cutInit, double passSpeed, double kickPower, kickPowerType kickType, bool isChip,bool closeloop, int tmpf)
        {
            double ballSpeedTresh = 0.05;

            bool containPasser = Model.OurRobots.ContainsKey(PasserID);
            bool containPositioner = Model.OurRobots.ContainsKey(CutterID);
            SingleObjectState passerRobot = (containPasser) ? Model.OurRobots[PasserID] : null;
            SingleObjectState positionerRobot = (containPositioner) ? Model.OurRobots[CutterID] : null;
            
            if (firstCut)
            {

                double bestPassS = 0, bestPassD = 0;
                Initilize(Model, CutterID, PassTarget, cutInit, passInit, 0.1, passSpeed, true);
                Line RobotTargetLine = new Line(cutInit, Target);
                Line PassLine = new Line(passInit, PassTarget);
                Position2D inter = Position2D.Zero;

                if (!PassLine.IntersectWithLine(RobotTargetLine, ref inter))
                    inter = PassTarget + (PassTarget - Target).GetNormalizeToCopy(0.1);
                CutTarget = inter;

                if (!CheckContain(passSpeed, CutTarget.DistanceFrom(passInit), ref bestPassS, ref bestPassD))
                {
                    passSpeed = bestPassS;
                    Circle tmpC = new Circle(passInit, bestPassD);
                    Line tmpL = new Line(Target, cutInit, new Pen(Color.Red, 0.01f));
                    List<Position2D> inters = tmpC.Intersect(tmpL);
                    DrawingObjects.AddObject(new Circle(CutTarget, 0.1, new Pen(Color.OrangeRed, 0.01f)));
                    DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.OrangeRed, 0.01f)));
                    Position2D tmpIntersect = PassTarget;
                    if (inters.Count == 1)
                        tmpIntersect = inters[0];
                    else if (inters.Count == 2)
                    {
                        tmpIntersect = (cutInit.DistanceFrom(inters[0]) > cutInit.DistanceFrom(inters[1])) ? inters[1] : inters[0];
                    }
                    else
                    {

                        Position2D cuttar = passInit + Vector2D.FromAngleSize((CutTarget - passInit).AngleInRadians, bestPassD);
                        cutInit = Target + (cuttar - Target).GetNormalizeToCopy((Target - cutInit).Size);
                        tmpIntersect = cuttar;
                    }
                    PassTarget = tmpIntersect;
                    CutTarget = tmpIntersect;
                    DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Purple, 0.01f)));
                  
                }
                refvec = (CutTarget - cutInit);
                double cutD = (CutTarget.DistanceFrom(cutInit) * 100);
                double passD = (CutTarget.DistanceFrom(passInit) * 100);

                tmpCutKey = (int)Math.Round(cutD);
                tmpDistKey = (int)Math.Round(passD);
                tmpSpeedKey = (int)(passSpeed * 100);
                cutTreshDist = GetMaximumDistance(Model, CutterID, 4, 3, passInit, CutTarget, passSpeed);
                isCut = false;
                firstCut = false;
                calcErr = true;
                gogocut = false;
                RotateDelay = 1;
                initCounter = 0;
                md = new MotionData();
            }
            DrawingObjects.AddObject(new Circle(CutTarget, 0.1, new Pen(Color.Plum, 0.01f)));
            if (!initialGo2point && !isCut && positionerRobot.Location.DistanceFrom(CutTarget) < 1.1 * cutTreshDist)
            {
                isCut = true;
            }

            if (isCut && gogocut)
            {
                SingleWirelessCommand SWC = new SingleWirelessCommand();
                try
                {
                    int i = 0;
                    foreach (var item in srList)
                    { 
                        var tmp = ML.MotionDatas[item.passSpeedKey][item.passDistKey][item.cutDistKey];
                        if (tmp.stepsData[item.stepKey].ListData.Count > 0)
                        {
                            DrawingObjects.AddObject(new StringDraw("PassSpeedNew: " + item.passSpeedKey/100.0 , "PassSpeedNew" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                            DrawingObjects.AddObject(new StringDraw("PassDistNew: " + item.passDistKey / 100.0, "PassDistNew" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                            DrawingObjects.AddObject(new StringDraw("CutDistNew: " + item.cutDistKey / 100.0, "CutDistNew" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                            DrawingObjects.AddObject(new StringDraw("StepKeyNew: " + item.stepKey / 100.0, "StepKeyNew" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                            DrawingObjects.AddObject(new StringDraw("Vo: " + tmp.stepsData[item.stepKey].ListData[0].Speed.Y, "vos" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                            DrawingObjects.AddObject(new StringDraw("Vc: " + tmp.stepsData[item.stepKey].ListData[0].RealSpeed.Y, "vcs" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                            DrawingObjects.AddObject(new StringDraw("acc: " + tmp.stepsData[item.stepKey].ListData[0].Ay, "acc" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                        }
                    }
                    DrawingObjects.AddObject(new StringDraw("vccut: " + GameParameters.InRefrence(Model.OurRobots[CutterID].Speed, refvec).Y, "vccut" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                    DrawingObjects.AddObject(new StringDraw("vocut: " + GameParameters.InRefrence(Model.lastVelocity[CutterID], refvec).Y, "vocut" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                    DrawingObjects.AddObject(new StringDraw("Vcutkey: " + tmpCutSpeedKey / 100.0, "Vcutkey" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                    DrawingObjects.AddObject(new StringDraw("a4cut: " + a4cut, "a4cut" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));
                    DrawingObjects.AddObject(new StringDraw("v4cut: " + v4cut, "v4cut" + i, new Position2D(2.2, 0) + new Vector2D((i++) * -0.1, 0)));

                    if (ML.GetCommand(Model, CutterID, a4cut, v4cut, refvec, refvec.AngleInRadians, ref SWC,closeloop))
                    {
                        if (calcErr)
                        {
                            error = Model.BallState.Location - ballInit;
                            error = GameParameters.InRefrence(error, (CutTarget - ballInit));
                            error.Y = error.Y - (CutTarget - ballInit).Size;
                            calcErr = false;
                            Reset();
                        }
                        Stop(Model, CutterID);
                    }
                    else
                        Planner.Add(CutterID, SWC, false);
                }

                catch (Exception ex)
                {
                    Logger.Write(LogType.Exception, ex.ToString());
                }
            }
            else
            {

                if (Model.BallState.Speed.Size > ballSpeedTresh)
                {
                    gogocut = true;
                    Vector2D robotInit = CutTarget - Model.OurRobots[CutterID].Location;
                    double imgRobotInit = Math.Max(robotInit.InnerProduct(refvec) / refvec.Size, 0);
                    tmpCutKey = (int)(100 * imgRobotInit);

                    Vector2D tmp = Vector2D.Zero;
                    if (!closeloop)
                        tmp = GameParameters.InRefrence(Model.lastVelocity[CutterID], refvec);//Vector2D.FromAngleSize(Model.lastVelocity[CutterID].Speed.AngleInRadians - (refvec.AngleInRadians - Math.PI / 2), Model.OurRobots[CutterID].Speed.Size);
                    else
                        tmp = GameParameters.InRefrence(Model.OurRobots[CutterID].Speed, refvec);
                    DrawingObjects.AddObject(new StringDraw("vccut: " + GameParameters.InRefrence(Model.OurRobots[CutterID].Speed, refvec).Y, "vccut" + 0, new Position2D(2.2, 0) + new Vector2D( -0.1, 0)));
                    DrawingObjects.AddObject(new StringDraw("vocut: " + GameParameters.InRefrence(Model.lastVelocity[CutterID], refvec).Y, "vocut" + 0, new Position2D(2.2, 0) + new Vector2D(-0.2, 0)));
                    tmpCutSpeedKey = (int)(tmp.Y * 100);
                    srList = FindBestMotiondata(tmpSpeedKey, tmpDistKey, tmpCutKey, tmpCutSpeedKey, true,closeloop);
                    a4cut = 0;
                    double sum = 0;
                    for (int i = 0; i < srList.Count; i++)
                    {
                        MotionData tmpMd = (MotionData)ML.MotionDatas[srList[i].passSpeedKey][srList[i].passDistKey][srList[i].cutDistKey].Clone();
                        if (tmpMd.stepsData[srList[i].stepKey].ListData.Count > 0)
                        {
                            if (!closeloop)
                                sum += Math.Abs(tmpMd.stepsData[srList[i].stepKey].ListData[0].Speed.Y - (tmpCutSpeedKey / 100.0));
                            else
                                sum += Math.Abs(tmpMd.stepsData[srList[i].stepKey].ListData[0].RealSpeed.Y - (tmpCutSpeedKey / 100.0));
                        }
                    }
                    for (int i = 0; i < srList.Count; i++)
                    {
                        MotionData tmpMd = (MotionData)ML.MotionDatas[srList[i].passSpeedKey][srList[i].passDistKey][srList[i].cutDistKey].Clone();
                        if (srList.Count == 1)
                        {
                            a4cut = tmpMd.stepsData[srList[i].stepKey].ListData[0].Accel.Y;
                        }
                        else
                        {
                            if (tmpMd.stepsData[srList[i].stepKey].ListData.Count > 0)
                            {
                                if (!closeloop)
                                    a4cut += (1 - Math.Abs(tmpMd.stepsData[srList[i].stepKey].ListData[0].Speed.Y - (tmpCutSpeedKey / 100.0)) / sum) * tmpMd.stepsData[srList[i].stepKey].ListData[0].Accel.Y;
                                else
                                    a4cut += (1 - Math.Abs(tmpMd.stepsData[srList[i].stepKey].ListData[0].RealSpeed.Y - (tmpCutSpeedKey / 100.0)) / sum) * tmpMd.stepsData[srList[i].stepKey].ListData[0].Accel.Y;
                            }
                        }
                    }
                    a4cut *= 0.5;
                    if (srList.Count > 0)
                        v4cut = ML.MotionDatas[srList[0].passSpeedKey][srList[0].passDistKey][srList[0].cutDistKey].lastVmax;
                    else
                        v4cut = 0;
                    //stepIdx = sr.stepKey;
                }
                if (!isCut)
                    RotateDelay++;
                else
                    RotateDelay = 0;
                Planner.ChangeDefaulteParams(CutterID, false);
                Planner.SetParameter(CutterID, 3, 2);
                if (initialGo2point)
                    Planner.Add(CutterID, cutInit, (Target - cutInit).AngleInDegrees, false);
                else
                    Planner.Add(CutterID, CutTarget, (Target - cutInit).AngleInDegrees, false);
                if (initialGo2point && positionerRobot.Location.DistanceFrom(cutInit) < 0.01)
                    initCounter++;
                if (initCounter > 30)
                    initialGo2point = false;
            }

            Planner.AddRotate(Model, PasserID, CutTarget, RotateAngle, kickPowerType.Speed, passSpeed + 0.4, false, RotateDelay);

            DrawingObjects.AddObject(new Line(cutInit, cutInit + refvec, new Pen(Color.RoyalBlue, 0.01f)), "RefLine");
            DrawingObjects.AddObject(new Circle(CutTarget, 0.07, new Pen(Color.SteelBlue, 0.01f)), "CutTargetCircle");
            DrawingObjects.AddObject(new Circle(Target, 0.07, new Pen(Color.SteelBlue, 0.01f)), "TargetCircle");
            DrawingObjects.AddObject(new Circle(cutInit, 0.07, new Pen(Color.SteelBlue, 0.01f)), "cutInitCircle");
            DrawingObjects.AddObject(new StringDraw("gogoCut: " + gogocut.ToString(), "gogoCut", CutTarget + new Vector2D(-0.1, 0)));
            DrawingObjects.AddObject(new StringDraw("initialGo2Point: " + initialGo2point.ToString(), "initialGo2point", CutTarget + new Vector2D(-0.2, 0)));
            DrawingObjects.AddObject(new StringDraw("isCut: " + isCut.ToString(), "isCut", CutTarget + new Vector2D(-0.3, 0)));
            DrawingObjects.AddObject(new StringDraw("dist: " + positionerRobot.Location.DistanceFrom(CutTarget).ToString(), "distFromCutTarget", CutTarget + new Vector2D(-0.4, 0)));
            DrawingObjects.AddObject(new StringDraw("RotateDelay: " + RotateDelay.ToString(), "rotateDelay", CutTarget + new Vector2D(-0.5, 0)));
            DrawingObjects.AddObject(new StringDraw("SpeedKey: " + tmpSpeedKey.ToString(), "SpeedKey", CutTarget + new Vector2D(-0.6, 0)));
            DrawingObjects.AddObject(new StringDraw("DistKey: " + tmpDistKey.ToString(), "distKey", CutTarget + new Vector2D(-0.7, 0)));
            DrawingObjects.AddObject(new StringDraw("CutKey: " + tmpCutKey.ToString(), "cutKey", CutTarget + new Vector2D(-0.8, 0)));
            DrawingObjects.AddObject(new StringDraw("CutSpeedKey: " + tmpCutSpeedKey.ToString(), "cutspeedkey", CutTarget + new Vector2D(-0.9, 0)));

            DrawingObjects.AddObject(new StringDraw("srPassSpeedKey: " + sr.passSpeedKey.ToString(), "srpassspeedkey", CutTarget + new Vector2D(-1, 0)));
            DrawingObjects.AddObject(new StringDraw("srPassDistKey: " + sr.passDistKey.ToString(), "srpassDistkey", CutTarget + new Vector2D(-1.1, 0)));
            DrawingObjects.AddObject(new StringDraw("srCutDistKey: " + sr.cutDistKey.ToString(), "srcutDistkey", CutTarget + new Vector2D(-1.2, 0)));
            DrawingObjects.AddObject(new StringDraw("srStepIdx: " + stepIdx.ToString(), "srstepidx", CutTarget + new Vector2D(-1.3, 0)));
            DrawingObjects.AddObject(new StringDraw("stepCounts: " + md.stepsData.Count.ToString(), "stepcounts", CutTarget + new Vector2D(-1.4, 0)));
            DrawingObjects.AddObject(new StringDraw("maxDistCut: " + cutTreshDist.ToString(), "maxDistCut", CutTarget + new Vector2D(-1.5, 0)));
            DrawingObjects.AddObject(new StringDraw("calcError: " + calcErr.ToString(), "calcError", CutTarget + new Vector2D(-1.6, 0)));
            DrawingObjects.AddObject(new StringDraw("firstCut: " + firstCut.ToString(), "firstCut", CutTarget + new Vector2D(-1.7, 0)));
            DrawingObjects.AddObject(new StringDraw("error: " + error.ToString(), "errrror", CutTarget + new Vector2D(-1.8, 0)));
        }
        private void Reset()
        {
            firstCut = true;
            ML.ResetTimers();
        }
        private MotionData FindBestMotiondata(int cutKey, int distKey, int speedKey)
        {
            if (ML == null || ML.MotionDatas == null||ML.MotionDatas.Count < 1)
                return null;

            int rSpeedKey = ML.MotionDatas.OrderBy(o => Math.Abs(o.Key - speedKey)).First().Key;
            
            if (ML.MotionDatas[rSpeedKey].Count < 1)
                return null;
            
            int rDistKey = ML.MotionDatas[rSpeedKey].OrderBy(o => Math.Abs(o.Key - distKey)).First().Key;
            
            if (ML.MotionDatas[rSpeedKey][rDistKey].Count < 1)
                return null;
            
            int rCutKey = ML.MotionDatas[rSpeedKey][distKey].OrderBy(o => Math.Abs(o.Key - cutKey)).First().Key;

            return ML.MotionDatas[rSpeedKey][rDistKey][rCutKey];
        }
        private SearchResult FindBestMotiondata(int speedKey, int distKey, int cutkey, int cutspeedkey, bool closeloop)
        {
            if (ML == null || ML.MotionDatas == null || ML.MotionDatas.Count < 1 ||
                (closeloop && (ML.OmidTableCloseLoop == null || ML.OmidTableCloseLoop.Count < 1)) || (!closeloop && (ML.OmidTableOpenLoop == null || ML.OmidTableOpenLoop.Count < 1)))
                return null;
            
            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> tmpDic;
            if (!closeloop)
                tmpDic = ML.OmidTableOpenLoop.OrderBy(o => Math.Abs(o.Key - speedKey)).ToDictionary(k => k.Key, v => v.Value);
            else
                tmpDic = ML.OmidTableCloseLoop.OrderBy(o => Math.Abs(o.Key - speedKey)).ToDictionary(k => k.Key, v => v.Value);
            foreach (var item in tmpDic.Keys.ToList())
            {
                tmpDic[item] = tmpDic[item].OrderBy(o => Math.Abs(o.Key - distKey)).ToDictionary(k => k.Key, v => v.Value);
                foreach (var item2 in tmpDic[item].Keys.ToList())
                {
                    tmpDic[item][item2] = tmpDic[item][item2].OrderBy(o => Math.Abs(o.Key - cutkey)).ToDictionary(k => k.Key, v => v.Value);
                    foreach (var item3 in tmpDic[item][item2].Keys.ToList())
                    {
                        tmpDic[item][item2][item3] = tmpDic[item][item2][item3].OrderBy(o => Math.Abs(o.Key - cutspeedkey)).ToDictionary(k => k.Key, v => v.Value);
                    }
                }
            }

            int rSpeedKey = tmpDic.First().Key;

            if (tmpDic[rSpeedKey].Count < 1)
                return null;

            int rDistKey = tmpDic[rSpeedKey].First().Key;
            
            if (tmpDic[rSpeedKey][rDistKey].Count < 1)
                return null;

            int rCutKey = tmpDic[rSpeedKey][rDistKey].First().Key;
            
            if (tmpDic[rSpeedKey][rDistKey][rCutKey].Count < 1)
                return null;
            
            int rCutSpeedKey = tmpDic[rSpeedKey][rDistKey][rCutKey].First().Key;

            if (tmpDic[rSpeedKey][rDistKey][rCutKey][rCutSpeedKey].Count < 1)
                return null;
            
            var tmp = tmpDic[rSpeedKey][rDistKey][rCutKey][rCutSpeedKey].First();
            //stepIdx = tmp.stepKey;
            return tmp;
        }
        private List<SearchResult> FindBestMotiondata(int speedKey, int distKey, int cutkey, int cutspeedkey, bool f,bool closeloop)
        {
            if (ML == null || ML.MotionDatas == null || ML.MotionDatas.Count < 1 ||
                  (closeloop && (ML.OmidTableCloseLoop == null || ML.OmidTableCloseLoop.Count < 1)) || (!closeloop && (ML.OmidTableOpenLoop == null || ML.OmidTableOpenLoop.Count < 1)))
                return null;

             Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> tmpDic;
            if (!closeloop)
                tmpDic = ML.OmidTableOpenLoop.OrderBy(o => Math.Abs(o.Key - speedKey)).ToDictionary(k => k.Key, v => v.Value);
            else
                tmpDic = ML.OmidTableCloseLoop.OrderBy(o => Math.Abs(o.Key - speedKey)).ToDictionary(k => k.Key, v => v.Value);

            foreach (var item in tmpDic.Keys.ToList())
            {
                tmpDic[item] = tmpDic[item].OrderBy(o => Math.Abs(o.Key - distKey)).ToDictionary(k => k.Key, v => v.Value);
                foreach (var item2 in tmpDic[item].Keys.ToList())
                {
                    tmpDic[item][item2] = tmpDic[item][item2].OrderBy(o => Math.Abs(o.Key - cutkey)).ToDictionary(k => k.Key, v => v.Value);
                    foreach (var item3 in tmpDic[item][item2].Keys.ToList())
                    {
                        tmpDic[item][item2][item3] = tmpDic[item][item2][item3].OrderBy(o => Math.Abs(o.Key - cutspeedkey)).ToDictionary(k => k.Key, v => v.Value);
                    }
                }
            }

            int rSpeedKey = tmpDic.First().Key;

            if (tmpDic[rSpeedKey].Count < 1)
                return null;

            int rDistKey = tmpDic[rSpeedKey].First().Key;

            if (tmpDic[rSpeedKey][rDistKey].Count < 1)
                return null;

            int rCutKey = tmpDic[rSpeedKey][rDistKey].First().Key;
            
            int lessKey = -1, greatKey = -1; 

            foreach (var item in tmpDic[rSpeedKey][rDistKey][rCutKey].Keys.ToList())
            {
                if ((cutspeedkey - item) < 0)
                    greatKey = item;
                if ((cutspeedkey - item) >= 0)
                    lessKey = item;
                if (lessKey != -1 && greatKey != -1)
                    break;
            }
            

            if (tmpDic[rSpeedKey][rDistKey][rCutKey].Count < 1)
                return new List<SearchResult>();

            List<SearchResult> resList = new List<SearchResult>();
            if (lessKey != -1 && tmpDic[rSpeedKey][rDistKey][rCutKey][lessKey].Count >= 1)
                resList.Add(tmpDic[rSpeedKey][rDistKey][rCutKey][lessKey].First());
            if (greatKey != -1 && tmpDic[rSpeedKey][rDistKey][rCutKey][greatKey].Count >= 1)
                resList.Add(tmpDic[rSpeedKey][rDistKey][rCutKey][greatKey].First());
          
            return resList;
        }
        private SearchResult FindBestMotiondata(int speedKey, int distKey, bool closeloop)
        {
            if (ML == null || ML.MotionDatas == null || ML.MotionDatas.Count < 1 ||
                 (closeloop && (ML.OmidTableCloseLoop == null || ML.OmidTableCloseLoop.Count < 1)) || (!closeloop && (ML.OmidTableOpenLoop == null || ML.OmidTableOpenLoop.Count < 1)))
                return null;

            Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> tmpDic;
            if (!closeloop)
                tmpDic = ML.OmidTableOpenLoop.OrderBy(o => Math.Abs(o.Key - speedKey)).ToDictionary(k => k.Key, v => v.Value);
            else
                tmpDic = ML.OmidTableCloseLoop.OrderBy(o => Math.Abs(o.Key - speedKey)).ToDictionary(k => k.Key, v => v.Value);

            foreach (var item in tmpDic.Keys.ToList())
            {
                tmpDic[item] = tmpDic[item].OrderBy(o => Math.Abs(o.Key - distKey)).ToDictionary(k => k.Key, v => v.Value);
            }

            int rSpeedKey = tmpDic.First().Key;

            if (tmpDic[rSpeedKey].Count < 1)
                return null;

            int rDistKey = tmpDic[rSpeedKey].First().Key;

            return new SearchResult(rSpeedKey, rDistKey, 0, 0);
        }
        enum LearnState
        {
            Stop,
            Go,
            Learn,
            Back,
            Save
        }

    }
    [Serializable]
    class MotionLearning
    {
        const double MaxIntegral = 5000;
        #region NonSerilizedFeilds
        [NonSerialized]
        BinaryFormatter formatter;
        [NonSerialized]
        Position2D initState = Position2D.Zero;
        [NonSerialized]
        Position2D targetState = Position2D.Zero;
        [NonSerialized]
        Vector2D RefrenceVec = Vector2D.Zero;
        [NonSerialized]
        Position2D ballInit = Position2D.Zero;
        [NonSerialized]
        public Position2D PassTarget = Position2D.Zero;
        [NonSerialized]
        Vector2D lastV = Vector2D.Zero;
        [NonSerialized]
        Vector2D v = Vector2D.Zero;
        [NonSerialized]
        Vector2D A = Vector2D.Zero;
        [NonSerialized]
        Vector2D lA = Vector2D.Zero;
        [NonSerialized]
        Position2D lastPos = Position2D.Zero;
        [NonSerialized]
        MotionData thisMotion = new MotionData();
        [NonSerialized]
        MotionData lastMotion = new MotionData();

       
        #endregion
        #region Feilds

        Dictionary<int, Dictionary<int,Dictionary<int, MotionData>>> motionDatas;
        Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> omidTableOpenLoop;
        Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> omidTableCloseLoop;
        Dictionary<int, Dictionary<int, List<SearchResult>>> aminTable;

      
        double  passTarY = 0, passTarX = 0, iErr = 0.0,
            lastdist=0, lastErr = 0, lastErrX = 0, iErrX = 0, DefrentionalT = 0, LastDr = 0, IntegralT2 = 0,
            sumError = 0, lastPIDangular = 0, Ierr = 0, lastErrY = 0, lastError = 0, learningRateV = 1, learningRateAccel = 1,
            errTresh = 0.0, initX = 0, initY = 0, targetX = 0, targetY = 0;

        int stepCounter = 0, accelCounter = 0, timeCounter = 0, counterLine = 0,passSpeed = 0,distance = 0,di = 0;
        bool first = true, firstInref = true, firstInCloseLoop = true, loaded = false, timeClaculated = false;
        int timer = 0;
        public bool Loaded
        {
            get { return loaded; }
            set { loaded = value; }
        }
        #endregion
        #region Properties
        public Position2D InitState
        {
            get { return initState; }
            set
            {
                initState = value;

                lastdist = initState.DistanceFrom(targetState);

                RefrenceVec = targetState - initState;
            }
        }
        public Position2D TargetState
        {
            get { return targetState; }
            set
            {
                targetState = value;
                lastdist = initState.DistanceFrom(targetState);
                RefrenceVec = targetState - initState;
                //stepsData =  ReCalculateCoefs(initState, targetState);
            }
        }
        internal Dictionary<int, Dictionary<int,Dictionary<int, MotionData>>> MotionDatas
        {
            get { return motionDatas; }
        }
        public Dictionary<int, Dictionary<int, List<SearchResult>>> AminTable
        {
            get { return aminTable; }
            set { aminTable = value; }
        }

        public Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> OmidTableOpenLoop
        {
            get { return omidTableOpenLoop; }
            set { omidTableOpenLoop = value; }
        }
        public Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> OmidTableCloseLoop
        {
            get { return omidTableCloseLoop; }
            set { omidTableCloseLoop = value; }
        }
        public double SumError
        {
            get { return sumError; }
            set { sumError = value; }
        }

        public MotionData ThisMotion
        {
            get { return thisMotion; }
            set { thisMotion = value; }
        }
        public MotionData LastMotion
        {
            get { return lastMotion; }
            set { lastMotion = value; }
        }
        #endregion

        public MotionLearning(WorldModel Model, int RobotID, Position2D init, Position2D target, Position2D ballInit, double step, double PassSpeed, bool load, string filename,Position2D passTar,bool test)
        {
            distance = (int)(ballInit.DistanceFrom(passTar) * 100.0);
            passSpeed = (int)(PassSpeed * 100.0);
            di = (int)(init.DistanceFrom(target) * 100);

            DrawingObjects.AddObject(new StringDraw("PassSpeed: " + passSpeed.ToString(), new Position2D(2.3, 1)));
            DrawingObjects.AddObject(new StringDraw("passDist: " + distance.ToString(), new Position2D(2.4, 1)));
            DrawingObjects.AddObject(new StringDraw("cutDist: " + di.ToString(), new Position2D(2.5, 1)));

            formatter = new BinaryFormatter();


            bool ld = (load) ? Load(filename) : false;
            loaded = true;
            initState = init;
            targetState = target;
            PassTarget = passTar;
            initX = initState.X;
            initY = initState.Y;
            targetX = targetState.X;
            targetY = targetState.Y;
            passTarX = PassTarget.X;
            passTarY = PassTarget.Y;
            RefrenceVec = targetState - initState;
            this.ballInit = ballInit;
            int cc = 0;

            foreach (var item in motionDatas.Keys)
            {
                foreach (var item2 in motionDatas[item].Keys)
                {
                    foreach (var item3 in motionDatas[item][item2].Keys)
                    {
                        cc++;
                    }
                }
            }

            if (!test && (!ld || ld && (motionDatas == null || !motionDatas.ContainsKey(passSpeed) || !motionDatas[passSpeed].ContainsKey(distance) || !motionDatas[passSpeed][distance].ContainsKey(di) || motionDatas[passSpeed][distance][di].stepsData.Count == 0)))
            {
                if (motionDatas == null)
                    motionDatas = new Dictionary<int, Dictionary<int,Dictionary<int, MotionData>>>();
                if (!motionDatas.ContainsKey(passSpeed))
                    motionDatas[passSpeed] = new Dictionary<int,Dictionary<int,MotionData>>();
                if (!motionDatas[passSpeed].ContainsKey(distance))
                    motionDatas[passSpeed][distance] = new Dictionary<int,MotionData>();
                if (!motionDatas[passSpeed][distance].ContainsKey(di))
                    motionDatas[passSpeed][distance][di] = new MotionData();
                motionDatas[passSpeed][distance][di].Distance = initState.DistanceFrom(targetState);
                motionDatas[passSpeed][distance][di].step = step;
                motionDatas[passSpeed][distance][di].lastA = motionDatas[passSpeed][distance][di].aMax;
                motionDatas[passSpeed][distance][di].lastVmax = motionDatas[passSpeed][distance][di].vMax;
                motionDatas[passSpeed][distance][di].inputValue = InputFunction(Model, RobotID, passTar, ballInit, PassSpeed);
                motionDatas[passSpeed][distance][di].lastdist = initState.DistanceFrom(targetState);
                motionDatas[passSpeed][distance][di].stepsData = motionDatas[passSpeed][distance][di].ReCalculateCoefs(motionDatas[passSpeed][distance][di].aMax, motionDatas[passSpeed][distance][di].vMax, init, target);
             
            }
            omidTableOpenLoop = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>>();
            omidTableCloseLoop = new Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>>();
            aminTable = new Dictionary<int, Dictionary<int, List<SearchResult>>>();
            FillTable(motionDatas, ref omidTableOpenLoop, ref omidTableCloseLoop);
            FillTable(motionDatas, ref aminTable);
            if (!test)
                thisMotion = motionDatas[passSpeed][distance][di];
            
       
        }
        private void FillTable(Dictionary<int, Dictionary<int, Dictionary<int, MotionData>>> mdata, ref Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> tableO, ref Dictionary<int, Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>> tableC)
        {
            foreach (var pskey in mdata.Keys)
            {
                foreach (var pdkey in mdata[pskey].Keys)
                {
                    foreach (var cdkey in mdata[pskey][pdkey].Keys)
                    {
                        int count = mdata[pskey][pdkey][cdkey].stepsData.Count;
                        for (int i = 0; i < count; i++)
                        {
                            int dc = (int)(((count - i) * mdata[pskey][pdkey][cdkey].step) * 100.0);
                            for (int j = 0; j < mdata[pskey][pdkey][cdkey].stepsData[i].ListData.Count; j++)
                            {
                                int v0 = (int)(mdata[pskey][pdkey][cdkey].stepsData[i].ListData[j].Speed.Y * 100);
                                if (!tableO.ContainsKey(pskey))
                                    tableO[pskey] = new Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>();
                                if (!tableO[pskey].ContainsKey(pdkey))
                                    tableO[pskey][pdkey] = new Dictionary<int, Dictionary<int, List<SearchResult>>>();
                                if (!tableO[pskey][pdkey].ContainsKey(dc))
                                    tableO[pskey][pdkey][dc] = new Dictionary<int, List<SearchResult>>();
                                if (!tableO[pskey][pdkey][dc].ContainsKey(v0))
                                    tableO[pskey][pdkey][dc][v0] = new List<SearchResult>();
                                tableO[pskey][pdkey][dc][v0].Add(new SearchResult(pskey, pdkey, cdkey, Math.Min(i , count)));

                                v0 = (int)(mdata[pskey][pdkey][cdkey].stepsData[i].ListData[j].RealSpeed.Y * 100);
                                if (!tableC.ContainsKey(pskey))
                                    tableC[pskey] = new Dictionary<int, Dictionary<int, Dictionary<int, List<SearchResult>>>>();
                                if (!tableC[pskey].ContainsKey(pdkey))
                                    tableC[pskey][pdkey] = new Dictionary<int, Dictionary<int, List<SearchResult>>>();
                                if (!tableC[pskey][pdkey].ContainsKey(dc))
                                    tableC[pskey][pdkey][dc] = new Dictionary<int, List<SearchResult>>();
                                if (!tableC[pskey][pdkey][dc].ContainsKey(v0))
                                    tableC[pskey][pdkey][dc][v0] = new List<SearchResult>();
                                tableC[pskey][pdkey][dc][v0].Add(new SearchResult(pskey, pdkey, cdkey, Math.Min(i, count)));
                            }
                            
                        }
                    }             
                }
            }
        }
        private void FillTable(Dictionary<int, Dictionary<int, Dictionary<int, MotionData>>> mdata, ref Dictionary<int,Dictionary<int,List<SearchResult>>> table)
        {
            foreach (var pskey in mdata.Keys)
            {
                foreach (var pdkey in mdata[pskey].Keys)
                {
                    foreach (var cdkey in mdata[pskey][pdkey].Keys)
                    {
                        int count = mdata[pskey][pdkey][cdkey].stepsData.Count;
                        for (int i = 0; i < count; i++)
                        {
                            for (int j = 0; j < mdata[pskey][pdkey][cdkey].stepsData[i].ListData.Count; j++)
                            {
                                int v0 = (int)(mdata[pskey][pdkey][cdkey].stepsData[i].ListData[j].RealSpeed.Y * 100);
                                int t = (int)(mdata[pskey][pdkey][cdkey].stepsData[i].ListData[j].bTime2target * 100);
                                if (!table.ContainsKey(v0))
                                    table[v0] = new Dictionary<int, List<SearchResult>>();
                                if (!table[v0].ContainsKey(t))
                                    table[v0][t] = new List<SearchResult>();
                                table[v0][t].Add(new SearchResult(pskey, pdkey, cdkey, i));
                            }

                        }
                    }
                }
            }
        }
        public double InputFunction(WorldModel Model, int RobotID, Position2D Target,Position2D BallInit, double PassSpeed)
        {
            double dx = BallInit.DistanceFrom(Target);
            double time = dx / PassSpeed;
            return time;
            
        }
        private double XController(WorldModel Model, int RobotID,Vector2D refVec)
        {
            double Kp = 6, Ki = 1.4, Kd = 0.2, lamda = 0.98;
            Vector2D robotInint = Model.OurRobots[RobotID].Location - initState;
            Vector2D Ref = GameParameters.InRefrence(refVec, refVec);
            Vector2D robotInitInRef = GameParameters.InRefrence(robotInint, refVec);
            double err = -(Ref - robotInitInRef).X;
            double maxOut = 0.7;
            
            double dErr = (err - lastErrX) / StaticVariables.FRAME_PERIOD;
            iErrX = iErrX * lamda + err * StaticVariables.FRAME_PERIOD;
            lastErrX = err;
            if (iErrX > MaxIntegral)
                iErrX = MaxIntegral;
            else if (iErrX < -MaxIntegral)
                iErrX = -MaxIntegral;
            CharterData.AddData("syncXErr", Color.Aqua,err);
            
            double outPut = Kp * err + Ki * iErrX + Kd * dErr;
            if (outPut > maxOut)
                outPut = maxOut;
            else if (outPut < -maxOut)
                outPut = -maxOut;
            return outPut;
        }
        private double AngleController(WorldModel Model, int RobotID, double angle, ref double err)
        {
            double Kp = 5, Ki = 0.05, Kd = 0.32, lamda = 0.99;
           
            err = angle -  Model.OurRobots[RobotID].Angle.Value* Math.PI / 180;
           
            if (err > Math.PI)
                err -= 2 * Math.PI;
            else if (err < -Math.PI)
                err += 2 * Math.PI;
            sumError += err;
            double dErr = (err - lastErr) / StaticVariables.FRAME_PERIOD;
            iErr = iErr * lamda + err * StaticVariables.FRAME_PERIOD;
            lastErr = err;
            if (iErr > MaxIntegral)
                iErr = MaxIntegral;
            else if (iErr < -MaxIntegral)
                iErr = -MaxIntegral;
            CharterData.AddData("syncAngleErr2", err * 180 / Math.PI);
            if (counterLine % 5 == 0)
            {
                DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 0.3)) { IsShown = true, DrawPen = new Pen(Color.Blue, 0.01f) }, "syncLineAngle" + counterLine.ToString());
                DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize((targetState - initState).AngleInRadians, 0.3)) { IsShown = true , DrawPen = new Pen(Color.Red,0.01f)}, "syncLineAngleRef" + counterLine.ToString());
            }
            if (counterLine >= 100)
                counterLine = 0;

            counterLine++;

            double outPut = Kp * err + Ki * iErr + Kd * dErr;
            return outPut;
        }
        bool firstInCut = true;
        double firstImgRobotInit = 0;
        double v0InCut = 0, vxInCut = 0;
        bool firstInSync = true;
        int cutState = -1, lastCutState = -1;
        double d0InCut = 0, dInCut = 0;
        double a0InCut = 0;
        double lastDInCut = 0;
        bool stayInTable = false;
        bool startSync = false;
        public bool GetCommand(WorldModel Model, int RobotID, double a4cut, double v4cut, Vector2D refrence, double angle, ref SingleWirelessCommand SWC, bool closeloop)
        {

            firstInCloseLoop = true;
            Vector2D robotInit = Model.OurRobots[RobotID].Location - initState;
            double imgRobotInit = Math.Max(robotInit.InnerProduct(refrence) / refrence.Size, 0);
            Vector2D tmp = Vector2D.Zero;
            if (!closeloop)
                tmp = GameParameters.InRefrence(Model.lastVelocity[RobotID], refrence);
            else
                tmp = GameParameters.InRefrence(Model.OurRobots[RobotID].Speed, refrence);
            if (firstInCut)
            {
                firstImgRobotInit = imgRobotInit;
                lastV = GameParameters.InRefrence(Model.lastVelocity[RobotID], refrence);
                firstInCut = false;
            }
             
            if (imgRobotInit - refrence.Size > errTresh)
            {
                return true;
            }
            else
            {
                double err = 0;
                double W = -AngleController(Model, RobotID, (targetState - initState).AngleInRadians, ref err);
                Vector2D accel = new Vector2D(0, a4cut);
                lastV += accel * StaticVariables.FRAME_PERIOD;
                lastV.X = XController(Model, RobotID, refrence);
                lastV.NormalizeTo(Math.Min(v4cut, lastV.Size));
                v = Vector2D.FromAngleSize((refrence).AngleInRadians - Math.PI / 2 + lastV.AngleInRadians, lastV.Size);
                v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);
                SWC = new SingleWirelessCommand(v, W, false, 0, 0, false, false, false);
                return false;
            }

        }
        public bool GetCommand(WorldModel Model, int RobotID, MotionData md, int stepIdx, Vector2D refrence, double angle, ref SingleWirelessCommand SWC, bool closeloop)
        {
            bool res = false;
            firstInCloseLoop = true;
            double tresh = 0.05, maxA = 7;
            Vector2D robotInit = Model.OurRobots[RobotID].Location - initState;
            double imgRobotInit = Math.Max(robotInit.InnerProduct(refrence) / refrence.Size, 0);
            Vector2D tmp = Vector2D.Zero;
            if (closeloop)
                tmp = GameParameters.InRefrence(Model.OurRobots[RobotID].Speed, refrence);//Vector2D.FromAngleSize(Model.OurRobots[RobotID].Speed.AngleInRadians - (refrence.AngleInRadians - Math.PI / 2), Model.OurRobots[RobotID].Speed.Size);
            else
                tmp = GameParameters.InRefrence(Model.lastVelocity[RobotID], refrence);
            if (firstInCut)
            {
                firstImgRobotInit = imgRobotInit;
                lastV = GameParameters.InRefrence(Model.lastVelocity[RobotID], refrence);//Vector2D.FromAngleSize(Model.lastVelocity[RobotID].AngleInRadians - (refrence.AngleInRadians - Math.PI / 2), Model.lastVelocity[RobotID].Size);
                d0InCut = imgRobotInit;
                firstInCut = false;
            }

            Vector2D vReal = Vector2D.Zero;
            stepCounter = stepIdx + (int)((imgRobotInit - firstImgRobotInit) / md.step);
            double accelDuty = (imgRobotInit - firstImgRobotInit) - (int)((imgRobotInit - firstImgRobotInit) * md.step);
            if (stepCounter > md.stepsData.Count - 1)
            {
                res = true;
            }
            if (!res)
            {
                accelCounter = 0;
                double sumDuty = 0;
                for (int i = 0; i < md.stepsData[stepCounter].ListData.Count; i++)
                {
                    accelCounter = i;
                    sumDuty += md.stepsData[stepCounter].ListData[i].Duty;
                    if (closeloop)
                    {
                            vReal = md.stepsData[stepCounter].ListData[i].RealSpeed;
                    }
                    else
                    {
                            vReal = md.stepsData[stepCounter].ListData[i].Speed;
                    }
                    if (accelDuty < sumDuty)
                        break;
                }
                if (accelCounter > md.stepsData[stepCounter].ListData.Count - 1)
                    res = true;
            }
            
            double err = 0;
            double W = -AngleController(Model, RobotID, angle, ref err);
            Vector2D v = Model.lastVelocity[RobotID];
            
            Vector2D accel = Vector2D.Zero;
            try
            {
                if (!res)
                {
                    if (cutState  < 3 && vReal.Y - tmp.Y > 0)
                    {
                        accel = new Vector2D(0, maxA);
                        cutState = 0;
                    }
                    else if (cutState < 3 && vReal.Y - tmp.Y < 0)
                    {
                        accel = new Vector2D(0, -maxA);
                        cutState = 1;
                    }

                    if (lastCutState != -1 && cutState < 3 && cutState != lastCutState)
                    {
                        //double vmax = md.vMaxReal;
                        //double am =  md.aMaxReal;
                        //v0InCut = vReal.Y;
                        //double a = (cutState == 0) ? -5 : 5;
                        //double d = Math.Max(imgRobotInit - d0InCut, 0);
                        //double d2 = 0;
                        //double v2 = vmax;
                        //if ((vmax * vmax - v0InCut * v0InCut) / (2 * am) >= d)
                        //{
                        //    v2 = Math.Sqrt(v0InCut * v0InCut + 2 * am * d);      
                        //}
                        //d0InCut = imgRobotInit;
                        //lastDInCut = d;
                        //vxInCut = Math.Sqrt(a * (d2 - d) + (v0InCut * v0InCut + vmax * vmax) / 2);
                        a0InCut = (cutState == 0) ? -5 : 5;
                        dInCut = /*Math.Sign(a0InCut) **/ Math.Max(imgRobotInit - d0InCut, 0);
                        d0InCut = imgRobotInit;
                        lastDInCut = dInCut;
                        cutState = 3;
                    }
                    if (cutState == 3)
                    {
                        double a = -a0InCut;
                        double vmax = md.vMaxReal;
                        double am = md.aMaxReal;
                        double vv = (a * vReal.Y - am * tmp.Y) / (a - am);
                        double t1 = 0;
                        double t2=0;
                        double t3=0;
                        double d1=0;
                        double d2=0;
                        double d3=0;
                        double dd = 0;
                        if (Math.Abs(tmp.Y - vReal.Y) >= 0.4)
                            startSync = true;
                        if (vv >= vmax)
                        {
                            t1 = (vmax - tmp.Y) / a;
                            t2 = (vmax - vReal.Y) / am;
                            t3 = t1 - t2;
                            d1 = t1 * (vmax + tmp.Y) / 2;
                            d2 = t2 * (vmax + vReal.Y) / 2;
                            d3 = t3 * vmax;
                            dd = Math.Abs(d2 + d3 - d1);
                          
                        }
                        else
                        {
                            t1 = (vv - tmp.Y) / a;
                            d1 = t1 * (vv + tmp.Y) / 2;
                            d2 = t1 * (vv + vReal.Y) / 2;
                            dd = Math.Abs(d2 - d1);

                        }
                        double tmpd = 0;
                        if (Math.Abs(dd - dInCut) <= 0.05)
                            cutState = 4;
                        else
                        {
                            accel = new Vector2D(0, maxA * Math.Sign(a0InCut));
                            tmpd = dInCut - Math.Max(imgRobotInit - d0InCut, 0);
                            
                        }
                        DrawingObjects.AddObject(new StringDraw("t1: " + t1.ToString(), "t1", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.1,0)));
                        DrawingObjects.AddObject(new StringDraw("t2: " + t2.ToString(), "t2", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.2, 0)));
                        DrawingObjects.AddObject(new StringDraw("t3: " + t3.ToString(), "t3", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.3, 0)));
                        DrawingObjects.AddObject(new StringDraw("d1: " + d1.ToString(), "d1", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.4, 0)));
                        DrawingObjects.AddObject(new StringDraw("d2: " + d2.ToString(), "d2", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.5, 0)));
                        DrawingObjects.AddObject(new StringDraw("d3: " + d3.ToString(), "d3", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.6, 0)));
                        DrawingObjects.AddObject(new StringDraw("dd: " + dd.ToString(), "dd", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.7, 0)));
                        DrawingObjects.AddObject(new StringDraw("ax: " + accel.Y.ToString(), "ax", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.8, 0)));
                        DrawingObjects.AddObject(new StringDraw("vmax: " + vmax.ToString(), "vmax", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-0.9, 0)));
                        DrawingObjects.AddObject(new StringDraw("amax: " + am.ToString(), "amax", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-1, 0)));
                        DrawingObjects.AddObject(new StringDraw("vv: " + vv.ToString(), "vv", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-1.1, 0)));
                        DrawingObjects.AddObject(new StringDraw("dincut: " + dInCut.ToString(), "dincut", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-1.2, 0)));
                        DrawingObjects.AddObject(new StringDraw("tmp.y: " + tmp.Y.ToString(), "tmp.yy", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-1.3, 0)));
                        DrawingObjects.AddObject(new StringDraw("vreal.y: " + vReal.Y.ToString(), "vreal.yy", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-1.4, 0)));
                        DrawingObjects.AddObject(new StringDraw("tmpd: " + tmpd.ToString(), "tmpd", new Position2D(0, GameParameters.OppLeftCorner.Y) + new Vector2D(-1.5, 0)));
                    }
                    if (cutState == 4 && vReal.Y - tmp.Y > tresh)
                    {
                        accel = new Vector2D(0, maxA );
                    }
                    else if (cutState == 4 && vReal.Y - tmp.Y < -tresh)
                    {
                        accel = new Vector2D(0, -maxA);
                    }
                    else if (cutState == 4)
                    {
                        accel = md.stepsData[stepCounter].ListData[accelCounter].Accel;
                        //if (!stayInTable && Math.Abs(Math.Max(imgRobotInit - d0InCut, 0) - lastDInCut) > 2 * tresh)
                        //    cutState = -1;
                        //else
                        //    stayInTable = true;
                    }
                    CharterData.AddData("commmandVref", lastV.Y);
                    lastV += accel * StaticVariables.FRAME_PERIOD;
                    lastV.X = XController(Model, RobotID, refrence);
                    lastV.NormalizeTo(Math.Min(md.lastVmax, lastV.Size));

                    v = Vector2D.FromAngleSize(refrence.AngleInRadians - Math.PI / 2 + lastV.AngleInRadians, lastV.Size);
                }
                else
                {
                    ;
                }
            }

            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());
            }
            lastCutState = cutState;
            DrawingObjects.AddObject(new StringDraw("res: " + res.ToString(), "resbool", GameParameters.OurGoalCenter));
            DrawingObjects.AddObject(new StringDraw("vReal.Y: " + vReal.Y.ToString(), "vreal.y", GameParameters.OurGoalCenter + new Vector2D(-0.1, 0)));
            DrawingObjects.AddObject(new StringDraw("lastV.Y: " + lastV.Y.ToString(), "lastv.y", GameParameters.OurGoalCenter + new Vector2D(-0.2, 0)));
            DrawingObjects.AddObject(new StringDraw("tmp.Y: " + tmp.Y.ToString(), "tmp.y", GameParameters.OurGoalCenter + new Vector2D(-0.3, 0)));
            DrawingObjects.AddObject(new StringDraw("cutState: " + cutState.ToString(), "cutState", GameParameters.OurGoalCenter + new Vector2D(-0.4, 0)));
            DrawingObjects.AddObject(new StringDraw("accel.Y: " + accel.Y.ToString(), "accel.y", GameParameters.OurGoalCenter + new Vector2D(-0.5, 0)));
            //DrawingObjects.AddObject(new StringDraw("res: " + res.ToString(), "resbool", GameParameters.OppGoalCenter));
            DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + v) { DrawPen = new Pen(Color.Tomato, 0.01f) });
            DrawingObjects.AddObject(new Line(Position2D.Zero, Position2D.Zero + lastV) { DrawPen = new Pen(Color.Tomato, 0.01f) });
            DrawingObjects.AddObject(new Line(Position2D.Zero, Position2D.Zero + refrence) { DrawPen = new Pen(Color.SkyBlue, 0.01f) });
            v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);
            SWC = new SingleWirelessCommand(v, W, false, 0, 0, false, false, false);

            if (imgRobotInit - targetState.DistanceFrom(initState) > errTresh)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        double tmpNxtR = 0, tmpNxtB = 0, tmpNxt = 0, tmpNxtBD = 0, tmpNxtBt2t = 0, tmpNxtRt2t = 0;
        int lastStepCounter = -1, lastAccelCounter = -1;
        public bool GetCommand(WorldModel Model, int RobotID, ref SingleWirelessCommand SWC)
        {
            bool res = false;
            firstInCloseLoop = true;
            int time = 0;

            Vector2D robotInit = Model.OurRobots[RobotID].Location - initState;
            double imgRobotInit = Math.Max(robotInit.InnerProduct(RefrenceVec) / RefrenceVec.Size, 0);
       
            stepCounter = (int)(imgRobotInit / thisMotion.step);
            double accelDuty = imgRobotInit - stepCounter * thisMotion.step;
            if (stepCounter > thisMotion.stepsData.Count - 1)
            {
                res = true;
            }
            if (!res )
            {
                accelCounter = 0;
                double sumDuty = 0;
                for (int i = 0; i < thisMotion.stepsData[stepCounter].ListData.Count; i++)
                {
                    accelCounter = i;
                    sumDuty += thisMotion.stepsData[stepCounter].ListData[i].Duty;
                    if (accelDuty < sumDuty)
                        break;
                }
                if (accelCounter > thisMotion.stepsData[stepCounter].ListData.Count - 1)
                    res = true;
            }
            double err = 0;
            double W =  -AngleController(Model, RobotID, (targetState - initState).AngleInRadians, ref err);
            Vector2D v = Model.lastVelocity[RobotID];

            if (!res)
            {
                time = thisMotion.stepsData[stepCounter].ListData[accelCounter].Time;
                Vector2D accel = thisMotion.stepsData[stepCounter].ListData[accelCounter].Accel;
                lastV += accel * StaticVariables.FRAME_PERIOD;
                lastV.X = XController(Model, RobotID,RefrenceVec);
                lastV.NormalizeTo(Math.Min(thisMotion.lastVmax, lastV.Size));

                if (lastAccelCounter == -1 || lastStepCounter == -1)
                {
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].Speed = new Vector2D(0, 0);
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].RealSpeed = new Vector2D(0, 0);
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].BallSpeed = new Vector2D(0, 0);
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].bTime2target = 0;
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].rTime2target = 0;
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].ballDistance = Model.BallState.Location.DistanceFrom(PassTarget);
                }
                else if (lastAccelCounter != accelCounter || stepCounter != lastStepCounter)
                {
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].Speed = new Vector2D(0, tmpNxt);
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].RealSpeed = new Vector2D(0, tmpNxtR);
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].BallSpeed = new Vector2D(0, tmpNxtB);
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].bTime2target = tmpNxtBt2t;
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].rTime2target = tmpNxtRt2t;
                    thisMotion.stepsData[stepCounter].ListData[accelCounter].ballDistance = tmpNxtBD;
                }

                tmpNxt = lastV.Y ;
                tmpNxtR = GameParameters.InRefrence(Model.OurRobots[RobotID].Speed, RefrenceVec).Y;
                
                tmpNxtB = GameParameters.InRefrence(Model.BallState.Speed, (PassTarget - ballInit)).Y;
                v = Vector2D.FromAngleSize((RefrenceVec).AngleInRadians - Math.PI / 2 + lastV.AngleInRadians, lastV.Size);
                
                tmpNxtBD = Model.BallState.Location.DistanceFrom(PassTarget);
                tmpNxtBt2t = timer * StaticVariables.FRAME_PERIOD;
                tmpNxtRt2t = timer * StaticVariables.FRAME_PERIOD;
                
                lastStepCounter = stepCounter;
                lastAccelCounter = accelCounter;
            }
            else
            {
                ;
            }
            DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + v) { DrawPen = new Pen(Color.Tomato, 0.01f) });
            v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);
            SWC = new SingleWirelessCommand(v, W, false, 0, 0, false, false, false);
            timer++;
            if (imgRobotInit - targetState.DistanceFrom(initState) > errTresh)
            {
                if (!timeClaculated)
                {
                    double totalTime = timer * StaticVariables.FRAME_PERIOD;
                    timeClaculated = true;
                    for (int i = 0; i < thisMotion.stepsData.Count; i++)
                    {
                        for (int j = 0; j < thisMotion.stepsData[i].ListData.Count; j++)
                        {
                            thisMotion.stepsData[i].ListData[j].bTime2target = totalTime - thisMotion.stepsData[i].ListData[j].bTime2target;
                            thisMotion.stepsData[i].ListData[j].rTime2target = totalTime - thisMotion.stepsData[i].ListData[j].rTime2target;
                        }
                    }
                }
                timer = 0;
                return true;
            }
            else
            {
                timeClaculated = false;
                return false;
            }
        }
        public void go(WorldModel Model, int RobotID)
        {
            Position2D Target = PassTarget;
            Vector2D targetVec = Target - Model.OurRobots[RobotID].Location;
            Line targetLine = new Line(Target, Model.OurRobots[RobotID].Location);
            Line ballLine = new Line((Model.BallState.Location + Model.BallState.Speed), Model.BallState.Location);
            Position2D? intersectTemp = targetLine.IntersectWithLine(ballLine);
            Position2D intersect = new Position2D();
            if (!intersectTemp.HasValue)
                return;
            else
                intersect = intersectTemp.Value;
            //if (intersect.X > Model.OurRobots[RobotID].Location.X)
            //{
            //    Planner.Add(RobotID, new Position2D(Model.OurRobots[RobotID].Location.X + 2, Model.OurRobots[RobotID].Location.Y), 180, true);
            //    return;
            //}

            intersect = intersect - targetVec.GetNormalizeToCopy(0.09);
            intersect.DrawColor = Color.BlanchedAlmond;
            DrawingObjects.AddObject(intersect);
            DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + targetVec.GetNormalizeToCopy(2), new Pen(Color.DarkOrange, 0.01f)), "TarRobotLine");
            DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(2), new Pen(Color.DarkMagenta, 0.01f)), "BallSpeedLine");
            double robotFinalTeta = targetVec.AngleInDegrees;
            double db = Model.BallState.Location.DistanceFrom(intersect);
            double vb = Model.BallState.Speed.Size;
            double dr = Model.OurRobots[RobotID].Location.DistanceFrom(intersect);
            double vr = vb * dr / db;
            double tb = db / vb, tr = dr / Math.Max(Model.OurRobots[RobotID].Speed.Size, 0.01);
            double kP = 1.3;
            double  dt = (tb - tr);
            DrawingObjects.AddObject(new StringDraw("db: " + db.ToString(), Model.BallState.Location + new Vector2D(-1, 1)));
            DrawingObjects.AddObject(new StringDraw("vb: " + vb.ToString(), Model.BallState.Location + new Vector2D(-1.2, 1)));
            DrawingObjects.AddObject(new StringDraw("tb: " + (tb).ToString(), Model.BallState.Location + new Vector2D(-2, 1)));
            DrawingObjects.AddObject(new StringDraw("dr: " + dr.ToString(), Model.BallState.Location + new Vector2D(-1.4, 1)));
            DrawingObjects.AddObject(new StringDraw("vr: " + vr.ToString(), Model.BallState.Location + new Vector2D(-1.6, 1)));
            DrawingObjects.AddObject(new StringDraw("tr: " + (Math.Min(dr / vr, 1)).ToString(), Model.BallState.Location + new Vector2D(-1.8, 1)));
            Vector2D roobotFinalSpeed = targetVec.GetNormalizeToCopy(kP * Math.Min(vr, 4));

            Vector2D targetRobotVec = intersect - Model.OurRobots[RobotID].Location;
            targetRobotVec = GameParameters.InRefrence(targetRobotVec, RefrenceVec);
            //    Model.BallState.
            //if ((lastSpeed - roobotFinalSpeed).Size > maxAcc / 60)
            //{
            //    roobotFinalSpeed.NormalizeTo(maxAcc / 60);
            //}
            //lastSpeed = roobotFinalSpeed;
            //
            SingleWirelessCommand SWC = Accelerate(Model, RobotID,tb, targetRobotVec.Y);
            Vector2D v = new Vector2D(SWC.Vx, SWC.Vy);
            // v.NormalizeTo(kP * vr);
            // SWC.Vy = v.Y;
            //SWC.Vx = v.X + kPx * Math.Abs(dt);

            SWC.KickPower = 150;
            SWC.isChipKick = false;
            SWC.BackSensor = false;
            Planner.Add(RobotID, SWC, false);
        }
        
        public SingleWirelessCommand Accelerate(WorldModel Model, int RobotID, Position2D Target, double TargetAngle, Vector2D CutSpeed)
        {
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            double robotAngle = Model.OurRobots[RobotID].Angle.Value;
            Vector2D movingDirection = Target - robotLocation;
            Vector2D A = new Vector2D(0, 8);
            if (Model.OurRobots[RobotID].Speed.Size > (CutSpeed.Size))
                A = new Vector2D(0,-8);
            
            //if ((CutSpeed - lastV).Size < 1)
            //    lastV = CutSpeed;

            lastV += A * StaticVariables.FRAME_PERIOD;
            lastV.X = XController(Model, RobotID,RefrenceVec);;
            lastV.NormalizeTo(Math.Min(4, lastV.Size));
            
            v = Vector2D.FromAngleSize((RefrenceVec).AngleInRadians - Math.PI / 2 + lastV.AngleInRadians, lastV.Size);
            v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);

            double err = 0;
            double ww = -AngleController(Model, RobotID, (targetState - initState).AngleInRadians, ref err);
            return new SingleWirelessCommand(new Vector2D(v.X, v.Y), ww, false, 0, 0, false, false);
        }
        public SingleWirelessCommand Accelerate(WorldModel Model, int RobotID, Position2D Target, double time)
        {
            Vector2D targetRobotVec = Target - Model.OurRobots[RobotID].Location;
            Vector2D v = GameParameters.InRefrence(Model.OurRobots[RobotID].Speed, RefrenceVec);
            targetRobotVec = GameParameters.InRefrence(targetRobotVec, RefrenceVec);
            DrawingObjects.AddObject(new StringDraw("v.Y: " + v.Y.ToString(), Model.BallState.Location + new Vector2D(-2.6, 1)));
            double d = targetRobotVec.Y;
            double t = d / v.Y;
            double aa = 8;    
            DrawingObjects.AddObject(new StringDraw("t: " + t.ToString(), Model.BallState.Location + new Vector2D(-2.4, 1)));

            if (Math.Sign(A.Y) == Math.Sign(lA.Y) && Math.Sign(lA.Y) == Math.Sign(lastV.Y) && Math.Abs(lastV.Y - v.Y) > Math.Abs(5 * lA.Y * StaticVariables.FRAME_PERIOD))
            {
                lastV.Y = v.Y;

            }

            if ((d > 0 && v.Y < 0) || (v.Y > 0 && d > 0 && t > time) || (v.Y < 0 && d < 0 && t < time))
                A = new Vector2D(0, aa);
            else
                A = new Vector2D(0, -aa);
            if (Math.Sign(A.Y) != Math.Sign(lA.Y) && Math.Sign(lA.Y) == Math.Sign(lastV.Y) && Math.Abs(lastV.Y - v.Y) > Math.Abs(5 * lA.Y * StaticVariables.FRAME_PERIOD))
            {
                lastV.Y = v.Y;

            }
            lA = A;
            DrawingObjects.AddObject(new StringDraw("A: " + A.ToString(), Model.BallState.Location + new Vector2D(-2.2, 1)));
            lastV += A * StaticVariables.FRAME_PERIOD;
            lastV.X = XController(Model, RobotID,RefrenceVec); ;
            lastV.NormalizeTo(Math.Min(20, lastV.Size));

            v = Vector2D.FromAngleSize((RefrenceVec).AngleInRadians - Math.PI / 2 + lastV.AngleInRadians, lastV.Size);
            v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);
            DrawingObjects.AddObject(new StringDraw("Vcommand: " + v.ToString(), GameParameters.OurGoalCenter + new Vector2D(-0.9, 0)));
            double err = 0;
            double ww = -AngleController(Model, RobotID, (targetState - initState).AngleInRadians, ref err);
            return new SingleWirelessCommand(new Vector2D(v.X, v.Y), ww, false, 0, 0, false, false);
        }
        public SingleWirelessCommand Accelerate(WorldModel Model, int RobotID, double time, double dr)
        {
            double vMax = 30, aa = 12;

            double kP = 3.2;
            double va = Math.Min((2 * dr / time), vMax);
            va = Math.Max((2  * dr / time ), -vMax);
            Vector2D v0 = GameParameters.InRefrence(Model.OurRobots[RobotID].Speed, RefrenceVec);
            double vf = va - v0.Y;
            double errY =  kP * vf - v0.Y;
            if (firstInCloseLoop)
            {
                lastErrY = errY;
                firstInCloseLoop = false;
            }
            double derr = errY - lastErrY;
            double s =  errY;
            lastErrY = errY;
            double aMax = errY;

            //if (s > lastV.Y)
            //    aMax = aa;
            //else if (s < lastV.Y)
            //    aMax = -aa;

            if (aMax > aa)
                aMax = aa;
            else if (s < -aa)
                aMax = -aa;

            //aMax = s;
            if (Math.Sign(aMax) == Math.Sign(lA.Y) && Math.Sign(lA.Y) == Math.Sign(lastV.Y) && Math.Abs(lastV.Y - v.Y) > Math.Abs(5 * lA.Y * StaticVariables.FRAME_PERIOD))
            {
                lastV.Y = v.Y;

            }
            
            lastV.Y += aMax * StaticVariables.FRAME_PERIOD;
            //if(
            lA.Y = aMax;
            DrawingObjects.AddObject(new StringDraw("dr: " + dr.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1.7, 0)));
            DrawingObjects.AddObject(new StringDraw("time: " + time.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1.8, 0)));
            DrawingObjects.AddObject(new StringDraw("Va: " + va.ToString(), GameParameters.OurGoalCenter + new Vector2D(-0.9, 0)));
            DrawingObjects.AddObject(new StringDraw("V0: " + v0.Y.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1, 0)));
            DrawingObjects.AddObject(new StringDraw("Vf: " + vf.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1.1, 0)));
            DrawingObjects.AddObject(new StringDraw("errY: " + errY.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1.2, 0)));
            DrawingObjects.AddObject(new StringDraw("aMax: " + aMax.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1.3, 0)));
            DrawingObjects.AddObject(new StringDraw("lastV.Y: " + lastV.Y.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1.4, 0)));
            DrawingObjects.AddObject(new StringDraw("lastV.Size: " + lastV.Size.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1.5, 0)));

            lastV.X = XController(Model, RobotID,RefrenceVec); ;
            lastV.NormalizeTo(Math.Min(vMax, lastV.Size));

            v = Vector2D.FromAngleSize((RefrenceVec).AngleInRadians - Math.PI / 2 + lastV.AngleInRadians, lastV.Size);
            v = GameParameters.RotateCoordinates(v, Model.OurRobots[RobotID].Angle.Value);
            DrawingObjects.AddObject(new StringDraw("Vcommand: " + v.ToString(), GameParameters.OurGoalCenter + new Vector2D(-1.6, 0)));
            double err = 0;
            double ww = -AngleController(Model, RobotID, (targetState - initState).AngleInRadians, ref err);
            return new SingleWirelessCommand(new Vector2D(v.X, v.Y), ww, false, 0, 0, false, false);
        }

        public void ResetTimers()
        {
            stepCounter = 0;
            accelCounter = 0;
            timeCounter = 0;
            iErr = 0;
            iErrX = 0;
            lastV = Vector2D.Zero;
            v = Vector2D.Zero;
            firstInCut = true;
            firstInSync = true;
            cutState = -1;
            lastCutState = -1;
            d0InCut = 0;
            lastDInCut = 0;
            v0InCut = 0;
            vxInCut = 0;
            stayInTable = false;
            a0InCut = 0;
            dInCut = 0;
        }
        public bool Save(string filename, bool saveLastMotion)
        {
            bool saved = false;
            Save(saveLastMotion);
            try
            {
                FileStream writerFileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                formatter.Serialize(writerFileStream, this.motionDatas);
                writerFileStream.Close();
                saved = true;
            }

            catch (Exception ex)
            {
                Logger.Write(LogType.Exception, ex.ToString());

                saved = false;
                Console.WriteLine(ex);
            }
            return saved;
        }
        public void Save(bool lastLearned)
        {
            if (!lastLearned)
                motionDatas[passSpeed][distance][di] = thisMotion;
            else
                motionDatas[passSpeed][distance][di] = lastMotion;
            motionDatas = motionDatas.OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.Value);
            foreach (var item in motionDatas.Keys.ToList())
            {
                motionDatas[item] = motionDatas[item].OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.Value);
                foreach (var item2 in motionDatas[item].Keys.ToList())
                {
                    motionDatas[item][item2] = motionDatas[item][item2].OrderBy(o => o.Key).ToDictionary(k => k.Key, v => v.Value);
                    foreach (var item3 in motionDatas[item][item2].Keys.ToList())
                    {
                        double nxtR = 0, nxtB = 0, nxt = 0, nxtBD = 0, nxtBt2t = 0, nxtRt2t = 0;
                        double maxBt2t = double.MinValue, maxRt2t = double.MinValue, maxBd = double.MinValue;

                        for (int i = 0; i < motionDatas[item][item2][item3].stepsData.Count; i++)
                        {
                            for (int j = 0; j < motionDatas[item][item2][item3].stepsData[i].ListData.Count; j++)
                            {
                                double currR = motionDatas[item][item2][item3].stepsData[i].ListData[j].RealSpeed.Y;
                                double currB = motionDatas[item][item2][item3].stepsData[i].ListData[j].BallSpeed.Y;
                                double curr = motionDatas[item][item2][item3].stepsData[i].ListData[j].Speed.Y;

                                double currBD = motionDatas[item][item2][item3].stepsData[i].ListData[j].ballDistance;
                                double currBt2t = motionDatas[item][item2][item3].stepsData[i].ListData[j].bTime2target;
                                double currRt2t = motionDatas[item][item2][item3].stepsData[i].ListData[j].rTime2target;
                                
                                motionDatas[item][item2][item3].stepsData[i].ListData[j].RealSpeed = new Vector2D(0, nxtR);
                                motionDatas[item][item2][item3].stepsData[i].ListData[j].BallSpeed = new Vector2D(0, nxtB);
                                motionDatas[item][item2][item3].stepsData[i].ListData[j].Speed = new Vector2D(0, nxt);

                                motionDatas[item][item2][item3].stepsData[i].ListData[j].ballDistance = nxtBD;
                                motionDatas[item][item2][item3].stepsData[i].ListData[j].bTime2target = nxtBt2t;
                                motionDatas[item][item2][item3].stepsData[i].ListData[j].rTime2target = nxtRt2t;
                          
                                if (currBt2t > maxBt2t)
                                    maxBt2t = currBt2t;
                                if (currRt2t > maxRt2t)
                                    maxRt2t = currRt2t;
                                if (currBD > maxBd)
                                    maxBd = currBD;

                                nxtR = currR;
                                nxtB = currB;
                                nxt = curr;

                                nxtBD = currBD;
                                nxtBt2t = currBt2t;
                                nxtRt2t = currRt2t;
                            }
                        }
                        if (motionDatas[item][item2][item3].stepsData.Count > 0)
                        {
                            if (motionDatas[item][item2][item3].stepsData[0].ListData.Count > 0)
                            {
                                motionDatas[item][item2][item3].stepsData[0].ListData[0].rTime2target = maxRt2t;
                                motionDatas[item][item2][item3].stepsData[0].ListData[0].bTime2target = maxBt2t;
                                motionDatas[item][item2][item3].stepsData[0].ListData[0].ballDistance = maxBd;
                            }
                        }
                    }
                }
            }
        }
        public bool Load(string filename)
        {
            bool loaded = false;
            if (File.Exists(filename))
            {
                try
                {
                    FileStream readerFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    motionDatas = (Dictionary<int, Dictionary<int, Dictionary<int, MotionData>>>)formatter.Deserialize(readerFileStream);
                    readerFileStream.Close();
                    int count = 0;
                    double maxVReal = double.MinValue;
                    double v0 = 0;
                    double t = 0;
                    foreach (var item in motionDatas.Keys)
                    {
                        foreach (var item2 in motionDatas[item].Keys)
                        {
                            foreach (var item3 in motionDatas[item][item2].Keys)
                            {


                                for (int i = 0; i < motionDatas[item][item2][item3].stepsData.Count; i++)
                                {
                                    for (int j = 0; j < motionDatas[item][item2][item3].stepsData[i].ListData.Count; j++)
                                    {
                                        motionDatas[item][item2][item3].stepsData[i].ListData[j].Accel = new Vector2D(motionDatas[item][item2][item3].stepsData[i].ListData[j].Ax, motionDatas[item][item2][item3].stepsData[i].ListData[j].Ay);
                                        motionDatas[item][item2][item3].stepsData[i].ListData[j].Speed = new Vector2D(motionDatas[item][item2][item3].stepsData[i].ListData[j].Vx, motionDatas[item][item2][item3].stepsData[i].ListData[j].Vy);
                                        motionDatas[item][item2][item3].stepsData[i].ListData[j].RealSpeed = new Vector2D(motionDatas[item][item2][item3].stepsData[i].ListData[j].Rvx, motionDatas[item][item2][item3].stepsData[i].ListData[j].Rvy);
                                        motionDatas[item][item2][item3].stepsData[i].ListData[j].BallSpeed = new Vector2D(motionDatas[item][item2][item3].stepsData[i].ListData[j].Bvx, motionDatas[item][item2][item3].stepsData[i].ListData[j].Bvy);
                                        if (motionDatas[item][item2][item3].stepsData[i].ListData[j].RealSpeed.Y > maxVReal)
                                        {
                                            maxVReal = motionDatas[item][item2][item3].stepsData[i].ListData[j].RealSpeed.Y;
                                            v0 = motionDatas[item][item2][item3].stepsData[0].ListData[0].RealSpeed.Y;
                                            t = motionDatas[item][item2][item3].stepsData[0].ListData[0].rTime2target - motionDatas[item][item2][item3].stepsData[i].ListData[j].rTime2target;
                                        }
                                    }

                                }
                                count++;
                                motionDatas[item][item2][item3].vMaxReal = (maxVReal > 0) ? maxVReal : 0;
                                motionDatas[item][item2][item3].aMaxReal = (t > 0) ? (maxVReal - v0) / t : 0;
                            }
                        }

                    }

                    loaded = true;

                }

                catch (Exception ex)
                {
                    Logger.Write(LogType.Exception, ex.ToString());

                    loaded = false;
                    Console.WriteLine(ex);
                }
            }
            return loaded;
        }
        
    }
    [Serializable]
    public class SearchResult
    {
        public SearchResult(int psKey, int pdKey, int cdKey, int skey)
        {
            passSpeedKey = psKey;
            passDistKey = pdKey;
            cutDistKey = cdKey;
            stepKey = skey;
        }
        public int passSpeedKey = 0;
        public int passDistKey = 0;
        public int cutDistKey = 0;
        public int stepKey = 0;
    }
    [Serializable]
    class LearningData
    {
        public LearningData(int time, Vector2D accel,double duty)
        {
            Time = time;
            Accel = accel;
            Duty = duty;
        }
        public int Time = 0;
        public double Duty = 0;
        
        public double ballDistance = 0;
        public double bTime2target = 0;
        public double rTime2target = 0;

        double ax = 0, ay = 0;
        double vx = 0, vy = 0;
        double rvx = 0, rvy = 0;
        double bvx = 0, bvy = 0;

        public double Bvx
        {
            get { return bvx; }
            set 
            {
                bvx = value;
                ballSpeed.X = value;
            }
        }
        public double Bvy
        {
            get { return bvy; }
            set 
            {
                bvy = value;
                ballSpeed.Y = value;
            }
        }

        public double Rvx
        {
            get { return rvx; }
            set 
            {
                rvx = value;
                realSpeed.X = value;
            }
        }
        public double Rvy
        {
            get { return rvy; }
            set 
            {
                rvy = value;
                realSpeed.Y = value; 
            }
        }

        public double Vy
        {
            get { return vy; }
            set 
            {
                vy = value;
                speed.Y = value;
            }
        }
        public double Vx
        {
            get { return vx; }
            set 
            {
                vx = value;
                speed.X = value;
            }
        }

        public double Ax
        {
            get { return ax; }
            set 
            {
                ax = value;
                accel.X = value;
            }
        }
        public double Ay
        {
            get { return ay; }
            set 
            { 
                ay = value;
                accel.Y = value;
            }
        }

        [NonSerialized]
        Vector2D accel = Vector2D.Zero;
        public Vector2D Accel
        {
            get { return accel; }
            set 
            { 
                accel = value;
                ax = value.X;
                ay = value.Y;
            }
        }
        [NonSerialized]
        Vector2D speed = Vector2D.Zero;
        public Vector2D Speed
        {
            get { return speed; }
            set 
            {
                speed = value;
                vx = value.X;
                vy = value.Y;
            }
        }
        [NonSerialized]
        Vector2D realSpeed = Vector2D.Zero;
        public Vector2D RealSpeed
        {
            get { return realSpeed; }
            set
            {
                realSpeed = value;
                rvx = value.X;
                rvy = value.Y;
            }
        }
        [NonSerialized]
        Vector2D ballSpeed = Vector2D.Zero;
        public Vector2D BallSpeed
        {
            get { return ballSpeed; }
            set
            {
                ballSpeed = value;
                bvx = value.X;
                bvy = value.Y;
            }
        }
    }
    [Serializable]
    class StepData
    {
        public List<LearningData> ListData = new List<LearningData>();
    }
    [Serializable]
    class MotionData:ICloneable
    {
        public List<StepData> stepsData = new List<StepData>();
        public double Distance = 0;
        public double step = 0;
        public double vMax = 3.5;
        public double aMax = 7;
        public double lastA = 0;
        public double lastVmax = 0;
        public double inputValue = 0;
        public double lastdist = 0;
        [NonSerialized]
        public double vMaxReal = 0;
        [NonSerialized]
        public double aMaxReal = 0;
        public bool Learn(WorldModel Model, int RobotID, Vector2D Error, Position2D initPos, Position2D targetPos)
        {
            double newA = 0, newV = 0;
            double err = Error.Y * 0.5;

            double error = err;
             
            if ((lastVmax / lastA) <= inputValue)
            {
                newA = (lastA * lastVmax * lastVmax) / (lastVmax * lastVmax - 2 * lastA * error);
                if ((lastVmax / newA) > inputValue)
                    newA = (-lastVmax * lastVmax + 2 * lastA * lastVmax * inputValue + 2 * lastA * error) / (lastA * inputValue * inputValue);
            }
            else
            {
                newA = (lastVmax * lastVmax) / (2 * lastVmax * inputValue - 2 * error - lastA * inputValue * inputValue);
                if (lastVmax / newA > inputValue)
                    newA = lastA + (2 * error / (inputValue * inputValue));
            }
            newV = lastVmax;
            DrawingObjects.AddObject(new StringDraw("Learned With A: " + newA + " And V: " + newV, new Position2D(-1, -1)), "LearnedString");

            stepsData = ReCalculateCoefs(newA, newV, initPos, targetPos);
            lastA = newA;
            lastVmax = newV;
            return true;
        }
        public bool Learn(WorldModel Model, int RobotID, Position2D initPos, Position2D targetPos, double PassSpeed)
        {
           /* inputValue = InputFunction(Model, RobotID, targetPos, ballInit, PassSpeed);
            double tcruiz = lastVmax / lastA;
            double dt = (inputValue - lastInputFunc);
            double vl = Math.Min(lastA * lastInputFunc, lastVmax);
            double vi = Math.Min(lastA * inputValue, lastVmax);
            double dist = targetPos.DistanceFrom(initPos);

            double dxr = 0.5 * (vl + vi) * dt;
            double dxd = dist - lastdist;
            double dx = dxd - dxr;
            double k = 4;
            double newA = lastA + k * dx, newV = lastVmax;

            //if ((lastVmax / lastA) <= inputValue)
            //{
            //    newA = (lastA * lastVmax * lastVmax) / (lastVmax * lastVmax - 2 * lastA * dx);
            //    if ((lastVmax / newA) > inputValue)
            //        newA = (-lastVmax * lastVmax + 2 * lastA * lastVmax * inputValue + 2 * lastA * dx) / (lastA * inputValue * inputValue);
            //}
            //else
            //{
            //    newA = (lastVmax * lastVmax) / (2 * lastVmax * inputValue - 2 * dx - lastA * inputValue * inputValue);
            //    if (lastVmax / newA > inputValue)
            //        newA = lastA + (2 * dx / (inputValue * inputValue));
            //}
            //newV = lastVmax;
            //if (newA > 10)
            //    newA = 10;
            //else if (newA < 0)
            //    newA = 0;
            DrawingObjects.AddObject(new StringDraw("Learned With A: " + newA + " And V: " + newV, new Position2D(-1, 1)), "LearnedString2");

            stepsData = ReCalculateCoefs(newA, newV, initPos, targetPos);
            //lastA = newA;
            //lastVmax = newV;
            //   lastInputFunc = inputValue;
            * */
            return true;
        }

        public List<StepData> ReCalculateCoefs(double newA, double newV, Position2D initPos, Position2D targetPos)
        {

            double dist = initPos.DistanceFrom(targetPos);
            int counts = (int)Math.Ceiling(dist / step);
            List<StepData> res = new List<StepData>();
            for (int i = 0; i < counts; i++)
            {
                res.Add(new StepData());
            }

            double v = Math.Sqrt(2 * dist * newA);
            double dAccel = dist;
            if (v > newV)
            {
                dAccel = newV * newV / (2 * newA);
            }
            double stepAccel = Math.Min(dAccel / step, counts);
            double v0Step = 0;
            for (int i = 0; i < (int)stepAccel; i++)
            {
                double vEndstep = Math.Sqrt(2 * newA * step + v0Step * v0Step);
                double tStep = (vEndstep - v0Step) / newA;
                v0Step = vEndstep;
                LearningData ld = new LearningData((int)Math.Round(tStep * StaticVariables.FRAME_RATE), new Vector2D(0, newA), 1);
                res[i].ListData.Add(ld);
            }
            double r = stepAccel - (int)stepAccel;
            if (r > 0)
            {
                double vEndstep = Math.Sqrt(2 * newA * r * step + v0Step * v0Step);
                double tStep = (vEndstep - v0Step) / newA;
                v0Step = vEndstep;
                LearningData ld = new LearningData((int)Math.Round(tStep * StaticVariables.FRAME_RATE), new Vector2D(0, newA), r);
                res[(int)stepAccel].ListData.Add(ld);
                double tStep2 = ((1 - r) * step) / newV;
                ld = new LearningData((int)Math.Round(tStep2 * StaticVariables.FRAME_RATE), Vector2D.Zero, 1 - r);
                res[(int)stepAccel].ListData.Add(ld);
            }
            for (int i = (int)Math.Ceiling(stepAccel); i < counts; i++)
            {
                double tStep = step / newV;
                LearningData ld = new LearningData((int)Math.Round(tStep * StaticVariables.FRAME_RATE), Vector2D.Zero, 1);
                res[i].ListData.Add(ld);
            }
            return res;
        }
        public List<StepData> ReCalculateCoefs(Position2D initPos, Position2D targetPos)
        {
            double newA = lastA, newV = lastVmax;
            double dist = initPos.DistanceFrom(targetPos);
            int counts = (int)Math.Ceiling(dist / step);
            List<StepData> res = new List<StepData>();
            for (int i = 0; i < counts; i++)
            {
                res.Add(new StepData());
            }

            double v = Math.Sqrt(2 * dist * newA);
            double dAccel = dist;
            if (v > newV)
            {
                dAccel = newV * newV / (2 * newA);
            }
            double stepAccel = Math.Min(dAccel / step, counts);
            double v0Step = 0;
            for (int i = 0; i < (int)stepAccel; i++)
            {
                double vEndstep = Math.Sqrt(2 * newA * step + v0Step * v0Step);
                double tStep = (vEndstep - v0Step) / newA;
                v0Step = vEndstep;
                LearningData ld = new LearningData((int)Math.Round(tStep * StaticVariables.FRAME_RATE), new Vector2D(0, newA), 1);
                res[i].ListData.Add(ld);
            }
            double r = stepAccel - (int)stepAccel;
            if (r > 0)
            {
                double vEndstep = Math.Sqrt(2 * newA * r * step + v0Step * v0Step);
                double tStep = (vEndstep - v0Step) / newA;
                v0Step = vEndstep;
                LearningData ld = new LearningData((int)Math.Round(tStep * StaticVariables.FRAME_RATE), new Vector2D(0, newA), r);
                res[(int)stepAccel].ListData.Add(ld);
                double tStep2 = ((1 - r) * step) / newV;
                ld = new LearningData((int)Math.Round(tStep2 * StaticVariables.FRAME_RATE), Vector2D.Zero, 1 - r);
                res[(int)stepAccel].ListData.Add(ld);
            }
            for (int i = (int)Math.Ceiling(stepAccel); i < counts; i++)
            {
                double tStep = step / newV;
                LearningData ld = new LearningData((int)Math.Round(tStep * StaticVariables.FRAME_RATE), Vector2D.Zero, 1);
                res[i].ListData.Add(ld);
            }
            return res;
        }

        public object Clone()
        {
            return (MotionData)this.MemberwiseClone();
        }
    }
}
