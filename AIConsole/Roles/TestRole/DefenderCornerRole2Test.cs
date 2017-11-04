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
//    public class DefenderCornerRole2Test : RoleBase, ISecondDefender
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
//            double teta;
//            DefenceInfo inf = null;
//            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
//                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
//            Target = Cost(engine, Model, RobotID, TargetPos, Teta, inf, out teta);




//            currentRobot = Model.OurRobots[RobotID].Location;
//            double motiontime = motionTime(RobotID, Model, Target, lastrobotpos);
//            DrawingObjects.AddObject(new StringDraw("Motion Time" + motiontime.ToString(), Model.OurRobots[RobotID].Location.Extend(1.2, 0)), "98765964697646546");
//            lastposRobot = Model.OurRobots[RobotID].Location;

//            //-------------------------------- | Collision Denied | ------------------------------------------------------------------------------------------------

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
//                    if (ballcoeff > robotCoeff - .2 && ballcoeff < robotCoeff + .2 && ballcoeff > 0 && robotCoeff > 0 && Model.OurRobots[RobotID].Location.DistanceFrom(intersect) + .2 < Model.OurRobots[RobotID].Location.DistanceFrom(Target) && Model.BallState.Speed.InnerProduct(intersect - Model.BallState.Location) > 0 && Model.OurRobots[RobotID].Speed.InnerProduct(intersect - Model.OurRobots[RobotID].Location) > 0)
//                    {
//                        wehaveintersect = true;
//                    }
//                    if (wehaveintersect && firsttime)
//                    {
//                        lastrobotpos = Model.OurRobots[RobotID].Location;
//                        lastintersect = intersect;
//                        DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, .7, new Pen(Brushes.Purple, .1f)), "3724243453");
//                        intermediatePos = /*intersect + Model.BallState.Speed.GetNormalizeToCopy(.3);//*//*/ Target + (lastrobotpos - Target).GetNormalizeToCopy(.4) + Vector2D.FromAngleSize((lastrobotpos - Target).AngleInRadians + Math.PI / 2, .5)*/ (Target + (lastrobotpos - Target).GetNormalizeToCopy((lastintersect - Target).Size - 0)) + Model.BallState.Speed.GetNormalizeToCopy(.5);
//                        gotointermediatepos = true;
//                        firsttime = false;
//                        DataBridge.BallCutSituation = true;
//                        DataBridge.BallCutPos = intermediatePos;
//                        DataBridge.CutBallRobotIDCR2 = RobotID;
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
//                        firsttime = true;
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
//            FreekickDefence.PreviousPositions[typeof(DefenderCornerRole2)] = Target;
//            Planner.Add(RobotID, Target, teta, PathType.UnSafe, false, false, true, false);
//            Obstacles obs = new Obstacles(Model);
//            obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
//            Vector2D v = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 0.45);
//            double kickSpeed = 255;
//            if (obs.Meet(ballState, new SingleObjectState(ballState.Location + v, Vector2D.Zero, 0), 0.022) || Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1).GetNormalizeToCopy(1).InnerProduct(GameParameters.OurGoalCenter - Model.OurRobots[RobotID].Location) > .5)
//                kickSpeed = 0;
//        }


//        int counterBalInFront = 0;
//        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
//        {
//            double dist, dist2;
//            DefenceInfo inf = null;
//            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
//                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
//            var excludeIds = AssignedRoles.Where(w => w.Value.GetType() == typeof(DefenderCornerRole1)).Select(s => s.Key).ToList();
//            ControlParameters.BallIsMoved = false;
//            int? ballOwner = 2;//OppFreeKickDefenceUtils.GetOurBallOwner(engine, Model, RobotID, (DefenderStates)CurrentState, excludeIds);
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
//                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
//                {
//                    CurrentState = (int)DefenderStates.BallInFront;
//                }
//                else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.Normal2OppInDangerZoneMargin, out dist, out dist2))// inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) < tresh )
//                {
//                    CurrentState = (int)DefenderStates.OppIndDangerZone;
//                }
//            }
//            else if (CurrentState == (int)DefenderStates.InPenaltyArea)
//            {

//                if (!GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.InPenaltyArea2NormalMargin, out dist, out dist2))
//                {
//                    CurrentState = (int)DefenderStates.Normal;
//                }
//                else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.InPenaltyArea2OppDangerMargin, out dist, out dist2)) // inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) < tresh )
//                {
//                    CurrentState = (int)DefenderStates.OppIndDangerZone;
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
//                    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
//                    {
//                        CurrentState = (int)DefenderStates.BallInFront;
//                    }
//                    else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.K2G2OppDangerMargin, out dist, out dist2))// inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) < tresh )
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
//                        else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.BallInFront2OppDangerMargin, out dist, out dist2))// inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) < tresh )
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
//                if (inf == null || (inf != null && inf.TargetState.Type != ObjectType.Opponent) || (inf != null && !GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.OppDanger2OppDangerMargin, out dist, out dist2)))// inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) > tresh ) )
//                {
//                    if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.OppInDanger2InPenaltyAreaMargin, out dist, out dist2))
//                    {
//                        CurrentState = (int)DefenderStates.InPenaltyArea;
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
//            DrawingObjects.AddObject(new StringDraw(((DefenderStates)CurrentState).ToString(), Model.OurRobots[RobotID].Location + new Vector2D(0.25, 0)), "Def2");
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

//                CurrentState = (FreekickDefence.CurrentStates.ContainsKey(this)) ? FreekickDefence.CurrentStates[this] : CurrentState;
//                Position2D pos = Cost(engine, Model, RobotID, inf.DefenderPosition.Value, inf.Teta, inf, out teta);
//                double d = pos.DistanceFrom(Model.OurRobots[RobotID].Location);
//                return d * d;
//            }
//            return 100;
//        }
//        bool isInZone = false;
//        private bool firsttime = true;
//        public Position2D Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta, DefenceInfo inf, out double teta)
//        {
//            inf = new DefenceInfo();
//            inf.OppID = 0;
            
//            Position2D target = Position2D.Zero;
//            teta = 180;
//            CurrentState = (int)DefenderStates.OppIndDangerZone;
//            if (CurrentState == (int)DefenderStates.InPenaltyArea)
//            {
//                target = OppFreeKickDefenceUtils.MarkFront(engine, Model, RobotID, inf, FreekickDefence.AdditionalSafeRadi, out teta);
//            }
//            else if (CurrentState == (int)DefenderStates.Normal)
//            {
//                target = TargetPos;
//                Vector2D vec = target - Model.OurRobots[RobotID].Location;
//                if (inf.TargetState.Speed.Size > 1)
//                    target = target + vec.GetNormalizeToCopy(Math.Min(inf.TargetState.Speed.Size * 0.2, 0.08));
//                teta = Teta;
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
//                        Vector2D vec = -(Model.Opponents[oppid.Value].Location - Model.OurRobots[RobotID].Location);
//                        target = Model.Opponents[oppid.Value].Location + vec.GetNormalizeToCopy(0.2);
//                        double Mindist = GameParameters.SafeRadi(new SingleObjectState(target, Vector2D.Zero, 0), OppFreeKickMarkerUtils.MinDistBehindFromZone);
//                        bool meet = false;
//                        double d = target.DistanceFrom(GameParameters.OurGoalCenter);

//                        if (!isInZone && d < Mindist)
//                        {
//                            isInZone = true;
//                        }
//                        else if (isInZone && d > Mindist + 0.1)
//                        {
//                            isInZone = false;
//                        }
//                        if (isInZone)
//                        {
//                            Obstacles obstacles = new Obstacles(Model);

//                            List<int> exclude = new List<int> { RobotID, Model.GoalieID ?? 100 };
//                            obstacles.AddObstacle(1, 0, 0, 0, exclude, ((inf.OppID.HasValue) ? new List<int>() { inf.OppID.Value } : null));

//                            meet = obstacles.Meet(Model.OurRobots[RobotID], new SingleObjectState(GameParameters.OurGoalCenter, Vector2D.Zero, null), 0.07);
//                            if (meet)
//                            {
//                                target = GameParameters.OurGoalCenter + (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(target, Vector2D.Zero, 0), .22));
//                            }
//                        }

//                        teta = (target - GameParameters.OurGoalCenter).AngleInDegrees;
//                    }
//                }
//            }
//            else if (CurrentState == (int)DefenderStates.KickToGoal)
//            {
//                target = OppFreeKickDefenceUtils.Dive(engine, Model, RobotID);
//                teta = Model.OurRobots[RobotID].Angle.Value;
//            }
//            if ((CurrentState == (int)DefenderStates.Normal && inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X))
//            {
//                target = new Position2D(2.9, target.Y);
//            }
//            return target;
//        }

//        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            List<RoleBase> res = new List<RoleBase>();
//            if (FreekickDefence.WeAreInCorner && FreekickDefence.BallIsMoved)
//            {
//                res.Add(new DefenderCornerRole1());
//                res.Add(new DefenderCornerRole2());
//                res.Add(new DefenderCornerRole3());
//                res.Add(new DefenderMarkerRole2());
//                res.Add(new DefenderCornerRole4());
//                res.Add(new DefenderMarkerRole());
//                res.Add(new NewDefenderMrkerRole());
//                res.Add(new NewDefenderMarkerRole2());
//            }
//            else
//            {
//                res.Add(new DefenderCornerRole1());
//                res.Add(new DefenderCornerRole2());
//                res.Add(new DefenderCornerRole3());
//                res.Add(new RegionalDefenderRole());
//                res.Add(new DefenderMarkerRole2());
//                res.Add(new DefenderCornerRole4());
//                res.Add(new DefenderMarkerRole());
//                res.Add(new NewDefenderMrkerRole());
//                res.Add(new NewDefenderMarkerRole2());
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
