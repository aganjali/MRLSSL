using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;


namespace MRL.SSL.AIConsole.Plays
{
    class OppNewPenaltyPlay : PlayBase
    {

        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, PlayBase LastPlay, ref GameStatus Status)
        {
            return false;
            return Status == GameStatus.Penalty_Opponent_Go || Status == GameStatus.Penalty_Opponent_Waiting;
        }
        bool activeBool = false;

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            int? st1 = null;
            int? st2 = null;
            int? goalie = Model.GoalieID;
            int? penaltyShooter = null;//Model.OurRobots.Keys.Where(q => q != goalie.Value).ToList().First();
            int? oppShooter = null;
            var minDist = double.MaxValue;
            foreach (var item in Model.Opponents)
            {
                if (item.Value.Location.DistanceFrom(Model.BallState.Location) < minDist)
                {
                    minDist = item.Value.Location.DistanceFrom(Model.BallState.Location);
                    oppShooter = item.Key;
                }
            }
            List<DefenderCommand> defence = new List<DefenderCommand>();
            defence.Add(new DefenderCommand()
            {
                RoleType = typeof(StaticDefender1)
            });
            defence.Add(new DefenderCommand()
            {
                RoleType = typeof(StaticDefender2)
            });

            List<DefenceInfo> list = FreekickDefence.MatchStatic(engine, Model, defence);
            var first = list.First(f => f.RoleType == typeof(StaticDefender1));
            var second = list.First(f => f.RoleType == typeof(StaticDefender2));
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, penaltyShooter, typeof(GotoPointRole)))
                Functions[penaltyShooter.Value] = (eng, wmd) => GetRole<GotoPointRole>(penaltyShooter.Value).GotoPoint(eng, wmd, penaltyShooter.Value, new Position2D(-2, -2), 180, false, true, true, false, 0, false);

            if (Model.Status == GameStatus.Penalty_Opponent_Waiting)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(GotoPointRole)))
                    Functions[goalie.Value] = (eng, wmd) => GetRole<GotoPointRole>(goalie.Value).GotoPoint(eng, wmd, goalie.Value, GameParameters.OurGoalCenter.Extend(-0.09, 0), 180, false, true, true, false, 0, false);
            }
            else
            {
                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(GoalieNormalRole)))
                //    Functions[goalie.Value] = (eng, wmd) => GetRole<GoalieNormalRole>(goalie.Value).Run(engine,Model,Model.GoalieID.Value,new Position2D(5,0),180);
                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(IntelligencePenaltyGoalKeeperRole)))
                //    Functions[goalie.Value] = (eng, wmd) => GetRole<IntelligencePenaltyGoalKeeperRole>(goalie.Value).Run(engine,Model,goalie.Value);

                if ((!BallKickedToOurGoal(Model) &&(oppShooter.HasValue && Model.Opponents[oppShooter.Value].Location.DistanceFrom(Model.BallState.Location) < 0.7 && Model.BallState.Location.X > 3 || oppShooter.HasValue && Model.Opponents[oppShooter.Value].Location.DistanceFrom(Model.BallState.Location) > 1)) || activeBool)
                {
                    activeBool = true;
                    //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(ActiveRole2017)))
                    //    Functions[goalie.Value] = (eng, wmd) => GetRole<ActiveRole2017>(goalie.Value).Perform(engine, Model, goalie.Value, true);
                    //GetBallSkill skill = new GetBallSkill();
                    Vector2D exVec = (Model.BallState.Location - GameParameters.OurGoalCenter).GetNormalizeToCopy(2);
                    //skill.OutGoingSideTrack(Model, goalie.Value, Model.BallState.Location + exVec, false, 0.08);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(ActiveRole2017)))
                        Functions[goalie.Value] = (eng, wmd) => GetRole<ActiveRole2017>(goalie.Value).Perform(engine, Model, goalie.Value, true);
                    DrawingObjects.AddObject(Model.BallState.Location + exVec);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(StaticGoalieRole)))
                        Functions[goalie.Value] = (eng, wmd) => GetRole<StaticGoalieRole>(goalie.Value).perform(eng, wmd, goalie.Value, (st1.HasValue) ? first.TargetState : Model.BallState, st1, st2);
                }
                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(ActiveRole2017)))
                //    Functions[goalie.Value] = (eng, wmd) => GetRole<ActiveRole2017>(goalie.Value).Perform(engine, Model, goalie.Value, true);
                
                
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }
        public bool BallKickedToOurGoal(WorldModel Model)
        {

            double tresh = 0.20;
            double tresh2 = 1.3;

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
        public override void ResetPlay(WorldModel Model, GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();
            activeBool = false;
        }
    }
}
