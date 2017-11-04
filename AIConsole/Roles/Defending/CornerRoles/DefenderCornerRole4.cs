using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;


namespace MRL.SSL.AIConsole.Roles
{
    class DefenderCornerRole4 : RoleBase, ISecondDefender
    {
        public Position2D Target = new Position2D();
        bool calculateCost = false;
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        Position2D intermediatePos = new Position2D();

        public static Position2D lasttarget = new Position2D();
        public static Position2D lastinitpos = new Position2D();
        private static bool wehaveintersect = false;
        private static int lastRobotID;
        private bool gotointermediatepos = false;

        public static Position2D lasttargetPoint = new Position2D();
        private bool firstTime3 = true;

        public static bool BallCutSituationr = false;
        public static Position2D BallCutPosr = new Position2D();
        public static int CutBallRobotIDCR4r = 1000;
        public static double balltimer = 0;
        public static double Robottimer = 0;
        public static Position2D InitialDefenderCutr = new Position2D();
        public static Position2D TargetDefenderCutr = new Position2D();
        public static bool getActiver = false;
        public static bool farFlagr = false;

        public static Position2D BallBreakPosr = new Position2D();
        private bool gotointermediateposBreak = false;
        private bool gotoBreakpos = false;


        private Position2D lastrobotpos = new Position2D();
        private Position2D lastintersect = new Position2D();

        private Position2D currentRobot = new Position2D();
        private Position2D lastposRobot = new Position2D();

        private static Position2D initialpos = new Position2D();

        int counterBalInFront = 0;
        private bool firstTtime = true;
        private double counter;
        private double Deccelcounter;
        private bool firstTime2 = true;
        private static bool weHaveInitialState = false;

        public void Run(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta)
        {
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
            double teta;
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

            Target = Cost(engine, Model, RobotID, TargetPos, Teta, inf, out teta);


            FreekickDefence.PreviousPositions[typeof(DefenderCornerRole4)] = Target;
            //------====||====------- DATA BRIDGE
            DataBridge.CornerRole4ID = RobotID;
            if (inf != null)
            {
                DataBridge.CornerRole4OppID = inf.OppID;
                DataBridge.CornerRole4IsBallTarget = (inf.TargetState.Type == ObjectType.Ball) ? true : false;
            }
            DataBridge.CornerRole4target = Target;

            Planner.Add(RobotID, Target, teta, PathType.UnSafe, false, false, true, false);

        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            double dist, dist2;
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
            bool NormalFromBehind = false;
            int? ballOwner = OppFreeKickDefenceUtils.GetOurBallOwner(engine, Model, RobotID, (DefenderStates)CurrentState, null);//engine.GameInfo.OurTeam.BallOwner;

            Obstacles robots = new Obstacles(Model);
            robots.AddObstacle(1, 0, 1, 0, new List<int>() { RobotID }, null);
            bool meet = robots.Meet(Model.BallState, new SingleObjectState(Position2D.Zero, Vector2D.Zero, 0f), .04);



            #region Collision Denied

            if (DataBridge.ourRobots.ContainsKey(RobotID))
            {
                initialpos = DataBridge.ourRobots[RobotID];
                weHaveInitialState = true;
            }
            else
            {
                weHaveInitialState = false;

            }
            if (lastRobotID != RobotID)
            {
                wehaveintersect = false;
            }
            Position2D intersect = new Position2D();
            double velocity = 0;
            double ballcoeff = 0;
            double robotCoeff = 0;
            double robotIntersectTime = 0;
            if (inf.DefenderPosition.HasValue && inf != null && weHaveInitialState)
            {
                intersect = IntersectFind(Model, RobotID, initialpos, inf.DefenderPosition.Value);
                velocity = Model.BallState.Speed.Size;
                ballcoeff = root(-.3, velocity, Model.BallState.Location.DistanceFrom(intersect));
                robotCoeff = predicttime(Model, RobotID, initialpos, intersect);
                robotIntersectTime = timeRobotToTargetInIntersect(Model, RobotID, initialpos, inf.DefenderPosition.Value, intersect);

                bool nb = robotCoeff - ballcoeff > -0.1 && robotIntersectTime - ballcoeff > -0.15 && robotIntersectTime - ballcoeff < 0.15;//B->N
                bool cn = robotCoeff - ballcoeff < -0.1 && robotCoeff - ballcoeff > -.2;//C->N
                bool bn = (robotCoeff - ballcoeff > -0.1 && (robotIntersectTime - ballcoeff > 0.15 + 0.05 || robotIntersectTime - ballcoeff < -0.15 - 0.05))
                                     || robotCoeff - ballcoeff < -0.2;
                //DrawingObjects.AddObject(new StringDraw("normal - break  : " + nb.ToString(), new Position2D(2, 0)), "56456464654564");
                //DrawingObjects.AddObject(new StringDraw("normal-cut  : " + cn.ToString(), new Position2D(2.1, 0)), "654564654");

                //DrawingObjects.AddObject(new StringDraw("breaknormal  : " + bn.ToString(), new Position2D(2.2, 0)), "56465456464");


                //DrawingObjects.AddObject(new StringDraw("RobotIntersect CR2: " + robotIntersectTime.ToString(), Model.OurRobots[RobotID].Location.Extend(-1.6, 0)), "4534534535437378");
                //DrawingObjects.AddObject(new Circle(intersect, .1, new Pen(Brushes.HotPink, .03f)), "7373734543543453453");
                //DrawingObjects.AddObject(new Circle(lastintersect, .1, new Pen(Brushes.Yellow, .03f)), "646556464654");
                //DrawingObjects.AddObject(new Circle(initialpos, .1, new Pen(Brushes.Yellow, .03f)), "73573737373737");
                //DrawingObjects.AddObject(new Circle(inf.DefenderPosition.Value, .1, new Pen(Brushes.Yellow, .03f)), "357537375374537573");
                //DrawingObjects.AddObject(new StringDraw("BallTime CR2: " + ballcoeff.ToString(), Model.BallState.Location.Extend(-1.4, 0)), "737583737373753");
                //DrawingObjects.AddObject(new StringDraw("RobotTime: " + robotCoeff.ToString(), Model.OurRobots[RobotID].Location.Extend(-1.5, 0)), "54275753753");
                //DrawingObjects.AddObject(new StringDraw("intersectballinner: " + Model.BallState.Speed.InnerProduct(intersect - Model.BallState.Location).ToString(), Model.BallState.Location.Extend(-1.7, 0)), "5646465665546");

            }
            bool ready4CUTandBREAK = false;
            if (inf != null && inf.DefenderPosition.HasValue && weHaveInitialState)
            {
                if (intersect != new Position2D(100, 100))
                {
                    if (ballcoeff > 0 && robotCoeff > 0 // time horizons
                        && Model.OurRobots[RobotID].Location.DistanceFrom(intersect) < Model.OurRobots[RobotID].Location.DistanceFrom(inf.DefenderPosition.Value) // target must be far than intersect /// changed& !meet // intersect doesnt have meet with other robots
                        && !GameParameters.IsInDangerousZone(intersect, false, .1, out dist, out dist2) // intersect is not in dangerzone 
                        && GameParameters.IsInField(intersect, 0) // intersect is in fiedld
                        && Model.BallState.Speed.Size > .5
                                       && Model.BallState.Speed.InnerProduct(intersect - Model.BallState.Location) > 0
                        )
                    {
                        ready4CUTandBREAK = true;

                        if (CurrentState != (int)DefenderStates.Break && CurrentState != (int)DefenderStates.Cut && robotCoeff - ballcoeff > -0.1 && robotIntersectTime - ballcoeff > -0.15 && robotIntersectTime - ballcoeff < 0.15 && firstTime2)
                        {
                            //DrawingObjects.AddObject(new StringDraw("firstBreak CR2: ", new Position2D(-1.2, 1)), "64564654");
                            lastintersect = intersect;
                            intermediatePos = intersect;
                            gotointermediateposBreak = true;
                            wehaveintersect = true;
                            firstTime2 = false;
                            gotoBreakpos = true;
                        }
                        else if (CurrentState != (int)DefenderStates.Cut && CurrentState != (int)DefenderStates.Break && robotCoeff - ballcoeff < -0.1 && robotCoeff - ballcoeff > -.2 && firstTtime)
                        {
                            //DrawingObjects.AddObject(new StringDraw("FirstCut CR2: ", new Position2D(-1.3, 1)), "1313132231323232");
                            lastrobotpos = Model.OurRobots[RobotID].Location;
                            lastintersect = intersect;
                            //DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, .7, new Pen(Brushes.Purple, .1f)), "654564654564");
                            intermediatePos = intersect;
                            gotointermediatepos = true;
                            firstTtime = false;
                            BallCutSituationr = true;
                            DataBridge.balltime = ballcoeff;
                            DataBridge.Robottime = robotCoeff;
                            wehaveintersect = true;
                            // its the permision of cut
                        }

                    }

                }
                if (gotointermediateposBreak) // update important parameters
                {
                    if (intermediatePos != new Position2D())
                    {
                        //DrawingObjects.AddObject(new StringDraw("SecondBreak CR2: ", new Position2D(-1.4, 1)), "564564654654564654");
                        TargetDefenderCutr = inf.DefenderPosition.Value;
                        InitialDefenderCutr = initialpos;

                        BallBreakPosr = initialpos + (intermediatePos - InitialDefenderCutr).GetNormalizeToCopy((intermediatePos - InitialDefenderCutr).Size - .3);

                    }
                    else
                    {
                        gotointermediateposBreak = false;
                        wehaveintersect = false;
                        firstTime2 = true;
                    }
                }
                else
                {
                    gotoBreakpos = false;
                }
                if (gotointermediatepos) // update important parameters
                {
                    if (intermediatePos != new Position2D())
                    {
                        DataBridge.BallCutSituationCR4 = true;
                        DataBridge.CutBallRobotIDCR4 = RobotID;
                        TargetDefenderCutr = inf.DefenderPosition.Value;
                        InitialDefenderCutr = initialpos;

                        BallCutPosr = initialpos + (intermediatePos - Model.OurRobots[RobotID].Location).GetNormalizeToCopy((intermediatePos - Model.OurRobots[RobotID].Location).Size);
                        CutBallRobotIDCR4r = RobotID;
                    }
                    else
                    {
                        gotointermediatepos = false;
                        wehaveintersect = false;
                        firstTtime = true;
                    }
                }
                else
                {
                    BallCutSituationr = false;
                }



            }

            //==========================ballDenied==============================
            #endregion
            #region Break
            if (CurrentState == (int)DefenderStates.Break)
            {

                if (ready4CUTandBREAK && robotCoeff - ballcoeff > -0.2 && robotCoeff - ballcoeff < -0.1 - 0.05)
                {
                    CurrentState = (int)DefenderStates.Cut;
                    BallCutSituationr = true;
                    DataBridge.BallCutSituationCR4 = true;
                    gotoBreakpos = false;
                }
                else if (
                                     inf == null || !inf.DefenderPosition.HasValue
                                    || (robotCoeff - ballcoeff > -0.1 && (robotIntersectTime - ballcoeff > 0.15 + 0.05 || robotIntersectTime - ballcoeff < -0.15 - 0.05))
                                    || robotCoeff - ballcoeff < -0.2
                                    || ballcoeff < 0 || robotCoeff < 0
                                    || Model.BallState.Speed.InnerProduct(lastintersect - Model.BallState.Location) < 0
                                    || Model.BallState.Speed.Size < .4
             || meet
                                    || GameParameters.IsInDangerousZone(intersect, false, .1, out dist, out dist2)
                                    || !GameParameters.IsInField(intersect, 0)

            )
                {
                    DrawingObjects.AddObject(new StringDraw("OutOfBreakBreak CR2: ", new Position2D(-1.8, 1)), "654654645");
                    CurrentState = (int)DefenderStates.Normal;
                    gotoBreakpos = false;
                }



            }
            #endregion
            #region CUT
            else if (CurrentState == (int)DefenderStates.Cut)
            {
                if (DataBridge.CutBallRobotIDCR1 != RobotID)
                {
                    CurrentState = (int)DefenderStates.Normal;
                }
                if (ready4CUTandBREAK && robotCoeff - ballcoeff > -0.1 && (robotIntersectTime - ballcoeff > -0.15 && robotIntersectTime - ballcoeff < 0.15))
                {
                    DrawingObjects.AddObject(new StringDraw("SecondBreak CR2: ", new Position2D(1.5, 1)), "654654654564");
                    BallBreakPosr = initialpos + (intermediatePos - InitialDefenderCutr).GetNormalizeToCopy((intermediatePos - InitialDefenderCutr).Size - .3);
                    gotoBreakpos = true;
                    CurrentState = (int)DefenderStates.Break;
                }
                else if (
                     inf == null || !inf.DefenderPosition.HasValue ||
                     robotCoeff - ballcoeff < -.2 - .1
                    || (-.1 + .05 < robotCoeff - ballcoeff && !(-0.15 < robotIntersectTime - ballcoeff && robotIntersectTime - ballcoeff < 0.15))
                || ballcoeff < 0 || robotCoeff < 0
                || Model.BallState.Speed.InnerProduct(lastintersect - Model.BallState.Location) < 0
                || Model.BallState.Speed.Size < .4
                    || meet
                    || GameParameters.IsInDangerousZone(intersect, false, .1, out dist, out dist2)
                    || !GameParameters.IsInField(intersect, 0)
                    )
                {
                    CurrentState = (int)DefenderStates.Normal;
                    DataBridge.BallCutSituationCR4 = false;
                    BallCutSituationr = false;
                }


            }
            #endregion
            #region Normal
            else if (CurrentState == (int)DefenderStates.Normal)
            {
                if (DataBridge.BallBehind && DataBridge.BallBehindID == RobotID)
                {
                    CurrentState = (int)DefenderStates.Behind;
                }
                else if (DataBridge.CutBallRobotIDCR4 == RobotID && DataBridge.BallCutSituationCR4 && DataBridge.staticCutAssign)
                {
                    CurrentState = (int)DefenderStates.Cut;
                }
                else if (OppFreeKickDefenceUtils.BallKickedToGoal(engine, Model))
                {
                    CurrentState = (int)DefenderStates.KickToGoal;
                }
                else if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.CornerInPenaltyAreaMargin, out dist, out dist2))
                {
                    CurrentState = (int)DefenderStates.InPenaltyArea;
                }
                else if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind) && !DataBridge.BallBehind)//ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                {
                    CurrentState = (int)DefenderStates.Behind;
                }
                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
                {
                    CurrentState = (int)DefenderStates.BallInFront;
                }
                else if (BallCutSituationr && !DataBridge.staticCutAssign)
                {
                    CurrentState = (int)DefenderStates.Cut;
                }
                //else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.Normal2OppInDangerZoneMargin, out dist, out dist2))//  inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) < tresh )
                //{
                //    CurrentState = (int)DefenderStates.OppIndDangerZone;
                //}
            }
            #endregion
            #region Behind
            else if (CurrentState == (int)DefenderStates.Behind)
            {
                OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, true, ref NormalFromBehind);

                if (DataBridge.BallBehind && DataBridge.BallBehindID == RobotID && GameParameters.IsInDangerousZone(Model.BallState.Location, false, OppFreeKickDefenceUtils.Behind2InPenaltyAreaMargin, out dist, out dist2))
                {
                    CurrentState = (int)DefenderStates.InPenaltyArea;
                }
                else if (BallCutSituationr && !DataBridge.staticCutAssign)
                {
                    CurrentState = (int)DefenderStates.Cut;
                }
                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
                {
                    CurrentState = (int)DefenderStates.BallInFront;
                }
                else if (NormalFromBehind)
                {
                    CurrentState = (int)DefenderStates.Normal;
                }
            }
            #endregion
            #region In Penalty Area
            else if (CurrentState == (int)DefenderStates.InPenaltyArea)
            {
                if (DataBridge.CutBallRobotIDCR4 == RobotID && DataBridge.BallCutSituationCR4 && DataBridge.staticCutAssign)
                {
                    CurrentState = (int)DefenderStates.Cut;
                }
                else if (BallCutSituationr && !DataBridge.staticCutAssign)
                {
                    CurrentState = (int)DefenderStates.Cut;
                }
                else if (!GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.CornerInPenaltyAreaMargin + .02, out dist, out dist2))
                {
                    if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind) && !DataBridge.BallBehind)
                    {
                        CurrentState = (int)DefenderStates.Behind;
                    }
                    else
                        CurrentState = (int)DefenderStates.Normal;
                }

            }
            #endregion
            #region KickToGoal
            else if (CurrentState == (int)DefenderStates.KickToGoal)
            {
                if (!OppFreeKickDefenceUtils.BallKickedToGoal(engine, Model))
                {
                    if (DataBridge.CutBallRobotIDCR4 == RobotID && DataBridge.BallCutSituationCR4 && DataBridge.staticCutAssign)
                    {
                        CurrentState = (int)DefenderStates.Cut;
                    }

                    else if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.CornerInPenaltyAreaMargin, out dist, out dist2))
                    {
                        CurrentState = (int)DefenderStates.InPenaltyArea;
                    }
                    else if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind) && !DataBridge.BallBehind)//hadi//ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                    {
                        CurrentState = (int)DefenderStates.Behind;
                    }
                    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
                    {
                        CurrentState = (int)DefenderStates.BallInFront;
                    }
                    else if (BallCutSituationr && !DataBridge.staticCutAssign)
                    {
                        CurrentState = (int)DefenderStates.Cut;
                    }
                    //else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, 0.1, out dist, out dist2))// inf.TargetState.Location.DistanceFrom ( GameParameters.OurGoalCenter ) - Model.OurRobots [RobotID].Location.DistanceFrom ( GameParameters.OurGoalCenter ) < tresh )
                    //{
                    //    CurrentState = (int)DefenderStates.OppIndDangerZone;
                    //}
                    else
                        CurrentState = (int)DefenderStates.Normal;
                }
            }
            #endregion
            #region BallInFront
            else if (CurrentState == (int)DefenderStates.BallInFront)
            {
                counterBalInFront++;
                if (DataBridge.BallBehindID == RobotID && DataBridge.BallBehind)
                {
                    CurrentState = (int)DefenderStates.Behind;
                }
                if (DataBridge.CutBallRobotIDCR4 == RobotID && DataBridge.BallCutSituationCR4)
                {
                    CurrentState = (int)DefenderStates.Cut;
                }
                else if (engine.Status == GameStatus.Stop || !ControlParameters.BallIsMoved)
                {
                    CurrentState = (int)DefenderStates.Normal;
                }
                else if (counterBalInFront > 30)
                {
                    if (!ballOwner.HasValue || (ballOwner.HasValue && ballOwner.Value != RobotID))
                    {
                        if (OppFreeKickDefenceUtils.BallKickedToGoal(engine, Model))
                        {
                            CurrentState = (int)DefenderStates.KickToGoal;
                        }
                        else if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.CornerInPenaltyAreaMargin, out dist, out dist2))
                        {
                            CurrentState = (int)DefenderStates.InPenaltyArea;
                        }
                        else if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind) && !DataBridge.BallBehind)
                        {
                            CurrentState = (int)DefenderStates.Behind;
                        }
                        //else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Type == ObjectType.Opponent && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, 0.1, out dist, out dist2))
                        //{
                        //    CurrentState = (int)DefenderStates.OppIndDangerZone;
                        //}
                        else if (BallCutSituationr && !DataBridge.staticCutAssign)
                        {
                            CurrentState = (int)DefenderStates.Cut;
                        }
                        else
                            CurrentState = (int)DefenderStates.Normal;
                    }
                }
            }
            #endregion
            #region OppInDangerZone // we dont have it
            //else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
            //{
            //    if (DataBridge.CutBallRobotIDCR4 == RobotID && DataBridge.BallCutSituationCR4)
            //    {
            //        CurrentState = (int)DefenderStates.Cut;
            //    }
            //    else if (inf == null || (inf != null && inf.TargetState.Type != ObjectType.Opponent) || (inf != null && GameParameters.IsInDangerousZone(inf.TargetState.Location, false, OppFreeKickDefenceUtils.OppDanger2OppDangerMargin, out dist, out dist2)))
            //    {
            //        if (GameParameters.IsInDangerousZone(ballState.Location, false, OppFreeKickDefenceUtils.OppInDanger2InPenaltyAreaMargin, out dist, out dist2))
            //        {
            //            CurrentState = (int)DefenderStates.InPenaltyArea;
            //        }
            //        else if (OppFreeKickDefenceUtils.BallInBehind(engine, Model, RobotID, false, ref NormalFromBehind))
            //        {
            //            CurrentState = (int)DefenderStates.Behind;
            //        }
            //        else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
            //        {
            //            CurrentState = (int)DefenderStates.BallInFront;
            //        }
            //        else
            //            CurrentState = (int)DefenderStates.Normal;
            //    }
            //    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && ControlParameters.BallIsMoved)
            //    {
            //        CurrentState = (int)DefenderStates.BallInFront;
            //    }
            //}
            #endregion


            //ball in front reset state
            if (CurrentState != (int)DefenderStates.BallInFront)
                counterBalInFront = 0;

            DrawingObjects.AddObject(new StringDraw(((DefenderStates)CurrentState).ToString(), Model.OurRobots[RobotID].Location + new Vector2D(0.25, 0)), "Def2");
            if (!calculateCost)
                FreekickDefence.CurrentStates[this] = CurrentState;
            lastRobotID = RobotID;
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }
        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
            if (inf != null)
            {
                double teta;
                calculateCost = true;
                //     DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles);

                CurrentState = (FreekickDefence.CurrentStates.ContainsKey(this)) ? FreekickDefence.CurrentStates[this] : CurrentState;
                Position2D pos = Cost(engine, Model, RobotID, inf.DefenderPosition.Value, inf.Teta, inf, out teta);
                double cost = pos.DistanceFrom(Model.OurRobots[RobotID].Location);
                return cost * cost;
            }
            return 100;
        }
        public Position2D Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta, DefenceInfo inf, out double teta)
        {
            Position2D target = Position2D.Zero;
            teta = 180;
            if (CurrentState == (int)DefenderStates.Cut)
            {
                DataBridge.BallCutSituationCR4 = true;
                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .6)
                {
                    DataBridge.getActive = true;
                }
                target = BallCutPosr;
                teta = -(180 - Math.Abs(Model.BallState.Speed.AngleInDegrees));

            }
            else if (CurrentState == (int)DefenderStates.Break)
            {
                target = BallBreakPosr;
                teta = -(180 - Math.Abs(Model.BallState.Speed.AngleInDegrees));
            }
            else if (CurrentState == (int)DefenderStates.InPenaltyArea)
            {
                target = OppFreeKickDefenceUtils.MarkFront(engine, Model, RobotID, inf, FreekickDefence.AdditionalSafeRadi, out teta);
            }
            else if (CurrentState == (int)DefenderStates.Behind)
            {
                target = OppFreeKickDefenceUtils.BehindSatate(engine, Model, inf, RobotID, out teta, FreekickDefence.CurrentStates);
            }
            else if (CurrentState == (int)DefenderStates.Normal)
            {
                target = TargetPos;
                Vector2D vec = target - Model.OurRobots[RobotID].Location;
                if (inf != null)
                    if (inf.TargetState.Speed.Size > 1)
                        target = target + vec.GetNormalizeToCopy(Math.Min(inf.TargetState.Speed.Size * 0.2, 0.08));
                teta = Teta;
            }
            else if (CurrentState == (int)DefenderStates.KickToGoal)
            {
                target = OppFreeKickDefenceUtils.Dive(engine, Model, RobotID);
                teta = Model.OurRobots[RobotID].Angle.Value;
            }
            else if (CurrentState == (int)DefenderStates.BallInFront)
            {
                target = OppFreeKickDefenceUtils.GetBackBallPoint(engine, Model, RobotID, out teta);
            }
            else if (CurrentState == (int)DefenderStates.OppIndDangerZone)
            {
                if (inf != null)
                {
                    int? oppid = inf.OppID;
                    if (oppid.HasValue)
                    {
                        Vector2D vec = (Model.Opponents[oppid.Value].Location - Model.OurRobots[RobotID].Location);
                        target = Model.Opponents[oppid.Value].Location + vec.GetNormalizeToCopy(0.2);
                        teta = (target - GameParameters.OurGoalCenter).AngleInDegrees;//(target - Model.OurRobots[RobotID].Location).AngleInDegrees + 180;
                    }
                }
            }
            if ((CurrentState == (int)DefenderStates.Normal && inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X))
            {
                target = new Position2D(GameParameters.OurGoalCenter.X - .1, target.Y);
            }
            return target;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            if (!FreekickDefence.switchAllMode)
            {
                if (CurrentState == (int)DefenderStates.Cut && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .6 && FreekickDefence.WeAreInCorner)
                {
                    List<RoleBase> res = new List<RoleBase>() { new DefenderCornerRole4() };
                    if (FreekickDefence.DefenderCornerRole4ToActive)
                    {
                        res.Add(new ActiveRole());
                    }
                    return res;
                }
                else
                {
                    if (FreekickDefence.WeAreInCorner && FreekickDefence.BallIsMoved)
                    {
                        List<RoleBase> res = new List<RoleBase>() { new DefenderCornerRole1(), 
                                                        new DefenderCornerRole2(), 
                                                        new DefenderCornerRole3(),
                                                        new DefenderMarkerRole2(),
                                                        new DefenderCornerRole4(),
                                                        new DefenderMarkerRole(),
            new NewDefenderMrkerRole(),
            new NewDefenderMarkerRole2()
            };
                        if (FreekickDefence.DefenderCornerRole4ToActive)
                        {
                            res.Add(new ActiveRole());
                        }
                        return res;
                    }
                    else
                    {
                        List<RoleBase> res = new List<RoleBase>() { new DefenderCornerRole1(), 
                                                        new DefenderCornerRole2(), 
                                                        new DefenderCornerRole3(),
                                                        new RegionalDefenderRole(),
                                                        new DefenderMarkerRole2(),
                                                        new DefenderCornerRole4(),
                                                        new DefenderMarkerRole(),
            new NewDefenderMrkerRole(),
            new NewDefenderMarkerRole2(),
            new NewDefenderMarkerRole3()
            };
                        if (FreekickDefence.DefenderCornerRole4ToActive)
                        {
                            res.Add(new ActiveRole());
                        }
                        return res;
                    }
                }
            }
            else
            {
                if (false && CurrentState == (int)DefenderStates.Cut && Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location) < .6 && FreekickDefence.WeAreInCorner)
                {
                    List<RoleBase> res = new List<RoleBase>() { new ActiveRole(), new DefenderCornerRole4() };
                    return res;
                }
                else
                {
                    List<RoleBase> res = new List<RoleBase>();
                    res.Add(new DefenderCornerRole1());
                    res.Add(new DefenderCornerRole2());
                    res.Add(new DefenderCornerRole3());
                    res.Add(new DefenderMarkerRole2());
                    res.Add(new DefenderCornerRole4());
                    res.Add(new DefenderMarkerRole());
                    res.Add(new NewDefenderMrkerRole());
                    res.Add(new NewDefenderMarkerRole2());
                    res.Add(new RegionalDefenderRole());
                    res.Add(new RegionalDefenderRole2());
                    if (FreekickDefence.DefenderCornerRole4ToActive)
                    {
                        res.Add(new ActiveRole());
                    }
                    return res;
                }
            }
        }
        //List<RoleBase> res = new List<RoleBase>() { 
        ////////    new DefenderCornerRole1(), 
        ////////    new DefenderCornerRole2(), 
        ////////    new DefenderCornerRole3(),
        ////////                    new DefenderCornerRole4(),
        ////////    new RegionalDefenderRole() 
        ////////};
        ////////if (FreekickDefence.SwitchDefender42Marker1)
        ////////{
        ////////    res.Add(new DefenderMarkerRole());
        ////////}
        ////////if (FreekickDefence.SwitchDefender42Marker2)
        ////////{
        ////////    res.Add(new DefenderMarkerRole2());
        ////////}
        ////////if (FreekickDefence.SwitchDefender42Marker3)
        ////////{
        ////////    res.Add(new DefenderMarkerRole3());
        ////////}

        ////////if (FreekickDefence.LastSwitchDefender42Marker1)//New IO2014
        ////////{
        ////////    res.Add(new DefenderMarkerRole());
        ////////}
        ////////if (FreekickDefence.LastSwitchDefender42Marker2)//New IO2014
        ////////{
        ////////    res.Add(new DefenderMarkerRole2());
        ////////}
        ////////if (FreekickDefence.LastSwitchDefender42Marker3)//New IO2014
        ////////{
        ////////    res.Add(new DefenderMarkerRole3());
        ////////}
        //return res;
        //}

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        private Position2D IntersectFind(WorldModel model, int RobotID, Position2D target)
        {
            Position2D robotSpeedPos = model.OurRobots[RobotID].Location + model.OurRobots[RobotID].Speed;
            Position2D ballspeedpos = model.BallState.Location + model.BallState.Speed;

            double x4 = target.X;
            double x3 = model.OurRobots[RobotID].Location.X;
            double y4 = target.Y;
            double y3 = model.OurRobots[RobotID].Location.Y;
            double x2 = ballspeedpos.X;
            double y2 = ballspeedpos.Y;
            double x1 = model.BallState.Location.X;
            double y1 = model.BallState.Location.Y;
            //double x = (((((x1 * y2) - (y1 * x2)) * (x3 - x4)) - ((x1 - x2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));
            //double y = (((((x1 * y2) - (y1 * x2)) * (y3 - y4)) - ((y1 - y2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));

            Line first = new Line(new Position2D(x1, y1), new Position2D(x2, y2));
            Line second = new Line(new Position2D(x3, y3), new Position2D(x4, y4));
            Position2D intersect = new Position2D();
            if (first.IntersectWithLine(second).HasValue)
                intersect = first.IntersectWithLine(second).Value;
            else
            {
                intersect = new Position2D(100, 100);
            }
            return intersect;
        }

        private double motionTime(int RobotID, WorldModel Model, Position2D target, Position2D lastRobotPos)
        {
            Position2D RobotPos = Model.OurRobots[RobotID].Location;
            double distToTarget = RobotPos.DistanceFrom(target);
            double timeRobot = (Planner.GetMotionTime(Model, RobotID, RobotPos, target, ActiveParameters.RobotMotionCoefs) * StaticVariables.FRAME_PERIOD) - ((ControlParameters.MaxSpeed - ((lastRobotPos.DistanceFrom(RobotPos)) / StaticVariables.FRAME_PERIOD)) / distToTarget);
            return timeRobot;
        }

        private double predicttime(WorldModel Model, int RobotID, Position2D initialpos, Position2D lastpos)
        {
            Position2D initialstate = initialpos;
            Position2D target = lastpos;

            if (firstTime3)
            {
                firstTime3 = false;
                lasttargetPoint = lastpos;
            }
            if (target.DistanceFrom(lasttargetPoint) > .05)
            {
                counter = 0;
                Deccelcounter = 0;
                firstTime3 = true;
            }
            Position2D currentPos = Model.OurRobots[RobotID].Location;
            double deccelDX = Math.Min(1.09, .54 * initialstate.DistanceFrom(target));
            double daccel = Math.Min(0.942, .46 * initialstate.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);
            double Va = 3.14 * (counter * StaticVariables.FRAME_PERIOD);
            double ta = root(3.14, Va, daccel - Model.OurRobots[RobotID].Location.DistanceFrom(initialstate));
            double tc = (Model.OurRobots[RobotID].Location.DistanceFrom(target) - deccelDX) / vmax;
            double tc2 = (initialstate.DistanceFrom(target) - deccelDX - daccel) / vmax;

            double td = (vmax - 3.04 * Deccelcounter * 0.016) / 3.04;
            double Td = Math.Min(.850, vmax / 3.04);
            double total = 0;


            counter++;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < deccelDX)
            {
                Deccelcounter++;
            }
            if (Deccelcounter > 10)
            {
                int g = 0;
            }
            if (initialstate.DistanceFrom(target) > deccelDX + daccel)
            {

                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //1
                    total = ta + tc2 + Td;
                    //DrawingObjects.AddObject(new StringDraw("1", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "2145665445496789456");
                }
                if (currentPos.DistanceFrom(target) > deccelDX && currentPos.DistanceFrom(initialstate) > daccel)
                {
                    //4
                    total = tc + Td;
                    //DrawingObjects.AddObject(new StringDraw("4", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "54975645696854645664564456");
                }
                if (currentPos.DistanceFrom(target) < deccelDX)
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "546464564645456984566");
                }
            }
            else
            {
                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //2
                    total = ta + Td;
                    //DrawingObjects.AddObject(new StringDraw("2", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "9876454652132");
                }
                else
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "56413121364564");
                }
            }
            lasttarget = target;
            lastinitpos = initialstate;
            return total;

        }


        private double predicttime(WorldModel Model, int RobotID, Position2D initialpos, Position2D lastpos, bool inittarget)
        {
            Position2D initialstate = initialpos;
            Position2D target = lastpos;

            if (firstTime3)
            {
                firstTime3 = false;
                lasttargetPoint = lastpos;
            }
            if (target.DistanceFrom(lasttargetPoint) > .05)
            {
                counter = 0;
                Deccelcounter = 0;
                firstTime3 = true;
            }
            Position2D currentPos = initialpos;
            double deccelDX = Math.Min(1.09, .54 * initialstate.DistanceFrom(target));
            double daccel = Math.Min(0.942, .46 * initialstate.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * daccel);
            double Va = 3.14 * (counter * StaticVariables.FRAME_PERIOD);
            double ta = root(3.14, Va, daccel - currentPos.DistanceFrom(initialstate));
            double tc = (currentPos.DistanceFrom(target) - deccelDX) / vmax;
            double tc2 = (initialstate.DistanceFrom(target) - deccelDX - daccel) / vmax;

            double td = (vmax - 3.04 * Deccelcounter * 0.016) / 3.04;
            double Td = Math.Min(.850, vmax / 3.04);
            double total = 0;


            counter++;
            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < deccelDX)
            {
                Deccelcounter++;
            }
            if (Deccelcounter > 10)
            {
                int g = 0;
            }
            if (initialstate.DistanceFrom(target) > deccelDX + daccel)
            {

                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //1
                    total = ta + tc2 + Td;
                    //DrawingObjects.AddObject(new StringDraw("1", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "2145665445496789456");
                }
                if (currentPos.DistanceFrom(target) > deccelDX && currentPos.DistanceFrom(initialstate) > daccel)
                {
                    //4
                    total = tc + Td;
                    //DrawingObjects.AddObject(new StringDraw("4", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "54975645696854645664564456");
                }
                if (currentPos.DistanceFrom(target) < deccelDX)
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "546464564645456984566");
                }
            }
            else
            {
                if (currentPos.DistanceFrom(initialstate) < daccel)
                {
                    //2
                    total = ta + Td;
                    //DrawingObjects.AddObject(new StringDraw("2", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "9876454652132");
                }
                else
                {
                    //3
                    total = td;
                    //DrawingObjects.AddObject(new StringDraw("3", Model.OurRobots[RobotID].Location.Extend(1.1, 0)), "56413121364564");
                }
            }
            lasttarget = target;
            lastinitpos = initialstate;
            return total;

        }

        double predicttime(WorldModel model, int RobotID, Position2D init, Position2D target, Position2D intersect)
        {
            Position2D currentPos = model.OurRobots[RobotID].Location;
            double deccelDX = Math.Min(1.09, .54 * init.DistanceFrom(target));
            double accelDx = Math.Min(0.942, .46 * init.DistanceFrom(target));
            double vmax = Math.Sqrt(2 * 3.14 * accelDx);
            double adeccel = 3.04;
            double aaccel = 3.14;

            double deltaXIntersectTarget = intersect.DistanceFrom(target);

            double coeff1 = deltaXIntersectTarget / deccelDX;
            double v0deccel = vmax * coeff1;
            double tTemp = v0deccel / adeccel;
            double tdeccel = predicttime(model, RobotID, init, target, true) - tTemp;

            double deltaxInitialIntersect = init.DistanceFrom(intersect);
            double coeff2 = deltaxInitialIntersect / accelDx;

            double V0accel = coeff2 * vmax;
            double taccel = V0accel / aaccel;

            double tcruise = ((deltaxInitialIntersect - accelDx) / vmax) + (vmax / accelDx);

            double deltaXInitialTarget = init.DistanceFrom(target);
            double ttotal = 0;
            if (deltaXIntersectTarget < accelDx + deccelDX) // Accel - Deccel
            {
                if (deltaXIntersectTarget > deccelDX)
                {
                    ttotal = taccel;
                }
                else
                {
                    ttotal = tdeccel;
                }
            }
            else // Accel - Cruise - Deccel
            {
                if (deltaxInitialIntersect < accelDx)
                {
                    ttotal = taccel;
                }
                else if (deltaXIntersectTarget < deccelDX)
                {
                    ttotal = tdeccel;
                }
                else
                {
                    ttotal = tcruise;
                }
            }
            return ttotal;
        }

        double timeRobotToTargetInIntersect(WorldModel model, int RobotID, Position2D init, Position2D target, Position2D intersect)
        {
            double timeInittarget = predicttime(model, RobotID, init, target, true);
            double timeInitIntersect = predicttime(model, RobotID, init, target, intersect);
            double timeIntersecttarget = timeInittarget - timeInitIntersect;
            double timeRobotTarget = predicttime(model, RobotID, init, target);
            double timeRobotIntesect = timeRobotTarget - timeIntersecttarget;
            return timeRobotIntesect;
        }
        private Position2D IntersectFind(WorldModel model, int RobotID, Position2D initpoint, Position2D target)
        {
            Position2D robotSpeedPos = model.OurRobots[RobotID].Location + model.OurRobots[RobotID].Speed;
            Position2D ballspeedpos = model.BallState.Location + model.BallState.Speed;

            double x4 = target.X;

            double x3 = initpoint.X;
            double y4 = target.Y;
            double y3 = initpoint.Y;
            double x2 = ballspeedpos.X;
            double y2 = ballspeedpos.Y;
            double x1 = model.BallState.Location.X;
            double y1 = model.BallState.Location.Y;
            //double x = (((((x1 * y2) - (y1 * x2)) * (x3 - x4)) - ((x1 - x2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));
            //double y = (((((x1 * y2) - (y1 * x2)) * (y3 - y4)) - ((y1 - y2) * ((x3 * y4) - (y3 * x4)))) / ((x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4)));

            Line first = new Line(new Position2D(x1, y1), new Position2D(x2, y2));
            Line second = new Line(new Position2D(x3, y3), new Position2D(x4, y4));
            Position2D intersect = new Position2D();
            if (first.IntersectWithLine(second).HasValue)
                intersect = first.IntersectWithLine(second).Value;
            else
            {
                intersect = new Position2D(100, 100);
            }
            return intersect;
        }

        static double root(double a, double initialV, double deltaX)
        {
            double t = 0;
            double delta = (initialV * initialV) - (2 * a * -deltaX);
            if (delta == 0)
            {
                t = -initialV / (.5 * a);
            }
            if (delta > 0)
            {

                double t1 = (-initialV - Math.Sqrt(delta)) / a;
                double t2 = (-initialV + Math.Sqrt(delta)) / a;
                if (t2 > 0 && t1 < 0)
                    t = t2;
                else if (t1 > 0 && t2 < 0)
                    t = t1;
                else if (t1 > 0 && t2 > 0)
                    if (t1 < t2)
                        t = t1;
                    else
                        t = t2;
            }
            if (delta < 0)
                return -1000;
            return t;
        }
    }
}
