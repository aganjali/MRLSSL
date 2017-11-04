using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.Planning.MotionPlanner;

using PickerState = MRL.SSL.AIConsole.Engine.NormalSharedState.PickerState; 

namespace MRL.SSL.AIConsole.Roles
{
    class NewPickerRole:RoleBase
    {
        const double goBlockRadi = 0.4;
        const double kExtendBallPoint = 0;
        Position2D Target = GameParameters.OurGoalCenter;
        int oppID = -1;
        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        public void Perform(GameStrategyEngine engine, WorldModel Model, int RobotID)
        {
            Planner.ChangeDefaulteParams(RobotID, false);
            Planner.SetParameter(RobotID, 6, 3.5);
            if (CurrentState == (int)PickerState.GotoPoint)
            {
                GetSkill<PickOppSkill>().Perform(engine, Model, RobotID, NormalSharedState.PickerInfo.TargetPoint, NormalSharedState.PickerInfo.PickDistance); 
            }
            else if (CurrentState == (int)PickerState.Block)
            {
                GetSkill<PickOppSkill>().Perform(engine, Model, RobotID, NormalSharedState.PickerInfo.TargetPoint, NormalSharedState.PickerInfo.PickDistance); 
            }
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {
           // CurrentState = (int)PickerState.Block;
            if (CurrentState ==(int) PickerState.GotoPoint)
            {
                CurrentState = (int)PickerState.Block;
                Circle c = new Circle(Model.BallState.Location + kExtendBallPoint * Model.BallState.Speed, goBlockRadi);
                if (c.IsInCircle(Model.OurRobots[RobotID].Location))
                {
                    CurrentState = (int)PickerState.Block;
                }
            }
            else if (CurrentState == (int)PickerState.Block)
            {
                double margin = 0.2;
                Circle c = new Circle(Model.BallState.Location + kExtendBallPoint * Model.BallState.Speed, goBlockRadi + margin);
                if (!c.IsInCircle(Model.OurRobots[RobotID].Location) && oppID != -1)
                {
                    CurrentState = (int)PickerState.GotoPoint;
                }
            } 
            CurrentState = (int)PickerState.Block;
            oppID = FindNearestOpp(engine, Model);
            if (CurrentState == (int)PickerState.GotoPoint)
            {
                Target = GameParameters.OurGoalCenter;//GetPickPoint(Model, RobotID, 0);
            }
            else if (CurrentState == (int)PickerState.Block)
            {
                if (oppID != -1)
                {
                    //Position2D extendedballPoint = (Model.BallState.Location + kExtendBallPoint * Model.BallState.Speed);
                    //Target = extendedballPoint + (Model.Opponents[oppID].Location - extendedballPoint).GetNormalizeToCopy(NormalSharedState.PickerInfo.PickDistance);
                    Target = Model.BallState.Location + (Model.BallState.Location - Model.Opponents[oppID].Location).GetNormalizeToCopy(1);
                }
                else
                    Target = GameParameters.OurGoalCenter;//GetPickPoint(Model, RobotID, 0);
            }
            NormalSharedState.PickerInfo.CurrentState = (PickerState)CurrentState;
            NormalSharedState.PickerInfo.TargetPoint = Target;
        }

        private static int FindNearestOpp(GameStrategyEngine engine, WorldModel Model)
        {
            int minID = -1;
            double minDist = double.MaxValue;
            Circle c = new Circle(Model.BallState.Location + kExtendBallPoint * Model.BallState.Speed, 1);
            foreach (var item in Model.Opponents.Keys)
            {
                if (engine.GameInfo.OppTeam.GoaliID.HasValue && item == engine.GameInfo.OppTeam.GoaliID.Value)
                    continue;
                if (c.IsInCircle(Model.Opponents[item].Location) && Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                {
                    minDist = Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location);
                    minID = item;
                }
            }
            return minID;
        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return GetPickPoint(Model, RobotID, 0).DistanceFrom(Model.OurRobots[RobotID].Location);
        }

        private static Position2D GetPickPoint(WorldModel Model, int RobotID, double margin)
        {
            return (Model.BallState.Location + (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(NormalSharedState.PickerInfo.PickDistance + margin));
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            List<RoleBase> res = new List<RoleBase>() { new NewPickerRole() };
            if (NormalSharedState.CommonInfo.PickIsFeasible && !NormalSharedState.CommonInfo.IsPicking)
            {
                if (NormalSharedState.CommonInfo.PickerID == NormalSharedState.CommonInfo.ActiveID)
                    res.Add(new NewActiveRole());

                if (NormalSharedState.CommonInfo.PickerID == NormalSharedState.CommonInfo.SupporterID)
                    res.Add(new NewSupporter2Role());
            }
            else if (!NormalSharedState.CommonInfo.PickIsFeasible && NormalSharedState.CommonInfo.IsPicking)
            {
                res.Add(new NewActiveRole());
                res.Add(new NewSupporter2Role());
            }
            return res;
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }
    }
}
