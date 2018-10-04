using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class MoveRobotRole : RoleBase
    {
        int counter = 0;
        bool first = true;
        Position2D Last_Target = Position2D.Zero;
        Position2D TempTarget = Position2D.Zero;
        bool Changed = false;
        public SingleWirelessCommand MoveToTarget(GameStrategyEngine engine, MRL.SSL.GameDefinitions.WorldModel Model, int RobotID)
        {
            //Position2D pos2Go = engine.GameInfo.OurTeam.CatchBallLines[RobotID].FirstOrDefault().Head;
            //DrawingObjects.AddObject(new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed, new Pen(Color.OrangeRed, 0.03f)));
            //DrawingObjects.AddObject(new Circle(pos2Go, 0.2, new System.Drawing.Pen(Color.LightCyan, 0.02f)));
            //try
            {
                if (Model.OurRobots[RobotID].Location.DistanceFrom(RobotComponentsController.Target) <= 0.1)
                {
                    if (first)
                    {
                        VisualizerConsole.WriteLine(counter.ToString());
                        first = false;
                        counter = 0;
                    }
                }
                else
                {
                    counter++;
                    first = true;
                }
                if (TempTarget != RobotComponentsController.Target)
                    Last_Target = TempTarget;

                TempTarget = RobotComponentsController.Target;
                // GetSkill<GotoPointSkill>().SetController(false);
                //  Planner.SetParameter(RobotID, 1);
                DrawingObjects.AddObject(new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1),
                new Pen(Color.LemonChiffon, 0.01f)));
                Planner.Add(RobotID, new SingleObjectState(RobotComponentsController.Target, Vector2D.Zero, (float)(RobotComponentsController.Target - Last_Target).AngleInDegrees), PathType.UnSafe, true, true, true, true);
                Planner.AddKick(RobotID, true);
                return new SingleWirelessCommand();
                //   return GetSkill<GotoPointSkill>().GotoPoint(engine, Model, RobotID, RobotComponentsController.Target, 180/*(RobotComponentsController.Target - Model.BallState.Location).AngleInDegrees /*RobotComponentsController.Angle*/, false, true, false, 0, 0, 5);
            }
            /*catch
            {
                return new SingleWirelessCommand();
            }*/
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Positioner;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            CurrentState = 0;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 0;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
