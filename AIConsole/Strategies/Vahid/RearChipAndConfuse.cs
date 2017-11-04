using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using System.Drawing;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.AIConsole.Skills;
namespace MRL.SSL.AIConsole.Strategies
{
    class RearChipAndConfuse : StrategyBase
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
            CurrentState = (int)State.main;
            InitialState = 0;

            FinalState = 2;
            TrapState = 2;
        }
        public override void FillInformation()
        {
            UseInMiddle = false;
            UseOnlyInMiddle = false;
            StrategyName = "V_Rear Chip Catch And Confuse_Rear";
            AttendanceSize = 3;
            About = "2robots will confuse defenders and one robot in center of opponent's field catch the ball!";
        }
        #endregion
        #region Reset State
        public override void ResetState()
        {
            firstFlag = true;
            debug = false;
            chooseSideFlag = true;
            isChip = true;
            ourRobots = new List<int>();
            firstBallPos = new Position2D();
            peekerPos1 = new Position2D();
            attackerPos1 = new Position2D();
            Catch = new StarkCatchSkill();
            circleSkill = new CircularMotionSkill();
            strategyMode = Mode.normalGoToPoint;
            startSpinPos = new Position2D();

        }

        #endregion
        #region Global Variables
        bool firstFlag;
        bool debug;
        bool chooseSideFlag;
        bool isChip;
        List<int> ourRobots;
        Mode strategyMode;
        int? peekerID;
        int? attackerID;
        int passerID;
        int sgn;
        int nearestOppId;
        Position2D firstBallPos;
        Position2D peekerPos1;
        Position2D peekerPos2;
        Position2D attackerPos1;
        Position2D attackerPos2;
        Position2D startSpinPos;
        int waitCounter;
        StarkCatchSkill Catch;
        CircularMotionSkill circleSkill;
        float angle;
        bool angleFlag = true;
        #endregion
        #region Custom Functions
        void FindOurRobots(WorldModel model, ref List<int> ourRobots)
        {
            var tempCount = model.OurRobots.Count;
            ourRobots = new List<int>();
            Dictionary<int, SingleObjectState> tempRobots = new Dictionary<int, SingleObjectState>();
            foreach (var item in Attendance)
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
            //if (model.GoalieID.HasValue && mode)
            //{
            //    ourRobots.Remove(model.GoalieID.Value);
            //    ourRobots.Add(model.GoalieID.Value);
            //    //set goalie id the last item of our robots
            //}
        }
        int FindNearestRobotID(Position2D nearestFromPoint, ref Dictionary<int, SingleObjectState> targetRobots)
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
            targetRobots.Remove(NearestID);
            return NearestID;
        }
        private void drawPos(Position2D centerPos, string posName, System.Drawing.Color color)
        {
            Pen myPen = new Pen(color, 0.01f);

            //DrawingObjects.AddObject(new Circle(centerPos, 0.09, myPen));
            //DrawingObjects.AddObject(centerPos);
            //DrawingObjects.AddObject(new StringDraw(posName, centerPos.Extend(-0.22, 0)));
        }
        #endregion
        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            drawPos(startSpinPos, "startSpinPos", Color.FromArgb(1, 233, 146, 53));
            if (CurrentState == (int)State.main)
            {
                if (Model.BallState.Location.DistanceFrom(firstBallPos) < 0.10)
                {
                    waitCounter = 60;
                    drawPos(peekerPos1, "peeker pos", Color.FromArgb(1, 233, 146, 53));
                    drawPos(attackerPos1, "attacker pos", Color.FromArgb(1, 233, 146, 53));

                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, new List<int>() { passerID, peekerID.Value, attackerID.Value }, null);
                    isChip = obs.Meet(new SingleObjectState(firstBallPos, Vector2D.Zero, 0), new SingleObjectState(attackerPos1, Vector2D.Zero, 0), 0.10);

                    double chipPassSpeed = 4;
                    double directPassSpeed = 7;
                    Planner.AddRotate(Model, passerID, attackerPos1, 30, kickPowerType.Speed, isChip ? chipPassSpeed : directPassSpeed, isChip, 120, false);

                    Planner.Add(attackerID.Value, attackerPos1, (Model.BallState.Location - Model.OurRobots[attackerID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                    Planner.Add(peekerID.Value, peekerPos1, (Model.BallState.Location - Model.OurRobots[peekerID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                }
                else
                {

                    Planner.Add(passerID, Model.OurRobots[passerID].Location, (GameParameters.OppGoalCenter - Model.OurRobots[peekerID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);

                    peekerPos2 = new Position2D();
                    peekerPos2 = Model.Opponents[nearestOppId].Location.Extend(0, Math.Sign(attackerPos2.Y) * .16);// Model.Opponents[FindNearestRobotID(attackerPos2, ref tempOppRobots)].Location + (attackerPos2 - Model.Opponents[FindNearestRobotID(attackerPos2, ref tempOppRobots)].Location).GetNormalizeToCopy(.18);
                    drawPos(peekerPos2, "peeker pos", Color.FromArgb(1, 233, 146, 53));
                    drawPos(attackerPos2, "attacker pos", Color.FromArgb(1, 233, 146, 53));
                    Planner.Add(peekerID.Value, peekerPos2, (GameParameters.OppGoalCenter - Model.OurRobots[peekerID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, false);
                    if (Catch.currentState == 4)
                    {
                        CurrentState = (int)State.finish;
                    }
                    else if (Catch.currentState != 3)
                    {
                        Catch.perform(engine, Model, attackerID.Value, isChip, attackerPos1, true, 70);
                        startSpinPos = Model.BallState.Location;
                        angle = Model.OurRobots[attackerID.Value].Angle.Value;
                    }
                    else if (Catch.currentState == 3)
                    {
                        waitCounter = 0;
                        #region choose go side
                        if (chooseSideFlag)
                        {
                            Position2D A = startSpinPos + (GameParameters.OppLeftCorner - startSpinPos).GetNormalizeToCopy(0.95);
                            Position2D B = startSpinPos + (GameParameters.OppRightCorner - startSpinPos).GetNormalizeToCopy(0.95);
                            //startSpinPos.Extend(0, Math.Sign(attackerPos2.Y))
                            //double distancesToA = 0;
                            //double distancesToB = 0;
                            //foreach (var item in Model.Opponents)
                            //{
                            //if (item.Value.Location.DistanceFrom(GameParameters.OppGoalCenter) < 2.5)
                            //{
                            //    distancesToA += item.Value.Location.DistanceFrom(A);
                            //    distancesToB += item.Value.Location.DistanceFrom(B);
                            //}
                            var temp = new Dictionary<int, SingleObjectState>();
                            foreach (var item in Model.Opponents)
                            {
                                temp.Add(item.Key, item.Value);
                            }
                            nearestOppId = FindNearestRobotID(A, ref temp);

                            //}
                            var oppLoc = Model.Opponents[nearestOppId].Location;
                            if (A.DistanceFrom(oppLoc) > B.DistanceFrom(oppLoc))
                                attackerPos2 = startSpinPos + (A - startSpinPos).GetNormalizeToCopy(1.10);
                            else
                                attackerPos2 = startSpinPos + (B - startSpinPos).GetNormalizeToCopy(1.10);

                        }

                        chooseSideFlag = false;
                        #endregion
                        if (strategyMode == Mode.earlyRotate)
                        {
                            #region Early Rotate mode
                            if (circleSkill.reachedFlag == false)
                            {
                                double redious = 0;
                                circleSkill.perform(Model, attackerID.Value, GameParameters.OppGoalCenter, redious, false);
                                Obstacles obs = new Obstacles(Model);
                                obs.AddObstacle(1, 0, 0, 0, new List<int>() { attackerID.Value }, null);
                                Line GoalLine = new Line(GameParameters.OppGoalLeft, GameParameters.OppGoalRight);
                                Vector2D robotAngleVector = Vector2D.FromAngleSize(Math.PI * Model.OurRobots[attackerID.Value].Angle.Value / 180.0, 1);
                                Vector2D robotToGoalVector = GameParameters.OppGoalCenter - Model.OurRobots[attackerID.Value].Location;
                                Position2D pos = Model.OurRobots[attackerID.Value].Location + robotAngleVector;
                                Line robotTargetLine = new Line(Model.OurRobots[attackerID.Value].Location, pos);
                                Position2D? intersect = GoalLine.IntersectWithLine(robotTargetLine);
                                if (debug)
                                {
                                    DrawingObjects.AddObject(GoalLine);
                                    DrawingObjects.AddObject(robotTargetLine);
                                    DrawingObjects.AddObject(pos);
                                    if (intersect.HasValue)
                                        DrawingObjects.AddObject(intersect.Value);
                                }

                                DrawingObjects.AddObject(new StringDraw(Model.OurRobots[attackerID.Value].Location.DistanceFrom(attackerPos1).ToString(), Position2D.Zero.Extend(1, 0)));

                                if (intersect.HasValue && Position2D.IsBetween(GameParameters.OppGoalRight, GameParameters.OppGoalLeft, intersect.Value)
                                    && robotAngleVector.AngleInDegrees - robotToGoalVector.AngleInDegrees > -8
                                    && robotAngleVector.AngleInDegrees - robotToGoalVector.AngleInDegrees < 8
                                    && !obs.Meet(Model.OurRobots[attackerID.Value], new SingleObjectState(intersect.Value, Vector2D.Zero, 0), 0.05))
                                    Planner.AddKick(attackerID.Value, kickPowerType.Speed, Program.MaxKickSpeed, false, false);

                            }
                            else
                            {

                                goToPointAndKick(Model);
                            }
                            #endregion
                        }
                        else
                        {
                            #region Early Rotate mode
                            goToPointAndKick(Model);
                            #endregion
                        }
                    }
                }
            }
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            #region Init
            if (firstFlag)
            {
                initialize(Model);
            }
            #endregion
            if (CurrentState == (int)State.main)
            {

                if (Catch.currentState == 3 && Model.OurRobots[attackerID.Value].Location.DistanceFrom(Model.BallState.Location) > .20 && Model.BallState.Speed.Size > 0.5)
                {
                    CurrentState = (int)State.finish;
                }
                waitCounter++;

                if (waitCounter > 300)
                {
                    waitCounter = 0;
                    CurrentState = (int)State.finish;
                }

            }
        }
        void goToPointAndKick(WorldModel Model)
        {
            waitCounter = 100;
            //robot speed and accel setting 
            Planner.ChangeDefaulteParams(attackerID.Value, false);
            double accel = 2.5;
            double maxSpeed = 6;
            Planner.SetParameter(attackerID.Value, accel, maxSpeed);
            //Planner.Add(attackerID.Value, attackerPos2, (GameParameters.OppGoalCenter - Model.OurRobots[attackerID.Value].Location).AngleInDegrees, PathType.UnSafe, true, true, true, true, true);
            //meet stuff
            Obstacles obs = new Obstacles(Model);
            obs.AddObstacle(1, 0, 0, 0, new List<int>() { attackerID.Value }, null);
            Line GoalLine = new Line(GameParameters.OppGoalLeft, GameParameters.OppGoalRight);
            Vector2D robotAngleVector = Vector2D.FromAngleSize(Math.PI * Model.OurRobots[attackerID.Value].Angle.Value / 180.0, 1);
            Vector2D robotToGoalVector = GameParameters.OppGoalCenter - Model.OurRobots[attackerID.Value].Location;
            Position2D pos = Model.OurRobots[attackerID.Value].Location + robotAngleVector;
            Line robotTargetLine = new Line(Model.OurRobots[attackerID.Value].Location, pos);
            Position2D? intersect = GoalLine.IntersectWithLine(robotTargetLine);
            if (debug)
            {
                DrawingObjects.AddObject(GoalLine);
                DrawingObjects.AddObject(robotTargetLine);
                DrawingObjects.AddObject(pos);
                if (intersect.HasValue)
                    DrawingObjects.AddObject(intersect.Value);
            }

            DrawingObjects.AddObject(new StringDraw(Model.OurRobots[attackerID.Value].Location.DistanceFrom(attackerPos1).ToString(), Position2D.Zero.Extend(1, 0)));

            if (intersect.HasValue && Position2D.IsBetween(GameParameters.OppGoalRight, GameParameters.OppGoalLeft, intersect.Value)
                && robotAngleVector.AngleInDegrees - robotToGoalVector.AngleInDegrees > -11
                && robotAngleVector.AngleInDegrees - robotToGoalVector.AngleInDegrees < 11
                && !obs.Meet(Model.OurRobots[attackerID.Value], new SingleObjectState(intersect.Value, Vector2D.Zero, 0), 0.08))
                Planner.AddKick(attackerID.Value, kickPowerType.Speed, Program.MaxKickSpeed, false, false);


            else
            {
                //DrawingObjects.AddObject(new StringDraw("robotAngle = " + robotAngleVector.AngleInDegrees.ToString() +
                //    "       robotToGoalAngle = " + (GameParameters.OppGoalCenter - Model.OurRobots[attackerID.Value].Location).AngleInDegrees.ToString()
                //    , Model.OurRobots[attackerID.Value].Location.Extend(.25, 0)));
                //DrawingObjects.AddObject(new StringDraw("kickAnyWay = " + (Model.OurRobots[attackerID.Value].Location.DistanceFrom(attackerPos1) > 0.85).ToString()
                //    , Model.OurRobots[attackerID.Value].Location.Extend(.45, 0)));

                angle = (float)(GameParameters.OppGoalCenter - attackerPos2).AngleInDegrees;
                //if ((robotAngleVector.AngleInDegrees - robotToGoalVector.AngleInDegrees > -5
                //    && robotAngleVector.AngleInDegrees - robotToGoalVector.AngleInDegrees < 5
                //    ) || !angleFlag)//angleCounter > 80
                //{

                //    angleFlag = false;
                //}
                //else
                //{
                //    angle += Math.Sign(attackerPos2.Y) * 6f;
                //}

                if (Model.OurRobots[attackerID.Value].Location.DistanceFrom(startSpinPos) > 0.95)
                {
                    if (strategyMode == Mode.normalGoToPoint)
                    {
                        //circleSkill.perform(Model, attackerID.Value, GameParameters.OppGoalCenter, 0, true);
                        // if (circleSkill.reachedFlag)
                        //{
                        Planner.AddKick(attackerID.Value, kickPowerType.Speed, Program.MaxKickSpeed, false, false);
                        //}
                        CurrentState = (int)State.finish;
                    }
                    else
                        Planner.AddKick(attackerID.Value, kickPowerType.Speed, Program.MaxKickSpeed, false, false);
                }
                else
                    Planner.Add(attackerID.Value, attackerPos2, angle, PathType.UnSafe, true, true, true, true, true);

            }
        }
        void initialize(WorldModel Model)
        {
            sgn = Math.Sign(Model.BallState.Location.Y);
            firstBallPos = Model.BallState.Location;

            #region assigning robot IDs
            FindOurRobots(Model, ref ourRobots);
            passerID = ourRobots[0];
            peekerID = ourRobots[1];
            attackerID = ourRobots[2];
            #endregion

            #region finding Positions
            peekerPos1 = new Position2D(-3, sgn * 0.54);
            attackerPos1 = new Position2D(-2.5, sgn * 0.42);
            attackerPos2 = new Position2D(-4, -sgn * 1.6);
            #endregion

            firstFlag = false;
        }

        enum State
        {
            main = 0,
            finish = 1
        }
        enum Mode
        {
            earlyRotate = 0,
            normalGoToPoint = 1,
        }
    }
}
