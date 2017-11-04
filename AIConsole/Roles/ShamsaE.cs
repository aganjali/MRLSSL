using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class ShamsaE : RoleBase
    {
        private static Dictionary<int, Rotate> rotates = new Dictionary<int, Rotate>();
        Position2D Gotopoint = new Position2D();
        enum States
        {
            gotopoint,
            goforball,
            pass,
            go,
            shoot
        }
        public bool go = true;
        private bool first = true;
        private int counter = 0;
        private int confit = 20;
        private bool goball = false;
        private int counter2=0;
        private int counter3;
        Position2D ShootPoint = new Position2D();
        Position2D ShootPoint2 = new Position2D();
        public override RoleCategory QueryCategory()
        {
            throw new NotImplementedException();
        }

        public void RunRole(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {

            if (CurrentState == (int)States.gotopoint)
            {
                GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, GotopointPos(Model, RobotID), (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, false);
            }
            if (CurrentState == (int)States.goforball)
            {
                Line catchline = engine.GameInfo.OurTeam.CatchBallLines[RobotID].First();
                Circle cathcirc = new Circle(GameParameters.OppGoalCenter, 1.5);
                List<Position2D> catchpoints = cathcirc.Intersect(catchline);
                Position2D catchpoint = Position2D.Zero;
                double mindist = double.MaxValue;
                foreach (var item in catchpoints)
                {
                    if (item.DistanceFrom(Model.BallState.Location) < mindist)
                    {
                        mindist = item.DistanceFrom(Model.BallState.Location);
                        catchpoint = item;
                    }
                }
                counter++;
                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) > .5)
                    GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, catchpoint, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, true, false, false, 0, false, false);

                else
                {
                    ShootPoint2 = Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize((22.5 * Math.PI) / 180, .7f);
                    counter++;
                    GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Model.OurRobots[RobotID].Location, (ShootPoint2 - Model.OurRobots[RobotID].Location).AngleInDegrees, true, false, false, 0, false, true);
                }

            }
            
            if (confit == counter)
            {
                counter2++;
            }
            if (CurrentState == (int)States.pass && counter2 >= 10)
            {

                if (first == true)
                {
                    ShootPoint = Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize((22.5 * Math.PI) / 180, .7f);
                    first = false;
                }
                GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, ShootPoint);
                Planner.AddKick(RobotID, kickPowerType.Power, 65, false, true);
            }
            if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .5)
            {
                counter3++;
            }
            FlatRectangle rec = new FlatRectangle(-3,2.5,.5,.5);
            DrawingObjects.AddObject(rec, "rectangle");
            DrawingObjects.AddObject(new StringDraw("Informations", new Position2D(-2.9, 2.75)));
            DrawingObjects.AddObject(new StringDraw(counter.ToString(), new Position2D(-2.8, 2.75)));
            DrawingObjects.AddObject(new StringDraw(counter.ToString(), new Position2D(-2.7, 2.75)));
            DrawingObjects.AddObject(new StringDraw(counter.ToString(), new Position2D(-2.6, 2.75)));
            if (CurrentState == (int)States.go )
            {
                SingleWirelessCommand SWC = GetSkill<rotatedrible>().rotateWFeedbackBig(Model, GameParameters.OppGoalCenter, 180, kickPowerType.Speed, 7, RobotID, false, true);
                Planner.Add(RobotID, SWC);
            }
            DrawingObjects.AddObject(ShootPoint, "d");
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (Model.OurRobots[RobotID].Location.DistanceFrom(GotopointPos(Model, RobotID)) < .01 && torobot(Model, RobotID) && CurrentState == (int)States.gotopoint)
            {
                CurrentState = (int)States.goforball;
                DrawingObjects.AddObject(new StringDraw("goforball", Position2D.Zero), "set");
            }

            if (ballinRobot(Model, RobotID) && CurrentState == (int)States.goforball)
            {
                confit = counter;
                CurrentState = (int)States.pass;
                DrawingObjects.AddObject(new StringDraw("Pass", Position2D.Zero), "set1");
            }
            if (CurrentState == (int)States.pass && go &&  counter3 >25)
            {
                CurrentState = (int)States.go;
                DrawingObjects.AddObject(new StringDraw("Shoot", Position2D.Zero), "set2");
            }
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 1;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        private bool ballinRobot(WorldModel model, int Robotid)
        {
            Position2D ballinrobot = model.OurRobots[Robotid].Location + Vector2D.FromAngleSize((model.OurRobots[Robotid].Angle.Value * Math.PI) / 180, .07);
            if (model.BallState.Location.DistanceFrom(ballinrobot) < .03)
                return true;
            return false;
        }

        private Position2D GotopointPos(WorldModel MOdel, int Robotid)
        {
            Position2D Goto = Position2D.Zero;
            if (MOdel.BallState.Location.Y >= 0)
            {
                Vector2D vecforpos = Vector2D.FromAngleSize((45 * Math.PI) / 180, 1.3f);
                Goto = GameParameters.OppGoalCenter + vecforpos;
            }
            if (MOdel.BallState.Location.Y < 0)
            {
                Vector2D vecforopos = Vector2D.FromAngleSize((-45 * Math.PI) / 180, 1.3f);
                Goto = GameParameters.OppGoalCenter + vecforopos;
            }
            return Goto;
        }

        public bool torobot(WorldModel Model, int RobotID)
        {
            if (Model.BallState.Speed.Size > .5)
                return true;
            return false;
        }

    }
}
