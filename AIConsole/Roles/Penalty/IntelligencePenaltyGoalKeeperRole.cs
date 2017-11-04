using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.IO;
using System.Xml;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;
using MRL.SSL.AIConsole.Skills;

namespace MRL.SSL.AIConsole.Roles
{
    class IntelligencePenaltyGoalKeeperRole : RoleBase
    {
        private const double FixCounterThreshold = 40;
        private const double RotationConfidence = 2;
        private const double DistFromLine = .1;
        private const int KickConfidence = 15;
        private const int DiveShiftFrame = 12;
        private const int FrequenceTime = 15;
        private const int DiveDuration = 14;

        public static PenaltyGoalKeeperLearningUnit.PenaltyOutputLearning PenaltyLastStatus = new PenaltyGoalKeeperLearningUnit.PenaltyOutputLearning();
        public PenaltyGoalKeeperLearningUnit penaltyGoalKeeper = new PenaltyGoalKeeperLearningUnit();
        public static Dictionary<int, bool> penaltyLeaderBoardInfo = new Dictionary<int, bool>();
        public static SingleObjectState initialStateInDive = new SingleObjectState();
        public static SingleWirelessCommand SWC = new SingleWirelessCommand();
        private SingleWirelessCommand swc = new SingleWirelessCommand();
        static SingleObjectState shooterState = new SingleObjectState();
        private static Position2D lastPos = new Position2D();
        private static bool successInFrquencyMotion = false;
        private static bool successInLearnPattern = false;
        private static bool successInGeneralMode = false;
        static Position2D target = new Position2D();
        private static bool logicallyLogic = false;
        public static int centerToLeftOrRight = 0;
        private static float lastRobotAngle2 = 0;
        public static bool nextState2Cal = false;
        public static bool nextStateCal = false;
        private static int lastPenaltyCount = 0;
        private static bool oneRecieve = false;
        public static bool nextState2 = false;
        public static bool firstTime10 = true;
        public static bool firstTime11 = true;
        public static bool firstTime1 = true;
        public static bool firstTime2 = true;
        public static bool firstTime3 = true;
        public static bool firstTime4 = true;
        public static bool firstTime5 = true;
        public static bool firstTime6 = true;
        public static bool firstTime7 = true;
        public static bool firstTime8 = true;
        public static bool nextState = false;
        public static bool firstTime9 = true;
        private static int PenaltyStates = 0;
        public static int KickedCounter = 0;
        private static int averageFrame = 0;
        public static int counterCenter = 0;
        public static bool onceFlag2 = true;
        public static bool onceFlag3 = true;
        private int counterFixIntersect = 0;
        public static bool onceFlag = true;
        public static double currentV = 0;
        public static double currentW = 0;
        public static int diveCounter = 0;
        public static int diveFrame = 0;
        public static int timeLine = 0;
        private double lastRobotAngle;
        private double posVar = 0.03;
        private int counter = 0;
        private int state = 0;

        /// <summary>
        /// Main Function for Run Intelligent Goaler
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="RobotID">RobotID</param>
        public void Run(GameStrategyEngine engine, WorldModel model, int RobotID)
        {

            if (PenaltyGoalKeeperLearningUnit.kicked)
            {
                KickedCounter++;
            }
            else
            {
                KickedCounter = 0;
            }

            penaltyGoalKeeper.Run(model);
            int ShooterID = PenaltyGoalKeeperLearningUnit.ShooterID.Value;
            PenaltyGoalKeeperLearningUnit.PenaltyOutputLearning POL = PenaltyGoalKeeperLearningUnit.DecisionMakingOutPut;
            DrawLeaderBoard(PenaltyGoalKeeperLearningUnit.Succes, PenaltyGoalKeeperLearningUnit.penaltyCount);

            if (PenaltyGoalKeeperLearningUnit.penaltyCount > 1)
            {
                POL = (PenaltyGoalKeeperLearningUnit.Succes) ? PenaltyLastStatus : PenaltyGoalKeeperLearningUnit.DecisionMakingOutPut;
                POL = PenaltyGoalKeeperLearningUnit.DecisionMakingOutPut;
            }

            if (PenaltyGoalKeeperLearningUnit.penaltyCount == 1 && POL.success)
            {
                successInLearnPattern = true;
                successInFrquencyMotion = false;
                successInGeneralMode = false;
            }

            if ((PenaltyGoalKeeperLearningUnit.penaltyCount == 2 && POL.success) || (PenaltyGoalKeeperLearningUnit.penaltyCount > 2 && POL.retryFrequencyMotion && POL.success))
            {
                successInFrquencyMotion = true;
                successInLearnPattern = false;
                successInGeneralMode = false;
            }
            if (PenaltyGoalKeeperLearningUnit.penaltyCount > 2 && !POL.retryFrequencyMotion && POL.success)
            {
                successInGeneralMode = true;
                successInLearnPattern = false;
                successInFrquencyMotion = false;
            }
            ///In first penalty we want start learn with specific pattern if 
            ///we succeed in first penalty we retry catch penalty with initial 
            ///pattern
            if (PenaltyGoalKeeperLearningUnit.penaltyCount == 0 && PenaltyGoalKeeperLearningUnit.inLearnMotion)
            {
                LearnState(model, RobotID, PenaltyGoalKeeperLearningUnit.ShooterID.Value);
            }
            else
            {
                if (POL.immediateOrGo == PenaltyGoalKeeperLearningUnit.immediateORWaited.immediate)
                {
                    if (POL.leftOrRightGoBall == POL.leftOrRightGoRobot)
                    {
                        Position2D intersect = GameParameters.OurGoalCenter.Extend(-.1, 0);
                        Line IntersectLine = new Line(GameParameters.OurLeftCorner.Extend(-.1, 0), GameParameters.OurRightCorner.Extend(-0.1, 0));
                        Line headLine = new Line(model.Opponents[ShooterID].Location, (model.Opponents[ShooterID].Location + Vector2D.FromAngleSize(model.Opponents[ShooterID].Angle.Value * (Math.PI / 180), 2)));
                        DrawingObjects.AddObject(headLine);
                        if (IntersectLine.IntersectWithLine(headLine).HasValue)
                            if (IntersectLine.IntersectWithLine(headLine).Value.DistanceFrom(GameParameters.OurGoalCenter.Extend(-.1, 0)) < .3)
                                intersect = IntersectLine.IntersectWithLine(headLine).Value;
                        target = intersect;
                        Planner.ChangeDefaulteParams(RobotID, false);
                        Planner.SetParameter(RobotID, 15, 15);
                        Planner.Add(RobotID, target, 180, false);
                    }
                    if (POL.leftOrRightGoBall != POL.leftOrRightGoRobot)
                    {
                        Execusion(engine, model, POL, RobotID, ShooterID, POL.time);
                    }
                }
                else
                {
                    if (POL.dependOrNotWaiting == PenaltyGoalKeeperLearningUnit.DependORNot.Independent)
                    {
                        if (PenaltyGoalKeeperLearningUnit.penaltyCount == 1 && POL.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Left)
                        {
                            logicallyLogic = true;
                        }
                        if (logicallyLogic)
                        {
                            if (((PenaltyGoalKeeperLearningUnit.penaltyCount == 1 || (PenaltyGoalKeeperLearningUnit.penaltyCount > 1 && POL.retryFrequencyMotion)) || successInFrquencyMotion))// && PenaltyGoalkeeper.inFrequencyMotion)
                            {
                                FrequencyMotion(model, 20, RobotID, true);
                            }
                            else if (((PenaltyGoalKeeperLearningUnit.penaltyCount > 1 && !POL.retryFrequencyMotion) || successInGeneralMode))//&& PenaltyGoalkeeper.inExecutionMotion)
                            {
                                Execusion(engine, model, POL, RobotID, PenaltyGoalKeeperLearningUnit.ShooterID.Value, POL.time);
                            }
                        }
                        else if (PenaltyGoalKeeperLearningUnit.penaltyCount > 0)
                        {
                            ExecusionTurnOnFirstPenalty(engine, model, POL, RobotID, PenaltyGoalKeeperLearningUnit.ShooterID.Value, POL.time);
                        }
                    }
                    else if (POL.dependOrNotWaiting == PenaltyGoalKeeperLearningUnit.DependORNot.Dependent)
                    {
                        Execusion(engine, model, POL, RobotID, PenaltyGoalKeeperLearningUnit.ShooterID.Value, POL.time);
                    }

                    if (PenaltyGoalKeeperLearningUnit.penaltyCount > 0 && (POL.dependOrNotWaiting == PenaltyGoalKeeperLearningUnit.DependORNot.Dependent && POL.whereLookWaiting.HasValue && POL.whereLookWaiting.Value == PenaltyGoalKeeperLearningUnit.WhereLook.Lookatgoalie))
                    {
                        shooterState = model.Opponents[ShooterID];
                        if (model.Status == GameStatus.Penalty_Opponent_Waiting)
                        {
                            if (POL.dependOrNotWaiting == PenaltyGoalKeeperLearningUnit.DependORNot.Independent && POL.centerOrCornerWaiting == PenaltyGoalKeeperLearningUnit.CenterOrCorner.Center)
                            {
                                Position2D intersect = GameParameters.OurGoalCenter.Extend(-.1, 0);
                                Line IntersectLine = new Line(GameParameters.OurLeftCorner.Extend(-.1, 0), GameParameters.OurRightCorner.Extend(-0.1, 0));
                                Line headLine = new Line(shooterState.Location, (shooterState.Location + Vector2D.FromAngleSize(shooterState.Angle.Value * (Math.PI / 180), 2)));
                                if (IntersectLine.IntersectWithLine(headLine).HasValue)
                                    if (IntersectLine.IntersectWithLine(headLine).Value.DistanceFrom(GameParameters.OurGoalCenter.Extend(-.1, 0)) < .3)
                                        intersect = IntersectLine.IntersectWithLine(headLine).Value;
                                target = intersect.Extend(0, -1 * Math.Sign(intersect.Y) * posVar);
                                Planner.Add(RobotID, target, 180, false);
                            }
                            else
                            {
                                Position2D intersect = GameParameters.OurGoalCenter.Extend(-.1, 0);
                                Line IntersectLine = new Line(GameParameters.OurLeftCorner.Extend(-.1, 0), GameParameters.OurRightCorner.Extend(-0.1, 0));
                                DrawingObjects.AddObject(IntersectLine);
                                Line headLine = new Line(shooterState.Location, (shooterState.Location + Vector2D.FromAngleSize(shooterState.Angle.Value * (Math.PI / 180), 2)));
                                if (IntersectLine.IntersectWithLine(headLine).HasValue)
                                    if (IntersectLine.IntersectWithLine(headLine).Value.DistanceFrom(GameParameters.OurGoalCenter.Extend(-.1, 0)) < .3)
                                        intersect = IntersectLine.IntersectWithLine(headLine).Value;
                                target = intersect.Extend(0, -1 * Math.Sign(intersect.Y) * posVar);
                                Planner.Add(RobotID, target, 180, false);
                            }
                        }
                    }
                }
            }
            if (firstTime7 && PenaltyGoalKeeperLearningUnit.Succes)
            {
                PenaltyLastStatus = POL;
                firstTime7 = true;
            }
            #region Drawing
            //if (POL.nearOrFarGO == PenaltyGoalkeeper.NearOrFar.Far)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Go: Far", new Position2D(.1, 0)), "far");
            //}
            //else if (POL.nearOrFarGO == PenaltyGoalkeeper.NearOrFar.Near)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Go: Near", new Position2D(.1, 0)), "near");
            //}
            //if (POL.centerOrCornerWaiting == PenaltyGoalkeeper.CenterOrCorner.Center)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Waithing: Center", new Position2D(.2, 0)), "center");
            //}
            //else if (POL.centerOrCornerWaiting == PenaltyGoalkeeper.CenterOrCorner.Corner)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Waithing: corner", new Position2D(.2, 0)), "Corner");
            //}
            //if (POL.dependOrNotWaiting == PenaltyGoalkeeper.DependORNot.Dependent)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Waithing: Depend", new Position2D(.3, 0)), "Depend");
            //}
            //else if (POL.dependOrNotWaiting == PenaltyGoalkeeper.DependORNot.Independent)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Waithing: Independ", new Position2D(.3, 0)), "Independ");
            //}
            //if (POL.immediateOrGo == PenaltyGoalkeeper.immediateOrWaited.immediate)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Go: immediate", new Position2D(.4, 0)), "immediate");
            //}
            //else if (POL.immediateOrGo == PenaltyGoalkeeper.immediateOrWaited.Waited)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Go: Waited", new Position2D(.4, 0)), "Waited");
            //}
            //if (POL.leftOrRightWaiting == PenaltyGoalkeeper.LeftOrRight.Left)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Waithing:Left", new Position2D(.5, 0)), "Left");
            //}
            //else if (POL.leftOrRightWaiting == PenaltyGoalkeeper.LeftOrRight.Right)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Waithing:Right", new Position2D(.5, 0)), "Right");
            //}
            //DrawingObjects.AddObject(new StringDraw("Max = " + POL.max.ToString(), new Position2D(.6, 0)), "Max");
            //DrawingObjects.AddObject(new StringDraw("Min = " + POL.min.ToString(), new Position2D(.7, 0)), "Min");
            //if (POL.nearOrFarWaiting == PenaltyGoalkeeper.NearOrFar.Far)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Waithing: Far Waiting", new Position2D(.8, 0)), "Far Waiting");
            //}
            //else if (POL.nearOrFarWaiting == PenaltyGoalkeeper.NearOrFar.Near)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Waithing: Near Waiting", new Position2D(.8, 0)), "nearwaiting");
            //}
            //if (POL.patternTypeGO == PenaltyGoalkeeper.PatternType.FixedSumDiff)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Go: Fixed Sum Dif Go", new Position2D(.9, 0)), "fixed Sum Dif");
            //}
            //else if (POL.patternTypeGO == PenaltyGoalkeeper.PatternType.Random)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Go: Random Go", new Position2D(.9, 0)), "Random");
            //}
            //else if (POL.patternTypeGO == PenaltyGoalkeeper.PatternType.StaticDiff)
            //{
            //    DrawingObjects.AddObject(new StringDraw("Go: Static Differantial", new Position2D(.9, 0)), "Static Differrential");
            //}
            //switch (POL.shootOrTurnGO)
            //{
            //    case PenaltyGoalkeeper.ShootOrTurn.ShootAtLook:
            //        DrawingObjects.AddObject(new StringDraw("Go: Shoot At Look", new Position2D(1, 0)), "ShootAtLook");
            //        break;
            //    case PenaltyGoalkeeper.ShootOrTurn.TurnAndShoot:
            //        DrawingObjects.AddObject(new StringDraw("Go: Turn And Shoot", new Position2D(1, 0)), "Turn And Shoot");
            //        break;
            //    default:
            //        break;
            //}
            //switch (POL.whereLookWaiting)
            //{
            //    case PenaltyGoalkeeper.WhereLook.Lookatgoalie:
            //        DrawingObjects.AddObject(new StringDraw("Waiting Look At Goalie", new Position2D(1.1, 0)), "Look At Goalie");
            //        break;
            //    case PenaltyGoalkeeper.WhereLook.LookOtherSide:
            //        DrawingObjects.AddObject(new StringDraw("Waiting Look Other side", new Position2D(1.1, 0)), "LookOtherSide");
            //        break;
            //    default:
            //        break;
            //}
            //DrawingObjects.AddObject(new StringDraw("Time" + averageFrame, new Position2D(1.2, 0)), "Time");
            #endregion

        }

        /// <summary>
        /// Goalie Move according to Shooter Robot its Good for Slow Shooters 
        /// and we don't need Dive
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="ShooterID">Shooter Robot ID</param>
        /// <param name="RobotID">Goalie ID</param>
        public void MoveByAngle(WorldModel model, int ShooterID, int RobotID)
        {
            Position2D intersect = GameParameters.OurGoalCenter.Extend(-.1, 0);
            Line IntersectLine = new Line(GameParameters.OurLeftCorner.Extend(-.1, 0), GameParameters.OurRightCorner.Extend(-0.1, 0));
            DrawingObjects.AddObject(IntersectLine);
            Line headLine = new Line(model.Opponents[PenaltyGoalKeeperLearningUnit.ShooterID.Value].Location, (model.Opponents[PenaltyGoalKeeperLearningUnit.ShooterID.Value].Location + Vector2D.FromAngleSize(model.Opponents[PenaltyGoalKeeperLearningUnit.ShooterID.Value].Angle.Value * (Math.PI / 180), 2)));
            if (IntersectLine.IntersectWithLine(headLine).HasValue)
                if (IntersectLine.IntersectWithLine(headLine).Value.DistanceFrom(GameParameters.OurGoalCenter.Extend(-.1, 0)) < .3)
                    intersect = IntersectLine.IntersectWithLine(headLine).Value;
            target = intersect.Extend(0, -1 * Math.Sign(intersect.Y) * posVar);
            Planner.Add(RobotID, target, 180, false);
        }

        /// <summary>
        /// Execution Part of Move Robot after decision Making.(HBM)
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="pol">Penalty Output Learning</param>
        /// <param name="RobotID">Goalie ID</param>
        /// <param name="ShooterID">Shooter ID</param>
        /// <param name="diveFrameNum">Frame of Dive Start</param>
        private void Execusion(GameStrategyEngine engine, WorldModel model, PenaltyGoalKeeperLearningUnit.PenaltyOutputLearning pol, int RobotID, int ShooterID, int diveFrameNum)
        {

            PenaltyGoalKeeperLearningUnit.inExecutionMotion = true;
            PenaltyGoalKeeperLearningUnit.inFrequencyMotion = false;
            PenaltyGoalKeeperLearningUnit.inLearnMotion = false;

            //In Center Mode Shooter Robot see the center of our Goal
            if (pol.centerOrCornerWaiting == PenaltyGoalKeeperLearningUnit.CenterOrCorner.Center)
            {
                if (model.Status == GameStatus.Penalty_Opponent_Waiting)
                {
                    target = GameParameters.OurGoalCenter.Extend(-DistFromLine, -.03);
                    Planner.Add(RobotID, target, 180, false);
                }
                if (model.Status == GameStatus.Penalty_Opponent_Go)
                {
                    if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.bigAxis || (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.RLRLRL && PenaltyGoalKeeperLearningUnit.penaltyCount % 2 != 0))
                    {
                        if (pol.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Left)
                        {
                            CenterToLeftOrRight(engine, model, RobotID, ShooterID, true, diveFrameNum);
                        }
                        else if (pol.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Right)
                        {
                            CenterToLeftOrRight(engine, model, RobotID, ShooterID, false, diveFrameNum);
                        }
                    }
                    if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.smallAxis || (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.LRLRLR && PenaltyGoalKeeperLearningUnit.penaltyCount % 2 == 0) || PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.unPattern)
                    {
                        if (pol.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Right)
                        {
                            CenterToLeftOrRight(engine, model, RobotID, ShooterID, false, diveFrameNum);
                        }
                        else if (pol.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Left)
                        {
                            CenterToLeftOrRight(engine, model, RobotID, ShooterID, true, diveFrameNum);
                        }
                    }
                }
            }
            //In Corner Mode Shooter Robot see the Corner of our Goal
            else if (pol.centerOrCornerWaiting == PenaltyGoalKeeperLearningUnit.CenterOrCorner.Corner)
            {
                #region Independent
                if (model.Status == GameStatus.Penalty_Opponent_Waiting)
                {
                    target = GameParameters.OurGoalCenter.Extend(-.1, -.03);
                    Planner.Add(RobotID, target, 180, false);
                }
                if (model.Status == GameStatus.Penalty_Opponent_Go)
                {
                    timeLine++;
                }
                if (model.Status == GameStatus.Penalty_Opponent_Go)
                {
                    //State before Dive 
                    if (timeLine < diveFrameNum - DiveShiftFrame)
                    {
                        target = GameParameters.OurGoalCenter.Extend(-.1, -.03);
                        Planner.Add(RobotID, target, 180, false);
                    }
                    //Go For Dive
                    if (timeLine > diveFrameNum - DiveShiftFrame)
                    {
                        bool islefts = false;
                        if (pol.penaltytype == PenaltyGoalKeeperLearningUnit.penaltyType.bigAxis)
                        {
                            islefts = true;
                            DrawingObjects.AddObject(new StringDraw("Big axis", new Position2D(1.5, 1.5)), "346");
                        }
                        if (pol.penaltytype == PenaltyGoalKeeperLearningUnit.penaltyType.smallAxis)
                        {
                            islefts = false;
                            DrawingObjects.AddObject(new StringDraw("Small axis", new Position2D(1.5, 1.5)), "65465");
                        }
                        if (pol.penaltytype == PenaltyGoalKeeperLearningUnit.penaltyType.LRLRLR)
                        {
                            DrawingObjects.AddObject(new StringDraw("LRLRLRLR", new Position2D(1.5, 1.5)), "654684");
                            if (PenaltyGoalKeeperLearningUnit.penaltyCount % 2 == 0)
                            {
                                islefts = false;
                                DrawingObjects.AddObject(new StringDraw("Right", new Position2D(1.6, 1.5)), "4654645");
                            }
                            else
                            {
                                islefts = true;
                                DrawingObjects.AddObject(new StringDraw("Left", new Position2D(1.6, 1.5)), "324654");
                            }
                        }
                        if (pol.penaltytype == PenaltyGoalKeeperLearningUnit.penaltyType.RLRLRL)
                        {
                            DrawingObjects.AddObject(new StringDraw("RLRLRL", new Position2D(1.5, 1.5)), "65664");
                            if (PenaltyGoalKeeperLearningUnit.penaltyCount % 2 == 0)
                            {
                                DrawingObjects.AddObject(new StringDraw("Left", new Position2D(1.6, 1.5)), "3546540");
                                islefts = true;
                            }
                            else
                            {
                                DrawingObjects.AddObject(new StringDraw("Right", new Position2D(1.6, 1.5)), "54654465");
                                islefts = false;
                            }
                        }
                        if (pol.penaltytype == PenaltyGoalKeeperLearningUnit.penaltyType.unPattern)
                        {
                            DrawingObjects.AddObject(new StringDraw("UnPattern", new Position2D(1.5, 1.5)), "546546456");
                            islefts = true;
                        }
                        Dive(engine, model, RobotID, islefts, false, true);
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// This function calls when in first Penalty Shooter Robot
        /// turn and shoot
        /// </summary>
        /// <param name="engine">game Strategy Engine</param>
        /// <param name="model">World Model</param>
        /// <param name="pol">Penalty Output Learning</param>
        /// <param name="RobotID">RobotID</param>
        /// <param name="ShooterID">ShooterID</param>
        /// <param name="diveFrameNum">Dive Frame Number</param>
        private void ExecusionTurnOnFirstPenalty(GameStrategyEngine engine, WorldModel model, PenaltyGoalKeeperLearningUnit.PenaltyOutputLearning pol, int RobotID, int ShooterID, int diveFrameNum)
        {
            //In Center Mode Shooter Robot see the center of our Goal
            if (pol.centerOrCornerWaiting == PenaltyGoalKeeperLearningUnit.CenterOrCorner.Center)
            {
                #region Center
                if (model.Status == GameStatus.Penalty_Opponent_Waiting)
                {
                    target = GameParameters.OurGoalCenter.Extend(-DistFromLine, -.03);
                    Planner.Add(RobotID, target, 180, false);
                }
                if (model.Status == GameStatus.Penalty_Opponent_Go)
                {
                    if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.bigAxis || (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.RLRLRL && PenaltyGoalKeeperLearningUnit.penaltyCount % 2 != 0))
                    {
                        if (pol.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Left)
                        {
                            CenterToLeftOrRight(engine, model, RobotID, ShooterID, true, diveFrameNum);
                        }
                        else if (pol.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Right)
                        {
                            CenterToLeftOrRight(engine, model, RobotID, ShooterID, false, diveFrameNum);
                        }
                    }
                    if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.smallAxis || (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.LRLRLR && PenaltyGoalKeeperLearningUnit.penaltyCount % 2 == 0) || PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.unPattern)
                    {
                        if (pol.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Right)
                        {
                            CenterToLeftOrRight(engine, model, RobotID, ShooterID, false, diveFrameNum);
                        }
                        else if (pol.leftOrRightGoBall == PenaltyGoalKeeperLearningUnit.LeftOrRight.Left)
                        {
                            CenterToLeftOrRight(engine, model, RobotID, ShooterID, true, diveFrameNum);
                        }
                    }
                }
                #endregion
            }
            //In Corner Mode Shooter Robot see the Corner of our Goal
            else if (pol.centerOrCornerWaiting == PenaltyGoalKeeperLearningUnit.CenterOrCorner.Corner)
            {
                #region Independent
                if (model.Status == GameStatus.Penalty_Opponent_Waiting)
                {
                    target = GameParameters.OurGoalCenter.Extend(-.1, -.03);
                    Planner.Add(RobotID, target, 180, false);
                    lastRobotAngle2 = model.Opponents[ShooterID].Angle.Value;
                }
                if (model.Status == GameStatus.Penalty_Opponent_Go)
                {
                    if (Math.Abs(Math.Abs(model.Opponents[ShooterID].Angle.Value) - Math.Abs(lastRobotAngle2)) > 2)
                    {
                        target = GameParameters.OurGoalCenter.Extend(-.1, -.15);
                        Planner.Add(RobotID, target, 180, false);
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// This State use when we want to Dive from Center to Left or Right.(HBM)
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="RobotID">Robot ID</param>
        /// <param name="shooterID">Shooter ID</param>
        /// <param name="isLeft">Dive to Left Or Right</param>
        /// <param name="diveFrameNum">Frame Of dive</param>
        private void CenterToLeftOrRight(GameStrategyEngine engine, WorldModel model, int RobotID, int shooterID, bool isLeft, int diveFrameNum)
        {
            if (centerToLeftOrRight > diveFrameNum - DiveShiftFrame)
            {
                bool islefts = true;
                //if Penalty Type is BigAxis goalie Dive to Left because goalie stand a bit right and empty space of left is Bigger than Right
                if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.bigAxis)
                {
                    islefts = true;
                    DrawingObjects.AddObject(new StringDraw("Big axis", new Position2D(1.5, 1.5)), "346");
                }
                if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.smallAxis)
                {
                    islefts = false;
                    DrawingObjects.AddObject(new StringDraw("Small axis", new Position2D(1.5, 1.5)), "65465");
                }
                if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.LRLRLR)
                {
                    DrawingObjects.AddObject(new StringDraw("LRLRLRLR", new Position2D(1.5, 1.5)), "654684");
                    if (PenaltyGoalKeeperLearningUnit.penaltyCount % 2 == 0)
                    {
                        islefts = false;
                        DrawingObjects.AddObject(new StringDraw("Right", new Position2D(1.6, 1.5)), "334345");
                    }
                    else
                    {
                        islefts = true;
                        DrawingObjects.AddObject(new StringDraw("Left", new Position2D(1.6, 1.5)), "324654");
                    }
                }
                if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.RLRLRL)
                {
                    DrawingObjects.AddObject(new StringDraw("RLRLRL", new Position2D(1.5, 1.5)), "354654");
                    if (PenaltyGoalKeeperLearningUnit.penaltyCount % 2 == 0)
                    {
                        DrawingObjects.AddObject(new StringDraw("Left", new Position2D(1.6, 1.5)), "3546540");
                        islefts = true;
                    }
                    else
                    {
                        DrawingObjects.AddObject(new StringDraw("Right", new Position2D(1.6, 1.5)), "54654465");
                        islefts = false;
                    }
                }
                if (PenaltyGoalKeeperLearningUnit.penaltyLogic == PenaltyGoalKeeperLearningUnit.penaltyType.unPattern)
                {
                    DrawingObjects.AddObject(new StringDraw("UnPattern", new Position2D(1.5, 1.5)), "546546456");
                    islefts = true;
                }
                Dive(engine, model, RobotID, islefts, true, false);
            }
        }

        /// <summary>
        /// Function for Dive
        /// </summary>
        /// <param name="model">Wolrd Model</param>
        /// <param name="RobotID">Robot ID</param>
        /// <param name="isLeft">Dive to Left or Right</param>
        /// <param name="manual">Manual Or automatic Dive</param>
        /// <param name="closeLoop">closeLoop or Open Loop Dive</param>
        private void Dive(GameStrategyEngine engine, WorldModel model, int RobotID, bool isLeft, bool manual, bool closeLoop)
        {

            closeLoop = true;
            manual = true;
            if (KickedCounter < KickConfidence)
            {
                if (closeLoop)
                {
                    if (!manual)
                    {
                        Vector2D Stop = new Vector2D();
                        if (onceFlag)
                        {
                            Vector2D Bigger = new Vector2D();
                            Vector2D leftVector = GameParameters.OurGoalLeft.Extend(-0.13, 0) - model.OurRobots[RobotID].Location;
                            Vector2D rightVector = GameParameters.OurGoalRight.Extend(-0.13, 0) - model.OurRobots[RobotID].Location;
                            if (leftVector.Size > rightVector.Size)
                            {
                                Bigger = leftVector;
                            }
                            else
                            {
                                Bigger = rightVector;
                            }
                            onceFlag = false;
                            Stop = Bigger;
                            target = new Position2D(3.9, model.OurRobots[RobotID].Location.Y) + Bigger.GetNormalizeToCopy(2.5);
                        }

                        if (model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) > .13)
                        {
                            if (onceFlag2)
                            {
                                onceFlag2 = false;
                                Position2D stoppos = model.OurRobots[RobotID].Location;
                                target = stoppos;
                            }
                        }
                    }
                    else
                    {
                        if (!isLeft)
                        {
                            if (onceFlag2)
                            {
                                Vector2D leftVector = new Position2D(3.93, GameParameters.OurGoalRight.Y) - model.OurRobots[RobotID].Location;
                                target = model.OurRobots[RobotID].Location + leftVector.GetNormalizeToCopy(1);
                            }
                            if (model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalRight) < .5)
                            {
                                Vector2D leftVector = new Position2D(GameParameters.OurGoalCenter.X - .08, GameParameters.OurGoalRight.Y) - new Position2D(GameParameters.OurGoalCenter.X - .08, model.OurRobots[RobotID].Location.Y);
                                if (onceFlag2)
                                {
                                    onceFlag2 = false;
                                    Position2D stoppos = new Position2D(3.93, model.OurRobots[RobotID].Location.Y) - leftVector.GetNormalizeToCopy(1);
                                    target = stoppos;
                                }
                            }
                            if (model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalLeft) < .5)
                            {
                                Vector2D rightVector = new Position2D(GameParameters.OurGoalCenter.X - .08, GameParameters.OurGoalLeft.Y) - new Position2D(GameParameters.OurGoalCenter.X - .08, model.OurRobots[RobotID].Location.Y);
                                if (onceFlag3)
                                {
                                    onceFlag3 = false;
                                    Position2D stoppos = new Position2D(GameParameters.OurGoalCenter.X - .08, model.OurRobots[RobotID].Location.Y) - rightVector.GetNormalizeToCopy(1);
                                    target = stoppos;
                                }
                                onceFlag2 = true;
                            }
                        }
                        else
                        {
                            if (onceFlag2)
                            {
                                Vector2D rightVector = new Position2D(GameParameters.OurGoalCenter.X - .08, GameParameters.OurGoalLeft.Y) - new Position2D(GameParameters.OurGoalCenter.X - .08, model.OurRobots[RobotID].Location.Y);
                                target = model.OurRobots[RobotID].Location + rightVector.GetNormalizeToCopy(1);
                            }
                            if (model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalLeft) < .5)
                            {
                                Vector2D rightVector = new Position2D(GameParameters.OurGoalCenter.X - .08, GameParameters.OurGoalLeft.Y) - new Position2D(GameParameters.OurGoalCenter.X - .08, model.OurRobots[RobotID].Location.Y);
                                if (onceFlag2)
                                {
                                    onceFlag2 = false;
                                    Position2D stoppos = new Position2D(GameParameters.OurGoalCenter.X - .08, model.OurRobots[RobotID].Location.Y) - rightVector.GetNormalizeToCopy(1);
                                    target = stoppos;
                                }
                                onceFlag3 = true;
                            }
                            if (model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalRight) < .5)
                            {
                                Vector2D LeftVector = new Position2D(GameParameters.OurGoalCenter.X - .08, GameParameters.OurGoalRight.Y) - new Position2D(GameParameters.OurGoalCenter.X - .08, model.OurRobots[RobotID].Location.Y);
                                if (onceFlag3)
                                {
                                    onceFlag3 = false;
                                    Position2D stoppos = new Position2D(GameParameters.OurGoalCenter.X - .08, model.OurRobots[RobotID].Location.Y) - LeftVector.GetNormalizeToCopy(1);
                                    target = stoppos;
                                }
                                onceFlag2 = true;
                            }

                        }
                    }
                    Planner.AddKick(RobotID, kickPowerType.Power, true, 255);
                    Planner.ChangeDefaulteParams(RobotID, false);
                    Planner.SetParameter(RobotID, 15, 15);
                    Planner.Add(RobotID, target, 180, PathType.UnSafe, false, false, false, false);
                }
                else
                {

                    diveCounter++;

                    if (!isLeft)
                    {
                        if (diveCounter < DiveDuration)
                        {
                            currentV += .4;
                            if (RobotID == 0)
                            {
                                currentW = currentV / 10;
                                SWC.Vx = currentV;
                                SWC.W = -currentW * 3;
                                SWC.Vy = -currentW * 3;
                            }
                            else
                            {
                                currentW = currentV / 10;
                                SWC.Vx = currentV;
                                SWC.W = currentW * 2.2;
                                SWC.Vy = currentW * .8;
                            }
                            currentW = currentV / 10;
                            SWC.Vx = currentV;
                            SWC.W = currentW * 2.3;
                            SWC.Vy = currentW * 2.2;
                        }
                        else
                        {
                            SWC.Vx = 0;
                            SWC.W = 0;
                            SWC.Vy = 0;
                        }
                    }
                    else
                    {
                        if (diveCounter < DiveDuration)
                        {
                            currentV -= .4;
                            if (RobotID == 0)
                            {
                                currentW = currentV / 15;
                                SWC.Vx = currentV;
                                SWC.W = -currentW * 1.5;
                                SWC.Vy = currentW * .3;
                            }
                            if (RobotID == 3)
                            {
                                currentW = currentV / 10;
                                SWC.Vx = currentV;
                                SWC.W = -currentW * 12;
                                SWC.Vy = currentW * 2.5;
                            }
                        }
                        else
                        {
                            currentW = currentV / 10;
                            SWC.Vx = currentV;
                            SWC.W = -currentW * 12;
                            SWC.Vy = currentW * 2.5;
                        }
                        Planner.ChangeDefaulteParams(RobotID, false);
                        Planner.SetParameter(RobotID, 15, 15);
                        Planner.Add(RobotID, target, 180, false);
                    }

                    if (diveCounter > 40)
                    {
                        //isLeft = !isLeft;
                        //if (model.BallState.Speed.Size < .02)
                        //{

                        //    if (isLeft)
                        //    {
                        //        if (diveCounter <( 2*DiveDuration)+4)
                        //        {
                        //            currentV += .4;
                        //            if (RobotID == 0)
                        //            {
                        //                currentW = currentV / 10;
                        //                SWC.Vx = currentV;
                        //                SWC.W = -currentW * 3;
                        //                SWC.Vy = -currentW * 3;
                        //            }
                        //            else
                        //            {
                        //                currentW = currentV / 10;
                        //                SWC.Vx = currentV;
                        //                SWC.W = currentW * 2.2;
                        //                SWC.Vy = currentW * .8;
                        //            }
                        //            currentW = currentV / 10;
                        //            SWC.Vx = currentV;
                        //            SWC.W = currentW * 2.3;
                        //            SWC.Vy = currentW * 2.2;
                        //        }
                        //        else
                        //        {
                        //            SWC.Vx = 0;
                        //            SWC.W = 0;
                        //            SWC.Vy = 0;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (diveCounter <(2* DiveDuration)+4)
                        //        {
                        //            currentV -= .4;
                        //            if (RobotID == 0)
                        //            {
                        //                currentW = currentV / 15;
                        //                SWC.Vx = currentV;
                        //                SWC.W = -currentW * 1.5;
                        //                SWC.Vy = currentW * .3;
                        //            }
                        //            if (RobotID == 3)
                        //            {
                        //                currentW = currentV / 10;
                        //                SWC.Vx = currentV;
                        //                SWC.W = -currentW * 12;
                        //                SWC.Vy = currentW * 2.5;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            SWC.Vx = 0;
                        //            SWC.W = 0;
                        //            SWC.Vy = 0;
                        //        }
                        //    }
                        //}
                    }
                    //Planner.Add(RobotID, SWC);
                }
            }
            else
            {
                GetSkill<GetBallSkill>().PerformStatic(engine, model, RobotID, GameParameters.OurGoalCenter + (model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(2));
            }
        }

        /// <summary>
        /// Learning Mode in this state Robot in first penalty run pattern for
        /// fit shooter shooting function
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="RobotID">Robot ID</param>
        /// <param name="ShooterRobotID"> Shooter ID</param>
        private void LearnState(WorldModel model, int RobotID, int ShooterRobotID)
        {
            PenaltyGoalKeeperLearningUnit.inLearnMotion = true;
            PenaltyGoalKeeperLearningUnit.inExecutionMotion = false;
            PenaltyGoalKeeperLearningUnit.inFrequencyMotion = false;
            if (model.Status == GameStatus.Penalty_Opponent_Waiting)
            {
                firstTime10 = true;
                if (firstTime10)
                {
                    shooterState = model.Opponents[ShooterRobotID];
                    firstTime10 = false;
                }
            }
            if (model.Status == GameStatus.Penalty_Opponent_Go)
            {
                firstTime11 = true;
                if (firstTime11)
                {
                    shooterState = model.Opponents[ShooterRobotID];
                    firstTime11 = false;
                }
            }
            #region analyze
            if (model.Status == GameStatus.Penalty_Opponent_Waiting)
            {
                Position2D Pos = GameParameters.OurGoalCenter.Extend(-.1, posVar);
                Position2D left = Pos.Extend(0, -.3);
                if (PenaltyStates == (int)LearnStates.Initialpos)
                {
                    target = Pos;
                }

                if (PenaltyStates == (int)LearnStates.Go)
                {
                    target = left;
                }

                if (PenaltyStates == (int)LearnStates.ComeBack)
                {
                    target = Pos;
                }
                #region With Angle
                if (PenaltyStates == (int)LearnStates.End)
                {
                    Position2D intersect = GameParameters.OurGoalCenter.Extend(-.1, 0);
                    Line IntersectLine = new Line(GameParameters.OurLeftCorner.Extend(-.1, 0), GameParameters.OurRightCorner.Extend(-0.1, 0));
                    Line headLine = new Line(shooterState.Location, (shooterState.Location + Vector2D.FromAngleSize(shooterState.Angle.Value * (Math.PI / 180), 2)));
                    if (IntersectLine.IntersectWithLine(headLine).HasValue)
                    {
                        if (IntersectLine.IntersectWithLine(headLine).Value.DistanceFrom(GameParameters.OurGoalCenter.Extend(-.1, 0)) < .3)
                            intersect = IntersectLine.IntersectWithLine(headLine).Value;
                        else
                        {
                            Vector2D targetOutofGoal = IntersectLine.IntersectWithLine(headLine).Value - GameParameters.OurGoalCenter.Extend(.1, 0);
                            intersect = GameParameters.OurGoalCenter + targetOutofGoal.GetNormalizeToCopy(.28);
                        }
                    }
                    intersect = (IntersectLine.IntersectWithLine(headLine).Value.Y > 0.0) ? GameParameters.OurGoalLeft.Extend(-.1, -.1) : GameParameters.OurGoalRight.Extend(-.1, .1);

                    target = intersect.Extend(0, -1 * Math.Sign(intersect.Y) * posVar);
                    counterCenter++;
                    if (Math.Abs(Vector2D.AngleBetweenInDegrees(model.BallState.Location - model.Opponents[ShooterRobotID].Location, Vector2D.FromAngleSize(model.Opponents[ShooterRobotID].Angle.Value * (Math.PI / 180), 1))) < 2)
                    {
                        counterCenter++;
                    }
                    else
                    {
                        counterCenter = 0;
                    }
                    if (counterCenter > 10)
                    {
                        target = GameParameters.OurGoalCenter.Extend(-.1, 0);
                    }
                }
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 12, 8);
                Planner.Add(RobotID, target, 180, false);
                #endregion

            }

            if (model.Status == GameStatus.Penalty_Opponent_Go)
            {
                Position2D Pos = GameParameters.OurGoalCenter.Extend(-.1, posVar);
                Position2D left = Pos.Extend(0, -.3);
                if (PenaltyStates == (int)LearnStates.Initialpos)
                {
                    target = Pos;
                }

                if (PenaltyStates == (int)LearnStates.Go)
                {
                    target = left;
                }

                if (PenaltyStates == (int)LearnStates.ComeBack)
                {
                    target = Pos;
                }

                #region With Angle
                if (PenaltyStates == (int)LearnStates.End)
                {
                    Position2D intersect = GameParameters.OurGoalCenter.Extend(-.1, 0);
                    Line IntersectLine = new Line(GameParameters.OurLeftCorner.Extend(-.1, 0), GameParameters.OurRightCorner.Extend(-0.1, 0));
                    DrawingObjects.AddObject(IntersectLine);
                    Line headLine = new Line(shooterState.Location, (shooterState.Location + Vector2D.FromAngleSize(shooterState.Angle.Value * (Math.PI / 180), 2)));
                    if (IntersectLine.IntersectWithLine(headLine).HasValue)
                    {
                        if (IntersectLine.IntersectWithLine(headLine).Value.DistanceFrom(GameParameters.OurGoalCenter.Extend(-.1, 0)) < .3)
                            intersect = IntersectLine.IntersectWithLine(headLine).Value;
                        else
                        {
                            Vector2D targetOutofGoal = IntersectLine.IntersectWithLine(headLine).Value - GameParameters.OurGoalCenter.Extend(.1, 0);
                            intersect = GameParameters.OurGoalCenter + targetOutofGoal.GetNormalizeToCopy(.28);
                        }
                        intersect = (IntersectLine.IntersectWithLine(headLine).Value.Y > 0.0) ? GameParameters.OurGoalLeft.Extend(-.1, -.1) : GameParameters.OurGoalRight.Extend(-.1, .1);
                    }
                    target = intersect.Extend(0, -1 * Math.Sign(intersect.Y) * posVar);
                    counterCenter++;
                    if (Math.Abs(lastRobotAngle - model.Opponents[ShooterRobotID].Angle.Value) < 3)//(Math.Abs(Vector2D.AngleBetweenInDegrees(GameParameters.OurGoalCenter - model.Opponents[ShooterRobotID].Location, Vector2D.FromAngleSize(model.Opponents[ShooterRobotID].Angle.Value * (Math.PI / 180), 1))) < 2)
                    {
                        counterCenter++;
                    }
                    else
                    {
                        counterCenter = 0;
                    }
                    if (counterCenter > 10)
                    {
                        target = GameParameters.OurGoalCenter.Extend(-.1, 0);
                    }
                    lastRobotAngle = model.Opponents[ShooterRobotID].Angle.Value;

                    DrawingObjects.AddObject(new Circle(target, .08), "314546423");
                }
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 12, 8);
                Planner.Add(RobotID, target, 180, false);
                #endregion
            }
            #endregion
        }

        /// <summary>
        /// Frequency Motion for create fake free space in goal
        /// </summary>
        /// <param name="model">World Model</param>
        /// <param name="Frequency">Frequence of go and come back</param>
        /// <param name="RobotID"> Robot ID</param>
        /// <param name="left">Left or right</param>
        public void FrequencyMotion(WorldModel model, int Frequency, int RobotID, bool left)
        {
            PenaltyGoalKeeperLearningUnit.inFrequencyMotion = true;
            PenaltyGoalKeeperLearningUnit.inLearnMotion = false;
            PenaltyGoalKeeperLearningUnit.inExecutionMotion = false;
            if (firstTime4)
            {
                shooterState = model.Opponents[PenaltyGoalKeeperLearningUnit.ShooterID.Value];
                firstTime4 = false;
            }
            if (firstTime3)
            {
                Position2D intersect = GameParameters.OurGoalCenter.Extend(-.1, 0);
                Line IntersectLine = new Line(GameParameters.OurLeftCorner.Extend(-.1, 0), GameParameters.OurRightCorner.Extend(-0.1, 0));
                DrawingObjects.AddObject(IntersectLine);
                Line headLine = new Line(model.Opponents[PenaltyGoalKeeperLearningUnit.ShooterID.Value].Location, (model.Opponents[PenaltyGoalKeeperLearningUnit.ShooterID.Value].Location + Vector2D.FromAngleSize(model.Opponents[PenaltyGoalKeeperLearningUnit.ShooterID.Value].Angle.Value * (Math.PI / 180), 2)));
                if (IntersectLine.IntersectWithLine(headLine).HasValue)
                    if (IntersectLine.IntersectWithLine(headLine).Value.DistanceFrom(GameParameters.OurGoalCenter.Extend(-.1, 0)) < .3)
                        intersect = IntersectLine.IntersectWithLine(headLine).Value;
                if (lastPos.DistanceFrom(intersect) < .01)
                {
                    counterFixIntersect++;
                }
                else
                {
                    counterFixIntersect = 0;
                }
                lastPos = intersect;
                target = intersect.Extend(0, 1 * Math.Sign(intersect.Y) * 0);
                if (counterFixIntersect > FixCounterThreshold)
                {

                    firstTime3 = false;
                    counterFixIntersect = 0;
                }
                oneRecieve = false;
            }

            if (model.OurRobots[RobotID].Location.DistanceFrom(target) > .01 && Math.Abs(Math.Abs(model.OurRobots[RobotID].Angle.Value) - 180) > 1 && !oneRecieve)
            {
                Planner.Add(RobotID, target, 180, false);
            }
            else
            {
                oneRecieve = true;
            }
            if (oneRecieve)
            {
                counter++;
                if (counter % FrequenceTime == 0)
                {
                    if (state == 0)
                    {
                        state = 1;
                    }
                    else if (state == 1)
                    {
                        state = 0;
                    }
                }
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 15, 15);
                if (state == 0)
                {
                    Planner.Add(RobotID, target.Extend(0, -1 * Math.Sign(target.Y) * .3), 180, false);
                }
                if (state == 1)
                {
                    Planner.Add(RobotID, target.Extend(0, 0), 180, false);
                }
            }
        }

        /// <summary>
        /// Role Calculate Cost Function 
        /// </summary>
        /// <param name="engine">Engine</param>
        /// <param name="Model">World Model</param>
        /// <param name="RobotID">RobotID</param>
        /// <param name="previouslyAssignedRoles"></param>
        /// <returns>return Cost</returns>
        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            Position2D Pos = GameParameters.OurGoalCenter.Extend(-.1, posVar);
            Position2D left = Pos.Extend(0, -.3);
            if (Model.Status == GameStatus.Penalty_Opponent_Go && firstTime8)
            {
                PenaltyStates = 0;
                firstTime8 = false;
            }
            if (Model.Status == GameStatus.Penalty_Opponent_Waiting && firstTime9)
            {
                PenaltyStates = 0;
                firstTime9 = false;
            }
            if (PenaltyStates == (int)LearnStates.Initialpos && Model.OurRobots[RobotID].Location.DistanceFrom(Pos) < .01)
            {
                PenaltyStates = (int)LearnStates.Go;
            }
            if (PenaltyStates == (int)LearnStates.Go && Model.OurRobots[RobotID].Location.DistanceFrom(left) < .01)
            {
                PenaltyStates = (int)LearnStates.ComeBack;
            }
            if (PenaltyStates == (int)LearnStates.ComeBack && Model.OurRobots[RobotID].Location.DistanceFrom(Pos) < .01)
            {
                PenaltyStates = (int)LearnStates.End;
            }
        }

        private void DrawLeaderBoard(bool penaltySuccess, int penaltyCount)
        {
            //    Position2D leaderboard = GameParameters.OurGoalCenter.Extend(.2, 2);
            //    if (lastPenaltyCount != penaltyCount)
            //        penaltyLeaderBoardInfo.Add(penaltyCount, penaltySuccess);
            //    for (int i = 0 ; i < penaltyLeaderBoardInfo.Count ; i++)
            //    {
            //        double key = leaderboard.Y * i;
            //        DrawingObjects.AddObject(new Circle(leaderboard.Extend(0, i * -.35), .08, new Pen((penaltyLeaderBoardInfo.ElementAt(i).Value) ? Color.LimeGreen : Color.Red, .08f)), key.ToString());
            //    }
            //    lastPenaltyCount = penaltyCount;
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        public enum LearnStates
        {
            Initialpos,
            Go,
            ComeBack,
            End,
        }
    }
}