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
    class OppBallPalcementPlay : PlayBase
    {
        Position2D target;
        Position2D jj;
        bool t = true;
        bool FirstBall = true;
        Circle ballToAvoid = new Circle();
        Circle targetToAvoid = new Circle();
        Circle ballPlacementPos = new Circle();
        Line lineToAvoid = new Line();
        Line line1 = new Line();
        Line line2 = new Line();
        Line line11 = new Line();
        Line line22 = new Line();
        Line ourGoalBall = new Line();
        double dist1;
        double dist2;
        double distOppGoal, distOurGoal;
        Vector2D vec1 = new Vector2D();
        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            return engine.Status == GameDefinitions.GameStatus.BallPlace_Opponent;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            Position2D FirstBallPos = new Position2D();

            int? goalieID = (engine.GameInfo.OppTeam.GoaliID.HasValue && Model.Opponents.ContainsKey(engine.GameInfo.OppTeam.GoaliID.Value)) ? engine.GameInfo.OppTeam.GoaliID : null;
            Dictionary<int, float> tempOpp;
            if (goalieID.HasValue)
            {
                tempOpp = engine.GameInfo.OppTeam.Scores.Where(w => w.Key != engine.GameInfo.OppTeam.GoaliID.Value).ToDictionary(k => k.Key, v => v.Value);
                tempOpp.OrderByDescending(t => t.Value);
            }
            else
            {
                tempOpp = engine.GameInfo.OppTeam.Scores;
                tempOpp.OrderByDescending(t => t.Value);
            }
            //List<int> oppAttackerIds = new List<int>();
            //oppAttackerIds = new List<int>();
            //for (int i = 0; i < tempOpp.Count; i++)
            //{
            //    oppAttackerIds.Add(tempOpp.ElementAt(i).Key);
            //}
            if (true)
            {
                FirstBallPos = Model.BallState.Location;
                ballToAvoid = new Circle(Model.BallState.Location, 0.5);
                targetToAvoid = new Circle(StaticVariables.ballPlacementPos, 0.5);
                lineToAvoid = new Line(Model.BallState.Location, StaticVariables.ballPlacementPos);

                Vector2D ballConfVec1 = (ballToAvoid.Intersect(line1).FirstOrDefault() - ballToAvoid.Center);
                Vector2D ballConfVec2 = (ballToAvoid.Intersect(line2).FirstOrDefault() - ballToAvoid.Center);
                Vector2D targetConfVec1 = (targetToAvoid.Intersect(line1).FirstOrDefault() - targetToAvoid.Center);
                Vector2D targetConfVec2 = (targetToAvoid.Intersect(line2).FirstOrDefault() - targetToAvoid.Center);
                ballPlacementPos = new Circle(StaticVariables.ballPlacementPos, 1);
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
                #region Stop
                NormalDefenceAssigner def = new Engine.NormalDefenceAssigner();
                Dictionary<RoleBase, Position2D?> Positions = new Dictionary<RoleBase, Position2D?>();
                Dictionary<RoleBase, double> Angles = new Dictionary<RoleBase, double>();
                bool isInDangerZone = false;
                double d, d2;
                RoleBase rt;
                List<RoleInfo> roles = new List<RoleInfo>();
                if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.2, out d, out d2))
                    isInDangerZone = true;
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

                rt = typeof(palcment6).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));

                rt = typeof(palcment7).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
                roles.Add(new RoleInfo(rt, 1, 0));

                Dictionary<int, RoleBase> matched;

                //if (Model.GoalieID.HasValue)
                //    matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
                //else
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

                int? stop4 = null;
                if (matched.Any(w => w.Value.GetType() == typeof(palcment6)))
                    stop4 = matched.Where(w => w.Value.GetType() == typeof(palcment6)).First().Key;

                int? stop5 = null;
                if (matched.Any(w => w.Value.GetType() == typeof(palcment7)))
                    stop5 = matched.Where(w => w.Value.GetType() == typeof(palcment7)).First().Key;

                if (!isInDangerZone)
                {
                    Position2D GoaliPos = new Position2D();
                    double gteta = 0;
                    if (Positions.Any(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)))
                        GoaliPos = Positions.Where(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)).First().Value.Value;
                    if (Angles.Any(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)))
                        gteta = Angles.Where(w => w.Key.GetType() == typeof(GoaliBallPalcmentRole)).First().Value;

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

                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoaliBallPalcmentRole)))
                        Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoaliBallPalcmentRole>(Model.GoalieID.Value).RunStop(eng, wmd, Model.GoalieID.Value, GoaliPos, (Model.BallState.Location - Model.OurRobots[Model.GoalieID.Value].Location).AngleInDegrees);

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
                if (palcment3.targetOverLap3.DistanceFrom(palcment4.targetOverLap4) < 0.18)
                {
                    vec1 = palcment3.targetOverLap3 - palcment4.targetOverLap4;
                    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment4.targetOverLap4);
                    palcment4.targetOverLap4 = jj;
                    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Pink, 0.01f)), "targetoverlap");
                }
                //
                if (palcment3.targetOverLap3.DistanceFrom(palcment5.targetOverLap5) < 0.18)
                {
                    vec1 = palcment3.targetOverLap3 - palcment5.targetOverLap5;
                    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment5.targetOverLap5);
                    palcment5.targetOverLap5 = jj;
                    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Red, 0.01f)), "targetoverlap");
                }
                if (palcment4.targetOverLap4.DistanceFrom(palcment5.targetOverLap5) < 0.18)
                {
                    vec1 = palcment4.targetOverLap4 - palcment5.targetOverLap5;
                    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment5.targetOverLap5);
                    palcment5.targetOverLap5 = jj;
                    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
                }
                //
                //if (palcment3.targetOverLap3.DistanceFrom(palcment6.targetOverLap6) < 0.18)
                //{
                //    vec1 = palcment3.targetOverLap3 - palcment6.targetOverLap6;
                //    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment6.targetOverLap6);
                //    palcment6.targetOverLap6 = jj;
                //    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
                //}
                //if (palcment4.targetOverLap4.DistanceFrom(palcment6.targetOverLap6) < 0.18)
                //{
                //    vec1 = palcment4.targetOverLap4 - palcment6.targetOverLap6;
                //    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment6.targetOverLap6);
                //    palcment6.targetOverLap6 = jj;
                //    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
                //}
                //if (palcment5.targetOverLap5.DistanceFrom(palcment6.targetOverLap6) < 0.18)
                //{
                //    vec1 = palcment5.targetOverLap5 - palcment6.targetOverLap6;
                //    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment6.targetOverLap6);
                //    palcment6.targetOverLap6 = jj;
                //    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
                //}
                ////
                //if (palcment3.targetOverLap3.DistanceFrom(palcment7.targetOverLap7) < 0.18)
                //{
                //    vec1 = palcment3.targetOverLap3 - palcment7.targetOverLap7;
                //    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment7.targetOverLap7);
                //    palcment7.targetOverLap7 = jj;
                //    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
                //}
                //if (palcment4.targetOverLap4.DistanceFrom(palcment7.targetOverLap7) < 0.18)
                //{
                //    vec1 = palcment4.targetOverLap4 - palcment7.targetOverLap7;
                //    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment7.targetOverLap7);
                //    palcment7.targetOverLap7 = jj;
                //    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
                //}
                //if (palcment5.targetOverLap5.DistanceFrom(palcment7.targetOverLap7) < 0.18)
                //{
                //    vec1 = palcment5.targetOverLap5 - palcment7.targetOverLap7;
                //    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment7.targetOverLap7);
                //    palcment7.targetOverLap7 = jj;
                //    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
                //}
                //if (palcment6.targetOverLap6.DistanceFrom(palcment7.targetOverLap7) < 0.18)
                //{
                //    vec1 = palcment6.targetOverLap6 - palcment7.targetOverLap7;
                //    jj = (vec1.GetNormalizeToCopy(vec1.Size + 0.2) + palcment7.targetOverLap7);
                //    palcment7.targetOverLap7 = jj;
                //    DrawingObjects.AddObject(new Circle(jj, 0.04, new Pen(Color.Black, 0.01f)), "targetoverlap");
                //}
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop1, typeof(palcment3)))
                    Functions[stop1.Value] = (eng, wmd) => GetRole<palcment3>(stop1.Value).RunRoleStop(eng, wmd, stop1.Value,palcment3.targetOverLap3);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop2, typeof(palcment4)))
                    Functions[stop2.Value] = (eng, wmd) => GetRole<palcment4>(stop2.Value).RunRoleStop(eng, wmd, stop2.Value,palcment4.targetOverLap4);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop3, typeof(palcment5)))
                    Functions[stop3.Value] = (eng, wmd) => GetRole<palcment5>(stop3.Value).RunRoleStop(eng, wmd, stop3.Value);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop4, typeof(palcment6)))
                    Functions[stop4.Value] = (eng, wmd) => GetRole<palcment6>(stop4.Value).RunRoleStop(eng, wmd, stop4.Value);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, stop5, typeof(palcment7)))
                    Functions[stop5.Value] = (eng, wmd) => GetRole<palcment7>(stop5.Value).RunRoleStop(eng, wmd, stop5.Value);


                #endregion
                #region Ballpalcement
              
                #endregion
            }

            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.04, new Pen(Color.Orange, 0.01f)), "ballcircle");
           
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            return null;
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            target = new Position2D();
        }
    }
}
