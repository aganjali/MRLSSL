using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Skills.GoalieSkills
{
    public class GoalieInPenaltyAreaSkill : SkillBase
    {
        public GoalieInPenaltyAreaSkill()
        {
            //      Controller = new Controller();
            GotoPoint = new GotoPointSkill();
        }
        GotoPointSkill GotoPoint;
        public SingleWirelessCommand GoGetBall(GameStrategyEngine engine, WorldModel Model, int RobotID, bool isChipKick, double kickPower)
        {
            SingleWirelessCommand SWC = new SingleWirelessCommand();

            //    GotoPoint.SetController(false);
            //    Controller.DontGoInDangerZone = false;

            Position2D robotLoc = Model.OurRobots[RobotID].Location;
            bool gotoTarget = true;
            double k = 0.06;
            double tresh = 0.17;

            if (Model.BallState.Location.X > GameParameters.OurGoalCenter.X)
            {
                if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalLeft) < Model.BallState.Location.DistanceFrom(GameParameters.OurGoalRight))
                    SWC = GotoPoint.GotoPoint(Model, RobotID, new Position2D(GameParameters.OurGoalLeft.X - 0.13f, GameParameters.OurGoalLeft.Y - 0.04), 180, false, false, 3.5, false);
                else
                    SWC = GotoPoint.GotoPoint(Model, RobotID, new Position2D(GameParameters.OurGoalRight.X - 0.13f, GameParameters.OurGoalRight.Y + 0.04), 180, false, false, 3.5, false);
            }
            else if (robotLoc.X - Model.BallState.Location.X > 0.05 || Math.Abs(Model.BallState.Location.Y - GameParameters.OurGoalCenter.Y) > 0.37)
            {
                float to = (float)(Model.BallState.Location.X - GameParameters.OppGoalCenter.X);
                float from = to - 1.5f;
                if (engine.GameInfo != null && engine.GameInfo.OppTeam.Distance[from, to].Count == 0)
                {
                    Vector2D vec = Model.BallState.Location - GameParameters.OppGoalCenter;
                    Vector2D ballSpeed = Model.BallState.Speed;
                    Position2D ballLocation = Model.BallState.Location;
                    Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
                    Position2D robotLocation = Model.OurRobots[RobotID].Location;
                    Vector2D robotBallVec = ballLocation - robotLocation;
                    Vector2D ballTargetVec = GameParameters.OppGoalCenter - ballLocation;
                    Position2D backBallPoint = ballLocation - ballTargetVec.GetNormalizeToCopy(0.09);
                    Vector2D robotBackBallVec = backBallPoint - robotLocation;
                    Vector2D ballBackBallVec = backBallPoint - ballLocation;
                    double segmentConst = 0.7;
                    double rearDistance = 0.15;
                    Vector2D tmp1 = robotBackBallVec.GetNormalizeToCopy(robotBackBallVec.Size * segmentConst);
                    Position2D p1 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians + Math.PI / 2, rearDistance);
                    Position2D p2 = (robotLocation + tmp1) + Vector2D.FromAngleSize(tmp1.AngleInRadians - Math.PI / 2, rearDistance);
                    Position2D midPoint = p1;
                    if (ballLocation.DistanceFrom(p2) > ballLocation.DistanceFrom(p1))
                    {
                        midPoint = p2;
                    }
                    Position2D finalPosToGo = midPoint;
                    double teta = Vector2D.AngleBetweenInRadians(robotBackBallVec, robotBallVec);
                    double alfa = Vector2D.AngleBetweenInRadians(robotBackBallVec, ballBackBallVec);

                    double distance = robotBackBallVec.Size / ((1 / Math.Tan(Math.Abs(teta))) + (1 / Math.Tan(Math.Abs(alfa))));

                    if (Math.Abs(Vector2D.AngleBetweenInRadians(robotBallVec, ballTargetVec)) < Math.PI / 6 && (Math.Abs(alfa) > Math.PI / 1.5 || Math.Abs(distance) > RobotParameters.OurRobotParams.Diameter / 2 + .01))
                        finalPosToGo = backBallPoint;
                    else
                    {
                        Vector2D robotMidPointVec = finalPosToGo - robotLocation;
                        double Angle = Vector2D.AngleBetweenInRadians(robotMidPointVec, ballSpeed);
                        if (Math.Abs(Angle) < Math.PI / 15)
                            finalPosToGo = finalPosToGo + ballSpeed.GetNormalizeToCopy((ballLocation - robotLocation).Size);
                    }

                    finalPosToGo = Model.OurRobots[RobotID].Location + (finalPosToGo - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((backBallPoint - Model.OurRobots[RobotID].Location).Size);

                    if ((robotLoc.Y > GameParameters.OurGoalLeft.Y && finalPosToGo.Y < GameParameters.OurGoalLeft.Y && finalPosToGo.Y > GameParameters.OurGoalRight.Y)
                        || (robotLoc.Y < GameParameters.OurGoalLeft.Y && robotLoc.Y > GameParameters.OurGoalRight.Y && finalPosToGo.Y > GameParameters.OurGoalLeft.Y))
                        if ((finalPosToGo - robotLoc).PrependecularPoint(robotLoc, GameParameters.OurGoalLeft).DistanceFrom(GameParameters.OurGoalLeft) < tresh)
                        {
                            Position2D midP = GameParameters.OurGoalLeft + new Vector2D(-tresh, 0);
                            finalPosToGo = robotLoc + (midP - robotLoc).GetNormalizeToCopy((finalPosToGo - robotLoc).Size);
                        }
                    if ((robotLoc.Y < GameParameters.OurGoalRight.Y && finalPosToGo.Y < GameParameters.OurGoalLeft.Y && finalPosToGo.Y > GameParameters.OurGoalRight.Y)
                        || (robotLoc.Y > GameParameters.OurGoalRight.Y && robotLoc.Y < GameParameters.OurGoalLeft.Y && finalPosToGo.Y < GameParameters.OurGoalRight.Y))
                        if ((finalPosToGo - robotLoc).PrependecularPoint(robotLoc, GameParameters.OurGoalRight).DistanceFrom(GameParameters.OurGoalRight) < tresh)
                        {
                            Position2D midP = GameParameters.OurGoalRight + new Vector2D(-tresh, 0);
                            finalPosToGo = robotLoc + (midP - robotLoc).GetNormalizeToCopy((finalPosToGo - robotLoc).Size);
                        }
                    if ((robotLoc.Y > GameParameters.OurGoalLeft.Y && finalPosToGo.Y < GameParameters.OurGoalRight.Y)
                       || (robotLoc.Y < GameParameters.OurGoalRight.Y && finalPosToGo.Y > GameParameters.OurGoalLeft.Y))
                        if (((finalPosToGo - robotLoc).PrependecularPoint(robotLoc, GameParameters.OurGoalLeft).DistanceFrom(GameParameters.OurGoalLeft) < tresh)
                            || ((finalPosToGo - robotLoc).PrependecularPoint(robotLoc, GameParameters.OurGoalRight).DistanceFrom(GameParameters.OurGoalRight) < tresh))
                        {
                            if ((finalPosToGo - robotLoc).PrependecularPoint(robotLoc, GameParameters.OurGoalLeft).DistanceFrom(GameParameters.OurGoalLeft) < tresh)
                            {
                                Position2D midP = GameParameters.OurGoalLeft + new Vector2D(-tresh, 0);
                                finalPosToGo = robotLoc + (midP - robotLoc).GetNormalizeToCopy((finalPosToGo - robotLoc).Size);
                                midP = GameParameters.OurGoalRight + new Vector2D(-tresh, 0);
                                finalPosToGo = robotLoc + (midP - robotLoc).GetNormalizeToCopy((finalPosToGo - robotLoc).Size);
                            }
                            else
                            {
                                Position2D midP = GameParameters.OurGoalRight + new Vector2D(-tresh, 0);
                                finalPosToGo = robotLoc + (midP - robotLoc).GetNormalizeToCopy((finalPosToGo - robotLoc).Size);
                                midP = GameParameters.OurGoalLeft + new Vector2D(-tresh, 0);
                                finalPosToGo = robotLoc + (midP - robotLoc).GetNormalizeToCopy((finalPosToGo - robotLoc).Size);
                            }
                        }
                    if (finalPosToGo.X > GameParameters.OurGoalCenter.X - 0.05)
                        gotoTarget = false;
                    if (gotoTarget)
                        Planner.Add(RobotID, finalPosToGo, (-vec).AngleInDegrees, false);
                    //SWC = Controller.CalculateTargetSpeed(Model, RobotID, finalPosToGo, (-vec).AngleInDegrees, null);
                    else
                        Planner.Add(RobotID, Model.BallState.Location, (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                    //SWC = Controller.CalculateTargetSpeed(Model, RobotID, Model.BallState.Location, (Model.BallState.Location - robotLoc).AngleInDegrees, null);
                }
                else
                    Planner.Add(RobotID, Model.BallState.Location, (Model.BallState.Location - robotLoc).AngleInDegrees, false);
                //SWC = Controller.CalculateTargetSpeed(Model, RobotID, Model.BallState.Location, (Model.BallState.Location - robotLoc).AngleInDegrees, null);
            }
            else
            {
                Position2D p;

                if (GameParameters.OurGoalCenter.X - Model.BallState.Location.X > 0.2)
                {
                    p = (GameParameters.OurGoalCenter - Model.BallState.Location).GetNormalizeToCopy(0.2) + Model.BallState.Location;
                    SWC = GotoPoint.GotoPoint(Model, RobotID, p, 180, true, true, 2, false);
                }
                else
                {
                    if (engine.GameInfo.OppTeam.InDangerousZone.Count == 0)
                    {
                        p = new Position2D(Model.BallState.Location.X - 0.2, Model.BallState.Location.Y);
                        SWC = GotoPoint.GotoPoint(Model, RobotID, p, 180, true, true, 3.5, false);
                    }
                    else
                    {
                        int oppId = engine.GameInfo.OppTeam.InDangerousZone.OrderBy(o => o.Value).First().Key;
                        p = Model.BallState.Location + (Model.Opponents[oppId].Location - Model.BallState.Location).GetNormalizeToCopy(0.2);
                        SWC = GotoPoint.GotoPoint(Model, RobotID, p, (p - Model.BallState.Location).AngleInDegrees, true, true, 3.5, false);
                    }
                }
            }
            Planner.AddKick(RobotID, kickPowerType.Power, true, kickPower);
            //SWC.isChipKick = isChipKick;
            //SWC.KickPower = kickPower;
            //SWC.BackSensor = false;
            // SingleWirelessCommand SWC = new SingleWirelessCommand();
            return SWC;
        }
    }
}
