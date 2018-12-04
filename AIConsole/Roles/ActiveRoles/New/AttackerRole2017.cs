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
using AttackerState = MRL.SSL.AIConsole.Engine.NormalSharedState.AttacekrState;

namespace MRL.SSL.AIConsole.Roles
{
    public class AttackerRole2017 : RoleBase
    {
        bool Debug = true;
        const double oppDefenseMargin = 0.8;

        int? oppMarkId = null;
        const double goTowardBallMarg = 0.1;
        Position2D markPoint;
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
        Position2D? lastBall = null;
        public AttackerRole2017()
        {
            lastBall = null;
        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        bool findPos(GameStrategyEngine engine, WorldModel Model, int RobotId, Position2D attackerPos)
        {
            if (lastBall.HasValue && Model.BallState.Location.DistanceFrom(lastBall.Value) > 0.6
                )
                lastBall = null;
            if (lastBall.HasValue)
            {
                //return false;
            }



            List<PassPointData> poses = new List<PassPointData>();
            #region calculate First attacker
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
            poses = engine.GameInfo.CalculatePassScore(Model, NormalSharedState.CommonInfo.ActiveID.Value, NormalSharedState.CommonInfo.AttackerID, topLeft, passSpeed, shootSpeed, width, heigth, Rows, column);
            double mScore = double.MinValue;
            int sgn = 0;
            foreach (var item in poses)
            {
                if (item.score > mScore)
                {
                    mScore = item.score;
                    sgn = Math.Sign(item.pos.Y);
                }
            }
            if (NormalSharedState.ActiveInfo.CurrentAction == NormalSharedState.ActiveActionMode.Pass)
            {
                sgn = Math.Sign(NormalSharedState.CommonInfo.PassTarget.Y);
            }
            #endregion 

            topLeft = new Position2D(regionX, sgn < 0 ? 0 : GameParameters.OurRightCorner.Y);
            width = (GameParameters.OurGoalCenter.X - 0.5 - 0.25) ;
            heigth = GameParameters.OurLeftCorner.Y;

            poses = engine.GameInfo.CalculateAttackerPassScore(Model, NormalSharedState.CommonInfo.ActiveID.Value, RobotId/*, attackerPos*/, topLeft, passSpeed, shootSpeed, width, heigth, Rows, column);

            double maxSc = double.MinValue;
            foreach (var item in poses)
            {
                if (item.score > maxSc)
                {
                    maxSc = item.score;
                    markPoint = item.pos;
                }
            }
            markPoint.DrawColor = Color.DarkGreen;
            DrawingObjects.AddObject(markPoint);
            lastBall = Model.BallState.Location;
            return true;
        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            Position2D? attackerPos = null;
            foreach (var item in AssignedRoles)
            {
                if (item.Value.GetType() == typeof(NewAttackerRole))
                {
                    NewAttackerRole role = (NewAttackerRole)item.Value;
                    attackerPos = role.markPoint;
                }
            }
            if (attackerPos.HasValue)
            {

                findPos(engine, Model, RobotID, attackerPos.Value);
            }
            else
                markPoint = new Position2D();
            
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
            //double dist, DistFromBorder;
            //if (GameParameters.IsInDangerousZone(markPoint, true, 0.1, out dist, out DistFromBorder))
            //{
            //    markPoint = GameParameters.OppGoalCenter + (markPoint - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-markPoint, Vector2D.Zero, 0), 0));
            //}

            //if (CurrentState == (int)AttackerState.WaitForPass)
            //{
            Planner.Add(RobotID, markPoint, (Model.BallState.Location - markPoint).AngleInDegrees, PathType.UnSafe, true, true, true, true);
            //}
            //else if (CurrentState == (int)AttackerState.MarkGotoPoint)
            //{
            //    Planner.Add(RobotID, markPoint, (Model.BallState.Location - markPoint).AngleInDegrees, PathType.UnSafe, true, true, true, false);
            //}
            //else if (CurrentState == (int)AttackerState.MarkGoTowardBall)
            //{
            //    Planner.Add(RobotID, markPoint, (Model.BallState.Location - markPoint).AngleInDegrees, PathType.UnSafe, true, true, true, false);
            //}
            //else if (CurrentState == (int)AttackerState.CatchPass)
            //{
            //    if (gogetPass)
            //    {
            //        Position2D Pos2go = Position2D.Zero;
            //        if (pKind == NormalSharedState.ActivePassKind.OneTouch)
            //        {
            //            Pos2go = (markPoint - shootTarget).GetNormalizeToCopy(distBehindBallTresh) + markPoint;
            //        }
            //        else
            //        {
            //            Pos2go = markPoint;
            //        }

            //        if (GameParameters.IsInDangerousZone(Pos2go, true, 0.1, out dist, out DistFromBorder))
            //        {
            //            Pos2go = GameParameters.OppGoalCenter + (Pos2go - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-Pos2go, Vector2D.Zero, 0), 0));
            //        }
            //        if (gogetPass)
            //        {
            //            int? ActiveID = NormalSharedState.CommonInfo.ActiveID;

            //            if (pKind == NormalSharedState.ActivePassKind.OneTouch)
            //            {
            //                GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, new SingleObjectState(Pos2go, Vector2D.Zero, 0),
            //                       (ActiveID.HasValue && Model.OurRobots.ContainsKey(ActiveID.Value)) ? Model.OurRobots[ActiveID.Value] : null, passIsChip, shootTarget, ShootSpeed,
            //                       false, gotoPoint, PassSpeed);
            //            }
            //            else
            //            {
            //                GetSkill<CatchBallSkill>().Catch(engine, Model, RobotID, passIsChip, new SingleObjectState(Pos2go, Vector2D.Zero, 0), true, gotoPoint);
            //            }

            //        }
            //        if (!gotoPoint || !gogetPass)
            //        {
            //            double teta = (Model.BallState.Location - markPoint).AngleInDegrees;
            //            if (pKind == NormalSharedState.ActivePassKind.OneTouch)
            //                teta = (shootTarget - Pos2go).AngleInDegrees;
            //            Planner.Add(RobotID, Pos2go, teta, PathType.UnSafe, true, true, true, false);
            //        }

            //    }
            //    else
            //    {
            //        double teta = (Model.BallState.Location - markPoint).AngleInDegrees;
            //        if (pKind == NormalSharedState.ActivePassKind.OneTouch)
            //            teta = (shootTarget - markPoint).AngleInDegrees;
            //        Planner.Add(RobotID, markPoint, teta, PathType.UnSafe, true, true, true, false);
            //    }
            //}

        }
        public override double CalculateCost(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return Model.OurRobots[RobotID].Location.DistanceFrom(markPoint);
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>() { new NewAttackerRole(), new AttackerRole2017(), new ActiveRole2017() };
        }

        public override bool Evaluate(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }

}
