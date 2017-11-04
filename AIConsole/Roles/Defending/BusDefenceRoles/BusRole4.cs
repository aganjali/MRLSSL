using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class BusRole4 : RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }
        int counterBalInFront = 0;
        public int? OppID = null;
        double AdditionalSafeRadi = 0.08;
        public Position2D target = new Position2D();
        public Position2D? overlaptarget = null;
        public int RegionID = 3;
        double angleDegree = 180;
        public int regionNum = 3;
        int? oppFirstkicker = null;
        Position2D lastBestTarget = Position2D.Zero;
        Position2D lastBestOverlaptarget = Position2D.Zero;
        double MinDistRobots = 0.21;

        public void perform(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, int RegionID, Dictionary<int, List<BusRegion>> oppAttackers, List<Vector2D> RegionVec, int? OppToMark, bool ballIsMoeved, Position2D firstBallPos, Dictionary<int, RoleBase> AssignedRoles)
        {
            this.RegionID = RegionID;
            OppID = null;
            //if (firstBallPos.Y > 0 && RegionID > 0)
            //{
            //if (oppAttackers[RegionID].Count == 0 && oppAttackers[RegionID - 1].Count > 2)
            //    RegionID = RegionID - 1;
            //if (oppAttackers[RegionID].Count == 0 && oppAttackers[RegionID - 1].Count == 1)
            //    RegionID = RegionID - 1;
            //else if (oppAttackers[RegionID].Count == 0 && RegionID == 2 && oppAttackers[RegionID - 1].Count > 2 && oppAttackers[RegionID - 2].Count > 0)
            //    RegionID = RegionID - 1;
            //else if (oppAttackers[RegionID].Count == 0 && RegionID == 2 && oppAttackers[RegionID - 1].Count > 0 && oppAttackers[RegionID - 2].Count > 2)
            //    RegionID = RegionID - 1;
            //else if (oppAttackers[RegionID].Count == 0 && RegionID == 2 && oppAttackers[RegionID - 2].Count > 3 && oppAttackers[RegionID - 2].Count > 0)
            //    RegionID = RegionID - 2;
            //}
            OppID = selectOppID(engine, Model, RobotID, RegionID, oppAttackers, RegionVec, OppToMark, firstBallPos, AssignedRoles);


            #region Mark
            if (CurrentState == (int)BusState.Mark && OppID.HasValue && Model.Opponents.ContainsKey(OppID.Value))
            {

                Line l1 = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalLeft + (Model.Opponents[OppID.Value].Location - GameParameters.OurGoalLeft).GetNormalizeToCopy(3));
                Line l2 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + (Model.Opponents[OppID.Value].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(3));
                Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                posOnDangerzon = GameParameters.OurGoalLeft + (posOnDangerzon - GameParameters.OurGoalLeft).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalLeft) + 0.2);
                Line l3 = l2.PerpenducilarLineToPoint(posOnDangerzon);
                Position2D posToGo = l2.IntersectWithLine(l3).Value;
                posOnDangerzon = posOnDangerzon + (posToGo - posOnDangerzon).GetNormalizeToCopy(posToGo.DistanceFrom(posOnDangerzon) / 2);
                DrawingObjects.AddObject(l1);
                DrawingObjects.AddObject(l2);
                DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05));
                target = posOnDangerzon;
                angleDegree = (Model.Opponents[OppID.Value].Location - GameParameters.OurGoalLeft).AngleInDegrees;

                double x, y;
                Position2D? intersectPos = null;
                Line oppSpeedLine = new Line(Model.Opponents[OppID.Value].Location, Model.Opponents[OppID.Value].Location + Model.Opponents[OppID.Value].Speed);
                intersectPos = GameParameters.LineIntersectWithDangerZone(oppSpeedLine, true).FirstOrDefault();
                if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.05, out x, out y)
                    && OppID.HasValue && Model.Opponents.ContainsKey(OppID.Value) && Model.Opponents[OppID.Value].Location.DistanceFrom(target) < 0.5
                    && Model.OurRobots[RobotID].Location.DistanceFrom(target) > 0.2
                    && intersectPos.HasValue)
                {

                    intersectPos = null;
                    Line ballSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                    intersectPos = GameParameters.LineIntersectWithDangerZone(ballSpeedLine, true).FirstOrDefault();
                    if (intersectPos.HasValue)
                    {
                        target = intersectPos.Value + (intersectPos.Value - GameParameters.OurGoalLeft).GetNormalizeToCopy(0.12);
                        angleDegree = -Model.BallState.Speed.AngleInDegrees;
                    }
                }



                //Line l1 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter+(Model.Opponents[OppID.Value].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(10));
                //Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                //posOnDangerzon = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                //DrawingObjects.AddObject(l1);
                //DrawingObjects.AddObject(new Circle(posOnDangerzon, 0.05));
                //target = posOnDangerzon;
                //angleDegree = (Model.Opponents[OppID.Value].Location - GameParameters.OurGoalCenter).AngleInDegrees;

                //double x, y;
                //Position2D? intersectPos = null;
                //Line oppSpeedLine = new Line(Model.Opponents[OppID.Value].Location, Model.Opponents[OppID.Value].Location + Model.Opponents[OppID.Value].Speed);
                //intersectPos = GameParameters.LineIntersectWithDangerZone(oppSpeedLine, true).FirstOrDefault();
                //if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.05, out x, out y)
                //    && OppID.HasValue && Model.Opponents.ContainsKey(OppID.Value) && Model.Opponents[OppID.Value].Location.DistanceFrom(target) < 0.5
                //    && Model.OurRobots[RobotID].Location.DistanceFrom(target) > 0.2
                //    && intersectPos.HasValue)
                //{

                //    intersectPos = null;
                //    Line ballSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                //    intersectPos = GameParameters.LineIntersectWithDangerZone(ballSpeedLine, true).FirstOrDefault();
                //    if (intersectPos.HasValue)
                //    {
                //        target = intersectPos.Value + (intersectPos.Value - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.12);
                //        angleDegree = -Model.BallState.Speed.AngleInDegrees;
                //    }
                //}
            }
            #endregion
            #region KickToGoal
            if (CurrentState == (int)BusState.KickToGoal)
            {
                target = OppFreeKickDefenceUtils.Dive(engine, Model, RobotID);
                angleDegree = Model.OurRobots[RobotID].Angle.Value;
            }
            #endregion
            #region BallInBehind
            if (CurrentState == (int)BusState.BallInBehind)
            {
                target = BusBase.BehindSatate(engine, Model, new DefenceInfo(), RobotID, out angleDegree, FreekickDefence.CurrentStates);
            }
            #endregion
            #region InPenaltyArea
            if (CurrentState == (int)BusState.InPenaltyArea)
            {
                target = BusBase.MarkFront(engine, Model, RobotID, OppID, FreekickDefence.AdditionalSafeRadi, out angleDegree);
            }
            #endregion
            #region Regional
            else if (CurrentState == (int)BusState.Regional)
            {
                //if (!bus3.HasValue || (bus3.HasValue && !bus3.Value.OppID.HasValue))
                {
                    Vector2D v1 = RegionVec[RegionID];
                    Vector2D v2 = RegionVec[RegionID + 1];
                    double d = v1.AngleInDegrees + 5;//(v1.AngleInDegrees >= 0 ? (v1.AngleInDegrees + Math.Abs(Vector2D.AngleBetweenInDegrees(v1, v2)) / 2) : (v1.AngleInDegrees + Math.Abs(Vector2D.AngleBetweenInDegrees(v1, v2)) / 2));
                    v1 = Vector2D.FromAngleSize(d * (Math.PI / 180), 5);
                    Line l1 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + v1);
                    Position2D posOnDangerzon = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                    DrawingObjects.AddObject(posOnDangerzon);
                    if (posOnDangerzon != Position2D.Zero)
                    {
                        target = GameParameters.OurGoalCenter + (posOnDangerzon - GameParameters.OurGoalCenter).GetNormalizeToCopy(posOnDangerzon.DistanceFrom(GameParameters.OurGoalCenter) + 0.2);
                        angleDegree = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                    }
                }
                //else
                //{
                //    if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.OurRobots[bus3.Value.RobotID].Location) > 0.3)
                //    {
                //        target = Model.OurRobots[bus3.Value.RobotID].Location;
                //        angleDegree = bus3.Value.angleDegree;
                //    }
                //    else
                //    {
                //        target = Model.OurRobots[bus3.Value.RobotID].Location + (Model.OurRobots[RobotID].Location - Model.OurRobots[bus3.Value.RobotID].Location).GetNormalizeToCopy(0.22);
                //        angleDegree = bus3.Value.angleDegree;
                //    }
                //}
            }
            #endregion


            overlaptarget = OverlapPosition(engine, Model, RobotID, RegionID, oppAttackers, RegionVec, AssignedRoles);
            if (overlaptarget.HasValue)
            {
                Line ol = new Line(GameParameters.OurGoalCenter, overlaptarget.Value);
                Position2D? p = GameParameters.LineIntersectWithDangerZone(ol, true).FirstOrDefault();
                if (p.HasValue && p.Value != Position2D.Zero)
                {
                    overlaptarget = p.Value + (p.Value - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.2);
                }
            }


            if (overlaptarget.HasValue && overlaptarget.Value.X > 4.38)
                overlaptarget = new Position2D(4.38, overlaptarget.Value.Y);
            else if (target.X > 4.38)
                target = new Position2D(4.38, target.Y);


            Planner.Add(RobotID, (overlaptarget.HasValue ? overlaptarget.Value : target), angleDegree, PathType.Safe, false, false, true, true);
            DrawingObjects.AddObject(new StringDraw(((BusState)CurrentState).ToString(), Model.OurRobots[RobotID].Location.Extend(0.2, 0)), "mdug35968");
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            int? ballOwner = GetOurBallOwner(engine, Model, RobotID, (BusState)CurrentState, null);
            double d1, d2;
            #region Mark
            if (CurrentState == (int)BusState.Mark)
            {
                if (BusBase.BallKickedToGoal(engine, Model))
                {
                    CurrentState = (int)BusState.KickToGoal;
                }
                else if (BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.BallInBehind;
                }
                else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, OppFreeKickDefenceUtils.CornerInPenaltyAreaMargin, out d1, out d2))
                {
                    CurrentState = (int)BusState.InPenaltyArea;
                }
                else if ((!OppID.HasValue || (OppID.HasValue && !Model.Opponents.ContainsKey(OppID.Value))) && !BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.Regional;
                }
            }
            #endregion
            #region KickToGoal
            else if (CurrentState == (int)BusState.KickToGoal)
            {
                if (!BusBase.BallKickedToGoal(engine, Model) && OppID.HasValue && Model.Opponents.ContainsKey(OppID.Value) && !GameParameters.IsInDangerousZone(Model.Opponents[OppID.Value].Location, false, 0.02, out d1, out d2) && !BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.Mark;
                }
                else if (BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.BallInBehind;
                }
                else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, OppFreeKickDefenceUtils.CornerInPenaltyAreaMargin, out d1, out d2))
                {
                    CurrentState = (int)BusState.InPenaltyArea;
                }
                else if ((!OppID.HasValue || (OppID.HasValue && !Model.Opponents.ContainsKey(OppID.Value))) && !BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.Regional;
                }
            }
            #endregion
            #region InPenaltyArea
            if (CurrentState == (int)BusState.InPenaltyArea)
            {
                if (BusBase.BallKickedToGoal(engine, Model))
                {
                    CurrentState = (int)BusState.KickToGoal;
                }
                else if (BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.BallInBehind;
                }
                else if (!BusBase.BallKickedToGoal(engine, Model) && OppID.HasValue && Model.Opponents.ContainsKey(OppID.Value) && !GameParameters.IsInDangerousZone(Model.Opponents[OppID.Value].Location, false, 0.02, out d1, out d2) && !BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.Mark;
                }
                else if ((!OppID.HasValue || (OppID.HasValue && !Model.Opponents.ContainsKey(OppID.Value))) && !BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.Regional;
                }
            }
            #endregion
            #region BallInBehind
            if (CurrentState == (int)BusState.BallInBehind)
            {
                if (BusBase.BallKickedToGoal(engine, Model))
                {
                    CurrentState = (int)BusState.KickToGoal;
                }
                else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, OppFreeKickDefenceUtils.CornerInPenaltyAreaMargin, out d1, out d2))
                {
                    CurrentState = (int)BusState.InPenaltyArea;
                }
                else if (!BusBase.BallKickedToGoal(engine, Model) && OppID.HasValue && Model.Opponents.ContainsKey(OppID.Value) && !GameParameters.IsInDangerousZone(Model.Opponents[OppID.Value].Location, false, 0.02, out d1, out d2) && !BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.Mark;
                }
                else if ((!OppID.HasValue || (OppID.HasValue && !Model.Opponents.ContainsKey(OppID.Value))) && !BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.Regional;
                }
            }
            #endregion
            #region Regional
            else if (CurrentState == (int)BusState.Regional)
            {
                if (BusBase.BallKickedToGoal(engine, Model))
                {
                    CurrentState = (int)BusState.KickToGoal;
                }
                else if (BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.BallInBehind;
                }
                else if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, OppFreeKickDefenceUtils.CornerInPenaltyAreaMargin, out d1, out d2))
                {
                    CurrentState = (int)BusState.InPenaltyArea;
                }
                else if (!BusBase.BallKickedToGoal(engine, Model) && OppID.HasValue && Model.Opponents.ContainsKey(OppID.Value) && !GameParameters.IsInDangerousZone(Model.Opponents[OppID.Value].Location, false, 0.02, out d1, out d2) && !BusBase.BallInBehind(engine, Model, RobotID))
                {
                    CurrentState = (int)BusState.Mark;
                }
            }
            #endregion

            if (CurrentState != (int)BusState.BallInFront)
                counterBalInFront = 0;
        }

        private int? selectOppID(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, int RegionID, Dictionary<int, List<BusRegion>> oppAttackers, List<Vector2D> RegionVec, int? OppToMark, Position2D firstBallPos, Dictionary<int, RoleBase> AssignedRoles)
        {
            BusRole1 bus1 = null;
            BusRole2 bus2 = null;
            BusRole3 bus3 = null;
            BusRole4 bus4 = null;
            int? bus1ID = null;
            int? bus2ID = null;
            int? bus3ID = null;
            int? bus4ID = null;
            foreach (var item in AssignedRoles)
            {
                if (item.Value.GetType() == typeof(BusRole1))
                { bus1 = (BusRole1)item.Value; bus1ID = item.Key; }
                if (item.Value.GetType() == typeof(BusRole2))
                { bus2 = (BusRole2)item.Value; bus2ID = item.Key; }
                if (item.Value.GetType() == typeof(BusRole3))
                { bus3 = (BusRole3)item.Value; bus3ID = item.Key; }
                if (item.Value.GetType() == typeof(BusRole4))
                { bus4 = (BusRole4)item.Value; bus4ID = item.Key; }
            }
            OppID = null;
            double maxAngle = 0;
            double minAngle = double.MaxValue;
            foreach (var item in oppAttackers[RegionID])
            {
                Vector2D oppVec = (Model.Opponents[item.key].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(5);
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(oppVec, RegionVec[RegionID])) > maxAngle)
                {
                    maxAngle = Math.Abs(Vector2D.AngleBetweenInDegrees(oppVec, RegionVec[RegionID]));
                    OppID = item.key;
                }
            }

            return OppID;
        }

        private Position2D? OverlapPosition(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, int RegionID, Dictionary<int, List<BusRegion>> oppAttackers, List<Vector2D> RegionVec, Dictionary<int, RoleBase> AssignedRoles)
        {
            BusRole1 bus1 = null;
            BusRole2 bus2 = null;
            BusRole3 bus3 = null;
            BusRole4 bus4 = null;
            int? bus1ID = null;
            int? bus2ID = null;
            int? bus3ID = null;
            int? bus4ID = null;
            foreach (var item in AssignedRoles)
            {
                if (item.Value.GetType() == typeof(BusRole1))
                { bus1 = (BusRole1)item.Value; bus1ID = item.Key; }
                if (item.Value.GetType() == typeof(BusRole2))
                { bus2 = (BusRole2)item.Value; bus2ID = item.Key; }
                if (item.Value.GetType() == typeof(BusRole3))
                { bus3 = (BusRole3)item.Value; bus3ID = item.Key; }
                if (item.Value.GetType() == typeof(BusRole4))
                { bus4 = (BusRole4)item.Value; bus4ID = item.Key; }
            }
            Position2D? overlaptarget = null;

            if (target.X < 4.38)
            {
                if (bus3 != null && bus3.target.DistanceFrom(target) < MinDistRobots)
                {
                    if (target.DistanceFrom(bus3.target) > 0.02)
                        lastBestOverlaptarget = target;
                    else
                        target = lastBestOverlaptarget;
                    double dist = 0.1 + (target.DistanceFrom(bus3.target) / 2);
                    Vector2D v = (target - bus3.target).GetNormalizeToCopy(dist);
                    overlaptarget = bus3.target + v;
                }
                //if (bus3.HasValue && Model.OurRobots.ContainsKey(bus3.Value.RobotID) && Model.OurRobots[bus3.Value.RobotID].Location.DistanceFrom(target) < MinDistRobots)
                //{
                //    double dist = 0.1 + (target.DistanceFrom(Model.OurRobots[bus3.Value.RobotID].Location) / 2);
                //    Vector2D v = (target - Model.OurRobots[bus3.Value.RobotID].Location).GetNormalizeToCopy(dist);
                //    overlaptarget = Model.OurRobots[bus3.Value.RobotID].Location + v;
                //}
                //else if (bus3.HasValue && bus3.Value.OverlapLocation.HasValue && bus3.Value.OverlapLocation.Value.DistanceFrom(target) < MinDistRobots)
                //{
                //    double dist = 0.1 + (target.DistanceFrom(bus3.Value.OverlapLocation.Value) / 2);
                //    Vector2D v = (target - bus3.Value.OverlapLocation.Value).GetNormalizeToCopy(dist);
                //    overlaptarget = bus3.Value.OverlapLocation.Value + v;
                //}
                //else if (bus3.HasValue && bus3.Value.Location.DistanceFrom(target) < MinDistRobots)
                //{
                //    double dist = 0.1 + (target.DistanceFrom(bus3.Value.Location) / 2);
                //    Vector2D v = (target - bus3.Value.Location).GetNormalizeToCopy(dist);
                //    overlaptarget = bus3.Value.Location + v;
                //}
            }
            //else
            //    overlaptarget = new Position2D(4.38, target.Y);

            if ((overlaptarget.HasValue && overlaptarget.Value.DistanceFrom(target) > MinDistRobots) || (overlaptarget.HasValue && bus3.target.DistanceFrom(target) > MinDistRobots))
            {
                overlaptarget = null;
            }

            if (target.X > 4.38)
                overlaptarget = new Position2D(4.38, target.Y);
            if (overlaptarget.HasValue && overlaptarget.Value.X > 4.38)
                overlaptarget = new Position2D(4.38, target.Y);

            //if (bus3.HasValue && bus3.Value.Location.DistanceFrom(target) < 0.22)
            //{
            //    if ((bus2.HasValue && bus2.Value.Location.DistanceFrom(bus3.Value.Location) > 0.2) || !bus2.HasValue)
            //    {
            //        double dist = 0.1 + (target.DistanceFrom(bus3.Value.Location) / 2);
            //        Vector2D v = (target - bus3.Value.Location).GetNormalizeToCopy(dist);
            //        overlaptarget = bus3.Value.Location + v;
            //    }
            //    else
            //    {
            //        if (bus3.HasValue && bus3.Value.Location.DistanceFrom(bus2.Value.Location) < 0.22)
            //        {
            //            double dist = 0.1 +(target.DistanceFrom(bus3.Value.Location) / 2);
            //            Vector2D v = (target - bus3.Value.Location).GetNormalizeToCopy(dist);
            //            overlaptarget = bus3.Value.Location + v;
            //        }
            //        else if (bus3.HasValue && bus3.Value.OverlapLocation.HasValue)
            //        {
            //            double dist = 0.2;// +(target.DistanceFrom(bus3.Value.Location) / 2);
            //            Vector2D v = (target - bus3.Value.Location).GetNormalizeToCopy(dist);
            //            overlaptarget = bus3.Value.Location + v;
            //        }
            //    }
            //}
            return overlaptarget;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            if (target != Position2D.Zero)
                return target.DistanceFrom(Model.BallState.Location);
            return (Model.OurRobots[RobotID].Location.DistanceFrom(new Position2D(4.20, -1.5)));
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { new BusRole4(), new NewActiveRole() };
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public int? GetOurBallOwner(GameStrategyEngine engine, WorldModel Model, int RobotID, BusState CurrentState, List<int> exclude)
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
            if (CurrentState == BusState.BallInFront)
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
    }
}
