//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using MRL.SSL.AIConsole.Engine;
//using MRL.SSL.GameDefinitions;
//using MRL.SSL.CommonClasses.MathLibrary;
//using MRL.SSL.AIConsole.Skills;
//using MRL.SSL.Planning.MotionPlanner;

//namespace MRL.SSL.AIConsole.Roles
//{
//    public class NewSupporterRole:RoleBase
//    {
//        public override RoleCategory QueryCategory()
//        {
//            return RoleCategory.Active;
//        }
//        double angelInterval = 90, xDangerTresh = 0.6, xMargin = 0.1, angleMargin = 20, distNextTheBallY = 0.15, distNextTheBallX = -0.4, safeRadiMargin = 0.3, behindBallDist = 0;
//        bool Debug = true;
//        int angSt = 0;
//        Position2D Target = Position2D.Zero;
//        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
//        {
//            Target = CalculateTarget(engine, Model, RobotID);
//            if (CurrentState == (int)State.Normal)
//            {
//                if (IsInFrontOfDanger(Model, 0, 0))
//                    CurrentState = (int)State.InFrontOfDangerZone;
//                else if (!CalculateBehindBallDist(engine, Model, RobotID, Target, out behindBallDist))
//                    CurrentState = (int)State.Side;
//            }
//            else if (CurrentState == (int)State.InFrontOfDangerZone)
//            {
//               if (!CalculateBehindBallDist(engine, Model, RobotID, Target, out behindBallDist))
//                    CurrentState = (int)State.Side;
//               else if (!IsInFrontOfDanger(Model, xMargin, angleMargin))
//                   CurrentState = (int)State.Normal;
//            }

//            if (CurrentState == (int)State.InFrontOfDangerZone)
//            {
//                Target = Model.BallState.Location + new Vector2D(distNextTheBallX, Math.Sign(Model.BallState.Location.Y) * distNextTheBallY);
//                CalculateBehindBallDist(engine, Model, RobotID, Target, out behindBallDist);
//                angSt = 1;
//            }

//            if (Debug)
//            {
//                DrawingObjects.AddObject(new StringDraw("SupportRoleState: " + (State)CurrentState, new Position2D(-1, 0)));
//            }
//        }
        
//        private bool IsInFrontOfDanger(WorldModel Model, double marginX, double marginAng)
//        {
//            if (Model.BallState.Location.X > (GameParameters.OurGoalCenter.X - GameParameters.DefenceareaRadii-  xDangerTresh - marginX))
//            {
//                Vector2D ballOurGoalCenter = (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormnalizedCopy();
//                Vector2D leftBoundVec = Vector2D.FromAngleSize((180 - (angelInterval + marginAng) / 2).ToRadian(), 1);
//                Vector2D rightBoundVec = Vector2D.FromAngleSize((-180 + (angelInterval + marginAng) / 2).ToRadian(), 1);
//                Vector3D n = leftBoundVec * rightBoundVec;
//                if (n.InnerProduct(leftBoundVec * ballOurGoalCenter) >= 0 && n.InnerProduct(ballOurGoalCenter * rightBoundVec) >= 0)
//                    return true;
//            }
//            return false;
//        }
//        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, int ActiveID,int ActiveState, Position2D ActiveTarget, bool far, Position2D incomPred)
//        {
//            if (CurrentState == (int)State.Side)
//            {
//                Target = Model.BallState.Location + (Target - Model.BallState.Location).GetNormalizeToCopy(behindBallDist);
//                Planner.Add(RobotID, Target, (Model.BallState.Location - GameParameters.OurGoalCenter).AngleInDegrees, PathType.UnSafe, true, true, true, false);
//            }
//            else
//                GetSkill<SupportBallSkill>().Perform(engine, Model, RobotID, ActiveID, ActiveState, Target, ActiveTarget, behindBallDist, far, incomPred, angSt);
//        }
//        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            Position2D tar = CalculateTarget(engine, Model, RobotID);
//            if (IsInFrontOfDanger(Model, 0, 0))
//                  tar = Model.BallState.Location + new Vector2D(distNextTheBallX, Math.Sign(Model.BallState.Location.Y) * distNextTheBallY);
//            double d;
//            CalculateBehindBallDist(engine, Model, RobotID, tar, out d);
//            tar = (tar - Model.BallState.Location).GetNormalizeToCopy(d) + Model.BallState.Location;
//            return tar.DistanceFrom(Model.OurRobots[RobotID].Location);
//        }

//        private bool CalculateBehindBallDist(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, out double res)
//        {
//            double d = 0.5;
//            double DistFromBorder, dist;
//            Vector2D refrence = Target - Model.BallState.Location;
//            Target = Model.BallState.Location + (Target - Model.BallState.Location).GetNormalizeToCopy(d);
//            if (GameParameters.IsInDangerousZone(Target, false, safeRadiMargin + 0.1, out dist, out DistFromBorder))
//            {
//                double safeRad = GameParameters.SafeRadi(new SingleObjectState(Target, Vector2D.Zero, 0), safeRadiMargin);
//                Target = GameParameters.OurGoalCenter + (Target - GameParameters.OurGoalCenter).GetNormalizeToCopy(safeRad);
//                Vector2D tmpV = Target - Model.BallState.Location;
//                tmpV = GameParameters.InRefrence(tmpV, refrence);
//                res = tmpV.Y;
//                return false;
//            }
//            res = d;
//            return true;
//        }

//        private Position2D CalculateTarget(GameStrategyEngine engine, WorldModel Model, int RobotID)
//        {
//            return GameParameters.OurGoalCenter;
//        }

//        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            List<RoleBase> res = new List<RoleBase>() {
//            new ActiveRole(),new AttackerRole(),
//            new NewSupporterRole(), new DefenderMarkerNormalRole1()};
//            return res;
//        }

//        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
//        {
//            throw new NotImplementedException();
//        }
//        enum State
//        {
//            Normal = 0,
//            InFrontOfDangerZone = 1,
//            Side = 2
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    public class NewSupporterRole : RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        double angelInterval = 90, sideHist = 0.1, xDangerTresh = 0.6, xMargin = 0.1, angleMargin = 20, distNextTheBallY = 0.15, distNextTheBallX = -0.4, safeRadiMargin = 0.3, behindBallDist = 0;
        bool Debug = true;
        int angSt = 0;
        double margin = 0, sign = 1;
        bool calCost = false;
        DangerZoneSide side = DangerZoneSide.Left;
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (CurrentState == (int)State.Normal)
            {
                margin = 0;
                Position2D p = Model.BallState.Location + (GameParameters.OurGoalCenter - Model.BallState.Location ).GetNormalizeToCopy(0.5);
                double dist, DistFromBorder;
                if (GameParameters.IsInDangerousZone(p, false, 0.3, out dist, out DistFromBorder))
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
                if (side == DangerZoneSide.Right &&  Model.BallState.Location.Y > sideHist)
                    side = DangerZoneSide.Left;
                else if (side == DangerZoneSide.Left && Model.BallState.Location.Y < -sideHist)
                    side = DangerZoneSide.Right;
                margin = 0.1;
                Position2D p = Model.BallState.Location + (GameParameters.OurGoalCenter - Model.BallState.Location ).GetNormalizeToCopy(0.5);
                double dist, DistFromBorder;
                if (!GameParameters.IsInDangerousZone(p, false, 0.4, out dist, out DistFromBorder))
                    CurrentState = (int)State.Normal;
            }


            if (Debug && !calCost)
            {
                DrawingObjects.AddObject(new StringDraw("SupportRoleState: " + (State)CurrentState, new Position2D(-1, 0)));
            }
        }

        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, int ActiveID, int ActiveState, Position2D ActiveTarget, bool far, Position2D incomPred)
        {
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
                if(side == DangerZoneSide.Left)
                {
                    tar = new Position2D(1.65, 0.5);
                }
                else
                    tar = new Position2D(1.65, -0.5);
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
            new ActiveRole(),new AttackerRole(),
            new NewSupporterRole(), new DefenderMarkerNormalRole1()};
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
            Left= 0,
            Right = 1
        }
    }
}

