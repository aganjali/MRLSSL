using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using MRL.SSL.AIConsole.Roles.Stop;

namespace MRL.SSL.AIConsole.Plays
{
    class ourBallPalcmentPlay : PlayBase
    {
        bool flag = true;
        bool first = true, passed = true;
        int? CatcherID, ShooterID;
        const double faildBallMovedDist = 0.06;
        Position2D firstBallPos;
        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            if (Model.Status == GameStatus.BallPlace_OurTeam)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #region comment
        /* private void AssignIDs(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> att)
        {
            firstBallPos = Model.BallState.Location;
            double minDist = double.MaxValue;
            int minIdx = -1;
            foreach (var item in Attendance.Keys.ToList())
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            CatcherID = minIdx;
            ShooterID = -1;
            foreach (var item in Attendance.Keys.ToList())
            {
                if (item != CatcherID)
                {
                    ShooterID = item;
                    break;
                }
            }
            if (ShooterID == -1)
                return;
            first = false;
        }*/
        #endregion
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            firstBallPos = Model.BallState.Location;
            #region First
            if (flag)
            {
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                {
                    flag = false;
                }
                //if (Model.FieldIsInverted)
                //    StaticVariables.ballPlacementPos = new Position2D(-StaticVariables.ballPlacementPos.X / 1000, StaticVariables.ballPlacementPos.Y / 1000);
                //else
                //    StaticVariables.ballPlacementPos = new Position2D(StaticVariables.ballPlacementPos.X / 1000, -StaticVariables.ballPlacementPos.Y / 1000);
                CatcherID = null;
                ShooterID = null;
                double min = double.MaxValue;
                foreach (var item in Model.OurRobots.Keys)
                {
                    if (!(Model.GoalieID.HasValue && item == Model.GoalieID.Value))
                    {
                        if (Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < min)
                        {
                            min = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                            ShooterID = item;
                        }
                    }
                }
                min = double.MaxValue;
                foreach (var item in Model.OurRobots.Keys)
                {
                    if (!(Model.GoalieID.HasValue && item == Model.GoalieID.Value) && !(ShooterID.HasValue && item == ShooterID.Value))
                    {
                        if (Model.OurRobots[item].Location.DistanceFrom(StaticVariables.ballPlacementPos) < min)
                        {
                            min = Model.OurRobots[item].Location.DistanceFrom(StaticVariables.ballPlacementPos);
                            CatcherID = item;
                        }
                    }
                }

                DrawingObjects.AddObject(new StringDraw("ShooterID= " + ShooterID, new Position2D(4.5, 5)), "ShooterID");
                DrawingObjects.AddObject(new StringDraw("CatcherID= " + CatcherID, new Position2D(4, 5)), "CatcherID");
            #endregion
                double d = Model.OurRobots[ShooterID.Value].Location.DistanceFrom(StaticVariables.ballPlacementPos);
                Planner.Add(ShooterID.Value, Model.BallState.Location, (StaticVariables.ballPlacementPos - Model.BallState.Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (d < 5)
                {
                    Planner.AddKick(ShooterID.Value, d);
                    DrawingObjects.AddObject(new StringDraw("d1= " + CatcherID, new Position2D(4, 5)), "d1");
                }
                else
                {
                    Planner.AddKick(ShooterID.Value, 5);
                    DrawingObjects.AddObject(new StringDraw("d2= " + CatcherID, new Position2D(4, 5)), "d2");
                }

                Position2D p1, p2;
                double dist1, dist2;
                Position2D tar;
                Circle C = new Circle(StaticVariables.ballPlacementPos, 0.7);
                DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.7, new Pen(Color.Red, 0.01f)), "C");
                Line line = new Line(Model.BallState.Location, StaticVariables.ballPlacementPos);
                List<Position2D> pos = C.Intersect(line);
                p1 = pos.First();
                p2 = pos.Last();

                dist1 = Model.BallState.Location.DistanceFrom(p1);
                dist2 = Model.BallState.Location.DistanceFrom(p2);
                if (dist1 < dist2)
                {
                    tar = p2;
                    DrawingObjects.AddObject(new Circle(p2, 0.04, new Pen(Color.Yellow, 0.01f)), "p2c");
                }
                else
                {
                    tar = p1;
                    DrawingObjects.AddObject(new Circle(p1, 0.04, new Pen(Color.White, 0.01f)), "p1c");
                }

                Planner.Add(CatcherID.Value, tar, (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            }
            if (!flag)
            {
                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID.Value].Location) > 0.9)
                {
                    Planner.Add(ShooterID.Value, Model.BallState.Location, (StaticVariables.ballPlacementPos - Model.BallState.Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                    Planner.Add(CatcherID.Value, Model.BallState.Location, (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                    if (Model.OurRobots[ShooterID.Value].Location.DistanceFrom(Model.BallState.Location) < 0.1 && Model.OurRobots[CatcherID.Value].Location.DistanceFrom(Model.BallState.Location) < 0.1 && Model.OurRobots[CatcherID.Value].Location.DistanceFrom(Model.OurRobots[ShooterID.Value].Location) < 0.2)
                    {
                        Planner.Add(ShooterID.Value, StaticVariables.ballPlacementPos, (StaticVariables.ballPlacementPos - Model.BallState.Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                        Planner.Add(CatcherID.Value, StaticVariables.ballPlacementPos, (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                    }
                }
            }
            //if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID.Value].Location) > 0.9)
            //{
            //    Planner.Add(ShooterID.Value, Model.BallState.Location, (StaticVariables.ballPlacementPos - Model.BallState.Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            //    Planner.Add(CatcherID.Value, Model.BallState.Location, (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            //    if (Model.OurRobots[ShooterID.Value].Location.DistanceFrom(Model.BallState.Location) < 0.1 && Model.OurRobots[CatcherID.Value].Location.DistanceFrom(Model.BallState.Location) < 0.1 && Model.OurRobots[CatcherID.Value].Location.DistanceFrom(Model.OurRobots[ShooterID.Value].Location) < 0.2)
            //    {
            //        Planner.Add(ShooterID.Value, StaticVariables.ballPlacementPos, (StaticVariables.ballPlacementPos - Model.BallState.Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            //        Planner.Add(CatcherID.Value, StaticVariables.ballPlacementPos, (Model.BallState.Location - StaticVariables.ballPlacementPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            //    }
            //}
            Dictionary<RoleBase, Position2D?> Positions = new Dictionary<RoleBase, Position2D?>();
            Dictionary<RoleBase, double> Angles = new Dictionary<RoleBase, double>();
            bool isInDangerZone = false;
            double d1, d2;
            if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.2, out d1, out d2))
                isInDangerZone = true;

            if (!isInDangerZone)
            {
                Position2D GoaliPos = new Position2D();
                double gteta = 0;
                if (Positions.Any(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)))
                    GoaliPos = Positions.Where(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)).First().Value.Value;
                if (Angles.Any(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)))
                    gteta = Angles.Where(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)).First().Value;

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoaliBallPalcmentRole)))
                    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoaliBallPalcmentRole>(Model.GoalieID.Value).RunStop(eng, wmd, Model.GoalieID.Value, GoaliPos, (Model.BallState.Location - Model.OurRobots[Model.GoalieID.Value].Location).AngleInDegrees);
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            flag = true;
            //StaticVariables.ballPlacementPos=Position2D.Zero;

        }
    }
}
