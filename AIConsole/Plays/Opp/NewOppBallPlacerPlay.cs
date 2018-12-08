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
        }
        bool FirstBool = false;
        bool FirstBall = true;
        Circle ballToAvoid = new Circle();
        Circle targetToAvoid = new Circle();
        Line lineToAvoid = new Line();
        Line line1 = new Line();
        Line line2 = new Line();
        Vector2D exLine = new Vector2D();
        Position2D pointHead = new Position2D();
        Position2D pointTail = new Position2D();
        Dictionary<int, Position2D> noneRobotTargets = new Dictionary<int, Position2D>();
        List<Position2D> ballConf = new List<Position2D>();
        List<Position2D> targetConf = new List<Position2D>();
        
        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            Position2D FirstBallPos = new Position2D();

            if (FirstBall)
            {
                if (Model.FieldIsInverted)
                    StaticVariables.ballPlacementPos = new Position2D(-StaticVariables.ballPlacementPos.X / 1000, StaticVariables.ballPlacementPos.Y / 1000);
                else
                    StaticVariables.ballPlacementPos = new Position2D(StaticVariables.ballPlacementPos.X / 1000, -StaticVariables.ballPlacementPos.Y / 1000);
                FirstBallPos = Model.BallState.Location;
                ballToAvoid = new Circle(Model.BallState.Location, 0.5);
                targetToAvoid = new Circle(StaticVariables.ballPlacementPos, 0.5);
                lineToAvoid = new Line(Model.BallState.Location, StaticVariables.ballPlacementPos);
                Vector2D vec = StaticVariables.ballPlacementPos - Model.BallState.Location;
                Vector2D exVec1 = Vector2D.FromAngleSize(vec.AngleInRadians + Math.PI / 2, 0.5);
                Vector2D exVec2 = Vector2D.FromAngleSize(vec.AngleInRadians - Math.PI / 2, 0.5);
                line1 = new Line(Model.BallState.Location + exVec1, StaticVariables.ballPlacementPos + exVec1);
                line2 = new Line(Model.BallState.Location + exVec2, StaticVariables.ballPlacementPos + exVec2);
                Vector2D ballConfVec1 = (ballToAvoid.Intersect(line1).FirstOrDefault() - ballToAvoid.Center);
                Vector2D ballConfVec2 = (ballToAvoid.Intersect(line2).FirstOrDefault() - ballToAvoid.Center);
                Vector2D targetConfVec1 = (targetToAvoid.Intersect(line1).FirstOrDefault() - targetToAvoid.Center);
                Vector2D targetConfVec2 = (targetToAvoid.Intersect(line2).FirstOrDefault() - targetToAvoid.Center);

                Dictionary<int, SingleObjectState> ours = new Dictionary<int, SingleObjectState>();
                var list = Model.OurRobots.Keys.ToList();
                list.Sort();
                int? goalie = Model.GoalieID;
                list.Remove(goalie.Value);
                foreach (var item in list)
                {
                    ours.Add(item, Model.OurRobots[item]);
                }
                int c1 = 0, c2 = 0, c3 = 0;
                foreach (var item in ours)
                {
                    Position2D intersect = new Position2D();
                    intersect = lineToAvoid.PerpenducilarLineToPoint(item.Value.Location).IntersectWithLine(lineToAvoid).Value;

                    //Vector2D tempVec =  ;
                    //new Line(Position2D.Zero,Position2D.Zero).PerpenducilarLineToPoint()

                    if (!noneRobotTargets.ContainsKey(item.Key))
                    {
                        if (targetToAvoid.IsInCircle(item.Value.Location))
                        {
                            Vector2D extend = Vector2D.FromAngleSize(20 * Math.PI / 180 + 20 * c1 * Math.PI / 180, 0.60);
                            if (true)//Vector2D.IsBetweenWithDirection(targetConfVec1,targetConfVec2,extend))
                            {
                                noneRobotTargets.Add(item.Key, StaticVariables.ballPlacementPos + extend);
                                c1++;
                            }
                        }
                        else if (ballToAvoid.IsInCircle(item.Value.Location))
                        {

                            Vector2D extend = Vector2D.FromAngleSize(-(20 * Math.PI / 180 + 20 * c2 * Math.PI / 180), 0.60);
                            if (true)//Vector2D.IsBetweenWithDirection(ballConfVec1, ballConfVec2, extend))
                            {
                                noneRobotTargets.Add(item.Key, Model.BallState.Location - extend);
                                c2++;
                            }
                        }

                    }
                    exLine = (FirstBallPos - StaticVariables.ballPlacementPos);
                    pointHead = StaticVariables.ballPlacementPos + exLine.GetNormalizeToCopy(0.5);
                    pointTail = FirstBallPos - exLine.GetNormalizeToCopy(0.5);



                    if (intersect.DistanceFrom(item.Value.Location) < 0.50 && Position2D.IsBetween(pointHead, pointTail, intersect))
                    {
                        if (noneRobotTargets.ContainsKey(item.Key))
                        {
                            noneRobotTargets[item.Key] = line1.Head + (line1.Tail - line1.Head).GetNormalizeToCopy(1 + 0.18 * c3);//(intersect - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.50);
                            c3++;
                        }
                        else
                        {
                            noneRobotTargets.Add(item.Key, line1.Head + (line1.Tail - line1.Head).GetNormalizeToCopy(1 + 0.18 * c3));//(intersect - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.50);
                            c3++;
                        }
                        //else
                        //    noneRobotTargets.Add(item.Key, intersect + (intersect - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.50));
                    }
                }
                FirstBall = false;

            }
                
                //foreach (var item in noneRobotTargets)
                //{
                //    foreach (var ourRobot in ours)
                //    {
                //        double minDist = double.MaxValue;
                //        if ( minDist > item.Value.DistanceFrom(ourRobot.Value.Location))
                //        {
                //            minDist = item.Value.DistanceFrom(ourRobot.Value.Location);
                //            if (minDist < 0.20)
                //            {

                //            }
                //        }

                //    }

                //}
                
            Position2D Target = new Position2D();
            double firstRadi = 0.5, finalRadi = 0.1;
            int k = 0;
            //noneRobotTargets.Clear();


            line1.DrawPen = new Pen(Color.Red, 0.02f);
            line2.DrawPen = new Pen(Color.Blue, 0.02f);
            DrawingObjects.AddObject(line1);
            DrawingObjects.AddObject(line2);
            DrawingObjects.AddObject(new Circle(Target, 0.01, new Pen(Color.Pink, 0.01f)), "ballcirclp");

            //for (int j = 0; j < robotCount; j++)
            //{
            //    int item = list[j];
            //    Planner.ChangeDefaulteParams(item, false);
            //    Planner.SetParameter(item, 2, 1.5);
            //}
            foreach (var item in Model.OurRobots.Keys)
            {
                var counter = 0;
                if (noneRobotTargets.ContainsKey(item))
                {


                    int index = item; // Warning: Very Important to use "item" like here
                    //Vector2D vec = Vector2D.FromAngleSize((GameParameters.OppGoalCenter - StaticVariables.ballPlacementPos).AngleInRadians + counter * 25 * Math.PI / 180, 0.7);
                    //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, index, typeof(GotoPointRole)))
                    //    Functions[index] = (eng, wmd) => GetRole<GotoPointRole>(index).GotoPoint(Model, item, noneRobotTargets[item], (GameParameters.OurGoalCenter - Model.OurRobots[item].Location).AngleInDegrees,true,true);
                    Planner.ChangeDefaulteParams(item, false);
                    Planner.SetParameter(item, 2, 1.5);
                    Planner.Add(item, noneRobotTargets[item], (-(GameParameters.OurGoalCenter - Model.OurRobots[item].Location)).AngleInDegrees, PathType.UnSafe, true, true, false, false, false);
                    DrawingObjects.AddObject(noneRobotTargets[item]);
                }
            }

            DrawingObjects.AddObject(new Circle(pointHead, 0.09, new Pen(Color.Blue, 0.01f)));
            DrawingObjects.AddObject(new Circle(pointTail, 0.09, new Pen(Color.Blue, 0.01f)));

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
            FirstBall = true;
            ballToAvoid = new Circle();
            targetToAvoid = new Circle();
            lineToAvoid = new Line();

            noneRobotTargets = new Dictionary<int, Position2D>();
        }
    }
}
