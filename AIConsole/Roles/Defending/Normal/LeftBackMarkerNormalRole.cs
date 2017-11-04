using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.AIConsole.Skills.GoalieSkills;

using AtackerInfo = MRL.SSL.AIConsole.Engine.NormalSharedState.NewAttackerInfo;

namespace MRL.SSL.AIConsole.Roles
{
    class LeftBackMarkerNormalRole : RoleBase
    {
#if NEW
        static Position2D inthelastmoment = new Position2D();
        private static Position2D target = new Position2D();
        private Position2D InitialPos = new Position2D();
        private Position2D predictPos = new Position2D();
        static List<double> times = new List<double>();
        private bool intialPositioningFg = true;
        Position2D intersectG = new Position2D();
        private operation opr = operation.noth;
        Position2D markPos = new Position2D();
        HiPerfTimer time = new HiPerfTimer();
        Vector2D markSpeed = new Vector2D();
        private bool firstTimeDanger = true;
        private bool firstTimeAngle = false;
        private float staticAngleSize = 0;
        private double maxDistance = 5;
        private double MarkDistance = 0.18;
        private bool noIntersect = false;
        private bool staticAngle = true;
        private bool onlyoneTime = true;
        private bool dangerZone = true;
        private bool firstTime = true;
        private bool oceanTime = true;
        private bool oldCost = false;
        static int markID = 1000;
        static int markIDFake = 1000;
        bool isInZone = false;
        double angle = 0.00;
        double a = 3.50;
        Position2D LastCutpos = new Position2D();
        bool flagcut = true;
        bool disrupt = false;

        private static Position2D FarTarget = new Position2D();
        public SingleObjectState ballState = new SingleObjectState();
        public SingleObjectState ballStateFast = new SingleObjectState();
        int CurrentState = 0;

        public void Mark(GameStrategyEngine engine, WorldModel Model, int RobotID, int? MarkID)
        {
            #region Attacker
            if (CurrentState == (int)MarkStateInNear.Attacker)
            {
                if (AtackerInfo.PosToGo1.HasValue)
                {
                    double moveAngle = (Model.BallState.Location - AtackerInfo.PosToGo1.Value).AngleInDegrees;
                    Planner.Add(RobotID, AtackerInfo.PosToGo1.Value, moveAngle, PathType.UnSafe, true, true, true, true);
                }
                else
                {
                    if (AtackerInfo.PosToGo2.HasValue)
                    {
                        double yBall = Model.BallState.Location.Y;
                        Planner.Add(RobotID, new Position2D(Model.BallState.Location.X, -Math.Sign(yBall) * Math.Min(Math.Max(Math.Abs(yBall), 0.5), 2.5)), 0, PathType.UnSafe, true, true, true, true);
                    }
                    else
                    {
                        double xBall = Model.BallState.Location.X;
                        //Planner.Add(RobotID, new Position2D(Model.BallState.Location.X, -Math.Sign(yBall) * Math.Min(Math.Max(Math.Abs(yBall), 0.5), 2.5)), 0, PathType.UnSafe, true, true, true, true);
                        Planner.Add(RobotID, new Position2D(-Math.Sign(xBall) * Math.Min(Math.Max(Math.Abs(xBall), 0.5), 2), Model.BallState.Location.Y), 0, PathType.UnSafe, true, true, true, true);
                    }
                }
            }
            #endregion
            #region Others
            else
            {
                if (DefenceTest.BallTest)
                {
                    ballState = DefenceTest.currentBallState;
                    ballStateFast = DefenceTest.currentBallState;
                }
                else
                {
                    ballStateFast = Model.BallState;
                    ballStateFast = DefenceTest.currentBallState;
                }

                double moveAngle = 0.00;
                bool cutBall = true;
                Position2D target = CostFunction(engine, Model, RobotID, out moveAngle, out cutBall, true);
                //MOSTAFA
                Planner.ChangeDefaulteParams(RobotID, false);
                Planner.SetParameter(RobotID, 8, 8);

                //
                Planner.Add(RobotID, target, moveAngle, PathType.UnSafe, false, true, dangerZone, false);
                if (CurrentState == (int)MarkStateInNear.Disrupt)
                {
                    Planner.Add(RobotID, target, moveAngle, PathType.UnSafe, false, false, dangerZone, false);
                }
                DataBridge.LBMarkerRoleID = RobotID;
                DataBridge.CenterBacktarget = target;
                DrawingObjects.AddObject(new Circle(target, 0.10, new Pen(Brushes.Pink, 0.01f)));
            }
            #endregion
        }

        public Position2D CostFunction(GameStrategyEngine engine, WorldModel Model, int RobotID, out double retAngle, out bool cutBall, bool fartarget)
        {
            Position2D lastTarget = new Position2D();
            cutBall = false;
            if (FreekickDefence.OppToMark1.HasValue)
            {
                markID = FreekickDefence.OppToMark1.Value;
                markSpeed = Model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
                markPos = Model.Opponents[FreekickDefence.OppToMark1.Value].Location;

                angle = 0.00;
                Position2D markRobot = Model.Opponents[markID].Location;
            }
            Circle min = new Circle();
            retAngle = 0.00;
            Circle max = new Circle();
            Predict(Model, RobotID, markID, out min, out max, true);
            Position2D target = min.Center;
            if (fartarget)
            {
                FarTarget = target + (GameParameters.OurGoalCenter - target).GetNormalizeToCopy(0.018);
            }
            if (opr == operation.DecreaseAcc)
            {
                a -= 0.10;
                opr = operation.noth;
            }
            else if (opr == operation.increaseAcc)
            {
                a += 0.10;
                opr = operation.noth;
            }

            Position2D marknearmin = new Position2D();
            if (CurrentState != (int)MarkStateInNear.Stop)
            {
                firstTime = true;
                staticAngle = true;
            }
            if (markID != 1000)
            {

                #region Cut
                if (CurrentState == (int)MarkStateInNear.Cut)
                {
                    Circle centerOnOppRobot = new Circle(Model.Opponents[markID].Location, 0.35);
                    Line ballSpeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(15));
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 1, 1, new List<int> { RobotID }, new List<int> { markID });
                    bool meet = obs.Meet(Model.BallState, Model.Opponents[markID], 0.04);
                    List<Position2D> intersects = centerOnOppRobot.Intersect(ballSpeedLine);
                    Position2D cutpos = new Position2D();
                    if (intersects.Count > 0 && (intersects.FirstOrDefault()) != Position2D.Zero)
                    {
                        cutpos = intersects.OrderBy(u => u.DistanceFrom(GameParameters.OurGoalCenter)).FirstOrDefault();
                    }
                    if ((Model.BallState.Speed.Size > 1 && flagcut) || Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) > 0.35)
                    {
                        LastCutpos = cutpos;
                        flagcut = false;
                    }
                    if (LastCutpos != Position2D.Zero)
                    {
                        lastTarget = LastCutpos + (LastCutpos - GameParameters.OppRightCorner).GetNormalizeToCopy(0.09);
                    }
                    else
                        lastTarget = Model.Opponents[markID].Location;

                    //retAngle = -(180 - Math.Abs(Model.BallState.Speed.AngleInDegrees));
                    retAngle = (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees;
                    double kickPower = 150;
                    if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) < .3 && lastTarget != Position2D.Zero)
                    {
                        if (Model.BallState.Location.DistanceFrom(lastTarget) > Model.BallState.Location.DistanceFrom(Model.Opponents[markID].Location))
                        {
                            Vector2D v = (Model.Opponents[markID].Location - Model.BallState.Location).GetNormalizeToCopy(0.15);
                            lastTarget = Model.Opponents[markID].Location + v;
                            //DrawingObjects.AddObject(new Line(Model.Opponents[markID].Location,Model.Opponents[markID].Location+(Model.Opponents[markID].Location - Model.BallState.Location).GetNormalizeToCopy(0.15)),"dskjghfksuiah54f56x42vgdf5sz454ds");
                        }
                        GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, lastTarget);
                        Planner.AddKick(RobotID, kickPowerType.Power, kickPower, true, false);
                    }
                    DrawingObjects.AddObject(new Circle(lastTarget, 0.09, new Pen(Brushes.Red, 0.02f)), "dsfhjgdsuyaeftrigua5364");
                }
                #endregion
                #region Stop
                else if (CurrentState == (int)MarkStateInNear.Stop)
                {
                    firstTimeDanger = true;
                    if (fartarget) ;
                    //DrawingObjects.AddObject(new StringDraw("STOP", Model.OurRobots[RobotID].Location.Extend(0.50, 0)), Model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                    if (firstTime)
                    {
                        firstTime = false;
                        inthelastmoment = Model.OurRobots[RobotID].Location;
                    }
                    dangerZone = true;
                    target = inthelastmoment;
                    retAngle = angle;
                    lastTarget = inthelastmoment;
                }
                #endregion
                #region InTheWay
                else if (CurrentState == (int)MarkStateInNear.IntheWay)
                {
                    firstTimeDanger = true;
                    if (fartarget) ;
                    //DrawingObjects.AddObject(new StringDraw("IN THE WAY", Model.OurRobots[RobotID].Location.Extend(0.50, 0)), Model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                    noIntersect = false;
                    Line ourGoalCenterOppRobot = new Line(Model.Opponents[markID].Location, Model.Opponents[markID].Location + Model.Opponents[markID].Speed);
                    Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                    Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                    if (intersect.HasValue)
                    {
                        if (firstTimeAngle)
                        {
                            angle = Model.OurRobots[RobotID].Angle.Value;
                        }
                        double x, y;
                        if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                            intersect = GameParameters.OurGoalCenter + (Model.Opponents[markID].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(Model.Opponents[markID], 0.20));

                        intersectG = intersect.Value;
                        int counter = 0;
                        intersectG = intersect.Value;
                        bool ourconflict = true;
                        bool oppconflict = true;
                        while (!ourconflict && !oppconflict && counter < 15)
                        {
                            counter++;
                            intersectG = GameParameters.OurGoalCenter + (intersectG - GameParameters.OurGoalCenter).GetNormalizeToCopy((intersectG - GameParameters.OurGoalCenter).Size);//+ 0.05);
                            Position2D? ourNearRobot = Model.OurRobots.Where(u => u.Value.Location.DistanceFrom(intersectG) < 0.20).FirstOrDefault().Value.Location;
                            Position2D? oppNearRobot = Model.Opponents.Where(t => t.Value.Location.DistanceFrom(intersectG) < 0.20).FirstOrDefault().Value.Location;
                            if (ourNearRobot != null || !((intersectG - GameParameters.OurGoalCenter).Size < (Model.OurRobots[RobotID].Location - GameParameters.OurGoalCenter).Size - 0.20))
                            {
                                ourconflict = true;
                            }
                            else
                            {
                                ourconflict = false;
                            }
                            if (oppNearRobot != null || !((intersectG - GameParameters.OurGoalCenter).Size < (Model.OurRobots[RobotID].Location - GameParameters.OurGoalCenter).Size - 0.20))
                            {
                                oppconflict = true;
                            }
                            else
                            {
                                oppconflict = false;
                            }
                        }
                        dangerZone = true;
                        target = intersectG;
                        DrawingObjects.AddObject(new Circle(intersectG, GameDefinitions.RobotParameters.OurRobotParams.Diameter / 2), "57863");
                        double f = 1.2;
                        if (Model.OurRobots[RobotID].Location.DistanceFrom(intersectG) > 0.05)
                        {
                            Vector2D v = intersectG - Model.OurRobots[RobotID].Location;
                            intersectG = intersectG + Vector2D.FromAngleSize(v.AngleInRadians, v.Size * f);
                            target = intersectG;
                        }
                        else
                        {
                            target = intersectG;
                        }

                        //Planner.Add(RobotID, intersectG, angle, PathType.UnSafe, false, avoidness, true, true);
                    }
                    else
                    {
                        dangerZone = true;
                        noIntersect = true; // You Are Near Why go to Prependicular, Don't Fear go to Near Marking State 
                    }
                    retAngle = angle;
                    lastTarget = intersectG;
                    //return intersectG;
                }
                #endregion
                #region FarFront
                else if (CurrentState == (int)MarkStateInNear.FarFront)
                {
                    if (fartarget) ;
                    //DrawingObjects.AddObject(new StringDraw("FarFront", Model.OurRobots[RobotID].Location.Extend(0.50, 0)), "376376537798");
                    Line ourGoalCenterOppRobot = new Line(Model.Opponents[markID].Location, GameParameters.OurGoalCenter);
                    Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                    Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                    if (intersect.HasValue)
                    {
                        double dist1 = 0;
                        double dist2 = 0;
                        if (intersect.Value.DistanceFrom(GameParameters.OurGoalCenter) + 0.25 < Model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter) && intersect.Value.X < GameParameters.OurGoalCenter.X - .1 && GameParameters.IsInDangerousZone(intersect.Value, false, .2, out dist1, out dist2))// Hey man go to perpendicular point for opponent goes to Blindfold to Goal
                        {
                            noIntersect = false;

                            if (firstTimeAngle)
                            {
                                angle = Model.OurRobots[RobotID].Angle.Value;
                            }
                            double x, y;
                            if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                                intersect = GameParameters.OurGoalCenter + (Model.Opponents[markID].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(Model.Opponents[markID], 0.20));
                            intersectG = intersect.Value;

                            if ((markSpeed.GetNormnalizedCopy().InnerProduct((perpendicular.Tail - perpendicular.Head).GetNormnalizedCopy()) > 0.80 && markSpeed.Size > 1.00))
                            {
                                Line robotSpeed = new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Model.BallState.Speed);
                                Position2D? speedIntersect = robotSpeed.IntersectWithLine(perpendicular);
                                if (intersect.HasValue)
                                {
                                    intersectG = speedIntersect.Value;
                                }
                                double x1 = 0;
                                double y1 = 0;
                                if (GameParameters.IsInDangerousZone(intersectG, false, FreekickDefence.AdditionalSafeRadi, out x1, out y1))
                                {
                                    PointOutOfdangerZone(Model, RobotID, markID, intersectG, out intersectG);
                                }
                            }
                            Line speed = new Line(Model.OurRobots[RobotID].Location, Model.OurRobots[RobotID].Location + Model.OurRobots[RobotID].Speed.GetNormalizeToCopy(10.00));
                            Line oppRobot = new Line(Model.Opponents[markID].Location, GameParameters.OurGoalCenter);
                            Position2D? Intersect = speed.IntersectWithLine(oppRobot);
                            Position2D near = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(MarkDistance);

                        }
                        else
                        {
                            dangerZone = true;
                            noIntersect = true; // You Are Near Why go to Prependicular, Don't Fear go to Near fucking Marking State 
                            intersectG = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(MarkDistance);
                        }
                    }
                    else
                    {
                        intersectG = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(MarkDistance);
                        noIntersect = true; // Are you Crazy??? two line don't have any intersect ???!! special state :)
                    }
                    retAngle = angle;
                    lastTarget = intersectG;
                    if (lastTarget == new Position2D())
                    {
                        lastTarget = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(MarkDistance);
                    }
                    //return target;
                }
                #endregion
                #region goBack
                else if (CurrentState == (int)MarkStateInNear.goback)
                {
                    if (fartarget) ;
                    //DrawingObjects.AddObject(new StringDraw("goback", Model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), Model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                    Line ourGoalCenterOppRobot = new Line(Model.Opponents[markID].Location, GameParameters.OurGoalCenter);
                    Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                    Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                    Position2D extendedtarget = new Position2D();
                    if (intersect.HasValue)
                    {
                        noIntersect = false;

                        if (firstTimeAngle)
                        {
                            angle = Model.OurRobots[RobotID].Angle.Value;
                        }
                        double x, y;
                        if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                            intersect = GameParameters.OurGoalCenter + (Model.Opponents[markID].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(Model.Opponents[markID], 0.20));
                        intersectG = intersect.Value;

                        if (firstTimeDanger)
                        {
                            firstTimeDanger = false;
                            dangerZone = PointOutOfdangerZone(Model, RobotID, markID, intersectG, out extendedtarget);
                        }
                        PointOutOfdangerZone(Model, RobotID, markID, intersectG, out extendedtarget);
                    }
                    else
                    {
                        noIntersect = true; // Are you Crazy??? two line don't have any intersect ???!! special state :)
                    }
                    target = extendedtarget;
                    retAngle = angle;
                    lastTarget = extendedtarget;
                }
                #endregion
                #region NearFront
                else if (CurrentState == (int)MarkStateInNear.NearFront) // Sir you can go to near of the Opponent Robot and mark it with pressure
                {
                    cunter = 0;
                    double markdistance = MarkDistance;
                    //Obstacles obs = new Obstacles(Model);
                    Position2D checkpoint = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(markdistance);
                    //foreach (int item in Model.Opponents.Keys)
                    //{
                    //    obs.AddRobot(Model.Opponents[item].Location, false, item);
                    //}
                    //if(obs.Meet(Model.OurRobots[RobotID] , new SingleObjectState(checkpoint , Vector2D.Zero , 0) ,   
                    dangerZone = true;
                    if (fartarget) ;
                    //DrawingObjects.AddObject(new StringDraw("NearFront", Model.OurRobots[RobotID].Location.Extend(.5, 0)), Model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                    firstTimeAngle = true;
                    if (Model.Opponents[markID].Speed.Size > 1.00) // Don't change your angle beacause you are boozy and slow when move and change angle together
                    {
                        predictPos = min.Center;
                        onlyoneTime = true;
                    }
                    if (Model.Opponents[markID].Speed.Size < 0.10 && onlyoneTime) // hallloooo your predict have very overshoooootttttttttttt
                    {
                        marknearmin = Model.Opponents[markID].Location;
                        onlyoneTime = false;
                        if (predictPos.DistanceFrom(Model.Opponents[markID].Location) > 0.30) // .3 is hi :) enougth for you
                        {
                            opr = operation.DecreaseAcc;
                        }
                    }
                    if (Model.Opponents[markID].Speed.Size > 1.00)// I think Im ..... Robot I Pursuit it and .... it in this state
                    {
                        firstTime = true;
                        if (staticAngle)
                        {
                            staticAngle = false;
                            staticAngleSize = Model.OurRobots[RobotID].Angle.Value;
                            angle = staticAngleSize;
                        }
                        double t = 0.00;
                        Position2D sPP = StopPossiblePoint(Model, markID, out t);
                        marknearmin = min.Center + (GameParameters.OurGoalCenter - min.Center).GetNormalizeToCopy(MarkDistance);
                    }
                    else // Slow move and .... Opponent Robot 
                    {
                        //staticAngle = true;
                        //angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                        //if (engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[markID].Location.X < Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.X + 0.09
                        //    && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.Y > 0 && Model.Opponents[markID].Location.Y > Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.Y - 0.05)
                        //{
                        //    if (FreekickDefence.OppToMarkRegion.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMarkRegion.Value) && Model.Opponents[FreekickDefence.OppToMarkRegion.Value].Location.Y < 0)
                        //    {
                        //        Vector2D OppMarkToOppRegionMarkVect = Model.Opponents[FreekickDefence.OppToMarkRegion.Value].Location - Model.Opponents[markID].Location;
                        //        Vector2D OppMarkToLeftGoalVect = GameParameters.OurGoalLeft - Model.Opponents[markID].Location;
                        //        Line bisector = Vector2D.Bisector(OppMarkToLeftGoalVect, OppMarkToOppRegionMarkVect, Model.Opponents[markID].Location);
                        //        Vector2D bisectorVect = bisector.Tail - bisector.Head;
                        //        marknearmin = Model.Opponents[markID].Location + bisectorVect.GetNormalizeToCopy(2);
                        //        DrawingObjects.AddObject(marknearmin, marknearmin.X.ToString() + "53646");
                        //    }
                        //    else
                        //    {
                        //        marknearmin = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(MarkDistance * 5);
                        //    }
                        //}
                        //else
                        //{
                        //    marknearmin = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(MarkDistance);
                        //}


                        staticAngle = true;
                        angle = (Model.BallState.Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                        marknearmin = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(MarkDistance);
                    }
                    target = marknearmin;
                    if (marknearmin.DistanceFrom(GameParameters.OurGoalCenter) > maxDistance)
                    {
                        marknearmin = GameParameters.OurGoalCenter + (marknearmin - GameParameters.OurGoalCenter).GetNormalizeToCopy(maxDistance);
                    }
                    retAngle = angle;
                    retAngle = angle;
                    lastTarget = marknearmin;
                    double x = 0;
                    double y = 0;
                    if (GameParameters.IsInDangerousZone(lastTarget, false, 0.1, out x, out y))
                    {
                        lastTarget = GameParameters.OurGoalCenter + (Model.Opponents[markID].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(Model.Opponents[markID], FreekickDefence.AdditionalSafeRadi));
                    }

                }
                #endregion
                #region Far Behind
                else if (CurrentState == (int)MarkStateInNear.FarBehind) // Stupid Idiot, opponent is in your behind, Skidoo! to mark it stupid
                {
                    retAngle = 0.00;
                    lastTarget = new Position2D();
                    DrawingObjects.AddObject(new StringDraw("FarBehind", Model.OurRobots[RobotID].Location.Extend(0.50, 0)), Model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                }
                #endregion
                #region Near Behind
                else if (CurrentState == (int)MarkStateInNear.NearBehind) // Rotate and look back for understanding you are crazy and usless , stupid , .... , ...... , .....
                {
                    retAngle = 0.00;
                    lastTarget = new Position2D();
                    DrawingObjects.AddObject(new StringDraw("NearBehind", Model.OurRobots[RobotID].Location.Extend(0.50, 0)), Model.OurRobots[RobotID].Location.Extend(.5, 0).X.ToString());
                }
                #endregion
                #region Correctness
                if (lastTarget.DistanceFrom(GameParameters.OurGoalCenter) > maxDistance)
                {
                    lastTarget = GameParameters.OurGoalCenter + (lastTarget - GameParameters.OurGoalCenter).GetNormalizeToCopy(maxDistance);
                }

                Line directPath = new Line(Model.OurRobots[RobotID].Location, lastTarget);
                Circle oppCenter = new Circle();
                if (FreekickDefence.OppToMark1.HasValue)
                {
                    oppCenter = new Circle(Model.Opponents[markID].Location, 0.20);
                }
                List<Position2D> Positions = oppCenter.Intersect(directPath);
                bool correctness = true;
                if (Positions.Count > 0)
                {
                    if (Positions.Count == 1)
                    {
                        if (Positions[0].DistanceFrom(Model.OurRobots[RobotID].Location) < Model.OurRobots[RobotID].Location.DistanceFrom(lastTarget))
                        {
                            if (lastTarget.DistanceFrom(Model.Opponents[markID].Location) < 0.185)
                                correctness = false;
                        }
                    }
                    else if (Positions.Count == 2)
                    {
                        if (Positions[0].DistanceFrom(Model.OurRobots[RobotID].Location) < Model.OurRobots[RobotID].Location.DistanceFrom(lastTarget) || Positions[1].DistanceFrom(Model.OurRobots[RobotID].Location) < Model.OurRobots[RobotID].Location.DistanceFrom(lastTarget))
                        {
                            if (target.DistanceFrom(Model.Opponents[markID].Location) < 0.185)
                                correctness = false;
                        }
                    }
                }

                if (correctness == false && Model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter) + .10 < Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter))
                {
                    lastTarget = Model.Opponents[markID].Location + (GameParameters.OurGoalCenter - Model.Opponents[markID].Location).GetNormalizeToCopy(0.26);
                }
                #endregion

                #region Disrupt

                //extend targetpos to far target
                if (CurrentState == (int)MarkStateInNear.Disrupt)
                {
                    Position2D extendedTargetPos = new Position2D();
                    extendedTargetPos = Model.OurRobots[RobotID].Location + (/*Model.BallState.Location*/lastTarget - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(Model.OurRobots[RobotID].Location.DistanceFrom(Model.Opponents[markID].Location) * 2);
                    DrawingObjects.AddObject(new Circle(extendedTargetPos, 0.02f, new Pen(Brushes.Red, 0.02f)), "943238");
                    lastTarget = extendedTargetPos;
                }
                #endregion

                #region MarkFromDangerZone
                if (CurrentState == (int)MarkStateInNear.MarkFromDangerZone)
                {
                    if (/*FreekickDefence.OppInRBArea && */FreekickDefence.OppInLBAreaFake && markIDFake != 1000 && Model.Opponents.ContainsKey(markIDFake))
                    {
                        //Vector2D vectFromMarkIdToGoalCenter = Model.Opponents[markIDFake].Location - GameParameters.OurGoalCenter;
                        //Vector2D vectFromMarkIdFakeToGoalCenter = Model.Opponents[markID].Location - GameParameters.OurGoalCenter;
                        //Line bisectorLine = Vector2D.Bisector(vectFromMarkIdFakeToGoalCenter, vectFromMarkIdToGoalCenter, Model.Opponents[markID].Location);
                        //Vector2D bisectorVect = bisectorLine.Tail - bisectorLine.Head;

                        //double d = Model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter);
                        //int id = markID;
                        //if (markID !=1000 && markIDFake!=1000)
                        //{
                        //    d = (Model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter) < Model.Opponents[markIDFake].Location.DistanceFrom(GameParameters.OurGoalCenter)) ? (Model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter)) : (Model.Opponents[markIDFake].Location.DistanceFrom(GameParameters.OurGoalCenter));
                        //    id = (Model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter) < Model.Opponents[markIDFake].Location.DistanceFrom(GameParameters.OurGoalCenter)) ? markID : markIDFake;
                        //}
                        //double dist = GameParameters.SafeRadi(Model.OurRobots[RobotID], 0.25);
                        //dist = Math.Max(d, dist);
                        //bisectorVect.NormalizeTo(dist);
                        ////bisectorVect.NormalizeTo(1.5);//static dist
                        //Position2D p = GameParameters.OurGoalCenter + bisectorVect;
                        //retAngle = (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees;
                        //lastTarget = p;
                        //DrawingObjects.AddObject(new Circle(p, 0.09, new Pen(Brushes.Red, 0.02f)), "ry564tryer");
                        Vector2D vectFromMarkIdToGoalCenter = GameParameters.OurGoalCenter - Model.Opponents[markIDFake].Location;
                        Vector2D vectFromMarkIdFakeToGoalCenter = GameParameters.OurGoalCenter - Model.Opponents[markID].Location;
                        Line bisectorLine = Vector2D.Bisector(vectFromMarkIdFakeToGoalCenter, vectFromMarkIdToGoalCenter, Model.Opponents[markID].Location);
                        Vector2D bisectorVect = bisectorLine.Head - bisectorLine.Tail;
                        Position2D p = GameParameters.OurGoalCenter + bisectorVect.GetNormalizeToCopy(1.5);
                        retAngle = (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees;
                        lastTarget = p;
                        DrawingObjects.AddObject(new Circle(p, 0.09, new Pen(Brushes.Red, 0.02f)), "t4s56et4e");
                    }
                    else if (markID != 1000)  //if (!FreekickDefence.OppInRBAreaFake)
                    {
                        retAngle = (Model.Opponents[markID].Location - Model.OurRobots[RobotID].Location).AngleInDegrees;
                        Vector2D vectFromMarkIdToGoalCenter = GameParameters.OurGoalCenter - Model.Opponents[markIDFake].Location;
                        double dist = 1.5;
                        vectFromMarkIdToGoalCenter.NormalizeTo(dist); //static dist
                        lastTarget = GameParameters.OurGoalCenter + vectFromMarkIdToGoalCenter;
                    }

                }
                #endregion

                #region NewPositioninig
                if (CurrentState == (int)MarkStateInNear.NewPositioning)
                {
                    retAngle = (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees;
                    //lastTarget = new Position2D(2.1, -0.85);//GameParameters.OurGoalCenter + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(markPos, markSpeed, 0.0f), 0.20));
                    //if (FreekickDefence.StaticCBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticCBID.Value) && Model.BallState.Location.X < 1)
                    //{
                    //    Vector2D ballTocenterDef = GameParameters.OurGoalCenter - Model.BallState.Location;
                    //    lastTarget = Model.BallState.Location.Extend(0, 0.10) + ballTocenterDef.GetNormalizeToCopy(1);

                    //}
                    //else if (engine.GameInfo.OppTeam.BallOwner.HasValue)
                    //{
                    //    Vector2D v = Model.BallState.Location - GameParameters.OurGoalCenter;
                    //    lastTarget = GameParameters.OurGoalCenter + v.GetNormalizeToCopy(2.5);

                    //}
                    //else
                    //{
                    //    lastTarget = new Position2D(Model.BallState.Location.X, 2);
                    //}
                    ////if (Model.Opponents.Count > 0)
                    ////{
                    ////    var list = engine.GameInfo.OppTeam.Scores.OrderByDescending(v => v.Value);
                    ////    int oppid = list.First().Key;
                    ////    Vector2D r = GameParameters.OurGoalLeft/*.Extend(-0.5, 0)*/ - ballState.Location;//Model.Opponents[oppid].Location;

                    ////    lastTarget = ballState.Location + r.GetNormalizeToCopy(1.5);
                    ////}
                    lastTarget = new Position2D(2, 1.5);
                }
                #endregion
            }
            else
            {
                #region NewPositioninig
                DrawingObjects.AddObject(new StringDraw("markID is Null", new Position2D(-0.2, -3.5)), "53465mbkhjg24kj");
                retAngle = (GameParameters.OppGoalCenter - Model.OurRobots[RobotID].Location).AngleInDegrees;
                lastTarget = new Position2D(2, 1.5);
                #endregion
            }
            if (!(CurrentState == (int)MarkStateInNear.Cut) || lastTarget == Position2D.Zero)
            {
                lastTarget = CommonDefenceUtils.CheckForStopZoneMarker(FreekickDefence.BallIsMoved, lastTarget, Model, Model.OurRobots[RobotID]);
            }

            return lastTarget;
        }

        int cunter = 0;
        bool first = true, calculateCost = false;
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            #region attacker
            if (AtackerInfo.IsAttacker1)
            {
                CurrentState = (int)MarkStateInNear.Attacker;
            }
            else
            #endregion
            #region defence
            {
                if (FreekickDefence.leftFirstSet)
                {
                    if (FreekickDefence.OppToMark1.HasValue)
                    {
                        FreekickDefence.Area a = new FreekickDefence.Area();
                        FreekickDefence.OppToMark1 = a.OpponnentToMarkeRobots(engine, Model, true, false);
                        if (FreekickDefence.OppToMark1.HasValue)
                        {
                            markID = FreekickDefence.OppToMark1.Value;
                            markSpeed = Model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
                            markPos = Model.Opponents[FreekickDefence.OppToMark1.Value].Location;
                        }
                    }
                    else
                    {
                        FreekickDefence.Area a = new FreekickDefence.Area();
                        FreekickDefence.OppToMark1 = a.OpponnentToMarkeRobots(engine, Model, true, false);
                    }
                    if (FreekickDefence.OppToMark1.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
                    {
                        markID = FreekickDefence.OppToMark1.Value;
                    }
                    if (FreekickDefence.OppToMark1Fake.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1Fake.Value))
                    {
                        markIDFake = FreekickDefence.OppToMark1Fake.Value;
                    }
                }
                if (FreekickDefence.OppToMark1.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
                {
                    FreekickDefence.leftFirstSet = false;
                }
                else
                {
                    FreekickDefence.leftFirstSet = true;
                    FreekickDefence.OppToMark1 = null;
                    FreekickDefence.OppToMark1Fake = null;
                    markID = 1000;
                    markIDFake = 1000;
                }
                List<int> oppRobots = new List<int>();
                Line oppLineX = new Line();
                Position2D? intersectP = new Position2D();
                List<int> temp = new List<int>();
                foreach (var item in Model.Opponents.Keys)
                {
                    if (Model.Opponents[item].Location.X > -0.2)
                    {
                        oppLineX = new Line(new Position2D(Model.Opponents[item].Location.X, GameParameters.OurLeftCorner.Y), new Position2D(Model.Opponents[item].Location.X, GameParameters.OurGoalRight.Y));
                        intersectP = oppLineX.IntersectWithLine(FreekickDefence.LBLine);
                        if (intersectP.HasValue)
                        {
                            if (Model.Opponents[item].Location.Y > intersectP.Value.Y && !(engine.GameInfo.OppTeam.BallOwner.HasValue && engine.GameInfo.OppTeam.BallOwner.Value == item)
                                && ((FreekickDefence.OppToMark2.HasValue && item != FreekickDefence.OppToMark2.Value) || !FreekickDefence.OppToMark2.HasValue))
                            {
                                temp.Add(item);
                            }
                        }
                    }
                }
                FreekickDefence.oppCountLeft = temp.Count;
                if (FreekickDefence.oppCountLeft > 1)
                {
                    FreekickDefence.leftFirstSet = true;
                }

                if (markID != 1000)
                {
                    if (Model.Opponents.ContainsKey(markID) && Model.Opponents[markID].Location.X > -0.2)
                    {
                        oppLineX = new Line(new Position2D(Model.Opponents[markID].Location.X, GameParameters.OurRightCorner.Y), new Position2D(Model.Opponents[markID].Location.X, GameParameters.OurGoalLeft.Y));
                        intersectP = oppLineX.IntersectWithLine(FreekickDefence.LBLine);
                        if (intersectP.HasValue)
                        {
                            if (Model.Opponents[markID].Location.Y > intersectP.Value.Y)
                                FreekickDefence.OppInLBArea = true;
                            else
                                FreekickDefence.OppInLBArea = false;
                        }
                    }
                    else
                    {
                        FreekickDefence.OppInLBArea = false;
                    }
                }

                if (markIDFake != 1000)
                {
                    if (Model.Opponents.ContainsKey(markIDFake) && Model.Opponents[markIDFake].Location.X > -0.2)
                    {
                        oppLineX = new Line(new Position2D(Model.Opponents[markIDFake].Location.X, GameParameters.OurRightCorner.Y), new Position2D(Model.Opponents[markIDFake].Location.X, GameParameters.OurGoalLeft.Y));
                        intersectP = oppLineX.IntersectWithLine(FreekickDefence.LBLine);
                        if (intersectP.HasValue)
                        {
                            if (Model.Opponents[markIDFake].Location.Y > intersectP.Value.Y && !(engine.GameInfo.OppTeam.BallOwner.HasValue && engine.GameInfo.OppTeam.BallOwner.Value == markIDFake)
                                && ((FreekickDefence.OppToMark2.HasValue && markIDFake != FreekickDefence.OppToMark2.Value) || !FreekickDefence.OppToMark2.HasValue))
                                FreekickDefence.OppInLBAreaFake = true;
                            else
                                FreekickDefence.OppInLBAreaFake = false;
                        }
                    }
                    else
                    {
                        FreekickDefence.OppInLBAreaFake = false;
                    }
                }
                else
                {
                    FreekickDefence.OppInLBAreaFake = false;
                }
                bool OppInMyDefenceArea = false;

                oppRobots = new List<int>();
                foreach (var item in Model.Opponents.Keys)
                {
                    if (Model.Opponents.ContainsKey(item) && Model.Opponents[item].Location.X > -0.2 && (FreekickDefence.OppToMark2.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark2.Value) && item != FreekickDefence.OppToMark2.Value))
                    {
                        oppLineX = new Line(new Position2D(Model.Opponents[item].Location.X, GameParameters.OurRightCorner.Y), new Position2D(Model.Opponents[item].Location.X, GameParameters.OurGoalLeft.Y));
                        intersectP = oppLineX.IntersectWithLine(FreekickDefence.LBLine);
                        if (intersectP.HasValue)
                        {
                            if (Model.Opponents[item].Location.Y > intersectP.Value.Y)
                                oppRobots.Add(item);
                        }
                    }
                }
                if (oppRobots.Count > 0)
                {
                    if (CurrentState != (int)MarkStateInNear.NearFront && CurrentState != (int)MarkStateInNear.NearBehind && CurrentState != (int)MarkStateInNear.Cut)
                    {
                        FreekickDefence.leftFirstSet = true;
                    }
                    OppInMyDefenceArea = true;
                }
                FreekickDefence.OppInLBAreaReal = FreekickDefence.OppInLBArea;
                if (!FreekickDefence.OppInLBArea /*&& !FreekickDefence.OppInRBAreaReal*/ && markID != 1000 && Model.Opponents.ContainsKey(markID) && Model.Opponents[markID].Location.X > 0 && !OppInMyDefenceArea)
                {
                    FreekickDefence.OppInLBArea = true;
                    FreekickDefence.leftFirstSet = true;
                }

                if (FreekickDefence.OppToMark1.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
                {
                    markSpeed = Model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
                    markPos = Model.Opponents[FreekickDefence.OppToMark1.Value].Location;

                    Position2D p = GameParameters.OurGoalCenter + (markPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(markPos, markSpeed, 0.0f), 0.20));
                    DrawingObjects.AddObject(new Line(p, GameParameters.OurGoalCenter, new Pen(Brushes.Red, 0.01f)));

                    #region defence
                    if ((FreekickDefence.OppInLBArea && markID != 1000 && !FreekickDefence.OppInLBAreaFake) || markID == markIDFake)
                    {
                        #region 1
                        markID = FreekickDefence.OppToMark1.Value;
                        markSpeed = Model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
                        markPos = Model.Opponents[FreekickDefence.OppToMark1.Value].Location;
                        Line ourGoalCenterOppRobot = new Line(markPos, markPos + markSpeed);
                        Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(markPos);
                        Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                        if (intersect.HasValue)
                        {
                            if (firstTimeAngle)
                            {
                                angle = Model.OurRobots[RobotID].Angle.Value;
                            }
                            double x, y;

                            if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                                intersect = GameParameters.OurGoalCenter + (markPos - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(markPos, markSpeed, 0.0f), 0.20));
                            intersectG = intersect.Value;
                            int counter = 0;
                            bool ourconflict = true;
                            bool oppconflict = true;
                            while (!ourconflict && !oppconflict && counter < 7)
                            {
                                counter++;
                                intersectG = GameParameters.OurGoalCenter + (intersectG - GameParameters.OurGoalCenter).GetNormalizeToCopy((intersectG - GameParameters.OurGoalCenter).Size + 0.10);
                                Position2D? ourNearRobot = Model.OurRobots.Where(u => u.Value.Location.DistanceFrom(intersectG) < 0.20).FirstOrDefault().Value.Location;
                                Position2D? oppNearRobot = Model.Opponents.Where(t => t.Value.Location.DistanceFrom(intersectG) < 0.20).FirstOrDefault().Value.Location;
                                if (ourNearRobot != null)
                                {
                                    ourconflict = true;
                                }
                                else
                                {
                                    ourconflict = false;
                                }
                                if (oppNearRobot != null)
                                {
                                    oppconflict = true;
                                }
                                else
                                {
                                    oppconflict = false;
                                }
                            }
                        }

                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 1, 1, new List<int>() { RobotID }, new List<int> { markID });
                        Line ourGoalCenterOppRobot1 = new Line(Model.Opponents[markID].Location, Model.Opponents[markID].Location + Model.Opponents[markID].Speed);
                        Line perpendicular1 = ourGoalCenterOppRobot1.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                        Position2D? intersect1 = ourGoalCenterOppRobot1.IntersectWithLine(perpendicular1);
                        bool cancelInTheWay = false;
                        if (intersect1.HasValue)
                        {
                            if (obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(intersect1.Value, Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi))
                            {
                                cancelInTheWay = true;
                            }
                        }

                        Circle CenterOnOppRobot = new Circle(Model.Opponents[markID].Location, .3);
                        Line ballspeedLine = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(15));
                        Obstacles OObs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 1, 1, new List<int> { RobotID }, new List<int> { markID });
                        bool meet = obs.Meet(Model.BallState, Model.Opponents[markID], .04);
                        List<Position2D> inters = CenterOnOppRobot.Intersect(ballspeedLine);


                        bool dd = false;
                        if (markID != 1000)
                            dd = PointOutOfdangerZone(Model, RobotID, markID, intersectG, out intersectG);
                        if (markPos != Position2D.Zero)
                        {
                            if (!disrupt)
                            {
                                Line ballspeed = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed.GetNormalizeToCopy(3));
                                double dist2 = ballspeed.Distance(Model.OurRobots[RobotID].Location);
                                if (CurrentState == (int)MarkStateInNear.Cut)
                                {
                                    if ((Model.BallState.Speed.Size < .7 && Model.BallState.Speed.GetNormnalizedCopy().InnerProduct((Model.OurRobots[RobotID].Location - Model.BallState.Location).GetNormnalizedCopy()) < 0) || Model.BallState.Speed.Size < 0.50)
                                    {
                                        CurrentState = (int)MarkStateInNear.NearFront;
                                    }
                                }
                                else
                                {
                                    //new parameters for cut
                                    if (markID != 1000 && CurrentState == (int)MarkStateInNear.NearFront && !meet && inters.Count > 0 && dist2 < 0.30 && !obs.Meet(Model.BallState, Model.Opponents[markID], MotionPlannerParameters.BallRadi) && Model.BallState.Speed.GetNormnalizedCopy().InnerProduct((Model.Opponents[markID].Location - Model.BallState.Location).GetNormnalizedCopy()) > FreekickDefence.InnerProductValue)//&& Model.BallState.Speed.Size < 2)
                                    {
                                        CurrentState = (int)MarkStateInNear.Cut;
                                    }
                                    else if (((markSpeed.GetNormnalizedCopy().InnerProduct((GameParameters.OurGoalCenter - markPos).GetNormnalizedCopy()) > 0.70 && markSpeed.Size > 0.30)
                                        && (markPos.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) > 0.090
                                        && Math.Abs(Vector2D.AngleBetweenInDegrees(GameParameters.OurGoalCenter - markPos, Model.OurRobots[RobotID].Location - markPos)) < 20
                                        && markPos.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.30)) && CurrentState != (int)MarkStateInNear.IntheWay) // Opponent is very bull*** it goes to my face hehe i stand here forever
                                    {
                                        CurrentState = (int)MarkStateInNear.Stop;
                                    }
                                    else if ((markSpeed.GetNormnalizedCopy().InnerProduct((new Vector2D(1.00, 0.00)).GetNormnalizedCopy()) > 0.60 && markSpeed.Size > 1.00) && new Line(markPos + markSpeed, markPos).Distance(Model.OurRobots[RobotID].Location) < 0.50 &&
                                        (((new Line(markPos + markSpeed, markPos).PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location)).IntersectWithLine(new Line(markPos + markSpeed, markPos)).Value) - markPos).InnerProduct((markPos + markSpeed) - markPos) > 0 &&
                                        !cancelInTheWay) //&& (intersectG - GameParameters.OurGoalCenter).Size < (markPos - GameParameters.OurGoalCenter).Size - .2)
                                    {
                                        CurrentState = (int)MarkStateInNear.IntheWay;
                                    }
                                    //else if (!dangerZone && (CurrentState == (int)MarkState.goback || CurrentState == (int)MarkState.FarFront) && markspeed.Size > 0.50)
                                    //{
                                    //    CurrentState = (int)MarkState.NearFront;
                                    //}
                                    else if (!((FarTarget - GameParameters.OurGoalCenter).Size < (markPos - GameParameters.OurGoalCenter).Size) && CurrentState == (int)MarkStateInNear.IntheWay && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) > 0.6)
                                    {
                                        CurrentState = (int)MarkStateInNear.NearFront;
                                    }
                                    else if (CurrentState == (int)MarkStateInNear.goback && markSpeed.Size < 0.10)
                                    {
                                        if (cunter > 15)
                                        {
                                            CurrentState = (int)MarkStateInNear.NearFront;
                                        }
                                        else
                                            cunter++;
                                    }
                                    else if (Math.Abs(Vector2D.AngleBetweenInDegrees(markSpeed, GameParameters.OurGoalCenter - markPos)) > 100 && markSpeed.Size > 0.50 && Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) + .1 < Model.Opponents[markID].Location.DistanceFrom(GameParameters.OurGoalCenter))
                                    {
                                        CurrentState = (int)MarkStateInNear.goback;
                                    }
                                    else if (CurrentState == (int)MarkStateInNear.FarFront && new Line(FarTarget, GameParameters.OurGoalCenter).Distance(Model.OurRobots[RobotID].Location) < 0.35 && FarTarget != Position2D.Zero && Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) > 0.6)
                                    {
                                        CurrentState = (int)MarkStateInNear.NearFront;
                                    }
                                    else if (new Line(FarTarget, GameParameters.OurGoalCenter).Distance(Model.OurRobots[RobotID].Location) > 0.5 && Model.OurRobots[RobotID].Location.DistanceFrom(markPos) > .5)
                                    {
                                        CurrentState = (int)MarkStateInNear.FarFront;
                                    }
                                    else if (noIntersect)
                                    {
                                        CurrentState = (int)MarkStateInNear.NearFront;
                                    }
                                    //else if (markPos.DistanceFrom(GameParameters.OurGoalCenter) > Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) || ((GameParameters.OurGoalCenter - markPos).InnerProduct(Model.OurRobots[RobotID].Location - markPos) > 0))
                                    //{
                                    //    double dist = markPos.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter);
                                    //    if (dist < 0.25)
                                    //    {
                                    //        CurrentState = (int)MarkStateInNear.NearFront;
                                    //    }
                                    //    else if (CurrentState != (int)MarkStateInNear.NearFront)
                                    //    {
                                    //        CurrentState = (int)MarkStateInNear.FarFront;
                                    //    }
                                    //}
                                    else
                                    {
                                        double dist = Math.Abs(markPos.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter));
                                        if (dist < 0.25)
                                        {
                                            CurrentState = (int)MarkStateInNear.NearFront;
                                        }
                                        else
                                        {
                                            CurrentState = (int)MarkStateInNear.NearFront;
                                        }
                                    }

                                    double ballDistFromOpp = Model.BallState.Location.DistanceFrom(Model.Opponents[markID].Location);
                                    obs.AddObstacle(1, 0, 1, 1, new List<int> { RobotID }, new List<int> { markID });
                                    meet = obs.Meet(Model.BallState, Model.Opponents[markID], .04);

                                    if (Model.Opponents[markID].Angle.HasValue)
                                    {
                                        bool MTime1 = false;
                                        double t1 = predicttime(Model, RobotID, Model.OurRobots[RobotID].Location, target, target);
                                        double t2 = predicttime(Model, RobotID, Model.OurRobots[RobotID].Location, target, false);
                                        if (t1 == t2)
                                        {
                                            MTime1 = false;
                                        }
                                        else if (t1 < t2)
                                        {
                                            MTime1 = true;
                                        }
                                        else if (t1 > t2)
                                        {
                                            MTime1 = false;
                                        }
                                        bool DTime2 = false;
                                        if (Model.OurRobots[RobotID].Location.DistanceFrom(Model.BallState.Location) / 2 > Model.Opponents[markID].Location.DistanceFrom(Model.BallState.Location))
                                        {
                                            DTime2 = true;
                                        }
                                        else
                                            DTime2 = false;
                                        Line goalLine = new Line(GameParameters.OurGoalLeft, GameParameters.OurGoalRight);
                                        Line markIDLine = new Line(Model.Opponents[markID].Location, (Model.Opponents[markID].Location + Vector2D.FromAngleSize(Model.Opponents[markID].Angle.Value, 10)));
                                        Line ballSpeepLine = new Line(Model.BallState.Location, (Model.BallState.Location + Model.BallState.Speed.GetNormnalizedCopy()));

                                        if (markIDLine.IntersectWithLine(goalLine).HasValue)
                                        {
                                            if ((CurrentState == (int)MarkStateInNear.FarBehind || CurrentState == (int)MarkStateInNear.FarFront) && FreekickDefence.OppInLBArea && ballDistFromOpp < 0.25 && MTime1 && DTime2)
                                            {
                                                CurrentState = (int)MarkStateInNear.Disrupt;
                                            }
                                        }
                                    }
                                }

                            }
                        }
                        else if (disrupt)
                        {
                            if (Model.Opponents[markID].Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.20)
                            {
                                disrupt = false;
                            }
                        }
                        #endregion
                    }
                    else if (FreekickDefence.OppInLBArea && markID != 1000 && FreekickDefence.OppInLBAreaFake && markIDFake != 1000)
                    {
                        #region 2
                        CurrentState = (int)MarkStateInNear.MarkFromDangerZone;
                        #endregion
                    }
                    else
                    {
                        //if (markID != 1000 && Model.Opponents[markID].Location.X < 0 && Model.Opponents[markID].Location.X > -0.2)
                        //{
                        //    CurrentState = (int)MarkStateInNear.MarkFromDangerZone;
                        //}
                        //else //if ((int)targetInArea.ChoosePositionMode(engine, Model, markID, true) == 1)
                        //{
                        //    CurrentState = (int)MarkStateInNear.NewPositioning;
                        //}
                        CurrentState = (int)MarkStateInNear.NewPositioning;
                    }

                    #endregion

                    if (markID != 1000 && CurrentState != (int)MarkStateInNear.Cut && Model.BallState.Location.DistanceFrom(Model.Opponents[markID].Location) < 0.6)
                    {
                        markID = 1000;
                        markIDFake = 1000;
                        FreekickDefence.OppToMark1 = null;
                        FreekickDefence.OppToMark1Fake = null;
                        FreekickDefence.leftFirstSet = true;
                    }
                }
                else
                {
                    CurrentState = (int)MarkStateInNear.NewPositioning;
                }

                if (CurrentState == (int)MarkStateInNear.NewPositioning)
                {
                    FreekickDefence.leftFirstSet = true;
                    if (CurrentState != (int)MarkStateInNear.NearFront && CurrentState != (int)MarkStateInNear.NearBehind && CurrentState != (int)MarkStateInNear.FarFront && CurrentState != (int)MarkStateInNear.FarBehind)
                    {
                        markID = 1000;
                        FreekickDefence.OppToMark1 = null;
                    }
                }
            }
            #endregion

            #region debug
            DrawingObjects.AddObject(new StringDraw(((MarkStateInNear)CurrentState).ToString(), new Position2D(-.1, 3.5)), "325146842657");
            DrawingObjects.AddObject(new StringDraw("OppInRBArea: " + (FreekickDefence.OppInLBArea == true ? "True" : "False"), new Position2D(0, 3.5)), "56546667365247");
            DrawingObjects.AddObject(new StringDraw("FirstSet: " + (FreekickDefence.leftFirstSet == true ? "True" : "False"), new Position2D(0.1, 3.5)), "517465749821687");
            DrawingObjects.AddObject(new StringDraw("markID: " + markID.ToString(), new Position2D(0.20, 3.5)), "985632458638");
            if (FreekickDefence.OppToMark1.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
            {
                DrawingObjects.AddObject(new StringDraw("oppToMark1 : " + FreekickDefence.OppToMark1.Value.ToString(), new Position2D(0.3, 3.5)), "972498sdfsads6f457999");
            }
            if (FreekickDefence.OppToMark1Fake.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1Fake.Value))
            {
                DrawingObjects.AddObject(new StringDraw("oppToMark1Fake : " + FreekickDefence.OppToMark1Fake.Value.ToString(), new Position2D(0.4, 3.5)), "9724983545dfsdsf7999");
            }
            #endregion
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            if (markID != 1000 && Model.Opponents.ContainsKey(markID)
                && (FreekickDefence.OppToMark1.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
                && (FreekickDefence.OppToMark2.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark2.Value)))
            {
                DrawingObjects.AddObject(new StringDraw("marker dist : " + (Model.Opponents[markID].Location.DistanceFrom(new Position2D(Model.Opponents[markID].Location.X, 0)) < 0.7).ToString(), GameParameters.OurLeftCorner.Extend(0.1, 0)), "ftgh56y4dftryh");
                DrawingObjects.AddObject(new StringDraw("InnerProduct : " + (Model.Opponents[FreekickDefence.OppToMark1.Value].Speed.GetNormnalizedCopy().InnerProduct((Model.Opponents[FreekickDefence.OppToMark2.Value].Speed).GetNormnalizedCopy()) > 0).ToString(), GameParameters.OurLeftCorner.Extend(0.2, 0)), "fxtgh564tsr5h4gdsf");
                if (Model.Opponents[markID].Location.DistanceFrom(new Position2D(Model.Opponents[markID].Location.X, 0)) < 0.7
                    && Model.Opponents[FreekickDefence.OppToMark1.Value].Speed.GetNormnalizedCopy().InnerProduct((Model.Opponents[FreekickDefence.OppToMark2.Value].Speed).GetNormnalizedCopy()) > 0)
                {
                    FreekickDefence.SwitchLBMarkerToRBMarker = true;
                }
                else
                    FreekickDefence.SwitchLBMarkerToRBMarker = false;

            }
            if (FreekickDefence.SwitchLBMarkerToRBMarker == false)
            {
                if ((FreekickDefence.StaticLBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticLBID.Value)) && (FreekickDefence.StaticRBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticRBID.Value)))
                {
                    if (Model.OurRobots[FreekickDefence.StaticLBID.Value].Location.Y < 0 && Model.OurRobots[FreekickDefence.StaticRBID.Value].Location.Y > 0)
                    {
                        FreekickDefence.SwitchLBMarkerToRBMarker = true;
                    }
                    else if (!(Model.OurRobots[FreekickDefence.StaticLBID.Value].Location.Y < 0) && !(Model.OurRobots[FreekickDefence.StaticRBID.Value].Location.Y < 0))
                    {
                        FreekickDefence.SwitchLBMarkerToRBMarker = false;
                    }
                }
            }

            if ((FreekickDefence.StaticLBID.HasValue && Model.OurRobots.ContainsKey(FreekickDefence.StaticLBID.Value)))
            {
                Line l = new Line(GameParameters.OurGoalCenter, Model.OurRobots[FreekickDefence.StaticLBID.Value].Location);
                Position2D? i = GameParameters.LineIntersectWithDangerZone(l, true).FirstOrDefault();
                if (i.HasValue && i.Value != Position2D.Zero)
                {
                    if (Model.OurRobots[FreekickDefence.StaticLBID.Value].Location.DistanceFrom(i.Value) < .25)
                    {
                        FreekickDefence.SwitchCBMarkerToLBMarker = true;
                    }
                    else
                    {
                        FreekickDefence.SwitchCBMarkerToLBMarker = false;
                    }
                }
                else
                {
                    FreekickDefence.SwitchCBMarkerToLBMarker = false;
                }
            }
            else
            {
                FreekickDefence.SwitchCBMarkerToLBMarker = false;
            }

            if (CurrentState == (int)MarkStateInNear.Attacker)
                FreekickDefence.SwitchLBMarkerToRBMarker = false;

            DrawingObjects.AddObject(new StringDraw("Switch LBMarker To RBMarker : " + (FreekickDefence.SwitchLBMarkerToRBMarker).ToString(), GameParameters.OurLeftCorner.Extend(0.3, 0)), "fd564f56f456ff56f");
            DrawingObjects.AddObject(new StringDraw("Switch CBMarker To RBMarker : " + (FreekickDefence.SwitchCBMarkerToRBMarker).ToString(), GameParameters.OurLeftCorner.Extend(0.4, 0)), "f5gh4sfg4ds56tgsddfg");

            if (Model.Opponents.Count > 1)
            {
                #region
                //    List<RoleBase> res = new List<RoleBase>() { new LeftBackMarkerNormalRole(),
                //                                                new CenterBackNormalRole()
                //                                            /*new DefenderMarkerRole(),
                //                                            new DefenderMarkerRole2(),
                //                                            new DefendGotoPointRole(),
                //                                            new DefenderCornerRole2(),
                //                                            new NewDefenderMrkerRole(),
                //                                            new NewDefenderMarkerRole2(),
                //                                            new DefenderCornerRole4()*/
                //                                             };

                //    if (FreekickDefence.SwitchToActiveLBMarker)
                //    {
                //        res.Add(new ActiveRole());
                //    }
                //    //if (FreekickDefence.SwitchDefender2Marker1)
                //    //{
                //    //    res.Add(new DefenderNormalRole1());
                //    //    res.Add(new DefenderCornerRole1());
                //    //}
                //    //if (FreekickDefence.SwitchDefender32Marker1)
                //    //{
                //    //    res.Add(new DefenderCornerRole3());
                //    //}

                //    //if (FreekickDefence.SwitchDefender42Marker1)
                //    //{
                //    //    res.Add(new DefenderCornerRole4());
                //    //}

                //    //if (FreekickDefence.LastSwitchDefender2Marker1)//New IO2014
                //    //{
                //    //    res.Add(new DefenderNormalRole1());
                //    //    res.Add(new DefenderCornerRole1());
                //    //}
                //    //if (FreekickDefence.LastSwitchDefender32Marker1)//New IO2014
                //    //{
                //    //    res.Add(new DefenderCornerRole3());
                //    //}
                //    //if (FreekickDefence.LastSwitchDefender42Marker1)//New IO2014
                //    //{
                //    //    res.Add(new DefenderCornerRole4());
                //    //}
                //    return res;
                //}
                //return new List<RoleBase>() { new LeftBackMarkerNormalRole() };
                ////else
                ////{
                ////    List<RoleBase> res = new List<RoleBase>() { };
                ////    //new DefenderMarkerRole(),
                ////    //                                        new DefenderMarkerRole2(),
                ////    //                                        new DefendGotoPointRole(),
                ////    //                                        new DefenderCornerRole2(),
                ////    //                                        new NewDefenderMrkerRole(),
                ////    //                                        new NewDefenderMarkerRole2(),
                ////    //                                        new NewDefenderMarkerRole3(),
                ////    //                                        new DefenderCornerRole4()
                ////    //                                         };

                ////    //if (FreekickDefence.freeSwitchbetweenRegionalAndMarker || FreekickDefence.RearRegional)
                ////    //{
                ////    //    res.Add(new RegionalDefenderRole());
                ////    //    res.Add(new RegionalDefenderRole2());
                ////    //}

                ////    //if (FreekickDefence.SwitchToActiveMarker1)
                ////    //{
                ////    //    res.Add(new ActiveRole());
                ////    //}
                ////    //if (FreekickDefence.SwitchDefender2Marker1)
                ////    //{
                ////    //    res.Add(new DefenderNormalRole1());
                ////    //    res.Add(new DefenderCornerRole1());
                ////    //}
                ////    //if (FreekickDefence.SwitchDefender32Marker1)
                ////    //{
                ////    //    res.Add(new DefenderCornerRole3());
                ////    //}

                ////    //if (FreekickDefence.SwitchDefender42Marker1)
                ////    //{
                ////    //    res.Add(new DefenderCornerRole4());
                ////    //}

                ////    //if (FreekickDefence.LastSwitchDefender2Marker1)//New IO2014
                ////    //{
                ////    //    res.Add(new DefenderNormalRole1());
                ////    //    res.Add(new DefenderCornerRole1());
                ////    //}
                ////    //if (FreekickDefence.LastSwitchDefender32Marker1)//New IO2014
                ////    //{
                ////    //    res.Add(new DefenderCornerRole3());
                ////    //}
                ////    //if (FreekickDefence.LastSwitchDefender42Marker1)//New IO2014
                ////    //{
                ////    //    res.Add(new DefenderCornerRole4());
                ////    //}
                ////    return res;
                ////}
                #endregion
                List<RoleBase> res = new List<RoleBase>() { new LeftBackMarkerNormalRole() };//, new CenterBackNormalRole() };
                if (FreekickDefence.SwitchLBMarkerToRBMarker)
                {
                    res.Add(new RightBackMarkerNormalRole());
                }
                if (FreekickDefence.SwitchCBMarkerToLBMarker)
                {
                    res.Add(new CenterBackNormalRole());
                }
                if (FreekickDefence.SwitchToActiveRBMarker)
                {
                    res.Add(new ActiveRole());
                }

                return res;
            }
            return new List<RoleBase>() { new LeftBackMarkerNormalRole() };
        }

        private bool PointOutOfdangerZone(WorldModel Model, int RobotID, int MarkID, Position2D targetreference, out Position2D targetvalue)
        {
            targetvalue = targetreference;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.ZoneCircle;
            obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW, MotionPlannerParameters.DangerZoneW);
            obs.State = new SingleObjectState(GameParameters.OurGoalCenter, new Vector2D(), null);
            bool meet = true;
            int counter = 0;
            while (meet && counter < 15 && targetreference.DistanceFrom(GameParameters.OurGoalCenter) + 0.20 < Model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter))
            {
                counter++;
                meet = obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(targetreference, Vector2D.Zero, 0.00f), RobotParameters.OurRobotParams.Diameter / 2.00);
                targetvalue = GameParameters.OurGoalCenter + (targetreference - GameParameters.OurGoalCenter).GetNormalizeToCopy((targetvalue - GameParameters.OurGoalCenter).Size + 0.10);
                if (counter > 14 || !(targetvalue.DistanceFrom(GameParameters.OurGoalCenter) + 0.20 < Model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter)))
                {
                    targetvalue = Model.Opponents[MarkID].Location + (GameParameters.OurGoalCenter - Model.Opponents[MarkID].Location).GetNormalizeToCopy(MarkDistance);
                    return false;

                }
                else if (!meet)
                {
                    return true;
                }
            }
            if (targetvalue.DistanceFrom(GameParameters.OurGoalCenter) - .2 > Model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter))
            {
                targetvalue = Model.Opponents[MarkID].Location + (GameParameters.OurGoalCenter - Model.Opponents[MarkID].Location).GetNormalizeToCopy(MarkDistance);
            }
            targetvalue = targetreference;
            return false;
        }

        public void Predict(WorldModel Model, int RobotID, int markedID, out Circle minimumCircle, out Circle maximumCircle, bool isStopPosblePoint)
        {
            minimumCircle = new Circle();
            maximumCircle = new Circle();
            double BallSpeed = Model.BallState.Speed.Size;
            double BallArriveTimeForRobot1Max = 0.40;
            double BallArriveTimeForRobot1Min = 0.40;
            Position2D FrontRobot1Max = new Position2D();
            Position2D FrontRobot1Min = new Position2D();
            Vector2D FirstRobotSpeedSize = new Vector2D();
            if (FreekickDefence.OppToMark1.HasValue)
            {
                FirstRobotSpeedSize = Model.Opponents[markedID].Speed;
            }
            List<Position2D> Robot1PosesMax = new List<Position2D>();
            List<Position2D> Robot1PosesMin = new List<Position2D>();
            if (!isStopPosblePoint)
            {
                for (int i = 0; i < 157; i++)
                {
                    double Convert = (double)i / 25.00;
                    double FrontRobot1XMax = 0.50 * a * Math.Cos(Convert) * (BallArriveTimeForRobot1Max * BallArriveTimeForRobot1Max) + (FirstRobotSpeedSize.Size * Math.Cos(FirstRobotSpeedSize.AngleInRadians) * BallArriveTimeForRobot1Max) + Model.Opponents[markedID].Location.X;
                    double FrontRobot1YMax = 0.50 * a * Math.Sin(Convert) * (BallArriveTimeForRobot1Max * BallArriveTimeForRobot1Max) + (FirstRobotSpeedSize.Size * Math.Sin(FirstRobotSpeedSize.AngleInRadians) * BallArriveTimeForRobot1Max) + Model.Opponents[markedID].Location.Y;
                    double FrontRobot1XMin = 0.50 * -a * Math.Cos(Convert) * (BallArriveTimeForRobot1Min * BallArriveTimeForRobot1Min) + (FirstRobotSpeedSize.Size * Math.Cos(FirstRobotSpeedSize.AngleInRadians) * BallArriveTimeForRobot1Min) + Model.Opponents[markedID].Location.X;
                    double FrontRobot1YMin = 0.50 * -a * Math.Sin(Convert) * (BallArriveTimeForRobot1Min * BallArriveTimeForRobot1Min) + (FirstRobotSpeedSize.Size * Math.Sin(FirstRobotSpeedSize.AngleInRadians) * BallArriveTimeForRobot1Min) + Model.Opponents[markedID].Location.Y;
                    FrontRobot1Max = new Position2D(FrontRobot1XMax, FrontRobot1YMax);
                    FrontRobot1Min = new Position2D(FrontRobot1XMin, FrontRobot1YMin);
                    Robot1PosesMax.Add(FrontRobot1Max);
                    Robot1PosesMin.Add(FrontRobot1Min);
                }

                if (Model.OurRobots.ContainsKey(RobotID))
                {
                    List<Position2D> YSortRobot1Max = Robot1PosesMax.OrderBy(g => g.Y).ToList();
                    List<Position2D> YSortRobot1Min = Robot1PosesMin.OrderBy(g => g.Y).ToList();
                    double SmallYRobot1Max = YSortRobot1Max.First().Y;
                    double BigYRobot1Max = YSortRobot1Max.Last().Y;

                    double SmallYRobot1Min = YSortRobot1Min.First().Y;
                    double BigYRobot1Min = YSortRobot1Min.Last().Y;

                    Position2D Robot1CenterCircleMax = new Position2D(YSortRobot1Max.First().X, (SmallYRobot1Max + BigYRobot1Max) / 2.00);
                    Position2D Robot1CenterCircleMin = new Position2D(YSortRobot1Min.First().X, (SmallYRobot1Min + BigYRobot1Min) / 2.00);
                    Circle Robot1CircleMax = new Circle();
                    Circle Robot1CircleMin = new Circle();
                    Robot1CircleMax = new Circle(Robot1CenterCircleMax, Math.Max(Robot1PosesMax.OrderBy(u => u.DistanceFrom(Robot1CenterCircleMax)).First().DistanceFrom(Robot1CenterCircleMax), 0.13) + 0.02);
                    Robot1CircleMin = new Circle(Robot1CenterCircleMin, Math.Max(Robot1PosesMin.OrderBy(u => u.DistanceFrom(Robot1CenterCircleMin)).First().DistanceFrom(Robot1CenterCircleMin), 0.13) + 0.02);

                    minimumCircle = Robot1CircleMin;
                    maximumCircle = Robot1CircleMax;

                }


            }
            double x = 0;
            if (FreekickDefence.OppToMark1.HasValue)
            {
                minimumCircle = new Circle(StopPossiblePoint(Model, markedID, out x), .3);
            }
        }

        public Position2D StopPossiblePoint(WorldModel model, int MarkID, out double t)
        {
            double v = 1;
            if (FreekickDefence.OppToMark1.HasValue)
            {
                v = model.Opponents[MarkID].Speed.Size;
            }
            double displacement = (v * v) / (2.00 * a);
            Position2D target = model.Opponents[MarkID].Location + model.Opponents[MarkID].Speed.GetNormalizeToCopy(displacement);

            t = displacement / (v / 2.00);
            if (target == new Position2D())
            {
                double s = 0;
            }
            return target;
        }

        private static Position2D OldMarkerPos(GameStrategyEngine engine, WorldModel Model, int oppid)
        {
            Position2D Target = Position2D.Zero;
            SingleObjectState state;
            state = Model.Opponents[oppid];

            Vector2D oppSpeedVector = state.Speed;
            Vector2D oppOurGoalCenter = GameParameters.OurGoalCenter - state.Location;
            double innerpOppOurGoal = oppSpeedVector.InnerProduct(oppOurGoalCenter);

            double oppSpeed = state.Speed.Size;
            double minDist = GameParameters.SafeRadi(state, MarkerDefenceUtils.MinDistMarkMargin);

            Position2D minimum = GameParameters.OurGoalCenter + (state.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(minDist);
            Position2D maximum = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(0.2);
            Position2D posToGo = Position2D.Zero;

            double MarkFromDist = MarkerDefenceUtils.MarkFromDist;

            posToGo = state.Location + (GameParameters.OurGoalCenter - state.Location).GetNormalizeToCopy(MarkFromDist);

            if (minimum.DistanceFrom(GameParameters.OurGoalCenter) > posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = minimum;
            else if (maximum.DistanceFrom(GameParameters.OurGoalCenter) < posToGo.DistanceFrom(GameParameters.OurGoalCenter))
                Target = maximum;
            else
                Target = posToGo;

            Position2D maxpos = GameParameters.OurGoalCenter + (Target - GameParameters.OurGoalCenter).GetNormalizeToCopy(3);
            if (GameParameters.OurGoalCenter.DistanceFrom(Target) > GameParameters.OurGoalCenter.DistanceFrom(maxpos))
                Target = maxpos;
            if (Target.X > GameParameters.OurGoalCenter.X)
                Target.X = GameParameters.OurGoalCenter.X;
            if (Math.Abs(Target.Y) > Math.Abs(GameParameters.OurLeftCorner.Y))
                Target = new Line(Target, GameParameters.OurGoalCenter).CalculateX(Math.Abs(GameParameters.OurLeftCorner.Y) * Math.Sign(Target.Y));
            target = Target;
            return target;
        }

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            double cost = RobotID;
            if (FreekickDefence.OppToMark1.HasValue && Model.Opponents.ContainsKey(FreekickDefence.OppToMark1.Value))
            {
                markID = FreekickDefence.OppToMark1.Value;
                markSpeed = Model.Opponents[FreekickDefence.OppToMark1.Value].Speed;
                markPos = Model.Opponents[FreekickDefence.OppToMark1.Value].Location;
                if (oldCost)
                {
                    double ang = 0;
                    bool avd = false;
                    if (CostFunction(engine, Model, RobotID, out ang, out avd, false) == new Position2D())
                    {
                        int breakk = 0;
                    }
                    cost = Model.OurRobots[RobotID].Location.DistanceFrom(CostFunction(engine, Model, RobotID, out ang, out avd, false));
                    cost = cost * cost;
                }
                else
                {
                    cost = Model.OurRobots[RobotID].Location.DistanceFrom(CalculateTarget(engine, Model, RobotID, OldMarkerPos(engine, Model, markID), markID));
                    cost = cost * cost;
                }

            }
            return cost;
        }

        private Position2D CalculateTarget(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, int oppid)
        {
            Position2D pos = new Position2D();
            SingleObjectState oppState = (Model.Opponents.ContainsKey(oppid)) ? Model.Opponents[oppid] : Model.BallState;
            Position2D postosee = new Position2D(GameParameters.OppGoalCenter.X, -1 * Math.Sign(Model.BallState.Location.Y) * 2.00);
            Circle OpprobotCenter = new Circle(Model.Opponents[oppid].Location, 0.22);
            Vector2D BallVectorSpeed = Model.BallState.Speed.GetNormalizeToCopy(10.00);
            Line BallMotionLine = new Line(Model.BallState.Location, Model.BallState.Location + BallVectorSpeed);
            List<Position2D> Intersections = OpprobotCenter.Intersect(BallMotionLine);
            Position2D first = Intersections.OrderBy(o => o.DistanceFrom(Model.BallState.Location)).FirstOrDefault();
            pos = first.Extend(0.05, 0);
            if (first == Position2D.Zero)
            {
                pos = oppState.Location + (Model.BallState.Location - oppState.Location).GetNormalizeToCopy(OppFreeKickMarkerUtils.DistCutPassFromOpp);
            }
            Position2D targetforkick = (Model.BallState.Location.Y > 0) ? GameParameters.OppLeftCorner : GameParameters.OppRightCorner;
            Position2D stoppos = Model.BallState.Location + Vector2D.FromAngleSize((GameParameters.OurGoalCenter - Model.BallState.Location).AngleInRadians, CommonDefenceUtils.StopZone);
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, null, Model.Opponents.Keys.ToList());
            if (pos.DistanceFrom(stoppos) < RobotParameters.OurRobotParams.Diameter + 0.02 && obs.Meet(new SingleObjectState(stoppos, Vector2D.Zero, 0), 0.10))
            {
                Vector2D vec = pos - stoppos;
                pos = stoppos + vec.GetNormalizeToCopy(RobotParameters.OurRobotParams.Diameter + 0.02);
            }

            double Mindist = GameParameters.SafeRadi(new SingleObjectState(pos, Vector2D.Zero, 0), OppFreeKickMarkerUtils.MinDistBehindFromZone);
            bool meet = false;
            double d = pos.DistanceFrom(GameParameters.OurGoalCenter);

            if (!isInZone && d < Mindist)
            {
                isInZone = true;
            }
            else if (isInZone && d > Mindist + 0.10)
            {
                isInZone = false;
            }
            if (isInZone)
            {
                Obstacles obstacles = new Obstacles(Model);

                List<int> exclude = new List<int> { RobotID, Model.GoalieID ?? 100 };
                obstacles.AddObstacle(1, 0, 0, 0, exclude, new List<int>() { oppid });

                meet = obstacles.Meet(Model.OurRobots[RobotID], new SingleObjectState(GameParameters.OurGoalCenter, Vector2D.Zero, null), 0.07);
                if (meet)
                {
                    //kpos = (pos - GameParameters.OurGoalCenter).GetNormalizeToCopy(.1) + pos;
                }
            }
            pos = CommonDefenceUtils.CheckForStopZoneMarker(FreekickDefence.BallIsMoved, pos, Model, Model.OurRobots[RobotID]);
            return pos;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        //
        private bool firstTime3 = true;
        public static Position2D lasttargetPoint = new Position2D();
        private int counter = 0;
        int Deccelcounter = 0;
        Position2D lasttarget = new Position2D();
        Position2D lastinitpos = new Position2D();
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


        enum operation
        {
            noth,
            increaseAcc,
            DecreaseAcc
        }

        public enum MarkStateInNear
        {
            FarFront,
            NearFront,
            NearBehind,
            FarBehind,
            goback,
            IntheWay,
            Stop,
            Cut,
            Disrupt,
            MarkFromDangerZone,
            NewPositioning,
            Attacker
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