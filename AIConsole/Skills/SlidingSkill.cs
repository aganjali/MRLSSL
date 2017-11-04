using MRL.SSL.CommonCLasses.MathLibarary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Skills
{
    class SlidingSkill : SkillBase
    {
        Circle oppCircle = null;
        int id = 7;
        int index;
        SingleObjectState sobj;
        bool meet = false;
        List<int> opponent = new List<int>();
        Line rightpositionLine = new Line();
        Line leftpositionLine = new Line();
        Position2D rightPosition = new Position2D();
        Position2D leftPosition = new Position2D();
        bool clockWise = true;
        bool gotoTarget = false;
        bool gotoKick = false;
        int counter = 0;
        double angle = 0;
        bool isfirst = true;
        Obstacles obstacle = new Obstacles();
        Position2D nextTarget = new Position2D();
        Position2D finalTarget = new Position2D();
        Position2D? targetpos = new Position2D();



        public Position2D Target(WorldModel Model, Position2D target, Position2D posToGo, bool isLeft)
        {
            Vector2D centerHeadVec = Vector2D.FromAngleSize((Model.OurRobots[id].Location - GameParameters.OppGoalCenter).AngleInRadians, 0.09);

            Vector2D TargetVec = Vector2D.FromAngleSize(centerHeadVec.AngleInRadians + Math.PI / 2, 0.11);
            if (isLeft)
                TargetVec = Vector2D.FromAngleSize(centerHeadVec.AngleInRadians - Math.PI / 2, 0.11);

            if (obstacle.Meet(GameParameters.OppGoalCenter, posToGo, 0.05, out index))
            {
                posToGo = Target(Model, target, posToGo + TargetVec, isLeft);
                return posToGo;
            }
            else
                return posToGo;


        }
        public void sliding(WorldModel Model, double radius)
        {
            Position2D ballPos = Model.BallState.Location;
            Line ballgoalLine = new Line(Model.BallState.Location, GameParameters.OppGoalCenter);
            Position2D robotPos = Model.OurRobots[id].Location;

            Vector2D centerHeadVec = Vector2D.FromAngleSize((Model.OurRobots[id].Location - GameParameters.OppGoalCenter).AngleInRadians, 0.09);
            Position2D centerHeadPos = Model.OurRobots[id].Location + centerHeadVec;

            Vector2D leftTargetVec = Vector2D.FromAngleSize(centerHeadVec.AngleInRadians - 90, 0.5);
            Position2D targetLeftPos = centerHeadPos + leftTargetVec;
            DrawingObjects.AddObject(targetLeftPos, "qazerfgh");
            Vector2D rightTargetVec = Vector2D.FromAngleSize(centerHeadVec.AngleInRadians + 90, 0.5);
            Position2D targetRightPos = centerHeadPos + rightTargetVec;
            DrawingObjects.AddObject(targetRightPos, "yghbnmh");
            DrawingObjects.AddObject(new Line(Model.BallState.Location, GameParameters.OppGoalCenter), "rdfcvbnm");
            Line targetLine = new Line(targetRightPos, targetLeftPos);

            foreach (var item in Model.Opponents.Keys)
            {
                obstacle.AddRobot(Model.Opponents[item], false, item);
            }

            if (isfirst)
            {
                Planner.Add(id, Model.OurRobots[id].Location + (Model.BallState.Location - Model.OurRobots[id].Location).GetNormalizeToCopy(0.001), (GameParameters.OppGoalCenter - Model.BallState.Location).AngleInDegrees, PathType.Safe, false, true, true, true, true);
                counter++;
            }
            if (counter > 60)
            {
                DrawingObjects.AddObject(new Line(GameParameters.OppGoalCenter, Model.BallState.Location, new Pen(Color.DarkKhaki, 0.01f)), "redxcvbnm");
                if (isfirst)
                {
                    isfirst = false;
                    if (obstacle.Meet(Model.BallState, new SingleObjectState(GameParameters.OppGoalCenter, Vector2D.Zero, 0), 0.1))
                    {
                        meet = true;
                    }
                    else
                    {
                        meet = false;
                    }

                    if (meet)
                    {
                        gotoTarget = true;
                        finalTarget = Model.BallState.Location;
                        Position2D rightTarget = Target(Model, GameParameters.OppGoalCenter, finalTarget, false);
                        finalTarget = Model.BallState.Location;
                        Position2D leftTarget = Target(Model, GameParameters.OppGoalCenter, finalTarget, true);

                        finalTarget = leftTarget;
                        if (Model.BallState.Location.DistanceFrom(rightTarget) < Model.BallState.Location.DistanceFrom(leftTarget))
                            finalTarget = rightTarget;
                    }
                    else
                    {
                        Planner.AddKick(id, kickPowerType.Speed, 180, false, true);
                    }
                }

                if (gotoTarget)
                {
                    Planner.ChangeDefaulteParams(id, false);
                    Planner.SetParameter(id, 15, 15);

                    DrawingObjects.AddObject(new StringDraw("meet:" + meet, new Position2D(2.5, 2.5)), "bhbhb");
                    DrawingObjects.AddObject(new Circle(finalTarget, 0.07, new Pen(Color.CadetBlue, 0.01f)));

                    angle = (GameParameters.OppGoalCenter - finalTarget).AngleInDegrees;
                    Planner.Add(id,finalTarget, angle, PathType.Safe, false, true, true, true, true);
                    DrawingObjects.AddObject(new Line(Model.BallState.Location, GameParameters.OppGoalCenter), "hana");
                    gotoKick = true;
                }
                if (Model.OurRobots[id].Location.DistanceFrom(finalTarget) < 0.05 && gotoKick)
                {
                    Planner.AddKick(id, kickPowerType.Speed, 180, false);
                    DrawingObjects.AddObject(new StringDraw("Kick", Color.Bisque, new Position2D(-2.5, -2.5)), "dfghj,");
                    DrawingObjects.AddObject(finalTarget, "uyhgbnm");
                    gotoTarget = false;
                }

            }
            DrawingObjects.AddObject(new Circle(finalTarget, 0.02, new Pen(Color.Pink, 0.01f)), "asgharHajMamad");
        }
        public void Reset()
        {
            counter = 0;
            gotoKick = false;
            isfirst = true;
            obstacle = new Obstacles();
        }
    }
}


