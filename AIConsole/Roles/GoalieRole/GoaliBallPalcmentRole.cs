using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills.GoalieSkills;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Roles
{
    class GoaliBallPalcmentRole : RoleBase
    {
        int currentState;
        public Position2D TargetFainal = new Position2D();
        Position2D Target = new Position2D();
        public SingleObjectState ballState = new GameDefinitions.SingleObjectState();
        public SingleObjectState ballStateFast = new GameDefinitions.SingleObjectState();

        public SingleWirelessCommand Run(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta)
        {
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(palcment1)))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(palcment1)).First();
            if (inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X)
            {
            if (ballStateFast.Location.Y < 0)
                {
                    TargetPos = GameParameters.OurGoalRight + new Vector2D(-0.12, 0);
                }
                else
                {

                    TargetPos = GameParameters.OurGoalLeft + new Vector2D(-0.12, 0);
                }
            }

            SingleWirelessCommand SWc = new GameDefinitions.SingleWirelessCommand();
            DrawingObjects.AddObject(new Circle(TargetPos, 0.2, new System.Drawing.Pen(System.Drawing.Color.Red, 0.02f)));
            if (CurrentState == (int)GoalieStates.Normal)
            {
                //GetSkill<GotoPointSkill>().SetController(false);
                SWc = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, TargetPos, Teta, false, false, 3.5, false);
            }
            else if (CurrentState == (int)GoalieStates.InPenaltyArea)
            {
                //SWc = GetSkill<GoalieInPenaltyAreaSkill>().GoGetBall(engine, model, robotID, true, 200);
                GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                Position2D tar = TargetToKick(Model, RobotID);
                GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                Vector2D v = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 0.35);
                double kickSpeed = 4;

                if (ballState.Location.X > GameParameters.OurGoalCenter.X - 0.1 || Math.Abs(Model.OurRobots[RobotID].Angle.Value) < 100 || obs.Meet(ballState, new SingleObjectState(ballState.Location + v, Vector2D.Zero, 0), 0.022))
                    kickSpeed = 0;
                Planner.AddKick(RobotID, kickPowerType.Speed, kickSpeed, (kickSpeed > 0) ? true : false, false);
            }
            else if (CurrentState == (int)GoalieStates.KickToGoal)
                SWc = GetSkill<GoaliDiveSkill>().Dive(engine, Model, RobotID, true, 200);
            else //Kick to robot
                Planner.Add(RobotID, Model.OurRobots[RobotID].Location, (ballStateFast.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false);
            SWc.isChipKick = true;
            SWc.KickPower = 255;
            return SWc;
        }

        public SingleWirelessCommand RunStop(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta)
        {
            Line ll = new Line();
            Line line1 = new Line();
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 1);
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                ballStateFast = DefenceTest.currentBallState;
            }
            else
            {
                ballState = Model.BallState;
                ballStateFast = Model.BallStateFast;
            }
            //DefenceInfo inf = null;
            //if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(palcment1)))
            //    inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(palcment1)).First();
            //if (inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X)
            //{
            if (ballStateFast.Location.Y < 0)
            {
                TargetFainal = GameParameters.OurGoalRight + new Vector2D(-0.12, 0);
            }
            else
            {

                TargetFainal = GameParameters.OurGoalLeft + new Vector2D(-0.12, 0);
            }
            TargetPos = TargetFainal;
            // }

            SingleWirelessCommand SWc = new GameDefinitions.SingleWirelessCommand();
            DrawingObjects.AddObject(new Circle(TargetPos, 0.2, new System.Drawing.Pen(System.Drawing.Color.Red, 0.02f)));
            if (CurrentState == (int)GoalieStates.Normal)
            {
                //GetSkill<GotoPointSkill>().SetController(false);
                SWc = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, TargetPos, Teta, false, false, 1, false);
            }
            else if (CurrentState == (int)GoalieStates.InPenaltyArea)
            {
                //SWc = GetSkill<GoalieInPenaltyAreaSkill>().GoGetBall(engine, model, robotID, true, 200);

                GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                Position2D tar = TargetToKick(Model, RobotID);
                GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                Vector2D v = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 0.35);
                double kickSpeed = 4;

                if (ballState.Location.X > GameParameters.OurGoalCenter.X - 0.1 || Math.Abs(Model.OurRobots[RobotID].Angle.Value) < 100 || obs.Meet(ballState, new SingleObjectState(ballState.Location + v, Vector2D.Zero, 0), 0.022))
                    kickSpeed = 0;
                Planner.AddKick(RobotID, kickPowerType.Speed, kickSpeed, (kickSpeed > 0) ? true : false, false);
            }
            else if (CurrentState == (int)GoalieStates.KickToGoal)
                SWc = GetSkill<GoaliDiveSkill>().Dive(engine, Model, RobotID, true, 200);
            else //Kick to robot
                Planner.Add(RobotID, Model.OurRobots[RobotID].Location, (ballStateFast.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false);
            SWc.isChipKick = true;
            SWc.KickPower = 255;
            return SWc;
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if (!GameParameters.IsInField(ballState.Location, 0.05))
                CurrentState = (int)GoalieStates.Normal;
            else
            {

                Vector2D ballSpeed = ballStateFast.Speed;
                double v = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[RobotID].Location - ballStateFast.Location));
                double maxIncomming = 1.5, maxVertical = 1, maxOutGoing = 1;
                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v))
                    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v)
                    + maxVertical;
                double maxSpeedToGet = 0.5;
                double dist, dist2;
                double margin = 0.1;



                double distToBall = ballState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
                if (distToBall == 0)
                    distToBall = 0.5;
                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);

                double innerProduct = Vector2D.InnerProduct(ballStateFast.Speed, (Model.OurRobots[RobotID].Location - ballStateFast.Location));
                double difAngle = Vector2D.AngleBetweenInDegrees(ballStateFast.Speed, (ballStateFast.Location - Model.OurRobots[RobotID].Location));

                Circle c = new Circle(Model.OurRobots[RobotID].Location, 0.12);
                Line l = new Line(ballStateFast.Location, ballStateFast.Location + ballStateFast.Speed);

                List<Position2D> inters = c.Intersect(l);

                if (CurrentState == (int)GoalieStates.Normal)
                {
                    if (BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                        CurrentState = (int)GoalieStates.InPenaltyArea;

                }
                else if (CurrentState == (int)GoalieStates.InPenaltyArea)
                {
                    margin = 0.2;
                    if (BallKickedToOurGoal(Model) &&
                        (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2)
                        || acceptableballRobotSpeed * 1.2 < ballSpeed.Size))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) || acceptable2 * 1.2 < ballSpeed.Size)
                        CurrentState = (int)GoalieStates.Normal;
                }
                else if (CurrentState == (int)GoalieStates.KickToGoal)
                {
                    margin = 0.1;
                    if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (!BallKickedToOurGoal(Model) && GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if (!BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.Normal;
                }
                else if (CurrentState == (int)GoalieStates.KickToRobot)
                {
                    if (ballSpeed.Size < 1.5 || BallKickedToOurGoal(Model) || inters.Count == 0 || innerProduct < -0.1)
                    {
                        if (BallKickedToOurGoal(Model))
                            CurrentState = (int)GoalieStates.KickToGoal;
                        else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 < ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                            CurrentState = (int)GoalieStates.InPenaltyArea;
                        else
                            CurrentState = (int)GoalieStates.Normal;
                    }
                }
            }
            FreekickDefence.CurrentStates[this] = CurrentState;
            currentState = CurrentState;
            DrawingObjects.AddObject(new StringDraw(((GoalieStates)CurrentState).ToString(), GameParameters.OurGoalCenter.Extend(0.3, 0)), "gstate");
        }
        public bool BallKickedToOurGoal(WorldModel Model)
        {
            double tresh = 0.25;
            if ((GoalieStates)currentState == GoalieStates.KickToGoal)
                tresh = 0.3;
            Line line = new Line();
            line = new Line(ballState.Location, ballState.Location - ballStateFast.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = ballState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + tresh && BallGoal.Y > GameParameters.OurGoalRight.Y - tresh)
                if (ballStateFast.Speed.InnerProduct(GameParameters.OurGoalRight - ballState.Location) > 0)
                    if (ballStateFast.Speed.Size > 0.1 && d / ballStateFast.Speed.Size < 1.3)
                        return true;
            return false;
        }
        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 20 * RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new GoaliBallPalcmentRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public Position2D TargetToKick(WorldModel Model, int robotID)
        {
            //double minopp = Model.Opponents.Min ( m => m.Value.Location.DistanceFrom ( ballState.Location ) );
            //double ourDist = Model.OurRobots [robotID].Location.DistanceFrom ( ballState.Location );
            //if ( minopp > 0.5 && ourDist < 0.2 )
            //    return GameParameters.OppGoalCenter;
            //return ballState.Location + ( ballState.Location - GameParameters.OurGoalCenter ).GetNormalizeToCopy ( 2 );
            return ballState.Location + (ballState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(2);
            //return ballState.Location + ( -1 * ballStateFast.Speed ).GetNormalizeToCopy ( 2 );
        }

        public enum GoalieStates
        {
            Normal = 0,
            InPenaltyArea = 1,
            KickToGoal = 2,
            KickToRobot = 3
        }
    }
}