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

namespace MRL.SSL.AIConsole.Strategies
{
    class RC2019SimpleStrategy : StrategyBase
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

            FinalState = 1;
            TrapState = 1;
        }
        public override void FillInformation()
        {
            UseInMiddle = true;
            UseOnlyInMiddle = false;
            StrategyName = "Very Simple chip";
            AttendanceSize = 3;
            About = "simple chip for australia RoboCup";
        }
        #endregion
        #region Reset State
        public override void ResetState()
        {
            sync = new Syncronizer();
            CurrentState = (int)State.go;
            firstFlag = true;
            debug = true;
            passSpeed = 4;
            isChip = true;
            ourRobots = new List<int>();
            firstBallPos = new Position2D();
            passerPos = new Position2D();
            fakePos = new Position2D();
        }

        #endregion
        #region Global Variables
        Syncronizer sync = new Syncronizer();
        bool firstFlag;
        bool debug = true;
        List<int> ourRobots;
        int passerID;
        int shooterID;
        int fakeID;
        int sgn;
        double passSpeed;
        int ruleTimer;
        bool isChip;
        Position2D firstBallPos = new Position2D();
        Position2D fakePos = new Position2D();
        Position2D passerPos = new Position2D();
        Position2D shooterPos = new Position2D();
        const int maxFiveTresh = 325;
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
            DrawingObjects.AddObject(new Circle(shooterPos, 0.1, new Pen(Color.Red, 0.01f)));
            DrawingObjects.AddObject(new Circle(fakePos, 0.1, new Pen(Color.Red, 0.01f)));

            if (CurrentState == (int)State.go)
            {
                //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, passerID, typeof(ActiveRole)))
                //    Functions[passerID] = (eng, wmd) => GetRole<ActiveRole>(passerID).PerformWithoutKick(engine, Model, passerID, GameParameters.OppGoalCenter, false, 0.15);
                
                    Planner.Add(shooterID,shooterPos, (GameParameters.OppGoalCenter - shooterPos).AngleInDegrees, PathType.UnSafe,true,true,true,true,false);
                
                Planner.Add(fakeID,fakePos,(GameParameters.OppGoalCenter - fakePos).AngleInDegrees,PathType.UnSafe,true,true,true,true,false);
                if (Model.BallState.Location.DistanceFrom(firstBallPos) < 0.15)
                {
                    if (Model.OurRobots[passerID].Location.DistanceFrom(firstBallPos) > 0.30)
                    {

                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, passerID, typeof(ActiveRole)))
                            Functions[passerID] = (eng, wmd) => GetRole<ActiveRole>(passerID).PerformWithoutKick(engine,Model,passerID,passerPos,false,0.12);
                    }
                    else
                    Planner.AddRotate(Model,passerID,shooterPos,0,kickPowerType.Speed,passSpeed,true,0,false);
                }
                else
                {
                    Planner.Add(passerID, firstBallPos, (GameParameters.OppGoalCenter - firstBallPos).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                }

            }


            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            ruleTimer++;
            DrawingObjects.AddObject(new StringDraw(ruleTimer.ToString(), Model.BallState.Location.Extend(1.2, 0)));
            #region Init
            if (firstFlag)
            {
                init(Model, Attendance);
            }
            #endregion
            if (CurrentState == (int)State.go)
            {
                if (ruleTimer > maxFiveTresh || Model.BallState.Location.DistanceFrom(firstBallPos)>3 )
                {
                    CurrentState = (int)State.finish;
                }
            }
            if (sync.Failed || sync.Finished)
            {
                CurrentState = (int)State.finish;
            }
        }
        void init(WorldModel Model, Dictionary<int, SingleObjectState> attendance)
        {
            sgn = Math.Sign(Model.BallState.Location.Y);

            firstBallPos = Model.BallState.Location;
            #region finding Positions
            //Find poses
            Position2D oppCorner = GameParameters.OppRightCorner;
            oppCorner.Y = sgn * oppCorner.Y;
            double ballDiffX = Math.Abs(oppCorner.X) - Math.Abs(Model.BallState.Location.X); 
            double ballDiffY = Math.Abs(oppCorner.Y) - Math.Abs(Model.BallState.Location.Y);
            Position2D interPol = GameParameters.OppGoalCenter.Extend(/*GameParameters.DefenceAreaHeight / 10 + */ballDiffX, 0);
            double bullshit = (Model.BallState.Location.X < 4.3) ? 6.5 : 7;
            Vector2D ExtendVec = (interPol - oppCorner).GetNormalizeToCopy(6.5 - oppCorner.DistanceFrom(Model.BallState.Location));
            
            shooterPos = Model.BallState.Location + ExtendVec;
            shooterPos = shooterPos + (shooterPos - Position2D.Zero).GetNormalizeToCopy(0.35);
            fakePos = GameParameters.OppGoalCenter.Extend(GameParameters.DefenceAreaHeight + 0.4,0.7 * sgn);
            #endregion
            #region assigning robot IDs
            //TODO : Find IDs
            passerID = FindNearestRobotID(Model.BallState.Location, ref attendance);
            shooterID  = FindNearestRobotID(shooterPos, ref attendance);
            fakeID = FindNearestRobotID(shooterPos, ref attendance);
            #endregion
            passSpeed = Math.Max(firstBallPos.DistanceFrom(shooterPos) * 0.5, 1);
            firstFlag = false;
        }
        enum State
        {
            go,
            finish
        }
    }
}
