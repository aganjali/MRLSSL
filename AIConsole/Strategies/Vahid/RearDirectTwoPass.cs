using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Skills;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Strategies
{
    class RearDirectTwoPass : StrategyBase
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
            //CurrentState = (int)State.passAndCatch;
            InitialState = 0;

            FinalState = 3;
            TrapState = 2;
        }
        public override void FillInformation()
        {
            UseInMiddle = false;
            UseOnlyInMiddle = false;
            StrategyName = "V_Rear Direct Two Pass_Rear";
            AttendanceSize = 3;
            About = "one direct pass and one chip pass on dangerzone";
        }
        #endregion
        #region Reset State
        public override void ResetState()
        {
            CurrentState = (int)State.positioning;
            firstFlag = true;
            debug = true;
            isChip = false;
            circleReached = false;
            ourRobots = new List<int>();
            firstBallPos = new Position2D();
            attacker1Pos1 = new Position2D();
            attacker1Pos2 = new Position2D();
            attacker2Pos1 = new Position2D();
            attacker2Pos2 = new Position2D();
            passerPos = new Position2D();
            Catch = new StarkCatchSkill();
            CircleSkill = new CircularMotionSkill();
            //getBall = new GetBallSkill();
        }

        #endregion
        #region Global Variables
        bool firstFlag;
        bool debug = true;
        bool circleReached;
        List<int> ourRobots;
        int attacker1ID;
        int oppStopCoverID;
        int attacker2ID;
        int passerID;
        int sgn;
        bool isChip;
        Position2D firstBallPos;
        Position2D attacker1Pos1;
        Position2D attacker1Pos2;
        Position2D attacker2Pos1;
        Position2D attacker2Pos2;
        Position2D passerPos;
        int waitCounter, circleCounter;
        StarkCatchSkill Catch;
        CircularMotionSkill CircleSkill;
        GetBallSkill getBall;
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

            //DrawingObjects.AddObject(new Circle(centerPos, 0.09, myPen));
            //DrawingObjects.AddObject(centerPos);
            //DrawingObjects.AddObject(new StringDraw(posName, centerPos.Extend(-0.22, 0)));
        }
        #endregion
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();

            #region Drawing Object
            drawPos(attacker1Pos1, "attacker1 pos", Color.FromArgb(1, 233, 146, 53));
            drawPos(attacker2Pos1, "attacker1 pos", Color.FromArgb(1, 233, 146, 53));
            #endregion
            if (CurrentState == (int)State.positioning)
            {
                Vector2D ballToCenterVec = (Position2D.Zero - Model.BallState.Location);
                Planner.Add(passerID, Model.BallState.Location - ballToCenterVec.GetNormalizeToCopy(0.25), ballToCenterVec.AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                Planner.Add(attacker2ID, attacker2Pos1, (Model.BallState.Location - attacker1Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                Planner.Add(attacker1ID, attacker1Pos1, (Model.BallState.Location - attacker1Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            }


            if (CurrentState == (int)State.passAndCatch)
            {
                bool ballIsMoved = Model.BallState.Location.DistanceFrom(firstBallPos) > 0.12;
                Catch.perform(engine, Model, attacker1ID, isChip, attacker1Pos1, true, 70);

                #region avoidOppDangerZoneForAttacker2
                Obstacles obstacles = new Obstacles(Model);
                obstacles.AddObstacle(1, 0, 0, 0, new List<int>() { attacker2ID }, null);
                bool avoidOppDangerZoneForAttacker2 = ballIsMoved && !obstacles.Meet(Model.OurRobots[attacker2ID], new SingleObjectState(attacker2Pos2, Vector2D.Zero, null), .18); 
                #endregion
                Planner.Add(attacker2ID, attacker2Pos2, (GameParameters.OppGoalRight - attacker2Pos2).AngleInDegrees, PathType.UnSafe, true, true, true, avoidOppDangerZoneForAttacker2, false);
               
                if (!ballIsMoved)
                {
                    double DirectPassSpeed = 7;
                    double ChipPassSpeed = 4;
                    Planner.AddRotate(Model, passerID, true, attacker1Pos1, 0, kickPowerType.Speed, isChip ? ChipPassSpeed : DirectPassSpeed, isChip, 30, false);
                    if (Model.OurRobots[passerID].Location.DistanceFrom(Model.BallState.Location) < .135)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, new List<int>() { passerID , attacker1ID}, null);
                        isChip = obs.Meet(Model.BallState, new SingleObjectState(attacker1Pos1, Vector2D.Zero, 0), 0.06);
                        if (isChip)
                        {
                            
                        }
                    }
                }
                else
                {
                    Planner.Add(passerID, passerPos, (GameParameters.OppGoalCenter - Model.OurRobots[attacker1ID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                }
            }

            else if (CurrentState == (int)State.secondPass)
            {
                #region avoidOppDangerZoneForAttacker2
                Obstacles obstacles = new Obstacles(Model);
                obstacles.AddObstacle(1, 0, 0, 0, new List<int>() { attacker2ID }, null);
                bool avoidOppDangerZoneForAttacker2 =  !obstacles.Meet(Model.OurRobots[attacker2ID], new SingleObjectState(attacker2Pos2, Vector2D.Zero, null), .18);
                #endregion
                Planner.Add(attacker2ID, attacker2Pos2, (GameParameters.OppGoalRight - attacker1Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, avoidOppDangerZoneForAttacker2, false);
                
                //if (!CircleSkill.reachedFlag)// && !circleReached)
                //{
                //    CircleSkill.staticRotate(Model, attacker1ID, attacker2Pos2, sgn > 0 ?true : false, 6, 4);
                //    //circleReached = true;
                //}
                //else if(CircleSkill.reachedFlag)
                //{
                    if (Model.OurRobots[attacker1ID].Location.DistanceFrom(Model.BallState.Location) > .45)
                    {
                        Planner.Add(attacker1ID, attacker1Pos2, (GameParameters.OppGoalRight - attacker1Pos2).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                    }
                    //Planner.AddKick(attacker1ID, kickPowerType.Speed, true, 2.2);

                    double secondPassSpeed = 2;
                    Planner.AddRotate(Model, attacker1ID, true, attacker2Pos2, 0, kickPowerType.Speed, secondPassSpeed, true, 0, false);
                    
                //}

                Planner.Add(passerID, passerPos, (GameParameters.OppGoalCenter-passerPos).AngleInDegrees, PathType.UnSafe, false, true, true, true, false);
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            #region Init
            if (firstFlag)
            {
                init(Model,Attendance);
            }
            #endregion
            if (CurrentState == (int)State.positioning)
            {
                waitCounter++;
                if (Model.OurRobots[attacker2ID].Location.DistanceFrom(attacker2Pos1) < 0.09
                    && Model.OurRobots[passerID].Location.DistanceFrom(Model.BallState.Location - (Position2D.Zero - Model.BallState.Location).GetNormalizeToCopy(0.25)) < 0.09)
                    //&& Model.OurRobots[attacker1ID].Location.DistanceFrom(attacker1Pos1) < 1)
                {
                    waitCounter = 0;
                    CurrentState = (int)State.passAndCatch;
                }
                if (waitCounter > 560)
                    CurrentState = (int)State.finish;
            }
            if (CurrentState == (int)State.passAndCatch)
            {
                waitCounter++;
                if (Catch.currentState == 3)
                {
                    CurrentState = (int)State.secondPass;
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { attacker1ID }, null);
                    if (!obs.Meet(new SingleObjectState(Model.BallState.Location, Vector2D.Zero, null), new SingleObjectState(GameParameters.OppGoalCenter, Vector2D.Zero, null), 0.06))
                    {
                        CurrentState = (int)State.finish;
                    }
                }
                else if (Catch.currentState == 4)
                    CurrentState = (int)State.finish;
                //if (waitCounter > 250)
                   // CurrentState = (int)State.finish;

            }
            else if (CurrentState == (int)State.secondPass)
            {
                if (Model.OurRobots[attacker2ID].Location.DistanceFrom(Model.BallState.Location) < 2.5)
                {
                    CurrentState = (int)State.finish;
                }

                waitCounter++;
                if (waitCounter > 130)
                {
                    //CurrentState = (int)State.finish;
                }
            }
        }
        void init(WorldModel Model,Dictionary<int,SingleObjectState> attendance)
        {
            sgn = Math.Sign(Model.BallState.Location.Y);

            firstBallPos = Model.BallState.Location;
            #region finding Positions
            attacker1Pos1 = new Position2D(-3.89, sgn * 2.38);
            attacker1Pos2 = new Position2D(-2.68, sgn * 0.22);
            attacker2Pos2 = new Position2D(-3.69, -sgn * 1.04);
            passerPos = new Position2D(-2.80, -sgn * 0.83);
            attacker2Pos1 = firstBallPos + (attacker1Pos1 - firstBallPos).GetNormalizeToCopy(0.62);
            #endregion
            #region assigning robot IDs
            attacker2ID = FindNearestRobotID(Model.BallState.Location, ref  attendance);
            passerID = FindNearestRobotID(Model.BallState.Location, ref attendance);
            attacker1ID = FindNearestRobotID(attacker1Pos1, ref  attendance);
            #endregion

            firstFlag = false;
        }
        enum State
        {
            positioning,
            passAndCatch,
            secondPass,
            finish
        }
    }
}