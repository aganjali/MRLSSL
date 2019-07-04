using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.AIConsole.Strategies
{
    public class Syncronizer
    {
        double minErrorRef = 0.0001, distOneTouchTresh = 0.8, passSpeedTresh = 0.12, moveBallTresh = 0.1, distBehindBallTresh = 0.08;
        double rotateInitDist = 0.13, Vmax = 3.0, Amax = 4, calculateInteralTresh = 0.3, extTimeCoef = 1.2, kiCoef = 5.5 / 5.0;

        double passTime, motionTime, errRefrence, accelTime, deccelTime, KI, extendTime, accelDist, deccelDist;

        bool finished, failed, first, calculateIntegral, firstInMove, gotoPoint, firstInOnetouch, goOneTouch, inRotate, inPassState, syncStarted, Debug = true;

        public bool InPassState
        {
            get { return inPassState; }
        }
        bool startRot = false;
        int catchState = 0;
        bool catchAndWait = false;

        public bool CatchKicked
        {
            get
            {
                if (catchNRot != null)
                {
                    return catchNRot.Kiick;
                }
                else
                    return false;
            }

        }

        public bool CatchAndWait
        {
            get
            {
                return catchAndWait;
            }
            set
            {
                catchAndWait = value;
                if (catchNRot != null)
                {
                    catchNRot.GoShoot = !value;
                }
            }
        }

        public int CatchState
        {
            get { return catchState; }
        }
        public bool StartRot
        {
            get { return startRot; }
        }
        public bool InRotate
        {
            get { return inRotate; }
        }

        public bool SyncStarted
        {
            get { return syncStarted; }
        }

        int integralTimer, execTimer, lastExecTimer, exteraRotateDelay, exteraIntegralRotateDelay;
        Position2D robotLastLocation, lastTarget, shooterInitLoc;
        Vector2D refrenceVec;
        OneTouchRole oneTouch;
        CatchAndRotateBallRole catchNRot;

        public Syncronizer()
        {
            Reset();
        }
        public bool Failed
        {
            get { return failed; }
        }

        public bool Finished
        {
            get { return finished; }
        }
        public bool GotoPoint
        {
            get { return gotoPoint; }
        }
        //1.12
        double kMotionDirect = 1.0, kMotionChip = 1.1, kPassDirect = 1.0, kPassChip = 1.0, kMotionDirectCatch = 1.3, kMotionChipCatch = 1.30, kPassDirectCatch = 1.0, kPassChipCatch = 1.0;
        Position2D firstBallPos = Position2D.Zero;
        double chipPassOffset = 80;

        public void SyncDirectPass(GameStrategyEngine engine, WorldModel Model, int PasserID, Position2D InitialPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, int rotateDelayOffset = 0)
        {
            Vector2D BallTarget = PassTarget - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            SyncDirectPass(engine, Model, PasserID, Teta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, rotateDelayOffset);
        }
        public void SyncDirectPass(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, int rotateDelayOffset = 0)
        {
            Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
            #region First
            if (first)
            {
                //PassSpeed -= 2;
                //PassSpeed = Math.Max(0, PassSpeed);
                motionTime = kMotionDirect * Planner.GetMotionTime(Model, ShooterID, Model.OurRobots[ShooterID].Location, Pos2go, ActiveParameters.RobotMotionCoefs);
                double passSpeedPhase2 = PassSpeed * 5 / 7;
                double ballAccelPhase1 = -5;
                double ballAccelPhase2 = -0.3;
                double dxPhase1 = 0;
                passTime = ((passSpeedPhase2 - PassSpeed) / ballAccelPhase1) / StaticVariables.FRAME_PERIOD;
                dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - PassSpeed * PassSpeed) / (2 * ballAccelPhase1);
                double dxPhase2 = Model.BallState.Location.DistanceFrom(PassTarget) - dxPhase1;
                double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
                double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;
                passTime += ((vf - passSpeedPhase2) / ballAccelPhase2) / StaticVariables.FRAME_PERIOD;
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Teta);
                passTime *= kPassDirect;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = Model.OurRobots[ShooterID].Location;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                //accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                //deccelTime = motionTime - accelTime;
                firstBallPos = Model.BallState.Location;
                integralTimer = 0;
                KI = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) * kiCoef;
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
                Planner.SetReCalculateTeta(PasserID, true);
            }
            #endregion

            if (inRotate && Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                gotoPoint = true;
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, false);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;
            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;
                if (passTime > motionTime)
                {

                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;

                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                }
            }

            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }

            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Direct", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new StringDraw("passTime: " + (passTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.5, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
            //    execTimer++;
        }

        public void SyncDirectPass(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, int ShooterID, Position2D RealPassTarget, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool goOt = true, int rotateDelayOffset = 0)
        {
            Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
            #region First
            if (first)
            {
                motionTime = kMotionDirect * Planner.GetMotionTime(Model, ShooterID, Model.OurRobots[ShooterID].Location, Pos2go, ActiveParameters.RobotMotionCoefs);
                double passSpeedPhase2 = PassSpeed * 5 / 7;
                double ballAccelPhase1 = -5;
                double ballAccelPhase2 = -0.3;
                double dxPhase1 = 0;
                passTime = ((passSpeedPhase2 - PassSpeed) / ballAccelPhase1) / StaticVariables.FRAME_PERIOD;
                dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - PassSpeed * PassSpeed) / (2 * ballAccelPhase1);
                double dxPhase2 = Model.BallState.Location.DistanceFrom(PassTarget) - dxPhase1;
                double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
                double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;
                passTime += ((vf - passSpeedPhase2) / ballAccelPhase2) / StaticVariables.FRAME_PERIOD;
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Teta);
                passTime *= kPassDirect;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = Model.OurRobots[ShooterID].Location;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                //accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                //deccelTime = motionTime - accelTime;
                firstBallPos = Model.BallState.Location;
                integralTimer = 0;
                KI = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) * kiCoef;
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
                Planner.SetReCalculateTeta(PasserID, true);
            }
            #endregion

            if (inRotate && Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                gotoPoint = true;
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, RealPassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, false);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;

            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;
                if (passTime > motionTime)
                {

                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint && goOt, PassSpeed);
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;

                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                    {
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint && goOt, PassSpeed);

                    }
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                }
            }

            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }

            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Direct", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
            //    execTimer++;
        }
        public void SyncDirectPass(GameStrategyEngine engine, WorldModel Model, Position2D initPos, int PasserID, int ShooterID, Position2D RealPassTarget, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool goOt = true, int rotateDelayOffset = 0)
        {
            Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
            #region First
            if (first)
            {
                motionTime = kMotionDirect * Planner.GetMotionTime(Model, ShooterID, Model.OurRobots[ShooterID].Location, Pos2go, ActiveParameters.RobotMotionCoefs);
                double passSpeedPhase2 = PassSpeed * 5 / 7;
                double ballAccelPhase1 = -5;
                double ballAccelPhase2 = -0.3;
                double dxPhase1 = 0;
                passTime = ((passSpeedPhase2 - PassSpeed) / ballAccelPhase1) / StaticVariables.FRAME_PERIOD;
                dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - PassSpeed * PassSpeed) / (2 * ballAccelPhase1);
                double dxPhase2 = Model.BallState.Location.DistanceFrom(PassTarget) - dxPhase1;
                double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
                double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;
                passTime += ((vf - passSpeedPhase2) / ballAccelPhase2) / StaticVariables.FRAME_PERIOD;
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Model, initPos, RealPassTarget);
                passTime *= kPassDirect;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = Model.OurRobots[ShooterID].Location;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                //accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                //deccelTime = motionTime - accelTime;
                integralTimer = 0;
                KI = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) * kiCoef;
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
                firstBallPos = Model.BallState.Location;
                Planner.SetReCalculateTeta(PasserID, true);
            }
            #endregion

            if (inRotate && Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                gotoPoint = true;
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, RealPassTarget, initPos, kickPowerType.Speed, PassSpeed, false, (Model.BallState.Location.Y > 0) ? true : false, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, false);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;

            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;
                if (passTime > motionTime)
                {

                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint && goOt, PassSpeed);
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;

                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint && goOt, PassSpeed);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                }
            }

            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }

            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Direct", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
            //    execTimer++;
        }

        public void SyncDirectPass(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, Position2D firstPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, int rotateDelayOffset = 0)
        {
            Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
            #region First
            if (first)
            {
                motionTime = kMotionDirect * Planner.GetMotionTime(Model, ShooterID, firstPos, Pos2go, ActiveParameters.RobotMotionCoefs);
                double passSpeedPhase2 = PassSpeed * 5 / 7;
                double ballAccelPhase1 = -5;
                double ballAccelPhase2 = -0.3;
                double dxPhase1 = 0;
                passTime = ((passSpeedPhase2 - PassSpeed) / ballAccelPhase1) / StaticVariables.FRAME_PERIOD;
                dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - PassSpeed * PassSpeed) / (2 * ballAccelPhase1);
                double dxPhase2 = Model.BallState.Location.DistanceFrom(PassTarget) - dxPhase1;
                double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
                double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;
                passTime += ((vf - passSpeedPhase2) / ballAccelPhase2) / StaticVariables.FRAME_PERIOD;
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Teta);
                passTime *= kPassDirect;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = firstPos;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= firstPos.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (firstPos.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = firstPos.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                //accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                //deccelTime = motionTime - accelTime;
                integralTimer = 0;
                KI = firstPos.DistanceFrom(Pos2go) * kiCoef;
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
                firstBallPos = Model.BallState.Location;
                Planner.SetReCalculateTeta(PasserID, true);
            }
            #endregion

            if (inRotate && Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (gotoPoint || goOneTouch)
            {
                if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                    gotoPoint = true;
            }
            else
            {
                if (firstPos.DistanceFrom(PassTarget) < distOneTouchTresh)
                    gotoPoint = true;
            }
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, false);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;
            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;
                if (passTime > motionTime)
                {

                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;

                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                }
            }

            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }

            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Direct", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
            //    execTimer++;
        }
        public void SyncDirectPass(GameStrategyEngine engine, WorldModel Model, int PasserID, Position2D InitialPos, Position2D firstPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, int rotateDelayOffset = 0)
        {
            Vector2D BallTarget = PassTarget - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            SyncDirectPass(engine, Model, PasserID, Teta, firstPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, rotateDelayOffset);
        }

        public void SyncChipPass(GameStrategyEngine engine, WorldModel Model, int PasserID, Position2D InitialPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool backSensor, int rotateDelayOffset = 0)
        {

            Vector2D BallTarget = PassTarget - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            SyncChipPass(engine, Model, PasserID, Teta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor, rotateDelayOffset);
        }
        public void SyncChipPass(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool backSensor, int rotateDelayOffset = 0)
        {
            KickPower = Math.Min(KickPower, 4.5);
            Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
            #region First
            if (first)
            {
                motionTime = kMotionChip * Planner.GetMotionTime(Model, ShooterID, Model.OurRobots[ShooterID].Location, Pos2go, ActiveParameters.RobotMotionCoefs);
                passTime = 2 * Math.Sqrt(5 * PassSpeed) / 5 / StaticVariables.FRAME_PERIOD;
                double d = Math.Max(0, Model.BallState.Location.DistanceFrom(PassTarget) - PassSpeed);
                double vx0 = PassSpeed;
                double ballDeccel = -0.3;
                double vsquare = vx0 * vx0 + 2 * ballDeccel * d;
                double vf = (vsquare > 0) ? Math.Sqrt(vsquare) : 0;
                //  passTime += 0.5*((vf - vx0) / ballDeccel / StaticVariables.FRAME_PERIOD);
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Teta);
                passTime *= kPassChip;
                if (backSensor)
                    passTime += chipPassOffset;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = Model.OurRobots[ShooterID].Location;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                deccelTime = motionTime - accelTime;
                integralTimer = 0;
                KI = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) * kiCoef;
                firstBallPos = Model.BallState.Location;
                Planner.SetReCalculateTeta(PasserID, true);
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
            }
            #endregion
            if (Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                gotoPoint = true;
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, true, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, backSensor);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;
            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;

                if (passTime > motionTime)
                {
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), Model.OurRobots[PasserID], true, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;

                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), Model.OurRobots[PasserID], true, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                }
            }
            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }

            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Chip", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
        }

        public void SyncChipPass(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, int ShooterID, Position2D RealPassTarget, Position2D PassTarget, Position2D ShootTarget, kickPowerType kickType, double PassSpeed, double KickPower, int RotateDelay, bool goOt, bool backSensor, int rotateDelayOffset = 0)
        {
            if (kickPowerType.Speed == kickType)
            {
                KickPower = Math.Min(KickPower, 4.5);
            }

            Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
            #region First
            if (first)
            {
                double ps = (kickPowerType.Speed == kickType) ? PassSpeed : 0.9;
                motionTime = kMotionChip * Planner.GetMotionTime(Model, ShooterID, Model.OurRobots[ShooterID].Location, Pos2go, ActiveParameters.RobotMotionCoefs);
                passTime = 1 * Math.Sqrt(5 * ps) / 5 / StaticVariables.FRAME_PERIOD;
                double d = Math.Max(0, Model.BallState.Location.DistanceFrom(PassTarget) - ps);
                double vx0 = ps * 1;
                double ballDeccel = -0.3;
                double vsquare = vx0 * vx0 + 2 * ballDeccel * d;
                double vf = (vsquare > 0) ? Math.Sqrt(vsquare) : 0;
                //  passTime += 0.5*((vf - vx0) / ballDeccel / StaticVariables.FRAME_PERIOD);
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Teta);
                passTime *= kPassChip;
                if (backSensor)
                    passTime += chipPassOffset;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = Model.OurRobots[ShooterID].Location;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                deccelTime = motionTime - accelTime;
                integralTimer = 0;
                KI = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) * kiCoef;
                firstBallPos = Model.BallState.Location;
                Planner.SetReCalculateTeta(PasserID, true);
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
            }
            #endregion
            if (Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                gotoPoint = true;
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, RealPassTarget, Teta, kickType, PassSpeed, true, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, backSensor);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;
            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;

                if (passTime > motionTime)
                {
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), Model.OurRobots[PasserID], true, ShootTarget, KickPower, false, gotoPoint && goOt, PassSpeed);
                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;
                        startRot = true;
                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), Model.OurRobots[PasserID], true, ShootTarget, KickPower, false, gotoPoint && goOt, PassSpeed);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    else
                        startRot = true;
                }
            }
            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }

            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Chip", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new StringDraw("PassTime: " + passTime.ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.5, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
        }
        public void SyncChipPass(GameStrategyEngine engine, WorldModel Model, Position2D InitPos, int PasserID, int ShooterID, Position2D RealPassTarget, Position2D PassTarget, Position2D ShootTarget, kickPowerType kickType, double PassSpeed, double KickPower, int RotateDelay, bool goOt, bool backSensor, int rotateDelayOffset = 0)
        {
            if (kickPowerType.Speed == kickType)
            {
                KickPower = Math.Min(KickPower, 4.5);
            }

            Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                double ps = (kickPowerType.Speed == kickType) ? PassSpeed : 0.9;
                motionTime = kMotionChip * Planner.GetMotionTime(Model, ShooterID, Model.OurRobots[ShooterID].Location, Pos2go, ActiveParameters.RobotMotionCoefs);
                passTime = 1.2 * Math.Sqrt(5 * ps) / 5 / StaticVariables.FRAME_PERIOD;
                double d = Math.Max(0, Model.BallState.Location.DistanceFrom(PassTarget) - ps);
                double vx0 = ps * 1;
                double ballDeccel = -0.3;
                double vsquare = vx0 * vx0 + 2 * ballDeccel * d;
                double vf = (vsquare > 0) ? Math.Sqrt(vsquare) : 0;
                //  passTime += 0.5*((vf - vx0) / ballDeccel / StaticVariables.FRAME_PERIOD);
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Model, InitPos, RealPassTarget);
                passTime *= kPassChip;
                if (backSensor)
                    passTime += chipPassOffset;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = Model.OurRobots[ShooterID].Location;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                deccelTime = motionTime - accelTime;
                integralTimer = 0;
                KI = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) * kiCoef;

                Planner.SetReCalculateTeta(PasserID, true);
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
            }
            #endregion
            if (Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                gotoPoint = true;
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, RealPassTarget, InitPos, kickType, PassSpeed, true, (Model.BallState.Location.Y < 0) ? false : true, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, backSensor);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;
            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;

                if (passTime > motionTime)
                {
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), Model.OurRobots[PasserID], true, ShootTarget, KickPower, false, gotoPoint && goOt, PassSpeed);
                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;
                        startRot = true;
                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), Model.OurRobots[PasserID], true, ShootTarget, KickPower, false, gotoPoint && goOt, PassSpeed);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    else
                        startRot = true;
                }
            }
            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }

            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Chip", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
        }

        public void SyncChipPass(GameStrategyEngine engine, WorldModel Model, int PasserID, Position2D InitialPos, Position2D firstPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool backSensor, int rotateDelayOffset = 0)
        {
            Vector2D BallTarget = PassTarget - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            SyncChipPass(engine, Model, PasserID, Teta, firstPos, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor, rotateDelayOffset);
        }
        public void SyncChipPass(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, Position2D firstPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool backSensor, int rotateDelayOffset = 0)
        {
            KickPower = Math.Min(KickPower, 4.5);

            Position2D Pos2go = (PassTarget - ShootTarget).GetNormalizeToCopy(distBehindBallTresh) + PassTarget;
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                motionTime = kMotionChip * Planner.GetMotionTime(Model, ShooterID, firstPos, Pos2go, ActiveParameters.RobotMotionCoefs);
                passTime = 1 * Math.Sqrt(5 * PassSpeed) / 5 / StaticVariables.FRAME_PERIOD;
                double d = Math.Max(0, Model.BallState.Location.DistanceFrom(PassTarget) - PassSpeed);
                double vx0 = PassSpeed * 1;
                double ballDeccel = -0.3;
                double vsquare = vx0 * vx0 + 2 * ballDeccel * d;
                double vf = (vsquare > 0) ? Math.Sqrt(vsquare) : 0;
                //  passTime += 0.5*((vf - vx0) / ballDeccel / StaticVariables.FRAME_PERIOD);
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Teta);
                passTime *= kPassChip;
                if (backSensor)
                    passTime += chipPassOffset;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = firstPos;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= firstPos.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (firstPos.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = firstPos.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                deccelTime = motionTime - accelTime;
                integralTimer = 0;
                KI = firstPos.DistanceFrom(Pos2go) * kiCoef;

                Planner.SetReCalculateTeta(PasserID, true);
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
            }
            #endregion
            if (Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (gotoPoint || goOneTouch)
            {
                if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                    gotoPoint = true;
            }
            else
            {
                if (firstPos.DistanceFrom(PassTarget) < distOneTouchTresh)
                    gotoPoint = true;
            }
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, true, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, backSensor);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;
            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;

                if (passTime > motionTime)
                {
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), Model.OurRobots[PasserID], true, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;

                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), Model.OurRobots[PasserID], true, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (ShootTarget - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                }
            }
            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }

            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Chip", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
        }

        public void SyncDirectCatch(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, bool kickIsChip, kickPowerType kType, int RotateDelay, int rotateDelayOffset = 0)
        {
            Position2D Pos2go = PassTarget;
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                motionTime = kMotionDirectCatch * Planner.GetMotionTime(Model, ShooterID, Model.OurRobots[ShooterID].Location, Pos2go, ActiveParameters.RobotMotionCoefs);
                //    PassSpeed /= GamePlannerInfo.DirectCoef[PasserID];
                double passSpeedPhase2 = PassSpeed * 5 / 7;
                double ballAccelPhase1 = -5;
                double ballAccelPhase2 = -0.3;
                double dxPhase1 = 0;
                passTime = ((passSpeedPhase2 - PassSpeed) / ballAccelPhase1) / StaticVariables.FRAME_PERIOD;
                dxPhase1 = (passSpeedPhase2 * passSpeedPhase2 - PassSpeed * PassSpeed) / (2 * ballAccelPhase1);
                double dxPhase2 = Model.BallState.Location.DistanceFrom(PassTarget) - dxPhase1;
                double tmpSqrSpeed = passSpeedPhase2 * passSpeedPhase2 + 2 * ballAccelPhase2 * dxPhase2;
                double vf = (tmpSqrSpeed > 0) ? Math.Sqrt(tmpSqrSpeed) : 0;
                passTime += ((vf - passSpeedPhase2) / ballAccelPhase2) / StaticVariables.FRAME_PERIOD;
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Teta);
                passTime *= kPassDirectCatch;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = Model.OurRobots[ShooterID].Location;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                //accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                //deccelTime = motionTime - accelTime;
                integralTimer = 0;
                KI = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) * kiCoef;
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
                //        PassSpeed *= GamePlannerInfo.DirectCoef[PasserID];
                Planner.SetReCalculateTeta(PasserID, true);
            }
            #endregion

            if (inRotate && Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                gotoPoint = true;
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, false);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;
            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;
                if (passTime > motionTime)
                {

                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (goOneTouch)
                        catchNRot.CatchAndRotate(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), ShootTarget, false, kickIsChip, true, gotoPoint, KickPower, true);
                    //oneTouch.Perform(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), null, false, ShootTarget, KickPower, false, gotoPoint, PassSpeed);
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;

                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (Model.BallState.Location - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        catchNRot.CatchAndRotate(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), ShootTarget, false, kickIsChip, true, gotoPoint, KickPower, true);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (Model.BallState.Location - Pos2go).AngleInDegrees, PathType.UnSafe, false, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                }
            }

            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }
            catchState = catchNRot.CurrentState;
            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Direct", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
            //    execTimer++;
        }
        public void SyncDirectCatch(GameStrategyEngine engine, WorldModel Model, int PasserID, Position2D InitialPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, bool kickIsChip, kickPowerType kType, int RotateDelay, int rotateDelayOffset = 0)
        {
            Vector2D BallTarget = PassTarget - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            SyncDirectCatch(engine, Model, PasserID, Teta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, kickIsChip, kType, RotateDelay, rotateDelayOffset);
        }
        public void SyncDirectCatch(GameStrategyEngine engine, WorldModel Model, int PasserID, Position2D InitialPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, int rotateDelayOffset = 0)
        {
            Vector2D BallTarget = PassTarget - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            SyncDirectCatch(engine, Model, PasserID, Teta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, false, kickPowerType.Speed, RotateDelay, rotateDelayOffset);
        }
        public void SyncDirectCatch(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, int rotateDelayOffset = 0)
        {
            SyncDirectCatch(engine, Model, PasserID, Teta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, false, kickPowerType.Speed, RotateDelay, rotateDelayOffset);
        }

        public void SyncChipCatch(GameStrategyEngine engine, WorldModel Model, int PasserID, Position2D InitialPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool kickIsChip, kickPowerType kType, bool backSensor, int rotateDelayOffset = 0)
        {

            Vector2D BallTarget = PassTarget - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            SyncChipCatch(engine, Model, PasserID, Teta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, kickIsChip, kType, backSensor, rotateDelayOffset);
        }
        public void SyncChipCatch(GameStrategyEngine engine, WorldModel Model, int PasserID, Position2D InitialPos, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool backSensor, int rotateDelayOffset = 0)
        {
            Vector2D BallTarget = PassTarget - Model.BallState.Location;
            Vector2D InitBall = Model.BallState.Location - InitialPos;
            double Teta = Math.Abs(Vector2D.AngleBetweenInDegrees(BallTarget, InitBall));
            SyncChipCatch(engine, Model, PasserID, Teta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, false, kickPowerType.Speed, backSensor, rotateDelayOffset);
        }
        public void SyncChipCatch(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool kickIsChip, kickPowerType kType, bool backSensor, int rotateDelayOffset = 0)
        {
            KickPower = Math.Min(KickPower, 4.5);
            Position2D Pos2go = PassTarget;
            #region First
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                motionTime = kMotionChipCatch * Planner.GetMotionTime(Model, ShooterID, Model.OurRobots[ShooterID].Location, Pos2go, ActiveParameters.RobotMotionCoefs);
                passTime = 1 * Math.Sqrt(5 * PassSpeed) / 5 / StaticVariables.FRAME_PERIOD;
                double d = Math.Max(0, Model.BallState.Location.DistanceFrom(PassTarget) - PassSpeed);
                double vx0 = PassSpeed * 1;
                double ballDeccel = -0.3;
                double vsquare = vx0 * vx0 + 2 * ballDeccel * d;
                double vf = (vsquare > 0) ? Math.Sqrt(vsquare) : 0;
                //  passTime += 0.5*((vf - vx0) / ballDeccel / StaticVariables.FRAME_PERIOD);
                passTime += RotateDelay;
                passTime += Planner.GetRotateTime(Teta);
                passTime *= kPassChipCatch;
                if (backSensor)
                    passTime += chipPassOffset;
                //Vector2D rotateInitVec = (Model.BallState.Location - PassTarget).GetNormalizeToCopy(rotateInitDist);
                //passTime += (Planner.GetMotionTime(Model, PasserID, Model.OurRobots[PasserID].Location, rotateInitVec + Model.BallState.Location));
                shooterInitLoc = Model.OurRobots[ShooterID].Location;
                refrenceVec = Pos2go - shooterInitLoc;
                robotLastLocation = shooterInitLoc;
                errRefrence = (refrenceVec.Size / motionTime);
                errRefrence = Math.Max(errRefrence, minErrorRef);
                calculateIntegral = false;
                Console.WriteLine("PassTime: " + passTime);
                Console.WriteLine("MotionTime: " + motionTime);
                double tmpD = Vmax * Vmax / (2 * Amax);
                if (2 * tmpD <= Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go))
                {
                    accelTime = Vmax / Amax;
                    double tCruise = (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) - 2 * tmpD) / Vmax;
                    deccelTime = accelTime + tCruise;
                }
                else
                {
                    double dhalf = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) / 2;
                    double vhalf = Math.Sqrt(2 * Amax * dhalf);
                    accelTime = vhalf / Amax;
                    deccelTime = accelTime;
                }
                accelDist = 0.5 * Amax * accelTime * accelTime;
                deccelDist = accelDist + (deccelTime - accelTime) * Vmax;
                accelTime /= StaticVariables.FRAME_PERIOD;
                deccelTime /= StaticVariables.FRAME_PERIOD;
                accelTime = (accelTime / (accelTime + deccelTime)) * motionTime;
                deccelTime = motionTime - accelTime;
                integralTimer = 0;
                KI = Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) * kiCoef;

                Planner.SetReCalculateTeta(PasserID, true);
                firstInOnetouch = true;
                gotoPoint = false;
                first = false;
            }
            #endregion
            if (Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > moveBallTresh)
                goOneTouch = true;
            if (Model.OurRobots[ShooterID].Location.DistanceFrom(PassTarget) < distOneTouchTresh)
                gotoPoint = true;
            // int exT = 0;
            var tmpRot = Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, true, RotateDelay + exteraRotateDelay + exteraIntegralRotateDelay + rotateDelayOffset, backSensor);
            int state = 0;
            if (tmpRot.IsInRotateDelay)
                inRotate = true;
            if (tmpRot.InKickState)
                inPassState = true;
            if (inRotate)
            {
                execTimer++;

                if (passTime > motionTime)
                {
                    if (goOneTouch)
                        catchNRot.CatchAndRotate(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), ShootTarget, true, kickIsChip, true, gotoPoint, KickPower, true);
                    //     Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                    exteraRotateDelay = 0;
                    if (execTimer >= (passTime - motionTime))
                    {
                        //if (firstInMove)
                        //    lastExecTimer = execTimer;

                        //firstInMove = false;
                        //lastExecTimer = execTimer;
                        syncStarted = true;

                        if (!gotoPoint || !goOneTouch)
                            Planner.Add(ShooterID, Pos2go, (Model.BallState.Location - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                        calculateIntegral = true;
                    }
                    else if (!goOneTouch)
                    {
                        syncStarted = false;
                        Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                        state = -1;
                    }
                }
                else
                {
                    syncStarted = true;
                    if (goOneTouch)
                        catchNRot.CatchAndRotate(engine, Model, ShooterID, new SingleObjectState(Pos2go, Vector2D.Zero, 0), ShootTarget, true, kickIsChip, true, gotoPoint, KickPower, true);
                    if (!gotoPoint || !goOneTouch)
                        Planner.Add(ShooterID, Pos2go, (Model.BallState.Location - Pos2go).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    calculateIntegral = true;
                    if (execTimer < (motionTime - passTime))
                        exteraRotateDelay++;//    Planner.AddRotate(Model, PasserID, PassTarget, Teta, kickPowerType.Speed, PassSpeed, false, RotateDelay + exT);
                }
            }
            else
            {
                Planner.Add(ShooterID, new SingleWirelessCommand(), false);
                state = -1;
            }
            double dy = 0;
            if (calculateIntegral)
            {
                Vector2D dLoc = Model.OurRobots[ShooterID].Location - robotLastLocation;
                dy = dLoc.Size * Math.Cos(Vector2D.AngleBetweenInRadians(dLoc, refrenceVec));
                Vector2D robotInitVec = Model.OurRobots[ShooterID].Location - shooterInitLoc;
                double dRobotOnRef = robotInitVec.Size * Math.Cos(Vector2D.AngleBetweenInRadians(robotInitVec, refrenceVec));
                integralTimer++;
                double dt = StaticVariables.FRAME_PERIOD;

                if (dRobotOnRef <= accelDist || dRobotOnRef >= deccelDist)
                {
                    errRefrence = accelDist / accelTime;
                }
                else
                {
                    errRefrence = (deccelDist - accelDist) / (deccelTime - accelTime);
                }

                if (Model.OurRobots[ShooterID].Location.DistanceFrom(Pos2go) > calculateInteralTresh)
                {
                    extendTime += (1 - (dy / errRefrence));
                }
                robotLastLocation = Model.OurRobots[ShooterID].Location;
            }
            catchState = catchNRot.CurrentState;
            exteraIntegralRotateDelay = (int)(extTimeCoef * Math.Ceiling(extendTime));
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("Chip", Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("state: " + state).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.6, 1)));
                DrawingObjects.AddObject(new StringDraw(("goOneTouch: " + goOneTouch).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.4, 1)));
                DrawingObjects.AddObject(new StringDraw(("gotoPoint: " + gotoPoint).ToString(), Color.Blue, Position2D.Zero + new Vector2D(-0.2, 1)));
                DrawingObjects.AddObject(new StringDraw((exteraRotateDelay + RotateDelay + exteraIntegralRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0, 1)));
                DrawingObjects.AddObject(new StringDraw(("InRotate: " + tmpRot.IsInRotateDelay).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("execTimer: " + execTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("AccelTime: " + accelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(0.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("DeccelTime: " + deccelTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTime: " + (accelTime + deccelTime)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.3, 1)));
                DrawingObjects.AddObject(new StringDraw(("motionTimeReal: " + motionTime).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.5, 1)));
                DrawingObjects.AddObject(new StringDraw(("integralTime: " + integralTimer).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.7, 1)));
                DrawingObjects.AddObject(new StringDraw(("errRefrence: " + errRefrence).ToString(), Color.Blue, Position2D.Zero + new Vector2D(1.9, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy: " + dy).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.1, 1)));
                DrawingObjects.AddObject(new StringDraw(("dy / errRefrence: " + (dy / errRefrence)).ToString(), Color.Blue, Position2D.Zero + new Vector2D(2.3, 1)));
                DrawingObjects.AddObject(new Circle(PassTarget, 0.1, new Pen(Color.Red, 0.01f)), "PassTargetCircle");
                DrawingObjects.AddObject(new Circle(Pos2go, 0.1, new Pen(Color.Blue, 0.01f)), "pos2goCircle");
            }
        }
        public void SyncChipCatch(GameStrategyEngine engine, WorldModel Model, int PasserID, double Teta, int ShooterID, Position2D PassTarget, Position2D ShootTarget, double PassSpeed, double KickPower, int RotateDelay, bool backSensor, int rotateDelayOffset = 0)
        {
            SyncChipCatch(engine, Model, PasserID, Teta, ShooterID, PassTarget, ShootTarget, PassSpeed, KickPower, RotateDelay, false, kickPowerType.Speed, backSensor, rotateDelayOffset);
        }
        public void Reset()
        {
            catchAndWait = false;
            firstBallPos = Position2D.Zero;
            goOneTouch = false;
            failed = false;
            first = true;
            finished = false;
            firstInMove = true;
            motionTime = 0;
            passTime = 0;
            robotLastLocation = Position2D.Zero;
            refrenceVec = Vector2D.Zero;
            calculateIntegral = false;
            errRefrence = 0;
            accelTime = 0;
            deccelTime = 0;
            integralTimer = 0;
            extendTime = 0;
            execTimer = 0;
            lastExecTimer = 0;
            oneTouch = new OneTouchRole();
            catchNRot = new CatchAndRotateBallRole();
            gotoPoint = false;
            firstInOnetouch = true;
            lastTarget = Position2D.Zero;
            exteraRotateDelay = 0;
            shooterInitLoc = Position2D.Zero;
            exteraIntegralRotateDelay = 0;
            accelDist = 0;
            deccelDist = 0;
            inRotate = false;
            startRot = false;
            inPassState = false;
        }

    }
}
