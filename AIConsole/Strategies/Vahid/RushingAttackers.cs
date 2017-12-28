using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;
using MRL.SSL.AIConsole.Skills;
using MRL.SSL.AIConsole.Roles;

namespace MRL.SSL.AIConsole.Strategies
{
    class RushingAttackers : StrategyBase
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
            CurrentState = (int)State.positioning;
            InitialState = 0;

            FinalState = 2;
            TrapState = 2;
        }
        public override void FillInformation()
        {
            UseInMiddle = false;
            UseOnlyInMiddle = false;
            StrategyName = "V_RushingAttackers_Corner";
            AttendanceSize = 6;
            About = "this strategy will rape the defender team!";
        }
        #endregion
        #region Reset State
        public override void ResetState()
        {
            waitCounter = 0;
            CurrentState = 0;
            firstFlag = true;
            ourRobots = new List<int>();
            oppRobots = new List<int>();
            rapist1ID = null;
            rapist2ID = null;
            rapist3ID = null;
            rapist4ID = null;
            goalieID = null;
            passerID = -1;
            pos1 = null;
            pos2 = null;
            pos3 = null;
            pos4 = null;
            pos5 = null;
            posList = new List<Position2D?>();
            exFromGoal = new Position2D(-3.85, 1.17) - GameParameters.OppGoalCenter;
            sync = new Syncronizer();

        }

        #endregion
        #region Global Variables
        bool firstFlag;
        bool debug = true;
        bool positioning2Flag = true;
        bool goalieFlag = true;
        List<int> ourRobots;
        List<int> oppRobots;
        int? rapist1ID;
        int? rapist2ID;
        int? rapist3ID;
        int? rapist4ID;
        int? goalieID;
        int passerID;
        int sgn;
        Position2D? pos1, pos2, pos3, pos4, pos5;
        List<Position2D?> posList;
        Position2D? goaliePos2;
        Position2D? targetPos;
        Vector2D exFromGoal;
        Position2D firstBallPos;
        Syncronizer sync;
        int waitCounter;
        #endregion
        #region Custom Functions
        private void drawPos(Position2D centerPos, string posName, System.Drawing.Color color)
        {
            Pen myPen = new Pen(color, 0.01f);

            DrawingObjects.AddObject(new Circle(centerPos, 0.09, myPen));
            DrawingObjects.AddObject(centerPos);
            DrawingObjects.AddObject(new StringDraw(posName, centerPos.Extend(-0.22, 0)));
        }
        void FindOurRobots(WorldModel model, ref List<int> ourRobots)
        {

            var tempCount = model.OurRobots.Count;
            ourRobots = new List<int>();
            Dictionary<int, SingleObjectState> tempRobots = new Dictionary<int, SingleObjectState>();
            foreach (var item in model.OurRobots)
            {
                tempRobots.Add(item.Key, item.Value);
            }
            for (int i = 0; i < tempCount; i++)
            {
                ourRobots.Add(100);
                var minDist = double.MaxValue;
                foreach (var item in tempRobots)
                {
                    if (item.Value.Location.DistanceFrom(model.BallState.Location) < minDist)
                    {
                        minDist = item.Value.Location.DistanceFrom(model.BallState.Location);
                        ourRobots[i] = item.Key;
                    }
                }
                tempRobots.Remove(ourRobots[i]);
                minDist = double.MaxValue;
            }
            if (model.GoalieID.HasValue)
            {
                ourRobots.Remove(model.GoalieID.Value);
                ourRobots.Add(model.GoalieID.Value);
                //set goalie id the last item of our robots
            }
        }
        void sortOppRobots(WorldModel model, ref List<int> oppRobots, Position2D refrence)
        {
            var tempCount = model.Opponents.Count;
            oppRobots = new List<int>();
            for (int i = 0; i < tempCount; i++)
            {
                oppRobots.Add(100);
                var minDist = double.MaxValue;
                foreach (var item in model.Opponents)
                {
                    if (item.Value.Location.DistanceFrom(model.BallState.Location) < minDist)
                    {
                        minDist = item.Value.Location.DistanceFrom(refrence);
                        oppRobots[i] = item.Key;
                    }
                }
                model.Opponents.Remove(oppRobots[i]);
                minDist = double.MaxValue;
            }
            if (model.GoalieID.HasValue)
            {
                oppRobots.Remove(model.GoalieID.Value);
                oppRobots.Add(model.GoalieID.Value);
                //set goalie id the last item of our robots
            }
        }

        #endregion
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            #region Init
            if (firstFlag)
            {
                firstBallPos = Model.BallState.Location;
                #region assigning robot IDs
                FindOurRobots(Model, ref ourRobots);
                passerID = ourRobots[0];
                rapist1ID = ourRobots[3];
                rapist2ID = ourRobots[2];
                rapist3ID = ourRobots[1];
                rapist4ID = ourRobots[4];
                goalieID = ourRobots[5];
                #endregion
                #region finding Positions
                sgn = Math.Sign(Model.BallState.Location.Y);
                posList.Add(GameParameters.IntersectWithDangerZone(new Position2D(-4.11, 1.48 * -sgn), false));

                for (int i = 1; i < 5; i++)
                {
                    Vector2D tempVec = posList[i - 1].Value - GameParameters.OppGoalCenter;
                    double angleTresh = sgn * (10 * Math.PI) / 180;
                    Position2D tempPos = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(tempVec.AngleInRadians + angleTresh, 1);
                    Vector2D extendVec = GameParameters.IntersectWithDangerZone(tempPos, false) - GameParameters.OppGoalCenter;
                    posList.Add(GameParameters.IntersectWithDangerZone(tempPos, false) + extendVec.GetNormalizeToCopy(.20 + .10 * i));
                }

                posList[0] += (posList[0].Value - GameParameters.OppGoalCenter).GetNormalizeToCopy(.20);
                posList[4] = new Position2D(.6, 1 * -sgn);
                posList.Add(new Position2D(-2.9, 0.2 * -sgn));
                targetPos = new Line(firstBallPos, firstBallPos.Extend(2, 0 * -sgn)).IntersectWithLine(new Line(firstBallPos.Extend(1.5, 1), firstBallPos.Extend(1.5, 0)));
                Vector2D extendGoaliPosVec = new Position2D(targetPos.Value.X, 3 * sgn) - targetPos.Value;

                extendGoaliPosVec = extendGoaliPosVec.GetNormalizeToCopy(extendGoaliPosVec.Size - (extendGoaliPosVec.Size - .20));

                // if (targetPos.Value.DistanceFrom(new Position2D(targetPos.Value.X, 3 * sgn)) > .20)
                // {
                goaliePos2 = targetPos - extendGoaliPosVec.GetNormalizeToCopy(.15);
                //}
                //else
                //goaliePos2 = targetPos;
                #endregion
                firstFlag = false;
            }
            #endregion
            #region Drawing Object
            int idx = 0;
            foreach (var item in posList)
            {
                if (idx > 5)
                    idx = 0;
                drawPos(item.Value, "pos" + idx.ToString(), Color.FromArgb(1, 233, 146, 53));
                idx++;
            }
            drawPos(goaliePos2.Value, "Goalie pos", Color.FromArgb(1, 233, 146, 53));
            #endregion
            if (CurrentState == (int)State.positioning)
            {
                Vector2D vec = Model.OurRobots[passerID].Location - GameParameters.OppGoalCenter;
                if (rapist1ID.HasValue && posList[0].HasValue)
                    Planner.Add(rapist1ID.Value, posList[0].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist1ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (rapist2ID.HasValue && posList[1].HasValue)
                    Planner.Add(rapist2ID.Value, posList[1].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist2ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (rapist3ID.HasValue && posList[2].HasValue)
                    Planner.Add(rapist3ID.Value, posList[2].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist3ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (rapist4ID.HasValue && posList[3].HasValue)
                    Planner.Add(rapist4ID.Value, posList[3].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist4ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (goalieID.HasValue && posList[4].HasValue)
                    Planner.Add(goalieID.Value, posList[4].Value, (GameParameters.OppGoalCenter - Model.OurRobots[goalieID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                Planner.Add(passerID, posList[5].Value, (GameParameters.OppGoalCenter - Model.OurRobots[passerID].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            }
            else if (CurrentState == (int)State.positioning2)
            {
                #region Re-calculate posList

                if (positioning2Flag)
                {
                    posList[0] = GameParameters.IntersectWithDangerZone(new Position2D(0, 1 * sgn), false);

                    for (int i = 1; i < 4; i++)
                    {
                        Vector2D tempVec = posList[i - 1].Value - GameParameters.OppGoalCenter;
                        double angleTresh = sgn * (5 * Math.PI) / 180;
                        Position2D tempPos = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(tempVec.AngleInRadians + angleTresh, 1);
                        Vector2D extendVec = GameParameters.IntersectWithDangerZone(tempPos, false) - GameParameters.OppGoalCenter;
                        posList[i] = (GameParameters.IntersectWithDangerZone(tempPos, false) + extendVec.GetNormalizeToCopy(.20 + .20 * i));
                    }
                    posList[0] += (posList[0].Value - GameParameters.OppGoalCenter).GetNormalizeToCopy(.20);
                    positioning2Flag = false;
                }

                #endregion
                bool ballIsMoved = Model.BallState.Location.DistanceFrom(firstBallPos) > .10;
                if (!ballIsMoved)
                    Planner.Add(rapist4ID.Value, posList[3].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist3ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                else
                    Planner.Add(rapist4ID.Value, Model.OurRobots[rapist4ID.Value].Location, (GameParameters.OppGoalCenter - Model.OurRobots[rapist3ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                if (waitCounter > 5 && !ballIsMoved)
                    Planner.Add(rapist3ID.Value, posList[2].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist3ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                else

                    Planner.Add(rapist3ID.Value, Model.OurRobots[rapist3ID.Value].Location, (GameParameters.OppGoalCenter - Model.OurRobots[rapist3ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (waitCounter > 10 && !ballIsMoved)
                    Planner.Add(rapist2ID.Value, posList[1].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist2ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                else
                    Planner.Add(rapist2ID.Value, Model.OurRobots[rapist2ID.Value].Location, (GameParameters.OppGoalCenter - Model.OurRobots[rapist3ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                if (waitCounter > 15 && !ballIsMoved)
                    Planner.Add(rapist1ID.Value, posList[0].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist1ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                else
                    Planner.Add(rapist1ID.Value, Model.OurRobots[rapist1ID.Value].Location, (GameParameters.OppGoalCenter - Model.OurRobots[rapist3ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
               // sync.kMotionDirect = 1;
                var passSpeed = 3.2;//(posList[2].Value.DistanceFrom(firstBallPos) - 1.5) * .93;      

                sync.SyncDirectPass(engine, Model, passerID, 45, goalieID.Value, targetPos.Value, GameParameters.OppGoalCenter, passSpeed, 8, 105);
                if (waitCounter > 40 && !ballIsMoved)
                {
                    //Planner.Add(passerID, Model.OurRobots[passerID].Location, (GameParameters.OppGoalCenter - Model.OurRobots[goalieID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);                    
                    //Planner.AddRotate(Model, passerID, targetPos.Value, 120, kickPowerType.Speed, passSpeed, false, 120, false);

                    //sync.kPassDirectCatch


                }
                else
                {
                    Planner.Add(passerID, firstBallPos, (GameParameters.OppGoalCenter - firstBallPos).AngleInDegrees);
                }

                if (firstBallPos.DistanceFrom(Model.OurRobots[passerID].Location) < 0.20 && goalieFlag)
                {
                    Planner.Add(goalieID.Value, goaliePos2.Value, (GameParameters.OppGoalCenter - Model.OurRobots[goalieID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                    if (Model.OurRobots[goalieID.Value].Location.DistanceFrom(goaliePos2.Value) < 0.11)
                    {
                        goalieFlag = false;
                    }
                }
                //else if (!ballIsMoved)
                //    Planner.Add(goalieID.Value, Model.OurRobots[goalieID.Value].Location, GameParameters.AngleModeD((GameParameters.OppGoalCenter - Model.OurRobots[goalieID.Value].Location).AngleInDegrees), PathType.UnSafe, true, true, true, true, false);
                //else if (ballIsMoved)
                //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, goalieID.Value, typeof(NewActiveRole)))
                //Functions[goalieID.Value] = (eng, wmd) => GetRole<NewActiveRole>(goalieID.Value).Perform(engine,Model,goalieID.Value,false);

                //CurrentState = (int)State.finish;
                //GetRole<ActiveRole>(goalieID.Value).PerformForStrategy(engine,Model,goalieID.Value,null,null,false,true,true);
                //GetRole<OneTouchRole>(goalieID.Value).Perform(engine, Model, goalieID.Value, Model.OurRobots[passerID], false, GameParameters.OppGoalCenter, 8, false, 4);
            }
            else if (CurrentState == (int)State.main)
            {

                if (rapist1ID.HasValue && posList[0].HasValue)
                    Planner.Add(rapist1ID.Value, posList[0].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist1ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (rapist2ID.HasValue && posList[1].HasValue)
                    Planner.Add(rapist2ID.Value, posList[1].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist2ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (rapist3ID.HasValue && posList[2].HasValue)
                    Planner.Add(rapist3ID.Value, posList[2].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist3ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                if (rapist4ID.HasValue && posList[3].HasValue)
                    Planner.Add(rapist4ID.Value, posList[3].Value, (GameParameters.OppGoalCenter - Model.OurRobots[rapist4ID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {

            if (CurrentState == (int)State.positioning)
            {
                waitCounter++;
                if (rapist3ID.HasValue)
                {
                    if (rapist1ID.HasValue && rapist2ID.HasValue && rapist3ID.HasValue && rapist4ID.HasValue && goalieID.HasValue
                        && Model.OurRobots[rapist1ID.Value].Location.DistanceFrom(posList[0].Value) < .09
                        && Model.OurRobots[rapist2ID.Value].Location.DistanceFrom(posList[1].Value) < .09
                        && Model.OurRobots[rapist3ID.Value].Location.DistanceFrom(posList[2].Value) < .09
                        && Model.OurRobots[rapist4ID.Value].Location.DistanceFrom(posList[3].Value) < .09
                        && Model.OurRobots[goalieID.Value].Location.DistanceFrom(posList[4].Value) < .09)
                    {
                        waitCounter = 0;
                        CurrentState = (int)State.positioning2;
                    }
                    if (waitCounter > 360 && Model.OurRobots[goalieID.Value].Location.DistanceFrom(posList[4].Value) < 0.12)
                    {
                        waitCounter = 0;
                        CurrentState = (int)State.positioning2;
                    }
                    else if (waitCounter > 360)
                    {
                        waitCounter = 0;
                        CurrentState = (int)State.finish;
                    }
                }
            }
            else if (CurrentState == (int)State.positioning2)
            {
                waitCounter++;
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > .10 && waitCounter > 85)
                {
                    CurrentState = (int)State.finish;
                }

            }
        }

        enum State
        {
            positioning,
            positioning2,
            main,
            finish
        }
    }
}
