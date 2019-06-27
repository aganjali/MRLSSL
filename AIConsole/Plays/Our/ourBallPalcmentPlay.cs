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
        Vector2D vec1 = new Vector2D();
        const double faildBallMovedDist = 0.06, initDist = 0.22;
        Position2D jj;
        public int CurrentState;
        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            //return Model.Status == GameStatus.Normal;

            return Model.Status == GameStatus.BallPlace_OurTeam;
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

            List<int> ourRobot = new List<int>();
            foreach (var item in Model.OurRobots.Keys)
            {
                ourRobot.Add(item);
            }
            // StaticVariables.ballPlacementPos = new Position2D(-4, -1);
            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.7, new Pen(Color.HotPink, 0.01f)), "asghar");
            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.1, new Pen(Color.White, 0.01f)), "akbar");
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            Circle ballToAvoid = new Circle();
            Circle targetToAvoid = new Circle();
            Circle ballPlacementPos = new Circle();
            Line lineToAvoid = new Line();
            Line line1 = new Line();
            Line line2 = new Line();
            Line line11 = new Line();
            Line line22 = new Line();
            Line ourGoalBall = new Line();
            #region m
            Vector2D vec = StaticVariables.ballPlacementPos - Model.BallState.Location;
            Vector2D exVec1 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI / 2, 0.5);
            Vector2D exVec2 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI / 2, 0.5);
            Vector2D exVec11 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI, 0.5);
            Vector2D exVec22 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI, 0.5);
            Vector2D vecc = Model.BallState.Location - StaticVariables.ballPlacementPos;
            Vector2D exVec111 = Vector2D.FromAngleSize(vecc.AngleInRadians + Math.PI, 0.5);
            Vector2D exVec222 = Vector2D.FromAngleSize(vecc.AngleInRadians - Math.PI, 0.5);
            line11 = new Line(Model.BallState.Location + exVec1 + exVec11, Model.BallState.Location + exVec2 + exVec22);
            line22 = new Line(StaticVariables.ballPlacementPos + exVec1 + exVec111, StaticVariables.ballPlacementPos + exVec2 + exVec222);
            line1 = new Line(Model.BallState.Location + exVec1 + exVec11, StaticVariables.ballPlacementPos + exVec1 + exVec111);
            line2 = new Line(Model.BallState.Location + exVec2 + exVec22, StaticVariables.ballPlacementPos + exVec2 + exVec222);
            ourGoalBall = new Line(GameParameters.OurGoalCenter, StaticVariables.ballPlacementPos);
            #region Debug
            if (true)
            {
                line1.DrawPen = new Pen(Color.Red, 0.02f);
                line2.DrawPen = new Pen(Color.Blue, 0.02f);
                DrawingObjects.AddObject(line1);
                DrawingObjects.AddObject(line2);
                line11.DrawPen = new Pen(Color.Black, 0.02f);
                line22.DrawPen = new Pen(Color.Pink, 0.02f);
                DrawingObjects.AddObject(line11);
                DrawingObjects.AddObject(line22);
                DrawingObjects.AddObject(new Circle(Model.BallState.Location + exVec22, 0.04, new Pen(Color.Blue, 0.01f)), "ballcirc");
                DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos + exVec222, 0.04, new Pen(Color.Black, 0.01f)), "ballcircl");
            }
            #endregion
            #endregion
            NormalDefenceAssigner def = new Engine.NormalDefenceAssigner();
            Dictionary<RoleBase, Position2D?> Positions = new Dictionary<RoleBase, Position2D?>();
            Dictionary<RoleBase, double> Angles = new Dictionary<RoleBase, double>();
            bool isInDangerZone = false;
            double d, d2;
            RoleBase rt;
            List<RoleInfo> roles = new List<RoleInfo>();
            if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.2, out d, out d2))
                isInDangerZone = true;
            rt = typeof(BallPalcementCatcher).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 2, 0));
            rt = typeof(BallPalcementShooter).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 2, 0));
            if (!isInDangerZone)
            {
                def.Assign(engine, Model, out Positions, out Angles, false, false, false, false);
                rt = typeof(palcment1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));
                rt = typeof(palcment2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));
            }
            else
            {
                rt = typeof(DefenderStopRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));
                rt = typeof(DefenderStopRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));
            }
            rt = typeof(palcment3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(palcment4).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));

            rt = typeof(palcment5).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 1, 0));


            Dictionary<int, RoleBase> matched;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            int? Defender1ID = null;

            if (!isInDangerZone && matched.Any(w => w.Value.GetType() == typeof(palcment1)))
                Defender1ID = matched.Where(w => w.Value.GetType() == typeof(palcment1)).First().Key;
            else if (isInDangerZone && matched.Any(w => w.Value.GetType() == typeof(DefenderStopRole1)))
                Defender1ID = matched.Where(w => w.Value.GetType() == typeof(DefenderStopRole1)).First().Key;

            int? Defender2ID = null;
            if (!isInDangerZone && matched.Any(w => w.Value.GetType() == typeof(palcment2)))
                Defender2ID = matched.Where(w => w.Value.GetType() == typeof(palcment2)).First().Key;
            else if (isInDangerZone && matched.Any(w => w.Value.GetType() == typeof(DefenderStopRole2)))
                Defender2ID = matched.Where(w => w.Value.GetType() == typeof(DefenderStopRole2)).First().Key;


            int? stop1 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(palcment3)))
                stop1 = matched.Where(w => w.Value.GetType() == typeof(palcment3)).First().Key;

            int? stop2 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(palcment4)))
                stop2 = matched.Where(w => w.Value.GetType() == typeof(palcment4)).First().Key;

            int? stop3 = null;
            if (matched.Any(w => w.Value.GetType() == typeof(palcment5)))
                stop3 = matched.Where(w => w.Value.GetType() == typeof(palcment5)).First().Key;

            int? CatcherID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(BallPalcementCatcher)))
                CatcherID = matched.Where(w => w.Value.GetType() == typeof(BallPalcementCatcher)).First().Key;

            int? ShooterID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(BallPalcementShooter)))
                ShooterID = matched.Where(w => w.Value.GetType() == typeof(BallPalcementShooter)).First().Key;


            if (!isInDangerZone)
            {
                Position2D GoaliPos = new Position2D();
                if (ourRobot.Count > 2)
                {
                    double gteta = 0;
                    if (Positions.Any(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)))
                        GoaliPos = Positions.Where(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)).First().Value.Value;
                    if (Angles.Any(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)))
                        gteta = Angles.Where(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)).First().Value;
                }
                Position2D Def1Pos = new Position2D();
                double d1teta = 0;
                if (Positions.Any(w => w.Key.GetType() == typeof(palcment1)))
                    Def1Pos = Positions.Where(w => w.Key.GetType() == typeof(palcment1)).First().Value.Value;
                if (Angles.Any(w => w.Key.GetType() == typeof(palcment1)))
                    d1teta = Angles.Where(w => w.Key.GetType() == typeof(palcment1)).First().Value;

                Position2D Def2Pos = new Position2D();
                double d2teta = 0;
                if (Positions.Any(w => w.Key.GetType() == typeof(palcment2)))
                    Def2Pos = Positions.Where(w => w.Key.GetType() == typeof(palcment2)).First().Value.Value;
                if (Angles.Any(w => w.Key.GetType() == typeof(palcment2)))
                    d2teta = Angles.Where(w => w.Key.GetType() == typeof(palcment2)).First().Value;
                if (ourRobot.Count >= 2)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(BallPalcementShooter)))
                        Functions[ShooterID.Value] = (eng, wmd) => GetRole<BallPalcementShooter>(ShooterID.Value).Perform(eng, wmd, ShooterID.Value, 0);

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, CatcherID, typeof(BallPalcementCatcher)))
                        Functions[CatcherID.Value] = (eng, wmd) => GetRole<BallPalcementCatcher>(CatcherID.Value).Perform(eng, wmd, CatcherID.Value, 1);

                }
                else if (ourRobot.Count < 2)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(BallPalcementShooter)))
                        Functions[ShooterID.Value] = (eng, wmd) => GetRole<BallPalcementShooter>(ShooterID.Value).Perform(eng, wmd, ShooterID.Value, 0);
                }
                if (ourRobot.Count > 2)
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoaliBallPalcmentRole)))
                        Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoaliBallPalcmentRole>(Model.GoalieID.Value).RunStop(eng, wmd, Model.GoalieID.Value, GoaliPos, (Model.BallState.Location - Model.OurRobots[Model.GoalieID.Value].Location).AngleInDegrees);

                }

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(palcment1)))
                {

                    Planner.ChangeDefaulteParams(Defender1ID.Value, false);
                    Planner.SetParameter(Defender1ID.Value, 1.2);
                    Functions[Defender1ID.Value] = (eng, wmd) => GetRole<palcment1>(Defender1ID.Value).RunStop(eng, wmd, Defender1ID.Value, Def1Pos, d1teta);

                }

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender2ID, typeof(palcment2)))
                {
                    Planner.ChangeDefaulteParams(Defender2ID.Value, false);
                    Planner.SetParameter(Defender2ID.Value, 1.2);
                    Functions[Defender2ID.Value] = (eng, wmd) => GetRole<palcment2>(Defender2ID.Value).RunStop(eng, wmd, Defender2ID.Value, Def2Pos, d2teta);

                }
            }
            else
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieStopRole)))
                    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieStopRole>(Model.GoalieID.Value).PositioningStop(eng, wmd, Model.GoalieID.Value);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender1ID, typeof(DefenderStopRole1)))
                    Functions[Defender1ID.Value] = (eng, wmd) => GetRole<DefenderStopRole1>(Defender1ID.Value).PositioningStop(engine, Model, Defender1ID.Value, true, 150);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Defender2ID, typeof(DefenderStopRole2)))
                    Functions[Defender2ID.Value] = (eng, wmd) => GetRole<DefenderStopRole2>(Defender2ID.Value).PositioningStop(engine, Model, Defender2ID.Value, true, 150);
            }
            if (palcment3.targetOverLap3.DistanceFrom(palcment4.targetOverLap4) < 0.12)
            {
                vec1 = palcment3.targetOverLap3 - palcment4.targetOverLap4;
                jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.12) + palcment4.targetOverLap4);
                palcment4.targetOverLap4 = jj;
                DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Pink, 0.01f)), "targetoverlap");
            }
            //
            if (palcment3.targetOverLap3.DistanceFrom(palcment5.targetOverLap5) < 0.12)
            {
                vec1 = palcment3.targetOverLap3 - palcment5.targetOverLap5;
                jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.12) + palcment5.targetOverLap5);
                palcment5.targetOverLap5 = jj;
                DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Red, 0.01f)), "targetoverlap");
            }
            if (palcment4.targetOverLap4.DistanceFrom(palcment5.targetOverLap5) < 0.12)
            {
                vec1 = palcment4.targetOverLap4 - palcment5.targetOverLap5;
                jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.12) + palcment5.targetOverLap5);
                palcment5.targetOverLap5 = jj;
                DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
            }
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop1, typeof(palcment3)))
                Functions[stop1.Value] = (eng, wmd) => GetRole<palcment3>(stop1.Value).RunRoleStop(eng, wmd, stop1.Value, palcment3.targetOverLap3);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop2, typeof(palcment4)))
                Functions[stop2.Value] = (eng, wmd) => GetRole<palcment4>(stop2.Value).RunRoleStop(eng, wmd, stop2.Value, palcment4.targetOverLap4);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop3, typeof(palcment5)))
                Functions[stop3.Value] = (eng, wmd) => GetRole<palcment5>(stop3.Value).RunRoleStop(eng, wmd, stop3.Value);


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

            //StaticVariables.ballPlacementPos=Position2D.Zero;
            CurrentState = (int)State.id1;
            //if (catcherID.HasValue)
            //    GetRole<BallPalcementCatcher>(placerID.Value).Reset();
        }
        enum State
        {
            id1,
            positionning,
            pass,
            id2,
            eatBall,
            moveToBalll,
            fainal
        }
    }
}
