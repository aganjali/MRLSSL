using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.GameDefinitions;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;
namespace MRL.SSL.AIConsole.Plays.Opp
{
    class OurBallPlacePlay : PlayBase
    {

        int? placerID = null;
        int? catcherID = null;
        bool firstFlag = true;
        Circle ballToAvoid = new Circle();
        Circle targetToAvoid = new Circle();
        Line lineToAvoid = new Line();
        Line line1 = new Line();
        Line line2 = new Line();
        Vector2D exLine = new Vector2D();
        Position2D pointHead = new Position2D();
        Position2D pointTail = new Position2D();
        Dictionary<int, Position2D> noneRobotTargets = new Dictionary<int, Position2D>();
        List<Position2D> ballConf = new List<Position2D>();
        List<Position2D> targetConf = new List<Position2D>();
        Position2D firstBallPos = new Position2D();
        bool catchBool = true;
        double speedTresh = 0.20;
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, PlayBase LastPlay, ref GameDefinitions.GameStatus Status)
        {
            //return false;
            if (Status == GameDefinitions.GameStatus.BallPlace_OurTeam)
                return true;
            firstFlag = true;
            return false;
        }

        public override Dictionary<int, RoleBase> RunPlay(GameStrategyEngine engine, GameDefinitions.WorldModel Model, bool RecalculateRoles, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>(Model.OurRobots.Count);
            Functions = new Dictionary<int, CommonDelegate>();
            if (Model.GoalieID.HasValue)
            {
                //Planner.Add();

            }

           // PreviouslyAssignedRoles = CurrentlyAssignedRoles;

            #region Matcher
            RoleBase r;
            roles = new List<RoleInfo>();
            r = typeof(BallPalcementShooter).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 2, 0));

            r = typeof(BallPalcementCatcher).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 2, 0));

            r = typeof(AvoiderRole1).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole2).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole3).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole4).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            r = typeof(AvoiderRole5).GetConstructor(new Type[] { }).Invoke(new object[] { }) as RoleBase;
            roles.Add(new RoleInfo(r, 1, 0));

            




            Dictionary<int, RoleBase> matched;

            if (Model.GoalieID.HasValue)
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.Where(w => w != Model.GoalieID.Value).ToList(), roles, PreviouslyAssignedRoles);
            else
                matched = _roleMatcher.MatchRoles(engine, Model, Model.OurRobots.Keys.ToList(), roles, PreviouslyAssignedRoles);

            int? goalie = Model.GoalieID;


            int? AvoiderRole1Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole1)))
                AvoiderRole1Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole1)).First().Key;

            int? AvoiderRole2Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole2)))
                AvoiderRole2Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole2)).First().Key;

            int? AvoiderRole3Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole3)))
                AvoiderRole3Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole3)).First().Key;

            int? AvoiderRole4Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole4)))
                AvoiderRole4Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole4)).First().Key;

            int? AvoiderRole5Id = null;
            if (matched.Any(w => w.Value.GetType() == typeof(AvoiderRole5)))
                AvoiderRole5Id = matched.Where(w => w.Value.GetType() == typeof(AvoiderRole5)).First().Key;

            int? shooterID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(BallPalcementShooter)))
                shooterID = matched.Where(w => w.Value.GetType() == typeof(BallPalcementShooter)).First().Key;

            int? catcherID = null;
            if (matched.Any(w => w.Value.GetType() == typeof(BallPalcementCatcher)))
                catcherID = matched.Where(w => w.Value.GetType() == typeof(BallPalcementCatcher)).First().Key;


            #endregion

            #region Assigner
            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalie, typeof(AvoiderGoalieRole)))
                Functions[goalie.Value] = (eng, wmd) => GetRole<AvoiderGoalieRole>(goalie.Value).perform(eng, wmd, goalie.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole1Id, typeof(AvoiderRole1)))
                Functions[AvoiderRole1Id.Value] = (eng, wmd) => GetRole<AvoiderRole1>(AvoiderRole1Id.Value).Perform(engine, Model, AvoiderRole1Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole2Id, typeof(AvoiderRole2)))
                Functions[AvoiderRole2Id.Value] = (eng, wmd) => GetRole<AvoiderRole2>(AvoiderRole2Id.Value).Perform(engine, Model, AvoiderRole2Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole3Id, typeof(AvoiderRole3)))
                Functions[AvoiderRole3Id.Value] = (eng, wmd) => GetRole<AvoiderRole3>(AvoiderRole3Id.Value).Perform(engine, Model, AvoiderRole3Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole4Id, typeof(AvoiderRole4)))
                Functions[AvoiderRole4Id.Value] = (eng, wmd) => GetRole<AvoiderRole4>(AvoiderRole4Id.Value).Perform(engine, Model, AvoiderRole4Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, AvoiderRole5Id, typeof(AvoiderRole5)))
                Functions[AvoiderRole5Id.Value] = (eng, wmd) => GetRole<AvoiderRole5>(AvoiderRole5Id.Value).Perform(engine, Model, AvoiderRole5Id.Value);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, shooterID, typeof(BallPalcementShooter)))
                Functions[shooterID.Value] = (eng, wmd) => GetRole<BallPalcementShooter>(shooterID.Value).Perform(engine, Model , shooterID.Value, catcherID.Value, 0);

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, catcherID, typeof(BallPalcementCatcher)))
                Functions[catcherID.Value] = (eng, wmd) => GetRole<BallPalcementCatcher>(catcherID.Value).Perform(engine, Model, catcherID.Value, shooterID.Value, 1);


            DefenceTest.WeHaveGoalie = true;

            #endregion

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }

        public override Dictionary<int, RoleBase> RoleAssigner(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            return new Dictionary<int, RoleBase>();
        }

        public override PlayResult QueryPlayResult()
        {
            return PlayResult.InPlay;
        }

        public override void ResetPlay(GameDefinitions.WorldModel Model, GameStrategyEngine engine)
        {
            PreviouslyAssignedRoles.Clear();

        }
    }
}
