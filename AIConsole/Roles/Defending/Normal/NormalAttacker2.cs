using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.Planning.GamePlanner.Types;
namespace MRL.SSL.AIConsole.Roles.Defending.Normal
{
    class NormalAttacker2:RoleBase
    {
        //Position2D Target;
        //public override RoleCategory QueryCategory()
        //{
        //    return RoleCategory.Active;
        //}

        //public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        //{
        //    throw new NotImplementedException();
        //}
        //public Position2D CalculateTarget(GameStrategyEngine engin, WorldModel Model, int RobotID)
        //{

        //    return Target;
        //}
        //public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        //{
        //    return Model.OurRobots[RobotID].Location.DistanceFrom(NormalSharedState.AttackerInfo.MarkPoint);
        //}

        //public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        //{
        //    return new List<RoleBase>() { new NewAttackerRole(), new ActiveRole2017() };
        //}

        //public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        //{
        //    throw new NotImplementedException();
        //}
        bool Debug = true;
        const double oppDefenseMargin = 0.8;

        int? oppMarkId = null;
        const double goTowardBallMarg = 0.1;
        public Position2D markPoint;
        const double goTowardBallThresh = 60;
        int goTowardBallCounter = 0;
        const double markDist = 0.3, dangerRadius = 1.5;
        double PassSpeed = 5, ShootSpeed = Program.MaxKickSpeed;
        bool passIsChip = false;
        Position2D shootTarget = GameParameters.OppGoalCenter;
        NormalSharedState.ActivePassKind pKind = NormalSharedState.ActivePassKind.OneTouch;
        Position2D? lastBallPos = null;
        private bool passed = false;
        private bool gogetPass = false;
        Position2D? firstPos = null;
        bool gotoPoint = false;
        const double distOneTouchTresh = 0.8;
        const double distBehindBallTresh = 0.07;
        private bool firstInWait = true;

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        bool MarkID(GameStrategyEngine engine, WorldModel Model, out List<int> ids)
        {
            ids = new List<int>();
            double dist, DistFromBorder;
            List<int> attackers = new List<int>(), dangers = new List<int>();
            foreach (var item in Model.Opponents)
            {
                if (!GameParameters.IsInDangerousZone(item.Value.Location, true, oppDefenseMargin, out dist, out DistFromBorder)
                    && engine.GameInfo.OppTeam.Scores.ContainsKey(item.Key))
                    attackers.Add(item.Key);
            }

            foreach (var item in attackers)
            {
                if (Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) > dangerRadius)
                    dangers.Add(item);
            }
            if (dangers.Count > 0)
            {
                ids = dangers.OrderByDescending(o => engine.GameInfo.OppTeam.Scores[o] * Math.Abs(Model.Opponents[o].Location.Y - Model.BallState.Location.Y)).ToList();

                return true;
            }
            return false;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            List<int> ids;
            if (MarkID(engine, Model, out ids))
                oppMarkId = ids[0];
            else
                oppMarkId = null;
            int i = 0;
            if (Debug)
            {
                foreach (var item in ids)
                {
                    DrawingObjects.AddObject(new Circle(Model.Opponents[item].Location, 0.15, new System.Drawing.Pen(Color.Purple, 0.01f)));
                    DrawingObjects.AddObject(new StringDraw("hi: " + (i++), "drawStrattak" + item, Model.Opponents[item].Location + new Vector2D(0.5, 0.5)));
                }
            }
            //if (CurrentState == (int)AttackerState.WaitForPass)
            //{
            //    if (NormalSharedState.CommonInfo.Ready2Pass && !NormalSharedState.CommonInfo.ActiveIsCatchingPass)
            //        CurrentState = (int)AttackerState.CatchPass;
            //    if (oppMarkId.HasValue)
            //        CurrentState = (int)AttackerState.MarkGotoPoint;
            //}
            //else if (CurrentState == (int)AttackerState.MarkGotoPoint)
            //{
            //    if (NormalSharedState.CommonInfo.Ready2Pass && !NormalSharedState.CommonInfo.ActiveIsCatchingPass)
            //        CurrentState = (int)AttackerState.CatchPass;
            //    if (!oppMarkId.HasValue)
            //        CurrentState = (int)AttackerState.WaitForPass;
            //    else if (Model.OurRobots[RobotID].Location.DistanceFrom(markPoint) < goTowardBallMarg)
            //        CurrentState = (int)AttackerState.MarkGoTowardBall;
            //}
            //else if (CurrentState == (int)AttackerState.MarkGoTowardBall)
            //{
            //    if (NormalSharedState.CommonInfo.Ready2Pass && !NormalSharedState.CommonInfo.ActiveIsCatchingPass)
            //        CurrentState = (int)AttackerState.CatchPass;
            //    if (!oppMarkId.HasValue)
            //        CurrentState = (int)AttackerState.WaitForPass;
            //    else if (NormalSharedState.AttackerInfo.OppMarkID.HasValue && oppMarkId.Value != NormalSharedState.AttackerInfo.OppMarkID.Value)
            //    {
            //        CurrentState = (int)AttackerState.MarkGotoPoint;
            //    }
            //}
            //else if (CurrentState == (int)AttackerState.CatchPass)
            //{
            //    if (!NormalSharedState.CommonInfo.Ready2Pass)
            //    {
            //        if (!oppMarkId.HasValue)
            //            CurrentState = (int)AttackerState.WaitForPass;
            //        else
            //            CurrentState = (int)AttackerState.MarkGotoPoint;
            //    }

            //}
            if (!oppMarkId.HasValue)
                CurrentState = (int)AttackerState.WaitForPass;
            if (CurrentState == (int)AttackerState.WaitForPass)
            {
                //if (Model.BallState.Location.Y > 0.5)
                //    markPoint = Model.BallState.Location + new Vector2D(-0.3, -4.5);
                //else if (Model.BallState.Location.Y < -0.5)
                //    markPoint = Model.BallState.Location + new Vector2D(-0.3, 4.5);
                //else if (firstInWait)
                //{
                //    markPoint = Model.BallState.Location + new Vector2D(-0.3, 2.5);
                //}
                //if (!GameParameters.IsInField(markPoint, -0.1))
                //{
                //    Position2D tmpInt = new Position2D();
                //    double minDist = double.MaxValue;
                //    Position2D NearestIntersect = new Position2D();
                //    List<Line> field = GameParameters.GetFieldLines(-0.1);
                //    Line BallLine = new Line(Model.BallState.Location, markPoint);
                //    foreach (var item in field)
                //    {
                //        if (!item.IntersectWithLine(BallLine, ref tmpInt))
                //            tmpInt = Model.BallState.Location;
                //        if ((tmpInt - Model.BallState.Location).InnerProduct(BallLine.Tail - BallLine.Head) > 0)
                //        {
                //            double dist = Model.BallState.Location.DistanceFrom(tmpInt);
                //            if (dist < minDist)
                //            {
                //                NearestIntersect = tmpInt;
                //                minDist = dist;
                //            }
                //        }
                //    }
                //    if (minDist < double.MaxValue)
                //    {
                //        markPoint = NearestIntersect;
                //    }
                //}
                //firstInWait = false;

                Position2D topLeft = new Position2D(-0.5, GameParameters.OurRightCorner.Y);
                double width = GameParameters.OurGoalCenter.X - 0.5 - 0.25, heigth = 2 * GameParameters.OurLeftCorner.Y, passSpeed = 4, shootSpeed = 8;
                int Rows = 5, column = 10;
                //List<PassPointData> poses = engine.GameInfo.CalculatePassScore(Model, NormalSharedState.CommonInfo.ActiveID.Value, RobotID, topLeft, passSpeed, shootSpeed, width, heigth, Rows, column);
                //double maxSc = double.MinValue;
                //foreach (var item in poses)
                //{
                //    if (item.score > maxSc)
                //    {
                //        maxSc = item.score;
                //        markPoint = item.pos;
                //    }
                //}
                if (Model.BallState.Location.Y>0)
                {
                markPoint =new Position2D (-2.5,-2.5);
                }
                if (Model.BallState.Location.Y < 0)
                {
                    markPoint = new Position2D(-2.5, 2.5);
                }
            }
            //else if (CurrentState == (int)AttackerState.MarkGotoPoint)
            //{
            //    Vector2D ballOppVec = (Model.BallState.Location - Model.Opponents[oppMarkId.Value].Location);
            //    Position2D oppBallMid = Model.Opponents[oppMarkId.Value].Location + ballOppVec * 0.5;
            //    markPoint = ballOppVec.PrependecularPoint(Model.Opponents[oppMarkId.Value].Location, Model.OurRobots[RobotID].Location);
            //    if ((oppBallMid - markPoint).InnerProduct(oppBallMid - Model.Opponents[oppMarkId.Value].Location) < 0)
            //        markPoint = oppBallMid;
            //    Position2D oppExtendedLoc = Model.Opponents[oppMarkId.Value].Location + (Model.BallState.Location - Model.Opponents[oppMarkId.Value].Location).GetNormalizeToCopy(markDist);
            //    if ((Model.BallState.Location - markPoint).InnerProduct(oppExtendedLoc - markPoint) >= 0)
            //        markPoint = oppExtendedLoc;
            //}
            //else if (CurrentState == (int)AttackerState.MarkGoTowardBall)
            //{
            //    Vector2D ballOppVec = (Model.BallState.Location - Model.Opponents[oppMarkId.Value].Location);
            //    Position2D p = Model.Opponents[oppMarkId.Value].Location + ballOppVec * 0.1;
            //    Vector2D ourPVec = p - Model.OurRobots[RobotID].Location;
            //    markPoint = ourPVec * (goTowardBallCounter++) / (goTowardBallThresh) + Model.OurRobots[RobotID].Location;
            //    if (goTowardBallCounter > goTowardBallThresh)
            //        goTowardBallCounter = (int)goTowardBallThresh;
            //}
            //else if (CurrentState == (int)AttackerState.CatchPass)
            //{
            //    if (!firstPos.HasValue)
            //        firstPos = Model.OurRobots[RobotID].Location;
            //    markPoint = NormalSharedState.CommonInfo.PassTarget;
            //    passIsChip = NormalSharedState.CommonInfo.PassIsChip;
            //    PassSpeed = NormalSharedState.CommonInfo.PassSpeed;
            //    pKind = NormalSharedState.CommonInfo.PassKind;
            //    shootTarget = NormalSharedState.CommonInfo.ShootTarget;
            //    ShootSpeed = NormalSharedState.CommonInfo.ShootSpeed;
            //    if (NormalSharedState.CommonInfo.Passed)
            //        passed = true;
            //    if (NormalSharedState.CommonInfo.Passed && Model.BallState.Speed.Size > 0.3 || (passed && Model.BallState.Speed.Size > 2))
            //        gogetPass = true;
            //    else
            //        gogetPass = false;

            //    if (gotoPoint || gogetPass)
            //    {
            //        if (Model.OurRobots[RobotID].Location.DistanceFrom(markPoint) < distOneTouchTresh)
            //            gotoPoint = true;
            //    }
            //    else
            //    {
            //        if (firstPos.HasValue && firstPos.Value.DistanceFrom(markPoint) < distOneTouchTresh)
            //            gotoPoint = true;
            //    }
            //}

            //if (CurrentState != (int)AttackerState.MarkGoTowardBall)
            //    ResetTowardState();
            //if (CurrentState != (int)AttackerState.MarkGotoPoint)
            //    ResetMarkState();
            //if (CurrentState != (int)AttackerState.WaitForPass)
            //    ResetPassWaitState();
            //if (CurrentState != (int)AttackerState.CatchPass)
            //    ResetCatchPass();

            //NormalSharedState.AttackerInfo.MarkPoint = markPoint;
            //NormalSharedState.AttackerInfo.OppMarkID = oppMarkId;
            //NormalSharedState.AttackerInfo.CurrentState = (AttackerState)CurrentState;
        }

        private void ResetPassWaitState()
        {
            lastBallPos = null;
            firstInWait = true;
        }
        private void ResetCatchPass()
        {
            PassSpeed = 5;
            ShootSpeed = Program.MaxKickSpeed;
            passIsChip = false;
            shootTarget = GameParameters.OppGoalCenter;
            pKind = NormalSharedState.ActivePassKind.OneTouch;
            passed = false;
            gogetPass = false;
            gotoPoint = false;
            firstPos = null;
        }
        private void ResetMarkState()
        {
            //   throw new NotImplementedException();
        }
        private void ResetTowardState()
        {
            goTowardBallCounter = 0;
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            double dist, DistFromBorder;
            if (GameParameters.IsInDangerousZone(markPoint, true, 0.1, out dist, out DistFromBorder))
            {
                markPoint = GameParameters.OppGoalCenter + (markPoint - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-markPoint, Vector2D.Zero, 0), 0));
            }

            if (CurrentState == (int)AttackerState.WaitForPass)
            {
                Planner.Add(RobotID, markPoint, (Model.BallState.Location - markPoint).AngleInDegrees, PathType.UnSafe, true, true, true, false);
            }
            else if (CurrentState == (int)AttackerState.MarkGotoPoint)
            {
                Planner.Add(RobotID, markPoint, (Model.BallState.Location - markPoint).AngleInDegrees, PathType.UnSafe, true, true, true, false);
            }
            else if (CurrentState == (int)AttackerState.MarkGoTowardBall)
            {
                Planner.Add(RobotID, markPoint, (Model.BallState.Location - markPoint).AngleInDegrees, PathType.UnSafe, true, true, true, false);
            }
            else if (CurrentState == (int)AttackerState.CatchPass)
            {
                if (gogetPass)
                {
                    Position2D Pos2go = Position2D.Zero;
                    if (pKind == NormalSharedState.ActivePassKind.OneTouch)
                    {
                        Pos2go = (markPoint - shootTarget).GetNormalizeToCopy(distBehindBallTresh) + markPoint;
                    }
                    else
                    {
                        Pos2go = markPoint;
                    }

                    if (GameParameters.IsInDangerousZone(Pos2go, true, 0.1, out dist, out DistFromBorder))
                    {
                        Pos2go = GameParameters.OppGoalCenter + (Pos2go - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-Pos2go, Vector2D.Zero, 0), 0));
                    }
                    if (gogetPass)
                    {
                        int? ActiveID = NormalSharedState.CommonInfo.ActiveID;

                        if (pKind == NormalSharedState.ActivePassKind.OneTouch)
                        {
                            GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, new SingleObjectState(Pos2go, Vector2D.Zero, 0),
                                   (ActiveID.HasValue && Model.OurRobots.ContainsKey(ActiveID.Value)) ? Model.OurRobots[ActiveID.Value] : null, passIsChip, shootTarget, ShootSpeed,
                                   false, gotoPoint, PassSpeed);
                        }
                        else
                        {
                            GetSkill<CatchBallSkill>().Catch(engine, Model, RobotID, passIsChip, new SingleObjectState(Pos2go, Vector2D.Zero, 0), true, gotoPoint);
                        }

                    }
                    if (!gotoPoint || !gogetPass)
                    {
                        double teta = (Model.BallState.Location - markPoint).AngleInDegrees;
                        if (pKind == NormalSharedState.ActivePassKind.OneTouch)
                            teta = (shootTarget - Pos2go).AngleInDegrees;
                        Planner.Add(RobotID, Pos2go, teta, PathType.UnSafe, true, true, true, false);
                    }

                }
                else
                {
                    double teta = (Model.BallState.Location - markPoint).AngleInDegrees;
                    if (pKind == NormalSharedState.ActivePassKind.OneTouch)
                        teta = (shootTarget - markPoint).AngleInDegrees;
                    Planner.Add(RobotID, markPoint, teta, PathType.UnSafe, true, true, true, false);
                }
            }

        }
        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.OurRobots[RobotID].Location.DistanceFrom(NormalSharedState.AttackerInfo.MarkPoint);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new NormalAttacker1(), new NormalAttacker2(), new ActiveRole2017()/*, new AttackerRole2017() */};
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
        enum AttackerState
        {
            WaitForPass,
            CatchPass,
            MarkGoTowardBall,
            MarkGotoPoint,

        }
    }
}
