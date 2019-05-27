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
    class Abdullah_Rear : StrategyBase
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
            StrategyName = "AbdullahAttacker_Rear";
            AttendanceSize = 4;
            About = "AB";
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
        int ghasemPasserID;
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
        private void drawPos(Position2D centerPos, string posName, System.Drawing.Color color)
        {
            Pen myPen = new Pen(new System.Drawing.Color(), 0.01f);

            DrawingObjects.AddObject(new Circle(centerPos, 0.09, myPen));
            DrawingObjects.AddObject(centerPos);
            DrawingObjects.AddObject(new StringDraw(posName, centerPos.Extend(-0.22, 0)));
        }
        #endregion
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();

            #region Drawing Object

            #endregion
            if (CurrentState == (int)State.positioning)
            {
                Planner.Add(karimID, karimPos, (GameParameters.OppGoalLeft - karimPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                Planner.ChangeDefaulteParams(karimID, false);
                Planner.SetParameter(karimID, 3, 3);
                Planner.Add(majidID, majidPos, (Model.BallState.Location - majidPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                Planner.ChangeDefaulteParams(majidID, false);
                Planner.SetParameter(majidID, 3, 3);
                if (Model.OurRobots[majidID].Location.DistanceFrom(majidPos) < 1.5)
                    Planner.Add(abdullahID, abdullahPos, (GameParameters.OppGoalRight - abdullahPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ghasemPasserID, typeof(ActiveRole)))
                    Functions[ghasemPasserID] = (eng, wmd) => GetRole<ActiveRole>(ghasemPasserID).PerformWithoutKick(engine, Model, ghasemPasserID, abdullahPos, false, 0.25);

            }
            else if (CurrentState == (int)State.passAndCatch)
            {
                Planner.Add(karimID, karimPos, (GameParameters.OppGoalCenter - karimPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                Planner.Add(majidID, majidPos, (Model.BallState.Location - majidPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                Planner.Add(abdullahID, abdullahPos, (GameParameters.OppGoalCenter - abdullahPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);



                //if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.09)
                //{
                //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, abdullahID, typeof(DefenderCornerRole1)))
                //        Functions[abdullahID] = (eng, wmd) => GetRole<DefenderCornerRole1>(abdullahID).Run(engine, Model, abdullahID, GameParameters.OppGoalCenter, 0);
                //}
                //else
                //{
                if (isChip)
                {
                    double chipKick = Model.OurRobots[ghasemPasserID].Location.DistanceFrom(firstBallPos) *0.9 ;
                    sync.SyncChipCatch(engine, Model, ghasemPasserID, 0, karimID, karimPos, GameParameters.OppGoalCenter, chipKick, Program.MaxKickSpeed, 0, false);
                }
                else
                {
                    sync.SyncDirectCatch(engine, Model, ghasemPasserID, 30, abdullahID, abdullahPos, GameParameters.OppGoalCenter, 4.2, Program.MaxKickSpeed, 60);

                }

                //}
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
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.09)
                {
                    CurrentState = (int)State.finish;
                }
                if (Model.OurRobots[karimID].Location.DistanceFrom(karimPos) < .25)
                {
                    CurrentState = (int)State.passAndCatch;
                }
            }
            if (CurrentState == (int)State.passAndCatch)
            {
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.09)
                {
                    plannerCounter++;
                }
                if (plannerCounter > 50 && karimPos.DistanceFrom(Model.OurRobots[karimID].Location) < 2)
                {
                    CurrentState = (int)State.finish;
                }
                if (sync.Failed || sync.Finished)
                {
                    CurrentState = (int)State.finish;
                }
            }
        }
        void init(WorldModel Model, Dictionary<int, SingleObjectState> attendance)
        {
            sgn = Math.Sign(Model.BallState.Location.Y);

            firstBallPos = Model.BallState.Location;
            #region finding Positions
            karimPos = new Position2D(-3.28, sgn * 1.49);
            majidPos = new Position2D(-2.68, sgn * 0.22);
            abdullahPos = new Position2D(-2.08, -sgn * 2.5);
            #endregion
            #region assigning robot IDs
            ghasemPasserID = FindNearestRobotID(Model.BallState.Location, ref  attendance);
            majidID = FindNearestRobotID(majidPos, ref attendance);
            karimID = FindNearestRobotID(karimPos, ref  attendance);
            abdullahID = FindNearestRobotID(abdullahPos, ref  attendance);
            #endregion
            passSpeed = firstBallPos.DistanceFrom(abdullahPos) * 0.60;
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
