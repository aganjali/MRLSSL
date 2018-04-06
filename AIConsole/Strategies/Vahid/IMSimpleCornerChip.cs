using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Strategies.Vahid
{
    class IMSimpleCornerChip : StrategyBase
    {

        #region intrupt
        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, ref GameDefinitions.GameStatus Status)
        {
            if (CurrentState == (int)State.finish)
            {
                Status = GameStatus.Normal;
                return false;
            }
            return true;
        }
        public override void InitializeStates(GameStrategyEngine engine, GameDefinitions.WorldModel Model, Dictionary<int, GameDefinitions.SingleObjectState> attendance)
        {
            Attendance = attendance;
            InitialState = 0;

            FinalState = 3;
            TrapState = 3;
        }
        public override void FillInformation()
        {
            UseInMiddle = true;
            UseOnlyInMiddle = false;
            StrategyName = "IMSimpleCornerChip";
            AttendanceSize = 3;
            About = "simple chip sync to corner of dangerzone";
        }
        #endregion
        #region Reset State
        public override void ResetState()
        {
            sync = new Syncronizer();
            CurrentState = (int)State.positioning;
            firstFlag = true;
            debug = true;
            passSpeed = 4;
            isChip = true;
            plannerCounter = 0;
            ourRobots = new List<int>();
            firstBallPos = new Position2D();
            abdullahPos = new Position2D();
            majidPos = new Position2D();
            karimPos = new Position2D();
        }

        #endregion
        #region Global Variables
        Syncronizer sync = new Syncronizer();
        bool firstFlag;
        bool debug = true;
        List<int> ourRobots;
        int abdullahID;
        int majidID;
        int karimID;
        int sgn;
        double passSpeed;
        bool isChip;
        int plannerCounter;
        Position2D firstBallPos = new Position2D();
        Position2D majidPos = new Position2D();
        Position2D abdullahPos = new Position2D();
        Position2D karimPos = new Position2D();
        int waitCounter;
        CircularMotionSkill CircleSkill;
        #endregion
        #region Custom Functions
        void FindOurRobots(WorldModel model, ref List<int> ourRobots, Dictionary<int, SingleObjectState> targetRobots)
        {
            ourRobots = new List<int>();

            for (int i = 0; i < targetRobots.Count; i++)
            {
                ourRobots.Add(-1);
                var minDist = double.MaxValue;
                foreach (var item in targetRobots)
                {
                    if (item.Value.Location.DistanceFrom(model.BallState.Location) < minDist)
                    {
                        minDist = item.Value.Location.DistanceFrom(model.BallState.Location);
                        ourRobots[i] = item.Key;
                    }
                }
                targetRobots.Remove(ourRobots[i]);
                minDist = double.MaxValue;
            }
            //if (model.GoalieID.HasValue && mode)
            //{
            //    ourRobots.Remove(model.GoalieID.Value);
            //    ourRobots.Add(model.GoalieID.Value);
            //    //set goalie id the last item of our robots
            //}
        }
        int FindNearestRobotID(Position2D nearestFromPoint, ref Dictionary<int, SingleObjectState> targetRobots)
        {
            int ID = FindNearestRobotID(nearestFromPoint, targetRobots);
            targetRobots.Remove(ID);
            return ID;
        }

        int FindNearestRobotID(Position2D nearestFromPoint, Dictionary<int, SingleObjectState> targetRobots)
        {
            int tempCount = targetRobots.Count;
            int NearestID = -1;
            var minDist = double.MaxValue;
            foreach (var item in targetRobots)
            {
                if (item.Value.Location.DistanceFrom(nearestFromPoint) < minDist)
                {
                    minDist = item.Value.Location.DistanceFrom(nearestFromPoint);
                    NearestID = item.Key;
                }
            }
            return NearestID;
        }

        #endregion
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            DrawingObjects.AddObject(karimPos);
            DrawingObjects.AddObject(majidPos);
            if (CurrentState == (int)State.positioning)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, abdullahID, typeof(ActiveRole)))
                    Functions[abdullahID] = (eng, wmd) => GetRole<ActiveRole>(abdullahID).PerformWithoutKick(engine, Model, abdullahID, GameParameters.OppGoalCenter, false, 0.15);
                Planner.ChangeDefaulteParams(majidID, false);
                Planner.SetParameter(majidID, 3, 3);
                Planner.Add(majidID, majidPos, (Model.BallState.Location - majidPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                Planner.ChangeDefaulteParams(karimID, false);
                Planner.SetParameter(karimID, 3, 3);
                Planner.Add(karimID, karimPos, (Model.BallState.Location - karimPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);


            }
            else if (CurrentState == (int)State.passAndCatch)
            {
                Planner.Add(majidID, majidPos, (Model.BallState.Location - karimPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                sync.SyncChipPass(engine, Model, abdullahID, 0, karimID, karimPos, GameParameters.OppGoalCenter, passSpeed, Program.MaxKickSpeed, 30, false);
                if(firstBallPos.DistanceFrom(Model.BallState.Location) > 0.10)
                Planner.Add(abdullahID,Position2D.Zero.Extend(0,sgn  * 2),0,PathType.UnSafe,true,true,true,true,false);

            }


            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            #region Init
            if (firstFlag)
            {
                init(Model, Attendance);
            }
            #endregion
            if (CurrentState == (int)State.positioning)
            {
                if (Model.OurRobots[karimID].Location.DistanceFrom(karimPos) < 0.12)
                {
                    CurrentState = (int)State.passAndCatch;
                }
            }
            if (CurrentState == (int)State.passAndCatch)
            {
            }
            if (sync.Failed || sync.Finished || firstBallPos.DistanceFrom(Model.BallState.Location) > 3 || (firstBallPos.DistanceFrom(Model.BallState.Location) > 0.60 && Model.BallState.Location.DistanceFrom(karimPos) < 0.5))
            {
                CurrentState = (int)State.finish;
            }
        }
        void init(WorldModel Model, Dictionary<int, SingleObjectState> attendance)
        {
            sgn = -Math.Sign(Model.BallState.Location.Y);

            firstBallPos = Model.BallState.Location;
            #region finding Positions
            karimPos = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(Math.PI / 4 - (30 * Math.PI / 180), GameParameters.DefenceAreaHeight * Math.Sqrt(2) + 0.20);
            karimPos = GameParameters.IntersectWithDangerZone(karimPos, false, 0.45);
            karimPos = new Position2D(karimPos.X, sgn * karimPos.Y);
            majidPos = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(Math.PI / 4 - (67 * Math.PI / 180), GameParameters.DefenceAreaHeight * Math.Sqrt(2) + 0.20);
            majidPos = GameParameters.IntersectWithDangerZone(majidPos, false, 0.45);
            majidPos = new Position2D(majidPos.X, sgn * majidPos.Y);
            //abdullahPos = new Position2D(-2.08, -sgn * 2.5);
            #endregion
            #region assigning robot IDs
            majidID = FindNearestRobotID(majidPos, ref attendance);
            karimID = FindNearestRobotID(karimPos, ref  attendance);
            abdullahID = FindNearestRobotID(firstBallPos, ref  attendance);
            #endregion
            passSpeed =  Math.Max( firstBallPos.DistanceFrom(karimPos) * 0.298 , 1);
            firstFlag = false;
        }
        enum State
        {
            positioning,
            passAndCatch,
            finish
        }
    }
}
