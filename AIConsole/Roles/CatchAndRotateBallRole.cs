using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    public class CatchAndRotateBallRole : RoleBase
    {
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        bool isChip = false;
        bool kiick = false;
        int counter = 0;
        bool goActive = false;
        bool passed = false;
        bool goShoot = false;
        Position2D ShootTarget = Position2D.Zero;
        public bool GoShoot
        {
            get { return goShoot; }
            set
            {
                goShoot = value;

            }
        }
        Rotate rotate = new Rotate();

        public bool Kiick
        {
            get { return kiick; }
        }
        bool accurated = false;
        public void CatchAndRotate(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, bool passIsChip, bool KickIsChip, bool SpinBack, double KickSpeed, bool determinedNextSt = false, int RotateDelay = 60)
        {
            CatchAndRotate(engine, Model, RobotID, Target, passIsChip, KickIsChip, SpinBack, true, KickSpeed, determinedNextSt, RotateDelay);
        }
        public void CatchAndRotate(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, bool passIsChip, bool KickIsChip, bool SpinBack, bool gotoPoint, double KickSpeed, bool determinedNextSt = false, int RotateDelay = 60)
        {
            CatchAndRotate(engine, Model, RobotID, Model.OurRobots[RobotID], Target, passIsChip, KickIsChip, SpinBack, gotoPoint, KickSpeed, determinedNextSt, RotateDelay);
        }
        public void CatchAndRotate(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState RobotState, Position2D Target, bool passIsChip, bool KickIsChip, bool SpinBack, bool gotoPoint, double KickSpeed, bool determinedNextSt = false, int RotateDelay = 60)
        {
            CatchAndRotate(engine, Model, RobotID, RobotState, Target, passIsChip, KickIsChip, kickPowerType.Speed, SpinBack, gotoPoint, KickSpeed, determinedNextSt, RotateDelay);
        }
        public void CatchAndRotate(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState RobotState, Position2D Target, bool passIsChip, bool KickIsChip, kickPowerType ktype, bool SpinBack, bool gotoPoint, double KickSpeed, bool determinedNextSt = false, int RotateDelay = 60)
        {
            ShootTarget = Target;
            if (determinedNextSt)
                DetermineNextState(engine, Model, RobotID, new Dictionary<int, RoleBase>());
            // kiick = false;
            if (CurrentState == (int)State.Catch)
            {
                kiick = false;
                accurated = false;
                GetSkill<CatchBallSkill>().Catch(engine, Model, RobotID, passIsChip, RobotState, SpinBack, gotoPoint);
            }
            else if (CurrentState == (int)State.Rotate)
            {
                Vector2D refr = Target - Model.BallState.Location;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Location - Model.OurRobots[RobotID].Location, refr);
                if (v.Y > 0.09)
                    goActive = true;
                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) > 0.25 || goActive)
                {
                    double bball = (goShoot) ? 0.1 : 0.16;
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, Target, goShoot, bball);
                    Planner.AddKick(RobotID, true);
                    DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, 1));
                }
                else
                {
                    var SWC = rotate.WithPID(Model, new SingleObjectState(Model.BallState.Location + 0.1 * Model.BallState.Speed, Model.BallState.Speed, 0), -0.13, Target, RobotID, false, 0, kickPowerType.Speed, false, false, 100, 100, 400, true, 0.07, 0.05);
                    SWC.SpinBack = 100;
                    Planner.Add(RobotID, SWC, false);
                }
                // Planner.AddKick(RobotID, SpinBack);
                if (accurated || !accurated && Math.Abs(Vector2D.AngleBetweenInDegrees((Target - Model.BallState.Location), Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1))) < 4)
                {
                    if (goShoot)
                    {
                        Planner.AddKick(RobotID, ktype, KickSpeed, KickIsChip, false);
                        Planner.AddBackSensor(RobotID, false);
                        accurated = true;
                    }
                }
                if (accurated && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.12)
                    kiick = true;
            }
            else if (CurrentState == (int)State.Stop)
            {
                Stop(Model, RobotID);
            }
            DrawingObjects.AddObject(new StringDraw(kiick.ToString(), Position2D.Zero + new Vector2D(-0.7, 0)));
        }
        public void Stop(WorldModel Model, int RobotID)
        {
            SingleWirelessCommand s = new SingleWirelessCommand();
            s.W = 0;
            Vector2D prev = Vector2D.Zero;
            if (Model.lastVelocity.ContainsKey(RobotID))
                prev = GameParameters.RotateCoordinates(Model.lastVelocity[RobotID], Model.OurRobots[RobotID].Angle.Value);
            s.Vx = prev.X / 1.07;
            s.Vy = prev.Y / 1.07;
            Planner.Add(RobotID, s, false);
        }

        Position2D lastRobot = Position2D.Zero;

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            int test = 0;
            double t = (Model.BallState.Speed.Size > 0.07) ? Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) / Model.BallState.Speed.Size : 0;
            if (CurrentState == (int)State.Catch)
            {
                DrawingObjects.AddObject(new StringDraw("ballRobotDis" + Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location), new Position2D(-3, 1)), "balldist");
                DrawingObjects.AddObject(new StringDraw("ballSpeed" + Model.BallState.Speed.Size, new Position2D(-3.3, 1)), "balldrrist");
                DrawingObjects.AddObject(new StringDraw("innerproduct" + Model.BallState.Speed.InnerProduct(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1)), new Position2D(-3.6, 1)), "baleeeeldrrist");
                DrawingObjects.AddObject(new StringDraw("Passed" + passed, new Position2D(-4.1, 1)), "balldrrytrewist");

                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 1 && Model.BallState.Speed.Size > 0.07 && Model.BallState.Speed.InnerProduct(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1.5)) >= 0)
                {
                    test = 1;
                    CurrentState = (int)State.Rotate;
                    lastRobot = Model.OurRobots[RobotID].Location;
                }
                else if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.3 && (Model.BallState.Speed.Size > 0.07 && Model.BallState.Speed.Size < 0.2))
                {
                    test = 2;
                    CurrentState = (int)State.Rotate;
                    lastRobot = Model.OurRobots[RobotID].Location;
                }
                else if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.5 && Model.BallState.Speed.Size < 0.07)
                {
                    test = 3;
                    counter++;
                    if (counter >= 4)
                    {
                        CurrentState = (int)State.Rotate;
                        lastRobot = Model.OurRobots[RobotID].Location;
                    }
                }
                else if (passed && (Model.BallState.Speed.Size <= 0.07 || t >= 4))
                {
                    test = 4;
                    counter++;
                    if (counter >= 4)
                    {
                        CurrentState = (int)State.Rotate;
                        lastRobot = Model.OurRobots[RobotID].Location;
                    }
                }
                DrawingObjects.AddObject(new StringDraw("test " + test.ToString(), new Position2D(-3.5, 1)), "balldrriswtdsdtyt");
            }
            else if (CurrentState == (int)State.Rotate)
            {
                counter = 0;
                Vector2D refer = ShootTarget - lastRobot;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refer);
                if (v.Y > 0.4 && Model.BallState.Location.DistanceFrom(lastRobot) > 1 && kiick)
                    CurrentState = (int)State.Stop;
            }
            else if (CurrentState == (int)State.Stop)
            {
                counter = 0;
                if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < 0.45 && Model.BallState.Speed.Size < .1)
                {
                    lastRobot = Model.OurRobots[RobotID].Location;
                    CurrentState = (int)State.Rotate;
                }
            }
            if (Model.BallState.Speed.Size > 0.12)
                passed = true;
            DrawingObjects.AddObject(new StringDraw(((State)CurrentState).ToString(), Position2D.Zero + new Vector2D(-0.5, 0)));

        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public enum State
        {
            Catch = 0,
            Rotate = 1,
            Stop = 2
        }
    }
}
