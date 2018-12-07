using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.AIConsole.Skills.GoalieSkills;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Strategies;
using System.Drawing;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.AIConsole.Skills;
namespace MRL.SSL.AIConsole.Plays.TestPlays
{
    public class GoaliTestPlay : PlayBase
    {
        bool isGo = false;
        bool isFirst = true;
        int catcherId = 0;
        int passerId = 0;

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            if (Status == GameStatus.TestOffend)
            {
                return true;
            }
            else
            {
                isFirst = true;
                isGo = false;
                return false;
            }
        }


        PreDefinedPath Skill = new PreDefinedPath();
        CircularMotionSkill circleSkill = new CircularMotionSkill();
        private void findPoints(out List<Position2D> squarePoints, Position2D center, double lenght)
        {
            squarePoints = new List<Position2D>();
            squarePoints.Add(new Position2D(center.X + lenght, center.Y + lenght));
            squarePoints.Add(new Position2D(center.X + lenght, center.Y - lenght));
            squarePoints.Add(new Position2D(center.X - lenght, center.Y - lenght));
            squarePoints.Add(new Position2D(center.X - lenght, center.Y + lenght));
        }
        int i = 0;

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            int robotId = 9;

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, robotId, typeof(TestRole)))
                Functions[robotId] = (eng, wmd) => GetRole<TestRole>(robotId).GetData(Model, robotId, 0.5, 30);


            //for (int i = 0; i < 180; i++)
            //{
            //    Line l = new Line(new Position2D(5.5, 2.2), new Position2D(5.5, -2.5));
            //    var poses = GameParameters.LineIntersectWithOurDangerZone(l); 
            //    DrawingObjects.AddObject("line_intersect_line_test", l);
            //    for (int i = 0; i < poses.Count; i++)
            //    {

            //        DrawingObjects.AddObject("line_intersect_test" + i.ToString(),
            //            new Circle(poses[i], 0.02,new Pen(Color.Red, 0.01f)));
            //    }

            //}

            //circleSkill.perform(Model, 2, GameParameters.OppGoalCenter, .2, false);
            //Skill.run(Model);
            //DrawingObjects.AddObject(new StringDraw(Model.BallState.Speed.Size.ToString(), Position2D.Zero), "asds");
            //int id1 = 4;
            //int id2 = 3;
            //if (Model.BallState.Speed.Size > 1)
            //{
            //    isGo = true;
            //}
            //if (isFirst)
            //{
            //    isFirst = false;
            //    if (Model.BallState.Location.DistanceFrom(Model.OurRobots[id1].Location) < Model.BallState.Location.DistanceFrom(Model.OurRobots[id2].Location))
            //    {
            //        passerId = id1;
            //        catcherId = id2;
            //    }
            //    else
            //    {
            //        passerId = id2;
            //        catcherId = id1;
            //    }
            //}

            //if (!isGo)
            //{
            //    Vector2D vec = Model.BallState.Location - Model.OurRobots[catcherId].Location;
            //    //Planner.AddRotate(Model, passerId, Model.OurRobots[catcherId].Location, 0, kickPowerType.Speed, 5, false);
            //    Planner.Add(catcherId, Model.OurRobots[catcherId].Location, vec.AngleInDegrees, PathType.UnSafe, true, true, true, true);
            //}
            //else
            //{
            //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, catcherId, typeof(TestRole)))
            //        Functions[catcherId] = (eng, wmd) => GetRole<TestRole>(catcherId).CatchTest(eng, wmd, catcherId);
            //    //Planner.Add(passerId, Model.OurRobots[passerId].Location, Model.OurRobots[passerId].Angle.Value, PathType.UnSafe, true, true, true, true);
            //}

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, passerId, typeof(TestRole)))
            //    Functions[passerId] = (eng, wmd) => GetRole<TestRole>(passerId).RotateSpin(eng, wmd, passerId, Model.OurRobots[catcherId].Location, 5, true);
            //Planner.AddKick(catcherId, true);

            ////if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, id, typeof(TestRole)))
            ////    Functions[id] = (eng, wmd) => GetRole<TestRole>(id).GetData(Model, id, 1, 30);

            ////Planner.GetMotionTime(Model, 9, Position2D.Zero, new Position2D(1, 0), ActiveParameters.RobotMotionCoefs);

            //SingleWirelessCommand SWC = new SingleWirelessCommand();

            //SWC.Vy = 0.1;
            //SWC.RobotID = id;
            //SWC.KickSpeed = 1;

            //Planner.Add(id, SWC);

            //int RobotID = 0;
            //Vector2D ballSpeed =( new Position2D(2.72, .51) - new Position2D(2.13, -.13)).GetNormalizeToCopy(1.33);
            //double v = Vector2D.AngleBetweenInRadians(ballSpeed, (new Position2D(2.73 , 0.04) -new Position2D(2.13, -.13)));
            //double maxIncomming = 1.5, maxVertical = 11, maxOutGoing = 1;
            //double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
            //    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
            //    + maxVertical;
            //double maxSpeedToGet = 0.5;
            //double dist, dist2;
            //double margin = 0.1;
            //double distToBall = .64;
            //if (distToBall == 0)
            //    distToBall = 0.5;
            //double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);

            //Position2D ballPos = Model.BallState.Location;
            //Line robotLine = new Line(Model.OurRobots[id].Location, GameParameters.OppGoalCenter);
            //Line prepLine = robotLine.PerpenducilarLineToPoint(Model.BallState.Location);
            //prepLine.DrawPen = new Pen(Color.Black, 0.01f);

            //DrawingObjects.AddObject(new Circle(prepLine.Head, 0.1, new Pen(Color.Red, 0.01f)), "dsfdfsf");
            //DrawingObjects.AddObject(new Circle(prepLine.Tail, 0.1, new Pen(Color.Black, 0.01f)), "dsfdfsdfvsf");
            //DrawingObjects.AddObject(prepLine, "adtrhssd");
            //DrawingObjects.AddObject(robotLine, "asdfsdssd");

            //if (staticroleassigner.assignrole(engine, model, previouslyassignedroles, currentlyassignedroles, id, typeof(testrole)))
            //    functions[id] = (eng, wmd) => GetRole<TestRole>(id).CutBall(wmd, id, GameParameters.OppGoalCenter);

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, id, typeof(OneTouchTestRole)))
            //    Functions[id] = (eng, wmd) => GetRole<OneTouchTestRole>(id).Perform(eng, wmd, id, GameParameters.OppGoalCenter);

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, id, typeof(NewCutBallTestRole)))
            //    Functions[id] = (eng, wmd) => GetRole<NewCutBallTestRole>(id).Perform(eng, wmd, id);

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, id, typeof(OneTouchRole)))
            //    Functions[id] = (eng, wmd) => GetRole<OneTouchRole>(id).PerformNew(eng, wmd, id, new SingleObjectState(), false, GameParameters.OppGoalCenter, 200, false);

            //DrawingObjects.AddObject(new Line(GameParameters.OppGoalCenter, GameParameters.OurGoalCenter), "sfdsfds");
            //Planner.AddRotate(Model, id, new Position2D(-Model.BallState.Location.X, Model.BallState.Location.Y), 0, kickPowerType.Speed, 8, false);
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, 2, typeof(TestRole)))
            //    Functions[2] = (eng, wmd) => GetRole<TestRole>(2).GetData(wmd, 2, 0.5, 30);

            //Planner.AddRotate(Model, ControlParameters.GoalieID, GameParameters.OppGoalCenter, 0, kickPowerType.Speed, 3, false);
            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, 8, typeof(TestRole)))
            //    Functions[8] = (eng, wmd) => GetRole<TestRole>(8).GetData(Model, 8, 0.5, 30);

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;

        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            throw new NotImplementedException();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            //GetRole<NewCutBallTestRole>(5).Reset();
        }

        enum state
        {

        }
    }
}
