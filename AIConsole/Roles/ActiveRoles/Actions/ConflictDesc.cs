using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NormalSharedState = MRL.SSL.AIConsole.Engine.NormalSharedState;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.Planning.GamePlanner.Types;

namespace MRL.SSL.AIConsole.Roles
{
    public class ConflictDesc : ActionDescriptionBase
    {

        public override NormalSharedState.ActiveActionMode ActionCategory()
        {
            return NormalSharedState.ActiveActionMode.Conflict;
        }

        public override void UpdateLambda()
        {
            lambda = 1 - 0.9 * Math.Exp(-0.6 * frame++);
        }
        public override void DetermineActionState(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            base.DetermineActionState(engine, Model, RobotID, activeRoleState, ref actInfo);
            SingleObjectState ball = Model.BallState;
            if (ActiveParameters.NewActiveParameters.conflictMode == ActiveParameters.NewActiveParameters.ConflictMode.SpinRotate)
            {
                Position2D ballLoc = Model.BallState.Location;
                if (ballLoc.X > 0)
                {
                    actInfo.Target = new Position2D(ballLoc.X - 2, -Math.Sign(ballLoc.Y) * 2.5);
                }
                else
                {
                    actInfo.Target = new Position2D(ballLoc.X + 2, -Math.Sign(ballLoc.Y) * 2.5);
                }
                //
                //if (NormalSharedState.CommonInfo.OppConfID != -1)
                //    actInfo.Target = Model.BallState.Location + (Model.Opponents[NormalSharedState.CommonInfo.OppConfID].Location - Model.BallState.Location).GetNormalizeToCopy(1);
                //else
                //    actInfo.Target = Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1);

                if (NormalSharedState.ActiveInfo.confWaitCounter >= NormalSharedState.ActiveInfo.confMaxWaitTresh)
                    actInfo.kick = engine.GameInfo.CalculateKickSpeed(Model, RobotID, Model.BallState.Location, actInfo.Target, false, false);
                else
                    actInfo.kick = 0;


                actInfo.isChip = false;
                actInfo.strState += "Conflicted Spin Rotate";
            }
            else if (ActiveParameters.NewActiveParameters.conflictMode == ActiveParameters.NewActiveParameters.ConflictMode.Rotate)
            {
                if (NormalSharedState.ActiveInfo.confWaitCounter >= NormalSharedState.ActiveInfo.confMaxWaitTresh)
                    actInfo.kick = 5;
                else
                    actInfo.kick = 0;
                if (NormalSharedState.CommonInfo.OppConfID != -1)
                    actInfo.Target = Model.BallState.Location + (Model.Opponents[NormalSharedState.CommonInfo.OppConfID].Location - Model.BallState.Location).GetNormalizeToCopy(1);
                else
                    actInfo.Target = Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1);
                actInfo.isChip = false;
                actInfo.strState += "Conflicted Rotate";
            }
            else
            {
                if (ball.Location.X < -0.4 * GameParameters.OurGoalCenter.X)
                {
                    actInfo.Target = GameParameters.OppGoalCenter;
                }
                else if (ball.Location.X >= -0.5 * GameParameters.OurGoalCenter.X && ball.Location.X < 0.166 * GameParameters.OurGoalCenter.X)
                {
                    actInfo.Target = ball.Location + new Vector2D(-1, 0);
                }
                else
                {
                    actInfo.Target = ball.Location + (ball.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(1);
                }
                if (ActiveParameters.NewActiveParameters.conflictMode == ActiveParameters.NewActiveParameters.ConflictMode.Direct)
                {
                    actInfo.isChip = false;
                    actInfo.kick = 1;
                    actInfo.strState += " Confilcted Direct";
                }
                else if (ActiveParameters.NewActiveParameters.conflictMode == ActiveParameters.NewActiveParameters.ConflictMode.Stop)
                {
                    actInfo.isChip = false;
                    actInfo.kick = 0;
                    actInfo.strState += " Confilcted Stop";

                }
            }


        }

        public override double Cost(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            if (NormalSharedState.ActiveInfo.isRotateStarted)
            {
                return -10000;
            }
            if (activeRoleState == NormalSharedState.ActiveRoleState.Conflict)
                return 0 + 0.1;
            return double.MaxValue;
        }

        public override void ProvideActionInfo(GameStrategyEngine engine, WorldModel Model, int RobotID, NormalSharedState.ActiveRoleState activeRoleState, ref NormalSharedState.ActionInfo actInfo)
        {
            NormalSharedState.ActiveInfo.spin = false;
            NormalSharedState.ActiveInfo.isChip = actInfo.isChip;
            NormalSharedState.ActiveInfo.kickSpeed = actInfo.kick;
            NormalSharedState.ActiveInfo.KickTarget = NormalSharedState.ActiveInfo.Target = actInfo.Target;
            if (ActiveParameters.NewActiveParameters.conflictMode == ActiveParameters.NewActiveParameters.ConflictMode.SpinRotate)
            {
                NormalSharedState.CommonInfo.PassTarget = actInfo.PassTarget;
            }
        }

        public override void Reset()
        {
            base.Reset();
        }
    }
}
