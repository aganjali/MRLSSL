//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MRL.SSL.AIConsole.Engine;
//using MRL.SSL.GameDefinitions;
//using MRL.SSL.CommonClasses.MathLibrary;
//using MRL.SSL.Planning.MotionPlanner;
//using MRL.SSL.AIConsole.Skills;

//using ActiveActionMode = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveActionMode;
//using ActiveInfo = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveInfo;
//using ActiveRoleState = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveRoleState;
//using NewActiveParameters = MRL.SSL.GameDefinitions.ActiveParameters.NewActiveParameters;
//using ActivePassKind = MRL.SSL.AIConsole.Engine.NormalSharedState.ActivePassKind;
//using ActiveDribleKind = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveDribleKind;

//using System.Drawing;
//using MRL.SSL.Planning.GamePlanner.Types;

//namespace MRL.SSL.AIConsole.Roles
//{
//    public class NewActiveRole : RoleBase
//    {
//        double lastM = double.MinValue;
//        ActiveActionMode CurrentAction = ActiveActionMode.None;
//        Position2D? lastTargetPoint = null;
//        Position2D? GoodPointInGoal = null;
//        bool reachedBehindBall = false;
//        double goodness = -1;
//        bool passSync = false, kickIsSuitable = false, Debug = true;

//        Position2D lastPassTarget = Position2D.Zero;
//        ActivePassKind lastPassKind = ActivePassKind.OneTouch; ActiveDribleKind lastdKind = ActiveDribleKind.SpaceDrible; bool lastPassIsChip = false;
//        ActiveActionMode lastPassAct = ActiveActionMode.None;
//        Position2D? lastBall = null;
//        bool firstInActive = true;

//        public override RoleCategory QueryCategory()
//        {
//            return RoleCategory.Active;
//        }

//        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, bool kickAnyway)
//        {

//            if (CurrentAction == ActiveActionMode.Conflict)
//            {
//                if (NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Rotate)
//                    Rotate(engine, Model, RobotID);
//                else
//                {
//                    GoGetBall(engine, Model, RobotID);
//                }
//            }
//            else if (CurrentAction == ActiveActionMode.None || CurrentAction == ActiveActionMode.Shoot)
//            {
//                GoGetBall(engine, Model, RobotID);
//            }
//            else if (CurrentAction == ActiveActionMode.Pass)
//            {
//                GoGetBall(engine, Model, RobotID, passSync, 0.15);
//                NormalSharedState.CommonInfo.PasserState = Model.OurRobots[RobotID];
//            }
//            else if (CurrentAction == ActiveActionMode.Drible)
//            {
//                GoGetBall(engine, Model, RobotID);
//            }

//            Planner.AddKick(RobotID, kickPowerType.Speed, ActiveInfo.kickSpeed, ActiveInfo.isChip, ActiveInfo.spin);
//        }

//        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
//        {
//            try
//            {
//                NormalSharedState.ActionInfo actInfo = new NormalSharedState.ActionInfo(false);
//                int minIndx = -1;
//                double minDist = double.MaxValue;
//                SingleObjectState ball = Model.BallState;


//                if (firstInActive && NormalSharedState.CommonInfo.Ready2Pass && NormalSharedState.ActiveInfo.CurrentAction == ActiveActionMode.Pass && NormalSharedState.CommonInfo.PassKind == ActivePassKind.OneTouch)
//                {
//                    dontResetPass = true;
//                }
//                //      var bb = NormalSharedState.ActiveInfo.ActiveSkillState;
//                if (!firstInActive && dontResetPass && (NormalSharedState.ActiveInfo.ActiveSkillState != NormalSharedState.GetBallState.Incomming || !NormalSharedState.ActiveInfo.IsNear))
//                    dontResetPass = false;
//                if (dontResetPass)
//                {
//                    GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, lastTargetPoint, Model.OurRobots[RobotID].Location, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null, new int[1] { RobotID });
//                }
//                else
//                {
//                    GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, lastTargetPoint, ball.Location, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null, new int[1] { RobotID });
//                    lastTargetPoint = GoodPointInGoal;
//                }

//                foreach (var item in Model.Opponents.Keys)
//                {
//                    Vector2D v = Model.Opponents[item].Location - ball.Location;
//                    if (v.Size < minDist && Math.Abs(v.AngleInRadians) > Math.PI / 2)
//                    {
//                        minDist = v.Size;
//                        minIndx = item;
//                    }
//                }
//                if (minIndx != -1)
//                {
//                    Obstacle obs = new Obstacle();
//                    obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
//                    obs.Type = ObstacleType.OppRobot;
//                    obs.State = Model.Opponents[minIndx];
//                    if (obs.Meet(Model.BallState, MotionPlannerParameters.BallRadi))
//                        goodness = 0;
//                }
//                Obstacles obses = new Obstacles(Model);
//                obses.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
//                if (GoodPointInGoal.HasValue && obses.Meet((dontResetPass) ? Model.OurRobots[RobotID] : Model.BallState, new SingleObjectState(GoodPointInGoal.Value, Vector2D.Zero, 0),
//                    MotionPlannerParameters.BallRadi))
//                {
//                    goodness = 0;
//                }
//                DetermineActiveState(Model, RobotID, ball, minDist);
//                ProvideStateInfo(engine, Model, RobotID, NormalSharedState.CommonInfo.AttackerID, ball, minDist, ref actInfo);

//                if (CurrentAction != ActiveActionMode.Shoot && CurrentAction != ActiveActionMode.None)
//                    dontResetPass = false;

//                NormalSharedState.CommonInfo.ActiveIsCatchingPass = dontResetPass;
//                NormalSharedState.CommonInfo.GoodPointInGoal = GoodPointInGoal;
//                NormalSharedState.CommonInfo.OppConfID = oppConfID;

//                ProvideActionInfo(engine, Model, RobotID, NormalSharedState.CommonInfo.AttackerID, actInfo);

//                if (Debug)
//                {
//                    DrawingObjects.AddObject(new StringDraw(actInfo.strState, GameParameters.OppGoalCenter + new Vector2D(-0.4, 0))); Color color = color = Color.LightPink;
//                    if (ActiveInfo.isChip && ActiveInfo.kickSpeed > 0)
//                    {
//                        color = Color.SkyBlue;
//                    }
//                    if (!ActiveInfo.isChip && ActiveInfo.kickSpeed > 0)
//                    {
//                        color = Color.YellowGreen;
//                    }
//                    DrawingObjects.AddObject(new Line(ball.Location, ActiveInfo.Target, new System.Drawing.Pen(color, 0.043f)));
//                }
//                NormalSharedState.ActiveInfo.CurrentState = (ActiveRoleState)CurrentState;
//                NormalSharedState.ActiveInfo.CurrentAction = CurrentAction;
//                NormalSharedState.CommonInfo.lastActiveID = RobotID;

//                firstInActive = false;
//            }
//            catch (Exception e)
//            {
//                ;
//            }
//        }
//        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            double failMargin = 0.1, maxFaildConst = 90, epsilon = 0.1;

//            if (engine.GameInfo.OurTeam.CatchBallLines.ContainsKey(RobotID) &&
//                engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
//            {

//                return engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head.DistanceFrom(Model.OurRobots[RobotID].Location);
//            }
//            double d = (double)MRL.SSL.Planning.GamePlanner.BallState.maxLengh / 100.0 + failMargin;

//            Vector2D ballSpeed = Model.BallState.Speed;

//            Vector2D v = GameParameters.InRefrence(Model.OurRobots[RobotID].Location - Model.BallState.Location, ballSpeed);
//            if (v.Y > epsilon)
//            {
//                d += Math.Abs(v.X) / v.Y;
//            }
//            else
//            {
//                d += maxFaildConst;
//                d += Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
//            }

//            return d;

//        }
//        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            List<RoleBase> res = new List<RoleBase>() {
//            new NewActiveRole(), new NewAttackerRole(), new NewSupporter2Role(),
//            new StaticDefender1(),new StaticDefender2()};
//            if (NormalSharedState.CommonInfo.PickIsFeasible && !NormalSharedState.CommonInfo.IsPicking)
//            {
//                if (NormalSharedState.CommonInfo.PickerID == NormalSharedState.CommonInfo.ActiveID)
//                    res.Add(new NewPickerRole());
//            }
//            else if (!NormalSharedState.CommonInfo.PickIsFeasible && NormalSharedState.CommonInfo.IsPicking)
//            {
//                res.Add(new NewPickerRole());
//            }
//            return res;
//        }
//        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            throw new NotImplementedException();
//        }

//        void GoGetBall(GameStrategyEngine engine, WorldModel Model, int RobotID, bool useDefaultBackBall = true, double backball = 0.1)
//        {
//            GetSkill<GetBallSkill>().SetAvoidDangerZone(true);
//            GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, ActiveInfo.KickTarget, NormalSharedState.CommonInfo.PasserState,
//                NormalSharedState.CommonInfo.PassIsChip, NormalSharedState.CommonInfo.PassSpeed,
//                 dontResetPass, useDefaultBackBall, backball);
//            NormalSharedState.ActiveInfo.ActiveSkillState = GetSkill<GetBallSkill>().CurrState;
//            NormalSharedState.ActiveInfo.IsNear = GetSkill<GetBallSkill>().InNear;
//            NormalSharedState.ActiveInfo.IncomingPred = GetSkill<GetBallSkill>().IncommingPred;
//        }
//        void DetermineActiveState(WorldModel Model, int RobotID, SingleObjectState ball, double minDist)
//        {


//            if (ball.Location.X > NewActiveParameters.sweepZone && minDist < 1)
//                CurrentState = (int)ActiveRoleState.Sweep;
//            else if (ball.Location.X < NewActiveParameters.kickAnyWayRegion)
//                CurrentState = (int)ActiveRoleState.KickAnyway;

//            else if (GoodPointInGoal.HasValue && goodness > NewActiveParameters.minGoodness)
//                CurrentState = (int)ActiveRoleState.Open2Kick;
//            else
//            {
//                Line l = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
//                Circle c = null;
//                if (NormalSharedState.CommonInfo.SupporterID.HasValue && Model.OurRobots.ContainsKey(NormalSharedState.CommonInfo.SupporterID.Value))
//                    c = new Circle(Model.OurRobots[NormalSharedState.CommonInfo.SupporterID.Value].Location, RobotParameters.OurRobotParams.Diameter / 2);
//                bool ins = c != null && c.Intersect(l).Count > 0;

//                if (ins && NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Rotate && IsConflictRoate(Model, RobotID, ball))
//                    CurrentState = (int)ActiveRoleState.Conflict;
//                else if (ins && NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Stop && IsConflict(Model, RobotID, ball))
//                {
//                    CurrentState = (int)ActiveRoleState.Conflict;
//                }
//                else if (minDist > NewActiveParameters.clearRobotZone)
//                    CurrentState = (int)ActiveRoleState.Clear;
//                else
//                    CurrentState = (int)ActiveRoleState.LittleSpace;
//            }
//        }

//        ActiveActionMode DeterminePassKind(GameStrategyEngine engine, WorldModel Model, int RobotID, int? AttackerID, Position2D from, Position2D shootTarget, double passSpeed, double shootSpeed, bool canDrible, out Position2D Target, out ActivePassKind pKind, out ActiveDribleKind dKind, out bool isChip)
//        {
//            ActiveActionMode res = ActiveActionMode.Shoot;
//            Obstacles obs = new Obstacles(Model);
//            if (lastBall.HasValue && Model.BallState.Location.DistanceFrom(lastBall.Value) > 0.6)
//                lastBall = null;
//            if (lastBall.HasValue)
//            {
//                if (AttackerID.HasValue && Model.OurRobots.ContainsKey(AttackerID.Value))
//                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID, AttackerID.Value }, null);
//                else
//                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
//                isChip = obs.Meet(new SingleObjectState(from, Vector2D.Zero, 0), new SingleObjectState(lastPassTarget, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi);

//                res = lastPassAct;
//                Target = lastPassTarget;
//                dKind = lastdKind;
//                pKind = lastPassKind;

//                return res;
//            }
//            var points = NormalSharedState.CommonInfo.GetPassPoints(true, true, engine, Model, from, shootTarget, new Position2D(0.75, GameParameters.OurRightCorner.Y),
//                        new Vector2D(3.5, 2 * GameParameters.OurLeftCorner.Y), 5, 10, passSpeed, shootSpeed);

//            double otScore = double.MaxValue, crScore = double.MaxValue, drScore = double.MaxValue;

//            double oc = 1, cc = 1;
//            if (AttackerID.HasValue && Model.OurRobots.ContainsKey(AttackerID.Value))
//            {
//                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID, AttackerID.Value }, null);

//                if (points.Count > 0)
//                {
//                    double d = Model.OurRobots[AttackerID.Value].Location.DistanceFrom(points[0]);
//                    oc = (obs.Meet(new SingleObjectState(from, Vector2D.Zero, 0), new SingleObjectState(points[0], Vector2D.Zero, 0), MotionPlannerParameters.BallRadi)) ? 0 : 1;
//                    double b = (obs.Meet(Model.OurRobots[AttackerID.Value], new SingleObjectState(points[0], Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi)) ? 0.1 : 1;
//                    double a = Math.Abs(Vector2D.AngleBetweenInDegrees(shootTarget - points[0], from - points[0])) > 70 ? 0 : 1;
//                    double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), points[0] - from));
//                    otScore = b * oc * d * a * t;
//                }
//                if (points.Count > 1)
//                {
//                    double d = Model.OurRobots[AttackerID.Value].Location.DistanceFrom(points[1]);
//                    cc = (obs.Meet(new SingleObjectState(from, Vector2D.Zero, 0), new SingleObjectState(points[1], Vector2D.Zero, 0), MotionPlannerParameters.BallRadi)) ? 0 : 1;
//                    double b = (obs.Meet(Model.OurRobots[AttackerID.Value], new SingleObjectState(points[1], Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi)) ? 0.1 : 1;
//                    double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), points[1] - from));
//                    crScore = b * cc * d * t;
//                }
//            }
//            Position2D dPoint;
//            bool dIsFeasible = NormalSharedState.CommonInfo.GetDribblePoint(engine, Model, RobotID, from, shootTarget, out dPoint);
//            if (canDrible && dIsFeasible)
//            {
//                points.Add(dPoint);
//                double t = Math.Abs(Vector2D.AngleBetweenInRadians(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), points.Last() - from));
//                double d = Model.OurRobots[RobotID].Location.DistanceFrom(points.Last()) * 1.5;
//                double a = (obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(points.Last(), Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi)) ? 0.1 : 1;
//                drScore = t * d * a;
//            }
//            otScore *= 1;
//            crScore *= 1.5;
//            drScore *= 6;

//            dKind = ActiveDribleKind.SpaceDrible;
//            pKind = ActivePassKind.OneTouch;
//            Target = shootTarget;
//            isChip = false;
//            bool pCalced = false;
//            foreach (var item in points)
//            {
//                if (item != Position2D.Zero)
//                    pCalced = true;
//            }
//            if ((otScore <= 1e5 || crScore <= 1e5 || drScore <= 1e5) && (pCalced))
//            {
//                if (otScore <= crScore && otScore <= drScore)
//                {
//                    isChip = (oc != 0) ? false : true;
//                    Target = points[0];
//                    res = ActiveActionMode.Pass;
//                    pKind = ActivePassKind.OneTouch;
//                }
//                else if (crScore < otScore && crScore <= drScore)
//                {
//                    isChip = (cc != 0) ? false : true;
//                    Target = points[1];
//                    res = ActiveActionMode.Pass;
//                    pKind = ActivePassKind.Catch;
//                }
//                else if (dIsFeasible && drScore < otScore && drScore < crScore)
//                {
//                    isChip = true;
//                    Target = points.Last();
//                    res = ActiveActionMode.Drible;
//                    dKind = ActiveDribleKind.SpaceDrible;
//                }
//                if (!lastBall.HasValue)
//                {
//                    lastBall = Model.BallState.Location;
//                }

//            }
//            lastPassTarget = Target;
//            lastPassIsChip = isChip;
//            lastdKind = dKind;
//            lastPassKind = pKind;
//            lastPassAct = res;
//            return res;
//        }

//        void ProvideOpen2KickInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState ball, ref NormalSharedState.ActionInfo actInfo)
//        {
//            Vector2D angleVec = Vector2D.FromAngleSize(Math.PI * Model.OurRobots[RobotID].Angle.Value / 180.0, 1);
//            Line angleLine = new Line(ball.Location, ball.Location + angleVec);
//            Line goalLine = new Line(GameParameters.OppGoalRight, GameParameters.OppGoalLeft);
//            Position2D? intersectVsGoalLine = angleLine.IntersectWithLine(goalLine);

//            if (dontResetPass)
//            {
//                actInfo.Target = GoodPointInGoal.Value;
//                actInfo.kick = 0;
//                actInfo.isChip = false;
//                actInfo.tolerance = 180;
//                actInfo.acc = actInfo.tolerance;
//                if (Model.OurRobots[RobotID].Angle.Value > 90 || Model.OurRobots[RobotID].Angle.Value < -90)
//                    actInfo.kick = Program.MaxKickSpeed;
//                actInfo.strState += " kick the pass";
//            }
//            else
//            {
//                if (NewActiveParameters.KickInRegion)
//                {
//                    List<VisibleGoalInterval> intervals = engine.GameInfo.OppGoalIntervals;
//                    foreach (var item in intervals)
//                    {
//                        Position2D ps = new Position2D(GameParameters.OppGoalCenter.X, item.interval.Start), pe = new Position2D(GameParameters.OppGoalCenter.X, item.interval.End);
//                        Vector2D v1 = ps - intersectVsGoalLine.Value;
//                        Vector2D v2 = pe - intersectVsGoalLine.Value;
//                        if (v1.InnerProduct(v2) < 0)
//                        {
//                            double tmp = Math.Abs(Vector2D.AngleBetweenInDegrees(ps - ball.Location, pe - ball.Location));
//                            if (tmp > NewActiveParameters.KickInRegionAcc)
//                            {
//                                actInfo.Target = Position2D.Interpolate(ps, pe, 0.5);
//                                actInfo.tolerance = tmp;
//                                actInfo.acc = actInfo.tolerance - NewActiveParameters.KickInRegionAcc;
//                                actInfo.strState += " KickInRegion";
//                                break;
//                            }
//                        }
//                    }
//                }
//                else
//                {
//                    actInfo.Target = GoodPointInGoal.Value;
//                    actInfo.kick = Program.MaxKickSpeed;
//                    actInfo.isChip = false;
//                    actInfo.acc = NewActiveParameters.kickAccuracy;
//                    actInfo.tolerance = 5;
//                    actInfo.strState += " AccuratedKick";
//                }
//            }
//        }
//        void ProvideSweepInfo(WorldModel Model, int RobotID, SingleObjectState ball, double minDist, ref NormalSharedState.ActionInfo actInfo)
//        {
//            double safeRadi = 0.5;
//            bool danger = false;
//            actInfo.Target = ball.Location + new Vector2D(-1, 0);
//            if (minDist < safeRadi)
//            {
//                danger = true;
//                actInfo.Target = ball.Location - (GameParameters.OurGoalCenter - ball.Location).GetNormalizeToCopy(1);
//            }
//            actInfo.kick = 4;
//            //actInfo.kick = Model.BallState.Location.DistanceFrom(;
//            actInfo.isChip = true;
//            if (NewActiveParameters.sweepMode == NewActiveParameters.SweepDefult.Direct)
//            {
//                Obstacles obs = new Obstacles(Model);
//                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
//                Vector2D ballTarget = (actInfo.Target - Model.BallState.Location).GetNormalizeToCopy(safeRadi);

//                if ((danger && !obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget, Vector2D.Zero, 0), 0.03))
//                    || (!danger && obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget.GetNormalizeToCopy(3), Vector2D.Zero, 0), 0.05)))
//                {
//                    actInfo.isChip = true;
//                }
//                else
//                {
//                    actInfo.isChip = false;
//                    actInfo.kick = Program.MaxKickSpeed;
//                }
//            }
//            actInfo.tolerance = 180;
//            actInfo.acc = actInfo.tolerance;
//            actInfo.strState += "Sweeping";
//        }
//        void ProvideConflictInfo(WorldModel Model, SingleObjectState ball, ref NormalSharedState.ActionInfo actInfo)
//        {
//            if (NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Rotate)
//            {
//                if (confWaitCounter >= confMaxWaitTresh)
//                    actInfo.kick = 5;
//                else
//                    actInfo.kick = 0;
//                if (oppConfID != -1)
//                    actInfo.Target = Model.BallState.Location + (Model.Opponents[oppConfID].Location - Model.BallState.Location).GetNormalizeToCopy(1);
//                else
//                    actInfo.Target = Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1);
//                actInfo.isChip = false;
//                actInfo.strState += "Conflicted Rotate";
//            }
//            else
//            {
//                if (ball.Location.X < -1.2)
//                {
//                    actInfo.Target = GameParameters.OppGoalCenter;
//                }
//                else if (ball.Location.X >= -1.5 && ball.Location.X < 0.5)
//                {
//                    actInfo.Target = ball.Location + new Vector2D(-1, 0);
//                }
//                else
//                {
//                    actInfo.Target = ball.Location + (ball.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1);
//                }
//                if (NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Direct)
//                {
//                    actInfo.isChip = false;
//                    actInfo.kick = 1;
//                    actInfo.strState += " Confilcted Direct";
//                }
//                else if (NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Stop)
//                {
//                    actInfo.isChip = false;
//                    actInfo.kick = 0;
//                    actInfo.strState += " Confilcted Stop";

//                }
//            }
//        }
//        ActiveActionMode ProvideClearInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, int? AttackerID, ref NormalSharedState.ActionInfo actInfo)
//        {
//            ActiveActionMode res = ActiveActionMode.Shoot;
//            if (NewActiveParameters.playMode == NewActiveParameters.PlayMode.Chip)
//            {
//                actInfo.kick = (GameParameters.OppGoalCenter - Model.BallState.Location).GetNormalizeToCopy((GameParameters.OppGoalCenter - Model.BallState.Location).Size - 0.5).Size;
//                actInfo.isChip = true;
//                actInfo.Target = GameParameters.OppGoalCenter;
//                actInfo.strState += " Clear Chip";
//                actInfo.tolerance = 20;
//                actInfo.acc = 20;
//            }
//            else if (NewActiveParameters.playMode == NewActiveParameters.PlayMode.Direct || (!AttackerID.HasValue && NewActiveParameters.playMode == NewActiveParameters.PlayMode.Pass))
//            {
//                Obstacles obs = new Obstacles(Model);
//                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, Model.Opponents.Keys.ToList());
//                Vector2D v = Vector2D.Zero;
//                if (NewActiveParameters.kickDefult == NewActiveParameters.KickDefult.Center)
//                {
//                    actInfo.Target = GameParameters.OppGoalCenter;
//                    v = (actInfo.Target - Model.BallState.Location).GetNormalizeToCopy(1.5);
//                    bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
//                    actInfo.kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(actInfo.Target) * 0.7;
//                    actInfo.isChip = b;
//                    actInfo.strState += " Clear Direct Center";
//                }
//                else
//                {
//                    actInfo.Target = (Model.BallState.Location.Y < 0 ? new Vector2D(0, -0.25) : new Vector2D(0, 0.25)) + GameParameters.OppGoalCenter;
//                    v = (actInfo.Target - Model.BallState.Location).GetNormalizeToCopy(1.5);
//                    bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
//                    actInfo.kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(actInfo.Target) * 0.7;
//                    actInfo.isChip = b;
//                    actInfo.strState += " Clear Direct Rear";
//                }
//                actInfo.tolerance = 20;
//                actInfo.acc = 20;
//            }
//            else if (NewActiveParameters.playMode == NewActiveParameters.PlayMode.Force)
//            {
//                actInfo.Target = GameParameters.OppGoalCenter;
//                actInfo.kick = 1;
//                actInfo.isChip = true;
//                actInfo.tolerance = 180;
//                actInfo.acc = 180;
//                actInfo.strState += " Clear Force";
//            }
//            else if (NewActiveParameters.playMode == NewActiveParameters.PlayMode.Pass || NewActiveParameters.playMode == NewActiveParameters.PlayMode.PassAndDribble)
//            {
//                bool b = NewActiveParameters.playMode == NewActiveParameters.PlayMode.PassAndDribble;
//                double passSpeed = 5;

//                res = DeterminePassKind(engine, Model, RobotID, AttackerID, Model.BallState.Location, GameParameters.OppGoalCenter, passSpeed, Program.MaxKickSpeed, b, out actInfo.PassTarget,
//                   out actInfo.pKind, out actInfo.dKind, out actInfo.isChip);

//                if (res == ActiveActionMode.Shoot)
//                {
//                    actInfo.kick = Program.MaxKickSpeed;
//                    actInfo.tolerance = 20;
//                    actInfo.acc = 5;
//                    actInfo.Target = GameParameters.OppGoalCenter;
//                    actInfo.strState += " Clear Shoot";
//                }
//                else if (res == ActiveActionMode.Pass)
//                {
//                    passSpeed = engine.GameInfo.CalculateKickSpeed(Model, RobotID, Model.BallState.Location, actInfo.PassTarget, actInfo.isChip, actInfo.pKind == ActivePassKind.OneTouch);

//                    actInfo.kick = passSpeed;
//                    actInfo.tolerance = 40;
//                    actInfo.acc = 20;
//                    actInfo.strState += (" Clear Pass " + actInfo.pKind);
//                    actInfo.Target = GameParameters.OppGoalCenter;


//                    if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.25
//                        && Vector2D.IsBetween(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180 + Math.PI / 6, 1),
//                        Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180 + Math.PI / 6, 1),
//                        actInfo.Target - Model.BallState.Location))
//                        reachedBehindBall = true;

//                    if (AttackerID.HasValue && Model.OurRobots.ContainsKey(AttackerID.Value) && NormalSharedState.PassSyncronizer.Sync(engine, Model, RobotID, AttackerID.Value, passSpeed, actInfo.isChip, actInfo.PassTarget, ref actInfo.kick)
//                        && reachedBehindBall)
//                        passSync = true;
//                }
//                else if (res == ActiveActionMode.Drible)
//                {
//                    actInfo.kick = Math.Max(Model.BallState.Location.DistanceFrom(actInfo.Target) * 0.4, 0.8);
//                    actInfo.tolerance = 20;
//                    actInfo.acc = 15;
//                    actInfo.Target = actInfo.PassTarget;
//                    actInfo.strState += " Clear Dribble ";
//                }

//            }
//            return res;
//        }

//        void ProvideStateInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, int? AttackerID, SingleObjectState ball, double minDist, ref NormalSharedState.ActionInfo actInfo)
//        {
//            if (CurrentState == (int)ActiveRoleState.Sweep)
//            {
//                ProvideSweepInfo(Model, RobotID, ball, minDist, ref actInfo);
//                CurrentAction = ActiveActionMode.Shoot;
//            }
//            else if (CurrentState == (int)ActiveRoleState.KickAnyway)
//            {
//                actInfo.isChip = false;
//                actInfo.kick = 0;
//                actInfo.Target = GameParameters.OppGoalCenter;

//                Vector2D v1 = GameParameters.OppGoalLeft - Model.BallState.Location;
//                Vector2D v2 = GameParameters.OppGoalRight - Model.BallState.Location;
//                Vector2D v = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1);

//                actInfo.tolerance = Math.Abs(Vector2D.AngleBetweenInDegrees(v1, v2));
//                actInfo.acc = actInfo.tolerance;

//                actInfo.kick = Program.MaxKickSpeed;
//                actInfo.strState += "kickAnyWay";
//                CurrentAction = ActiveActionMode.Shoot;
//            }
//            else if (CurrentState == (int)ActiveRoleState.Open2Kick)
//            {
//                ProvideOpen2KickInfo(engine, Model, RobotID, ball, ref actInfo);
//                CurrentAction = ActiveActionMode.Shoot;
//            }
//            else if (CurrentState == (int)ActiveRoleState.Conflict)
//            {
//                ProvideConflictInfo(Model, ball, ref actInfo);
//                CurrentAction = ActiveActionMode.Conflict;
//            }
//            else if (CurrentState == (int)ActiveRoleState.Clear)
//            {
//                CurrentAction = ProvideClearInfo(engine, Model, RobotID, AttackerID, ref actInfo);
//            }
//            else if (CurrentState == (int)ActiveRoleState.LittleSpace)
//            {
//                CurrentAction = ProvideClearInfo(engine, Model, RobotID, AttackerID, ref actInfo);
//            }

//            if (CurrentState != (int)ActiveRoleState.Conflict)
//                ResetConflictRotate();
//        }
//        void ProvideActionInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, int? AttackerID, NormalSharedState.ActionInfo actInfo)
//        {

//            if (CurrentAction == ActiveActionMode.Shoot)
//            {
//                Vector2D kickVec; Position2D KickTarget;
//                kickIsSuitable = IsSuitable4Kick(Model, RobotID, true, actInfo.tolerance, actInfo.acc, actInfo.Target, actInfo.kick, out kickVec, out KickTarget);

//                ActiveInfo.isChip = actInfo.isChip;
//                ActiveInfo.Target = actInfo.Target;
//                ActiveInfo.KickTarget = KickTarget;

//                if (kickIsSuitable)
//                    ActiveInfo.kickSpeed = kickVec.Size;
//                else
//                    ActiveInfo.kickSpeed = 0;
//            }
//            else if (CurrentAction == ActiveActionMode.None)
//            {
//                ActiveInfo.isChip = false;
//                ActiveInfo.Target = actInfo.Target;
//                ActiveInfo.KickTarget = actInfo.Target;
//                ActiveInfo.kickSpeed = 0;
//            }
//            else if (CurrentAction == ActiveActionMode.Conflict)
//            {
//                dontResetPass = false;
//                ActiveInfo.spin = false;
//                ActiveInfo.isChip = actInfo.isChip;
//                ActiveInfo.kickSpeed = actInfo.kick;
//                ActiveInfo.KickTarget = ActiveInfo.Target = actInfo.Target;
//            }
//            else if (CurrentAction == ActiveActionMode.Pass)
//            {
//                dontResetPass = false;
//                NormalSharedState.CommonInfo.Ready2Pass = true;
//                NormalSharedState.CommonInfo.PassSpeed = actInfo.kick;
//                NormalSharedState.CommonInfo.ShootSpeed = Program.MaxKickSpeed;
//                NormalSharedState.CommonInfo.PassTarget = actInfo.PassTarget;
//                NormalSharedState.CommonInfo.ShootTarget = actInfo.Target;
//                NormalSharedState.CommonInfo.PassKind = actInfo.pKind;
//                NormalSharedState.CommonInfo.PassIsChip = actInfo.isChip;

//                Vector2D kickVec; Position2D KickTarget;


//                if (!passSync)
//                {
//                    kickIsSuitable = IsSuitable4Kick(Model, RobotID, !actInfo.isChip, actInfo.tolerance, actInfo.acc, actInfo.Target, actInfo.kick, out kickVec, out KickTarget);
//                    ActiveInfo.isChip = actInfo.isChip;
//                    ActiveInfo.Target = actInfo.Target;
//                    ActiveInfo.KickTarget = KickTarget;
//                }
//                else
//                {
//                    kickIsSuitable = IsSuitable4Kick(Model, RobotID, !actInfo.isChip, actInfo.tolerance, actInfo.acc, actInfo.PassTarget, actInfo.kick, out kickVec, out KickTarget);
//                    ActiveInfo.isChip = actInfo.isChip;
//                    ActiveInfo.Target = actInfo.PassTarget;
//                    ActiveInfo.KickTarget = KickTarget;
//                }

//                if (kickIsSuitable && AttackerID.HasValue && passSync)
//                {
//                    ActiveInfo.kickSpeed = kickVec.Size;
//                    NormalSharedState.CommonInfo.Passed = true;
//                }
//                else
//                {
//                    ActiveInfo.kickSpeed = 0;
//                    NormalSharedState.CommonInfo.Passed = false;
//                }
//            }
//            else if (CurrentAction == ActiveActionMode.Drible)
//            {
//                Vector2D kickVec; Position2D KickTarget;
//                kickIsSuitable = IsSuitable4Kick(Model, RobotID, !actInfo.isChip, actInfo.tolerance, actInfo.acc, actInfo.Target, actInfo.kick, out kickVec, out KickTarget);

//                ActiveInfo.isChip = actInfo.isChip;
//                ActiveInfo.Target = actInfo.Target;
//                ActiveInfo.KickTarget = KickTarget;

//                if (kickIsSuitable)
//                {
//                    ActiveInfo.kickSpeed = kickVec.Size;
//                }
//                else
//                {
//                    ActiveInfo.kickSpeed = 0;
//                }
//            }


//            if (CurrentAction != ActiveActionMode.Pass && !dontResetPass)
//            {
//                NormalSharedState.CommonInfo.ResetPass();
//                dontResetPass = false;
//                lastBall = null;
//                passSync = false;
//                reachedBehindBall = false;
//            }
//        }
//        bool dontResetPass = false;
//        Vector2D CalculateKickAngle(WorldModel Model, int RobotID, Position2D Target, double finalKickSpeed, double maxKickSpeed)
//        {
//            double minBallSpeedTresh = double.MaxValue;//0.3;
//            Vector2D b = Model.BallState.Speed;
//            if (b.Size < minBallSpeedTresh)
//                b = Vector2D.Zero;
//            Vector2D c = Vector2D.FromAngleSize((Target - Model.BallState.Location).AngleInRadians,
//                finalKickSpeed - Math.Max(0, Math.Min(finalKickSpeed / 2, GameParameters.InRefrence(Model.OurRobots[RobotID].Speed, Target - Model.BallState.Location).Y)));
//            return c;
//            Vector2D a = c - b;
//            if (a.Size > maxKickSpeed)
//            {
//                double cs = finalKickSpeed;
//                double dc = -finalKickSpeed;
//                int counter = 0;
//                while (counter < 10)
//                {
//                    dc *= 0.5;
//                    double alfa = cs + dc;
//                    c.NormalizeTo(alfa);
//                    a = c - b;
//                    if (maxKickSpeed - a.Size < 0)
//                        cs = alfa;
//                    counter++;
//                }
//            }
//            return a;
//        }
//        bool IsSuitable4Kick(WorldModel Model, int RobotID, bool angleCorrection, double Tolerance, double acc, Position2D Target, double kickSpeed, out Vector2D kickVec, out Position2D KickTarget)
//        {
//            double beta = 1 - Math.Min(acc / Tolerance, 1);
//            if (angleCorrection)
//            {
//                kickVec = CalculateKickAngle(Model, RobotID, Target, kickSpeed, Program.MaxKickSpeed);
//                KickTarget = Model.BallState.Location + kickVec.GetNormalizeToCopy((Target - Model.BallState.Location).Size);
//            }
//            else
//            {
//                kickVec = (Target - Model.BallState.Location).GetNormalizeToCopy(kickSpeed);
//                KickTarget = Target;
//            }
//            double g0 = ((KickTarget - Model.BallState.Location).AngleInDegrees - Tolerance / 2.0);
//            double g1 = ((KickTarget - Model.BallState.Location).AngleInDegrees + Tolerance / 2.0);

//            double m = Math.Min(GameParameters.AngleModeD(Model.OurRobots[RobotID].Angle.Value - g0), GameParameters.AngleModeD(g1 - Model.OurRobots[RobotID].Angle.Value));
//            bool res = true;
//            if (m < 0)
//                res = false;
//            else if (m > beta * GameParameters.AngleModeD(g1 - g0) / 2)
//                res = true;
//            else if (m > lastM)
//                res = false;
//            lastM = m;
//            return res;
//        }

//        #region Conflict

//        double rotateSpeed = 0, maxRotateSpeed = 30, rotateStartTresh = 30, rotateStopTresh = 10;
//        int oppConfID = -1;
//        double? lastRobotAngle = null;
//        const int confMaxWaitTresh = 30;
//        int confWaitCounter = 0;
//        bool rotateStarted = false;
//        int counter = 0;

//        public void Rotate(GameStrategyEngine engine, WorldModel Model, int RobotID)
//        {
//            SingleWirelessCommand swc = new SingleWirelessCommand();

//            if (confWaitCounter < confMaxWaitTresh)
//            {
//                confWaitCounter++;

//                if (Debug)
//                {
//                    DrawingObjects.AddObject(new Circle(Position2D.Zero, 0.3, new Pen(Color.Purple, 0.01f)), "dayerevasatConflict");
//                    DrawingObjects.AddObject(new StringDraw(confWaitCounter.ToString(), new Position2D(-0.5, -0.5)), "dayerevasatConflictStr");
//                }
//                GetSkill<GetBallSkill>().SetAvoidDangerZone(true);
//                GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, ActiveInfo.KickTarget);
//            }
//            else
//            {
//                double rotateSgn = 1;
//                if (counter < 10)
//                    rotateSgn = (oppConfID != -1 && (Model.Opponents[oppConfID].Location - Model.BallState.Location).AngleInDegrees > 0) ? -1 : 1;//(Model.BallState.Location.Y > 0) ? -1 : 1;
//                else if (counter < 30)
//                    rotateSgn = (oppConfID != -1 && (Model.Opponents[oppConfID].Location - Model.BallState.Location).AngleInDegrees > 0) ? 1 : -1;
//                else if (counter < 50)
//                    rotateSgn = (oppConfID != -1 && (Model.Opponents[oppConfID].Location - Model.BallState.Location).AngleInDegrees > 0) ? -1 : 1;

//                double alfa = 40;
//                rotateSpeed += rotateSgn * alfa * StaticVariables.FRAME_PERIOD;
//                if (Math.Abs(rotateSpeed) > maxRotateSpeed)
//                    rotateSpeed = rotateSgn * maxRotateSpeed;

//                swc.W = rotateSpeed;
//                swc.Vy = 1.5;

//                if (Debug)
//                    DrawingObjects.AddObject(new Circle(Position2D.Zero, 0.3, new Pen(Color.Purple, 0.01f)), "dayerevasatConflict");


//                Vector2D vec = Vector2D.Zero;
//                if (oppConfID != -1)
//                {
//                    vec = (Model.Opponents[oppConfID].Location - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(1);
//                    vec = GameParameters.InRefrence(vec, Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1));
//                    swc.Vx = vec.X;
//                    swc.Vy = vec.Y;
//                }

//                if (!rotateStarted && lastRobotAngle.HasValue && Math.Abs(GameParameters.AngleModeD(Model.OurRobots[RobotID].Angle.Value - lastRobotAngle.Value)) >= rotateStopTresh)
//                {
//                    rotateStarted = true;
//                }
//                else if (rotateStarted && lastRobotAngle.HasValue && Math.Abs(GameParameters.AngleModeD(Model.OurRobots[RobotID].Angle.Value - lastRobotAngle.Value)) <= rotateStopTresh)
//                {
//                    ResetConflictRotate();
//                }
//                if (lastRobotAngle.HasValue && Debug)
//                    DrawingObjects.AddObject(new StringDraw(GameParameters.AngleModeD(Model.OurRobots[RobotID].Angle.Value - lastRobotAngle.Value).ToString(), new Position2D(-.7, -.7)), "danglemodeStr");
//                Planner.Add(RobotID, swc, true);
//            }
//        }
//        public bool IsConflictRoate(WorldModel Model, int RobotID, SingleObjectState ball)
//        {
//            bool behindBallOur = false;
//            Obstacle obs = new Obstacle(), obs2 = new Obstacle();

//            if ((CurrentState != (int)ActiveRoleState.Conflict && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < NewActiveParameters.ConfilictZone)
//            || (CurrentState == (int)ActiveRoleState.Conflict && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.3))
//            {
//                Vector2D vl = Vector2D.FromAngleSize((30.0).ToRadian(), 0.3);//new Vector2D(0.3, -0.3);
//                Vector2D vr = Vector2D.FromAngleSize((-30.0).ToRadian(), 0.3);//new Vector2D(0.3, 0.3);
//                Vector2D vm = Vector2D.FromAngleSize((0.0).ToRadian(), 0.3);//new Vector2D(0.3, 0);
//                obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
//                obs.State = Model.OurRobots[RobotID];
//                obs.Type = ObstacleType.OurRobot;
//                if (!rotateStarted)
//                {
//                    int count = 0;
//                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                        count++;
//                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                        count++;
//                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                        count++;
//                    if (CurrentState != (int)ActiveRoleState.Conflict && count > 2)
//                        behindBallOur = true;
//                    else if (CurrentState == (int)ActiveRoleState.Conflict && count > 1)
//                        behindBallOur = true;
//                }
//                else
//                {
//                    behindBallOur = true;
//                }
//            }
//            if (behindBallOur)
//            {
//                foreach (var item in Model.Opponents.Keys)
//                {
//                    SingleObjectState oppRobot = Model.Opponents[item];
//                    Vector2D v = oppRobot.Location - ball.Location;

//                    if (v.Size < NewActiveParameters.ConfilictZone)
//                    {
//                        Vector2D vl = Vector2D.FromAngleSize((150.0).ToRadian(), 0.3);//new Vector2D(-0.3, -0.3);
//                        Vector2D vr = Vector2D.FromAngleSize((-150.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0.3);
//                        Vector2D vm = Vector2D.FromAngleSize((180.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0);
//                        obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
//                        obs.State = Model.Opponents[item];
//                        obs.Type = ObstacleType.OppRobot;
//                        if (!rotateStarted)
//                        {
//                            int count = 0;
//                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                                count++;
//                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                                count++;
//                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                                count++;

//                            if (CurrentState != (int)ActiveRoleState.Conflict && count > 2)
//                            {
//                                if (!lastRobotAngle.HasValue)
//                                    lastRobotAngle = Model.OurRobots[RobotID].Angle.Value;
//                                oppConfID = item;
//                                return true;
//                            }
//                            else if (CurrentState == (int)ActiveRoleState.Conflict && count > 1)
//                            {
//                                oppConfID = item;
//                                return true;
//                            }
//                        }
//                        else
//                            return true;
//                    }

//                }
//            }
//            return false;
//        }
//        public void ResetConflictRotate()
//        {
//            confWaitCounter = 0;
//            rotateSpeed = 0;
//            lastRobotAngle = null;
//            rotateStarted = false;
//        }
//        bool IsConflict(WorldModel Model, int RobotID, SingleObjectState ball)
//        {
//            bool behindBallOur = false;
//            Obstacle obs = new Obstacle(), obs2 = new Obstacle();

//            if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < NewActiveParameters.ConfilictZone)
//            {
//                Vector2D vl = Vector2D.FromAngleSize((30.0).ToRadian(), 0.3);//new Vector2D(0.3, -0.3);
//                Vector2D vr = Vector2D.FromAngleSize((-30.0).ToRadian(), 0.3);//new Vector2D(0.3, 0.3);
//                Vector2D vm = Vector2D.FromAngleSize((0.0).ToRadian(), 0.3);//new Vector2D(0.3, 0);
//                obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
//                obs.State = Model.OurRobots[RobotID];
//                obs.Type = ObstacleType.OurRobot;

//                int count = 0;
//                if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                    count++;
//                if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                    count++;
//                if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                    count++;
//                if (CurrentState != (int)ActiveRoleState.Conflict && count > 2)
//                    behindBallOur = true;
//                else if (CurrentState == (int)ActiveRoleState.Conflict && count > 1)
//                    behindBallOur = true;

//            }
//            if (behindBallOur)
//            {
//                foreach (var item in Model.Opponents.Keys)
//                {
//                    SingleObjectState oppRobot = Model.Opponents[item];
//                    Vector2D v = oppRobot.Location - ball.Location;

//                    if (v.Size < NewActiveParameters.ConfilictZone)
//                    {
//                        Vector2D vl = Vector2D.FromAngleSize((150.0).ToRadian(), 0.3);//new Vector2D(-0.3, -0.3);
//                        Vector2D vr = Vector2D.FromAngleSize((-150.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0.3);
//                        Vector2D vm = Vector2D.FromAngleSize((180.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0);
//                        obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
//                        obs.State = Model.Opponents[item];
//                        obs.Type = ObstacleType.OppRobot;
//                        int count = 0;

//                        if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                            count++;
//                        if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                            count++;
//                        if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
//                            count++;

//                        if (count > 2)
//                            return true;

//                    }

//                }
//            }
//            return false;
//        }

//        #endregion

//    }

//}

