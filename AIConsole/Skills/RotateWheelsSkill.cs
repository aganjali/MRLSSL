using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Diagnostics;
using System.Timers;
using MRL.SSL.GameDefinitions.General_Settings;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Skills
{
    class RotateWheelsSkill : SkillBase
    {
        SingleWirelessCommand SWC;

        private static int MMTimerCounter;
        Stopwatch _stopWatch = new Stopwatch();
        public static double vx = 0, vy = 0, w = 14;
        static double counter = 3100;
        bool init = false;
        public RotateWheelsSkill()
        {
            //    Controller = new MRL.SSL.AIConsole.Control.Controller();
            SWC = new SingleWirelessCommand();
        }
        Position2D initializeTarget = new Position2D(GameParameters.OppGoalLeft.X, GameParameters.OppGoalLeft.Y + 0.15);

        public SingleWirelessCommand Rotate(GameStrategyEngine engine, WorldModel Model, int RobotID, double kickSpeed, SingleObjectState lastoppState)
        {
            //return new SingleWirelessCommand() { Kind = 3, isDelayedKick = true, BackSensor = true, SpinBack = 0, KickSpeed = kickPower, spinBackward = true };
            OppGoallerState state = CalculateGoallerState(engine, Model , lastoppState);

            if (state == OppGoallerState.Left)
                initializeTarget = new Position2D(GameParameters.OppGoalLeft.X, GameParameters.OppGoalLeft.Y + 0.15);
            else
                initializeTarget = new Position2D(GameParameters.OppGoalRight.X, GameParameters.OppGoalRight.Y - 0.15);

            //if(Model.BallState.Location
            int spin = (GameSettings.Default.Tactic["Penalty"] == (int)PenaltyShoter.Corner) ? 1 : 0;
            //int? robotID = OppGoallerId(engine, Model);
            Line l = new Line(GameParameters.OppGoalRight, GameParameters.OppGoalLeft);
            Line l2 = new Line(Model.BallState.Location, Model.OurRobots[RobotID].Location);
            Position2D? intersect = l2.IntersectWithLine(l);
            if (!intersect.HasValue) intersect = GameParameters.OppGoalCenter;
            DrawingObjects.AddObject(intersect.Value, "inter");
            Line ll = new Line(Model.BallState.Location, intersect.Value);
            Obstacle obs = new Obstacle();
            obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
            obs.Type = ObstacleType.OppRobot;
            if (lastoppState != null)
                obs.State = lastoppState;
            else
            {
                obs.State = new SingleObjectState();
            }
            bool isdelayed = true;
            if (intersect.HasValue && lastoppState != null && !obs.Meet(Model.BallState, new SingleObjectState(intersect.Value, Vector2D.Zero, 0), 0.09))
            {
                Planner.Add(RobotID, new SingleWirelessCommand() { Kind = 3, isDelayedKick = isdelayed, SpinBack = spin, statusRequest = true, KickSpeed = Program.MaxKickSpeed });
                return new SingleWirelessCommand();
            }
            else if (lastoppState == null)
            {
                Planner.Add(RobotID, new SingleWirelessCommand() { Kind = 3, isDelayedKick = isdelayed, SpinBack = spin, statusRequest = true, KickSpeed = Program.MaxKickSpeed });
                return new SingleWirelessCommand();
            }
            else if (state == OppGoallerState.Right)
            {
                Planner.Add(RobotID, new SingleWirelessCommand() { Kind = 3, isDelayedKick = isdelayed, SpinBack = spin, spinBackward = true, KickSpeed = Program.MaxKickSpeed });
                return new SingleWirelessCommand();
            }
            else
            {
                Planner.Add(RobotID, new SingleWirelessCommand() { Kind = 3, isDelayedKick = isdelayed, SpinBack = spin, spinBackward = false, KickSpeed = Program.MaxKickSpeed });
                return new SingleWirelessCommand();
            }

            //SWC.isDelayedKick = true;
            //if (CalculateGoallerState(engine, Model) == OppGoallerState.Left)
            //{
            //    SWC.spinBackward = false;
            //    SWC.statusRequest = true;
            //}
            //else
            //{
            //    SWC.spinBackward = true;
            //}

            //SWC.KickPower = kickPower;
            SWC.KickSpeed = kickSpeed;
            Planner.Add(RobotID, SWC);
            return new SingleWirelessCommand();
        }
        //private int? OppGoallerId(GameStrategyEngine engine, WorldModel Model)
        //{

        //    double MinDist = 10;
        //    int? OppGoallerId = null;
        //    foreach (int OppID in Model.Opponents.Keys)
        //    {
        //        if ((GameParameters.OppGoalCenter - Model.Opponents[OppID].Location).Size < MinDist)
        //        {
        //            MinDist = (GameParameters.OppGoalCenter - Model.Opponents[OppID].Location).Size;
        //            OppGoallerId = OppID;
        //        }
        //    }
        //    return OppGoallerId;
        //}
        private OppGoallerState CalculateGoallerState(GameStrategyEngine engine, WorldModel Model, SingleObjectState lastoppstate)
        {
            Random r = new Random();
            int v = r.Next((int)OppGoallerState.Left, (int)OppGoallerState.Right);
            if (lastoppstate == null)
                return OppGoallerState.Left;
            //int? a = OppGoallerId(engine, Model);
            Position2D oppGollerPosition = lastoppstate.Location;//(lastoppstate != null) ? Model.Opponents[a.Value].Location : GameParameters.OppGoalCenter;
            if (oppGollerPosition.Y > GameParameters.OppGoalCenter.Y)
                return OppGoallerState.Left;
            else if (oppGollerPosition.Y < GameParameters.OppGoalCenter.Y)
                return OppGoallerState.Right;
            else
                return (OppGoallerState)v;
        }
        enum OppGoallerState
        {
            Left,
            Right,
        }

    }
}
