using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using System.Drawing;

namespace MRL.SSL.AIConsole.Roles
{
    public enum robotState
    {
        ballFar,
        ballNear,
        ballBack
    }
    public enum targetPosition
    {
        left,
        right
    }
    class PenaltyShooterRole2014 : RoleBase
    {
        bool clockwise = false;
        Position2D firstTarget = new Position2D();
        Position2D secondTraget = new Position2D();
        Position2D Target = GameParameters.OppGoalCenter;
        Position2D goalBestPoint = new Position2D();
        Position2D initializePoint = new Position2D();
        List<targetPosition> targetAxisList = new List<targetPosition>();
        Position2D leftOrRightTarget = new Position2D();
        Position2D? robotFrontPoint = new Position2D();
        Position2D afterPenaltyTarget = new Position2D();
        Position2D opponentGoal = new Position2D();
        Position2D nearTarget = new Position2D();
        Vector2D goalieVec = new Vector2D();
        Position2D goaliePosition = new Position2D();
        List<Position2D> targetPoints = new List<Position2D>();
        List<double> distancePoint = new List<double>();
        Position2D target = new Position2D();
        Position2D ballFirstLocation = new Position2D();
        static Position2D ballState = new Position2D();
        Obstacle obstacle = new Obstacle();
        static double rotateAngle = 0;
        static double targetAngle = 0;
        static int targetAxisCounter = 0;
        bool robotHeadMeet = true;
        static bool LeftAngleFlag = true;
        bool flag = true;
        bool timeFlag = true;
        bool rotateAngleFlag = true;
        bool penaltyFlag = true;
        bool rotateTimeFlag = true;
        bool PenaltyRandomFlag = true;
        bool penaltyUpdateRandomFlag = true;
        bool flagAxisCounter = true;
        bool withoutChangego = false;
        bool withRotateWithoutChange = false;
        //static int PenaltyMood = 0;
        static int TimeMode = 0;
        int minTime = 0;
        int maxTime = 240;
        int frameCounter = 0;
        int counterConfidence = 0;
        static int? goalieID = null;

        public override RoleCategory QueryCategory()
        {
            return RoleCategory.Active;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="RobotID"></param>
        /// <param name="withRotateAngle"></param>
        /// <param name="penaltyMood"> penaltyMode=0 =>size logic
        /// penaltyMode=1 => repeat structure
        /// penaltyMode=2 => far structure</param>
        /// <param name="timeMode">timeMode=0 => random time
        /// timeMode=1 => pattern time</param>
        public void Perform(WorldModel model, int RobotID, bool withRotateAngle,/*  int penaltyMood ,*/int timeMode)
        {
            if (timeFlag)
            {
                frameCounter++;
            }
            //PenaltyMood = penaltyMood;
            TimeMode = timeMode;
            double angle = (GameParameters.OppGoalCenter - model.OurRobots[RobotID].Location).AngleInDegrees;

            if (PenaltyRandomFlag)
            {
                penaltyRandomMood = 1;//PenaltyRandom(0, 2, 1, true);
                PenaltyRandomFlag = false;
            }

            if (/*penaltyMood*/penaltyRandomMood == 2)
            {
                if (CurrentState == (int)robotState.ballFar)
                {
                    counterConfidence = 2;
                    Planner.Add(RobotID, firstTarget, angle, PathType.Safe, true, true, false, false);
                }

                if (CurrentState == (int)robotState.ballNear)
                {
                    anglePattern(model, RobotID);
                }

                if (CurrentState == (int)robotState.ballBack)
                {
                    if (rotateTimeFlag)
                    {
                        double rotateTime = calculateRoatateTime(model, RobotID);
                    }
                    Planner.AddRotate(model, RobotID, true, target, 0, kickPowerType.Speed, StaticVariables.MaxKickSpeed, false, 0, false);
                    if (timeFlag)
                    {
                        ballState = model.BallState.Location;
                        timeFlag = false;
                    }
                    if (ballState.DistanceFrom(model.BallState.Location) > 0.1)
                    {
                        afterPenaltyTarget = new Position2D(-2, 0);
                        Planner.Add(RobotID, afterPenaltyTarget, angle);
                    }
                }
            }
            if (/*penaltyMood*/penaltyRandomMood == 0 || penaltyRandomMood/*penaltyMood*/ == 1)
            {

                if (CurrentState == (int)robotState.ballFar)
                {
                    Planner.Add(RobotID, firstTarget, angle, PathType.Safe, true, true, true, false);
                }
                if (CurrentState == (int)robotState.ballNear)
                {
                    Planner.Add(RobotID, secondTraget, angle, PathType.Safe, true, true, true, false);
                }
            }
            if (CurrentState == (int)robotState.ballBack)
            {
                if (/*penaltyMood*/penaltyRandomMood == 0)
                {
                    
                    if (model.BallState.Speed.Size < 0.08)
                    {
                        
                        if (!robotHeadMeet && (withoutChangego || !withRotateWithoutChange))
                        {
                            if (robotFrontPoint.HasValue)
                            {
                                DrawingObjects.AddObject(robotFrontPoint.Value);
                                if (model.Status == GameStatus.Penalty_OurTeam_Go && (robotFrontPoint.Value.DistanceFrom(GameParameters.OppGoalCenter) < 0.32 || withoutChangego))
                                {
                                    if (rotateTimeFlag)
                                    {
                                        double rotateTime = calculateRoatateTime(model, RobotID);
                                    }

                                    //if (rotateAngle == 0)
                                    //{
                                    Planner.AddRotate(model, RobotID, true, robotFrontPoint.Value, 0, kickPowerType.Speed, StaticVariables.MaxKickSpeed, false, 0, false);
                                    //}
                                    //else
                                    //{
                                    //    Planner.AddRotate(model, RobotID, goalBestPoint, rotateAngle, kickPowerType.Speed, StaticVariables.MaxKickSpeed, false, (leftOrRightTarget.Y > 0) ? true : false, 20, false);
                                    //}
                                    withoutChangego = true;
                                    DrawingObjects.AddObject(new Circle(new Position2D(0, 0), 1));
                                }
                            }
                        }
                        else
                        {
                            if (firstTime3)
                            {
                                if (leftOrRightTarget.Y > 0)
                                    clockwise = true;
                                else
                                    clockwise = false;
                                firstTime3 = false;
                            }
                            if (rotateTimeFlag)
                            {
                                double rotateTime = calculateRoatateTime(model, RobotID);
                            }
                            withRotateWithoutChange = true;
                            Planner.AddRotate(model, RobotID, goalBestPoint, 45, kickPowerType.Speed, StaticVariables.MaxKickSpeed, false, clockwise, 60, false);
                            DrawingObjects.AddObject(new Circle(new Position2D(0, 0), 2));
                            DrawingObjects.AddObject(new StringDraw(goalBestPoint.ToString(), new Position2D(-1, -1)));
                        }
                    }
                    else
                    {
                        afterPenaltyTarget = new Position2D(-2, 0);
                        Planner.Add(RobotID, afterPenaltyTarget, angle);
                    }
                }
                if (/*penaltyMood*/penaltyRandomMood == 1)
                {
                    if (model.BallState.Speed.Size < .02)
                    {
                        
                        
                        if (!robotHeadMeet && (withoutChangego || !withRotateWithoutChange))
                        {
                            if (robotFrontPoint.HasValue)
                            {
                                DrawingObjects.AddObject(robotFrontPoint.Value);
                                if (model.Status == GameStatus.Penalty_OurTeam_Go && (robotFrontPoint.Value.DistanceFrom(GameParameters.OppGoalCenter) < 0.32 || withoutChangego))
                                {
                                    if (rotateTimeFlag)
                                    {
                                        double rotateTime = calculateRoatateTime(model, RobotID);
                                    }
                                    Planner.AddRotate(model, RobotID, true, robotFrontPoint.Value, 0, kickPowerType.Speed, StaticVariables.MaxKickSpeed, false, 0, false);
                                    withoutChangego = true;
                                }
                            }
                        }
                        else
                        {
                            if (rotateTimeFlag)
                            {
                                double rotateTime = calculateRoatateTime(model, RobotID);
                            }
                            withRotateWithoutChange = true;
                            Planner.AddRotate(model, RobotID, leftOrRightTarget, initializePoint, kickPowerType.Speed, StaticVariables.MaxKickSpeed, false, (leftOrRightTarget.Y > 0) ? true : false, 0/*(penaltyTime)*/, false);
                        }
                    }
                    else
                    {
                        afterPenaltyTarget = new Position2D(-2, 0);
                        Planner.Add(RobotID, afterPenaltyTarget, angle);
                    }
                }
            }

        }

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> AssignedRoles)
        {

            if (penaltyFlag)
            {
                ballFirstLocation = Model.BallState.Location;
                penaltyFlag = false;
            }
            goalieID = PenaltyGoalieID(Model);
            if (penaltyRandomMood == 2)
            {

                firstTarget = Model.BallState.Location + new Vector2D(0.4, 0);
                secondTraget = Model.BallState.Location + new Vector2D(0.15, 0);
                if (CurrentState == (int)robotState.ballFar && firstTarget.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.01 && Model.Status == GameStatus.Penalty_OurTeam_Waiting)
                {

                    if (flag)
                    {
                        penaltyTime = PenaltyTime(minTime, maxTime, 20, false);
                        flag = false;
                    }
                    CurrentState = (int)robotState.ballNear;
                }
                if (CurrentState == (int)robotState.ballNear && secondTraget.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.01)
                {
                    if (Model.Status == GameStatus.Penalty_OurTeam_Go)
                    {
                        Vector2D ballAndCenterVec = GameParameters.OppGoalCenter - Model.BallState.Location;
                        if (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * (Math.PI / 180), 1), ballAndCenterVec)) < 15)
                        {

                            CurrentState = (int)robotState.ballBack;
                        }
                    }

                    if (CurrentState == (int)robotState.ballBack)
                    {
                        for (int i = 0; i < 60; i++)
                        {
                            Position2D extendOppGoalLeft = GameParameters.OppGoalLeft.Extend(0, 0.05);
                            opponentGoal = extendOppGoalLeft.Extend(0, (double)i / 100);
                            targetPoints.Add(opponentGoal);
                            double distance = Model.Opponents[goalieID.Value].Location.DistanceFrom(opponentGoal);
                            distancePoint.Add(distance);

                        }
                        double max = double.MinValue;
                        for (int j = 0; j < distancePoint.Count; j++)
                        {
                            if (distancePoint[j] >= max)
                            {
                                max = distancePoint[j];
                                target = targetPoints[j];
                            }
                        }

                    }
                }
            }
            if (penaltyRandomMood == 1)
            {
                firstTarget = Model.BallState.Location + new Vector2D(0.3, 0);
                secondTraget = Model.BallState.Location + new Vector2D(0.15, 0);
                if (CurrentState == (int)robotState.ballFar && firstTarget.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.01)
                {
                    CurrentState = (int)robotState.ballNear;
                }
                if (CurrentState == (int)robotState.ballNear && Model.Status == GameStatus.Penalty_OurTeam_Go && secondTraget.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.01)
                {
                    CurrentState = (int)robotState.ballBack;
                }
                if (CurrentState == (int)robotState.ballBack)
                {
                    if (flag)
                    {
                        targetAxisList = TargetPatternMaker(true, 1, 20);
                        penaltyTime = PenaltyTime(100, 400, 20, false);
                        flag = false;
                    }
                    if (flagAxisCounter)
                    {
                        if (targetAxisList[targetAxisCounter] == targetPosition.left)
                        {
                            initializePoint = Model.BallState.Location + ((Model.BallState.Location - GameParameters.OppGoalLeft.Extend(0, 0.05)).GetNormalizeToCopy(0.2));
                            leftOrRightTarget = GameParameters.OppGoalRight.Extend(0, -0.05);
                        }
                        if (targetAxisList[targetAxisCounter] == targetPosition.right)
                        {
                            initializePoint = Model.BallState.Location + ((Model.BallState.Location - GameParameters.OppGoalRight.Extend(0, -0.05)).GetNormalizeToCopy(0.2));
                            leftOrRightTarget = GameParameters.OppGoalLeft.Extend(0, 0.05);
                        }

                        targetAxisCounter++;
                        flagAxisCounter = false;
                    }
                }

                Vector2D robotHeadVector = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * (Math.PI / 180), 1);
                Line l = new Line(GameParameters.OppGoalLeft, GameParameters.OppGoalRight);
                Line l1 = new Line(Model.OurRobots[RobotID].Location + robotHeadVector, Model.OurRobots[RobotID].Location);

                Position2D? intersectPoint = l.IntersectWithLine(l1);
                robotFrontPoint = intersectPoint;
                Obstacles obs = new Obstacles();

                if (goaliePositionFlag)
                {
                    Position2D goaliePosition = GoaliePos(Model);
                }
                foreach (var item in Model.Opponents.Keys)
                {
                    obs.AddRobot(/*Model.Opponents[item].Location*/goaliePosition, false, item);
                    
                }
                DrawingObjects.AddObject(new Circle(goaliePosition, 0.05,new Pen(Color.Red,0.05f)),"locationwwxws");

                robotHeadMeet = obs.Meet(Model.BallState, new SingleObjectState(robotFrontPoint.Value, Vector2D.Zero, 0), .04);//obstacle.Meet(Model.BallState, new SingleObjectState(intersectPoint.Value, Vector2D.Zero, 0), 0);

                if (CurrentState == (int)robotState.ballBack && goalieID.HasValue && robotHeadMeet)
                {
                    rotateAngle = 90;
                }
                if (CurrentState == (int)robotState.ballBack && goalieID.HasValue && !robotHeadMeet)
                {
                    rotateAngle = 0;
                }

                goalieID = PenaltyGoalieID(Model);
                SingleObjectState goalieState = (goalieID.HasValue) ? Model.Opponents[goalieID.Value] : new SingleObjectState(GameParameters.OppGoalCenter.Extend(0.1, 0), Vector2D.Zero, 0);
            }

            if (penaltyRandomMood == 0)
            {
                firstTarget = Model.BallState.Location + new Vector2D(0.3, 0);
                secondTraget = Model.BallState.Location + new Vector2D(0.15, 0);

                if (CurrentState == (int)robotState.ballFar && firstTarget.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.01)
                {
                    CurrentState = (int)robotState.ballNear;
                }
                if (CurrentState == (int)robotState.ballNear && Model.Status == GameStatus.Penalty_OurTeam_Go && secondTraget.DistanceFrom(Model.OurRobots[RobotID].Location) < 0.01)
                {
                    CurrentState = (int)robotState.ballBack;
                    if (goalieID.HasValue && firstTime)
                    {
                        goalBestPoint = BestPointInGoal(Model, goalieID.Value);
                        firstTime = false;
                    }
                    else if (!goalieID.HasValue)
                    {
                        goalBestPoint = GameParameters.OppGoalCenter.Extend(0, -.2);
                    }
                }

                Vector2D robotHeadVector = Vector2D.FromAngleSize(Model.OurRobots[RobotID].Angle.Value * (Math.PI / 180), 1);
                Line l = new Line(GameParameters.OppGoalLeft, GameParameters.OppGoalRight);
                Line l1 = new Line(Model.OurRobots[RobotID].Location + robotHeadVector, Model.OurRobots[RobotID].Location);

                Position2D? intersectPoint = l.IntersectWithLine(l1);
                robotFrontPoint = intersectPoint;
                Obstacles obs = new Obstacles();

                if (goaliePositionFlag)
                {
                    Position2D goaliePosition = GoaliePos(Model);
                }

                foreach (var item in Model.Opponents.Keys)
                {
                    obs.AddRobot(/*Model.Opponents[item].Location*/goaliePosition, false, item);
                }
                if (CurrentState == (int)robotState.ballBack && goalieID.HasValue && robotHeadMeet && firstTime2)
                {
                    robotHeadMeet = obs.Meet(Model.BallState, new SingleObjectState(robotFrontPoint.Value, Vector2D.Zero, 0), .04);//obstacle.Meet(Model.BallState, new SingleObjectState(intersectPoint.Value, Vector2D.Zero, 0), 0);

                    if (robotHeadMeet)
                    {
                        rotateAngle = 90;
                    }
                    else
                    {
                        rotateAngle = 0;
                    }
                    firstTime2 = false;
                }
            }

        }

        public override double CalculateCost(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return RobotID;
        }

        public override List<RoleBase> SwichToRole(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            return new List<RoleBase>();
        }

        public override bool Evaluate(GameStrategyEngine engine, WorldModel Model, int RobotID, Dictionary<int, RoleBase> previouslyAssignedRoles)
        {
            throw new NotImplementedException();
        }

        public robotState calculateGoaliState(WorldModel Model)
        {
            return robotState.ballFar;
        }
        public int? PenaltyGoalieID(WorldModel model)
        {
            int? goalieID = null;
            double min = double.MaxValue;
            foreach (int item in model.Opponents.Keys)
            {
                if (model.Opponents[item].Location.DistanceFrom(GameParameters.OppGoalCenter) < min)
                {
                    min = model.Opponents[item].Location.DistanceFrom(GameParameters.OppGoalCenter);
                    goalieID = item;
                }
            }
            return goalieID;
        }
        public Position2D BestPointInGoal(WorldModel model, int GoalieID)
        {
            Position2D bestPoint = new Position2D();
            double threshhold = 0.03;
            SingleObjectState goalieState = model.Opponents[GoalieID];
            Vector2D robotLeftVector = new Vector2D(0, -RobotParameters.OurRobotParams.Diameter / 2);
            Vector2D robotRightVector = new Vector2D(0, RobotParameters.OurRobotParams.Diameter / 2);
            Position2D goalRightExtended = new Position2D(goalieState.Location.X, GameParameters.OppGoalRight.Y);
            Position2D goalLeftExtended = new Position2D(goalieState.Location.X, GameParameters.OppGoalLeft.Y);
            double rightSize = goalRightExtended.DistanceFrom(goalieState.Location + robotRightVector);
            double leftSize = goalLeftExtended.DistanceFrom(goalieState.Location + robotLeftVector);

            if (rightSize > leftSize + threshhold)
            {
                bestPoint = goalieState.Location + robotRightVector.GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + ((1.7 * rightSize) / 3));

            }
            else if (leftSize > rightSize + threshhold)
            {
                bestPoint = goalieState.Location + robotLeftVector.GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + ((1.7 * leftSize) / 3));
            }
            else
            {
                bestPoint = goalieState.Location + robotRightVector.GetNormalizeToCopy((RobotParameters.OurRobotParams.Diameter / 2) + ((1.7 * rightSize) / 3));
            }

            return bestPoint;
        }
        private static bool penaltyUpdateTimeFlag = true;
        public int PenaltyTime(int minTime, int maxTime, int Accumulative, bool isRandom)
        {
            if (!isRandom)
            {
                if (minTime > maxTime || penaltyUpdateTimeFlag)
                {
                    penaltyTime = minTime;

                }
                penaltyTime += Accumulative;
                return penaltyTime;
            }
            else
            {
                int time = randomTime.Next(minTime, maxTime);
                return time;
            }

        }
        public int PenaltyRandom(int minMood, int maxMood, int Accumulative, bool penaltyRandom)
        {
            if (!penaltyRandom)
            {
                if (minMood > maxMood || penaltyUpdateRandomFlag)
                {
                    penaltyRandomMood = minMood;
                }
                penaltyRandomMood += Accumulative;
                return penaltyRandomMood;
            }
            else
            {
                int penalty = randomPenalty.Next(minMood, maxMood);
                return penalty;
            }
        }

        private void anglePattern(WorldModel model, int RobotID)
        {
            Vector2D leftVector = GameParameters.OppLeftCorner - GameParameters.OppGoalCenter;
            Position2D left = GameParameters.OppGoalCenter + leftVector.GetNormalizeToCopy(leftVector.Size / 4);
            Vector2D leftRobotVector = left - model.OurRobots[RobotID].Location;
            Vector2D rightVector = GameParameters.OppRightCorner - GameParameters.OppGoalCenter;
            Position2D right = GameParameters.OppGoalCenter + rightVector.GetNormalizeToCopy(rightVector.Size / 4);
            Vector2D rightRobotVector = right - model.OurRobots[RobotID].Location;
            double leftAngle = (left - model.OurRobots[RobotID].Location).AngleInDegrees;
            double rightAngle = (right - model.OurRobots[RobotID].Location).AngleInDegrees;
            if (LeftAngleFlag)
            {
                targetAngle = leftAngle;

                if (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(model.OurRobots[RobotID].Angle.Value * (Math.PI / 180), 1), leftRobotVector)) < 1)
                {
                    LeftAngleFlag = false;
                }
            }
            else
            {
                targetAngle = rightAngle;
                if (Math.Abs(Vector2D.AngleBetweenInDegrees(Vector2D.FromAngleSize(model.OurRobots[RobotID].Angle.Value * (Math.PI / 180), 1), rightRobotVector)) < 2)
                {
                    LeftAngleFlag = true;
                }
            }
            nearTarget = model.BallState.Location.Extend(0.20, 0);
            nearTarget = secondTraget;
            //Planner.ChangeDefaulteParams(RobotID, false);
            //Planner.SetParameter(RobotID, 0.2, 0.3);

            Planner.Add(RobotID, nearTarget, targetAngle, PathType.UnSafe, true, true, false, false);
        }

        private List<targetPosition> TargetPatternMaker(bool isLeft, int repeatCounter, int totalCounter)
        {
            bool firstTime = true;
            List<targetPosition> targetAxis = new List<targetPosition>();
            for (int i = 0; i < totalCounter; i++)
            {
                if (isLeft)
                {
                    if (firstTime)
                    {
                        targetAxis.Add(targetPosition.left);

                    }

                    if (!firstTime)
                    {
                        if (targetAxis.Count < repeatCounter)
                        {
                            targetAxis.Add(targetPosition.left);
                        }
                        else if (targetAxis.LastOrDefault() == targetPosition.left && targetAxis.Count % repeatCounter == 0)
                        {
                            targetAxis.Add(targetPosition.right);
                        }
                        else if (targetAxis.LastOrDefault() == targetPosition.right && targetAxis.Count % repeatCounter == 0)
                        {
                            targetAxis.Add(targetPosition.left);
                        }
                        else if (targetAxis.Count % repeatCounter != 0)
                        {
                            targetAxis.Add(targetAxis.LastOrDefault());
                        }
                    }
                }
                else
                {
                    if (firstTime)
                    {
                        targetAxis.Add(targetPosition.right);
                    }
                    if (!firstTime)
                    {
                        if (targetAxis.Count < repeatCounter)
                        {
                            targetAxis.Add(targetPosition.right);
                        }
                        else if (targetAxis.LastOrDefault() == targetPosition.left && targetAxis.Count % repeatCounter == 0)
                        {
                            targetAxis.Add(targetPosition.right);
                        }
                        else if (targetAxis.LastOrDefault() == targetPosition.right && targetAxis.Count % repeatCounter == 0)
                        {
                            targetAxis.Add(targetPosition.left);
                        }
                        else if (targetAxis.Count % repeatCounter != 0)
                        {
                            targetAxis.Add(targetAxis.LastOrDefault());
                        }
                    }

                }
                firstTime = false;
            }
            return targetAxis;
        }
        public Position2D GoaliePos(WorldModel model)
        {
            double ballTarget = Math.Sqrt(((GameParameters.OppGoalLeft.X + 2.30) * (GameParameters.OppGoalLeft.X + 2.30)) + (GameParameters.OppGoalLeft.Y) * ((GameParameters.OppGoalLeft.Y)));
            double ballTimeFrame = (ballTarget / 8) * 60;
            double ballTime = (ballTarget / 8);
            goalieVec = model.Opponents[goalieID.Value].Speed * ballTime;
            goaliePosition = goalieVec + (model.Opponents[goalieID.Value].Location);
            return goaliePosition.Extend(0, 0.5);
        }

        public double calculateRoatateTime(WorldModel model, int RobotID)
        {
            double ballTarget = Math.Sqrt(((GameParameters.OppGoalLeft.X + 2.30) * (GameParameters.OppGoalLeft.X + 2.30)) + (GameParameters.OppGoalLeft.Y) * ((GameParameters.OppGoalLeft.Y)));
            double ballTime = (ballTarget / 8);
            double rotateTime = Planner.GetRotateTime(model, ballFirstLocation, GameParameters.OppGoalLeft)+ballTime;
            return rotateTime;
        }

        Random randomTime = new Random();
        Random randomPenalty = new Random();
        private int penaltyTime = 0;
        private static int penaltyRandomMood = 0;
        private bool firstTime = true;
        private bool firstTime2 = true;
        private bool firstTime3 = true;

        private bool goaliePositionFlag=true;


       
    }

}
