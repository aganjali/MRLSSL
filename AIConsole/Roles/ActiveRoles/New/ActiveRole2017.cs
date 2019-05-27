using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Skills;

using ActiveActionMode = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveActionMode;
using ActiveInfo = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveInfo;
using ActiveRoleState = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveRoleState;
using NewActiveParameters = MRL.SSL.GameDefinitions.ActiveParameters.NewActiveParameters;
using ActivePassKind = MRL.SSL.AIConsole.Engine.NormalSharedState.ActivePassKind;
using ActiveDribleKind = MRL.SSL.AIConsole.Engine.NormalSharedState.ActiveDribleKind;

using System.Drawing;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.AIConsole.Roles.Defending.Normal;

namespace MRL.SSL.AIConsole.Roles
{
    public class ActiveRole2017 : RoleBase
    {
        #region Variables
        bool firstInPass = true;
        bool passed = false;
        Position2D firstBallPos = new Position2D();
        bool firstNearBallFlag = true;
        bool ballIsCatched = false;
        Position2D ballFirstPos;
        int stateCounter = 0;
        double lastM = double.MinValue;
        ActiveActionMode CurrentAction = ActiveActionMode.None;
        Position2D? lastTargetPoint = null;
        Position2D? GoodPointInGoal = null;
        bool reachedBehindBall = false;
        double goodness = -1;
        bool passSync = false, kickIsSuitable = false, Debug = true;

        Position2D lastPassTarget = Position2D.Zero;
        ActivePassKind lastPassKind = ActivePassKind.OneTouch; ActiveDribleKind lastdKind = ActiveDribleKind.SpaceDrible; bool lastPassIsChip = false;
        ActiveActionMode lastPassAct = ActiveActionMode.None;
        Position2D? lastBall = null;
        bool firstInActive = true;
        bool dontResetPass = false;

        ActionDescriptionBase currentActionDesc;
        #endregion

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, bool kickAnyway)
        {
            DrawingObjects.AddObject(new StringDraw("ballRobot Dist " + Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location).ToString(), new Position2D(3, 3)), "dasda");
            DrawingObjects.AddObject(new StringDraw("ballIsCatched " + ballIsCatched.ToString(), new Position2D(3.2, 3)), "dfdsasda");
            if (currentActionDesc.ActionCategory() == ActiveActionMode.Conflict)
            {
                if (NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.SpinRotate)
                    SpinRotate(engine, Model, RobotID);
                else if (NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Rotate)
                    Rotate(engine, Model, RobotID);
                else
                {
                    GoGetBall(engine, Model, RobotID);
                }
            }
            else if (currentActionDesc.ActionCategory() == ActiveActionMode.None || currentActionDesc.ActionCategory() == ActiveActionMode.Shoot)
            {
                GoGetBall(engine, Model, RobotID);
            }
            else if (currentActionDesc.ActionCategory() == ActiveActionMode.Pass)
            {
                GoGetBall(engine, Model, RobotID, passSync, 0.11);
                NormalSharedState.CommonInfo.PasserState = Model.OurRobots[RobotID];
            }
            else if (currentActionDesc.ActionCategory() == ActiveActionMode.Drible)
            {
                GoGetBall(engine, Model, RobotID);

            }
            if (GetSkill<GetBallSkill>().CurrState == NormalSharedState.GetBallState.Incomming)
            {
                if (!ballIsCatched)
                    ballIsCatched = Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.06;
            }
            else
                ballIsCatched = false;
            //if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.5 || GetSkill<GetBallSkill>().CurrState == GetBallState
            if ((Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.5 || (GetSkill<GetBallSkill>().CurrState == NormalSharedState.GetBallState.Incomming && !ballIsCatched)) && currentActionDesc.ActionCategory() != ActiveActionMode.Pass && ActiveInfo.kickSpeed == 0)
            {
                ActiveInfo.spin = false;
            }
            else
                ActiveInfo.spin = false;
            if (!(currentActionDesc.ActionCategory() == ActiveActionMode.Conflict && NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.SpinRotate))
            {
                if (currentActionDesc.ActionCategory() == ActiveActionMode.Drible)
                    Planner.AddKick(RobotID, kickPowerType.Power, ActiveInfo.kickSpeed, ActiveInfo.isChip, false);
                else
                    Planner.AddKick(RobotID, kickPowerType.Speed, ActiveInfo.kickSpeed, ActiveInfo.isChip, ActiveInfo.spin/* ActiveInfo.spin*/);
            }
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            SingleObjectState ball = Model.BallState;
            int minIndx; double minDist;

            FindNearestOpp(Model, ball, out minIndx, out minDist);
            FindGoalTarget(engine, Model, RobotID, ball, minIndx);

            DetermineActiveState(Model, RobotID, ball, minDist);


            NormalSharedState.CommonInfo.GoodPointInGoal = GoodPointInGoal;
            NormalSharedState.CommonInfo.OppConfID = oppConfID;

            NormalSharedState.ActionInfo actInfo = new NormalSharedState.ActionInfo(false);
            actInfo.minDist = minDist;
            actInfo.minIdx = minIndx;
            Type type;
            ActionDescriptionBase selectedAction = currentActionDesc;
            if (!(currentActionDesc != null && currentActionDesc.ActionCategory() == ActiveActionMode.Pass && !passed))
                selectedAction = FindBestAction(engine, Model, RobotID, out type, ref actInfo);
            else
                currentActionDesc.Cost(engine, Model, RobotID, (ActiveRoleState)CurrentState, ref actInfo);

            if (currentActionDesc == null || currentActionDesc.Name != selectedAction.Name)
            {
                stateCounter++;
                if (currentActionDesc == null || stateCounter >= 2)
                {
                    stateCounter = 0;
                    if (currentActionDesc != null)
                        currentActionDesc.Reset();
                    currentActionDesc = selectedAction;//(ActionDescriptionBase)type.GetConstructor(new Type[] { }).Invoke(new object[] { });
                }
            }
            else
                stateCounter = 0;
            foreach (var item in engine.ImplementedActions)
            {
                if (item.Value.Name != currentActionDesc.Name)
                {
                    item.Value.Reset();
                }
            }

            if (currentActionDesc.ActionCategory() == ActiveActionMode.Pass)
            {
                if (firstInPass)
                {
                    firstInPass = false;
                    firstBallPos = Model.BallState.Location;
                }
                passed = Model.BallState.Speed.Size > StaticVariables.PassSpeedTresh && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.05;
            }

            currentActionDesc.DetermineActionState(engine, Model, RobotID, (ActiveRoleState)CurrentState, ref actInfo);

            if (currentActionDesc.ActionCategory() != ActiveActionMode.Shoot && currentActionDesc.ActionCategory() != ActiveActionMode.None)
                dontResetPass = false;

            NormalSharedState.CommonInfo.ActiveIsCatchingPass = dontResetPass;

            currentActionDesc.ProvideActionInfo(engine, Model, RobotID, (ActiveRoleState)CurrentState, ref actInfo);

            if (Debug)
                currentActionDesc.Print(engine, Model, RobotID, actInfo);

            if (currentActionDesc.ActionCategory() != ActiveActionMode.Pass && !dontResetPass)
            {
                NormalSharedState.CommonInfo.ResetPass();
                lastBall = null;
            }

            if (CurrentState != (int)ActiveRoleState.Conflict)
                ResetConflictRotate();

            passSync = actInfo.passSync;
            NormalSharedState.CommonInfo.ActiveIsCatchingPass = dontResetPass;
            NormalSharedState.ActiveInfo.confWaitCounter = confWaitCounter;

            NormalSharedState.ActiveInfo.CurrentState = (ActiveRoleState)CurrentState;
            NormalSharedState.ActiveInfo.CurrentAction = currentActionDesc.ActionCategory();
            NormalSharedState.CommonInfo.lastActiveID = RobotID;

            firstInActive = false;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            double failMargin = 0.1, maxFaildConst = 90, epsilon = 0.1;

            if (engine.GameInfo.OurTeam.CatchBallLines.ContainsKey(RobotID) &&
                engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
            {

                return engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head.DistanceFrom(Model.OurRobots[RobotID].Location);
            }
            double d = (double)MRL.SSL.Planning.GamePlanner.BallState.maxLengh / 100.0 + failMargin;

            Vector2D ballSpeed = Model.BallState.Speed;

            Vector2D v = GameParameters.InRefrence(Model.OurRobots[RobotID].Location - Model.BallState.Location, ballSpeed);
            if (v.Y > epsilon)
            {
                d += Math.Abs(v.X) / v.Y;
            }
            else
            {
                d += maxFaildConst;
                d += Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
            }

            return d;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() {
                
            new NewActiveRole(),
            new ActiveRole2017(),
            new NewSupporter2Role(),
            new NewRegionalRole(),
            new StaticDefender1(),new StaticDefender2(),new staticDefender3(),
            new Marker1Normal8Robot(),new Marker2Normal8Robot() };
            if (NormalSharedState.CommonInfo.PickIsFeasible && !NormalSharedState.CommonInfo.IsPicking)
            {
                if (NormalSharedState.CommonInfo.PickerID == NormalSharedState.CommonInfo.ActiveID)
                    res.Add(new NewPickerRole());
            }
            else if (!NormalSharedState.CommonInfo.PickIsFeasible && NormalSharedState.CommonInfo.IsPicking)
            {
                res.Add(new NewPickerRole());
            }
            if (!NormalSharedState.CommonInfo.AttackerMode)
            {
                //res.Add(new AttackerRole2017());
                res.Add(new NewAttackerRole());
            }
            else
            {
                res.Add(new NewAttacker3Role());
                res.Add(new NewAttacker2Role());
            }
            return res;
        }
        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        private static void FindNearestOpp(WorldModel Model, SingleObjectState ball, out int minIndx, out double minDist)
        {
            minIndx = -1;
            minDist = double.MaxValue;

            foreach (var item in Model.Opponents.Keys)
            {
                Vector2D v = Model.Opponents[item].Location - ball.Location;
                if (v.Size < minDist && Math.Abs(v.AngleInRadians) > Math.PI / 2)
                {
                    minDist = v.Size;
                    minIndx = item;
                }
            }
        }
        private ActionDescriptionBase FindBestAction(GameStrategyEngine engine, WorldModel Model, int RobotID, out Type type, ref NormalSharedState.ActionInfo actInfo)
        {

            Dictionary<int, double> costs = new Dictionary<int, double>();
            foreach (var item in engine.ImplementedActions)
            {
                costs.Add(costs.Count, item.Value.Cost(engine, Model, RobotID, (ActiveRoleState)CurrentState, ref actInfo) * item.Value.Lambda);
            }
            if (costs.Count > 0)
            {
                costs = costs.OrderBy(o => o.Value).ToDictionary(k => k.Key, v => v.Value);
                type = engine.ImplementedActions.ElementAt(costs.First().Key).Key;
                return engine.ImplementedActions.ElementAt(costs.First().Key).Value;//1).Value;
            }
            else
                throw new Exception("No action feasible!");

        }
        private void FindGoalTarget(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState ball, int minIndx)
        {

            if (firstInActive && NormalSharedState.CommonInfo.Ready2Pass && NormalSharedState.ActiveInfo.CurrentAction == ActiveActionMode.Pass && NormalSharedState.CommonInfo.PassKind == ActivePassKind.OneTouch)
            {
                dontResetPass = true;
            }
            //      var bb = NormalSharedState.ActiveInfo.ActiveSkillState;
            if (!firstInActive && dontResetPass && (NormalSharedState.ActiveInfo.ActiveSkillState != NormalSharedState.GetBallState.Incomming || !NormalSharedState.ActiveInfo.IsNear))
                dontResetPass = false;
            if (dontResetPass)
            {
                GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, lastTargetPoint, Model.OurRobots[RobotID].Location, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null, new int[1] { RobotID });
            }
            else
            {
                GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, lastTargetPoint, ball.Location, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null, new int[1] { RobotID });
                lastTargetPoint = GoodPointInGoal;
            }


            if (minIndx != -1)
            {
                Obstacle obs = new Obstacle();
                obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
                obs.Type = ObstacleType.OppRobot;
                obs.State = Model.Opponents[minIndx];
                if (obs.Meet(Model.BallState, MotionPlannerParameters.BallRadi))
                    goodness = 0;
            }
            Obstacles obses = new Obstacles(Model);
            obses.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
            if (GoodPointInGoal.HasValue && obses.Meet((dontResetPass) ? Model.OurRobots[RobotID] : Model.BallState, new SingleObjectState(GoodPointInGoal.Value, Vector2D.Zero, 0),
                MotionPlannerParameters.BallRadi))
            {
                goodness = 0;
            }
        }

        void DetermineActiveState(WorldModel Model, int RobotID, SingleObjectState ball, double minDist)
        {

            if (firstNearBallFlag)
                ballFirstPos = ball.Location;
            double ballRobotDist = ball.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
            double angleBallRobot = (ball.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            if (angleBallRobot < 0)
                angleBallRobot += 360;
            angleBallRobot -= (double)Model.OurRobots[RobotID].Angle;
            if (ballRobotDist < .15 && Math.Abs(angleBallRobot) < 15 && firstNearBallFlag)
                firstNearBallFlag = false;
            if (ballRobotDist >= .15)
                firstNearBallFlag = true;

            if (ball.Location.X > NewActiveParameters.sweepZone && minDist < 1)
                CurrentState = (int)ActiveRoleState.Sweep;
            else if (ball.Location.X < NewActiveParameters.kickAnyWayRegion && Math.Abs(ball.Location.Y) < Math.Abs(GameParameters.OppLeftCorner.Y) / 2 - 0.2/* || ballFirstPos.DistanceFrom(ball.Location) > 0.4*/)
                CurrentState = (int)ActiveRoleState.KickAnyway;
            else if (GoodPointInGoal.HasValue && goodness >= NewActiveParameters.minGoodness)
                CurrentState = (int)ActiveRoleState.Open2Kick;
            else
            {
                Line l = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                Circle c = null;
                if (NormalSharedState.CommonInfo.SupporterID.HasValue && Model.OurRobots.ContainsKey(NormalSharedState.CommonInfo.SupporterID.Value))
                    c = new Circle(Model.OurRobots[NormalSharedState.CommonInfo.SupporterID.Value].Location, RobotParameters.OurRobotParams.Diameter / 2);
                bool ins = c != null && c.Intersect(l).Count > 0;

                if (ins && NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Rotate && IsConflictRoate(Model, RobotID, ball))
                    CurrentState = (int)ActiveRoleState.Conflict;
                else if (NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.Stop && IsConflict(Model, RobotID, ball))
                    CurrentState = (int)ActiveRoleState.Conflict;
                else if (ins && NewActiveParameters.conflictMode == NewActiveParameters.ConflictMode.SpinRotate && IsConflictRoateSpin(Model, RobotID, ball))
                    CurrentState = (int)ActiveRoleState.Conflict;
                else if (minDist > NewActiveParameters.clearRobotZone)
                    CurrentState = (int)ActiveRoleState.Clear;
                else
                    CurrentState = (int)ActiveRoleState.LittleSpace;
            }

            if (NormalSharedState.ActiveInfo.isRotateStarted)
                CurrentState = (int)ActiveRoleState.Conflict;
            DrawingObjects.AddObject(new StringDraw(((ActiveRoleState)CurrentState).ToString(), GameParameters.OppGoalCenter + new Vector2D(-0.6, 0)));
        }
        void GoGetBall(GameStrategyEngine engine, WorldModel Model, int RobotID, bool useDefaultBackBall = true, double backball = 0.1)
        {
            GetSkill<GetBallSkill>().SetAvoidDangerZone(true, true);
            GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, ref ActiveInfo.KickTarget, NormalSharedState.CommonInfo.PasserState,
                NormalSharedState.CommonInfo.PassIsChip, NormalSharedState.CommonInfo.PassSpeed,
                 dontResetPass, NormalSharedState.CommonInfo.PassKind, useDefaultBackBall, backball);
            NormalSharedState.ActiveInfo.ActiveSkillState = GetSkill<GetBallSkill>().CurrState;
            NormalSharedState.ActiveInfo.IsNear = GetSkill<GetBallSkill>().InNear;
            NormalSharedState.ActiveInfo.IncomingPred = GetSkill<GetBallSkill>().IncommingPred;
        }

        #region Conflict
        double rotateSpeed = 0, maxRotateSpeed = 30, rotateStartTresh = 30, rotateStopTresh = 10;
        int oppConfID = -1;
        double? lastRobotAngle = null;
        bool rotateStarted = false;
        int counter = 0;
        int confWaitCounter = 0;

        public void SpinRotate(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            SingleWirelessCommand swc = new SingleWirelessCommand();

            if (confWaitCounter < ActiveInfo.confRotateMaxWaitTresh)
            {
                confWaitCounter++;

                if (Debug)
                {
                    DrawingObjects.AddObject(new Circle(Position2D.Zero, 0.3, new Pen(Color.Purple, 0.01f)), "dayerevasatConflict");
                    DrawingObjects.AddObject(new StringDraw(confWaitCounter.ToString(), new Position2D(-0.5, -0.5)), "dayerevasatConflictStr");
                }
                GetSkill<GetBallSkill>().SetAvoidDangerZone(true, true);
                GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, GameParameters.OppGoalCenter);
                NormalSharedState.ActiveInfo.isRotateStarted = false;
            }
            else
            {
                GetSkill<ActiveRotateSkill>().Rotate(Model, RobotID, ActiveInfo.KickTarget, 3, Model.BallState.Location.Y > 0);
            }
            if (NormalSharedState.ActiveInfo.isRotateStarted && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) > 0.4)
                NormalSharedState.ActiveInfo.isRotateStarted = false;
        }
        public void Rotate(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            SingleWirelessCommand swc = new SingleWirelessCommand();

            if (confWaitCounter < ActiveInfo.confMaxWaitTresh)
            {
                confWaitCounter++;

                if (Debug)
                {
                    DrawingObjects.AddObject(new Circle(Position2D.Zero, 0.3, new Pen(Color.Purple, 0.01f)), "dayerevasatConflict");
                    DrawingObjects.AddObject(new StringDraw(confWaitCounter.ToString(), new Position2D(-0.5, -0.5)), "dayerevasatConflictStr");
                }
                GetSkill<GetBallSkill>().SetAvoidDangerZone(true, true);
                GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, ActiveInfo.KickTarget);
            }
            else
            {
                double rotateSgn = 1;
                if (counter < 10)
                    rotateSgn = (oppConfID != -1 && (Model.Opponents[oppConfID].Location - Model.BallState.Location).AngleInDegrees > 0) ? -1 : 1;//(Model.BallState.Location.Y > 0) ? -1 : 1;
                else if (counter < 30)
                    rotateSgn = (oppConfID != -1 && (Model.Opponents[oppConfID].Location - Model.BallState.Location).AngleInDegrees > 0) ? 1 : -1;
                else if (counter < 50)
                    rotateSgn = (oppConfID != -1 && (Model.Opponents[oppConfID].Location - Model.BallState.Location).AngleInDegrees > 0) ? -1 : 1;

                double alfa = 40;
                rotateSpeed += rotateSgn * alfa * StaticVariables.FRAME_PERIOD;
                if (Math.Abs(rotateSpeed) > maxRotateSpeed)
                    rotateSpeed = rotateSgn * maxRotateSpeed;

                swc.W = rotateSpeed;
                swc.Vy = 1.5;

                if (Debug)
                    DrawingObjects.AddObject(new Circle(Position2D.Zero, 0.3, new Pen(Color.Purple, 0.01f)), "dayerevasatConflict");


                Vector2D vec = Vector2D.Zero;
                if (oppConfID != -1)
                {
                    vec = (Model.Opponents[oppConfID].Location - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(1);
                    vec = GameParameters.InRefrence(vec, Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1));
                    swc.Vx = vec.X;
                    swc.Vy = vec.Y;
                }

                if (!rotateStarted && lastRobotAngle.HasValue && Math.Abs(GameParameters.AngleModeD(Model.OurRobots[RobotID].Angle.Value - lastRobotAngle.Value)) >= rotateStopTresh)
                {
                    rotateStarted = true;
                }
                else if (rotateStarted && lastRobotAngle.HasValue && Math.Abs(GameParameters.AngleModeD(Model.OurRobots[RobotID].Angle.Value - lastRobotAngle.Value)) <= rotateStopTresh)
                {
                    ResetConflictRotate();
                }
                if (lastRobotAngle.HasValue && Debug)
                    DrawingObjects.AddObject(new StringDraw(GameParameters.AngleModeD(Model.OurRobots[RobotID].Angle.Value - lastRobotAngle.Value).ToString(), new Position2D(-.7, -.7)), "danglemodeStr");
                Planner.Add(RobotID, swc, true);
            }
        }
        public bool IsConflictRoate(WorldModel Model, int RobotID, SingleObjectState ball)
        {

            bool behindBallOur = false;
            Obstacle obs = new Obstacle(), obs2 = new Obstacle();

            if ((CurrentState != (int)ActiveRoleState.Conflict && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < NewActiveParameters.ConfilictZone)
            || (CurrentState == (int)ActiveRoleState.Conflict && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.3))
            {
                Vector2D vl = Vector2D.FromAngleSize((30.0).ToRadian(), 0.3);//new Vector2D(0.3, -0.3);
                Vector2D vr = Vector2D.FromAngleSize((-30.0).ToRadian(), 0.3);//new Vector2D(0.3, 0.3);
                Vector2D vm = Vector2D.FromAngleSize((0.0).ToRadian(), 0.3);//new Vector2D(0.3, 0);
                obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
                obs.State = Model.OurRobots[RobotID];
                obs.Type = ObstacleType.OurRobot;
                if (!rotateStarted)
                {
                    int count = 0;
                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                        count++;
                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                        count++;
                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                        count++;
                    if (CurrentState != (int)ActiveRoleState.Conflict && count > 2)
                        behindBallOur = true;
                    else if (CurrentState == (int)ActiveRoleState.Conflict && count > 1)
                        behindBallOur = true;
                }
                else
                {
                    behindBallOur = true;
                }
            }
            if (behindBallOur)
            {
                foreach (var item in Model.Opponents.Keys)
                {
                    SingleObjectState oppRobot = Model.Opponents[item];
                    Vector2D v = oppRobot.Location - ball.Location;

                    if (v.Size < NewActiveParameters.ConfilictZone)
                    {
                        Vector2D vl = Vector2D.FromAngleSize((150.0).ToRadian(), 0.3);//new Vector2D(-0.3, -0.3);
                        Vector2D vr = Vector2D.FromAngleSize((-150.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0.3);
                        Vector2D vm = Vector2D.FromAngleSize((180.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0);
                        obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
                        obs.State = Model.Opponents[item];
                        obs.Type = ObstacleType.OppRobot;
                        if (!rotateStarted)
                        {
                            int count = 0;
                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                                count++;
                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                                count++;
                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                                count++;

                            if (CurrentState != (int)ActiveRoleState.Conflict && count > 2)
                            {
                                if (!lastRobotAngle.HasValue)
                                    lastRobotAngle = Model.OurRobots[RobotID].Angle.Value;
                                oppConfID = item;
                                return true;
                            }
                            else if (CurrentState == (int)ActiveRoleState.Conflict && count > 1)
                            {
                                oppConfID = item;
                                return true;
                            }
                        }
                        else
                            return true;
                    }

                }
            }
            return false;
        }
        public bool IsConflictRoateSpin(WorldModel Model, int RobotID, SingleObjectState ball)
        {
            if (NormalSharedState.ActiveInfo.isRotateStarted)
            {
                return true;
            }
            bool behindBallOur = false;
            Obstacle obs = new Obstacle(), obs2 = new Obstacle();

            if ((CurrentState != (int)ActiveRoleState.Conflict && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < NewActiveParameters.ConfilictZone)
            || (CurrentState == (int)ActiveRoleState.Conflict && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.3))
            {
                Vector2D vl = Vector2D.FromAngleSize((30.0).ToRadian(), 0.3);//new Vector2D(0.3, -0.3);
                Vector2D vr = Vector2D.FromAngleSize((-30.0).ToRadian(), 0.3);//new Vector2D(0.3, 0.3);
                Vector2D vm = Vector2D.FromAngleSize((0.0).ToRadian(), 0.3);//new Vector2D(0.3, 0);
                obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
                obs.State = Model.OurRobots[RobotID];
                obs.Type = ObstacleType.OurRobot;
                if (!rotateStarted)
                {
                    int count = 0;
                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                        count++;
                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                        count++;
                    if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                        count++;
                    if (CurrentState != (int)ActiveRoleState.Conflict && count > 2)
                        behindBallOur = true;
                    else if (CurrentState == (int)ActiveRoleState.Conflict && count > 1)
                        behindBallOur = true;
                }
                else
                {
                    behindBallOur = true;
                }
            }
            if (behindBallOur)
            {
                foreach (var item in Model.Opponents.Keys)
                {
                    SingleObjectState oppRobot = Model.Opponents[item];
                    Vector2D v = oppRobot.Location - ball.Location;

                    if (v.Size < NewActiveParameters.ConfilictZone)
                    {
                        Vector2D vl = Vector2D.FromAngleSize((150.0).ToRadian(), 0.3);//new Vector2D(-0.3, -0.3);
                        Vector2D vr = Vector2D.FromAngleSize((-150.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0.3);
                        Vector2D vm = Vector2D.FromAngleSize((180.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0);
                        obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
                        obs.State = Model.Opponents[item];
                        obs.Type = ObstacleType.OppRobot;
                        if (!rotateStarted)
                        {
                            int count = 0;
                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                                count++;
                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                                count++;
                            if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                                count++;

                            if (CurrentState != (int)ActiveRoleState.Conflict && count > 2)
                            {
                                if (!lastRobotAngle.HasValue)
                                    lastRobotAngle = Model.OurRobots[RobotID].Angle.Value;
                                oppConfID = item;
                                return true;
                            }
                            else if (CurrentState == (int)ActiveRoleState.Conflict && count > 1)
                            {
                                oppConfID = item;
                                return true;
                            }
                        }
                        else
                            return true;
                    }

                }
            }
            return false;
        }
        public void ResetConflictRotate()
        {
            confWaitCounter = 0;
            rotateSpeed = 0;
            lastRobotAngle = null;
            rotateStarted = false;
            NormalSharedState.ActiveInfo.isRotateStarted = false;
        }
        bool IsConflict(WorldModel Model, int RobotID, SingleObjectState ball)
        {
            bool behindBallOur = false;
            Obstacle obs = new Obstacle(), obs2 = new Obstacle();

            if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < NewActiveParameters.ConfilictZone)
            {
                Vector2D vl = Vector2D.FromAngleSize((30.0).ToRadian(), 0.3);//new Vector2D(0.3, -0.3);
                Vector2D vr = Vector2D.FromAngleSize((-30.0).ToRadian(), 0.3);//new Vector2D(0.3, 0.3);
                Vector2D vm = Vector2D.FromAngleSize((0.0).ToRadian(), 0.3);//new Vector2D(0.3, 0);
                obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
                obs.State = Model.OurRobots[RobotID];
                obs.Type = ObstacleType.OurRobot;

                int count = 0;
                if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                    count++;
                if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                    count++;
                if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                    count++;
                if (CurrentState != (int)ActiveRoleState.Conflict && count > 2)
                    behindBallOur = true;
                else if (CurrentState == (int)ActiveRoleState.Conflict && count > 1)
                    behindBallOur = true;

            }
            if (behindBallOur)
            {
                foreach (var item in Model.Opponents.Keys)
                {
                    SingleObjectState oppRobot = Model.Opponents[item];
                    Vector2D v = oppRobot.Location - ball.Location;

                    if (v.Size < NewActiveParameters.ConfilictZone)
                    {
                        Vector2D vl = Vector2D.FromAngleSize((150.0).ToRadian(), 0.3);//new Vector2D(-0.3, -0.3);
                        Vector2D vr = Vector2D.FromAngleSize((-150.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0.3);
                        Vector2D vm = Vector2D.FromAngleSize((180.0).ToRadian(), 0.3);//new Vector2D(-0.3, 0);
                        obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
                        obs.State = Model.Opponents[item];
                        obs.Type = ObstacleType.OppRobot;
                        int count = 0;

                        if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vl, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                            count++;
                        if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vr, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                            count++;
                        if (obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + vm, Vector2D.Zero, 0), MotionPlannerParameters.BallRadi))
                            count++;

                        if (count > 2)
                            return true;

                    }

                }
            }
            return false;
        }
        #endregion
    }
}
