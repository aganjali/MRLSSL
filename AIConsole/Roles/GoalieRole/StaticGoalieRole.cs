using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.AIConsole.Skills.GoalieSkills;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    class StaticGoalieRole : RoleBase
    {

        public InPenaltyMode inpenaltyMode;
        public Position2D Target = new Position2D();
        bool calculateCost = false;
        Position2D intermediatePos = new Position2D();

        bool gotoperp = false;

        public static Position2D lasttarget = new Position2D();
        public static Position2D lastinitpos = new Position2D();
        private static bool wehaveintersect = false;
        private static int lastRobotID;
        private bool gotointermediatepos = true;
        private bool gotointermediateposBreak = true;
        private bool gotoBreakpos = false;

        private static int kickerOpponent = 0;

        public static Position2D lasttargetPoint = new Position2D();
        private bool firstTime3 = true;

        public static bool BallCutSituationr = false;
        public static Position2D BallCutPosr = new Position2D();
        public static Position2D BallBreakPosr = new Position2D();
        public static int CutBallRobotIDr = 1000;
        public static double balltimer = 0;
        public static double Robottimer = 0;
        public static Position2D InitialDefenderCutr = new Position2D();
        public static Position2D TargetDefenderCutr = new Position2D();
        public static bool getActiver = false;
        public static bool farFlagr = false;

        private Position2D lastrobotpos = new Position2D();
        private Position2D lastintersect = new Position2D();

        private Position2D currentRobot = new Position2D();
        private Position2D lastposRobot = new Position2D();

        private static Position2D initialpos = new Position2D();

        private static Position2D lastPositionInPredive = new Position2D();

        int counterBalInFront = 0;
        private bool firstTtime = true;
        private double counter;
        private double counter1 = 0;
        private double Deccelcounter;
        private bool firstTime2 = true;


        private RBstate LastRB = RBstate.Ball;
        private bool Defender1Delayed = false;
        private bool Defender2Delayed = false;

        int currentState;
        Position2D StaticDefenderCurrentPos = new Position2D();
        Position2D StaticDefender2CurrentPos = new Position2D();
        Position2D GoalieCurrentPos = new Position2D();
        Position2D GoalieTargetPos = new Position2D();

        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState BallState = new SingleObjectState();

        int? StaticDefender1IDG = null;
        int? StaticDefender2IDG = null;

        Queue<double> robotAngle = new Queue<double>(100);
        Queue<double> robotDistance = new Queue<double>(100);
        Queue<Position2D> ballstates = new Queue<Position2D>(100);



        bool cut = false;//WC2017
        Position2D lastPosNormal = Position2D.Zero;//WC2017
        double ballcoeff;
        double robotCoeff;
        Position2D intersectTime;

        public SingleWirelessCommand perform(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState defenceSate, int? StaticDefender1ID, int? StaticDefender2ID)
        {
            StaticDefender1IDG = StaticDefender1ID;
            StaticDefender2IDG = StaticDefender2ID;
            if (DefenceTest.BallTest)
            {
                ballState = DefenceTest.currentBallState;
                BallState = DefenceTest.currentBallState;
            }
            else
            {
                ballState = Model.BallState;
                BallState = Model.BallState;
            }
            if (StaticDefender1ID.HasValue)
                StaticDefenderCurrentPos = Model.OurRobots[StaticDefender1ID.Value].Location;
            if (StaticDefender2ID.HasValue)
                StaticDefender2CurrentPos = Model.OurRobots[StaticDefender2ID.Value].Location;
            int? id = StaticRB(engine, Model);
            DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");

            defenceSate = (id.HasValue) ? Model.Opponents[id.Value] : ballState;
            SingleWirelessCommand SWc = new SingleWirelessCommand();
            #region Normal
            if (CurrentState == (int)GoalieStates.Normal)
            {
                bool DangerousState = false;
                Position2D ballLoc = Model.BallState.Location;
                Position2D target;
                if (ballLoc.X > 4.3 && ballLoc.Y < 1.2 && ballLoc.Y > -1.2)
                {
                    List<Position2D> intervalPos = new List<Position2D>();
                    double DangerousTresh = GameParameters.DefenceAreaWidth / 2;

                    intervalPos = engine.GameInfo.IntervalSelect(engine.GameInfo.GetVisibleIntervals(Model, ballLoc, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, false, true, null, new int[1] { RobotID }));
                    if (intervalPos.Count > 1 && intervalPos[0].DistanceFrom(intervalPos[1]) > MotionPlannerParameters.RobotRadi * 2)
                    {
                        Position2D center = Position2D.Interpolate(intervalPos[0], intervalPos[1], 0.5);
                        target = center + (ballLoc - center).GetNormalizeToCopy(DangerousTresh);
                    }
                    else
                    {
                        target = GameParameters.OurGoalCenter + (ballLoc - GameParameters.OurGoalCenter).GetNormalizeToCopy(DangerousTresh);
                    }
                    Planner.Add(RobotID, target, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);

                }
                Position2D postoGo = GameParameters.OurGoalCenter + (GameParameters.OurGoalCenter - defenceSate.Location).GetNormalizeToCopy(-0.4);
                //WC2017
                if (postoGo.X > GameParameters.OppGoalCenter.X - 0.11)
                {
                    Line l1 = new Line(postoGo, defenceSate.Location);
                    Position2D? p = l1.IntersectWithLine(new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight));
                    if (p.HasValue)
                        postoGo = p.Value + (defenceSate.Location - p.Value).GetNormalizeToCopy(0.11);
                }
                double x = postoGo.X;
                x = Math.Min(GameParameters.OurGoalCenter.X - 0.11, x);
                //
                postoGo = new Position2D(x, postoGo.Y);

                //WC 2017
                #region cut
                if (initialpos == Position2D.Zero)
                {
                    initialpos = Model.BallState.Location;
                }
                Line goalerLine = new Line(Model.OurRobots[RobotID].Location, postoGo);
                Line ballLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed);
                Position2D? PosIntersect = ballLine.IntersectWithLine(goalerLine);

                double xx, yy;

                initialpos = Model.BallState.Location;
                intersectTime = IntersectFind(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location);
                double velocity = Model.BallState.Speed.Size;
                ballcoeff = root(-.3, velocity, Model.BallState.Location.DistanceFrom(intersectTime));
                robotCoeff = predicttime(Model, RobotID, initialpos, intersectTime);
                double robotIntersectTime = timeRobotToTargetInIntersect(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location, intersectTime);

                cut = false;
                if (!BallKickedToOurGoal(Model) && PosIntersect.HasValue && (Model.OurRobots[RobotID].Location - PosIntersect.Value).GetNormnalizedCopy().InnerProduct((postoGo - PosIntersect.Value).GetNormnalizedCopy()) < -0.1 &&
                    Model.BallState.Speed.Size > 0.5 && (PosIntersect.Value - Model.BallState.Location).GetNormnalizedCopy().InnerProduct(Model.BallState.Speed.GetNormnalizedCopy()) > 0.1 && Model.OurRobots[RobotID].Location.X < PosIntersect.Value.X)// (PosIntersect.Value.X > Model.OurRobots[RobotID].Location.X + 0.9) && GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.2, out xx, out yy))//&& (PosTarget.X > Model.OurRobots[RobotID].Location.X + 0.9))//(PosTarget.DistanceFrom(GameParameters.OurGoalCenter) < Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)))
                {
                    if (ballcoeff > 0 && intersectTime != (new Position2D(100, 100)) && (((robotCoeff - ballcoeff) > -0.5 && (robotCoeff - ballcoeff) < 0.5)))
                    {
                        lastPosNormal = Model.OurRobots[RobotID].Location;
                        cut = true;
                    }

                    if (cut)
                    {
                        if (GameParameters.IsInDangerousZone(lastPosNormal, false, 0.2, out xx, out yy) && lastPosNormal != Position2D.Zero)
                            postoGo = lastPosNormal;
                        DrawingObjects.AddObject(new StringDraw("cut = true , cutpos = " + PosIntersect.Value.toString(), GameParameters.OurGoalCenter.Extend(1, 0)), "sd3fs2d13ah321hg3h21dfs2ghdfgh");
                    }
                }
                else
                {
                    cut = false;
                    lastPosNormal = Position2D.Zero;
                }

                if (cut)
                {
                    DrawingObjects.AddObject(new Circle(lastPosNormal, 0.1, new System.Drawing.Pen(System.Drawing.Color.Red, 0.02f)), "sdf32s1df32ds");
                }
                if ((robotCoeff - ballcoeff) > 0.6 || (robotCoeff - ballcoeff) < -0.6)
                {
                    cut = false;
                    lastPosNormal = Position2D.Zero;
                }
                SWc = GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, postoGo, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false, false, 3, false);
                var s = new SingleObjectState(postoGo, defenceSate.Speed, (float)(defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees);
                GoalieTargetPos = postoGo;
                Planner.Add(RobotID, s, false);



                if (!cut)
                {
                    lastPosNormal = Position2D.Zero;
                }

                bool debug = true;
                if (debug)
                {
                    DrawingObjects.AddObject(new StringDraw("time = " + (robotCoeff - ballcoeff).ToString(), new Position2D(5, 2)), "3asf21asf21a3sdsrytwr68hj514trehj");
                    DrawingObjects.AddObject(new Circle(intersectTime, 0.02, new Pen(Brushes.Red, 0.05f)), "ret5w4ert5uh1terj31eyt58kj6tr5j");

                    DrawingObjects.AddObject(new StringDraw("Ball", new Position2D(4.6, 0.0)), "12345ccxz6789abcdefgha35s21c4");
                    DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.15, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.02f)), "as.d02s3d21f3ds21f32ds14f8s2dqfjk6");
                    DrawingObjects.AddObject(new Circle(postoGo, 0.02, new Pen(Brushes.Blue, 0.05f)), "56cxvcxdv564sd2v44asd32dsd3f54dddss654654");
                    DrawingObjects.AddObject(new StringDraw("cut:" + cut.ToString() + " , " + postoGo.toString(), new Position2D(5.2, 2)), "5646543sd8sd3vcsd153vsdqwdeqwftry76546ij654");
                }
                #endregion
            }
            #endregion
            #region InPenaltyArea
            else if (CurrentState == (int)GoalieStates.InPenaltyArea)
            {
                Vector2D ballSpeed = BallState.Speed;
                double g = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[RobotID].Location - BallState.Location));
                double maxIncomming = 1.5, maxVertical = 1, maxOutGoing = 1;
                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(g) * Math.Cos(g))
                    + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(g)
                    + maxVertical;
                double maxSpeedToGet = 0.5;
                double distToBall = ballState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
                if (distToBall == 0)
                    distToBall = 0.5;
                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);
                Line ballspeed = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(15));
                Line goalline = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                Position2D? intersect = ballspeed.IntersectWithLine(goalline);
                bool skip = false;
                bool goActive = false;
                bool Gointersect = false;

                if (intersect.HasValue)
                {
                    Position2D intersects = intersect.Value;
                    //WC2017
                    //OurGoalRight.Y - .15 

                    // y between 0.75 and 1.15 or between negetive
                    if (((intersects.Y > GameParameters.OurGoalLeft.Y + .15 && intersects.Y < GameParameters.BorderWidth / 2) || (intersects.Y < GameParameters.OurGoalRight.Y - .15 && intersects.Y > -GameParameters.BorderWidth / 2))
                        && Model.BallState.Speed.Size > .3 && Model.BallState.Speed.InnerProduct(GameParameters.OurGoalCenter - Model.BallState.Location) > 0)
                    {
                        skip = true;
                    }
                    else
                    {
                        skip = false;
                    }
                }
                else
                {
                    skip = false;
                }

                if ((acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                {
                    goActive = true;
                }
                else
                {
                    goActive = false;
                }
                //TODO edit
                if ((inpenaltyMode == InPenaltyMode.GoIntersect && acceptable2 < ballSpeed.Size) || (inpenaltyMode != InPenaltyMode.GoIntersect && acceptable2 * 1.2 < ballSpeed.Size))
                {
                    Gointersect = true;
                }
                else
                {
                    Gointersect = false;
                }
                if (skip)
                {
                    //New Skip WC2017
                    DrawingObjects.AddObject(new StringDraw("skip", GameParameters.OurGoalCenter.Extend(0.6, 0)), "5645646465564");
                    Line ballSkipLine = new Line(Model.BallState.Location, intersect.Value);
                    Line goalerSkipLine = new Line(Model.OurRobots[RobotID].Location, GameParameters.OurGoalCenter);
                    Position2D SkipIntersect = ballSkipLine.IntersectWithLine(goalerSkipLine).Value;
                    Position2D targetforSkip = new Position2D();

                    //
                    if (SkipIntersect.DistanceFrom(intersect.Value) > Model.BallState.Location.DistanceFrom(intersect.Value) + 0.15
                        || SkipIntersect.DistanceFrom(GameParameters.OurGoalCenter) > Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) + 0.15)
                    {
                        targetforSkip = GameParameters.OurGoalCenter.Extend(-.2, 0);
                    }
                    else
                    {
                        targetforSkip = Model.OurRobots[RobotID].Location;
                    }
                    Planner.Add(RobotID, targetforSkip, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                    inpenaltyMode = InPenaltyMode.skip;
                }
                else if (goActive)
                {
                    DrawingObjects.AddObject(new StringDraw("goActive", GameParameters.OurGoalCenter.Extend(0.7, 0)), "654564654565464");
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = new Position2D();
                    double s = 0;
                    double s2 = 0;
                    if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out s, out s2) && Model.BallState.Speed.Size < .1)
                    {
                        tar = TargetToKick(Model, RobotID);// Position2D.Zero;// model.OurRobots[7].Location;
                    }
                    else
                    {
                        tar = TargetToKick(Model, RobotID);
                    }
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                    inpenaltyMode = InPenaltyMode.goActive;
                }
                else if (Gointersect)
                {
                    Position2D postoGo = GameParameters.OurGoalCenter;
                    double finalAngle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                    bool outGoing = false;
                    inpenaltyMode = InPenaltyMode.GoIntersect;
                    DrawingObjects.AddObject(new StringDraw("gointersect", GameParameters.OurGoalCenter.Extend(0.8, 0)), "987989856654564");
                    if (id == null)
                    {
                        Line ballSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                        List<Position2D> intersects = GameParameters.LineIntersectWithOurDangerZone(ballSpeedLine);
                        if (intersects.Count > 0)
                        {
                            Position2D pos = intersects.OrderBy(y => y.DistanceFrom(ballSpeedLine.Tail)).FirstOrDefault();
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Vector2D targetvector = GameParameters.OurGoalCenter - pos;
                                bool gotoperp = false;
                                Position2D perp = targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location);
                                Position2D intersectpos = new Line(perp, pos).IntersectWithLine(new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight)).Value;
                                perp = pos + (pos - intersectpos).GetNormalizeToCopy((pos - intersectpos).Size - 0.11);
                                if (perp.DistanceFrom(Model.OurRobots[RobotID].Location) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < pos.DistanceFrom(GameParameters.OurGoalCenter))
                                {
                                    gotoperp = true;
                                }
                                if (gotoperp)
                                {
                                    postoGo = perp;
                                    finalAngle = (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                    //Planner.Add(RobotID, perp, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                                else
                                {
                                    postoGo = s;
                                    finalAngle = (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                    //Planner.Add(RobotID, s, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                            }
                            else if (GameParameters.IsInField(Model.BallState.Location, -.1))
                            {
                                pos = Model.BallState.Location;
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                postoGo = s;
                                finalAngle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                //Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                            else
                            {
                                pos = new Position2D(GameParameters.OurGoalCenter.X - .1, Math.Sign(Model.BallState.Location.Y) * Math.Abs(GameParameters.OurGoalLeft.Y));
                                //Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                //Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                //double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                //Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                postoGo = pos;
                                finalAngle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                //Planner.Add(RobotID, pos, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }
                        else
                        {
                            Position2D pos = Model.BallState.Location;
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                outGoing = true;
                                GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                                double s = 0;
                                double s2 = 0;
                                Position2D tar = TargetToKick(Model, RobotID);
                                if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out s, out s2) && Model.BallState.Speed.Size < .1)
                                {
                                    tar = GameParameters.OurGoalCenter.Extend(-0.1, 0);
                                }
                                else
                                {
                                    tar = TargetToKick(Model, RobotID);
                                }
                                GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                            }
                        }
                    }
                    else if (id != null && id.HasValue && Model.Opponents.ContainsKey(id.Value))
                    {
                        Line ballSpeedLine = new Line(Model.BallState.Location, Model.Opponents[id.Value].Location);
                        List<Position2D> intersects = GameParameters.LineIntersectWithOurDangerZone(ballSpeedLine);
                        if (intersects.Count > 0)
                        {
                            Position2D pos = intersects.OrderBy(y => y.DistanceFrom(ballSpeedLine.Tail)).FirstOrDefault();
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                Vector2D targetvector = GameParameters.OurGoalCenter - pos;
                                bool gotoperp = false;
                                Planner.ChangeDefaulteParams(RobotID, false);
                                Planner.SetParameter(RobotID, 8, 8);
                                Position2D perp = new Position2D(Math.Min(targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location).X, GameParameters.OurGoalCenter.X - .1), targetvector.PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location).Y);
                                ;
                                if (perp.DistanceFrom(Model.OurRobots[RobotID].Location) > .1 && perp.DistanceFrom(GameParameters.OurGoalCenter) < pos.DistanceFrom(GameParameters.OurGoalCenter))
                                {
                                    gotoperp = true;
                                }
                                if (gotoperp)
                                {
                                    postoGo = perp;
                                    finalAngle = (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                    //Planner.Add(RobotID, perp, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                                else
                                {
                                    postoGo = s;
                                    finalAngle = (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                    //Planner.Add(RobotID, s, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                                }
                            }
                            else if (GameParameters.IsInField(Model.Opponents[id.Value].Location, -.1))
                            {
                                pos = Model.Opponents[id.Value].Location;
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                postoGo = s;
                                finalAngle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                //Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                            else
                            {
                                pos = new Position2D(GameParameters.OurGoalCenter.X - .1, Math.Sign(Model.BallState.Location.Y) * Math.Abs(GameParameters.OurGoalLeft.Y));
                                //Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                //Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                //double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                //Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                postoGo = pos;
                                finalAngle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                //Planner.Add(RobotID, pos, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }
                        else
                        {
                            Position2D pos = Model.BallState.Location;
                            if (GameParameters.IsInField(pos, -.1))
                            {
                                Vector2D leftvector = GameParameters.OurGoalLeft - pos;
                                Vector2D Rightvector = GameParameters.OurGoalRight - pos;
                                double R = .18 / Vector2D.AngleBetweenInRadians(leftvector, Rightvector);
                                Position2D s = pos + (GameParameters.OurGoalCenter - pos).GetNormalizeToCopy(Math.Min(pos.DistanceFrom(GameParameters.OurGoalCenter) - .2, R));
                                postoGo = s;
                                finalAngle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                                //Planner.Add(RobotID, s, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                            }
                        }


                    }


                    //WC 2017
                    #region cut
                    if (initialpos == Position2D.Zero)
                    {
                        initialpos = Model.BallState.Location;
                    }
                    Line goalerLine = new Line(Model.OurRobots[RobotID].Location, postoGo);
                    Line ballLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed);
                    Position2D? PosIntersect = ballLine.IntersectWithLine(goalerLine);

                    double xx, yy;

                    initialpos = Model.BallState.Location;
                    intersectTime = IntersectFind(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location);
                    double velocity = Model.BallState.Speed.Size;
                    ballcoeff = root(-.3, velocity, Model.BallState.Location.DistanceFrom(intersectTime));
                    robotCoeff = predicttime(Model, RobotID, initialpos, intersectTime);
                    double robotIntersectTime = timeRobotToTargetInIntersect(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location, intersectTime);

                    cut = false;
                    if (!BallKickedToOurGoal(Model) && PosIntersect.HasValue && (Model.OurRobots[RobotID].Location - PosIntersect.Value).GetNormnalizedCopy().InnerProduct((postoGo - PosIntersect.Value).GetNormnalizedCopy()) < -0.1 &&
                        Model.BallState.Speed.Size > 0.5 && (PosIntersect.Value - Model.BallState.Location).GetNormnalizedCopy().InnerProduct(Model.BallState.Speed.GetNormnalizedCopy()) > 0.1)// (PosIntersect.Value.X > Model.OurRobots[RobotID].Location.X + 0.9) && GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.2, out xx, out yy))//&& (PosTarget.X > Model.OurRobots[RobotID].Location.X + 0.9))//(PosTarget.DistanceFrom(GameParameters.OurGoalCenter) < Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)))
                    {
                        if (ballcoeff > 0 && intersectTime != (new Position2D(100, 100)) && (((robotCoeff - ballcoeff) > -0.5 && (robotCoeff - ballcoeff) < 0.5)))
                        {
                            lastPosNormal = Model.OurRobots[RobotID].Location;
                            cut = true;
                        }

                        if (cut)
                        {
                            if (GameParameters.IsInDangerousZone(lastPosNormal, false, 0.2, out xx, out yy) && lastPosNormal != Position2D.Zero)
                                postoGo = lastPosNormal;
                            DrawingObjects.AddObject(new StringDraw("cut = true , cutpos = " + PosIntersect.Value.toString(), GameParameters.OurGoalCenter.Extend(1, 0)), "sd3fs2d13ah321hg3h21dfs2ghdfgh");
                        }
                    }
                    else
                    {
                        cut = false;
                        lastPosNormal = Position2D.Zero;
                    }

                    if (cut)
                    {
                        DrawingObjects.AddObject(new Circle(lastPosNormal, 0.1, new System.Drawing.Pen(System.Drawing.Color.Red, 0.02f)), "sdf32s1df32ds");
                    }
                    if ((robotCoeff - ballcoeff) > 0.6 || (robotCoeff - ballcoeff) < -0.6)
                    {
                        cut = false;
                        lastPosNormal = Position2D.Zero;
                    }


                    if (!outGoing)
                        Planner.Add(RobotID, postoGo, finalAngle, PathType.UnSafe, false, false, false, false);

                    if (!cut)
                    {
                        lastPosNormal = Position2D.Zero;
                    }
                    bool debug = true;
                    if (debug)
                    {
                        DrawingObjects.AddObject(new StringDraw("time = " + (robotCoeff - ballcoeff).ToString(), new Position2D(5, 2)), "3asf21asf21a3sdsrytwr68hj514trehj");
                        DrawingObjects.AddObject(new Circle(intersectTime, 0.02, new Pen(Brushes.Red, 0.05f)), "ret5w4ert5uh1terj31eyt58kj6tr5j");

                        DrawingObjects.AddObject(new StringDraw("Ball", new Position2D(4.6, 0.0)), "12345ccxz6789abcdefgha35s21c4");
                        DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.15, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.02f)), "as.d02s3d21f3ds21f32ds14f8s2dqfjk6");
                        DrawingObjects.AddObject(new Circle(postoGo, 0.02, new Pen(Brushes.Blue, 0.05f)), "56cxvcxdv564sd2v44asd32dsd3f54dddss654654");
                        DrawingObjects.AddObject(new StringDraw("cut:" + cut.ToString() + " , " + postoGo.toString(), new Position2D(5.2, 2)), "5646543sd8sd3vcsd153vsdqwdeqwftry76546ij654");
                    }
                    #endregion
                }
                else
                {
                    inpenaltyMode = InPenaltyMode.nothing;
                    DrawingObjects.AddObject(new StringDraw("Nothing", GameParameters.OurGoalCenter.Extend(0.8, 0)), "213213213213212321");
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    double s = 0;
                    double s2 = 0;
                    Position2D tar = TargetToKick(Model, RobotID);
                    if (GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0, out s, out s2) && Model.BallState.Speed.Size < .1)
                    {
                        tar = Position2D.Zero;
                    }
                    else
                    {
                        tar = TargetToKick(Model, RobotID);
                    }
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                }
                Obstacles obs = new Obstacles(Model);
                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, null);
                Vector2D v = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 0.35);
                double kickSpeed = 1;
                //TODO edit
                if (Model.BallState.Location.X > GameParameters.OurGoalCenter.X - 0.1 || Math.Abs(Model.OurRobots[RobotID].Angle.Value) < 100 || obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.022))
                    kickSpeed = 0;
                Planner.AddKick(RobotID, kickPowerType.Speed, kickSpeed, (kickSpeed > 0) ? true : false, false);
            }
            #endregion
            #region KickToGoal
            else if (CurrentState == (int)GoalieStates.KickToGoal)
            {
                if (Model.BallState.Speed.InnerProduct(Model.OurRobots[RobotID].Location - Model.BallState.Location) > 0)
                    GetSkill<GoalieDiveSkill2017>().Dive(engine, Model, RobotID, true, 200);
                else
                {
                    GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                    Position2D tar = TargetToKick(Model, RobotID);
                    GetSkill<GetBallSkill>().OutGoingSideTrack(Model, RobotID, tar);
                }
            }
            #endregion
            #region KickToRobot
            else if (CurrentState == (int)GoalieStates.KickToRobot)
                Planner.Add(RobotID, Model.OurRobots[RobotID].Location, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, false);
            #endregion
            #region EatTheBall
            else if (CurrentState == (int)GoalieStates.EathTheBall)
            {
                Planner.Add(RobotID, GameParameters.OurGoalCenter + (defenceSate.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(defenceSate, -.2)), (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
            }
            #endregion
            #region Rahmati
            if (CurrentState == (int)GoalieStates.Rahmati)
            {
                Position2D posongoal = new Position2D();

                DefenceInfo inf1 = null;
                if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender1)))
                    inf1 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender1)).First();
                DefenceInfo inf2 = null;
                if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
                    inf2 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();

                Defender1Delayed = false;
                if (StaticDefender1ID.HasValue && inf1 != null && inf1.DefenderPosition.HasValue && Model.OurRobots.ContainsKey(StaticDefender1ID.Value))
                {
                    if (Model.OurRobots[StaticDefender1ID.Value].Location.DistanceFrom(inf1.DefenderPosition.Value) > .1)
                    {
                        Defender1Delayed = true;
                    }
                }
                Defender2Delayed = false;
                if (StaticDefender2ID.HasValue && inf2 != null && inf1.DefenderPosition.HasValue && Model.OurRobots.ContainsKey(StaticDefender2ID.Value))
                {
                    if (Model.OurRobots[StaticDefender2ID.Value].Location.DistanceFrom(inf2.DefenderPosition.Value) > .1)
                    {
                        Defender2Delayed = true;
                    }
                }

                if (!StaticDefender1ID.HasValue)
                {
                    Defender1Delayed = true;
                }
                if (!StaticDefender2ID.HasValue)
                {
                    Defender2Delayed = true;
                }

                posongoal = GameParameters.OurGoalCenter;
                if (Defender1Delayed && Defender2Delayed)
                {
                    posongoal = GameParameters.OurGoalCenter;
                }
                else if (Defender1Delayed)
                {

                    Position2D intersect1 = inf2.DefenderPosition.Value + Vector2D.FromAngleSize((defenceSate.Location - inf2.DefenderPosition.Value).AngleInRadians - (Math.PI / 2), RobotParameters.OurRobotParams.Diameter / 2);
                    Position2D? intersect2 = new Line(defenceSate.Location, intersect1).IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));

                    if (intersect2.HasValue)
                    {
                        if (intersect2.Value.Y > GameParameters.OurGoalRight.Y && intersect2.Value.Y < GameParameters.OurGoalLeft.Y)
                        {
                            posongoal = intersect2.Value;//new Position2D(GameParameters.OurGoalCenter.X, (intersect2.Value.Y));// + GameParameters.OurGoalLeft.Y) / 2);
                            DrawingObjects.AddObject(posongoal, "545649845646");
                        }
                        else
                        {
                            posongoal = GameParameters.OurGoalCenter;
                        }
                    }
                    else
                    {
                        posongoal = GameParameters.OurGoalCenter;
                    }
                }
                else if (Defender2Delayed)
                {

                    Position2D intersect1 = inf1.DefenderPosition.Value + Vector2D.FromAngleSize((defenceSate.Location - inf1.DefenderPosition.Value).AngleInRadians + (Math.PI / 2), RobotParameters.OurRobotParams.Diameter / 2);
                    DrawingObjects.AddObject(intersect1, "9879878979879");
                    Position2D? intersect2 = new Line(defenceSate.Location, intersect1).IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));
                    DrawingObjects.AddObject(intersect2, "68796464654");

                    if (intersect2.HasValue)
                    {
                        if (intersect2.Value.Y > GameParameters.OurGoalRight.Y && intersect2.Value.Y < GameParameters.OurGoalLeft.Y)
                        {
                            posongoal = new Position2D(GameParameters.OurGoalCenter.X, (intersect2.Value.Y + GameParameters.OurGoalRight.Y) / 2);
                            DrawingObjects.AddObject(posongoal, "654564564");
                        }
                        else
                        {
                            posongoal = GameParameters.OurGoalCenter;
                        }
                    }
                    else
                    {
                        posongoal = GameParameters.OurGoalCenter;
                    }
                }
                else
                {
                    posongoal = GameParameters.OurGoalCenter;
                }



                DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");

                Position2D currentPos = Model.OurRobots[RobotID].Location;

                DefenceInfo delayedrobot = new DefenceInfo();

                if (Defender2Delayed)
                {
                    delayedrobot = inf2;
                }
                else if (Defender1Delayed)
                {
                    delayedrobot = inf1;
                }
                else
                {
                    delayedrobot = inf1;
                }
                Position2D tempintersect1 = delayedrobot.DefenderPosition.Value + Vector2D.FromAngleSize((defenceSate.Location - delayedrobot.DefenderPosition.Value).AngleInRadians + (Math.PI / 2), RobotParameters.OurRobotParams.Diameter / 2);
                //DrawingObjects.AddObject(intersect1, "9879878979879");
                Position2D? tempintersect2 = new Line(defenceSate.Location, delayedrobot.DefenderPosition.Value).IntersectWithLine(new Line(GameParameters.OurGoalRight, GameParameters.OurGoalLeft));

                Circle goalcenter = new Circle(GameParameters.OurGoalCenter, GameParameters.SafeRadi(defenceSate, -.20));
                //Position2D? mainuntersect = goalcenter.Intersect(new Line(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X - .05), defenceSate.Location.Y), posongoal)).Where(y => y.X < GameParameters.OurGoalCenter.X).OrderBy(y => y.DistanceFrom(model.BallState.Location)).FirstOrDefault();
                //DrawingObjects.AddObject(mainuntersect, "6465464545");
                Vector2D leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                Vector2D rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                Position2D? mainuntersect = new Position2D();

                if (!GameParameters.IsInField(defenceSate.Location, 0))
                {
                    if (defenceSate.Location.X > 0)
                    {
                        if (defenceSate.Location.Y > 0)
                            defenceSate = new SingleObjectState(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Min(defenceSate.Location.Y, GameParameters.OurLeftCorner.Y)), Vector2D.Zero, 0f);
                        else
                        {
                            defenceSate = new SingleObjectState(new Position2D(Math.Min(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Max(defenceSate.Location.Y, GameParameters.OurRightCorner.Y)), Vector2D.Zero, 0f);
                        }
                    }
                    else
                    {
                        if (defenceSate.Location.Y > 0)
                            defenceSate = new SingleObjectState(new Position2D(Math.Max(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Min(defenceSate.Location.Y, GameParameters.OurLeftCorner.Y)), Vector2D.Zero, 0f);
                        else
                        {
                            defenceSate = new SingleObjectState(new Position2D(Math.Max(defenceSate.Location.X, GameParameters.OurGoalCenter.X), Math.Max(defenceSate.Location.Y, GameParameters.OurRightCorner.Y)), Vector2D.Zero, 0f);
                        }
                    }

                }

                //DrawingObjects.AddObject(new Line(defenceSate.Location, tempintersect2.Value));
                //DrawingObjects.AddObject(new Line(GameParameters.OurGoalLeft , defenceSate.Location));

                if (Defender1Delayed && tempintersect2.HasValue)
                {
                    leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                    rightVector = tempintersect2.Value - defenceSate.Location;
                }
                if (Defender2Delayed && tempintersect2.HasValue)
                {
                    leftVector = tempintersect2.Value - defenceSate.Location;
                    rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                }
                if (Defender1Delayed && Defender2Delayed)
                {
                    leftVector = GameParameters.OurGoalLeft - defenceSate.Location;
                    rightVector = GameParameters.OurGoalRight - defenceSate.Location;
                }
                //bisector line from a position
                Line bisector = Vector2D.Bisector(leftVector, rightVector, defenceSate.Location);
                DrawingObjects.AddObject(bisector, "bisicktiir");
                mainuntersect = goalcenter.Intersect(bisector).Where(y => y.X < GameParameters.OurGoalCenter.X).OrderBy(y => y.DistanceFrom(defenceSate.Location)).FirstOrDefault();

                #region Normal
                Position2D go = GameParameters.OurGoalCenter + (defenceSate.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(defenceSate, -.40));
                #endregion


                DrawingObjects.AddObject(mainuntersect, "654564655454564");
                Position2D mainTarget = (mainuntersect.HasValue && mainuntersect.Value != new Position2D()) ? mainuntersect.Value : go;
                Vector2D targetvector = defenceSate.Location - posongoal;

                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 8, 4);//to change
                Position2D normalprep = targetvector.PrependecularPoint(posongoal, currentPos);
                Position2D perp = (normalprep.X > GameParameters.OurGoalCenter.X) ? posongoal + targetvector.GetNormalizeToCopy(.2) : normalprep;
                perp = new Position2D(Math.Min(GameParameters.OurGoalCenter.X - .1, perp.X), perp.Y);

                if (perp.DistanceFrom(currentPos) > .25)
                {
                    gotoperp = true;
                }
                else if (perp.DistanceFrom(currentPos) < .2)
                {
                    gotoperp = false;
                }
                mainTarget = new Position2D(Math.Min(GameParameters.OurGoalCenter.X - .1, mainTarget.X), mainTarget.Y);
                Position2D postoGo = GameParameters.OurGoalCenter;
                if (gotoperp)
                {
                    postoGo = perp;
                    //Planner.Add(RobotID, perp, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }
                else
                {
                    postoGo = mainTarget;
                    //Planner.Add(RobotID, mainTarget, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }

                //WC 2017
                #region cut
                if (initialpos == Position2D.Zero)
                {
                    initialpos = Model.BallState.Location;
                }
                Line goalerLine = new Line(Model.OurRobots[RobotID].Location, postoGo);
                Line ballLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed);
                Position2D? PosIntersect = ballLine.IntersectWithLine(goalerLine);

                double xx, yy;

                initialpos = Model.BallState.Location;
                intersectTime = IntersectFind(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location);
                double velocity = Model.BallState.Speed.Size;
                ballcoeff = root(-.3, velocity, Model.BallState.Location.DistanceFrom(intersectTime));
                robotCoeff = predicttime(Model, RobotID, initialpos, intersectTime);
                double robotIntersectTime = timeRobotToTargetInIntersect(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location, intersectTime);

                cut = false;
                if (!BallKickedToOurGoal(Model) && PosIntersect.HasValue && (Model.OurRobots[RobotID].Location - PosIntersect.Value).GetNormnalizedCopy().InnerProduct((postoGo - PosIntersect.Value).GetNormnalizedCopy()) < -0.1 &&
                    Model.BallState.Speed.Size > 0.5 && (PosIntersect.Value - Model.BallState.Location).GetNormnalizedCopy().InnerProduct(Model.BallState.Speed.GetNormnalizedCopy()) > 0.1 && Model.OurRobots[RobotID].Location.X < PosIntersect.Value.X)// (PosIntersect.Value.X > Model.OurRobots[RobotID].Location.X + 0.9) && GameParameters.IsInDangerousZone(Model.BallState.Location, false, 0.2, out xx, out yy))//&& (PosTarget.X > Model.OurRobots[RobotID].Location.X + 0.9))//(PosTarget.DistanceFrom(GameParameters.OurGoalCenter) < Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)))
                {
                    if (ballcoeff > 0 && intersectTime != (new Position2D(100, 100)) && (((robotCoeff - ballcoeff) > -0.5 && (robotCoeff - ballcoeff) < 0.5)))
                    {
                        lastPosNormal = Model.OurRobots[RobotID].Location;
                        cut = true;
                    }

                    if (cut)
                    {
                        if (GameParameters.IsInDangerousZone(lastPosNormal, false, 0.2, out xx, out yy) && lastPosNormal != Position2D.Zero)
                            postoGo = lastPosNormal;
                        DrawingObjects.AddObject(new StringDraw("cut = true , cutpos = " + PosIntersect.Value.toString(), GameParameters.OurGoalCenter.Extend(1, 0)), "sd3fs2d13ah321hg3h21dfs2ghdfgh");
                    }
                }
                else
                {
                    cut = false;
                    lastPosNormal = Position2D.Zero;
                }

                if (cut)
                {
                    DrawingObjects.AddObject(new Circle(lastPosNormal, 0.1, new System.Drawing.Pen(System.Drawing.Color.Red, 0.02f)), "sdf32s1df32ds");
                }
                if ((robotCoeff - ballcoeff) > 0.6 || (robotCoeff - ballcoeff) < -0.6)
                {
                    cut = false;
                    lastPosNormal = Position2D.Zero;
                }
                Planner.Add(RobotID, postoGo, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);

                if (!cut)
                {
                    lastPosNormal = Position2D.Zero;
                }
                if (cut)
                {
                    DrawingObjects.AddObject(new Circle(lastPosNormal, 0.1, new System.Drawing.Pen(System.Drawing.Color.Red, 0.02f)), "sdf32s1df32ds");
                }
                bool debug = false;
                if (debug)
                {
                    DrawingObjects.AddObject(new StringDraw("time = " + (robotCoeff - ballcoeff).ToString(), new Position2D(5, 2)), "3asf21asf21a3sdsrytwr68hj514trehj");
                    DrawingObjects.AddObject(new Circle(intersectTime, 0.02, new Pen(Brushes.Red, 0.05f)), "ret5w4ert5uh1terj31eyt58kj6tr5j");

                    DrawingObjects.AddObject(new StringDraw("Ball", new Position2D(4.6, 0.0)), "12345ccxz6789abcdefgha35s21c4");
                    DrawingObjects.AddObject(new Circle(Model.BallState.Location, 0.15, new System.Drawing.Pen(System.Drawing.Color.Purple, 0.02f)), "as.d02s3d21f3ds21f32ds14f8s2dqfjk6");
                    DrawingObjects.AddObject(new Circle(postoGo, 0.02, new Pen(Brushes.Blue, 0.05f)), "56cxvcxdv564sd2v44asd32dsd3f54dddss654654");
                    DrawingObjects.AddObject(new StringDraw("cut:" + cut.ToString() + " , " + postoGo.toString(), new Position2D(5.2, 2)), "5646543sd8sd3vcsd153vsdqwdeqwftry76546ij654");
                }
                #endregion

            }
            #endregion
            #region OpponentIsInPass
            if (CurrentState == (int)GoalieStates.OpponentInPassState)
            {
                if (Model.Opponents.ContainsKey(kickerOpponent))
                {
                    Line intersect = new Line(Model.Opponents[kickerOpponent].Location, Model.Opponents[kickerOpponent].Location + Vector2D.FromAngleSize(Model.Opponents[kickerOpponent].Angle.Value * Math.PI / 180, 10));
                    Line goalLine = new Line(GameParameters.OurGoalLeft.Extend(/*-.1 => WC2017*/ .1, 0), GameParameters.OurGoalRight.Extend(-.1, 0));
                    Position2D? intersectpos = goalLine.IntersectWithLine(intersect);
                    if (intersectpos.HasValue && intersectpos.Value.Y < .5 && intersectpos.Value.Y > -.5)
                        Planner.Add(RobotID, intersectpos.Value, (Model.Opponents[kickerOpponent].Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                }
            }
            #endregion
            #region PreDive
            if (CurrentState == (int)GoalieStates.PreDive)
            {
                Position2D plannerTarget = new Position2D();
                Position2D target = defenceSate.Location;

                Position2D ourGoalLeftWithextend = GameParameters.OurGoalLeft.Extend(0, .04);
                Position2D ourGoalRightWithextend = GameParameters.OurGoalRight.Extend(0, -.04);

                Vector2D leftCornerCenterGoal = GameParameters.OurLeftCorner - GameParameters.OurGoalCenter;
                Vector2D rightCornerCentergoal = GameParameters.OurRightCorner - GameParameters.OurGoalCenter;

                Vector2D targetGoalCenter = target - GameParameters.OurGoalCenter;

                double angleBetweenLeftAndtarget = Math.Abs(Vector2D.AngleBetweenInDegrees(leftCornerCenterGoal, targetGoalCenter));
                angleBetweenLeftAndtarget = (angleBetweenLeftAndtarget < 180) ? angleBetweenLeftAndtarget : 360 - angleBetweenLeftAndtarget;

                double angleBetweenRightAndtarget = Math.Abs(Vector2D.AngleBetweenInDegrees(rightCornerCentergoal, targetGoalCenter));
                angleBetweenRightAndtarget = (angleBetweenRightAndtarget < 180) ? angleBetweenRightAndtarget : 360 - angleBetweenRightAndtarget;

                bool Left = (angleBetweenLeftAndtarget < angleBetweenRightAndtarget) ? true : false;
                double mainAngle = (Left) ? angleBetweenLeftAndtarget : angleBetweenRightAndtarget;
                Position2D goalNearCorner = (Left) ? ourGoalLeftWithextend : ourGoalRightWithextend;

                double dist = (.8 * mainAngle) / 90;

                Vector2D LeftwithExtend = (ourGoalLeftWithextend - target).GetNormalizeToCopy(target.DistanceFrom(goalNearCorner) - dist);
                Vector2D RightWithExtend = (ourGoalRightWithextend - target).GetNormalizeToCopy(target.DistanceFrom(goalNearCorner) - dist);

                Position2D leftGoal = target + LeftwithExtend;
                Position2D rightGoal = target + RightWithExtend;

                Line MovementLine = new Line(leftGoal, rightGoal);

                int? oppBallOwner = Model.Opponents.OrderBy(t => t.Value.Location.DistanceFrom(Model.BallState.Location)).Select(y => y.Key).FirstOrDefault();
                Vector2D RobotHeadVector = (GameParameters.OurGoalCenter - Model.BallState.Location);
                if (oppBallOwner.HasValue)
                    RobotHeadVector = Vector2D.FromAngleSize(((Model.Opponents[oppBallOwner.Value].Angle.Value * Math.PI / 180) + (Model.BallState.Location - Model.Opponents[oppBallOwner.Value].Location).AngleInRadians) / 2, 10);

                Line RobotHeadLine = new Line(Model.BallState.Location, Model.BallState.Location + RobotHeadVector);

                Position2D? intersection = RobotHeadLine.IntersectWithLine(MovementLine);

                if (intersection.HasValue)
                {
                    Position2D intersect = intersection.Value;
                    if (leftGoal.DistanceFrom(rightGoal) < RobotParameters.OurRobotParams.Diameter)
                    {
                        Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy(leftGoal.DistanceFrom(rightGoal) / 2);
                        Position2D robottarget = rightGoal + rightToLeft;

                        if (robottarget.DistanceFrom(goalNearCorner) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                            robottarget = rightGoal + rightToLeft;
                        }
                        plannerTarget = robottarget;
                    }
                    else
                    {
                        if (intersect.DistanceFrom(leftGoal) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            Vector2D rightToLeft = (rightGoal - leftGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                            Position2D robottarget = leftGoal + rightToLeft;
                            plannerTarget = robottarget;
                        }
                        else if (intersect.DistanceFrom(rightGoal) < RobotParameters.OurRobotParams.Diameter / 2)
                        {
                            Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                            Position2D robottarget = rightGoal + rightToLeft;
                            plannerTarget = robottarget;
                        }
                        else if (!Vector2D.IsBetween(ourGoalLeftWithextend - target, ourGoalRightWithextend - target, intersect - target))
                        {
                            if (Vector2D.IsBetween(ourGoalLeftWithextend - target, GameParameters.OurLeftCorner - target, intersect - target))
                            {
                                Vector2D rightToLeft = (rightGoal - leftGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                                Position2D robottarget = leftGoal + rightToLeft;
                                plannerTarget = robottarget;
                            }
                            else if (Vector2D.IsBetween(ourGoalRightWithextend - target, GameParameters.OurRightCorner - target, intersect - target))
                            {
                                Vector2D rightToLeft = (leftGoal - rightGoal).GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + .01);
                                Position2D robottarget = rightGoal + rightToLeft;
                                plannerTarget = robottarget;
                            }
                            else
                            {
                                plannerTarget = GameParameters.OurGoalCenter + (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(.5);
                            }
                        }
                        else
                        {
                            plannerTarget = intersect;
                        }
                    }
                }
                else
                {
                    plannerTarget = GameParameters.OurGoalCenter - (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(.4);
                }
                DrawingObjects.AddObject(new Circle(plannerTarget, .1, new Pen(Brushes.Blue, .01f)), "54564646646546");
                DrawingObjects.AddObject(new Circle(intersection.Value, .1, new Pen(Brushes.HotPink, .01f)), "56456464");

                Planner.Add(RobotID, plannerTarget, (target - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);


            }
            #endregion
            #region Piston
            else if (CurrentState == (int)GoalieStates.Piston)
            {

                DrawingObjects.AddObject(new StringDraw((id.HasValue) ? "Robot" : "Ball", GameParameters.OurGoalCenter.Extend(0.45, 0)), "rbstate");

                defenceSate = (id.HasValue) ? Model.Opponents[id.Value] : ballState;
                double dist = PistonDistance(defenceSate.Location);
                Position2D postoGo = GameParameters.OurGoalCenter + (GameParameters.OurGoalCenter - defenceSate.Location).GetNormalizeToCopy(-dist);

                double x = postoGo.X;
                x = Math.Min(GameParameters.OurGoalCenter.X - 0.11, x);
                postoGo = new Position2D(x, postoGo.Y);
                Planner.Add(RobotID, postoGo, (defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                var s = new SingleObjectState(postoGo, defenceSate.Speed, (float)(defenceSate.Location - Model.OurRobots[RobotID].Location).AngleInDegrees);
                GoalieTargetPos = postoGo;
            }
            #endregion

            return new SingleWirelessCommand();

        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            int? id = StaticRB(engine, Model);
            SingleObjectState defenceSate = (id.HasValue) ? Model.Opponents[id.Value] : ballState;

            DefenceInfo inf1 = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender1)))
                inf1 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender1)).First();
            DefenceInfo inf2 = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
                inf2 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();

            if (inf1 != null && inf1.DefenderPosition.HasValue && inf2.DefenderPosition.HasValue && inf2 != null)
            {
                if (!eattheball(engine, Model, inf1, inf2, true))
                {
                    FreekickDefence.EaththeBall = false;
                    FreekickDefence.ReadyForEatStatic1 = false;
                    FreekickDefence.ReadyForEatStatic2 = false;
                }
            }

            Line line = new Line();
            line = new Line(ballState.Location, ballState.Location - BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = ballState.Location.DistanceFrom(GameParameters.OurGoalCenter);

            if (!GameParameters.IsInField(ballState.Location, 0.05))
                CurrentState = (int)GoalieStates.Normal;
            else
            {
                Vector2D ballSpeed = BallState.Speed;
                double v = Vector2D.AngleBetweenInRadians(ballSpeed, (Model.OurRobots[RobotID].Location - BallState.Location));
                double maxIncomming = 1.5, maxVertical = 1, maxOutGoing = 1;
                double acceptableballRobotSpeed = ((maxIncomming + maxOutGoing) / 2 - maxVertical) * (Math.Cos(v) * Math.Cos(v)) + ((maxIncomming - maxOutGoing) / 2) * Math.Cos(v) + maxVertical;
                double maxSpeedToGet = 0.5;
                double dist, dist2;
                double margin = FreekickDefence.AdditionalSafeRadi;//+ RobotParameters.OurRobotParams.Diameter / 2 ;

                double distToBall = ballState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
                if (distToBall == 0)
                    distToBall = 0.5;
                double acceptable2 = acceptableballRobotSpeed / (3 * distToBall);

                double innerProduct = Vector2D.InnerProduct(BallState.Speed, (Model.OurRobots[RobotID].Location - BallState.Location));
                double difAngle = Vector2D.AngleBetweenInDegrees(BallState.Speed, (BallState.Location - Model.OurRobots[RobotID].Location));

                Circle c = new Circle(Model.OurRobots[RobotID].Location, 0.12);
                Line l = new Line(BallState.Location, BallState.Location + BallState.Speed);

                List<Position2D> inters = c.Intersect(l);

                #region Predive Configuration
                int? oppBallOwner = Model.Opponents.OrderBy(t => t.Value.Location.DistanceFrom(Model.BallState.Location)).Select(y => y.Key).FirstOrDefault();
                bool ballisinfront = false;
                bool rahmatiMainState = false;
                if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value))
                {
                    int oppballownerID = oppBallOwner.Value;
                    Line robotleftLine = new Line(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180) + Math.PI / 2, .11), (Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180) + Math.PI / 2, .11)) + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180), 1));
                    Line robotRightLine = new Line(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180) - Math.PI / 2, .11), (Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180) - Math.PI / 2, .11)) + Vector2D.FromAngleSize((Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180), 1));

                    if (robotleftLine.Distance(Model.BallState.Location) + robotRightLine.Distance(Model.BallState.Location) < .23)
                    {
                        ballisinfront = true;
                    }
                    else
                    {
                        ballisinfront = false;
                    }

                    if (inf1 != null && inf2 != null)
                    {
                        if (StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05 || StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05)
                        {
                            if (StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05 && StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) < .05)
                            {
                                List<int> IDsExeptthanwanted = new List<int>();
                                IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != FreekickDefence.Static1ID.Value).Select(y => y.Key).ToList();
                                Obstacles obstacles2 = new Obstacles(Model);
                                obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, Model.Opponents.Keys.ToList());
                                if (obstacles2.Meet(Model.Opponents[oppballownerID], new SingleObjectState(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize(Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                                {
                                    rahmatiMainState = false;
                                }
                                else
                                {
                                    rahmatiMainState = true;
                                }
                            }
                            else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) < .05)
                            {
                                List<int> IDsExeptthanwanted = new List<int>();
                                IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != FreekickDefence.Static2ID.Value).Select(y => y.Key).ToList();
                                Obstacles obstacles2 = new Obstacles(Model);
                                obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, Model.Opponents.Keys.ToList());
                                if (obstacles2.Meet(Model.Opponents[oppballownerID], new SingleObjectState(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize(Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                                {
                                    rahmatiMainState = false;
                                }
                                else
                                {
                                    rahmatiMainState = true;
                                }
                            }
                            else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .05)
                            {
                                rahmatiMainState = true;
                            }
                            else if (StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) < .05 && StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) < .05)
                            {
                                List<int> IDsExeptthanwanted = new List<int>();
                                IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != FreekickDefence.Static2ID.Value && t.Key != FreekickDefence.Static1ID.Value).Select(y => y.Key).ToList();
                                Obstacles obstacles2 = new Obstacles(Model);
                                obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, Model.Opponents.Keys.ToList());
                                if (obstacles2.Meet(Model.Opponents[oppballownerID], new SingleObjectState(Model.Opponents[oppballownerID].Location + Vector2D.FromAngleSize(Model.Opponents[oppballownerID].Angle.Value * Math.PI / 180, 5), Vector2D.Zero, 0f), .04))
                                {
                                    rahmatiMainState = false;
                                }
                                else
                                {
                                    rahmatiMainState = true;
                                }
                            }
                        }
                    }
                }
                if (FreekickDefence.Static2ID == null || FreekickDefence.Static1ID == null)
                {
                    rahmatiMainState = true;
                }
                #endregion
                #region Predive
                if (CurrentState == (int)GoalieStates.PreDive)
                {
                    counter1++;
                    Line ballspeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(10));
                    Line golline = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                    if (oppBallOwner.HasValue && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) / Model.BallState.Speed.Size < 1.3 && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) > .18)//&& golline.IntersectWithLine(ballspeedLine).HasValue && golline.IntersectWithLine(ballspeedLine).Value.Y < GameParameters.OurGoalLeft.Y && golline.IntersectWithLine(ballspeedLine).Value.Y > GameParameters.OurGoalRight.Y)// 1.3 zaribe khoobe az fasele va sorAt baraye raftan be dive
                    {
                        CurrentState = (int)GoalieStates.KickToGoal;
                    }
                    else if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && (Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) > .18 && golline.IntersectWithLine(ballspeedLine).HasValue && (golline.IntersectWithLine(ballspeedLine).Value.Y > GameParameters.OurGoalLeft.Y || golline.IntersectWithLine(ballspeedLine).Value.Y < GameParameters.OurGoalRight.Y)) || !rahmatiMainState)
                    {
                        CurrentState = (int)GoalieStates.Normal;
                    }
                    else if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) > .18 && (Model.BallState.Speed.Size < .05 || counter1 > 10))// goftim counter bishtar az 10 bezarim ta khialemoon rahat she ke too pre dive nemimoonim toopp biad az kenaremoon rad she
                    {
                        CurrentState = (int)GoalieStates.Normal;
                    }
                    else if (!oppBallOwner.HasValue || !Model.Opponents.ContainsKey(oppBallOwner.Value))
                    {
                        CurrentState = (int)GoalieStates.Normal;
                    }


                    if (CurrentState != (int)GoalieStates.PreDive)
                    {
                        counter1 = 0;
                    }
                }
                #endregion
                #region Normal
                else if (CurrentState == (int)GoalieStates.Normal)
                {

                    if (BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < .18 && Model.BallState.Location.X > 0 && Model.BallState.Speed.Size < 1 && ballisinfront && rahmatiMainState)
                    {
                        CurrentState = (int)GoalieStates.PreDive;
                    }
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (eattheball(engine, Model, inf1, inf2, false))
                    {
                        FreekickDefence.EaththeBall = true;
                        FreekickDefence.StaticFirstState = DefenderStates.EatTheBall;
                        FreekickDefence.StaticSecondState = DefenderStates.EatTheBall;
                        CurrentState = (int)GoalieStates.EathTheBall;
                    }
                    else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X < 2.7)
                        CurrentState = (int)GoalieStates.Piston;
                    else if ((GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet)) || ((GameParameters.IsInDangerousZone(ballState.Location, false, -.1, out dist, out dist2))))//WC 2017
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if ((inf1 != null && inf2 != null) || inf1.DefenderPosition.HasValue || inf2.DefenderPosition.HasValue || FreekickDefence.Static1ID == null || FreekickDefence.Static2ID == null) // TODO RAHMATI BACK
                    {
                        Position2D goalieTarget = (defenceSate.Location - GoalieTargetPos).PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location);
                        if ((StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .10 || StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .10))
                            CurrentState = (int)GoalieStates.Rahmati;

                    }
                }
                #endregion
                #region piston
                else if (CurrentState == (int)GoalieStates.Piston)
                {

                    Position2D goalieTarget = (defenceSate.Location - GoalieTargetPos).PrependecularPoint(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location);
                    if (BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.KickToGoal;

                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < .18 && Model.BallState.Location.X > 0 && Model.BallState.Speed.Size < 1 && ballisinfront && rahmatiMainState)
                    {
                        CurrentState = (int)GoalieStates.PreDive;
                    }
                    else if ((GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet)) || ((GameParameters.IsInDangerousZone(ballState.Location, false, -.1, out dist, out dist2))))//IO 2015

                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if ((inf1.DefenderPosition.HasValue || inf2.DefenderPosition.HasValue) && (StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) > .10 || StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) > .10 || FreekickDefence.Static1ID == null || FreekickDefence.Static2ID == null))//Rahmati Back)
                    {
                        CurrentState = (int)GoalieStates.Rahmati;
                    }
                    else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) < 2.275 / 1.1 || Model.BallState.Location.X > 2.7 * 1.1)
                        CurrentState = (int)GoalieStates.Normal;



                }
                #endregion
                #region InPenaltyArea
                else if (CurrentState == (int)GoalieStates.InPenaltyArea)
                {
                    margin = 0.2;
                    if (BallKickedToOurGoal(Model) &&
                        (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2)
                        || acceptable2 < ballSpeed.Size))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X < 2.7)
                        CurrentState = (int)GoalieStates.Piston;
                    else if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin + .05, out dist, out dist2) || (acceptable2 * 1.2 < ballSpeed.Size && !(GameParameters.IsInDangerousZone(ballState.Location, false, -.08, out dist, out dist2))))//WC2017
                        CurrentState = (int)GoalieStates.Normal;
                }
                #endregion
                #region KickToGoal
                else if (CurrentState == (int)GoalieStates.KickToGoal)
                {
                    margin = 0.1;
                    if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (!BallKickedToOurGoal(Model) && GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X < 2.7 && !BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.Piston;
                    else
                        if (!BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.Normal;
                }
                #endregion
                #region KickToRobot
                else if (CurrentState == (int)GoalieStates.KickToRobot)
                {
                    if (ballSpeed.Size < 1.5 || BallKickedToOurGoal(Model) || inters.Count == 0 || innerProduct < -0.1)
                    {
                        if (BallKickedToOurGoal(Model))
                            CurrentState = (int)GoalieStates.KickToGoal;
                        else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2))//&& (acceptable2 < ballSpeed.Size || ballSpeed.Size < maxSpeedToGet))//IranOpen2015
                            CurrentState = (int)GoalieStates.InPenaltyArea;
                        else if (GameParameters.OurGoalCenter.DistanceFrom(defenceSate.Location) > 2.275 && Model.BallState.Location.X < 2.7)
                            CurrentState = (int)GoalieStates.Piston;
                        else
                            CurrentState = (int)GoalieStates.Normal;
                    }
                }
                #endregion
                #region Rahmati
                else if (CurrentState == (int)GoalieStates.Rahmati)
                {
                    if (BallKickedToOurGoal(Model))
                        CurrentState = (int)GoalieStates.KickToGoal;
                    else if (ballSpeed.Size > 2 && !BallKickedToOurGoal(Model) && inters.Count > 0 && innerProduct > 0.1)
                        CurrentState = (int)GoalieStates.KickToRobot;
                    else if (oppBallOwner.HasValue && Model.Opponents.ContainsKey(oppBallOwner.Value) && Model.Opponents[oppBallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < .18 && Model.BallState.Location.X > 0 && Model.BallState.Speed.Size < 1 && ballisinfront && rahmatiMainState)
                    {
                        CurrentState = (int)GoalieStates.PreDive;
                    }
                    else if ((GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist, out dist2) && (acceptable2 > ballSpeed.Size || ballSpeed.Size < maxSpeedToGet)) || ((GameParameters.IsInDangerousZone(ballState.Location, false, -.1, out dist, out dist2))))//WC 2017
                        CurrentState = (int)GoalieStates.InPenaltyArea;
                    else if (inf2.DefenderPosition.HasValue && inf1.DefenderPosition.HasValue && FreekickDefence.Static2ID.HasValue && FreekickDefence.Static1ID.HasValue)// && new Line(ballState.Location, ballState.Location + ballState.Speed).IntersectWithLine(new Line(GameParameters.OurGoalCenter, Model.OurRobots[RobotID].Location), ref IntersectPoint))
                    {
                        if ((StaticDefender2CurrentPos.DistanceFrom(inf2.DefenderPosition.Value) < .07 && StaticDefenderCurrentPos.DistanceFrom(inf1.DefenderPosition.Value) < .07)) //&& IntersectPoint.DistanceFrom(GameParameters.OurGoalCenter) > Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter))
                            CurrentState = (int)GoalieStates.Normal;

                    }
                }
                #endregion
                #region Eat The Ball
                else if (CurrentState == (int)GoalieStates.EathTheBall)
                {
                    if (!eattheball(engine, Model, inf1, inf2, true))
                    {
                        FreekickDefence.EaththeBall = false;
                        FreekickDefence.StaticFirstState = DefenderStates.Normal;
                        FreekickDefence.StaticSecondState = DefenderStates.Normal;
                        CurrentState = (int)GoalieStates.Normal;
                    }
                }
                #endregion

            }
            FreekickDefence.CurrentStates[this] = CurrentState;
            currentState = CurrentState;

            int? nearestOpponent = null;
            if (Model.Opponents.Count > 0)
                nearestOpponent = Model.Opponents.OrderBy(r => r.Value.Location.DistanceFrom(Model.BallState.Location)).Select(r => r.Key).First();
            if (nearestOpponent.HasValue && Model.Opponents.ContainsKey(nearestOpponent.Value))
            {
                robotAngle.Enqueue(Model.Opponents[nearestOpponent.Value].Angle.Value);
                robotDistance.Enqueue(Model.Opponents[nearestOpponent.Value].Location.DistanceFrom(Model.BallState.Location));
            }
            ballstates.Enqueue(Model.BallState.Location);
            if (robotAngle.Count > 100)
            {
                robotAngle.Dequeue();
                robotDistance.Dequeue();
                ballstates.Dequeue();
            }
            int kickFrame = 0;
            for (int i = 0; i < robotDistance.Count; i++)
            {
                if (robotDistance.ElementAt(i) < .12)
                {
                    kickFrame = i;
                }
            }
            if (ballstates.Count > kickFrame && robotAngle.Count > kickFrame)
            {
                if (Model.BallState.Speed.Size > 4 && Vector2D.IsBetween(GameParameters.OppGoalLeft - ballstates.ElementAt(kickFrame), GameParameters.OppGoalRight - ballstates.ElementAt(kickFrame), Vector2D.FromAngleSize(robotAngle.ElementAt(kickFrame) * Math.PI / 180, 1)))
                {
                    currentState = (int)GoalieStates.KickToGoal;
                }
            }

            DrawingObjects.AddObject(new StringDraw(((GoalieStates)CurrentState).ToString(), GameParameters.OurGoalCenter.Extend(0.3, 0)), "gstate");
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Goalie;
        }

        double PistonDistance(Position2D target)
        {
            Position2D startPiston = GameParameters.OurGoalCenter + (target - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.OurGoalCenter.X - 1.77);
            double dist = target.DistanceFrom(GameParameters.OurGoalCenter);
            return Math.Max(Math.Min((0.2899 * dist) - 0.2594, .9), .4);



        }

        public bool BallKickedToOurGoal(WorldModel Model)
        {

            double tresh = 0.20;
            double tresh2 = 1.3;
            if ((GoalieStates)currentState == GoalieStates.KickToGoal)
            {
                tresh = 0.23;
                tresh2 = 1.4;
            }
            Line line = new Line();
            line = new Line(BallState.Location, BallState.Location - BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            DrawingObjects.AddObject(new StringDraw((d / BallState.Speed.Size < tresh2).ToString(), new Position2D(-1, 0)));
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + tresh && BallGoal.Y > GameParameters.OurGoalRight.Y - tresh)
                if (BallState.Speed.InnerProduct(GameParameters.OurGoalRight - BallState.Location) > 0)
                    if (BallState.Speed.Size > 0.1 && d / BallState.Speed.Size < tresh2)
                        return true;
            return false;
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return 20 * RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new StaticGoalieRole() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public enum GoalieStates
        {
            Normal = 0,
            InPenaltyArea = 1,
            KickToGoal = 2,
            KickToRobot = 3,
            Rahmati = 4,
            Piston = 5,
            EathTheBall = 6,
            OpponentInPassState = 7,
            PreDive = 8
        }

        public class BallDetails
        {
            public Vector2D Speed = new Vector2D();
            public Position2D? OurGoalIntersect = new Position2D();
            public Position2D EntryPoint = new Position2D();
            public Position2D ExitPoint = new Position2D();
            public Position2D Location = new Position2D();
        }

        public BallDetails GetBallIntersectWithArea(WorldModel model)
        {
            Circle c = new Circle(GameParameters.OurGoalCenter, 0.95);
            Line l = new Line(model.BallState.Location, ballState.Location + ballState.Speed);

            List<Position2D> intersect = c.Intersect(l);
            double min = double.MaxValue;
            Position2D pos = new Position2D();
            if (intersect.Count > 0)
            {
                min = intersect.Min(m => m.DistanceFrom(model.BallState.Location));
                pos = intersect.First(f => f.DistanceFrom(model.BallState.Location) == min);
            }
            Line l2 = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
            Position2D? intersect2 = l.IntersectWithLine(l2);
            BallDetails bd = null;
            if (intersect.Count > 0 && intersect2.HasValue)
            {
                bd = new BallDetails();
                if (intersect.Count == 1)
                {
                    bd.EntryPoint = intersect[0];
                    bd.ExitPoint = intersect[0];
                }
                else if (Vector2D.InnerProduct(model.BallState.Speed, (intersect[0] - ballState.Location)) <
                    Vector2D.InnerProduct(model.BallState.Speed, (intersect[1] - ballState.Location)))
                {
                    bd.EntryPoint = intersect[0];
                    bd.ExitPoint = intersect[1];
                }
                else
                {
                    bd.EntryPoint = intersect[1];
                    bd.ExitPoint = intersect[0];
                }

                bd.Location = ballState.Location;
                bd.OurGoalIntersect = intersect2.Value;
                bd.Speed = ballState.Speed;
            }
            return bd;
        }

        public Position2D TargetToKick(WorldModel Model, int robotID)
        {
            //double minopp = Model.Opponents.Min ( m => m.Value.Location.DistanceFrom ( ballState.Location ) );
            //double ourDist = Model.OurRobots [robotID].Location.DistanceFrom ( ballState.Location );
            //if ( minopp > 0.5 && ourDist < 0.2 )
            //    return GameParameters.OppGoalCenter;
            //return ballState.Location + ( ballState.Location - GameParameters.OurGoalCenter ).GetNormalizeToCopy ( 2 );
            Vector2D v = ballState.Location - GameParameters.OurGoalCenter;
            v = Vector2D.FromAngleSize(Math.Sign(v.AngleInRadians) * Math.Max(Math.Abs(v.AngleInRadians), (110.0).ToRadian()), v.Size);
            return ballState.Location + v.GetNormalizeToCopy(2);
            //return ballState.Location + ( -1 * BallState.Speed ).GetNormalizeToCopy ( 2 );
        }

        private bool InconmmingOutgoing(WorldModel Model, int RobotID, ref bool isNear)
        {
            Position2D temprobot = Model.Opponents[RobotID].Location + Model.Opponents[RobotID].Speed * 0.04;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + BallState.Speed * 0.04;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            if (BallState.Speed.Size > 2)
            {
                double coef = 1;
                if (LastRB == RBstate.Robot)
                    coef = 1.2;

                double ballspeedAngle = BallState.Speed.AngleInDegrees;
                double robotballInner = Model.Opponents[RobotID].Speed.InnerProduct((ballState.Location - Model.Opponents[RobotID].Location).GetNormnalizedCopy());
                bool ballinGoal = false;
                Line line = new Line();
                line = new Line(BallState.Location, BallState.Location - BallState.Speed);
                Position2D BallGoal = new Position2D();
                BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
                double d = BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
                if (BallGoal.Y < GameParameters.OurGoalLeft.Y + .65 / coef && BallGoal.Y > GameParameters.OurGoalRight.Y - .65 / coef)
                    if (BallState.Speed.InnerProduct(GameParameters.OurGoalRight - BallState.Location) > 0)
                        ballinGoal = true;

                if (ballState.Speed.InnerProduct((temprobot - tempball).GetNormnalizedCopy()) > 1.2 / coef
                    && robotballInner < 2 * coef && robotballInner > -1
                    && !ballinGoal)
                    return true;

            }
            return false;
        }

        private int? StaticRB(GameStrategyEngine engine, WorldModel Model)
        {
            if (currentState == (int)GoalieStates.InPenaltyArea)
                return null;

            var opps = engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();
            //if ( opps.Count > 0 && GameParameters.IsInDangerousZone ( ballState.Location , false , 0.2 , out d1 , out d2 ) )
            //{
            //    if ( !GameParameters.IsInDangerousZone ( Model.Opponents [opps.First ()].Location , false , 0.03 , out d1 , out d2 ) )
            //    {
            //        LastRB = RBstate.Robot;
            //        return opps.First ();

            //    }
            //    else if ( opps.Count > 1 )
            //    {
            //        LastRB = RBstate.Robot;
            //        return opps.Skip ( 1 ).First ();
            //    }
            //    else
            //        return null;
            //}
            //else if ( opps.Count > 0 && GameParameters.IsInDangerousZone ( Model.Opponents [opps.First ()].Location , false , 0.03 , out d1 , out d2 ) )
            //{
            //    return null;
            //}
            if (opps.Count == 0 || BallState.Speed.Size < 1)
                return null;

            SingleObjectState oppstate = Model.Opponents[opps.First()];


            Position2D temprobot = oppstate.Location + oppstate.Speed * 0.2;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + ballState.Speed * 0.2;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            SingleObjectState ball = ballState;
            Vector2D ballRobot = temprobot - tempball;
            Vector2D robotTarget = GameParameters.OurGoalCenter - temprobot;
            double ballAngle = Vector2D.AngleBetweenInDegrees(ballRobot, robotTarget);
            bool incomningNear = false;
            if (InconmmingOutgoing(Model, opps.First(), ref incomningNear))
            {
                LastRB = RBstate.Robot;
                return opps.First();
            }
            return null;
        }

        private bool eattheball(GameStrategyEngine engine, WorldModel model, DefenceInfo inf1, DefenceInfo inf2, bool fornormal)
        {
            int? id = StaticRB(engine, model);
            SingleObjectState defenceSate = (id.HasValue) ? model.Opponents[id.Value] : ballState;

            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender1)))
                inf1 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender1)).First();

            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(StaticDefender2)))
                inf2 = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(StaticDefender2)).First();


            Defender1Delayed = false;

            if (StaticDefender1IDG.HasValue && inf1 != null && inf1.DefenderPosition.HasValue && model.OurRobots.ContainsKey(StaticDefender1IDG.Value))
            {
                if (model.OurRobots[StaticDefender1IDG.Value].Location.DistanceFrom(inf1.DefenderPosition.Value) > .02)
                {
                    Defender1Delayed = true;
                }
            }
            Defender2Delayed = false;
            if (StaticDefender2IDG.HasValue && inf2 != null && inf2.DefenderPosition.HasValue && model.OurRobots.ContainsKey(StaticDefender2IDG.Value))
            {
                if (model.OurRobots[StaticDefender2IDG.Value].Location.DistanceFrom(inf2.DefenderPosition.Value) > .02)
                {
                    Defender2Delayed = true;
                }
            }
            if (StaticDefender1IDG != null && StaticDefender2IDG != null && StaticDefender1IDG.HasValue && StaticDefender2IDG.HasValue && model.OurRobots.ContainsKey(StaticDefender1IDG.Value) && model.OurRobots.ContainsKey(StaticDefender2IDG.Value))
            {
                double histersis = (fornormal) ? .02 : 0;
                Line leftRight = new Line(model.OurRobots[StaticDefender1IDG.Value].Location, model.OurRobots[StaticDefender2IDG.Value].Location);
                if (leftRight.Tail.DistanceFrom(leftRight.Head) < RobotParameters.OurRobotParams.Diameter + .04)
                {
                    Vector2D leftrightvector = leftRight.Tail - leftRight.Head;

                    Position2D middle = new Position2D((leftRight.Head.X + leftRight.Tail.X) / 2, (leftRight.Head.Y + leftRight.Tail.Y) / 2);

                    Position2D preppos = leftrightvector.PrependecularPoint(model.OurRobots[StaticDefender1IDG.Value].Location, model.BallState.Location);


                    double distballfromline = preppos.DistanceFrom(model.BallState.Location);

                    if (distballfromline < .09 && model.BallState.Location.DistanceFrom(middle) < .13)
                    {
                        if (model.BallState.Speed.Size < .2)
                            return true;
                    }
                }
            }
            return false;
        }

        // time from current pos
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
        // total time of path
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
        // time of arriving to intersect when we want to stop in the intersect pos
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
        // time of arriving to intersect when we dont to stop in the intersect pos
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

        public enum InPenaltyMode
        {
            skip,
            goActive,
            GoIntersect,
            nothing
        }
    }

}
