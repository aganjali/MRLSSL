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
            rt = typeof(BallPalcementShooter).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 2, 0));
            rt = typeof(BallPalcementCatcher).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(rt, 2, 0));


            //rt = typeof(palcment5).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            //roles.Add(new RoleInfo(rt, 1, 0));


            Dictionary<int, RoleBase> matched;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            int? ShooterID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(BallPalcementShooter)))
                ShooterID = matched.Where(w => w.Value.GetType() == typeof(BallPalcementShooter)).First().Key;


            int? CatcherID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(BallPalcementCatcher)))
                CatcherID = matched.Where(w => w.Value.GetType() == typeof(BallPalcementCatcher)).First().Key;



            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(AvoiderGoalieRole)))
            //    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoaliBallPalcmentRole>(Model.GoalieID.Value).RunStop(eng, wmd, Model.GoalieID.Value, GoaliPos, (Model.BallState.Location - Model.OurRobots[Model.GoalieID.Value].Location).AngleInDegrees);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(BallPalcementShooter)))
                Functions[ShooterID.Value] = (eng, wmd) => GetRole<BallPalcementShooter>(ShooterID.Value).Perform(eng, wmd, ShooterID.Value, CatcherID.Value, 0);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, CatcherID, typeof(BallPalcementCatcher)))
                Functions[CatcherID.Value] = (eng, wmd) => GetRole<BallPalcementCatcher>(CatcherID.Value).Perform(eng, wmd, CatcherID.Value, ShooterID.Value, 1);



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
            PreviouslyAssignedRoles.Clear();
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
