//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MRL.SSL.AIConsole.Engine;
//using MRL.SSL.GameDefinitions;
//using MRL.SSL.CommonClasses.MathLibrary;
//using MRL.SSL.AIConsole.Skills;
//using MRL.SSL.Planning.MotionPlanner;
//using System.Drawing;

//namespace MRL.SSL.AIConsole.Roles
//{
//    public class DefenderCornerRole1Test : RoleBase, IFirstDefender
//    {

//        public Position2D Target = new Position2D();
//        bool calculateCost = false;
//        public SingleObjectState ballState = new SingleObjectState();
//        public SingleObjectState ballStateFast = new SingleObjectState();
//        Position2D intermediatePos = new Position2D();
//        private static bool wehaveintersect = false;
//        private static int lastRobotID;
//        private bool gotointermediatepos = true;
//        private Position2D lastrobotpos = new Position2D();
//        private Position2D lastintersect = new Position2D();


//        private Position2D currentRobot = new Position2D();
//        private Position2D lastposRobot = new Position2D();

//        public void Run(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta)
//        {
//            intermediatePos = Target;
//            if (lastRobotID != RobotID)
//            {
//                wehaveintersect = false;
//            }


//            if (DefenceTest.BallTest)
//            {
//                ballState = DefenceTest.currentBallState;
//                ballStateFast = DefenceTest.currentBallState;
//            }
//            else
//            {
//                ballState = Model.BallState;
//                ballStateFast = Model.BallStateFast;
//            }
//            DefenceInfo inf = null;
//            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
//                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
//            double teta;
//            Target = Cost(engine, Model, RobotID, TargetPos, Teta, out teta, inf);


       

//            currentRobot = Model.OurRobots[RobotID].Location;
//            double motiontime = motionTime(RobotID, Model, Target, lastrobotpos);
//            DrawingObjects.AddObject(new StringDraw("Motion Time" + motiontime.ToString(), Model.OurRobots[RobotID].Location.Extend(1.2, 0)), "+65+64+656+54+66+54");
//            lastposRobot = Model.OurRobots[RobotID].Location;
//            //---------------------------| Collision Denied |------------------------------------------------------
//            if (!DataBridge.BallCutSituation)
//            {
//                Position2D intersect = IntersectFind(Model, RobotID);
//                if (intersect != new Position2D(100, 100) && Model.BallState.Speed.Size > .2)
//                {
//                    double ballcoeff = 1.73 * (Model.BallState.Speed.Size / intersect.DistanceFrom(Model.BallState.Location)) * Math.Sign(Model.BallState.Speed.InnerProduct(intersect - Model.BallState.Location));
//                    double robotCoeff = (Model.OurRobots[RobotID].Speed.Size / intersect.DistanceFrom(Model.OurRobots[RobotID].Location)) * Math.Sign(Model.OurRobots[RobotID].Speed.InnerProduct(intersect - Model.OurRobots[RobotID].Location));
//                    DrawingObjects.AddObject(new Circle(intersect, .1, new Pen(Brushes.Pink, .1f)), "897987956465");
//                    DrawingObjects.AddObject(new StringDraw(ballcoeff.ToString(), Model.BallState.Location.Extend(-1, 0)), "7537537537978");
//                    DrawingObjects.AddObject(new StringDraw(robotCoeff.ToString(), Model.OurRobots[RobotID].Location.Extend(-1, 0)), "3642345374373");
//                    if (ballcoeff > robotCoeff - .2 && ballcoeff < robotCoeff + .2 && ballcoeff > 0 && robotCoeff > 0 && Model.OurRobots[RobotID].Location.DistanceFrom(intersect) + 0 < Model.OurRobots[RobotID].Location.DistanceFrom(Target) && Model.BallState.Speed.InnerProduct(intersect - Model.BallState.Location) > 0 && Model.OurRobots[RobotID].Speed.InnerProduct(intersect - Model.OurRobots[RobotID].Location) > 0)
//                    {
//                        wehaveintersect = true;
//                    }
//                    if (wehaveintersect && firstTtime)
//                    {
//                        lastrobotpos = Model.OurRobots[RobotID].Location;
//                        lastintersect = intersect;
//                        DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, .7, new Pen(Brushes.Purple, .1f)), "3724243453");
//                        intermediatePos = (Target + (lastrobotpos - Target).GetNormalizeToCopy((lastintersect - Target).Size - .12)) + Model.BallState.Speed.GetNormalizeToCopy(.5);
//                        gotointermediatepos = true;
//                        firstTtime = false;
//                        DataBridge.BallCutSituation = true;
//                        DataBridge.BallCutPos = intermediatePos;
//                        DataBridge.CutBallRobotID = RobotID;
//                    }
//                }
//                if (gotointermediatepos)
//                {
//                    if (intermediatePos != new Position2D() && Model.BallState.Speed.InnerProduct(lastintersect - Model.BallState.Location) > 0)
//                    {
//                        //Target = intermediatePos;
//                    }
//                    else
//                    {
//                        gotointermediatepos = false;
//                        wehaveintersect = false;
//                        firstTtime = true;
//                    }
//                }
//                if (!wehaveintersect)
//                {
//                    DataBridge.BallCutSituation = false;
//                    gotointermediatepos = false;
//                }
//                DrawingObjects.AddObject(new Circle(intermediatePos, .05, new Pen(Brushes.DarkOrange, .05f)), "546454654987");
//            }
//            else
//            {
//                if (Model.BallState.Speed.InnerProduct(lastintersect - Model.BallState.Location) < 0 && RobotID == DataBridge.CutBallRobotID)
//                {
//                    DataBridge.BallCutSituation = false;
//                }
//                if (RobotID == DataBridge.CutBallRobotID)
//                {
//                    //Target = DataBridge.BallCutPos;
//                    //teta = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                }

//            }
//            //------------------------------------------------------------------------------------------------------------------------------------------------------
//            FreekickDefence.PreviousPositions[typeof(DefenderCornerRole1)] = Target;
//            //Position2D intersect = IntersectFind(Model, RobotID);
//            //double intersectDistance = intersect.DistanceFrom(Model.OurRobots[RobotID].Location);
//            //double targetDistance = Target.DistanceFrom(Model.OurRobots[RobotID].Location);
//            //double innerproduct = Model.OurRobots[RobotID].Speed.InnerProduct(intersect - Model.OurRobots[RobotID].Location);

//            Planner.Add(RobotID, Target, teta, PathType.UnSafe, false, false, true, false);

//            Obstacles obs = new Obstacles(Model);
//            obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
//            Vector2D v = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 0.45);
//            double kickSpeed = 255;
//            if (obs.Meet(ballState, new SingleObjectState(ballState.Location + v, Vector2D.Zero, 0), 0.022) || Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1).GetNormalizeToCopy(1).InnerProduct(GameParameters.OurGoalCenter - Model.OurRobots[RobotID].Location) > .5)
//                kickSpeed = 0;
//            lastRobotID = RobotID;
//        }

//        #region Comment
//        /*
         
//          public Position2D MarkFront(GameStrategyEngine engine, WorldModel Model, int RobotID, DefenceInfo info, double margin, out double Teta)
//          {
//              SingleObjectState Target = (info != null && info.OppID.HasValue) ? Model.Opponents[info.OppID.Value] : ballState;
//              //Teta = (Target.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
//              Teta = (Target.Location - GameParameters.OurGoalCenter).AngleInDegrees;
//              double min = GameParameters.SafeRadi(Target, margin);
//              Position2D Pos = GameParameters.OurGoalCenter - (GameParameters.OurGoalCenter - Target.Location).GetNormalizeToCopy(min);
//              Pos.DrawColor = System.Drawing.Color.Red;
//              DrawingObjects.AddObject(Pos);
//              return Pos;
//          }

//          public Position2D Dive(GameStrategyEngine engine, WorldModel Model, int RobotID)
//          {
//              Position2D pos = new Position2D();
//              Position2D robotLoc = Model.OurRobots[RobotID].Location;
//              Position2D ballLoc = ballState.Location;
//              Vector2D ballSpeed = ballState.Speed;
//              Position2D prep = ballSpeed.PrependecularPoint(ballState.Location, Model.OurRobots[RobotID].Location);
//              double dist, DistFromBorder, R;
//              if (GameParameters.IsInDangerousZone(prep, false, 0.15, out dist, out DistFromBorder))
//              {
//                  R = GameParameters.SafeRadi(new SingleObjectState(prep, new Vector2D(), 0), 0.05);
//                  pos = GameParameters.OurGoalCenter - ballSpeed.GetNormalizeToCopy(R);
//              }
//              else
//                  pos = prep;
//              return pos;
//          }

//          public Position2D BehindSatate(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta)
//          {
//              double dist;
//              Position2D target;
//              if (inf != null && inf.OppID.HasValue)
//              {
//                  if (FreekickDefence.CurrentStates.Any(a => a.Key.GetType() == typeof(GoalieCornerRole)) && (DefenderStates)FreekickDefence.CurrentStates.Where(w => w.Key.GetType() == typeof(GoalieCornerRole)).First().Value == DefenderStates.InPenaltyArea)
//                      target = InPenaltyAreaState(engine, Model, inf, RobotID, out Teta);
//                  else if (Model.Opponents[inf.OppID.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) > 2)
//                      target = GetBackBallPoint(Model, RobotID, out Teta);
//                  else
//                      target = MarkFront(engine, Model, RobotID, inf, 0.1, out Teta);
//              }
//              else
//                  target = GetBackBallPoint(Model, RobotID, out Teta);

//              return target;
//          }
        
//          public Position2D InPenaltyAreaState(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta)
//          {
//              Vector2D vec;
//              if (ballState.Location.Y >= 0)
//                  vec = new Vector2D(0, -0.2);
//              else
//                  vec = new Vector2D(0, 0.2);
//              double R = GameParameters.SafeRadi(ballState, 0.1);
//              Position2D target = GameParameters.OurGoalCenter + (ballState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(R);
//              target = target + vec;
//              Teta = (ballState.Location - target).AngleInDegrees;
//              return target;
//          }
        
//          public Position2D GetBackBallPoint(WorldModel Model, int RobotID, out double Teta)
//          {
//              Vector2D vec = ballState.Location - GameParameters.OurGoalCenter;
//              Position2D tar = ballState.Location + vec;
//              Vector2D ballSpeed = ballState.Speed;
//              Position2D ballLocation = ballState.Location;
//              Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
//              Position2D robotLocation = Model.OurRobots[RobotID].Location;
//              Vector2D robotBallVec = ballLocation - robotLocation;
//              Vector2D ballTargetVec = tar - ballLocation;
//              Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(0.09);
//              Vector2D robotBackBallVec = backBallPoint - robotLocation;
//              Vector2D ballBackBallVec = backBallPoint - ballLocation;
//              double segmentConst = 0.7;
//              double rearDistance = 0.15;
//              Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
//              Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
//              Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
//              Position2D midPoint = p1;
//              if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
//              {
//                  midPoint = p2;
//              }
//              Position2D finalPosToGo = midPoint;
//              double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
//              double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

//              double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));

//              if (Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < Math.PI / 6 && (Math.Abs(alfa) > Math.PI / 1.5 || Math.Abs(distance) > RobotParameters.OurRobotParams.Diameter / 2 + .01))
//                  finalPosToGo = backBallPoint;
//              else
//              {
//                  Vector2D robotMidPointVec = finalPosToGo - robotLocation;
//                  double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
//                  if (Math.Abs(Angle) < Math.PI / 15)
//                      finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
//              }

//              finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size);
//              Teta = (vec).AngleInDegrees;
//              return finalPosToGo;
//          }

//          public int? GetOurBallOwner(WorldModel Model, int RobotID, List<int> exclude)
//          {

//              var tmpExclude = exclude.ToList();
//              tmpExclude.Add(RobotID);

//              if (!GameParameters.IsInField(ballState.Location, 0.1))
//                  return null;
//              Position2D pos = new Position2D();
//              double minDistOpp = 100;
//              if (Model.Opponents.Count > 0)
//                  minDistOpp = Model.Opponents.Min(m => m.Value.Location.DistanceFrom(ballState.Location));
//              if (minDistOpp < 0.5)
//              {
//                  return null;
//              }



//              pos = Model.OurRobots[RobotID].Location;

//              if (pos.DistanceFrom(ballStateFast.Location) > 2)
//                  return null;
//              Vector2D ballSpeed = ballStateFast.Speed;
//              double v = Vector2D.AngleBetweenInRadians(ballSpeed, (pos - ballStateFast.Location));
//              double maxIncomming = 2, maxVertical = 0.5, maxOutGoing = 1;
//              double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
//                  + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
//                  + maxVertical;


//              double stateCoef = 1;
//              if (Cstate == DefenderStates.BallInFront)
//                  stateCoef = 1.2;

//              if (ballSpeed.Size < acceptableballRobotSpeed * stateCoef)
//              {
//                  double accour = 2, accopp = 3;

//                  //double dist = Model.OurRobots.Min(m => m.Value.Location.DistanceFrom(ballState.Location));
//                  //var robot = Model.OurRobots.First(f => f.Value.Location.DistanceFrom(ballState.Location) == dist);

//                  var T_our = Model.OurRobots.Where(w => w.Key == RobotID).Select(s => new
//                  {
//                      robotID = s.Key,
//                      t = 2 * Math.Sqrt(s.Value.Location.DistanceFrom(ballState.Location) / accour)
//                  });
//                  int goalieId = (Model.GoalieID.HasValue) ? Model.GoalieID.Value : -1;
//                  var Our_other = Model.OurRobots.Where(w => !tmpExclude.Contains(w.Key) && w.Key != goalieId).Select(s => new
//                  {
//                      t = Math.Sqrt((2 * s.Value.Location.DistanceFrom(ballState.Location)) / accopp)
//                  });
//                  var opp = Model.Opponents.Where(w => w.Value.Location.DistanceFrom(ballState.Location) == minDistOpp).Select(s => new
//                  {
//                      t = Math.Sqrt((2 * s.Value.Location.DistanceFrom(ballState.Location)) / accopp)
//                  });
//                  var T_other = Our_other.Union(opp);
//                  double minT_other = 100;
//                  double minT_our = 100;
//                  if (T_other.Count() > 0)
//                      minT_other = T_other.Min(m => m.t);
//                  if (T_our.Count() > 0)
//                      minT_our = T_our.Min(m => m.t);

//                  if (minT_our < minT_other * stateCoef)
//                  {
//                      return T_our.First(f => f.t == minT_our).robotID;

//                  }
//              }
//              return null;
//          }

//          private bool BallInBehind(WorldModel Model, int RobotID, bool SwitchToNormal, ref bool Normal)
//          {
//              double tresh = .15;
//              bool BallInCircle = false, BallInHalfCircle = false, BallInTriangle = false;
//              double TreshForGoal = 0;
//              double TreshOurRobot = 0;
//              if (SwitchToNormal)
//              {
//                  TreshForGoal = 0.1;
//                  TreshOurRobot = 0.1;
//                  tresh = 0.15 + TreshForGoal + 0.05;
//              }
//              Vector2D robottoGoal = Model.OurRobots[RobotID].Location - GameParameters.OurGoalCenter;
//              Position2D OurRobotLoc = Model.OurRobots[RobotID].Location + robottoGoal.GetNormalizeToCopy(TreshOurRobot);
//              SingleObjectState Ball = ballState;
//              Vector2D RobotToGoalRight = GameParameters.OurGoalRight.Extend(0, -TreshForGoal) - OurRobotLoc;
//              Vector2D RobotToGoalLeft = GameParameters.OurGoalLeft.Extend(0, TreshForGoal) - OurRobotLoc;
//              Vector2D RobottoBall = Ball.Location - OurRobotLoc;
//              double GoalRightToRobotAng = RobotToGoalRight.AngleInDegrees;
//              double GoalLeftToRobotAng = RobotToGoalLeft.AngleInDegrees;
//              double SmallAng = (GoalLeftToRobotAng > GoalRightToRobotAng) ? GoalRightToRobotAng : GoalLeftToRobotAng;
//              double BigAng = (GoalLeftToRobotAng > GoalRightToRobotAng) ? GoalLeftToRobotAng : GoalRightToRobotAng;
//              double BallToRobotAng = RobottoBall.AngleInDegrees;


//              if (Ball.Location.DistanceFrom(OurRobotLoc) < tresh)
//              {
//                  BallInCircle = true;
//              }
//              if (Ball.Location.DistanceFrom(GameParameters.OurGoalCenter) < OurRobotLoc.DistanceFrom(GameParameters.OurGoalCenter))
//              {
//                  BallInHalfCircle = true;
//              }
//              if (BallToRobotAng < BigAng && BallToRobotAng > SmallAng)
//              {
//                  BallInTriangle = true;
//              }
//              if ((BallInCircle && BallInHalfCircle) || BallInTriangle)
//              {
//                  Normal = false;
//                  return true;

//              }
//              Normal = true;
//              return false;

//          }

//          private bool BallKickedToGoal(WorldModel Model)
//          {
//              Line line = new Line();
//              line = new Line(ballState.Location, ballState.Location - ballState.Speed);
//              Position2D BallGoal = new Position2D();
//              BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
//              double d = ballState.Location.DistanceFrom(GameParameters.OurGoalCenter);
//              if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.15 && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.15)
//                  if (ballState.Speed.InnerProduct(GameParameters.OurGoalRight - ballState.Location) > 0)
//                      if (ballState.Speed.Size > 0.1 && d / ballState.Speed.Size < 2.2)
//                          return true;
//              return false;
//          }
//          public enum DefenderStates
//          {
//              Normal = 0,
//              InPenaltyArea = 1,
//              Behind = 2,
//              KickToGoal = 3,
//              OppIndDangerZone = 4,
//              BallInFront = 5,
//          }
//         * */
//        #endregion

//        int counterBalInFront = 0;
//        private bool firstTtime = true;
//        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
//        {

//            double dist, dist2;

//            DefenceInfo inf = null;
//            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
//                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();

//            var excludeIds = AssignedRoles.Where(w => w.Value.GetType() == typeof(DefenderCornerRole2)).Select(s => s.Key).ToList();

//            bool NormalFromBehind = false;
//            int? ballOwner = OppFreeKickDefenceUtils.GetOurBallOwner(engine, Model, RobotID, (DefenderStates)CurrentState, excludeIds);//engine.GameInfo.OurTeam.BallOwner;

//            if (CurrentState == (int)DefenderStates.Normal)
//            {
//                if (OppFreeKickDefenceUtils.BallKickedToGoal(engine, Model))
//                {
//                    CurrentState = (int)DefenderStates.KickToGoal;
//                }
//                else if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.Normal2InPenaltyAreaMargin, out dist, out dist2))
//                {
//                    CurrentState = (int)DefenderStates.InPenaltyArea;
//                }
//                else if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind))//ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
//                {
//                    CurrentState = (int)DefenderStates.Behind;
//                }
//                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
//                {
//                    CurrentState = (int)DefenderStates.BallInFront;
//                }
//                else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.Normal2OppInDangerZoneMargin, out dist, out dist2))//  inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) < tresh )
//                {
//                    CurrentState = (int)DefenderStates.OppIndDangerZone;
//                }
//            }
//            else if (CurrentState == (int)DefenderStates.Behind)
//            {
//                OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, true, ref NormalFromBehind);

//                if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.Behind2InPenaltyAreaMargin, out dist, out dist2))
//                {
//                    CurrentState = (int)DefenderStates.InPenaltyArea;
//                }
//                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
//                {
//                    CurrentState = (int)DefenderStates.BallInFront;
//                }
//                else if (NormalFromBehind)
//                {
//                    CurrentState = (int)DefenderStates.Normal;
//                }
//            }
//            else if (CurrentState == (int)DefenderStates.InPenaltyArea)
//            {

//                if (!GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.InPenaltyArea2NormalMargin, out dist, out dist2))
//                {
//                    if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind))
//                    {
//                        CurrentState = (int)DefenderStates.Behind;
//                    }
//                    else
//                        CurrentState = (int)DefenderStates.Normal;
//                }

//            }
//            else if (CurrentState == (int)DefenderStates.KickToGoal)
//            {
//                if (!OppFreeKickDefenceUtils.BallKickedToGoal(engine, Model))
//                {
//                    if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.K2G2InPenaltyAreaMargin, out dist, out dist2))
//                    {
//                        CurrentState = (int)DefenderStates.InPenaltyArea;
//                    }
//                    else if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind))//hadi//ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
//                    {
//                        CurrentState = (int)DefenderStates.Behind;
//                    }
//                    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
//                    {
//                        CurrentState = (int)DefenderStates.BallInFront;
//                    }
//                    else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, 0.1, out dist, out dist2))// inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) < tresh )
//                    {
//                        CurrentState = (int)DefenderStates.OppIndDangerZone;
//                    }
//                    else
//                        CurrentState = (int)DefenderStates.Normal;
//                }
//            }
//            else if (CurrentState == (int)DefenderStates.BallInFront)
//            {
//                counterBalInFront++;
//                if (engine.Status == GameStatus.Stop || !ControlParameters.BallIsMoved)
//                {
//                    CurrentState = (int)DefenderStates.Normal;
//                }
//                else if (counterBalInFront > 30)
//                {
//                    if (!ballOwner.HasValue || (ballOwner.HasValue && ballOwner.Value != RobotID))
//                    {
//                        if (OppFreeKickDefenceUtils.BallKickedToGoal(engine, Model))
//                        {
//                            CurrentState = (int)DefenderStates.KickToGoal;
//                        }
//                        else if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.BallInFront2InPenaltyAreaMargin, out dist, out dist2))
//                        {
//                            CurrentState = (int)DefenderStates.InPenaltyArea;
//                        }
//                        else if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind))
//                        {
//                            CurrentState = (int)DefenderStates.Behind;
//                        }
//                        else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, 0.1, out dist, out dist2))
//                        {
//                            CurrentState = (int)DefenderStates.OppIndDangerZone;
//                        }
//                        else
//                            CurrentState = (int)DefenderStates.Normal;
//                    }
//                }
//            }
//            else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
//            {

//                if (inf == null || (inf != null && inf.TargetState.Type != ObjectType.Opponent) || (inf != null && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.OppDanger2OppDangerMargin, out dist, out dist2)))
//                {
//                    if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.OppInDanger2InPenaltyAreaMargin, out dist, out dist2))
//                    {
//                        CurrentState = (int)DefenderStates.InPenaltyArea;
//                    }
//                    else if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind))
//                    {
//                        CurrentState = (int)DefenderStates.Behind;
//                    }
//                    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
//                    {
//                        CurrentState = (int)DefenderStates.BallInFront;
//                    }
//                    else
//                        CurrentState = (int)DefenderStates.Normal;
//                }
//                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
//                {
//                    CurrentState = (int)DefenderStates.BallInFront;
//                }
//            }
//            if (CurrentState != (int)DefenderStates.BallInFront)
//                counterBalInFront = 0;
//            DrawingObjects.AddObject(new StringDraw(((DefenderStates)CurrentState).ToString(), Model.OurRobots[RobotID].Location + new Vector2D(0.25, 0)));
//            if (!calculateCost)
//                FreekickDefence.CurrentStates[this] = CurrentState;
//        }

//        public override RoleCategory QueryCategory()
//        {
//            return RoleCategory.Defender;
//        }

//        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            DefenceInfo inf = null;

//            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
//                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
//            if (inf != null)
//            {
//                double teta;
//                calculateCost = true;
//                //      DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles);

//                CurrentState = (FreekickDefence.CurrentStates.ContainsKey(this)) ? FreekickDefence.CurrentStates[this] : CurrentState;
//                Position2D pos = Cost(engine, Model, RobotID, inf.DefenderPosition.Value, inf.Teta, out teta, inf);
//                double cost = pos.DistanceFrom(Model.OurRobots[RobotID].Location);
//                return cost * cost;
//            }

//            return 100;
//        }

//        public Position2D Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta, out double teta, DefenceInfo inf)
//        {
//            inf = new DefenceInfo();
//            teta = 180;
//            Position2D target = Position2D.Zero;
//            CurrentState = (int)DefenderStates.Behind;
//            inf.OppID = 0;
//            inf.TargetState = Model.Opponents[0];
//            if (CurrentState == (int)DefenderStates.InPenaltyArea)
//            {
//                target = OppFreeKickDefenceUtils.MarkFront(engine, Model, RobotID, inf, FreekickDefence.AdditionalSafeRadi, out teta);
//            }
//            else if (CurrentState == (int)DefenderStates.Behind)
//            {
//                target = OppFreeKickDefenceUtils.BehindSatate(engine, Model, inf, RobotID, out teta, FreekickDefence.CurrentStates);
//            }
//            else if (CurrentState == (int)DefenderStates.Normal)
//            {
//                target = TargetPos;
//                Vector2D vec = target - Model.OurRobots[RobotID].Location;
//                if (inf.TargetState.Speed.Size > 1)
//                    target = target + vec.GetNormalizeToCopy(Math.Min(inf.TargetState.Speed.Size * 0.2, 0.08));
//                teta = Teta;
//            }
//            else if (CurrentState == (int)DefenderStates.KickToGoal)
//            {
//                target = OppFreeKickDefenceUtils.Dive(engine, Model, RobotID);
//                teta = Model.OurRobots[RobotID].Angle.Value;
//            }
//            else if (CurrentState == (int)DefenderStates.BallInFront)
//            {
//                target = OppFreeKickDefenceUtils.GetBackBallPoint(engine, Model, RobotID, out teta);
//            }
//            else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
//            {
//                if (inf != null)
//                {
//                    int? oppid = inf.OppID;
//                    if (oppid.HasValue)
//                    {
//                        Vector2D vec = (Model.Opponents[oppid.Value].Location - Model.OurRobots[RobotID].Location);
//                        target = Model.Opponents[oppid.Value].Location + vec.GetNormalizeToCopy(0.2);
//                        teta = (target - Model.OurRobots[RobotID].Location).AngleInDegrees;
//                    }
//                }
//            }
//            if ((CurrentState == (int)DefenderStates.Normal && inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X) || (CurrentState == (int)DefenderStates.Behind && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X))
//            {
//                target = new Position2D(2.9, target.Y);
//            }
//            //   DrawingObjects.AddObject(new Circle(target, 0.03));
//            return target;
//        }

//        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            List<RoleBase> res = new List<RoleBase>() { 
//                new DefenderCornerRole1(), 
//                new DefenderCornerRole2(), 
//                new DefenderCornerRole3(),
//                new DefenderCornerRole4(),
//                new RegionalDefenderRole()
//            };
//            if (FreekickDefence.SwitchDefender2Marker1)
//            {
//                res.Add(new DefenderMarkerNormalRole1());
//                res.Add(new DefenderMarkerRole());
//                res.Add(new NewDefenderMrkerRole());
//            }
//            if (FreekickDefence.SwitchDefender2Marker2)
//            {
//                res.Add(new DefenderMarkerNormalRole2());
//                res.Add(new DefenderMarkerRole2());
//                res.Add(new NewDefenderMarkerRole2());
//            }
//            if (FreekickDefence.SwitchDefender2Marker3)
//            {
//                res.Add(new DefenderMarkerNormalRole3());
//                res.Add(new DefenderMarkerRole3());
//                res.Add(new NewDefenderMarkerRole3());
//            }

//            if (FreekickDefence.LastSwitchDefender2Marker1)//New IO2014
//            {
//                res.Add(new DefenderMarkerRole());
//                res.Add(new NewDefenderMrkerRole());
//            }
//            if (FreekickDefence.LastSwitchDefender2Marker2)//New IO2014
//            {
//                res.Add(new DefenderMarkerRole2());
//                res.Add(new NewDefenderMarkerRole2());
//            }
//            if (FreekickDefence.LastSwitchDefender2Marker3)//New IO2014
//            {
//                res.Add(new DefenderMarkerRole3());
//                res.Add(new NewDefenderMarkerRole3());
//            }
//            return res;
//        }

//        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            throw new NotImplementedException();
//        }

//        private Position2D IntersectFind(WorldModel model, int RobotID)
//        {
//            Position2D robotSpeedPos = model.OurRobots[RobotID].Location + model.OurRobots[RobotID].Speed;
//            Position2D ballspeedpos = model.BallState.Location + model.BallState.Speed;

//            double x4 = robotSpeedPos.X;
//            double x3 = model.OurRobots[RobotID].Location.X;
//            double y4 = robotSpeedPos.Y;
//            double y3 = model.OurRobots[RobotID].Location.Y;
//            double x2 = ballspeedpos.X;
//            double y2 = ballspeedpos.Y;
//            double x1 = model.BallState.Location.X;
//            double y1 = model.BallState.Location.Y;

//            //double x = (((((x1 * y2) - (y1 * x2)) * (x3 - x4)) - ((x1 - x2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));
//            //double y = (((((x1 * y2) - (y1 * x2)) * (y3 - y4)) - ((y1 - y2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));

//            Line first = new Line(new Position2D(x1, y1), new Position2D(x2, y2));
//            Line second = new Line(new Position2D(x3, y3), new Position2D(x4, y4));
//            Position2D intersect = new Position2D();
//            if (first.IntersectWithLine(second).HasValue)
//                intersect = first.IntersectWithLine(second).Value;
//            else
//            {
//                intersect = new Position2D(100, 100);
//            }
//            return intersect;
//        }

//        private double motionTime(int RobotID, WorldModel Model, Position2D target, Position2D lastRobotPos)
//        {
//            Position2D RobotPos = Model.OurRobots[RobotID].Location;
//            double distToTarget = RobotPos.DistanceFrom(target);
//            double timeRobot = (Planner.GetMotionTime(Model, RobotID, RobotPos, target, ActiveParameters.RobotMotionCoefs) * StaticVariables.FRAME_PERIOD) - ((ControlParameters.MaxSpeed - ((lastRobotPos.DistanceFrom(RobotPos)) / StaticVariables.FRAME_PERIOD)) / distToTarget);
//            return timeRobot;
//        }
//    }
//}
