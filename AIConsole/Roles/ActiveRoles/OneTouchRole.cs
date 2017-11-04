using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public class OneTouchRole : RoleBase
    {
        public OneTouchRole()
        {

        }
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }

        Position2D FinalTarget = new Position2D();
        bool first = false;
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
            var lastState = CurrentState;
            if (Model.BallState.Speed.Size > 0.05)
                CurrentState = (int)CutBallStatus.Go;
            else
                CurrentState = (int)CutBallStatus.Wait;
            if (lastState != CurrentState)
            {
                GetSkill<OneTouchSkill>().Reset();
            }

            if (Model.BallState.Speed.Y >= 0)
                FinalTarget = GameParameters.OppGoalLeft;
            else
            {
                FinalTarget = GameParameters.OppGoalRight;
            }

        }
        bool incomningNear = false;

        public void PerformActive(GameStrategyEngine engine, WorldModel Model, int RobotID,SingleObjectState RobotState, SingleObjectState passerState, bool passIsChip, Position2D Target, double KickPower, bool isChip)
        {
            if (CurrentState == (int)CutBallStatus.Go)
            {
                GetSkill<OneTouchSkill>().PerformActive(engine, Model, RobotID, RobotState, passerState, passIsChip, Target, KickPower, isChip);
            }
            else
            {
                GetSkill<GotoPointSkill>().GotoPoint(Model, RobotID, Model.OurRobots[RobotID].Location, 0, true, true);
            }
        }

        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState passerState, bool passIsChip, Position2D Target, double KickPower, bool isChip, double PassSpeed = 0)
        {

            //         DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, 0.2, new System.Drawing.Pen(System.Drawing.Color.Crimson, 0.01f)), "robotlocotFromRole");
            GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, passerState, passIsChip, Target, KickPower, isChip, PassSpeed);
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState RobotState, SingleObjectState passerState, bool passIsChip, Position2D Target, double KickPower, bool isChip, bool GotoPoint, double PassSpeed = 0)
        {
            GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, RobotState, passerState, passIsChip, Target, KickPower, isChip, GotoPoint, PassSpeed);
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, bool avoidRobots, SingleObjectState RobotState, SingleObjectState passerState, bool passIsChip, Position2D Target, double KickPower, bool isChip, double AngleTresh, bool GotoPoint, double PassSpeed = 0)
        {
            GetSkill<OneTouchSkill>().Perform(engine, Model, RobotID, avoidRobots, RobotState, passerState, passIsChip, Target, KickPower, isChip, GotoPoint, AngleTresh, PassSpeed);
        }

        /// <summary>
        /// Specially Tune Method
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="Model"></param>
        /// <param name="RobotID"></param>
        /// <param name="passerState"></param>
        /// <param name="passIsChip"></param>
        /// <param name="Target"></param>
        /// <param name="KickPower"></param>
        /// <param name="isChip"></param>
        /// <param name="Correction"></param>
        /// <param name="beta"></param>
        /// <param name="lambda"></param>
        /// <param name="PassSpeed"></param>
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID, SingleObjectState passerState, bool passIsChip, Position2D Target, double KickPower, bool isChip, bool Correction, double beta, double lambda, double PassSpeed = 0)
        {

            //         DrawingObjects.AddObject(new Circle(Model.OurRobots[RobotID].Location, 0.2, new System.Drawing.Pen(System.Drawing.Color.Crimson, 0.01f)), "robotlocotFromRole");
            if (!Correction)
                GetSkill<OneTouchSkill>().PerformWithoutCorrection(engine, Model, RobotID, passerState, passIsChip, Target, KickPower, isChip, PassSpeed);
            if (Correction)
                GetSkill<OneTouchSkill>().PerformManualcoefs(engine, Model, RobotID, passerState, passIsChip, Target, KickPower, beta, lambda, isChip, PassSpeed);
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

    }
}
