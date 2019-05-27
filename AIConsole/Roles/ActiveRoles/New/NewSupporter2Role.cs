
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles.Defending.Normal;

namespace MRL.SSL.AIConsole.Roles
{
    public class NewSupporter2Role : RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        double angelInterval = 90, sideHist = 0.1, xDangerTresh = 0.6, xMargin = 0.1, angleMargin = 20, distNextTheBallY = 0.15, distNextTheBallX = -0.4, safeRadiMargin = 0.3, behindBallDist = 0;
        bool Debug = false;
        int angSt = 0;
        double margin = 0, sign = 1;
        bool calCost = false;
        DangerZoneSide side = DangerZoneSide.Left;
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (CurrentState == (int)State.Normal)
            {
                margin = 0;
                Position2D p = Model.BallState.Location + (GameParameters.OurGoalCenter - Model.BallState.Location).GetNormalizeToCopy(0.5);
                double dist, DistFromBorder;
                if (GameParameters.IsInDangerousZone(p, false, 0.3, out dist, out DistFromBorder) || Model.BallState.Location.X > 3.2)
                {
                    CurrentState = (int)State.InFrontOfDangerZone;
                    if (Model.BallState.Location.Y > 0)
                        side = DangerZoneSide.Left;
                    else
                        side = DangerZoneSide.Right;
                }
            }
            else if (CurrentState == (int)State.InFrontOfDangerZone)
            {
                if (side == DangerZoneSide.Right && Model.BallState.Location.Y > sideHist)
                    side = DangerZoneSide.Left;
                else if (side == DangerZoneSide.Left && Model.BallState.Location.Y < -sideHist)
                    side = DangerZoneSide.Right;
                margin = 0.1;
                Position2D p = Model.BallState.Location + (GameParameters.OurGoalCenter - Model.BallState.Location).GetNormalizeToCopy(0.5);
                double dist, DistFromBorder;
                if (!GameParameters.IsInDangerousZone(p, false, 0.4, out dist, out DistFromBorder) && Model.BallState.Location.X <= 3.2)
                    CurrentState = (int)State.Normal;
            }


            if (Debug && !calCost)
            {
                DrawingObjects.AddObject(new StringDraw("SupportRoleState: " + (State)CurrentState, new Position2D(-1, 0)));
            }
        }

        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            int? ActiveID = NormalSharedState.CommonInfo.ActiveID;
            int ActiveState = (ActiveID.HasValue) ? (int)NormalSharedState.ActiveInfo.ActiveSkillState : (int)NormalSharedState.GetBallState.Static;
            Position2D ActiveTarget = (ActiveID.HasValue) ? NormalSharedState.ActiveInfo.Target : GameParameters.OppGoalCenter;
            bool far = (ActiveID.HasValue) ? !NormalSharedState.ActiveInfo.IsNear : false;
            Position2D incomPred = (ActiveID.HasValue) ? NormalSharedState.ActiveInfo.IncomingPred : Model.BallState.Location;

            double Teta;
            Position2D t = CalculateTarget(engine, Model, RobotID, out Teta);
            if (CurrentState == (int)State.Normal)
            {
                GetSkill<SupportBallSkill>().Perform(engine, Model, RobotID, ActiveID, ActiveState, GameParameters.OurGoalCenter, ActiveTarget, 0.5, far, incomPred, 0);
            }
            else if (CurrentState == (int)State.InFrontOfDangerZone)
            {
                Planner.Add(RobotID, t, Teta, PathType.UnSafe, false, true, true, false);
            }
        }
        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            //calCost = true;
            //DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles);
            //double Teta;
            //Position2D t = CalculateTarget(engine, Model, RobotID, out Teta);
            //return t.DistanceFrom(Model.OurRobots[RobotID].Location);
            double failMargin = 0.1, supportDist = 0.5;

            if (engine.GameInfo.OurTeam.CatchBallLines.ContainsKey(RobotID) &&
                engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
            {
                var p = engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head;
                p = p + (GameParameters.OurGoalCenter - p).GetNormalizeToCopy(supportDist);
                return p.DistanceFrom(Model.OurRobots[RobotID].Location);
            }
            double d1 = (double)MRL.SSL.Planning.GamePlanner.BallState.maxLengh / 100.0 + failMargin;

            Vector2D ballSpeed = Model.BallState.Speed.GetNormalizeToCopy(d1);
            Position2D p1 = Model.BallState.Location + ballSpeed;
            p1 = p1 + (GameParameters.OurGoalCenter - p1).GetNormalizeToCopy(supportDist);
            double d = Model.OurRobots[RobotID].Location.DistanceFrom(p1);
            return 0 + d;
        }


        private Position2D CalculateTarget(GameStrategyEngine engine, WorldModel Model, int RobotID, out double Teta)
        {
            Position2D tar = Position2D.Zero;
            Teta = (Model.BallState.Location - GameParameters.OurGoalCenter).AngleInDegrees;
            if (CurrentState == (int)State.Normal)
            {
                tar = Model.BallState.Location + (GameParameters.OurGoalCenter - Model.BallState.Location).GetNormalizeToCopy(0.5);
            }
            else if (CurrentState == (int)State.InFrontOfDangerZone)
            {
                if (side == DangerZoneSide.Left)
                {
                    tar = new Position2D(1.65 * GameParameters.OurGoalCenter.X / 3, 0.5 * Math.Abs(GameParameters.OurLeftCorner.Y) / 2);
                }
                else
                    tar = new Position2D(1.65 * GameParameters.OurGoalCenter.X / 3, -0.5 * Math.Abs(GameParameters.OurLeftCorner.Y) / 2);
                Teta = 180;
                //Vector2D ourGoalBallVec = (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(Model.BallState, 0.2));
                //if (Model.BallState.Location.Y >= margin)
                //{
                //    sign = 1;
                //}
                //else if (Model.BallState.Location.Y <= -margin)
                //{
                //    sign = -1;
                //}
                //Vector2D v = Vector2D.FromAngleSize(ourGoalBallVec.AngleInRadians + sign * Math.PI / 2, 0.2);
                //tar = (GameParameters.OurGoalCenter + ourGoalBallVec) + v;
                //Teta = ourGoalBallVec.AngleInDegrees;
            }
            return tar;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() {
            new ActiveRole2017(),
            new NewSupporter2Role(),new NewRegionalRole(),};
            if (NormalSharedState.CommonInfo.PickIsFeasible && !NormalSharedState.CommonInfo.IsPicking)
            {

                if (NormalSharedState.CommonInfo.PickerID == NormalSharedState.CommonInfo.SupporterID)
                    res.Add(new NewPickerRole());
            }
            else if (!NormalSharedState.CommonInfo.PickIsFeasible && NormalSharedState.CommonInfo.IsPicking)
            {
                res.Add(new NewPickerRole());
            }
            if (!NormalSharedState.CommonInfo.AttackerMode)
            {
                res.Add(new NewAttackerRole());
                //res.Add(new AttackerRole());
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
        enum State
        {
            Normal = 0,
            InFrontOfDangerZone = 1,
        }
        enum DangerZoneSide
        {
            Left = 0,
            Right = 1
        }
    }
}

