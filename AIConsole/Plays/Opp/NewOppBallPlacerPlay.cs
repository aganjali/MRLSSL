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

namespace MRL.SSL.AIConsole.Plays
{
    class NewOppBallPlacerPlay : PlayBase
    {
        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            return engine.Status == GameDefinitions.GameStatus.BallPlace_Opponent;
            //return false;
        }
        bool FirstBool = false;
        bool FirstBall = true;
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();

            Dictionary<int, SingleObjectState> ours = new Dictionary<int, SingleObjectState>();
            Dictionary<int, Position2D> firstPoses = new Dictionary<int, Position2D>();
            Dictionary<int, Position2D> secondPoses = new Dictionary<int, Position2D>();
            Position2D FirstBallPos = new Position2D();
            Position2D Target = new Position2D();
            double firstRadi = 0.5, secondRadi = 0.2;
            int k = 0;
            var list = Model.OurRobots.Keys.ToList();
            list.Sort();
            int? goalie = Model.GoalieID;
            list.Remove(goalie.Value);
            foreach (var item in list)
            {
                ours.Add(item, Model.OurRobots[item]);
            }
            double robotCount = ours.Count;
            if (FirstBall)
            {
                FirstBallPos = Model.BallState.Location;
                FirstBall = false;
            }
            Position2D BallP = new Position2D(StaticVariables.ballPlacementPos.X, StaticVariables.ballPlacementPos.Y - 0.5);
            Position2D BallP2 = new Position2D(StaticVariables.ballPlacementPos.X, StaticVariables.ballPlacementPos.Y + 0.5);

            Position2D Ball = new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y - 0.5);
            Position2D Ball2 = new Position2D(Model.BallState.Location.X, Model.BallState.Location.Y + 0.5);

            Line first = new Line(BallP, Ball);
            Line second = new Line(BallP2, Ball2);

            Target = new Position2D(-(FirstBallPos.X + 0.7), -(StaticVariables.ballPlacementPos.Y + 0.7));
            DrawingObjects.AddObject(new Circle(Target, 0.01, new Pen(Color.Pink, 0.01f)), "ballcirclp");
            Line Perp = first.PerpenducilarLineToPoint(Target);
            Position2D? intersect = first.IntersectWithLine(Perp);
            if (intersect.HasValue)
            {
                

                DrawingObjects.AddObject(new Circle(intersect.Value, 0.1, new Pen(Color.Blue, 0.01f)), "ballcircl");
                double o = Target.DistanceFrom(intersect.Value);
                if ((o <= 1.5))
                {
                    DrawingObjects.AddObject(new StringDraw("o " + o.ToString(), new Position2D(1.5, -1.5)), "o");
                    Vector2D Vec = (Target - intersect.Value).GetNormalizeToCopy(1.5);
                    Target = Target + Vec;
                    DrawingObjects.AddObject(new Circle(Target, 0.1, new Pen(Color.Aqua, 0.01f)), "ballcirclpa1");
                }
            }
            if (!(GameParameters.IsInField(Target, 0.01)))
            {
                if (Target.X> 5.5)
                {
                    Target = new Position2D(5.3,Target.Y);
                }
               else if (Target.X<-5.5)
                {
                    Target = new Position2D(-5.3, Target.Y);
                }
                if (Target.Y > 4)
                {
                    Target = new Position2D(Target.X, 3.8);
                }
               else if (Target.Y < -4)
                {
                    Target = new Position2D(Target.X,-3.8);
                }
                DrawingObjects.AddObject(new Circle(Target, 0.1, new Pen(Color.Brown, 0.01f)), "ballcirclpa");
            }
            foreach (var i in ours.Keys)
            {
                firstPoses.Add(i, Target + Vector2D.FromAngleSize(k * (2 * Math.PI / robotCount), firstRadi));
                secondPoses.Add(i, Target + Vector2D.FromAngleSize(k * (2 * Math.PI / robotCount), secondRadi));
                k++;
            }
            if (!FirstBool)
            {
                for (int j = 0; j < robotCount; j++)
                {
                    int item = list[j];
                    Planner.ChangeDefaulteParams(item,false);
                    Planner.SetParameter(item, 2, 1.5);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, item, typeof(GotoPointRole)))
                        Functions[item] = (eng, wmd) => GetRole<GotoPointRole>(item).GotoPoint(wmd, item, firstPoses[item], (Target - firstPoses[item]).AngleInDegrees, true, true);
                }
            }

            if (!ours.Any(p => p.Value.Location.DistanceFrom(firstPoses[p.Key]) > 0.01) || FirstBool)
            {
                FirstBool = true;
                for (int j = 0; j < robotCount; j++)
                {
                    int item = list[j];
                    Planner.ChangeDefaulteParams(item, false);
                    Planner.SetParameter(item, 2, 1.5);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, item, typeof(GotoPointRole)))
                        Functions[item] = (eng, wmd) => GetRole<GotoPointRole>(item).GotoPoint(wmd, item, secondPoses[item], (Target - secondPoses[item]).AngleInDegrees, true, true);
                }
            }
            DrawingObjects.AddObject(new Line(BallP, Ball, new Pen(Color.Yellow, 0.01f)), "jfd4873");
            DrawingObjects.AddObject(new Line(BallP2, Ball2, new Pen(Color.Red, 0.01f)), "jfd487");
            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.04, new Pen(Color.Orange, 0.01f)), "ballcircle");
            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.5, new Pen(Color.GreenYellow, 0.01f), true, 0.1f, false), "ballcigfdsrcle");
            DrawingObjects.AddObject(new StringDraw("Target " + Target.ToString(), new Position2D(1, -1)), "TargetRobotBallPlacer");
            DrawingObjects.AddObject(new StringDraw("-(FirstBallPos.X + 1), -(StaticVariables.ballPlacementPos.Y + 1) ", new Position2D(1.2, -1.2)), "TargetRobotBallPlacer1");
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            throw new NotImplementedException();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();
            FirstBool = false;
        }
    }
}
