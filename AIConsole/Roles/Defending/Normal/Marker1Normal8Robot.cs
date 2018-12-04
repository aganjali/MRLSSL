using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.AIConsole.Roles.Defending.Normal
{
    class
        Marker1Normal8Robot : RoleBase
    {
        bool ballIsMove = false, oppBallOwner = false;
        public static int? oppMarkID;
        double velocity = 0;
        double ballcoeff = 0;
        double robotCoeff = 0;
        double robotIntersectTime = 0;
        private double markDistance = 0.180;
        private bool firsttimedanger = true;
        private bool dangerzone = true;
        bool flagdraw = false;
        bool InRegional = false;
        private bool noIntersect = false;
        private bool firstTime = true;
        double GetNormalizeBehind = 0.15;
        Position2D intersect = new Position2D();
        Position2D firstballpos;
        Position2D intersectG = new Position2D();
        double treshTime = 0;
        Position2D? regional = null;
        public Position2D target = new Position2D();
        Position2D initialpos = new Position2D();
        double distToMark = 0.5 + RobotParameters.OurRobotParams.Diameter;
        bool cutFlag = false;
        bool testrole = false;
        double distNear = 0.18;
        public List<int> oppAttackerIds = new List<int>();
        List<Position2D> ballHistory = new List<Position2D>();
        List<int> opp = new List<int>();
        Queue<Position2D> lastTarget = new Queue<Position2D>();
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Defender;
        }
        public void Perform(GameStrategyEngine engin, WorldModel Model, int RobotID, double markRegion, int? _oppMarkID, List<int> oppAttackerIds, List<int> oppValue1, List<int> oppValue2, int? field)
        {
            if (_oppMarkID.HasValue)
                oppMarkID = _oppMarkID;
            //if (!_oppMarkID.HasValue || (_oppMarkID.HasValue && (!Model.Opponents.ContainsKey(_oppMarkID.Value) || (Model.Opponents.ContainsKey(_oppMarkID.Value) && Model.Opponents[_oppMarkID.Value].Location.X < markRegion))))
            //    oppMarkID = null;


            target = CalculateTarget(engin, Model, RobotID);

            if (CurrentState == (int)State.Attack)
            {
                #region Attack
                NormalSharedState.CommonInfo.AttackerID = RobotID;
                if (NormalSharedState.ActiveInfo.CurrentAction != NormalSharedState.ActiveActionMode.Pass)
                {
                    if (ballHistory.Count > 1 && ballHistory.First().DistanceFrom(ballHistory.Last()) > 0.5)
                        ballHistory.Clear();
                    if (ballHistory.Count == 0)
                    {
                        
                        List<PassPointData> poses = new List<PassPointData>();
                        double regionX = 0;
                        if (Model.BallState.Location.X > -StaticVariables.FIELD_LENGTH_H / 2)
                        {
                            regionX = Model.BallState.Location.X;
                        }
                        else if (Model.BallState.Location.X > -StaticVariables.FIELD_LENGTH_H)
                        {
                            regionX = -(StaticVariables.FIELD_LENGTH_H - 2 * (StaticVariables.FIELD_LENGTH_H) / 3);
                        }
                        Position2D topLeft = new Position2D(regionX, GameParameters.OurRightCorner.Y);
                        double width = GameParameters.OurGoalCenter.X - 0.5 - 0.25, heigth = 2 * GameParameters.OurLeftCorner.Y, passSpeed = 4, shootSpeed = Program.MaxKickSpeed;
                        int Rows = 5, column = 10;
                        poses = engin.GameInfo.CalculatePassScore(Model, NormalSharedState.CommonInfo.ActiveID.Value, RobotID, topLeft, passSpeed, shootSpeed, width, heigth, Rows, column);

                        double maxSc = double.MinValue;
                        foreach (var item in poses)
                        {
                            if (item.score > maxSc)
                            {
                                maxSc = item.score;
                                target = item.pos;
                            }
                        }
                        NormalSharedState.CommonInfo.PassTarget = target;
                    }
                }
                else
                {
                    target = NormalSharedState.CommonInfo.PassTarget;
                }

                #endregion

            }
            else
            {
                #region Defenc
                //reSet for attack State
                NormalSharedState.CommonInfo.AttackerID = RobotID;

                //if (oppValue1.Count == 0 && oppValue2.Count == 0 || oppValue2.Count == 1 || oppValue2.Count == 2))
                //{
                //    if (/*Model.BallState.Location.X > 0 && Model.BallState.Location.Y < 0*/field==2)
                //    {
                //        target = new Position2D(3, -2.25);
                //    }
                //    //OK
                //    if (/*Model.BallState.Location.X > 0 && Model.BallState.Location.Y > 0*/field==1)
                //    {
                //        target = new Position2D(3, 2.25);
                //    }
                //}
                if (oppValue1.Count == 0 && oppValue2.Count == 0)
                {
                    if (/*Model.BallState.Location.X > 0 && Model.BallState.Location.Y < 0*/field == 2)
                    {
                        target = new Position2D(3, -2.25);
                    }
                    //OK
                    if (/*Model.BallState.Location.X > 0 && Model.BallState.Location.Y > 0*/field == 1)
                    {
                        target = new Position2D(3, 2.25);
                    }
                }
                if (oppValue1.Count == 0 && oppValue2.Count == 1)
                {
                    if (/*Model.BallState.Location.X > 0 && Model.BallState.Location.Y < 0*/field == 2)
                    {
                        target = new Position2D(3, -2.25);
                    }
                    //OK
                    if (/*Model.BallState.Location.X > 0 && Model.BallState.Location.Y > 0*/field == 1)
                    {
                        target = new Position2D(3, 2.25);
                    }
                }
                if (oppValue1.Count == 0 && oppValue2.Count == 2)
                {
                    if (/*Model.BallState.Location.X > 0 && Model.BallState.Location.Y < 0*/field == 2)
                    {
                        target = new Position2D(3, -2.25);
                    }
                    //OK
                    if (/*Model.BallState.Location.X > 0 && Model.BallState.Location.Y > 0*/field == 1)
                    {
                        target = new Position2D(3, 2.25);
                    }
                }
                //moshkel dare (1shanbe)
                if (oppValue1.Count == 0 && oppValue2.Count >= 3)
                {
                    oppMarkID = oppValue2[1];
                }
                #endregion

            }
            NormalSharedState.CommonInfo.NormalAttackerMarker1Target = target;
            DrawingObjects.AddObject(new Circle(target, 0.09, new Pen(Color.DarkBlue, 0.01f)));
            var angle = (CurrentState == (int)State.Attack) ? (Model.BallState.Location - target).AngleInDegrees : (Model.OurRobots[RobotID].Angle.Value);
            Planner.Add(RobotID, target, angle, PathType.UnSafe, false, true, true, false);
            if (Model.OurRobots[RobotID].Location.DistanceFrom(target) < 0.2 && CurrentState != (int)State.Attack)
            {
                Position2D p = (oppMarkID.HasValue ? Model.Opponents[oppMarkID.Value].Location : Model.BallState.Location);
                if (CurrentState == (int)State.cutball)
                    p = Model.BallState.Location;
                Planner.Add(RobotID, target, (p - Model.OurRobots[RobotID].Location).AngleInDegrees, PathType.UnSafe, false, true, true, false);
            }
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            if ((Model.BallState.Location.X < -0.6 || !oppBallOwner) || NormalSharedState.ActiveInfo.CurrentAction == NormalSharedState.ActiveActionMode.Pass)
            {
                CurrentState = (int)State.Attack;
            }
            else
            {
                Vector2D staticVec = new Vector2D(1, 0);
                Line OppSpeedLine = new Line();
                bool cancelInTheWay = false;
                int? oppFirstOwnerId = engine.GameInfo.OppTeam.BallOwner;
                int? ourFirstOwnerId = engine.GameInfo.OppTeam.BallOwner;
                double MarkerDistFromGoal = 0, OppDistFromGoal = 0;

                if (!ballIsMove || (oppMarkID == oppFirstOwnerId && !ballIsMove /*&& (TargetPos.DistanceFrom(Model.BallState.Location)) > 0.5)*/))
                {
                    if (oppMarkID.HasValue && Model.Opponents.ContainsKey(oppMarkID.Value))
                    {
                        CurrentState = (int)State.marknear;
                    }
                }
                else if (ballIsMove)
                {
                    CurrentState = (int)State.marknear;
                }
                if (oppMarkID.HasValue && Model.Opponents.ContainsKey(oppMarkID.Value))
                {
                    regional = null;
                    OppSpeedLine = new Line(Model.Opponents[oppMarkID.Value].Location + Model.Opponents[oppMarkID.Value].Speed, Model.Opponents[oppMarkID.Value].Location);
                    MarkerDistFromGoal = Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter);
                    OppDistFromGoal = Model.Opponents[oppMarkID.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) + 0.07;
                    if (Model.Opponents[oppMarkID.Value].Location.DistanceFrom(Model.BallState.Location) < 0.17)
                    {
                        initialpos = Model.OurRobots[RobotID].Location;
                    }
                    intersect = IntersectFind(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location);
                    velocity = Model.BallState.Speed.Size;
                    ballcoeff = root(-.3, velocity, Model.BallState.Location.DistanceFrom(intersect));
                    robotCoeff = predicttime(Model, RobotID, initialpos, intersect);
                    robotIntersectTime = timeRobotToTargetInIntersect(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location, intersect);
                    if (ballcoeff > 0 && ballcoeff < robotCoeff + treshTime && Model.OurRobots[RobotID].Location.DistanceFrom(intersect) < 1)
                        cutFlag = true;
                    else
                        cutFlag = false;
                }
                if (oppMarkID.HasValue && Model.Opponents.ContainsKey(oppMarkID.Value))
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 1, 1, new List<int> { RobotID }, new List<int> { oppMarkID.Value });
                    Line ourGoalCenterOppRobot1 = new Line(Model.Opponents[oppMarkID.Value].Location, Model.Opponents[oppMarkID.Value].Location + Model.Opponents[oppMarkID.Value].Speed);
                    Line perpendicular1 = ourGoalCenterOppRobot1.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                    Position2D? intersect1 = ourGoalCenterOppRobot1.IntersectWithLine(perpendicular1);
                    cancelInTheWay = false;
                    if (intersect1.HasValue)
                    {
                        if (obs.Meet(Model.OurRobots[RobotID], new SingleObjectState(intersect1.Value, Vector2D.Zero, 0), MotionPlannerParameters.RobotRadi))
                        {
                            cancelInTheWay = true;
                        }
                    }
                }

                if (oppMarkID.HasValue && Model.Opponents.ContainsKey(oppMarkID.Value))
                {
                    if (((Model.Opponents[oppMarkID.Value].Speed.GetNormnalizedCopy().InnerProduct((GameParameters.OurGoalCenter - Model.Opponents[oppMarkID.Value].Location).GetNormnalizedCopy()) > 0.70 && Model.Opponents[oppMarkID.Value].Speed.Size > 0.30)
                                  && (Model.Opponents[oppMarkID.Value].Location.DistanceFrom(GameParameters.OurGoalCenter) - Model.OurRobots[RobotID].Location.DistanceFrom(GameParameters.OurGoalCenter) > 0.090
                                  && Math.Abs(Vector2D.AngleBetweenInDegrees(GameParameters.OurGoalCenter - Model.Opponents[oppMarkID.Value].Location, Model.OurRobots[RobotID].Location - Model.Opponents[oppMarkID.Value].Location)) < 20
                                  && Model.Opponents[oppMarkID.Value].Location.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.30))
                                  )
                    {
                        CurrentState = (int)State.Stop;
                    }
                    else if (cutFlag && Model.OurRobots[RobotID].Location.DistanceFrom(intersect) < 0.5)
                    {
                        CurrentState = (int)State.cutball;
                    }
                    else
                    {
                        if (((Model.Opponents[oppMarkID.Value].Speed.GetNormnalizedCopy().InnerProduct(staticVec.GetNormnalizedCopy()) > 0.60 && Model.Opponents[oppMarkID.Value].Speed.Size > 0.5)
                            && (((OppSpeedLine.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location)).IntersectWithLine(OppSpeedLine).Value)
                            - Model.Opponents[oppMarkID.Value].Location).InnerProduct((Model.Opponents[oppMarkID.Value].Location + Model.Opponents[oppMarkID.Value].Speed) - Model.Opponents[oppMarkID.Value].Location) > 0.00
                            && !cancelInTheWay) && MarkerDistFromGoal < OppDistFromGoal)
                        {
                            CurrentState = (int)State.IntheWay;
                        }
                        else if (Math.Abs(Vector2D.AngleBetweenInDegrees(Model.Opponents[oppMarkID.Value].Speed, GameParameters.OurGoalCenter - Model.Opponents[oppMarkID.Value].Location)) > 100 && Model.Opponents[oppMarkID.Value].Speed.Size > 0.50 && MarkerDistFromGoal + .1 < OppDistFromGoal)
                        {
                            CurrentState = (int)State.goback;
                        }
                        else if (!cutFlag && OppDistFromGoal < MarkerDistFromGoal && Model.Opponents[oppMarkID.Value].Location.X >= Model.OurRobots[RobotID].Location.X)
                        {
                            CurrentState = (int)State.behind;
                        }
                        else if (!cutFlag && Model.Opponents[oppMarkID.Value].Location.X > 0.5 + 0.1 && Model.Opponents[oppMarkID.Value].Speed.Size < 0.5 && OppDistFromGoal + 0.1 > MarkerDistFromGoal)
                        {
                            CurrentState = (int)State.marknear;
                        }
                        else if (!cutFlag && Model.Opponents[oppMarkID.Value].Location.X < 0.5 - 0.1 && Model.Opponents[oppMarkID.Value].Speed.Size < 0.5 && OppDistFromGoal + 0.1 > MarkerDistFromGoal)
                        {
                            CurrentState = (int)State.markfar;
                        }
                        //else if (!cutFlag && )
                        //{

                        //}
                    }
                }
                else
                {
                    CurrentState = (int)State.regional;
                }
            }
            //CurrentState = (int)State.regional;
        }


        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            double cost;
            cost = Model.OurRobots[RobotID].Location.DistanceFrom(NormalSharedState.CommonInfo.NormalAttackerMarker1Target);

            return cost;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { new Marker2Normal8Robot(), new Marker1Normal8Robot(), new ActiveRole2017() };
            //if (CurrentState == (int)State.regional)
            //{
            //    res = new List<RoleBase>() { new Marker1Normal8Robot() };
            //}
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return true;
        }

        public Position2D CalculateTarget(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            if (!oppBallOwner && engine.GameInfo.OppTeam.BallOwner.HasValue && Model.Opponents[engine.GameInfo.OppTeam.BallOwner.Value].Location.DistanceFrom(Model.BallState.Location) < 0.15)
                oppBallOwner = true;

            if (CurrentState == (int)State.Attack)
            {
                #region Attack

                #endregion
            }
            else
            {
                #region Defenc

                if (CurrentState != (int)State.Stop)
                {
                    firstTime = true;
                    //staticAngle = true;
                }
                #region marknear
                if (CurrentState == (int)State.marknear)
                {
                    DrawingObjects.AddObject(new StringDraw("Marknear", Model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), "467");
                    //oppMarkPos = Model.Opponents[oppMarkID.Value].Location;
                    Vector2D ourGoalOppRobot = new Vector2D();
                    ourGoalOppRobot = (GameParameters.OurGoalCenter - Model.Opponents[oppMarkID.Value].Location).GetNormalizeToCopy(distNear);
                    target = Model.Opponents[oppMarkID.Value].Location + ourGoalOppRobot;
                    target.DrawColor = Color.AliceBlue;
                    // DrawingObjects.AddObject(target);
                }
                #endregion
                #region markfar
                if (CurrentState == (int)State.markfar)
                {
                    DrawingObjects.AddObject(new StringDraw("Markfar", Model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), "88467");
                    // DrawingObjects.AddObject(new Circle(Model.Opponents[oppMarkID.Value].Location, 0.02f));
                    target = new Position2D();
                    Line leftGoalline = new Line(Model.Opponents[oppMarkID.Value].Location, GameParameters.OurGoalLeft);

                    Line rightGoalline = new Line(Model.Opponents[oppMarkID.Value].Location, GameParameters.OurGoalRight);

                    Vector2D goalRobot = Model.Opponents[oppMarkID.Value].Location - GameParameters.OurGoalCenter;

                    Position2D leftGoalpos = GameParameters.OurGoalCenter + (GameParameters.OurGoalLeft - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.09);

                    Position2D rightGoalpos = GameParameters.OurGoalCenter + (GameParameters.OurGoalRight - GameParameters.OurGoalCenter).GetNormalizeToCopy(0.09);

                    Position2D leftOpp = leftGoalpos + goalRobot;

                    Position2D rightOpp = rightGoalpos + goalRobot;

                    Line leftOppline = new Line(leftOpp, leftGoalpos);

                    Line rightOppline = new Line(rightOpp, rightGoalpos);

                    Position2D? leftIntersect = leftGoalline.IntersectWithLine(leftOppline);

                    Position2D? rightIntersect = rightGoalline.IntersectWithLine(rightOppline);

                    if (leftIntersect.HasValue && rightIntersect.HasValue)
                    {
                        Position2D bet = rightIntersect.Value + (leftIntersect.Value - rightIntersect.Value).GetNormalizeToCopy(leftIntersect.Value.DistanceFrom(rightIntersect.Value) / 2);
                        target = bet + (bet - Model.Opponents[oppMarkID.Value].Location).GetNormalizeToCopy(0.09);
                    }
                    else
                    {
                        target = GameParameters.OurGoalCenter + (GameParameters.OurGoalCenter - Model.Opponents[oppMarkID.Value].Location).GetNormalizeToCopy(GameParameters.OurGoalCenter.DistanceFrom(Model.Opponents[oppMarkID.Value].Location) - 0.2);
                    }
                    if (Model.Opponents[oppMarkID.Value].Speed.Size > 0.5)
                    {
                        target = target + Model.Opponents[oppMarkID.Value].Speed.GetNormalizeToCopy(Model.Opponents[oppMarkID.Value].Speed.Size * 2);
                    }
                    if (!ballIsMove && Model.Opponents[oppMarkID.Value].Location.DistanceFrom(Model.BallState.Location) < 0.2)
                    {
                        target = Model.Opponents[oppMarkID.Value].Location + (GameParameters.OurGoalCenter - Model.Opponents[oppMarkID.Value].Location).GetNormalizeToCopy(0.7);
                    }
                    if (flagdraw)
                    {
                        //DrawingObjects.AddObject(leftGoalline);
                        //DrawingObjects.AddObject(rightGoalline);
                        //DrawingObjects.AddObject(new Circle(leftGoalpos, 0.02f));
                        //DrawingObjects.AddObject(new Circle(rightGoalpos, 0.02f));
                        //DrawingObjects.AddObject(new Circle(leftOpp, 0.02f));
                        //DrawingObjects.AddObject(new Circle(rightOpp, 0.02f));
                        //DrawingObjects.AddObject(new Line(rightOpp, leftOpp));
                        //DrawingObjects.AddObject(new Line(leftGoalpos, leftOpp));
                        //DrawingObjects.AddObject(new Line(rightOpp, rightGoalpos));
                        //// DrawingObjects.AddObject(new Circle(bet, 0.02f));
                        //DrawingObjects.AddObject(new Circle(rightIntersect.Value, 0.05f, new Pen(Brushes.Red, 0.02f)));
                        //DrawingObjects.AddObject(new Circle(leftIntersect.Value, 0.05f, new Pen(Brushes.Blue, 0.02f)));
                        //DrawingObjects.AddObject(new Circle(target, 0.02f));
                    }
                }
                #endregion
                #region cutball
                if (CurrentState == (int)State.cutball)
                {
                    DrawingObjects.AddObject(new StringDraw("Cut", Model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), "58467");
                    if (Model.Opponents[oppMarkID.Value].Location.DistanceFrom(Model.BallState.Location) < 0.17)
                    {
                        initialpos = Model.OurRobots[RobotID].Location;
                    }
                    intersect = IntersectFind(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location);
                    velocity = Model.BallState.Speed.Size;
                    ballcoeff = root(-.3, velocity, Model.BallState.Location.DistanceFrom(intersect));
                    robotCoeff = predicttime(Model, RobotID, initialpos, intersect);
                    robotIntersectTime = timeRobotToTargetInIntersect(Model, RobotID, initialpos, Model.OurRobots[RobotID].Location, intersect);
                    if (intersect.DistanceFrom(Model.OurRobots[RobotID].Location) < 1 && GameParameters.IsInField(intersect, 0.05))
                    {
                        target = intersect;
                    }
                    //Circle feilCircle = new Circle(intersect, 0.08);
                    //if (!feilCircle.IsInCircle(Model.BallState.Location))
                    //{
                    //    cutFlag = false;
                    //}
                    double x, y;
                    if (GameParameters.IsInDangerousZone(target, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                        cutFlag = false;
                    if (flagdraw)
                    {
                        //DrawingObjects.AddObject(new StringDraw("cutFlag: " + cutFlag.ToString(), new Position2D(0.1, 3)));
                        //DrawingObjects.AddObject(new StringDraw("intersect: " + intersect.ToString(), new Position2D(0.2, 3)));
                        //DrawingObjects.AddObject(new Circle(intersect, 0.1, new Pen(Brushes.Red, 0.02f)));
                        //DrawingObjects.AddObject(new StringDraw("velocity: " + velocity.ToString(), new Position2D(0.3, 3)));
                        //DrawingObjects.AddObject(new StringDraw("ballcoeff: " + ballcoeff.ToString(), new Position2D(0.4, 3)));
                        //DrawingObjects.AddObject(new StringDraw("robotCoeff: " + robotCoeff.ToString(), new Position2D(0.5, 3)));
                        //DrawingObjects.AddObject(new StringDraw("robotIntersectTime: " + robotIntersectTime.ToString(), new Position2D(0.6, 3)));
                    }
                }
                #endregion
                #region Stop
                if (CurrentState == (int)State.Stop)
                {
                    firsttimedanger = true;
                    //DrawingObjects.AddObject(new StringDraw("STOP", Model.OurRobots[RobotID].Location.Extend(.5, 0)), "535132132");
                    if (firstTime)
                    {
                        firstTime = false;
                        target = Model.OurRobots[RobotID].Location;
                    }
                    dangerzone = true;
                    target = target;
                }
                #endregion
                #region InTheWay
                if (CurrentState == (int)State.IntheWay)
                {
                    DrawingObjects.AddObject(new StringDraw("Intheway", Model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), "8638ss63j1322");
                    firsttimedanger = true;
                    noIntersect = false;
                    Line ourGoalCenterOppRobot = new Line(Model.Opponents[oppMarkID.Value].Location, Model.Opponents[oppMarkID.Value].Location + Model.Opponents[oppMarkID.Value].Speed);
                    Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                    Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                    if (intersect.HasValue)
                    {
                        //if (firstTimeAngle)
                        //{
                        //    angle = Model.OurRobots[RobotID].Angle.Value;
                        //}
                        double x, y;
                        if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                            intersect = GameParameters.OurGoalCenter + (Model.Opponents[oppMarkID.Value].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(Model.Opponents[oppMarkID.Value], 0.20));

                        intersectG = intersect.Value;
                        int counter = 0;
                        intersectG = intersect.Value;
                        bool ourconflict = true;
                        bool oppconflict = true;
                        while (!ourconflict && !oppconflict && counter < 15)
                        {
                            counter++;
                            intersectG = GameParameters.OurGoalCenter + (intersectG - GameParameters.OurGoalCenter).GetNormalizeToCopy((intersectG - GameParameters.OurGoalCenter).Size + 0.05);
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
                        dangerzone = true;
                        target = intersectG;
                    }
                    else
                    {
                        dangerzone = true;
                        noIntersect = true; // You Are Near Why go to Prependicular, Don't Fear go to Near Marking State 
                    }

                    target = intersectG;
                }
                #endregion
                #region goBack
                if (CurrentState == (int)State.goback)
                {
                    if (oppMarkID.HasValue && Model.Opponents.ContainsKey(oppMarkID.Value))
                    {
                        DrawingObjects.AddObject(new StringDraw("goback", Model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), "638638ss63j1");
                        Line ourGoalCenterOppRobot = new Line(Model.Opponents[oppMarkID.Value].Location, GameParameters.OurGoalCenter);
                        Line perpendicular = ourGoalCenterOppRobot.PerpenducilarLineToPoint(Model.OurRobots[RobotID].Location);
                        Position2D? intersect = ourGoalCenterOppRobot.IntersectWithLine(perpendicular);
                        Position2D extendedtarget = new Position2D();
                        if (intersect.HasValue)
                        {
                            noIntersect = false;

                            //if (firstTimeAngle)
                            //{
                            //   // angle = Model.OurRobots[RobotID].Angle.Value;
                            //}
                            double x, y;
                            if (GameParameters.IsInDangerousZone(intersect.Value, false, FreekickDefence.AdditionalSafeRadi, out x, out y))
                                intersect = GameParameters.OurGoalCenter + (Model.Opponents[oppMarkID.Value].Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(Model.Opponents[oppMarkID.Value], 0.20));
                            intersectG = intersect.Value;

                            if (firsttimedanger)
                            {
                                firsttimedanger = false;
                                dangerzone = PointOutOfdangerZone(Model, RobotID, oppMarkID.Value, intersectG, out extendedtarget);
                            }
                            PointOutOfdangerZone(Model, RobotID, oppMarkID.Value, intersectG, out extendedtarget);
                        }
                        else
                        {
                            noIntersect = true; // Are you Crazy??? two line don't have any intersect ???!! special state :)
                        }
                        target = extendedtarget;
                        //retangle = angle;
                        target = extendedtarget;
                        if (Model.Opponents[oppMarkID.Value].Location.X < 1.5 && testrole)
                        {
                            Line l1 = new Line(GameParameters.OurGoalCenter, Model.Opponents[oppMarkID.Value].Location);
                            // DrawingObjects.AddObject(l1, "as32d4s3a2d321sa432d");
                            Line l2 = new Line(new Position2D(1.5, -3), new Position2D(1.5, 3));
                            //DrawingObjects.AddObject(l2, "asfdsaf564ds6f4awqwde");
                            Position2D? myIntersect = l1.IntersectWithLine(l2);
                            if (myIntersect.HasValue)
                            {
                                //DrawingObjects.AddObject(new Circle(myIntersect.Value, 0.09), "sr5t3yg45rawtaetraetsre");
                                target = myIntersect.Value;
                            }
                        }
                    }
                }
                #endregion
                #region behind
                if (CurrentState == (int)State.behind)
                {
                    DrawingObjects.AddObject(new StringDraw("behind", Model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), "6j1322");
                    Line linebehind = new Line(Model.OurRobots[RobotID].Location, GameParameters.OurGoalCenter);
                    List<Position2D> intersectwithdanger = GameParameters.LineIntersectWithOurDangerZone(linebehind);
                    if (intersectwithdanger.Count > 0 && (intersectwithdanger.FirstOrDefault()) != Position2D.Zero)
                        target = intersectwithdanger.OrderBy(o => o.DistanceFrom(GameParameters.OurGoalCenter)).FirstOrDefault();
                    Vector2D ourGoalDangerzone = (target - GameParameters.OurGoalCenter);
                    target = target + (ourGoalDangerzone).GetNormalizeToCopy(GetNormalizeBehind);
                    // target = target + Model.Opponents[oppMarkID.Value].Speed.GetNormalizeToCopy(Model.Opponents[oppMarkID.Value].Speed.Size*2);
                    //target += ((target - Model.OurRobots[RobotID].Location).GetNormalizeToCopy(5));
                    //DrawingObjects.AddObject(new Circle(target, 0.1), "shdjhd");
                }
                #endregion
                #region regional
                if (CurrentState == (int)State.regional)
                {
                    //DrawingObjects.AddObject(new StringDraw("regional", Model.OurRobots[RobotID].Location.Extend(0.50, 0.00)), "6322");
                    //Vector2D vec1 = Position2D.Zero - GameParameters.OurGoalCenter;
                    //Vector2D vec2 = Vector2D.FromAngleSize(vec1.AngleInRadians + (45 * Math.PI / 180), vec1.Size);
                    //Line l1 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + vec1);
                    //Line l2 = new Line(GameParameters.OurGoalCenter, GameParameters.OurGoalCenter + vec2);
                    //Position2D pos1 = GameParameters.LineIntersectWithDangerZone(l1, true).FirstOrDefault();
                    //Position2D pos2 = GameParameters.LineIntersectWithDangerZone(l2, true).FirstOrDefault();
                    //List<Position2D> regionalPos = new List<Position2D>();
                    //if (regional.HasValue && Model.OurRobots[RobotID].Location.DistanceFrom(regional.Value) < 0.12)
                    //{
                    //    InRegional = true;
                    //}
                    //if (regional.HasValue)
                    //{
                    //    foreach (var id in Model.OurRobots.Where(w => w.Key != RobotID))
                    //    {
                    //        if (new Circle(regional.Value, 0.12).IsInCircle(id.Value.Location))
                    //        {
                    //            InRegional = false;
                    //        }
                    //    }
                    //}
                    //if (!InRegional)
                    //{
                    //    regionalPos.Add(pos1);
                    //    if (firstballpos.Y > 0)
                    //    {
                    //        regionalPos.Add(new Position2D(pos2.X, -1 * pos2.Y));
                    //        regionalPos.Add(pos2);
                    //    }
                    //    else
                    //    {
                    //        regionalPos.Add(pos2);
                    //        regionalPos.Add(new Position2D(pos2.X, -1 * pos2.Y));
                    //    }

                    //    foreach (var item in regionalPos)
                    //    {
                    //        bool IsFull = false;
                    //        foreach (var id in Model.OurRobots.Where(w => w.Key != RobotID))
                    //        {
                    //            if (new Circle(item, 0.12).IsInCircle(id.Value.Location))
                    //            {
                    //                IsFull = true;
                    //            }
                    //        }
                    //        if (!IsFull)
                    //        {
                    //            regional = item;
                    //            target = item;
                    //        }
                    //    }
                    //}
                    if (oppAttackerIds.Count < 1)
                    {
                        target = Model.OurRobots[RobotID].Location;
                    }
                }
                #endregion



                if (!ballIsMove)
                {
                    if (oppMarkID.HasValue && Model.Opponents.ContainsKey(oppMarkID.Value))
                    {
                        if (Model.Opponents[oppMarkID.Value].Location.X > 0)
                        {
                            if (Model.Opponents[oppMarkID.Value].Location.DistanceFrom(Model.BallState.Location) < 0.5)
                            {
                                //double deltaDist = 0.5 - target.DistanceFrom(Model.BallState.Location);
                                //target = Model.Opponents[oppMarkID.Value].Location + (GameParameters.OurGoalCenter - Model.Opponents[oppMarkID.Value].Location).GetNormalizeToCopy(distToMark + deltaDist);
                                Circle circleStop = new Circle(Model.BallState.Location, 0.6);
                                Line lineStop = new Line(GameParameters.OurGoalCenter, Model.BallState.Location);
                                List<Position2D> intersect = circleStop.Intersect(lineStop);
                                if (intersect.Count > 0 && (intersect.FirstOrDefault()) != Position2D.Zero)
                                {
                                    target = intersect.OrderBy(u => u.DistanceFrom(GameParameters.OurGoalCenter)).FirstOrDefault();
                                }
                            }

                        }
                        if (Model.Opponents[oppMarkID.Value].Location.X < 0)
                        {
                            target = Model.Opponents[oppMarkID.Value].Location + (GameParameters.OurGoalCenter - Model.Opponents[oppMarkID.Value].Location).GetNormalizeToCopy(distToMark);
                            double deltaDist = 0;
                            Line halfLine = new Line(new Position2D(0, -3), new Position2D(0, 3));
                            Line l1 = new Line(target, Model.Opponents[oppMarkID.Value].Location);
                            Position2D? p = l1.IntersectWithLine(halfLine);
                            if (p.HasValue)
                            {
                                deltaDist = p.Value.DistanceFrom(Model.Opponents[oppMarkID.Value].Location);
                            }
                            target = target + (target - Model.Opponents[oppMarkID.Value].Location).GetNormalizeToCopy(deltaDist);
                        }
                    }
                }
                #endregion

            }
            Position2D? overlapTarget = null;
            foreach (var item in Model.OurRobots.Where(w => w.Key != RobotID))
            {
                Circle c = new Circle(target, 0.21);
                if (c.IsInCircle(item.Value.Location))
                {
                    double d = 0.1 + (0.1 - (target.DistanceFrom(item.Value.Location)));
                    overlapTarget = target + (target - item.Value.Location).GetNormalizeToCopy(d);
                }
            }
            //DrawingObjects.AddObject(new StringDraw(target.toString(), Position2D.Zero.Extend(0.1, 0)));
            return (overlapTarget.HasValue ? overlapTarget.Value : target);
        }

        private bool PointOutOfdangerZone(WorldModel model, int RobotID, int MarkID, Position2D targetreference, out Position2D targetvalue)
        {
            targetvalue = targetreference;
            Obstacle obs = new Obstacle();
            obs.Type = ObstacleType.ZoneCircle;
            obs.R = new Vector2D(MotionPlannerParameters.DangerZoneW, MotionPlannerParameters.DangerZoneW);
            obs.State = new SingleObjectState(GameParameters.OurGoalCenter, new Vector2D(), null);
            bool meet = true;
            int counter = 0;
            while (meet && counter < 15 && targetreference.DistanceFrom(GameParameters.OurGoalCenter) + 0.20 < model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter))
            {
                counter++;
                meet = obs.Meet(model.OurRobots[RobotID], new SingleObjectState(targetreference, Vector2D.Zero, 0.0f), RobotParameters.OurRobotParams.Diameter / 2);
                targetvalue = GameParameters.OurGoalCenter + (targetreference - GameParameters.OurGoalCenter).GetNormalizeToCopy((targetvalue - GameParameters.OurGoalCenter).Size + 0.05);
                if (counter > 14 || !(targetvalue.DistanceFrom(GameParameters.OurGoalCenter) + 0.20 < model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter)))
                {
                    targetvalue = model.Opponents[MarkID].Location + (GameParameters.OurGoalCenter - model.Opponents[MarkID].Location).GetNormalizeToCopy(markDistance);
                    return false;

                }
                else if (!meet)
                {
                    return true;
                }
            }
            if (targetvalue.DistanceFrom(GameParameters.OurGoalCenter) - .2 > model.Opponents[MarkID].Location.DistanceFrom(GameParameters.OurGoalCenter))
            {
                targetvalue = model.Opponents[MarkID].Location + (GameParameters.OurGoalCenter - model.Opponents[MarkID].Location).GetNormalizeToCopy(markDistance);
            }
            targetvalue = targetreference;
            return false;
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
        double root(double a, double initialV, double deltaX)
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

        enum State
        {
            regional,
            Stop,
            marknear,
            markfar,
            cutball,
            IntheWay,
            goback,
            behind,
            position,
            Attack
        }
    }
}

