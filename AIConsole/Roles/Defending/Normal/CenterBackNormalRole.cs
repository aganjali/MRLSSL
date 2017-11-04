using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Plays;
using System.Drawing;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.AIConsole.Roles
{
    class CenterBackNormalRole : RoleBase
    {
#if NEW
        Position2D lastopploc;
        int markid;
        int CurrentBallSide = 0;
        //   GetBallRole b = null;
        public Position2D lastTarget = new Position2D();
        public Position2D Target = new Position2D();
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();

        public static double staticAngle = 0;
        public static bool angleUpdateFlag = false;

        static int WantedRobotID = 0;

        static bool onceaTime = true;
        static int ballSign = -1;
        double lastAngle = 0;


        public static Position2D lackState = new Position2D();

        public CenterBackNormalRole()
        {
        }
        int lastCurrentSide = (int)SelectSide.left;
        int currentSide = (int)SelectSide.left;
        Position2D lastTargetSide = new Position2D();

        public SingleWirelessCommand Run(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, double Teta, Dictionary<int, RoleBase> roles)
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
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(CenterBackNormalRole)))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(CenterBackNormalRole)).First();

            double teta = 180;
            bool robotAvoidance = false;
        #region new motion IO 2015
            if (FreekickDefence.newmotion)
            {
                if (inf != null && inf.DefenderPosition.HasValue && FreekickDefence.Static1ID.HasValue)
                {
                    bool isBall = (inf.TargetState.Type == ObjectType.Ball) ? true : false;

                    double distanceST1ToHisDefence = Model.OurRobots[FreekickDefence.Static1ID.Value].Location.DistanceFrom(inf.DefenderPosition.Value);

                    Vector2D ballGoalCenter = (Model.BallState.Location) - GameParameters.OurGoalCenter;
                    Line ballGoalcentere = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + ballGoalCenter);

                    double robot1distance = Math.Abs(Vector2D.AngleBetweenInDegrees(ballGoalCenter, Model.OurRobots[FreekickDefence.Static1ID.Value].Location - GameParameters.OurGoalCenter));

                    List<int> ids = new List<int>() { WantedRobotID };

                    if (!ids.Contains(FreekickDefence.StaticCBID.Value) || !Model.OurRobots.ContainsKey(WantedRobotID))
                    {
                        onceaTime = true;
                    }
                    if (onceaTime)
                    {
                        onceaTime = false;
                        WantedRobotID = FreekickDefence.StaticCBID.Value;
                        ballSign = Math.Sign(robot1distance);
                    }

                    //DrawingObjects.AddObject(new StringDraw("Wanted Robot", Model.OurRobots[WantedRobotID].Location.Extend(-.5, 0)), "65454564646");
                    //if (pursueRobotID == RobotID && Model.OurRobots[RobotID].Location.DistanceFrom(inf.DefenderPosition.Value) > .05)
                    //{
                    //    TargetPos = SwimingFish(Model, WantedRobotID, pursueRobotID, inf.DefenderPosition.Value, Math.Max(inf.DefenderPosition.Value.DistanceFrom(inf2.DefenderPosition.Value), .2), out robotAvoidance);
                    //    TargetPos = GameParameters.OurGoalCenter + (TargetPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState( TargetPos , Vector2D.Zero , 0f), (RobotParameters.OurRobotParams.Diameter/2 )));
                    //}
                    //else
                    //{
                    robotAvoidance = true;
                    //}
                    DrawingObjects.AddObject(new Circle(inf.DefenderPosition.Value, 0.13, new System.Drawing.Pen((inf.OppID.HasValue) ? System.Drawing.Color.Blue : System.Drawing.Color.Orange, 0.01f)));

                }

            }
            #endregion


        #region Normal
            if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.Normal)
            {
                List<Planning.GamePlanner.Types.VisibleGoalInterval> ourGoalinterval = new List<Planning.GamePlanner.Types.VisibleGoalInterval>();
                List<Planning.GamePlanner.Types.VisibleGoalInterval> oppGoalinterval = new List<Planning.GamePlanner.Types.VisibleGoalInterval>();

                List<int> oppRobotIds = new List<int>();
                List<int> ourRobotIds = new List<int>();
                oppRobotIds = Model.Opponents.Keys.Where(a => Model.Opponents.ContainsKey(a)).ToList();
                ourRobotIds = Model.OurRobots.Keys.Where(a => Model.OurRobots.ContainsKey(a)).ToList();

                CalculateGoalIntervals(Model, out ourGoalinterval, out oppGoalinterval, false, true, oppRobotIds, ourRobotIds);

                double maxdist = 0;
                Line maxline = new Line();
                Planning.GamePlanner.Types.VisibleGoalInterval maxInterval = new Planning.GamePlanner.Types.VisibleGoalInterval();
                DrawingObjects.AddObject(new Line(maxline.Tail, maxline.Head, new Pen(Brushes.Red, 0.02f)), maxline.Tail.Y.ToString() + " 56547 ");


                Position2D middlepos = new Position2D();
                Vector2D middlevec = new Vector2D();
                Line middleBallLine = new Line();
                foreach (var item in ourGoalinterval)
                {
                    if (Math.Abs(item.interval.Start - item.interval.End) > maxdist)
                    {
                        //DrawingObjects.AddObject(new Position2D(GameParameters.OurGoalCenter.X, item.interval.Start), item.interval.Start.ToString() + " 67536546");
                        //DrawingObjects.AddObject(new Position2D(GameParameters.OurGoalCenter.X, item.interval.End), item.interval.End.ToString() + " 67536546");
                        maxdist = Math.Abs(item.interval.Start - item.interval.End);
                        maxInterval = item;
                        maxline = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End));
                    }
                }
                middlevec = new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End) - new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start);
                middlevec = Vector2D.FromAngleSize(middlevec.AngleInRadians, middlevec.Size / 2);
                middlepos = new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start) + middlevec;
                Line middlePosToBallLine = new Line(Model.BallState.Location, middlepos);
                Line TransverseVerticalLine = new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location.Extend(0, 1));//Transverse Vertical Line
                Position2D? IntersectPosCenter = null;
                IntersectPosCenter = TransverseVerticalLine.IntersectWithLine(middlePosToBallLine);

                bool TargetIsball = true;
                Position2D targetSide = new Position2D();
                int? id = StaticRB(engine, Model);
                if (id.HasValue)
                {
                    if (LastRB == RBstate.Robot)
                    {
                        targetSide = Model.Opponents[id.Value].Location;
                        DrawingObjects.AddObject(new StringDraw("targetSide : Robot", Model.OurRobots[RobotID].Location + new Vector2D(-0.2, 0.2)), Model.OurRobots[RobotID].Location.Y.ToString() + "3564");
                        TargetIsball = false;
                    }
                    else if (LastRB == RBstate.Ball)
                    {
                        targetSide = Model.BallState.Location;
                        DrawingObjects.AddObject(new StringDraw("targetSide : Ball", Model.OurRobots[RobotID].Location + new Vector2D(-0.2, 0.2)), Model.OurRobots[RobotID].Location.Y.ToString() + "568787");
                        TargetIsball = true;
                    }
                }
                else
                {
                    targetSide = Model.BallState.Location;
                    DrawingObjects.AddObject(new StringDraw("targetSide : Ball", Model.OurRobots[RobotID].Location + new Vector2D(-0.2, 0.2)), Model.OurRobots[RobotID].Location.Y.ToString() + "568787");
                    TargetIsball = true;
                }
                Position2D targetpos = new Position2D();

                if (TargetIsball)
                {
                    targetpos = Model.BallState.Location;
                }
                else
                {
                    targetpos = Model.Opponents[id.Value].Location;
                }

                if (currentSide == (int)SelectSide.left)
                 {
        #region left
                    Position2D middleposStart = new Position2D();
                    //DrawingObjects.AddObject(new Circle(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), .02, new Pen(Brushes.Red, 0.02f)), maxInterval.interval.Start.ToString() + " 5687");
                    //DrawingObjects.AddObject(new Circle(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End), .02, new Pen(Brushes.Blue, 0.02f)), maxInterval.interval.End.ToString() + " 5687");
                    Line startLine = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), targetSide);//Model.BallState.Location);
                    Line endLine = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End), targetSide);//Model.BallState.Location);
                    middleposStart = new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start);
                    //DrawingObjects.AddObject(new Circle(middleposStart, .02, new Pen(Brushes.Red, 0.02f)), middleposStart.Y.ToString() + " 5687");
                    Line middlePosStartBallLine = new Line(targetSide, middleposStart);
                    //DrawingObjects.AddObject(new Line(targetSide, middleposStart, new Pen(Brushes.Red, 0.01f)), middlePosStartBallLine.Tail.Y.ToString() + " 565436547 ");

                    TransverseVerticalLine = new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location.Extend(0, 1));//Transverse Vertical Line
                    Position2D? IntersectPos = null;
                    IntersectPos = TransverseVerticalLine.IntersectWithLine(middlePosStartBallLine);
                    if (IntersectPos.HasValue)
                    {
                        if (GameParameters.IsInField(Model.BallState.Location, 0.05))
                        {
                            Vector2D vvv = new Vector2D();//= middleposStart - targetSide;
                            Line L1 = new Line(middleposStart, targetSide);
                            List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(L1, true);
                            double d = 0;
                            if (intersects.Count > 0)
                            {
                                vvv = intersects.FirstOrDefault() - middleposStart;
                                d = vvv.Size;
                            }
                            targetSide = middleposStart + vvv.GetNormalizeToCopy(d + 0.2/*size*/);
                            if (targetpos.Y >= 0 && targetSide.Y <= 0 && lastTargetSide != Position2D.Zero)
                                targetSide = lastTargetSide;
                            else
                                lastTargetSide = targetSide;
                        }
                        else
                            targetSide = lastTargetSide;
                    }
                    #endregion
                }
                else if (currentSide == (int)SelectSide.right)
                {
        #region right
                    Position2D middleposEnd = new Position2D();
                    //DrawingObjects.AddObject(new Circle(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), .02, new Pen(Brushes.Red, 0.02f)), maxInterval.interval.Start.ToString() + " 5687");
                    //DrawingObjects.AddObject(new Circle(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End), .02, new Pen(Brushes.Blue, 0.02f)), maxInterval.interval.End.ToString() + " 5687");
                    Line startLine = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.Start), targetSide);
                    Line endLine = new Line(new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End), targetSide);
                    middleposEnd = new Position2D(GameParameters.OurGoalCenter.X, maxInterval.interval.End);
                    //DrawingObjects.AddObject(new Circle(middleposEnd, .02, new Pen(Brushes.Red, 0.02f)), middleposEnd.Y.ToString() + " 5687");
                    Line middlePosEndBallLine = new Line(targetSide, middleposEnd);
                    //DrawingObjects.AddObject(new Line(targetSide, middleposEnd, new Pen(Brushes.Red, 0.01f)), middlePosEndBallLine.Tail.Y.ToString() + " 565436547 ");

                    TransverseVerticalLine = new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location.Extend(0, 1));//Transverse Vertical Line
                    Position2D? IntersectPos = null;
                    IntersectPos = TransverseVerticalLine.IntersectWithLine(middlePosEndBallLine);
                    if (IntersectPos.HasValue)
                    {
                        if (GameParameters.IsInField(Model.BallState.Location, 0.05))
                        {
                            Vector2D vvv = new Vector2D();//= middleposStart - targetSide;
                            Line L1 = new Line(middleposEnd, targetSide);
                            List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(L1, true);
                            double d = 0;
                            if (intersects.Count > 0)
                            {
                                vvv = intersects.FirstOrDefault() - middleposEnd;
                                d = vvv.Size;
                            }
                            targetSide = middleposEnd + vvv.GetNormalizeToCopy(d + 0.2/*size*/);
                            if (targetpos.Y <= 0 && targetSide.Y >= 0 && lastTargetSide!=Position2D.Zero)
                                targetSide = lastTargetSide;
                            else
                                lastTargetSide = targetSide;

                        }
                        else
                            targetSide = lastTargetSide;
                        
                    }
                    #endregion
                }
        #region extend targetside
                
                //DrawingObjects.AddObject(new Circle(targetSide, 0.02f, new Pen(Brushes.Pink, 0.02f)), targetSide.Y.ToString() + "537643567");
                //
                Position2D extendTargetSide = targetSide;
                
                if (true)
                {
                    Vector2D Sideline = (currentSide == (int)SelectSide.left ? GameParameters.OurGoalRight : GameParameters.OurGoalLeft) - targetpos;
                    Vector2D middlePosToBallVec = middlePosToBallLine.Tail - middlePosToBallLine.Head;
                    //DrawingObjects.AddObject(middlePosToBallLine);
                    Line bisector = Vector2D.Bisector(middlePosToBallVec, Sideline, targetpos);

                    Vector2D bisectorVector = bisector.Tail - bisector.Head;
                    Position2D? IntersectPrep = bisector.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location).IntersectWithLine(bisector);
                    if (IntersectPrep.HasValue && IntersectPrep.Value.X > Model.BallState.Location.X)
                    {
                        Position2D? intersectWhiteGoslLine = (new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight)).IntersectWithLine(bisector);
                        Vector2D intersectWhiteGoslLineVector = (intersectWhiteGoslLine.Value - bisector.Head);
                        Line L1 = new Line(intersectWhiteGoslLine.Value, targetpos);
                        List<Position2D> intersects = GameParameters.LineIntersectWithDangerZone(L1, true);
                        if (intersects.Count > 0 && intersectWhiteGoslLine.HasValue)
                        {
                            intersectWhiteGoslLineVector = intersects.OrderBy(y => y.DistanceFrom(L1.Tail)).FirstOrDefault() - intersectWhiteGoslLine.Value;
                            intersectWhiteGoslLineVector = Vector2D.FromAngleSize(intersectWhiteGoslLineVector.AngleInRadians, intersectWhiteGoslLineVector.Size + (GameDefinitions.RobotParameters.OurRobotParams.Diameter));
                            extendTargetSide = intersectWhiteGoslLine.Value + intersectWhiteGoslLineVector;
                            if (extendTargetSide.DistanceFrom(targetSide) >0.1 /*(GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2)*/)
                            {
                                double Tresh = 0.02;
                                Vector2D extendTargetSideVecToTargetSide = (extendTargetSide - targetSide).GetNormalizeToCopy(GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2 + Tresh);
                                extendTargetSide = targetSide + extendTargetSideVecToTargetSide;
                            }
                        }
                        else
                        {
                            extendTargetSide = IntersectPrep.Value;
                        }
                    }
                }
                DrawingObjects.AddObject(new Line(targetSide, extendTargetSide, new Pen(Brushes.Pink, 0.01f)), targetSide.Y.ToString() + "654764");
                DrawingObjects.AddObject(extendTargetSide, extendTargetSide.Y.ToString() + "654764");
                #endregion
                double dist1, dist2;
                bool flag = true;
                Position2D posToGo=targetSide;

                if (extendTargetSide != Position2D.Zero && GameParameters.IsInField(extendTargetSide, 0.05) && !GameParameters.IsInDangerousZone(extendTargetSide, false, 0.05, out dist1, out dist2))
                {
                    flag = true;
                    posToGo = extendTargetSide;
                }
                else
                {
                    flag = false;
                    posToGo = targetSide;
                }
                if (Model.OurRobots[RobotID].Location.DistanceFrom(posToGo) > 0.1)
                {
                    teta = lastAngle;
                }
                else
                {
                    teta = (targetpos - Model.OurRobots[RobotID].Location).AngleInDegrees;
                    
                    lastAngle = teta;
                }
                
                if (targetpos.X > GameParameters.OurGoalCenter.X)
                {
                    Line l = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                    Position2D? p = GameParameters.LineIntersectWithDangerZone(l, true).FirstOrDefault();
                    if (p.HasValue && p.Value != Position2D.Zero)
                    {
                        if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) - p.Value.DistanceFrom(GameParameters.OurGoalCenter) > 0.25)
                        {
                            targetpos = p.Value + ((p.Value - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.2));
                        }
                    }
                }
                Planner.Add(RobotID, posToGo, teta, PathType.UnSafe, false, false, true, false);
                DrawingObjects.AddObject(new Circle(posToGo, .02, new Pen(Brushes.Silver, .03f)), "5d47ty554dfgd");
                FreekickDefence.CenterCurrentPosition = posToGo;
                
                return new SingleWirelessCommand();

            }
            #endregion
        #region BallInFront
            else if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.BallInFront)
            {
                bool GetBallSkillFlag = true;
                Vector2D GoalCenterToBallVect = Model.BallState.Location - GameParameters.OurGoalCenter;
                Line l1 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + GoalCenterToBallVect.GetNormalizeToCopy(10));
                Position2D? ip = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                GetSkill<GetBallSkill>().SetAvoidDangerZone(true, true);
                Circle c1=new Circle(Model.BallState.Location,0.12);
                if (ip.HasValue)
                {
                    GetBallSkillFlag = Model.BallState.Location.DistanceFrom(ip.Value) > GameDefinitions.RobotParameters.OurRobotParams.Diameter  ? true : false;
                }
                if (!GetBallSkillFlag)
                {
                    double tresh = 0.2;
                    if (ip.HasValue)
                    {
                        Target = ip.Value + GoalCenterToBallVect.GetNormalizeToCopy(tresh);
                        Planner.Add(RobotID, Target, (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, false, true, false);
                    }
                }
                else if (GetBallSkillFlag)// && Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value) &&!c1.IsInCircle(Model.OurRobots[Model.GoalieID.Value].Location))
                {
                    Line l = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                    Position2D? p = GameParameters.LineIntersectWithDangerZone(l, true).FirstOrDefault();
                    if (p.HasValue && p.Value != Position2D.Zero)
                    {
                        if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) - p.Value.DistanceFrom(GameParameters.OurGoalCenter) > .2)
                        {
                            GetSkill<GetBallSkill>().SetAvoidDangerZone(false, true);
                        }
                    }
                    GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, TargetToKick(Model, RobotID));
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, new List<int>());
                    if (!obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(Model.OurRobots[RobotID].Location + Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, .5), Vector2D.Zero, 0f), .04))
                    {
                        bool flag = false;
                        Vector2D RobotVect = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 9);
                        Line GoalLine = new Line(GameParameters.OurLeftCorner, GameParameters.OurRightCorner);

                        if (RobotVect.AngleInDegrees < 90 && RobotVect.AngleInDegrees > -90)
                        {
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            Planner.AddKick(RobotID, kickPowerType.Speed, true, 8);
                        }
                    }
                }
                if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)>1.7)
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                }
                return new SingleWirelessCommand();
            }
        #endregion
        #region KickToGoal
            else if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.KickToGoal)
            {
                double angle = 0;
                Position2D targetpos = Cost(engine, Model, RobotID, Model.OurRobots[RobotID].Location, out angle, (int)CenterDefenderStates.KickToGoal);
                Planner.Add(RobotID, targetpos, angle, PathType.UnSafe, false, true, true, true);
                return new SingleWirelessCommand();
            }
        #endregion
        #region Behind
            else if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.Behind)
            {
                double angle = 0;
                //goto ball side
                Position2D targetpos= Cost(engine, Model, RobotID, Model.OurRobots[RobotID].Location, out angle, (int)CenterDefenderStates.Behind);
                Circle c1 = new Circle(Model.BallState.Location, 0.3);
                double tresh = 0.15;
                Pen pen = new Pen(Brushes.DarkRed, 0.02f);
                //goto ball front
                //angle = (/*Model.BallState.Location*/GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees;
                //Vector2D GoalCenterToBallVect = Model.BallState.Location - GameParameters.OurGoalCenter;
                //Line l1 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + GoalCenterToBallVect.GetNormalizeToCopy(10));
                //Position2D? ip = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                //tresh = 0.2;
                //if (ip.HasValue && ip.Value != Position2D.Zero)
                //{
                //    targetpos = ip.Value + GoalCenterToBallVect.GetNormalizeToCopy(tresh);
                //}
                //if (Model.OurRobots.ContainsKey(Model.GoalieID.Value) && c1.IsInCircle(Model.OurRobots[Model.GoalieID.Value].Location) && c1.IsInCircle(Model.OurRobots[RobotID].Location))
                //{
                //    tresh = 0.15;
                //    if (ip.HasValue)
                //    {
                //        targetpos = ip.Value + GoalCenterToBallVect.GetNormalizeToCopy(tresh);
                //    }
                //}
                ////

                //if (c1.IsInCircle(Model.OurRobots[RobotID].Location))
                //{
                //    pen = new Pen(Brushes.Yellow, 0.02f);
                //    DrawingObjects.AddObject(new StringDraw("CenterBack: i'm here", GameParameters.OurRightCorner.Extend(-0.5, 0.5)), c1.Center.X.ToString() + "6868");
                //}

                //if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value))
                //{
                //    if (c1.IsInCircle(Model.OurRobots[Model.GoalieID.Value].Location) && c1.IsInCircle(Model.OurRobots[RobotID].Location))
                //    {
                //        pen = new Pen(Brushes.YellowGreen, 0.02f);
                //        DrawingObjects.AddObject(new StringDraw("GoalKeeper & CenterBack: we're here", GameParameters.OurRightCorner.Extend(-0.5, -0.5)), c1.Center.X.ToString() + "6868");
                //    }
                //    else if (c1.IsInCircle(Model.OurRobots[Model.GoalieID.Value].Location))
                //    {
                //        pen = new Pen(Brushes.Violet, 0.02f);
                //        DrawingObjects.AddObject(new StringDraw("GoalKeeper: i'm here", GameParameters.OurRightCorner.Extend(-0.5, -0.5)), c1.Center.X.ToString() + "6868");
                //    }

                //}
                //double maxDist=double.MaxValue;
                //double minDist = 0.25;
                //bool pushOppRobot=false;
                //int? oppId = null;
                //foreach (var item in Model.Opponents.Keys)
                //{
                //    if (c1.IsInCircle(Model.Opponents[item].Location))
                //    {
                //        if (Model.BallState.Location.DistanceFrom(Model.Opponents[item].Location) < maxDist && Model.BallState.Location.DistanceFrom(Model.Opponents[item].Location)< minDist)
                //        {
                //            maxDist = Model.BallState.Location.DistanceFrom(Model.Opponents[item].Location);
                //            oppId = item;
                //        }
                //    }
                //}
                //if (maxDist <= minDist && oppId.HasValue && Model.Opponents.ContainsKey(oppId.Value))
                //{
                //    pushOppRobot = true;
                //}
                //else
                //    pushOppRobot = false;
                //
                //if (!pushOppRobot)
                //{
                //    Planner.Add(RobotID, targetpos, angle, PathType.UnSafe, false, true, true, true);
                //}
                //else if (pushOppRobot)
                //{
                //    targetpos = Model.OurRobots[RobotID].Location+ (Model.Opponents[oppId.Value].Location - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(0.15);
                //    Planner.Add(RobotID, targetpos, angle, PathType.UnSafe, false, true, true, true);
                //}

                Vector2D BallToGoalCenter = Model.BallState.Location - GameParameters.OurGoalCenter;
                if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value) && c1.IsInCircle(Model.OurRobots[Model.GoalieID.Value].Location))
                {
                    tresh = 0.10;
                }
                else if (Model.GoalieID.HasValue && Model.OurRobots.ContainsKey(Model.GoalieID.Value) && !c1.IsInCircle(Model.OurRobots[Model.GoalieID.Value].Location))
                {
                    tresh = 0.15;
                }
                Vector2D RobotToGoalCenter = BallToGoalCenter.GetNormalizeToCopy(BallToGoalCenter.Size + tresh);
                targetpos = GameParameters.OurGoalCenter + RobotToGoalCenter;
                DrawingObjects.AddObject(new Circle(c1.Center, c1.Radious, pen), c1.Center.X.ToString() + "532456");
                DrawingObjects.AddObject(new Circle(targetpos, 0.09, new Pen(Brushes.Blue, 0.02f)), targetpos.X.ToString() + "szdf5g46");

                Planner.Add(RobotID, targetpos, angle, PathType.UnSafe, false, true, true, true);
                return new SingleWirelessCommand();
            }
        #endregion
        #region InPenaltyArea
            else if ( FreekickDefence.centerCurrentState == (int)CenterDefenderStates.InPenaltyArea)
            {
                //bool gotoKickBall = true;

                double angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;//180;
                Position2D targetpos = Cost(engine, Model, RobotID, Model.OurRobots[RobotID].Location, out angle, (int)CenterDefenderStates.InPenaltyArea);
                angle = (Model.OurRobots[RobotID].Location - Model.BallState.Location).AngleInDegrees;
                
                Circle c1 = new Circle(Model.BallState.Location, 0.5);
                double maxDist = double.MaxValue;
                double minDist = 0.25;
                bool pushOppRobot = false;
                int? oppId = null;
                foreach (var item in Model.Opponents.Keys)
                {
                    if (c1.IsInCircle(Model.Opponents[item].Location))
                    {
                        if (Model.BallState.Location.DistanceFrom(Model.Opponents[item].Location) < maxDist && Model.BallState.Location.DistanceFrom(Model.Opponents[item].Location) < minDist)
                        {
                            maxDist = Model.BallState.Location.DistanceFrom(Model.Opponents[item].Location);
                            oppId = item;
                        }
                    }
                }
                if (maxDist <= minDist && oppId.HasValue && Model.Opponents.ContainsKey(oppId.Value))
                {
                    pushOppRobot = true;
                }
                else
                    pushOppRobot = false;
                if (pushOppRobot)
                {
                    targetpos = Model.OurRobots[RobotID].Location + (Model.Opponents[oppId.Value].Location - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(0.15);
                    Planner.Add(RobotID, targetpos, angle, PathType.UnSafe, false, true, true, true);
                }
                else if (!pushOppRobot)
                {
                    Planner.Add(RobotID, targetpos, angle, PathType.UnSafe, false, true, true, true);
                }

                return new SingleWirelessCommand();
            }
        #endregion
        #region EaththeBall
            if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.EatTheBall)
            {
                if (FreekickDefence.EaththeBall)
                {
                    if (firstTime2)
                    {
                        firstTime2 = false;
                        eatballTarget = TargetPos;

                        teta = Teta;
                        avoidance = false;
                        robotAvoidance = false;
                        FreekickDefence.ReadyForEatStaticCB = true;
                    }
                }
                else
                {
                    FreekickDefence.ReadyForEatStaticCB = false;
                    firstTime2 = true;
                    avoidance = true;
                }

                if (FreekickDefence.ReadyForEatStaticCB)
                {
                    Target = eatballTarget;
                }
                else
                {
                    Target = TargetPos;
                }
                lackState = TargetPos;
                if (!FreekickDefence.newmotion)
                {
                    avoidance = true;
                    robotAvoidance = true;
                }
                //new code
            }
        #endregion


            DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, .1, new Pen(Brushes.Silver, .03f)), "565465465456");

            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 10, 10);
            Planner.Add(RobotID, Target, teta, PathType.UnSafe, false, robotAvoidance, avoidance, avoidance);

            return new SingleWirelessCommand();
        }

        #region new functions
        private int? StaticRB(GameStrategyEngine engine, WorldModel Model)
        {
            if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.InPenaltyArea)
                return null;

            Position2D? g;
            var opps = engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).Select(s => s.Key).ToList();
            if (opps.Count == 0 || Model.BallState.Speed.Size < 1)
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

        private RBstate LastRB = RBstate.Ball;

        private bool InconmmingOutgoing(WorldModel Model, int RobotID, ref bool isNear)
        {
            Position2D temprobot = Model.Opponents[RobotID].Location + Model.Opponents[RobotID].Speed * 0.04;
            temprobot.X = Math.Max(temprobot.X, GameParameters.OppGoalCenter.X + 0.05);
            temprobot.X = Math.Min(temprobot.X, GameParameters.OurGoalCenter.X - 0.05);
            temprobot.Y = Math.Max(temprobot.Y, GameParameters.OurRightCorner.Y + 0.05);
            temprobot.Y = Math.Min(temprobot.Y, GameParameters.OurLeftCorner.Y - 0.05);


            Position2D tempball = ballState.Location + Model.BallState.Speed * 0.04;
            tempball.X = Math.Max(tempball.X, GameParameters.OppGoalCenter.X + 0.05);
            tempball.X = Math.Min(tempball.X, GameParameters.OurGoalCenter.X - 0.05);
            tempball.Y = Math.Max(tempball.Y, GameParameters.OurRightCorner.Y + 0.05);
            tempball.Y = Math.Min(tempball.Y, GameParameters.OurLeftCorner.Y - 0.05);


            if (Model.BallState.Speed.Size > 2)
            {
                double coef = 1;
                if (LastRB == RBstate.Robot)
                    coef = 1.2;

                double ballspeedAngle = Model.BallState.Speed.AngleInDegrees;
                double robotballInner = Model.Opponents[RobotID].Speed.InnerProduct((ballState.Location - Model.Opponents[RobotID].Location).GetNormnalizedCopy());
                bool ballinGoal = false;
                Line line = new Line();
                line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
                Position2D BallGoal = new Position2D();
                BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
                double d = Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
                if (BallGoal.Y < GameParameters.OurGoalLeft.Y + .65 / coef && BallGoal.Y > GameParameters.OurGoalRight.Y - .65 / coef)
                    if (Model.BallState.Speed.InnerProduct(GameParameters.OurGoalRight - Model.BallState.Location) > 0)
                        ballinGoal = true;

                if (ballState.Speed.InnerProduct((temprobot - tempball).GetNormnalizedCopy()) > 1.2 / coef
                    && robotballInner < 2 * coef && robotballInner > -1
                    && !ballinGoal)
                    return true;

            }
            return false;
        }


        public bool BallKickedToOurGoal(WorldModel Model)
        {
            double tresh = 0.20;
            double tresh2 = 1.3;
            if ((DefenderStates)FreekickDefence.centerCurrentState == DefenderStates.KickToGoal)
            {
                tresh = 0.23;
                tresh2 = 1.4;
            }
            Line line = new Line();
            line = new Line(Model.BallState.Location, Model.BallState.Location - Model.BallState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            DrawingObjects.AddObject(new StringDraw((d / Model.BallState.Speed.Size < tresh2).ToString(), new Position2D(-1, 0)));
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + tresh && BallGoal.Y > GameParameters.OurGoalRight.Y - tresh)
                if (Model.BallState.Speed.InnerProduct(GameParameters.OurGoalRight - Model.BallState.Location) > 0)
                    if (Model.BallState.Speed.Size > 0.1 && d / Model.BallState.Speed.Size < tresh2)
                        return true;
            return false;
        }

        public void CalculateGoalIntervals(WorldModel Model, out List<VisibleGoalInterval> ourGoalIntervals, out List<VisibleGoalInterval> oppGoalIntervals, bool useOpp, bool useOur, List<int> OppRobotIDToExclude, List<int> OurRobotIDToExclude)
        {
            ourGoalIntervals = GetVisibleGoalIntervals(Model, Model.BallState.Location, GameParameters.OurGoalLeft, GameParameters.OurGoalRight, useOpp, useOur, OppRobotIDToExclude, OurRobotIDToExclude);
            oppGoalIntervals = GetVisibleGoalIntervals(Model, Model.BallState.Location, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, useOpp, useOur, OppRobotIDToExclude, OurRobotIDToExclude);
        }

        private List<VisibleGoalInterval> GetVisibleGoalIntervals(WorldModel Model, Position2D FromLocation, Position2D GoalStart, Position2D GoalEnd, bool UseOpponents, bool UseOurRobots, List<int> oppRobotIDsToExclude, List<int> ourRobotIDsToExclude)
        {
            List<VisibleGoalInterval> intervals = new List<VisibleGoalInterval>();
            Position2D goalCenter = Position2D.Interpolate(GoalStart, GoalEnd, 0.5);
            if (Model == null)
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), 1));
            }
            else
            {
                intervals.Add(new VisibleGoalInterval(new Interval((float)GoalStart.Y, (float)GoalEnd.Y), (float)(GoalEnd - GoalStart).Size * (float)Math.Sin(Math.Abs(Vector2D.AngleBetweenInRadians(GoalEnd - GoalStart, goalCenter - FromLocation)))));
                Vector2D centerDirection = goalCenter - FromLocation;
                if (UseOpponents)
                    if (Model.Opponents != null)
                        foreach (int oppID in Model.Opponents.Keys)
                            if (!oppRobotIDsToExclude.Contains(oppID))
                                if (centerDirection.InnerProduct(Model.Opponents[oppID].Location - FromLocation) > 0)
                                    ExcludeObstacle(intervals, new Circle(Model.Opponents[oppID].Location, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));

                if (UseOurRobots)
                {
                    List<int> robotIDsToExcludeList = new List<int>(ourRobotIDsToExclude);
                    foreach (int RobotID in Model.OurRobots.Keys)
                        if (!robotIDsToExcludeList.Contains(RobotID))
                            if (centerDirection.InnerProduct(Model.OurRobots[RobotID].Location - FromLocation) > 0)
                                ExcludeObstacle(intervals, new Circle(Model.OurRobots[RobotID].Location, 0.09f), FromLocation, centerDirection, goalCenter, new Line(GoalStart, GoalEnd));
                }
            }

            return intervals;
        }

        void ExcludeObstacle(List<VisibleGoalInterval> intervals, Circle obstacle, Position2D fromLocation, Vector2D centerDirection, Position2D goalCenter, Line goalLine)
        {
            if (intervals.Count == 0)
                return;
            List<Line> tangentLines;
            List<Position2D> tangentPoints;
            int tangents = obstacle.GetTangent(fromLocation, out tangentLines, out tangentPoints);
            Interval toExclude;
            if (tangents == 2)
                toExclude = new Interval(
                    GetExtreme(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], true, goalLine),
                    GetExtreme(fromLocation, tangentPoints[1], goalCenter, tangentLines[1], true, goalLine));
            else if (tangents == 1)
                toExclude = new Interval(
                    GetExtreme(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], true, goalLine),
                    GetExtreme(fromLocation, tangentPoints[0], goalCenter, tangentLines[0], false, goalLine)
                    );
            else //tangents == 0
            {
                Line l = new Line(fromLocation, obstacle.Center).PerpenducilarLineToPoint(fromLocation);
                toExclude = new Interval(
                    GetExtreme(fromLocation, fromLocation, goalCenter, l, true, goalLine),
                    GetExtreme(fromLocation, fromLocation, goalCenter, l, false, goalLine)
                    );
            }
            int i = 0;
            while (i < intervals.Count && intervals[i].interval.End <= toExclude.Start)
                i++;
            if (i < intervals.Count)
                if (intervals[i].interval.Start < toExclude.Start)
                {
                    double temp = intervals[i].interval.End;
                    intervals[i] = new VisibleGoalInterval(new Interval(intervals[i].interval.Start, toExclude.Start), intervals[i].ViasibleWidth);
                    i++;
                    if (temp > toExclude.End)
                    {
                        intervals.Insert(i, new VisibleGoalInterval(new Interval((float)toExclude.End, (float)temp), 0));
                        i++;
                    }
                }
            while (i < intervals.Count && intervals[i].interval.End < toExclude.End)
                intervals.RemoveAt(i);
            if (i < intervals.Count && intervals[i].interval.Start < toExclude.End)
                intervals[i] = new VisibleGoalInterval(new Interval(toExclude.End, intervals[i].interval.End), intervals[i].ViasibleWidth);
        }

        float GetExtreme(GPosition2D fromLocation, GPosition2D tangentPoint, GPosition2D goalCenter, GLine l, bool Pos, GLine goalLine)
        {
            GVector2D vect = tangentPoint.Sub(fromLocation);
            if (vect.SquareSize() == 0)
                vect = (Pos ? new GVector2D(l.B, l.A) : new GVector2D(-l.B, -l.A));
            if (sign(vect.X) == sign(goalCenter.X - fromLocation.X))
            {
                bool HasValue;
                GPosition2D pos = goalLine.IntersectWithLine(l, out HasValue);
                if (HasValue)
                    return pos.Y;
            }
            if (vect.Y < 0)
                return -1000;
            else
                return 1000;
        }

        static float sign(float x)
        {
            return ((x == 0) ? 0 : (x / (float)Math.Abs(x)));
        }
        bool hasBall = true;
        int hasBallCounter=0;
        void HasBall(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            bool currentHasBall = false;
            bool isInAngle = false;
            //if (hasBall & Model.BallState.Speed.Size < 0.2)
            //    goForIT = true;
            //else
            //    goForIT = false;
            if (!StaticVariables.FrameHasBall && hasBall)
                return;

            double robotAngle = Model.OurRobots[RobotID].Angle.Value;
            Vector2D robotBallVec = Model.BallState.Location - Model.OurRobots[RobotID].Location;
            double robotBallAngle = robotBallVec.AngleInDegrees < 0 ? robotBallVec.AngleInDegrees + 360 : robotBallVec.AngleInDegrees;
            double maxAngle = robotAngle + 35;
            if (maxAngle > 360)
                maxAngle -= 360;
            if (maxAngle < 0)
                maxAngle += 360;
            double minAngle = robotAngle - 35;
            if (minAngle > 360)
                minAngle -= 360;
            if (minAngle < 0)
                minAngle += 360;

            isInAngle = minAngle > maxAngle ? ((robotBallAngle > minAngle && robotBallAngle < 360) || (robotBallAngle > 0 && robotBallAngle < maxAngle)) : (robotBallAngle > minAngle && robotBallAngle < maxAngle);
            if (isInAngle)
                currentHasBall = hasBall ? !(robotBallVec.Size > 0.14) : (robotBallVec.Size < 0.12);
            else
                currentHasBall = false;

            if (currentHasBall)
                hasBallCounter++;
            else
                hasBallCounter--;

            if (hasBallCounter > 21)
                hasBallCounter = 21;
            else if (hasBallCounter < -1)
                hasBallCounter = -1;
            //DrawingObjects.AddObject(new StringDraw("Distanace:  " + robotBallVec.Size.ToString(), new Position2D(2, 2)), "sadasda");
            if (hasBallCounter < 0)
                hasBall = false;
            else if (hasBallCounter > 20)
                hasBall = true;
            DrawingObjects.AddObject(new StringDraw("hasBallCounter: " + hasBallCounter.ToString(), new Position2D(2, 2)), "fsdfdsf");
        }

        enum ballSide
        {
            Left,
            Right
        }

        #endregion

        public Position2D MarkFront(GameStrategyEngine engine, WorldModel Model, int RobotID, DefenceInfo info, double margin, out double Teta)
        {
            SingleObjectState Target = (info != null && info.OppID.HasValue) ? Model.Opponents[info.OppID.Value] : ballState;
            if (engine.GameInfo.OppTeam.Scores.Count > 0)
                Target = Model.Opponents[engine.GameInfo.OppTeam.Scores.OrderByDescending(o => o.Value).First().Key];

            Teta = (Target.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
            double min = GameParameters.SafeRadi(Target, margin);
            Position2D Pos = GameParameters.OurGoalCenter - (GameParameters.OurGoalCenter - Target.Location).GetNormalizeToCopy(min);
            Pos.DrawColor = System.Drawing.Color.Blue;
            DrawingObjects.AddObject(Pos);
            return Pos;

        }
        public Position2D Dive(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Position2D pos = new Position2D();
            Position2D robotLoc = Model.OurRobots[RobotID].Location;
            Position2D ballLoc = ballState.Location;
            Vector2D ballSpeed = ballState.Speed;
            Position2D prep = ballSpeed.PrependecularPoint(ballState.Location, Model.OurRobots[RobotID].Location);
            double dist, DistFromBorder, R;
            if (GameParameters.IsInDangerousZone(prep, false, 0, out dist, out DistFromBorder))
            {
                R = GameParameters.SafeRadi(new SingleObjectState(prep, new Vector2D(), 0), 0.05);
                pos = GameParameters.OurGoalCenter - ballSpeed.GetNormalizeToCopy(R);
            }
            else
                pos = prep;
            return pos;
        }
        public Position2D BehindSatate(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta)
        {
            double dist;
            Position2D target;
            if (inf != null && inf.OppID.HasValue)
            {
                if (FreekickDefence.CurrentStates.Any(a => a.Key.GetType() == typeof(GoalieNormalRole)) && (CenterDefenderStates)FreekickDefence.CurrentStates.Where(w => w.Key.GetType() == typeof(GoalieNormalRole)).First().Value == CenterDefenderStates.InPenaltyArea)
                    target = InPenaltyAreaState(engine, Model, inf, RobotID, out Teta);
                else if (Model.Opponents[inf.OppID.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) > 2)
                    target = GetBackBallPoint(Model, RobotID, out Teta);
                else
                    target = MarkFront(engine, Model, RobotID, inf, 0.1, out Teta);
            }
            else
                target = GetBackBallPoint(Model, RobotID, out Teta);

            return target;
        }
        public Position2D InPenaltyAreaState(GameStrategyEngine engine, WorldModel Model, DefenceInfo inf, int RobotID, out double Teta)
        {
            Vector2D vec;
            if (ballState.Location.Y >= 0)
                vec = new Vector2D(0, -0.2);
            else
                vec = new Vector2D(0, 0.2);
            double R = GameParameters.SafeRadi(ballState, 0.1);
            Position2D target = GameParameters.OurGoalCenter + (ballState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(R);
            target = target + vec;
            Teta = (ballState.Location - target).AngleInDegrees;
            return target;
        }
        public Position2D GetBackBallPoint(WorldModel Model, int RobotID, out double Teta)
        {
            Vector2D vec = ballState.Location - GameParameters.OurGoalCenter;
            Position2D tar = ballState.Location + vec;
            Vector2D ballSpeed = ballState.Speed;
            Position2D ballLocation = ballState.Location;
            Vector2D robotSpeed = Model.OurRobots[RobotID].Speed;
            Position2D robotLocation = Model.OurRobots[RobotID].Location;
            Vector2D robotBallVec = ballLocation - robotLocation;
            Vector2D ballTargetVec = tar - ballLocation;
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
            Teta = (vec).AngleInDegrees;
            return finalPosToGo;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            double dist1, dist2;
            double tresh = 0.05;
            double margin = 0.05;
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(CenterBackNormalRole)))
            {
                inf = FreekickDefence.CurrentInfos.Where(a => a.RoleType == typeof(CenterBackNormalRole)).First();
            }
            int? nullValue = null;
            int? ballOwner = engine.GameInfo.OurTeam.BallOwner.HasValue ? engine.GameInfo.OurTeam.BallOwner.Value : nullValue;
        #region Normal
            if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.Normal)
            {
                if (BallKickedToGoal(Model))
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.KickToGoal;
                }
                else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist1, out dist2))
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.InPenaltyArea;
                }
                else if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist1, out dist2)
                        && ballState.Location.X - Model.OurRobots[RobotID].Location.X < 0.5
                        && ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2)
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Behind;
                }
                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)< 3 && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)>(Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)+0.07))
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.BallInFront;
                }
                else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.OppIndDangerZone;
                }
                Line l = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                Position2D? p = GameParameters.LineIntersectWithDangerZone(l, true).FirstOrDefault();
                if (p.HasValue && p.Value != Position2D.Zero)
                {
                    if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) - p.Value.DistanceFrom(GameParameters.OurGoalCenter) > 0.3)
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.BallInFront;
                    } 
                    if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > 2)
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                    }
                }
            }
            #endregion Normal
        #region InPenaltyArea
            else if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.InPenaltyArea)
            {
                margin = 0.05;
                double eps = 0.02;
                if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist1, out dist2))
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                }
                else
                {
                    double d = ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter);
                    if (ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) >= tresh)
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                    }
                }
            }
        #endregion InPenaltyArea
        #region KickToGoal
            else if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.KickToGoal)
            {
                margin = 0.05;// = 0.1;
                if (!BallKickedToGoal(Model))
                {
                    if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist1, out dist2))
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.InPenaltyArea;
                    }
                    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)< 3 && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)>(Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)+0.07))
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.BallInFront;
                    }
                    else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.OppIndDangerZone;
                    }
                    else
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                    }
                }
            }
        #endregion KickToGoal
        #region BallInFront
            else if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.BallInFront)
            {
                margin = 0.05;
                if (engine.Status == GameStatus.Stop)
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                }
                else
                {
                    if (ballOwner.HasValue && (ballOwner.Value == RobotID || (Model.GoalieID.HasValue && Model.GoalieID == ballOwner.Value && !GameParameters.IsInDangerousZone(Model.BallState.Location, false,margin, out dist1, out dist2))) && engine.Status != GameStatus.Stop)
                    {
                        if (BallKickedToGoal(Model))
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.KickToGoal;
                        }
                        else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist1, out dist2))
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.InPenaltyArea;
                        }
                        else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.OppIndDangerZone;
                        }
                        else if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > 3)
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                        }
                        //if (Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location)>0.5)
                        //{
                        //    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                        //}
                        Line l = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                        Position2D? p = GameParameters.LineIntersectWithDangerZone(l, true).FirstOrDefault();
                        if (p.HasValue && p.Value != Position2D.Zero)
                        {
                            if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) - p.Value.DistanceFrom(GameParameters.OurGoalCenter) < 0.25)
                            {
                                FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                            }
                        }
                    }
                }
            }
        #endregion BallInFront
        #region Behind
            else if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.Behind)
            {
                margin = 0.05;
                if (engine.Status == GameStatus.Stop)
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                }
                else
                {
                    if (ballOwner.HasValue && (ballOwner.Value == RobotID || (Model.GoalieID.HasValue && Model.GoalieID == ballOwner.Value && !GameParameters.IsInDangerousZone(Model.BallState.Location, false, margin, out dist1, out dist2))) && engine.Status != GameStatus.Stop)
                    {
                        if (BallKickedToGoal(Model))
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.KickToGoal;
                        }
                        else if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist1, out dist2))
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.InPenaltyArea;
                        }
                        else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) < 3 && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)>(Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)+0.07))
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.BallInFront;
                        }
                        else if (inf != null && inf.TargetState.Type == ObjectType.Opponent && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < tresh)
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.OppIndDangerZone;
                        }
                        else if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter) > 3)
                        {
                            FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                        }
                    }
                    //else
                    //{
                    //    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                    //}
                }
                Line l = new Line(Model.BallState.Location, GameParameters.OurGoalCenter);
                Position2D? p = GameParameters.LineIntersectWithDangerZone(l, true).FirstOrDefault();
                if (p.HasValue && p.Value != Position2D.Zero)
                {
                    if (Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)-p.Value.DistanceFrom(GameParameters.OurGoalCenter) > 0.25)
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                    }
                }
            }
        #endregion
        #region OppIndDangerZone
            else if (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.OppIndDangerZone)
            {
                margin = 0.1;
                if (inf != null && (inf.TargetState.Type != ObjectType.Opponent) || (inf != null && inf.TargetState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) > tresh))
                {
                    if (GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist1, out dist2))
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.InPenaltyArea;
                    }
                    else if (!GameParameters.IsInDangerousZone(ballState.Location, false, margin, out dist1, out dist2)
                            && ballState.Location.X - Model.OurRobots[RobotID].Location.X < 0.5
                             && ballState.Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) < GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2)
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Behind;
                    }
                    else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)< 3 && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)>(Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)+0.07))
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.BallInFront;
                    }
                    else
                    {
                        FreekickDefence.centerCurrentState = (int)CenterDefenderStates.Normal;
                    }
                }
                else if (ballOwner.HasValue && ballOwner.Value == RobotID && engine.Status != GameStatus.Stop && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)< 3 && Model.BallState.Location.DistanceFrom(GameParameters.OurGoalCenter)>(Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter)+0.07))
                {
                    FreekickDefence.centerCurrentState = (int)CenterDefenderStates.BallInFront;
                }
            }
        #endregion OppIndDangerZone

        #region Debuge
            DrawingObjects.AddObject(new StringDraw("CenterBack State: "+((CenterDefenderStates)FreekickDefence.centerCurrentState).ToString(), GameParameters.OurRightCorner.Extend(0.5, 0.5)), RobotID.ToString() + "3568732");
            #endregion
            
        #region ball side
            bool change = true;
            if (ballOwner.HasValue && Model.Opponents.ContainsKey(ballOwner.Value) && Model.BallState.Location.DistanceFrom(Model.Opponents[ballOwner.Value].Location) < 0.23)
            {
                change = false;
            }
            if ( Model.BallState.Location.X <-.5 )
            {
                change = false;
            }
            //hysteresis
        #region use Hysteresis for change side
            if (change)
            {
                if (Model.BallState.Location.Y > 0.5)
                {
                    lastCurrentSide = currentSide;
                    currentSide = (int)SelectSide.center;
                }
                else if (Model.BallState.Location.Y < -0.5)
                {
                    lastCurrentSide = currentSide;
                    currentSide = (int)SelectSide.center;
                }
                else
                {
                    currentSide = lastCurrentSide;
                }
                if (currentSide == (int)SelectSide.center)
                {
                    if (Model.BallState.Location.Y > 0)
                    {
                        currentSide = (int)SelectSide.left;
                        lastCurrentSide = currentSide;
                    }
                    else
                    {
                        currentSide = (int)SelectSide.right;
                        lastCurrentSide = currentSide;
                    }                    
                }
            }
            FreekickDefence.CenterCurrentSide = currentSide;
            #endregion
            #endregion
        }

        private bool BallKickedToGoal(WorldModel Model)
        {
            Line line = new Line();
            line = new Line(ballState.Location, ballState.Location - ballState.Speed);
            Position2D BallGoal = new Position2D();
            BallGoal = line.CalculateY(GameParameters.OurGoalLeft.X);
            double d = ballState.Location.DistanceFrom(GameParameters.OurGoalCenter);
            if (BallGoal.Y < GameParameters.OurGoalLeft.Y + 0.23 && BallGoal.Y > GameParameters.OurGoalRight.Y - 0.23)
                if (ballState.Speed.InnerProduct(GameParameters.OurGoalRight - ballState.Location) > 0)
                    if (ballState.Speed.Size > 0.1 && d / ballState.Speed.Size < 2.2)
                        return true;
            return false;
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }
        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            //   DetermineNextState(engine, Model, RobotID, previouslyAssignedRoles);
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == this.GetType()))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == this.GetType()).First();
            // int state = FreekickDefence.CurrentStates.SingleOrDefault(s => s.Key.GetType() == this.GetType()).Value;
            if (inf != null)
            {

                Position2D pos = inf.DefenderPosition.Value;//Cost(engine, Model, RobotID, inf.DefenderPosition.Value, inf.Teta, state);
                double dist = pos.DistanceFrom(Model.OurRobots[RobotID].Location);
                //double dist = NewNormalPlay.calculatedDef1Temp.DistanceFrom(Model.OurRobots[RobotID].Location); 

                return dist * dist;
            }
            return 100;
        }
        public Position2D Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D TargetPos, out double angle, int state)
        {
            DefenceInfo inf = null;
            if (FreekickDefence.CurrentInfos.Any(a => a.RoleType == typeof(CenterBackNormalRole)))
                inf = FreekickDefence.CurrentInfos.Where(w => w.RoleType == typeof(CenterBackNormalRole)).First();

            double teta = 180;
            //if (state == (int)CenterDefenderStates.InPenaltyArea)
            //{
            //    Target = MarkFront(engine, Model, RobotID, inf, 0.1, out teta);//InPenaltyAreaState(engine, Model,inf, RobotID, out Teta);
            //}
            //else 
            if (state == (int)CenterDefenderStates.Behind)
            {
                Target = BehindSatate(engine, Model, inf, RobotID, out teta);
            }
            else if (state == (int)CenterDefenderStates.Normal)
            {
                Target = TargetPos;
                Vector2D vec = /*Target*/(new Position2D(GameParameters.OppGoalCenter.X, Model.OurRobots[RobotID].Location.Y)) - Model.OurRobots[RobotID].Location;
                Target = Target + vec.GetNormalizeToCopy(Math.Min(inf.TargetState.Speed.Size * 0.2, 0.08));
            }
            else if (state == (int)CenterDefenderStates.InPenaltyArea)
            {
                double tresh = 0.3;
                Vector2D GoalCenterToBallVect = Model.BallState.Location - GameParameters.OurGoalCenter;
                Line l1 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + GoalCenterToBallVect.GetNormalizeToCopy(10));
                Position2D? ip = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                if (ip.HasValue && ip.Value!=Position2D.Zero)
                {
                    Target = ip.Value + GoalCenterToBallVect.GetNormalizeToCopy(tresh);
                }
            }
            else if (state == (int)CenterDefenderStates.KickToGoal)
            {
                Target = Dive(engine, Model, RobotID);
                teta = Model.OurRobots[RobotID].Angle.Value;
            }
            else if (state == (int)CenterDefenderStates.BallInFront)
            {
                Target = GetBackBallPoint(Model, RobotID, out teta);
            }
            else if (state == (int)CenterDefenderStates.OppIndDangerZone)
            {
                if (inf != null)
                {
                    int? oppid = inf.OppID;
                    if (oppid.HasValue)
                    {
                        Vector2D vec = (Model.Opponents[oppid.Value].Location - Model.OurRobots[RobotID].Location);
                        Target = Model.Opponents[oppid.Value].Location + vec.GetNormalizeToCopy(0.2);
                        teta = (Target - Model.OurRobots[RobotID].Location).AngleInDegrees;
                    }
                }
            }
            if ((state == (int)CenterDefenderStates.Normal && inf != null && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X) || (FreekickDefence.centerCurrentState == (int)CenterDefenderStates.Behind && inf.TargetState.Location.X > GameParameters.OurGoalCenter.X))
            {
                Target = new Position2D(2.9, Target.Y);
            }
            angle = teta;
            return Target;
        }
        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            Position2D tempball = ballState.Location + ballState.Speed * 0.16;
            double d1, d2;

            List<RoleBase> res = new List<RoleBase>() { new CenterBackNormalRole(), new LeftBackMarkerNormalRole(), new RightBackMarkerNormalRole() };


            if (FreekickDefence.StaticCenterState == CenterDefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new ActiveRole());
            }
            if (FreekickDefence.StaticCenterState == CenterDefenderStates.BallInFront)
            {
                if (GameParameters.IsInField(tempball, 0.05) && !GameParameters.IsInDangerousZone(tempball, false, 0, out d1, out d2))
                    res.Add(new NewActiveRole());
            }
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }


        bool FreeToKick(GameStrategyEngine engine, WorldModel Model)
        {
            double accercy = 0.2;
            double minGoodnessToKick = 0.3;
            double KickAnyWayTresh = -1.7;
            double goodnessKick = 0;
            bool Chipkick = false;
            Position2D? PointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, ballState.Location, out goodnessKick, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null);


            bool FreeToKick = false;
            bool FreeToChip = false;
            bool Free = false;
            if (goodnessKick < minGoodnessToKick)
            {
                int? oppOwnerId = engine.GameInfo.OppTeam.BallOwner;
                //if (/*oppOwnerId.HasValue && */Model.Opponents.ContainsKey(oppOwnerId.Value))
                {
                    Line lGoal = new Line(GameParameters.OppGoalRight, GameParameters.OppGoalLeft);

                    bool flag1 = false, flag2 = false, flag3 = false;
                    foreach (var item in Model.Opponents)
                    {
                        Line l = new Line(Model.Opponents[item.Key].Location, ballState.Location);
                        Position2D? intersect = l.IntersectWithLine(lGoal);
                        if (intersect.HasValue && Math.Abs(intersect.Value.Y) < Math.Abs(GameParameters.OppGoalLeft.Y) + 0.2 &&
                            Model.Opponents[item.Key].Location.DistanceFrom(ballState.Location) < 0.25)
                        {

                            flag1 = true;
                            break;
                        }

                        else if (intersect.HasValue && Math.Abs(intersect.Value.Y) < Math.Abs(GameParameters.OppGoalLeft.Y) + 0.2 &&
                            Model.Opponents[item.Key].Location.DistanceFrom(ballState.Location) < 1)
                        {

                            flag2 = true;
                        }
                        else if (intersect.HasValue && Math.Abs(intersect.Value.Y) < Math.Abs(GameParameters.OppGoalLeft.Y) + 0.2 &&
                            Model.Opponents[item.Key].Location.DistanceFrom(ballState.Location) >= 1)
                        {

                            flag3 = true;
                        }
                    }

                    if (flag1)
                    {
                        Free = false;
                    }
                    else if (flag2)
                    {

                        Free = false;
                    }
                    else if (flag3)
                    {
                        Free = true;
                    }
                }
                Free = false;
            }
            else
            {
                Free = true;
            }
            return Free;

        }

        bool FreeToKick(GameStrategyEngine engine, WorldModel Model, int robotID)
        {
            Obstacles obs = new Obstacles(Model);
            List<int> ids = new List<int>();
            ids.Add(robotID);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { robotID }, null);
            var meet = obs.Meet(new SingleObjectState(ballState.Location, Vector2D.Zero, 0), new SingleObjectState(ballState.Location + Vector2D.FromAngleSize(Model.OurRobots[robotID].Angle.Value * Math.PI / 180, 4), Vector2D.Zero, 0), 0.04);
            return !meet;
        }

        public Position2D TargetToKick(WorldModel Model, int robotID)
        {
            return new Position2D(GameParameters.OppLeftCorner.X - .5, Math.Sign(Model.OurRobots[robotID].Location.Y) * Math.Abs(GameParameters.OppLeftCorner.Y));//GameParameters.OppGoalCenter;
        }

        private bool firstTime2 = true;
        private bool avoidance = true;
        private static Position2D eatballTarget = new Position2D();

        static Position2D SwimingFish(WorldModel Model, int wantedId, int pursueID, Position2D defenceTarget, double distBetween2Roles, out bool robotAvoidance)
        {

            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { wantedId, pursueID }, new List<int>());
            Position2D pursueRobot = Model.OurRobots[pursueID].Location;
            Position2D wantedrobot = Model.OurRobots[wantedId].Location;
            double deltaxCurrent = pursueRobot.DistanceFrom(wantedrobot);
            double targetdistcance = distBetween2Roles;
            double extendSize = -(targetdistcance - deltaxCurrent);
            Position2D targete = defenceTarget;
            Vector2D targetv = (wantedrobot - pursueRobot).GetNormalizeToCopy(((Math.Sign(extendSize) * deltaxCurrent) + (extendSize)));
            Position2D tg = wantedrobot + targetv;
            Obstacles obstmp = new Obstacles(Model);
            Obstacles obstmp2 = new Obstacles(Model);
            Obstacles obstmp3 = new Obstacles(Model);
            obstmp3.AddObstacle(1, 0, 0, 0, new List<int>() { pursueID }, new List<int>());// mohavate jarime va robotha bedoone robote taghib shavande
            obstmp2.AddObstacle(1, 0, 1, 0, new List<int>() { pursueID }, new List<int>());

            obstmp.AddObstacle(0, 0, 1, 0, new List<int>(), new List<int>());
            if (deltaxCurrent > Model.OurRobots[pursueID].Location.DistanceFrom(targete) && obstmp.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = true;
            }
            else if (deltaxCurrent > Model.OurRobots[pursueID].Location.DistanceFrom(targete) && obstmp2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = true;
            }
            else if (!obstmp2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = false;
            }
            else
            {
                if (!obs.Meet(Model.OurRobots[pursueID], Model.OurRobots[wantedId], .15))
                {

                    robotAvoidance = false;
                }
                if (!obs.Meet(Model.OurRobots[pursueID], Model.OurRobots[wantedId], .15))
                {

                    robotAvoidance = false;
                }
                else
                {
                    robotAvoidance = true;
                }
            }
            Obstacles obstacles = new Obstacles(Model);
            obstacles.AddObstacle(1, 0, 0, 0, new List<int>() { pursueID }, new List<int>());

            List<int> IDsExeptthanwanted = new List<int>();
            IDsExeptthanwanted = Model.OurRobots.Where(t => t.Key != wantedId).Select(y => y.Key).ToList();

            Obstacles obstacles2 = new Obstacles(Model);
            obstacles2.AddObstacle(1, 0, 0, 0, IDsExeptthanwanted, new List<int>());

            Obstacles obstacles3 = new Obstacles(Model);
            obstacles3.AddObstacle(1, 0, 0, 0, new List<int>() { pursueID, wantedId }, new List<int>());
            Position2D sttarget = Model.OurRobots[pursueID].Location + (Model.OurRobots[wantedId].Location - Model.OurRobots[pursueID].Location).GetNormalizeToCopy(Model.OurRobots[wantedId].Location.DistanceFrom(Model.OurRobots[pursueID].Location) - .2);

            if (obstacles.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12) && !obstacles2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12))
            {
                tg = targete;
                robotAvoidance = true;
            }
            if (obstacles3.Meet(Model.OurRobots[pursueID], Model.OurRobots[wantedId], .15) && obstacles.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .12) && sttarget.DistanceFrom(Model.OurRobots[pursueID].Location) < targete.DistanceFrom(Model.OurRobots[pursueID].Location) && targete.DistanceFrom(Model.OurRobots[pursueID].Location) > .15)
            {
                tg = Model.OurRobots[pursueID].Location + (Model.OurRobots[wantedId].Location - Model.OurRobots[pursueID].Location).GetNormalizeToCopy(Model.OurRobots[wantedId].Location.DistanceFrom(Model.OurRobots[pursueID].Location) - .2);
                robotAvoidance = true;
            }
            else
            {

            }
            return tg;

        }
        static Position2D SwimingFish(WorldModel Model, int pursueID, Position2D defenceTarget, out bool robotAvoidance)
        {
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { pursueID }, new List<int>());
            Position2D pursueRobot = Model.OurRobots[pursueID].Location;
            Position2D wantedrobot = defenceTarget;
            double deltaxCurrent = pursueRobot.DistanceFrom(wantedrobot);
            double extendSize = deltaxCurrent;
            Position2D targete = defenceTarget;
            Vector2D targetv = (wantedrobot - pursueRobot).GetNormalizeToCopy(((Math.Sign(extendSize) * deltaxCurrent) + (extendSize)));
            Position2D tg = wantedrobot + targetv;
            Obstacles obstmp = new Obstacles(Model);
            Obstacles obstmp2 = new Obstacles(Model);

            obstmp2.AddObstacle(1, 0, 1, 0, new List<int>() { pursueID }, new List<int>());

            obstmp.AddObstacle(0, 0, 1, 0, new List<int>(), new List<int>());
            if (deltaxCurrent > Model.OurRobots[pursueID].Location.DistanceFrom(targete) && obstmp.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = false;
            }
            else if (!obstmp2.Meet(Model.OurRobots[pursueID], new SingleObjectState(targete, Vector2D.Zero, 0f), .15))
            {
                tg = targete;
                robotAvoidance = false;
            }
            else
            {
                if (!obs.Meet(Model.OurRobots[pursueID], Model.BallState, .15))
                {
                    tg = targete;
                    robotAvoidance = false;
                }
                else
                {
                    robotAvoidance = true;
                }
            }
            return tg;

        }
#else

        public override RoleCategory QueryCategory()
        {
            throw new NotImplementedException();
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
#endif

    }
}
