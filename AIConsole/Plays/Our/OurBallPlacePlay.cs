using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Plays.Opp
{
    class OurBallPlacePlay : PlayBase
    {

        int? placerID = null;
        int? catcherID = null;
        bool firstFlag = true;
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
        Position2D firstBallPos = new Position2D();
        bool catchBool = true;
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            //return false;
            if (Status == GameDefinitions.GameStatus.BallPlace_OurTeam)
                return true;
            firstFlag = true;
            return false;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            //DrawingObjects.AddObject(new StringDraw(StaticVariables.ballPlacementPos.toString(),GameParameters.OppGoalCenter.Extend(0.5,0)), "ballcircle");




            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.04, new Pen(Color.Orange, 0.01f)), "ballcircle");
            DrawingObjects.AddObject(new Circle(StaticVariables.ballPlacementPos, 0.5, new Pen(Color.GreenYellow, 0.01f), true, 0.1f, false), "ballcigfdsrcle");



            if (firstFlag)
            {
                //if (Model.FieldIsInverted)
                //    StaticVariables.ballPlacementPos = new Position2D(-StaticVariables.ballPlacementPos.X / 1000, StaticVariables.ballPlacementPos.Y / 1000);
                //else
                //    StaticVariables.ballPlacementPos = new Position2D(StaticVariables.ballPlacementPos.X / 1000, -StaticVariables.ballPlacementPos.Y / 1000); 
                placerID = null;
                catcherID = null;
                double min = double.MaxValue;
                foreach (var item in Model.OurRobots.Keys)
                {
                    if (!(Model.GoalieID.HasValue && item == Model.GoalieID.Value))
                    {
                        if (Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < min)
                        {
                            min = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                            placerID = item;
                        }
                    }
                }
                min = double.MaxValue;
                foreach (var item in Model.OurRobots.Keys)
                {
                    if (!(Model.GoalieID.HasValue && item == Model.GoalieID.Value) && !(placerID.HasValue && item == placerID.Value))
                    {
                        if (Model.OurRobots[item].Location.DistanceFrom(StaticVariables.ballPlacementPos) < min)
                        {
                            min = Model.OurRobots[item].Location.DistanceFrom(StaticVariables.ballPlacementPos);
                            catcherID = item;
                        }
                    }
                }
                firstBallPos = Model.BallState.Location;


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
                list.Remove(catcherID.Value);
                list.Remove(placerID.Value);
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


                    exLine = (firstBallPos - StaticVariables.ballPlacementPos);
                    pointHead = StaticVariables.ballPlacementPos + exLine.GetNormalizeToCopy(0.5);
                    pointTail = firstBallPos - exLine.GetNormalizeToCopy(0.5);



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
                firstFlag = false;
            }
            if (Model.BallState.Speed.Size < 0.05)
            {
                firstBallPos = Model.BallState.Location;
            }
            if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.5)//Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.3 && Model.BallState.Speed.Size < 0.1 && catcherID.HasValue && placerID.HasValue)//&& Model.BallState.Location.DistanceFrom(firstBallPos) > 0.08 )
            {
                
                Planner.ChangeDefaulteParams(placerID.Value, false);
                Planner.SetParameter(placerID.Value, 3, 1.5);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, placerID, typeof(BallPlacerRole)))
                    Functions[placerID.Value] = (eng, wmd) => GetRole<BallPlacerRole>(placerID.Value).Perform(eng, wmd, placerID.Value, catcherID.Value, BallPlacerRole.PlacementModes.Pass);
                
            }
            if ((Model.BallState.Speed.Size > 0.09 && 
                Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.5)  || Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > 0.5)
            {
                if (Model.BallState.Location.Size < 0.09 && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < 0.5)
                {
                    catchBool = false;
                }
                Planner.ChangeDefaulteParams(catcherID.Value, false);
                Planner.SetParameter(catcherID.Value, 3, 1.5);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, catcherID, typeof(BallPalcementCatcher)))
                    Functions[catcherID.Value] = (eng, wmd) => GetRole<BallPalcementCatcher>(catcherID.Value).Perform(engine, Model, catcherID.Value);
            }

            List<int> tempDick = new List<int>(); 

            tempDick.Add(catcherID.Value);
            tempDick.Add(placerID.Value);
            //foreach (var item in Model.OurRobots.Keys)
            //{
            //    var counter = 0;
            //    if (tempDick.Contains(item) && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < .52 && Model.BallState.Speed.Size < 0.1
            //        )
            //    {
            //        Planner.ChangeDefaulteParams(item, false);
            //        Planner.SetParameter(item, 1);
            //        counter++;
            //        int index = item; // Warning: Very Important to use "item" like here
            //        Vector2D vec = Vector2D.FromAngleSize((GameParameters.OppGoalCenter - StaticVariables.ballPlacementPos).AngleInRadians + counter * 25 * Math.PI / 180, 0.7);
            //        Planner.Add(item, StaticVariables.ballPlacementPos + vec, (Model.BallState.Location - GameParameters.OppGoalCenter).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                    
                    
            //        //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, index, typeof(HaltRole)))
            //        //    Functions[index] = (eng, wmd) => GetRole<HaltRole>(index).Halt(Model, index);
            //    }
            //}
            if (Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) > .10 && Model.BallState.Location.DistanceFrom(StaticVariables.ballPlacementPos) < .5 && Model.BallState.Speed.Size < 0.07)
            {
                Planner.ChangeDefaulteParams(placerID.Value, false);
                Planner.SetParameter(placerID.Value, 1);
                int index = placerID.Value; // Warning: Very Important to use "item" like here
                Vector2D vec = Vector2D.FromAngleSize((GameParameters.OppGoalCenter - StaticVariables.ballPlacementPos).AngleInRadians + 1 * Math.PI / 180, 0.7);
                //Vector2D vec = Vector2D.FromAngleSize((GameParameters.OppGoalCenter - StaticVariables.ballPlacementPos).AngleInRadians + 1 * Math.PI / 180, 0.7);
                Vector2D vec2 = Vector2D.FromAngleSize((GameParameters.OppGoalCenter - StaticVariables.ballPlacementPos).AngleInRadians + 25 * Math.PI / 180, 0.7);
                //Planner.Add(index, StaticVariables.ballPlacementPos + vec, (Model.BallState.Location - GameParameters.OppGoalCenter).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, placerID, typeof(BallPlacerRole)))
                    Functions[placerID.Value] = (eng, wmd) => GetRole<BallPlacerRole>(placerID.Value).Perform(eng, wmd, placerID.Value, catcherID.Value, BallPlacerRole.PlacementModes.OneRobot);
                index = catcherID.Value;
                Planner.Add(index, StaticVariables.ballPlacementPos + vec, (Model.BallState.Location - GameParameters.OppGoalCenter).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);            
            }
            else
            {
                Vector2D vec = Vector2D.FromAngleSize((GameParameters.OppGoalCenter - StaticVariables.ballPlacementPos).AngleInRadians + 1 * Math.PI / 180, 0.7);
                Vector2D vec2 = Vector2D.FromAngleSize((GameParameters.OppGoalCenter - StaticVariables.ballPlacementPos).AngleInRadians + 25 * Math.PI / 180, 0.7);

                Planner.Add(placerID.Value, StaticVariables.ballPlacementPos + vec, (Model.BallState.Location - GameParameters.OppGoalCenter).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                Planner.Add(catcherID.Value, StaticVariables.ballPlacementPos + vec2, (Model.BallState.Location - GameParameters.OppGoalCenter).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            }
            foreach (var item in Model.OurRobots.Keys)
            {
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
            if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.10 && Model.BallState.Speed.Size < 0.10)
            {
                if (placerID.HasValue)
                    GetRole<BallPlacerRole>(placerID.Value).Reset();
                if (catcherID.HasValue)
                    GetRole<BallPalcementCatcher>(catcherID.Value).Reset();
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(GameDefinitions.WorldModel Model, GameStrategyEngine engine)
        {
            if (placerID.HasValue)
                GetRole<BallPlacerRole>(placerID.Value).Reset();
            if (catcherID.HasValue)
                GetRole<BallPalcementCatcher>(placerID.Value).Reset();
            firstFlag = true;
            catchBool = true;

        }
    }
}
