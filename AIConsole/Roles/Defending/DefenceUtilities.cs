using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{

    public static class OppFreeKickDefenceUtils
    {
        public static double Normal2InPenaltyAreaMargin = 0.1;
        public static double CornerInPenaltyAreaMargin = 0;
        public static double K2G2InPenaltyAreaMargin = 0.1;
        public static double BallInFront2InPenaltyAreaMargin = 0.1;
        public static double OppInDanger2InPenaltyAreaMargin = 0.1;
        public static double Normal2OppInDangerZoneMargin = 0.1;
        public static double Behind2InPenaltyAreaMargin = 0;
        public static double InPenaltyArea2NormalMargin = 0.3;
        public static double InPenaltyArea2OppDangerMargin = 0.1;
        public static double K2G2OppDangerMargin = 0.1;
        public static double BallInFront2OppDangerMargin = 0.1;
        public static double OppDanger2OppDangerMargin = 0.1;

        public static Position2D MarkFront(GameStrategyEngine engine, WorldModel Model, int RobotID, DefenceInfo info, double margin, out double Teta)
        {
            SingleObjectState Target = (info != null && info.OppID.HasValue) ? Model.Opponents[info.OppID.Value] : Model.BallState;
            //Teta = (Target.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            Teta = (Target.Location - GameParameters.OurGoalCenter).AngleInDegrees;
            double min = GameParameters.SafeRadi(Target, margin);
            Position2D Pos = GameParameters.OurGoalCenter - (GameParameters.OurGoalCenter - Target.Location).GetNormalizeToCopy(min);
            Pos.DrawColor = System.Drawing.Color.Red;
            DrawingObjects.AddObject(Pos);
            return Pos;
        }

        public static Position2D Dive(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Position2D pos = new Position2D();
            Position2D robotLoc = Model.OurRobots[RobotID].Location;
            Position2D ballLoc = Model.BallState.Location;
            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D prep = ballSpeed.PrependecularPoint(Model.BallState.Location, Model.OurRobots[RobotID].Location);
            double dist, DistFromBorder, R;
            if (GameParameters.IsInDangerousZone(prep, false, 0.15, out dist, out DistFromBorder))
            {
                R = GameParameters.SafeRadi(new SingleObjectState(prep, new Vector2D(), 0), .05 + (Math.Max(0, FreekickDefence.AdditionalSafeRadi - .05)));//);//FreekickDefence.AdditionalSafeRadi);
                pos = GameParameters.OurGoalCenter - ballSpeed.GetNormalizeToCopy(R);
            }
            else
                pos = prep;
            return pos;
        }
        public static Position2D BehindSatate(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta, Dictionary<RoleBase, int> CurrentStates)
        {
            //double dist;
            Position2D target = new Position2D();
            //if (inf != null && inf.OppID.HasValue)
            //{
            //    if (CurrentStates.Any(a => a.Key.GetType() == typeof(GoalieCornerRole)) && (DefenderStates)CurrentStates.Where(w => w.Key.GetType() == typeof(GoalieCornerRole)).First().Value == DefenderStates.InPenaltyArea)
            //        target = InPenaltyAreaState(engine, Model, inf, RobotID, out Teta);
            //    else if (Model.Opponents[inf.OppID.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) > 2)
            //        target = GetBackBallPoint(engine, Model, RobotID, out Teta);
            //    else
            //        target = MarkFront(engine, Model, RobotID, inf, 0.1, out Teta);
            //}
            //else
            //    target = GetBackBallPoint(engine, Model, RobotID, out Teta);
            SingleObjectState behind = BehindTarget(Model, RobotID);
            Teta = behind.Angle.Value;
            target = behind.Location;//
            //Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.14);
            DataBridge.BallBehindPos = target;
            DataBridge.BallBehindangle = Teta;
            return target;
        }
        public static Position2D InPenaltyAreaState(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta)
        {
            Vector2D vec;
            if (Model.BallState.Location.Y >= 0)
                vec = new Vector2D(0, -0.2);
            else
                vec = new Vector2D(0, 0.2);
            double R = GameParameters.SafeRadi(Model.BallState, 0.1);
            Position2D target = GameParameters.OurGoalCenter + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(R);
            target = target + vec;
            Teta = (Model.BallState.Location - target).AngleInDegrees;
            return target;
        }
        public static Position2D GetBackBallPoint(GameStrategyEngine engine, WorldModel Model, int RobotID, out double Teta)
        {
            Vector2D vec = Model.BallState.Location - GameParameters.OurGoalCenter;
            Position2D tar = Model.BallState.Location + vec;
            Vector2D ballSpeed = Model.BallState.Speed;
            Position2D ballLocation = Model.BallState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = tar - ballLocation;
            Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(0.09);
            Vector2D robotBackBallVec = backBallPoint - robotLocation;
            Vector2D ballBackBallVec = backBallPoint - ballLocation;
            double segmentConst = 0.7;
            double rearDistance = 0.15;
            Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
            Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
            Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
            Position2D midPoint = p1;
            if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
            {
                midPoint = p2;
            }
            Position2D finalPosToGo = midPoint;
            double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
            double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

            double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));

            if (Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < Math.PI / 6 && (Math.Abs(alfa) > Math.PI / 1.5 || Math.Abs(distance) > RobotParameters.OurRobotParams.Diameter / 2 + .01))
                finalPosToGo = backBallPoint;
            else
            {
                Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                if (Math.Abs(Angle) < Math.PI / 15)
                    finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
            }

            finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size);
            Teta = (vec).AngleInDegrees;
            return finalPosToGo;
        }
        public static int? GetOurBallOwner(GameStrategyEngine engine, WorldModel Model, int RobotID, DefenderStates CurrentState, List<int> exclude)
        {

            List<int> tmpExclude = new List<int>();
            if (exclude != null)
                tmpExclude = exclude.ToList();
            tmpExclude.Add(RobotID);

            if (!GameParameters.IsInField(Model.BallState.Location, 0.1))
                return null;
            Position2D pos = new Position2D();
            double minDistOpp = 100;
            if (Model.Opponents.Count > 0)
                minDistOpp = Model.Opponents.Min(m => m.Value.Location.DistanceFrom(Model.BallState.Location));
            if (minDistOpp < 0.5)
            {
                return null;
            }



            pos = Model.OurRobots[RobotID].Location;

            if (pos.DistanceFrom(Model.BallStateFast.Location) > 0.8)
                return null;
            Vector2D ballSpeed = Model.BallStateFast.Speed;
            double v = Vector2D.AngleBetweenInRadians(ballSpeed, (pos - Model.BallStateFast.Location));
            double maxIncomming = 2, maxVertical = 0.5, maxOutGoing = 1;
            double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
                + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
                + maxVertical;


            double stateCoef = 1;
            if (CurrentState == DefenderStates.BallInFront)
                stateCoef = 1.2;

            if (ballSpeed.Size < acceptableballRobotSpeed * stateCoef)
            {
                double accour = 2, accopp = 3;

                //double dist = Model.OurRobots.Min(m => m.Value.Location.DistanceFrom(Model.BallState.Location));
                //var robot = Model.OurRobots.First(f => f.Value.Location.DistanceFrom(Model.BallState.Location) == dist);

                var T_our = Model.OurRobots.Where(w => w.Key == RobotID).Select(s => new
                {
                    robotID = s.Key,
                    t = 2 * Math.Sqrt(s.Value.Location.DistanceFrom(Model.BallState.Location) / accour)
                });
                int goalieId = (Model.GoalieID.HasValue) ? Model.GoalieID.Value : -1;
                var Our_other = Model.OurRobots.Where(w => !tmpExclude.Contains(w.Key) && w.Key != goalieId).Select(s => new
                {
                    t = Math.Sqrt((2 * s.Value.Location.DistanceFrom(Model.BallState.Location)) / accopp)
                });
                var opp = Model.Opponents.Where(w => w.Value.Location.DistanceFrom(Model.BallState.Location) == minDistOpp).Select(s => new
                {
                    t = Math.Sqrt((2 * s.Value.Location.DistanceFrom(Model.BallState.Location)) / accopp)
                });
                var T_other = Our_other.Union(opp);
                double minT_other = 100;
                double minT_our = 100;
                if (T_other.Count() > 0)
                    minT_other = T_other.Min(m => m.t);
                if (T_our.Count() > 0)
                    minT_our = T_our.Min(m => m.t);

                if (minT_our < minT_other * stateCoef)
                {
                    return T_our.First(f => f.t == minT_our).robotID;

                }
            }
            return null;
        }
        static double tempDist = 0;
        static double tempDist2 = 0;

        public static void ResetBehindstate()
        {
            counter2 = 0;
            firstTime = true;
        }

        public static bool BallInBehind(GameStrategyEngine engine, WorldModel Model, int RobotID, bool SwitchToNormal, ref bool Normal)
        {
            if (SwitchToNormal)
            {
                if (!GameParameters.IsInDangerousZone(Model.BallState.Location, false, .16, out tempDist, out tempDist2) || Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) > .32 || Model.BallState.Speed.Size > .1)
                {
                    Normal = true;
                    DataBridge.BallBehind = false;
                    DataBridge.BallBehindID = 1000;
                }
            }
            else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, .13, out tempDist, out tempDist2) && !GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out tempDist, out tempDist2) && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .30 && Model.BallState.Speed.Size < .05)
            {
                DataBridge.BallBehind = true;
                DataBridge.BallBehindID = RobotID;
                return true;
            }
            return false;
            #region Comment
            //double tresh = .15;
            //bool BallInCircle = false, BallInHalfCircle = false, BallInTriangle = false;
            //double TreshForGoal = 0;
            //double TreshOurRobot = 0;
            //if (SwitchToNormal)
            //{
            //    TreshForGoal = 0.1;
            //    TreshOurRobot = 0.1;
            //    tresh = 0.15 + TreshForGoal + 0.05;
            //}
            //Vector2D robottoGoal = Model.OurRobots[RobotID].Location - GameParameters.OurGoalCenter;
            //Position2D OurRobotLoc = Model.OurRobots[RobotID].Location + robottoGoal.GetNormalizeToCopy(TreshOurRobot);
            //SingleObjectState Ball = Model.BallState;
            //Vector2D RobotToGoalRight = GameParameters.OurGoalRight.Extend(0, -TreshForGoal) - OurRobotLoc;
            //Vector2D RobotToGoalLeft = GameParameters.OurGoalLeft.Extend(0, TreshForGoal) - OurRobotLoc;
            //Vector2D RobottoBall = Ball.Location - OurRobotLoc;
            //double GoalRightToRobotAng = RobotToGoalRight.AngleInDegrees;
            //double GoalLeftToRobotAng = RobotToGoalLeft.AngleInDegrees;
            //double SmallAng = (GoalLeftToRobotAng > GoalRightToRobotAng) ? GoalRightToRobotAng : GoalLeftToRobotAng;
            //double BigAng = (GoalLeftToRobotAng > GoalRightToRobotAng) ? GoalLeftToRobotAng : GoalRightToRobotAng;
            //double BallToRobotAng = RobottoBall.AngleInDegrees;


            //if (Ball.Location.DistanceFrom(OurRobotLoc) < tresh)
            //{
            //    BallInCircle = true;
            //}
            //if (Ball.Location.DistanceFrom(GameParameters.OurGoalCenter) < OurRobotLoc.DistanceFrom(GameParameters.OurGoalCenter))
            //{
            //    BallInHalfCircle = true;
            //}
            //if (BallToRobotAng < BigAng && BallToRobotAng > SmallAng)
            //{
            //    BallInTriangle = true;
            //}
            //if ((BallInCircle && BallInHalfCircle) || BallInTriangle)
            //{
            //    Normal = false;
            //    return true;
            //}
            //Normal = true;
            //return false;
            #endregion
        }
        public static bool BallKickedToGoal(GameStrategyEngine engine, WorldModel Model)
        {
            Line line = new Line();
            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.15 && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.15)
                if (Model.BallState.Speed.InnerProduct(GameParameters.OurGoalRight - Model.BallState.Location) > 0)
                    if (Model.BallState.Speed.Size > 0.1 && d / Model.BallState.Speed.Size < 2.2)
                        return true;
            return false;
        }
        static bool firstTime = true;
        static double angle = 0;
        static Position2D pos = new Position2D();
        static int counter2 = 0;
        static SingleObjectState BehindTarget(WorldModel Model, int RobotID)
        {
            if (firstTime)
            {
                firstTime = false;
                angle = Model.OurRobots[RobotID].Angle.Value;
                pos = Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.14);
            }
            if (Model.OurRobots[RobotID].Location.DistanceFrom(pos) < .03)
            {
                counter2++;
            }
            else
            {
                counter2 = 0;
            }
            if (counter2 > 40)
            {
                pos = Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.06);
            }
            //Planner.ChangeDefaulteParams(5 , false);
            //Planner.SetParameter(5, 10, 10);
            //Planner.Add(5, pos, angle, PathType.UnSafe, true, true, true, false);
            return new SingleObjectState(pos, Vector2D.Zero, (float)angle);
            //Planner.Add(9, Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(.1), (Model.BallState.Location - Model.OurRobots[5].Location).AngleInDegrees, PathType.UnSafe, true, true, false, false);
        }
    }
    public static class CommonDefenceUtils
    {
        public static double StopZone = 0.6;
        public static double SafeStopZoneMargin = 0.6;
        public static double StopZoneMarker = 0.7;
        public static double SafeStopZoneMarginMarker = 0.7;
        public static Position2D CheckForStopZone(bool BallIsMoved, Position2D pos, WorldModel Model/*, SingleObjectState TargetState*/)
        {
            double dist, DistFromBorder;
            if (!BallIsMoved && pos.DistanceFrom(Model.BallState.Location) < CommonDefenceUtils.StopZone && !GameParameters.IsInDangerousZone(Model.BallState.Location, false, CommonDefenceUtils.SafeStopZoneMargin, out dist, out DistFromBorder))
            {
                Circle c = new Circle(Model.BallState.Location, CommonDefenceUtils.StopZone);
                Line l = new Line(/*TargetState.Location*/GameParameters.OurGoalCenter, pos);
                List<Position2D> inter = c.Intersect(l);
                var tar = inter.OrderBy(o => o.DistanceFrom(GameParameters.OurGoalCenter)).FirstOrDefault();

                if (tar != null)
                    pos = tar;
                else
                    pos = Model.BallState.Location + (Model.BallState.Location - pos).GetNormalizeToCopy(-CommonDefenceUtils.StopZone);
            }
            return pos;
        }

        public static Position2D CheckForStopZoneMarker(bool BallIsMoved, Position2D pos, WorldModel Model, SingleObjectState TargetState)
        {
            double dist, DistFromBorder;
            if (!BallIsMoved && pos.DistanceFrom(Model.BallState.Location) < CommonDefenceUtils.StopZoneMarker && !GameParameters.IsInDangerousZone(Model.BallState.Location, false, CommonDefenceUtils.SafeStopZoneMarginMarker, out dist, out DistFromBorder))
            {
                Circle c = new Circle(Model.BallState.Location, CommonDefenceUtils.StopZoneMarker);
                Line l = new Line(/*TargetState.Location*/GameParameters.OurGoalCenter, pos);
                List<Position2D> inter = c.Intersect(l);
                var tar = inter.OrderBy(o => o.DistanceFrom(GameParameters.OurGoalCenter)).FirstOrDefault();

                if (tar != null)
                    pos = tar;
                else
                    pos = Model.BallState.Location + (Model.BallState.Location - pos).GetNormalizeToCopy(-CommonDefenceUtils.StopZoneMarker);
            }
            return pos;
        }
    }
    public static class MarkerDefenceUtils
    {
        public static double MinDistMarkMargin = FreekickDefence.AdditionalSafeRadi;
        public static double MarkFromDist = 0.25;
        public static double MaxMarkDist = 2;
    }
    public static class OppFreeKickMarkerUtils
    {
        public static double MinDistBehindFromZone = 0.2;
        public static double DistCutPassFromOpp = 0.22;
        public static double passSpeedTresh = 0.5;
        public static double passAngleTresh = 15;
        public static double prependDistTresh = 0.25;
        public static double passDistTresh = 0.55;
        public static double robotXTresh = 3;
        public static bool PassedToOpponent(WorldModel Model, int RobotID, int? oppid, Position2D? DefenderLocation, ref bool wasAcc)
        {
            bool acceptableBall = false;
            if (!oppid.HasValue || !Model.Opponents.ContainsKey(oppid.Value))
                return false;

            Vector2D ballRobot = Model.Opponents[oppid.Value].Location - Model.BallState.Location;
            Vector2D ballSpeed = Model.BallState.Speed;
            double inner = ballRobot.InnerProduct(ballSpeed);
            double dist = Model.OurRobots[RobotID].Location.DistanceFrom(Model.Opponents[oppid.Value].Location);

            double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobot, ballSpeed));
            double prependDist = Model.Opponents[oppid.Value].Location.DistanceFrom(ballSpeed.PrependecularPoint(Model.BallState.Location, Model.Opponents[oppid.Value].Location));
            double robotx = Model.Opponents[oppid.Value].Location.X;
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(0, 0, 1, 0, Model.OurRobots.Keys.ToList(), Model.Opponents.Keys.ToList());

            double BallSpeed = Model.BallState.Speed.Size + .5;
            double BallToOpp = Model.Opponents[oppid.Value].Location.DistanceFrom(Model.BallState.Location);
            double distforvelocitycal = BallToOpp - .22;
            double BallArriveTime = distforvelocitycal / BallSpeed;
            int BallTimeFrameCount = (int)Math.Floor(BallArriveTime / StaticVariables.FRAME_PERIOD);
            double RobotSpeed = .7;
            double OurRobotToPoint = Model.OurRobots[RobotID].Location.DistanceFrom(Model.Opponents[oppid.Value].Location + (Model.BallState.Location - Model.Opponents[oppid.Value].Location).GetNormalizeToCopy(.22));
            double RobotArriveTime = OurRobotToPoint / RobotSpeed;
            int RobotArriveTimeFrame = (int)Math.Floor(RobotArriveTime / StaticVariables.FRAME_PERIOD);
            RobotArriveTime = 60;

            Vector2D BallOppRobot = Model.BallState.Location - Model.Opponents[oppid.Value].Location;
            Vector2D OurRobotOppRobot = Model.OurRobots[RobotID].Location - Model.Opponents[oppid.Value].Location;
            double InnerForBehindPoint = BallOppRobot.InnerProduct(OurRobotOppRobot);
            bool met = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10), Vector2D.Zero, 0), 0.03);

            //if (oppid.Value == 0)
            //{
            //    DrawingObjects.AddObject(new StringDraw("BallSpeed" + BallSpeed.ToString(), new Position2D(-2, 2)));
            //    DrawingObjects.AddObject(new StringDraw("BallToOpp" + BallToOpp.ToString(), new Position2D(-2.1, 2)));
            //    DrawingObjects.AddObject(new StringDraw("BallTimeFrameCount" + BallTimeFrameCount.ToString(), new Position2D(-2.2, 2)));
            //    DrawingObjects.AddObject(new StringDraw("Defender Location Dist" + DefenderLocation.Value.DistanceFrom(Model.OurRobots[RobotID].Location).ToString(), new Position2D(-2.3, 2)));
            //    DrawingObjects.AddObject(new StringDraw("OurRobotSpeed" + Model.OurRobots[RobotID].Speed.Size.ToString(), new Position2D(-2.4, 2)));

            //    DrawingObjects.AddObject(new StringDraw("dist" + dist.ToString(), new Position2D(-2.5, 2)));
            //    DrawingObjects.AddObject(new StringDraw("angle" + angle.ToString(), new Position2D(-2.6, 2)));
            //    DrawingObjects.AddObject(new StringDraw("Inner" + inner.ToString(), new Position2D(-2.7, 2)));
            //    DrawingObjects.AddObject(new StringDraw("ballSpeedSize" + Model.BallState.Speed.Size.ToString(), new Position2D(-2.8, 2)));
            //    DrawingObjects.AddObject(new StringDraw("InnerForBehindPoint" + InnerForBehindPoint.ToString(), new Position2D(-2.9, 2)));
            //    DrawingObjects.AddObject(new StringDraw("met" + met.ToString(), new Position2D(-3, 2)));
            //    DrawingObjects.AddObject(new StringDraw("perpDist: " + prependDist.ToString(), new Position2D(-3.1, 2)));
            //}
            if ((wasAcc || BallTimeFrameCount > RobotArriveTime) && Model.BallState.Speed.Size < 4.5 && Model.BallState.Speed.Size > 0.5)
            {
                wasAcc = true;
                acceptableBall = true;
            }
            if (DefenderLocation.HasValue)
            {
                bool b = acceptableBall && inner > 0 /*&& DefenderLocation.Value.DistanceFrom(Model.OurRobots[RobotID].Location) < .1 && Model.OurRobots[RobotID].Speed.Size < .6*/ && InnerForBehindPoint > 0.0 && /*angle < passAngleTresh &&*/ prependDist < prependDistTresh && dist < passDistTresh && robotx < robotXTresh && !met;
                if (b)
                {
                    DrawingObjects.AddObject(new StringDraw("CoverRobot", Model.BallState.Location.Extend(-2.8, 2)));
                    return true;
                }
                return false;
                //else if (inner < 0 || Model.BallState.Speed.Size < .2 || InnerForBehindPoint < .1)
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}
            }


            return false;
        }
    }
    public static class OurFreeKickMarkerUtils
    {
        public static double MinDistBehindFromZone = 0.2;
        public static double DistCutPassFromOpp = 0.22;
        public static double passSpeedTresh = 0.5;
        public static double passAngleTresh = 30;
        public static double passDistTresh = 0.5;
        public static double robotXTresh = 0.5;
        public static bool PassedToOpponent(WorldModel Model, int RobotID, int? oppid)
        {
            if (!oppid.HasValue || !Model.Opponents.ContainsKey(oppid.Value))
                return false;

            Vector2D ballRobot = Model.Opponents[oppid.Value].Location - Model.BallState.Location;
            Vector2D ballSpeed = Model.BallState.Speed;
            double inner = ballRobot.InnerProduct(ballSpeed);
            double dist = Model.OurRobots[RobotID].Location.DistanceFrom(Model.Opponents[oppid.Value].Location);
            double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobot, ballSpeed));
            double robotx = Model.Opponents[oppid.Value].Location.X;
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(0, 0, 1, 0, Model.OurRobots.Keys.ToList(), Model.Opponents.Keys.ToList());
            if (Model.BallState.Speed.Size > passSpeedTresh && inner > 0 && angle < passAngleTresh && dist < passDistTresh && robotx < robotXTresh && !obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10), Vector2D.Zero, 0), 0.03))
                return true;
            return false;
        }
    }
    public static class StaticMarkerDefenceUtils
    {
        public static double MinDistMarkMargin = .04 + (Math.Max(0, FreekickDefence.AdditionalSafeRadi - .04));
        public static double MarkFromDist = 0.2;
        public static double MinDistBehindFromZone = 0.2;
        public static double DistCutPassFromOpp = 0.22;
        public static double passSpeedTresh = 0.5;
        public static double passAngleTresh = 30;
        public static double passDistTresh = 0.5;
        public static double robotXTresh = 2;
        public static bool PassedToOpponent(WorldModel Model, int RobotID, int? oppid)
        {
            if (!oppid.HasValue || !Model.Opponents.ContainsKey(oppid.Value))
                return false;

            Vector2D ballRobot = Model.Opponents[oppid.Value].Location - Model.BallState.Location;
            Vector2D ballSpeed = Model.BallState.Speed;
            double inner = ballRobot.InnerProduct(ballSpeed);
            double dist = Model.OurRobots[RobotID].Location.DistanceFrom(Model.Opponents[oppid.Value].Location);
            double angle = Math.Abs(Vector2D.AngleBetweenInDegrees(ballRobot, ballSpeed));
            double robotx = Model.Opponents[oppid.Value].Location.X;
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(0, 0, 1, 0, Model.OurRobots.Keys.ToList(), Model.Opponents.Keys.ToList());
            if (Model.BallState.Speed.Size > passSpeedTresh && inner > 0 && angle < passAngleTresh && dist < passDistTresh && robotx < robotXTresh && !obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10), Vector2D.Zero, 0), 0.03))
                return true;
            return false;
        }
    }
    public static class RegionalDefenceUtils
    {
        public static double RegionalRegion = 2;
    }

    public enum MarkerState
    {
        CoverOurGoal,
        CoverOppRobot
    }

    public class DefenderCommand
    {
        public bool willUse = true;
        public int? OppID
        {
            get;
            set;
        }
        public double MarkMaximumDist = 0;
        public double RegionalDistFromDangerZone = 0.05;
        public List<Position2D> RegionalDefendPoints = new List<Position2D>();
        public Type RoleType
        {
            get;
            set;
        }

    }
    public enum DefenderType
    {
        NormalFirst = 0,
        NormalSecond = 1,
        Marker = 2,
        Regional = 3,
        Goalie = 4,
    }
    public class DefenceInfo
    {
        public int? OppID = null;
        public Position2D? DefenderPosition = null;
        public SingleObjectState TargetState = null;
        public double Teta = 0;
        public Type RoleType
        {
            get;
            set;
        }
        public GoaliPositioningMode Mode
        {
            get;
            set;
        }
    }

    public enum RBstate
    {
        Robot,
        Ball
    }

    public enum GoaliPositioningMode
    {
        InLeftSide = 0,
        InRightSide = 1
    }

    //
    public enum PositionMode
    {
        InNear = 0,
        InNearMiddle = 1,
        InFarMiddle = 2,
        InFar = 3
    }
    public struct Forbiden
    {
        public double radius;
        public Position2D center;
    }
    public enum DefenderStates
    {
        None = -1,
        Normal = 0,
        InPenaltyArea = 1,
        Behind = 2,
        KickToGoal = 3,
        OppIndDangerZone = 4,
        BallInFront = 5,
        //KickToGoal2 = 6,
        Cut = 7,
        Break = 8,
        EatTheBall = 9
    }
    public enum CenterDefenderStates
    {
        None = -1,
        Normal = 0,
        InPenaltyArea = 1,
        Behind = 2,
        KickToGoal = 3,
        OppIndDangerZone = 4,
        BallInFront = 5,
        EatTheBall = 7,
        kickToRobot = 8
    }
    enum SelectSide
    {
        left,
        right,
        center
    }
}

