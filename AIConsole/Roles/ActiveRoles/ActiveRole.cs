using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.Planning.GamePlanner.Types;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public class ActiveRole : RoleBase
    {
        //  ActiveParameters param = new ActiveParameters ();
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        Position2D Target = GameParameters.OppGoalCenter;

        public Position2D Tar
        {
            get { return Target; }
        }
        bool firstInForceStart = true;
        Position2D posInForceStart = Position2D.Zero;
        bool force = false, gotForce = false;
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, int? passID)
        {
            Position2D? passPoint = null;
            if (passID.HasValue && Model.OurRobots.ContainsKey(passID.Value))
                passPoint = Model.OurRobots[passID.Value].Location;
            SingleObjectState ball = Model.BallState;
            SingleObjectState robot = Model.OurRobots[RobotID];
            Vector2D angleVec = Vector2D.FromAngleSize(Math.PI * robot.Angle.Value / 180.0, 1);
            Line angleLine = new Line(robot.Location, robot.Location + angleVec);
            Line goalLine = new Line(GameParameters.OppGoalRight, GameParameters.OppGoalLeft);
            Position2D? intersectVsGoalLine = angleLine.IntersectWithLine(goalLine);
            Target = GameParameters.OppGoalLeft;
            string strState = "";

            double kick = 0;
            bool isChip = false;

            #region Sweep From dangerZone

            //double dist = Model.Opponents.Any ( p => p.Value.Location.DistanceFrom ( ball.Location ));

            int minIndx = int.MaxValue;
            double minDist = double.MaxValue;

            foreach (var item in Model.Opponents.Keys)
            {
                double dist = Model.Opponents[item].Location.DistanceFrom(ball.Location);
                if (dist < minDist)
                {
                    minDist = dist;
                    minIndx = item;
                }
            }

            if (ball.Location.X > ActiveParameters.sweepZone && minDist < 1)
            {
                double safeRadi = 0.5;
                bool danger = false;
                Target = ball.Location + new Vector2D(-1, 0);
                if (minDist < safeRadi)
                {
                    danger = true;
                    Target = ball.Location - (GameParameters.OurGoalCenter - ball.Location).GetNormalizeToCopy(1);
                }
                kick = 8;
                isChip = true;
                if (ActiveParameters.clearMode == ActiveParameters.NonClearMode.Direct)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, /*Model.OurRobots.Keys.ToList()*/ new List<int>() { RobotID }, null);
                    Vector2D ballTarget = (Target - Model.BallState.Location).GetNormalizeToCopy(safeRadi);

                    if ((danger && !obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget, Vector2D.Zero, 0), 0.03))
                        || (!danger && obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget.GetNormalizeToCopy(3), Vector2D.Zero, 0), 0.05)))
                    {
                        isChip = true;
                    }
                    else
                    {
                        isChip = false;
                    }
                }
                strState = "Sweeping";
            }
            #endregion
            else
            {

                #region Open to Kick
                if (GoodPointInGoal.HasValue && goodness > ActiveParameters.minGoodness)
                {
                    Target = GoodPointInGoal.Value;
                    if (angleVec.InnerProduct(GameParameters.OppGoalCenter - ball.Location) > 0 && intersectVsGoalLine.HasValue)
                    {
                        if (ActiveParameters.KickInRegion)
                        {
                            List<VisibleGoalInterval> intervals = engine.GameInfo.OppGoalIntervals;
                            foreach (var item in intervals)
                            {
                                Vector2D v1 = new Position2D(GameParameters.OppGoalCenter.X, item.interval.Start) - intersectVsGoalLine.Value;
                                Vector2D v2 = new Position2D(GameParameters.OppGoalCenter.X, item.interval.End) - intersectVsGoalLine.Value;
                                if (v1.InnerProduct(v2) < 0 && v1.Size >= ActiveParameters.kickRegionAcuercy && v2.Size >= ActiveParameters.kickRegionAcuercy)
                                {
                                    kick = Program.MaxKickSpeed;
                                    isChip = false;
                                    strState += " KickInRegion";
                                    break;
                                }
                            }
                        }
                        Vector2D v = GoodPointInGoal.Value - intersectVsGoalLine.Value;
                        DrawingObjects.AddObject(intersectVsGoalLine.Value);
                        if (v.Size < ActiveParameters.kickAcuercy)
                        {
                            kick = Program.MaxKickSpeed;
                            isChip = false;
                            strState += " AccuratedKick";
                        }
                    }

                }
                #endregion

                #region No way to kick
                else
                {
                    //chk is there any robot in front
                    Vector2D leftVec = (GameParameters.OppGoalLeft + new Vector2D(0, -2.5)) - ball.Location;
                    Vector2D rightVec = (GameParameters.OppGoalRight + new Vector2D(0, 2.5)) - ball.Location;
                    bool clear = true;
                    bool confilicted = false;
                    foreach (var item in Model.Opponents.Keys)
                    {
                        SingleObjectState oppRobot = Model.Opponents[item];
                        Vector2D v = oppRobot.Location - ball.Location;
                        if (v.Size < ActiveParameters.clearRobotZone && Math.Abs(v.AngleInRadians) > Math.PI / 2)
                        {
                            clear = false;
                            if (v.Size < ActiveParameters.ConfilictZone)
                            {
                                confilicted = true;
                            }
                        }
                    }

                    if (ball.Location.X < ActiveParameters.kickAnyWayRegion)
                    {
                        Target = GameParameters.OppGoalCenter;
                        kick = Program.MaxKickSpeed;
                        isChip = false;
                        strState += " kickAnyWay";
                    }
                    else
                    {
                        #region It is clear No robot in front BUT goal is close
                        if (clear)
                        {
                            if (ActiveParameters.playMode == ActiveParameters.PlayMode.Chip)
                            {
                                kick = (GameParameters.OppGoalCenter - ball.Location).GetNormalizeToCopy((GameParameters.OppGoalCenter - ball.Location).Size - 0.5).Size;
                                isChip = true;
                                Target = GameParameters.OppGoalCenter;
                                strState += " Clear Chip";
                            }
                            else if (ActiveParameters.playMode == ActiveParameters.PlayMode.Direct || (ActiveParameters.playMode == ActiveParameters.PlayMode.Pass && !passPoint.HasValue))
                            {
                                Obstacles obs = new Obstacles(Model);
                                obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, Model.Opponents.Keys.ToList());
                                Vector2D v = Vector2D.Zero;
                                if (ActiveParameters.kickDefult == ActiveParameters.KickDefult.Center)
                                {
                                    Target = GameParameters.OppGoalCenter;
                                    v = (Target - Model.BallState.Location).GetNormalizeToCopy(3);
                                    bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
                                    kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(Target) * 0.7;
                                    isChip = b;
                                    strState += " Clear Direct Center";
                                }
                                else
                                {
                                    Target = (ball.Location.Y < 0 ? new Vector2D(0, -0.25) : new Vector2D(0, 0.25)) + GameParameters.OppGoalCenter;
                                    v = (Target - Model.BallState.Location).GetNormalizeToCopy(3);
                                    bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
                                    kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(Target) * 0.7;
                                    isChip = b;
                                    strState += " Clear Direct Rear";
                                }

                            }
                            else if (ActiveParameters.playMode == ActiveParameters.PlayMode.Force)
                            {
                                Target = GameParameters.OppGoalCenter;
                                kick = 0.2;
                                isChip = true;
                                strState += " Clear Force";
                            }
                            else
                            {
                                Obstacles obstacles = new Obstacles(Model);
                                obstacles.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);

                                Target = passPoint.Value;
                                bool meet = obstacles.Meet(ball, new SingleObjectState(passPoint.Value, Vector2D.Zero, null), 0.05);
                                if (meet)
                                {
                                    kick = ball.Location.DistanceFrom(passPoint.Value) / 2;
                                    isChip = true;
                                    strState += " Clear Chip Pass";
                                }
                                else
                                {
                                    kick = (passPoint.HasValue) ? engine.GameInfo.CalculateKickSpeed(Model, RobotID, Model.BallState.Location, passPoint.Value, false, true) : 3;
                                    isChip = false;
                                    strState += " Clear Direct Pass";
                                }
                            }
                        }
                        #endregion
                        else
                        #region Enemy is near we dont have space to move
                        {

                            #region They are too too too close
                            if (confilicted)
                            {
                                if (ball.Location.X < -1.2)
                                {
                                    Target = GameParameters.OppGoalCenter;
                                }
                                else if (ball.Location.X >= -1.2 && ball.Location.X < 0.5)
                                {
                                    Target = ball.Location + new Vector2D(-1, 0);
                                }
                                else
                                {
                                    Target = ball.Location + (ball.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1);
                                }
                                if (ActiveParameters.conflictMode == ActiveParameters.ConflictMode.Direct)
                                {

                                    isChip = false;
                                    kick = 0.5;
                                    strState += " Confilcted Direct";
                                }
                                else
                                {
                                    isChip = false;
                                    kick = 0;
                                    strState += " Confilcted STOP";
                                }
                            }
                            #endregion
                            else
                            {
                                #region We have just liitle space
                                if (ActiveParameters.clearMode == ActiveParameters.NonClearMode.Chip)
                                {
                                    Target = GameParameters.OppGoalCenter;
                                    kick = (GameParameters.OppGoalCenter - ball.Location).GetNormalizeToCopy((GameParameters.OppGoalCenter - ball.Location).Size - 0.8).Size;
                                    isChip = true;
                                    strState += " Little Space Chip";
                                }
                                else
                                {
                                    Target = GameParameters.OppGoalCenter;
                                    kick = Program.MaxKickSpeed;
                                    isChip = false;
                                    strState += " Little Space Direct";
                                }
                                #endregion

                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }

            Color color = color = Color.LightPink;
            if (isChip && kick > 0)
            {
                color = Color.SkyBlue;
            }
            if (!isChip && kick > 0)
            {
                color = Color.YellowGreen;
            }
            DrawingObjects.AddObject(new Line(ball.Location, Target, new System.Drawing.Pen(color, 0.043f)));
            DrawingObjects.AddObject(new StringDraw(strState, GameParameters.OppGoalCenter + new Vector2D(-0.4, 0)));
            GetSkill<GetBallSkill>().SetAvoidDangerZone(true, true);
            GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, Target);

            CurrentState = (int)GetSkill<GetBallSkill>().CurrState;
            OneTouch = GetSkill<GetBallSkill>().InNear;
            IncomPred = GetSkill<GetBallSkill>().IncommingPred;
            bool Spin = false;
            //if (!GetSkill<GetBallSkill>().FarFromBack && GetSkill<GetBallSkill>().CurrState == GetBallSkill.GetBallState.Incomming)
            //{
            //    Spin = true;
            //}

            Planner.AddKick(RobotID, kickPowerType.Speed, kick, isChip, Spin);
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D? PassPoint, int? attackerID, bool PassIsChip, bool kickAnyAnyWay, bool forceStart)
        {

            SingleObjectState ball = Model.BallState;
            SingleObjectState robot = Model.OurRobots[RobotID];
            Vector2D angleVec = Vector2D.FromAngleSize(Math.PI * robot.Angle.Value / 180.0, 1);
            Line angleLine = new Line(ball.Location, ball.Location + angleVec);
            Line goalLine = new Line(GameParameters.OppGoalRight, GameParameters.OppGoalLeft);
            Position2D? intersectVsGoalLine = angleLine.IntersectWithLine(goalLine);
            Target = GameParameters.OppGoalLeft;
            string strState = "";
            if (!gotForce && !force)
            {
                gotForce = true;
                force = forceStart;
            }
            double kick = 0;
            bool isChip = false;
            if (force && attackerID.HasValue && Model.OurRobots.ContainsKey(attackerID.Value))
            {
                if (firstInForceStart)
                {
                    posInForceStart = Model.BallState.Location;
                    firstInForceStart = false;
                }

                if (Model.BallState.Location.X > 1.5 || Math.Abs(Model.BallState.Location.Y) < 1)
                {
                    Target = posInForceStart + new Vector2D(-1.5, 0);
                }
                else if (Model.BallState.Location.X > -1.2)
                    Target = posInForceStart + new Vector2D(-1.5, 0);//Model.BallState.Location + new Vector2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.7);
                else
                    Target = Model.BallState.Location + new Vector2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.7);

                Target.X = Math.Sign(Target.X) * Math.Min(Math.Abs(Target.X), GameParameters.OurGoalCenter.X - 0.5);
                Target.Y = Math.Sign(Target.Y) * Math.Min(Math.Abs(Target.Y), GameParameters.OurLeftCorner.Y - 0.5);

                if (Model.BallState.Location.X <= -2 || Model.BallState.Location.DistanceFrom(posInForceStart) > 0.1)
                {
                    firstInForceStart = true;
                    force = false;
                }
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), Target - Model.BallState.Location)) < 10)
                {
                    isChip = true;
                    kick = Math.Max(posInForceStart.DistanceFrom(Target), 1.2) * 1.2;
                }
                else
                {
                    isChip = false;
                    kick = 0;
                }
                strState = "Force Start";
            }
            else
                force = false;
            if (!force)
            {
                #region Sweep From dangerZone

                //double dist = Model.Opponents.Any ( p => p.Value.Location.DistanceFrom ( ball.Location ));

                int minIndx = int.MaxValue;
                double minDist = double.MaxValue;

                foreach (var item in Model.Opponents.Keys)
                {
                    double dist = Model.Opponents[item].Location.DistanceFrom(ball.Location);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minIndx = item;
                    }
                }

                if (ball.Location.X > ActiveParameters.sweepZone && minDist < 1)
                {
                    double safeRadi = 0.5;
                    bool danger = false;
                    Target = ball.Location + new Vector2D(-1, 0);
                    if (minDist < safeRadi)
                    {
                        danger = true;
                        Target = ball.Location - (GameParameters.OurGoalCenter - ball.Location).GetNormalizeToCopy(1);
                    }
                    kick = 4;
                    isChip = true;
                    if (ActiveParameters.clearMode == ActiveParameters.NonClearMode.Direct)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, /*Model.OurRobots.Keys.ToList()*/ new List<int>() { RobotID }, null);
                        Vector2D ballTarget = (Target - Model.BallState.Location).GetNormalizeToCopy(safeRadi);

                        if ((danger && !obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget, Vector2D.Zero, 0), 0.03))
                            || (!danger && obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget.GetNormalizeToCopy(3), Vector2D.Zero, 0), 0.05)))
                        {
                            isChip = true;
                        }
                        else
                        {
                            isChip = false;
                            kick = Program.MaxKickSpeed;
                        }
                    }
                    CurrentState = (int)ActiveRoleState.Sweep;
                    strState = "Sweeping";
                }
                #endregion
                else
                {

                    #region Open to Kick
                    if (GoodPointInGoal.HasValue && goodness > ActiveParameters.minGoodness)
                    {
                        CurrentState = (int)ActiveRoleState.Open2kick;
                        Target = GoodPointInGoal.Value;
                        if (kickAnyAnyWay && ball.Location.X < ActiveParameters.kickAnyWayRegion)
                        {
                            kick = Program.MaxKickSpeed;
                            isChip = false;
                            strState += " kickAnyWay";
                        }
                        if (angleVec.InnerProduct(GameParameters.OppGoalCenter - ball.Location) > 0 && intersectVsGoalLine.HasValue)
                        {
                            if (ActiveParameters.KickInRegion)
                            {
                                List<VisibleGoalInterval> intervals = engine.GameInfo.OppGoalIntervals;
                                foreach (var item in intervals)
                                {
                                    Vector2D v1 = new Position2D(GameParameters.OppGoalCenter.X, item.interval.Start) - intersectVsGoalLine.Value;
                                    Vector2D v2 = new Position2D(GameParameters.OppGoalCenter.X, item.interval.End) - intersectVsGoalLine.Value;
                                    if (v1.InnerProduct(v2) < 0 && v1.Size >= ActiveParameters.kickRegionAcuercy && v2.Size >= ActiveParameters.kickRegionAcuercy)
                                    {
                                        kick = Program.MaxKickSpeed;
                                        isChip = false;
                                        strState += " KickInRegion";
                                        break;
                                    }
                                }
                            }
                            Vector2D v = GoodPointInGoal.Value - intersectVsGoalLine.Value;
                            DrawingObjects.AddObject(intersectVsGoalLine.Value);
                            if (v.Size < ActiveParameters.kickAcuercy)
                            {
                                kick = Program.MaxKickSpeed;
                                isChip = false;
                                strState += " AccuratedKick";
                            }
                        }

                    }
                    #endregion

                    #region No way to kick
                    else
                    {
                        //chk is there any robot in front
                        Vector2D leftVec = (GameParameters.OppGoalLeft + new Vector2D(0, -2.5)) - ball.Location;
                        Vector2D rightVec = (GameParameters.OppGoalRight + new Vector2D(0, 2.5)) - ball.Location;
                        bool clear = true;
                        CurrentState = (int)ActiveRoleState.Clear;
                        bool confilicted = false;
                        foreach (var item in Model.Opponents.Keys)
                        {
                            SingleObjectState oppRobot = Model.Opponents[item];
                            Vector2D v = oppRobot.Location - ball.Location;
                            if (v.Size < ActiveParameters.clearRobotZone && Math.Abs(v.AngleInRadians) > Math.PI / 2)
                            {
                                clear = false;
                                if (v.Size < ActiveParameters.ConfilictZone)
                                {
                                    confilicted = true;
                                    CurrentState = (int)ActiveRoleState.Conflict;
                                }
                            }
                        }

                        if (ball.Location.X < ActiveParameters.kickAnyWayRegion)
                        {
                            Target = GameParameters.OppGoalCenter;
                            kick = Program.MaxKickSpeed;
                            isChip = false;
                            strState += " kickAnyWay";

                        }
                        else
                        {
                            #region It is clear No robot in front BUT goal is close
                            if (clear)
                            {
                                if (ActiveParameters.playMode == ActiveParameters.PlayMode.Chip)
                                {
                                    kick = (GameParameters.OppGoalCenter - ball.Location).GetNormalizeToCopy((GameParameters.OppGoalCenter - ball.Location).Size - 0.2).Size;
                                    isChip = true;
                                    Target = GameParameters.OppGoalCenter;
                                    strState += " Clear Chip";
                                }
                                else if (ActiveParameters.playMode == ActiveParameters.PlayMode.Direct || (ActiveParameters.playMode == ActiveParameters.PlayMode.Pass && (!PassPoint.HasValue || (attackerID.HasValue && Model.OurRobots.ContainsKey(attackerID.Value) && Model.OurRobots[attackerID.Value].Location.DistanceFrom(PassPoint.Value) > 0.65))))
                                {
                                    Obstacles obs = new Obstacles(Model);
                                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, Model.Opponents.Keys.ToList());
                                    Vector2D v = Vector2D.Zero;
                                    if (ActiveParameters.kickDefult == ActiveParameters.KickDefult.Center)
                                    {
                                        Target = GameParameters.OppGoalCenter;
                                        v = (Target - Model.BallState.Location).GetNormalizeToCopy(3);
                                        bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
                                        kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(Target) * 0.7;
                                        isChip = b;
                                        strState += " Clear Direct Center";
                                    }
                                    else
                                    {
                                        Target = (ball.Location.Y < 0 ? new Vector2D(0, -0.25) : new Vector2D(0, 0.25)) + GameParameters.OppGoalCenter;
                                        v = (Target - Model.BallState.Location).GetNormalizeToCopy(3);
                                        bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
                                        kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(Target) * 0.7;
                                        isChip = b;
                                        strState += " Clear Direct Rear";
                                    }

                                }
                                else if (ActiveParameters.playMode == ActiveParameters.PlayMode.Force)
                                {
                                    Target = GameParameters.OppGoalCenter;
                                    kick = 0.2;
                                    isChip = true;
                                    strState += " Clear Force";
                                }
                                else
                                {
                                    Target = PassPoint.Value;
                                    double ang = Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), PassPoint.Value - Model.BallState.Location));
                                    bool open2kick = false;
                                    if (ang < 5)
                                    {
                                        open2kick = true;
                                    }
                                    if (!open2kick)
                                    {
                                        kick = 0;
                                        isChip = false;
                                        strState += "Clear Accurate Pass";
                                    }
                                    else if (PassIsChip)
                                    {
                                        kick = ball.Location.DistanceFrom(PassPoint.Value) / 2;
                                        isChip = true;
                                        strState += " Clear Chip Pass";
                                    }
                                    else
                                    {
                                        kick = (PassPoint.HasValue) ? /*engine.GameInfo.CalculateKickSpeed(Model, RobotID, Model.BallState.Location, PassPoint.Value, false, true) * 0.9*/4.2 : 3;
                                        isChip = false;
                                        strState += " Clear Direct Pass";
                                    }

                                }
                            }
                            #endregion
                            else
                            #region Enemy is near we dont have space to move
                            {

                                #region They are too too too close
                                if (confilicted)
                                {
                                    if (ball.Location.X < -1.2)
                                    {
                                        Target = GameParameters.OppGoalCenter;
                                    }
                                    else if (ball.Location.X >= -1.5 && ball.Location.X < 0.5)
                                    {
                                        Target = ball.Location + new Vector2D(-1, 0);
                                    }
                                    else
                                    {
                                        Target = ball.Location + (ball.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1);
                                    }
                                    if (ActiveParameters.conflictMode == ActiveParameters.ConflictMode.Direct)
                                    {

                                        isChip = false;
                                        kick = 0.5;
                                        strState += " Confilcted Direct";
                                    }
                                    else
                                    {
                                        isChip = false;
                                        kick = 0;
                                        strState += " Confilcted STOP";
                                    }
                                }
                                #endregion
                                else
                                {
                                    CurrentState = (int)ActiveRoleState.LittleSpace;
                                    #region We have just liitle space
                                    if (ActiveParameters.clearMode == ActiveParameters.NonClearMode.Chip)
                                    {
                                        Target = GameParameters.OppGoalCenter;
                                        kick = (GameParameters.OppGoalCenter - ball.Location).GetNormalizeToCopy((GameParameters.OppGoalCenter - ball.Location).Size - 0.8).Size;
                                        isChip = true;
                                        strState += " Little Space Chip";
                                    }
                                    else
                                    {
                                        Target = GameParameters.OppGoalCenter;
                                        kick = Program.MaxKickSpeed;
                                        isChip = false;
                                        strState += " Little Space Direct";
                                    }
                                    #endregion

                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
            Color color = color = Color.LightPink;
            if (isChip && kick > 0)
            {
                color = Color.SkyBlue;
            }
            if (!isChip && kick > 0)
            {
                color = Color.YellowGreen;
            }
            DrawingObjects.AddObject(new Line(ball.Location, Target, new System.Drawing.Pen(color, 0.043f)));
            DrawingObjects.AddObject(new StringDraw(strState, GameParameters.OppGoalCenter + new Vector2D(-0.4, 0)));
            GetSkill<GetBallSkill>().SetAvoidDangerZone(true, true);
            GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, Target);

            CurrentState = (int)GetSkill<GetBallSkill>().CurrState;
            OneTouch = GetSkill<GetBallSkill>().InNear;
            IncomPred = GetSkill<GetBallSkill>().IncommingPred;
            bool spin = false;
            //if (!GetSkill<GetBallSkill>().FarFromBack)
            //    spin = true;
            Planner.AddKick(RobotID, kickPowerType.Speed, kick, isChip, spin);
        }
        public void PerformForStrategy(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D? PassPoint, int? attackerID, bool PassIsChip, bool kickAnyAnyWay, bool forceStart)
        {

            SingleObjectState ball = Model.BallState;
            SingleObjectState robot = Model.OurRobots[RobotID];
            Vector2D angleVec = Vector2D.FromAngleSize(Math.PI * robot.Angle.Value / 180.0, 1);
            Line angleLine = new Line(ball.Location, ball.Location + angleVec);
            Line goalLine = new Line(GameParameters.OppGoalRight, GameParameters.OppGoalLeft);
            Position2D? intersectVsGoalLine = angleLine.IntersectWithLine(goalLine);
            Target = GameParameters.OppGoalLeft;
            string strState = "";
            if (!gotForce && !force)
            {
                gotForce = true;
                force = forceStart;
            }
            double kick = 0;
            bool isChip = false;
            if (force && attackerID.HasValue && Model.OurRobots.ContainsKey(attackerID.Value))
            {
                if (firstInForceStart)
                {
                    posInForceStart = Model.BallState.Location;
                    firstInForceStart = false;
                }

                if (Model.BallState.Location.X > 1.5 || Math.Abs(Model.BallState.Location.Y) < 1)
                {
                    Target = posInForceStart + new Vector2D(-1.5, 0);
                }
                else if (Model.BallState.Location.X > -1.2)
                    Target = posInForceStart + new Vector2D(-1.5, 0);//Model.BallState.Location + new Vector2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.7);
                else
                    Target = Model.BallState.Location + new Vector2D(-0.7, -Math.Sign(Model.BallState.Location.Y) * 0.7);

                Target.X = Math.Sign(Target.X) * Math.Min(Math.Abs(Target.X), GameParameters.OurGoalCenter.X - 0.5);
                Target.Y = Math.Sign(Target.Y) * Math.Min(Math.Abs(Target.Y), GameParameters.OurLeftCorner.Y - 0.5);

                if (Model.BallState.Location.X <= -2 || Model.BallState.Location.DistanceFrom(posInForceStart) > 0.1)
                {
                    firstInForceStart = true;
                    force = false;
                }
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), Target - Model.BallState.Location)) < 10)
                {
                    isChip = true;
                    kick = Math.Max(posInForceStart.DistanceFrom(Target), 1.2) * 1.2;
                }
                else
                {
                    isChip = false;
                    kick = 0;
                }
                strState = "Force Start";
            }
            else
                force = false;
            if (!force)
            {
                #region Sweep From dangerZone

                //double dist = Model.Opponents.Any ( p => p.Value.Location.DistanceFrom ( ball.Location ));

                int minIndx = int.MaxValue;
                double minDist = double.MaxValue;

                foreach (var item in Model.Opponents.Keys)
                {
                    double dist = Model.Opponents[item].Location.DistanceFrom(ball.Location);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minIndx = item;
                    }
                }

                if (ball.Location.X > ActiveParameters.sweepZone && minDist < 1)
                {
                    double safeRadi = 0.5;
                    bool danger = false;
                    Target = ball.Location + new Vector2D(-1, 0);
                    if (minDist < safeRadi)
                    {
                        danger = true;
                        Target = ball.Location - (GameParameters.OurGoalCenter - ball.Location).GetNormalizeToCopy(1);
                    }
                    kick = 4;
                    isChip = true;
                    if (ActiveParameters.clearMode == ActiveParameters.NonClearMode.Direct)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, /*Model.OurRobots.Keys.ToList()*/ new List<int>() { RobotID }, null);
                        Vector2D ballTarget = (Target - Model.BallState.Location).GetNormalizeToCopy(safeRadi);

                        if ((danger && !obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget, Vector2D.Zero, 0), 0.03))
                            || (!danger && obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + ballTarget.GetNormalizeToCopy(3), Vector2D.Zero, 0), 0.05)))
                        {
                            isChip = true;
                        }
                        else
                        {
                            isChip = false;
                            kick = Program.MaxKickSpeed;
                        }
                    }
                    CurrentState = (int)ActiveRoleState.Sweep;
                    strState = "Sweeping";
                }
                #endregion
                else
                {

                    #region Open to Kick
                    if (GoodPointInGoal.HasValue && goodness > ActiveParameters.minGoodness)
                    {
                        CurrentState = (int)ActiveRoleState.Open2kick;
                        Target = GoodPointInGoal.Value;
                        if (kickAnyAnyWay && ball.Location.X < ActiveParameters.kickAnyWayRegion)
                        {
                            kick = Program.MaxKickSpeed;
                            isChip = false;
                            strState += " kickAnyWay";
                        }
                        if (angleVec.InnerProduct(GameParameters.OppGoalCenter - ball.Location) > 0 && intersectVsGoalLine.HasValue)
                        {
                            if (ActiveParameters.KickInRegion)
                            {
                                List<VisibleGoalInterval> intervals = engine.GameInfo.OppGoalIntervals;
                                foreach (var item in intervals)
                                {
                                    Vector2D v1 = new Position2D(GameParameters.OppGoalCenter.X, item.interval.Start) - intersectVsGoalLine.Value;
                                    Vector2D v2 = new Position2D(GameParameters.OppGoalCenter.X, item.interval.End) - intersectVsGoalLine.Value;
                                    if (v1.InnerProduct(v2) < 0 && v1.Size >= ActiveParameters.kickRegionAcuercy && v2.Size >= ActiveParameters.kickRegionAcuercy)
                                    {
                                        kick = Program.MaxKickSpeed;
                                        isChip = false;
                                        strState += " KickInRegion";
                                        break;
                                    }
                                }
                            }
                            Vector2D v = GoodPointInGoal.Value - intersectVsGoalLine.Value;
                            DrawingObjects.AddObject(intersectVsGoalLine.Value);
                            if (v.Size < ActiveParameters.kickAcuercy)
                            {
                                kick = Program.MaxKickSpeed;
                                isChip = false;
                                strState += " AccuratedKick";
                            }
                        }

                    }
                    #endregion

                    #region No way to kick
                    else
                    {
                        //chk is there any robot in front
                        Vector2D leftVec = (GameParameters.OppGoalLeft + new Vector2D(0, -2.5)) - ball.Location;
                        Vector2D rightVec = (GameParameters.OppGoalRight + new Vector2D(0, 2.5)) - ball.Location;
                        bool clear = true;
                        CurrentState = (int)ActiveRoleState.Clear;
                        bool confilicted = false;
                        foreach (var item in Model.Opponents.Keys)
                        {
                            SingleObjectState oppRobot = Model.Opponents[item];
                            Vector2D v = oppRobot.Location - ball.Location;
                            if (v.Size < ActiveParameters.clearRobotZone && Math.Abs(v.AngleInRadians) > Math.PI / 2)
                            {
                                clear = false;
                                if (v.Size < ActiveParameters.ConfilictZone)
                                {
                                    confilicted = true;
                                    CurrentState = (int)ActiveRoleState.Conflict;
                                }
                            }
                        }

                        if (ball.Location.X < ActiveParameters.kickAnyWayRegion)
                        {
                            Target = GameParameters.OppGoalCenter;
                            kick = Program.MaxKickSpeed;
                            isChip = false;
                            strState += " kickAnyWay";

                        }
                        else
                        {
                            #region It is clear No robot in front BUT goal is close
                            if (clear)
                            {
                                if (ActiveParameters.playMode == ActiveParameters.PlayMode.Chip)
                                {
                                    kick = (GameParameters.OppGoalCenter - ball.Location).GetNormalizeToCopy((GameParameters.OppGoalCenter - ball.Location).Size - 0.2).Size;
                                    isChip = true;
                                    Target = GameParameters.OppGoalCenter;
                                    strState += " Clear Chip";
                                }
                                else if (ActiveParameters.playMode == ActiveParameters.PlayMode.Direct || (ActiveParameters.playMode == ActiveParameters.PlayMode.Pass && (!PassPoint.HasValue || (attackerID.HasValue && Model.OurRobots.ContainsKey(attackerID.Value) && Model.OurRobots[attackerID.Value].Location.DistanceFrom(PassPoint.Value) > 0.65))))
                                {
                                    Obstacles obs = new Obstacles(Model);
                                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { RobotID }, Model.Opponents.Keys.ToList());
                                    Vector2D v = Vector2D.Zero;
                                    if (ActiveParameters.kickDefult == ActiveParameters.KickDefult.Center)
                                    {
                                        Target = GameParameters.OppGoalCenter;
                                        v = (Target - Model.BallState.Location).GetNormalizeToCopy(3);
                                        bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
                                        kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(Target) * 0.7;
                                        isChip = b;
                                        strState += " Clear Direct Center";
                                    }
                                    else
                                    {
                                        Target = (ball.Location.Y < 0 ? new Vector2D(0, -0.25) : new Vector2D(0, 0.25)) + GameParameters.OppGoalCenter;
                                        v = (Target - Model.BallState.Location).GetNormalizeToCopy(3);
                                        bool b = obs.Meet(Model.BallState, new SingleObjectState(Model.BallState.Location + v, Vector2D.Zero, 0), 0.03);
                                        kick = (!b) ? Program.MaxKickSpeed : Model.BallState.Location.DistanceFrom(Target) * 0.7;
                                        isChip = b;
                                        strState += " Clear Direct Rear";
                                    }

                                }
                                else if (ActiveParameters.playMode == ActiveParameters.PlayMode.Force)
                                {
                                    Target = GameParameters.OppGoalCenter;
                                    kick = 0.2;
                                    isChip = true;
                                    strState += " Clear Force";
                                }
                                else
                                {
                                    Target = PassPoint.Value;
                                    double ang = Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * Math.PI / 180, 1), PassPoint.Value - Model.BallState.Location));
                                    bool open2kick = false;
                                    if (ang < 5)
                                    {
                                        open2kick = true;
                                    }
                                    if (!open2kick)
                                    {
                                        kick = 0;
                                        isChip = false;
                                        strState += "Clear Accurate Pass";
                                    }
                                    else if (PassIsChip)
                                    {
                                        kick = ball.Location.DistanceFrom(PassPoint.Value) / 2;
                                        isChip = true;
                                        strState += " Clear Chip Pass";
                                    }
                                    else
                                    {
                                        kick = (PassPoint.HasValue) ? /*engine.GameInfo.CalculateKickSpeed(Model, RobotID, Model.BallState.Location, PassPoint.Value, false, true) * 0.9*/4.2 : 3;
                                        isChip = false;
                                        strState += " Clear Direct Pass";
                                    }

                                }
                            }
                            #endregion
                            else
                            #region Enemy is near we dont have space to move
                            {

                                #region They are too too too close
                                if (confilicted)
                                {
                                    if (ball.Location.X < -1.2)
                                    {
                                        Target = GameParameters.OppGoalCenter;
                                    }
                                    else if (ball.Location.X >= -1.5 && ball.Location.X < 0.5)
                                    {
                                        Target = ball.Location + new Vector2D(-1, 0);
                                    }
                                    else
                                    {
                                        Target = ball.Location + (ball.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1);
                                    }
                                    if (ActiveParameters.conflictMode == ActiveParameters.ConflictMode.Direct)
                                    {

                                        isChip = false;
                                        kick = 0.5;
                                        strState += " Confilcted Direct";
                                    }
                                    else
                                    {
                                        isChip = false;
                                        kick = 0;
                                        strState += " Confilcted STOP";
                                    }
                                }
                                #endregion
                                else
                                {
                                    CurrentState = (int)ActiveRoleState.LittleSpace;
                                    #region We have just liitle space
                                    if (ActiveParameters.clearMode == ActiveParameters.NonClearMode.Chip)
                                    {
                                        Target = GameParameters.OppGoalCenter;
                                        kick = (GameParameters.OppGoalCenter - ball.Location).GetNormalizeToCopy((GameParameters.OppGoalCenter - ball.Location).Size - 0.8).Size;
                                        isChip = true;
                                        strState += " Little Space Chip";
                                    }
                                    else
                                    {
                                        Target = GameParameters.OppGoalCenter;
                                        kick = Program.MaxKickSpeed;
                                        isChip = false;
                                        strState += " Little Space Direct";
                                    }
                                    #endregion

                                }
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
            }
            Color color = color = Color.LightPink;
            if (isChip && kick > 0)
            {
                color = Color.SkyBlue;
            }
            if (!isChip && kick > 0)
            {
                color = Color.YellowGreen;
            }
            DrawingObjects.AddObject(new Line(ball.Location, Target, new System.Drawing.Pen(color, 0.043f)));
            DrawingObjects.AddObject(new StringDraw(strState, GameParameters.OppGoalCenter + new Vector2D(-0.4, 0)));
            GetSkill<GetBallSkill>().SetAvoidDangerZone(true, true);
            GetSkill<GetBallSkill>().PerformForStrategy(engine, Model, RobotID, Target);

            CurrentState = (int)GetSkill<GetBallSkill>().CurrState;
            OneTouch = GetSkill<GetBallSkill>().InNear;
            IncomPred = GetSkill<GetBallSkill>().IncommingPred;
            bool spin = false;
            //if (!GetSkill<GetBallSkill>().FarFromBack)
            //    spin = true;
            Planner.AddKick(RobotID, kickPowerType.Speed, kick, isChip, spin);
        }

        Position2D? lastTargetPoint = null;
        Position2D? GoodPointInGoal = null;
        double goodness = -1;
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            SingleObjectState ball = Model.BallState;
            GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, lastTargetPoint, ball.Location, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, true, null, new int[1] { RobotID });
            lastTargetPoint = GoodPointInGoal;
            double minDist = double.MaxValue;
            int minIndx = -1;
            foreach (var item in Model.Opponents.Keys)
            {
                Vector2D v = Model.Opponents[item].Location - ball.Location;
                if (v.Size < minDist && Math.Abs(v.AngleInRadians) > Math.PI / 2)
                {
                    minDist = v.Size;
                    minIndx = item;
                }
            }
            if (minIndx != -1)
            {
                Obstacle obs = new Obstacle();
                obs.R = new Vector2D(RobotParameters.OurRobotParams.Diameter / 2, RobotParameters.OurRobotParams.Diameter / 2);
                obs.Type = ObstacleType.OppRobot;
                obs.State = Model.Opponents[minIndx];
                if (obs.Meet(Model.BallState, MotionPlannerParameters.BallRadi))
                    goodness = 0;
            }

        }
        public void PerformWithoutKick(GameStrategyEngine engine, WorldModel Model, int RobotID, Position2D Target, bool useDefaultDist = true, double behindBallDist = 0.09)
        {
            GetSkill<GetBallSkill>().Perform(engine, Model, RobotID, Target, useDefaultDist, behindBallDist);
        }
        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {

            double failMargin = 0.1, maxFaildConst = 90, epsilon = 0.1;

            if (engine.GameInfo.OurTeam.CatchBallLines.ContainsKey(RobotID) &&
                engine.GameInfo.OurTeam.CatchBallLines[RobotID].Count > 0)
            {

                return engine.GameInfo.OurTeam.CatchBallLines[RobotID].First().Head.DistanceFrom(Model.OurRobots[RobotID].Location);
            }
            double d = (double)MRL.SSL.Planning.GamePlanner.BallState.maxLengh / 100.0 + failMargin;

            Vector2D ballSpeed = Model.BallState.Speed;

            Vector2D v = GameParameters.InRefrence(Model.OurRobots[RobotID].Location - Model.BallState.Location, ballSpeed);
            if (v.Y > epsilon)
            {
                d += Math.Abs(v.X) / v.Y;
            }
            else
            {
                d += maxFaildConst;
                d += Model.BallState.Location.DistanceFrom(Model.OurRobots[RobotID].Location);
            }

            return d;

        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() {
                new NewActiveRole(),            new ActiveRole(), new SupporterRole(), new AttackerRole(),
            new NewSupporterRole(), new DefenderMarkerNormalRole1(),
            new StaticDefender1(),new StaticDefender2()};
            if (FreekickDefence.SwitchToActiveMarker1)
            {
                res.Add(new DefenderMarkerRole());
            }
            if (FreekickDefence.SwitchToActiveMarker2)
            {
                res.Add(new DefenderMarkerRole2());
            }
            if (FreekickDefence.SwitchToActiveMarker3)
            {
                res.Add(new DefenderMarkerRole3());
            }

            if (FreekickDefence.DefenderCornerRole1ToActive)
            {
                res.Add(new DefenderCornerRole1());
            }

            if (FreekickDefence.DefenderCornerRole2ToActive)
            {
                res.Add(new DefenderCornerRole2());
            }
            if (FreekickDefence.DefenderCornerRole3ToActive)
            {
                res.Add(new DefenderCornerRole3());
            }
            if (FreekickDefence.DefenderCornerRole4ToActive)
            {
                res.Add(new DefenderCornerRole4());
            }
            if (FreekickDefence.DefenderMarkerRole1ToActive)
            {
                res.Add(new NewDefenderMrkerRole());
            }
            if (FreekickDefence.DefenderMarkerRole2ToActive)
            {
                res.Add(new NewDefenderMarkerRole2());
            }
            if (FreekickDefence.DefenderMarkerRole3ToActive)
            {
                res.Add(new NewDefenderMarkerRole3());
            }
            if (FreekickDefence.DefenderRegionalRole1ToActive)
            {
                res.Add(new RegionalDefenderRole());
            }
            if (FreekickDefence.DefenderRegionalRole2ToActive)
            {
                res.Add(new RegionalDefenderRole2());
            }
            if (FreekickDefence.DefenderGoToPointToActive)
            {
                res.Add(new DefendGotoPointRole());
            }
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public bool OneTouch { get; set; }

        public Position2D IncomPred { get; set; }
        public enum ActiveRoleState
        {
            Sweep = 0,
            Open2kick = 1,
            Clear = 2,
            Conflict = 3,
            LittleSpace = 4
        }
    }
}
