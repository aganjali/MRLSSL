using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.AIConsole.Skills;
namespace MRL.SSL.AIConsole.Strategies
{
    public class CrazyRobots : StrategyBase
    {
        #region variables
        const double step = 0.5, passerShooterDist = 2.2, passSpeedTresh = StaticVariables.PassSpeedTresh;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 15, wait2Tresh = 60, finishTresh = 350, initDist = 0.22, maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 20, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;
        bool first = true, passer2Flag = false;
        int PasserID, ShooterID, Poser1ID, Passer2ID;
        Position2D firstBallPos;
        bool passed;
        Position2D PasserPos;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter, waitCounter, waitCounter2;
        Syncronizer sync;
        Position2D PasserPos1, PasserPos2, ShooterPos1, ShooterPos2, Poser1Pos1, Poser1Pos2, Passer2Pos1, Passer2Pos2, Passer2FakePos;
        Position2D oppRobotStopCoverPos;
        bool firstInState;
        Position2D DefenderPos, GoaliePos, ShooterPos;
        Position2D PassTarget, SecondPassTarget;
        Position2D ShootTarget;
        double PasserAngle, DefenderAng, GoalieAng, Poser1Ang, Poser2Ang, Poser3Ang, ShooterAng;
        bool inRotate;
        int rotateCounter;
        bool isChip, chipOrigin;
        bool inPassState;
        bool backSensor;
        double KickSpeed;
        int Mode, sgn, nearestRobotIDToBall, nearestRobotIDToPasser2Pos2;
        List<int> oppRobotKeys = new List<int>();
        Circle markShieldCircle1 = new Circle();
        Circle markShieldCircle2 = new Circle();

        StarkCatchSkill starkCatch;
        #endregion

        public override void ResetState()
        {

            PasserID = -1;
            ShooterID = -1;
            Passer2ID = -1;
            waitCounter = 0;
            waitCounter2 = 0;

            passed = false;
            isChip = false;

            PasserPos1 = Position2D.Zero;
            PasserPos2 = Position2D.Zero;
            ShooterPos1 = Position2D.Zero;
            ShooterPos2 = Position2D.Zero;
            Poser1Pos1 = Position2D.Zero;
            Poser1Pos2 = Position2D.Zero;
            Passer2Pos1 = Position2D.Zero;
            Passer2Pos2 = Position2D.Zero;
            Passer2FakePos = Position2D.Zero;
            oppRobotStopCoverPos = Position2D.Zero;

            firstBallPos = Position2D.Zero;
            RotateDelay = 120;
            sync = new Syncronizer();

            starkCatch = new StarkCatchSkill();
        }

        public override void InitializeStates(GameStrategyEngine engine, GameDefinitions.WorldModel Model, Dictionary<int, GameDefinitions.SingleObjectState> attendance)
        {
            Attendance = attendance;
            CurrentState = (int)State.First;
            InitialState = 0;
            FinalState = 3;
            TrapState = 3;
        }

        public override void FillInformation()
        {
            UseInMiddle = false;
            UseOnlyInMiddle = false;
            StrategyName = "V_4crazyRobots_Middle";
            AttendanceSize = 4;
            About = "this strategy will attack from sides by long passes!";
        }

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, ref GameDefinitions.GameStatus Status)
        {
            if (CurrentState == (int)State.Finish)
            {
                Status = GameStatus.Normal;
                return false;
            }
            return true;
        }

        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            #region init
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                sgn = Math.Sign(firstBallPos.Y);
                first = false;

                oppRobotKeys = oppRobotsFinder(Model);

                #region Find Positions
                Poser1Pos1 = Position2D.Zero.Extend(1, 0);
                Poser1Pos2 = setVectorizeSignedPos(GameParameters.OppGoalCenter, new Position2D(-2.82, 0.05));
                Passer2Pos1 = (sgn < 0) ? (GameParameters.OppLeftCorner + (firstBallPos - GameParameters.OppLeftCorner).GetNormalizeToCopy(1.5)).Extend(Math.Abs(firstBallPos.X / 2), 0)
                    : (GameParameters.OppRightCorner + (firstBallPos - GameParameters.OppRightCorner).GetNormalizeToCopy(1.5)).Extend(Math.Abs(firstBallPos.X / 2), 0);
                ShooterPos1 = setVectorizeSignedPos(GameParameters.OppGoalCenter, new Position2D(-0.84, 0.67), (firstBallPos - GameParameters.OppGoalCenter).Size - 0.15);
                ShooterPos2 = setVectorizeSignedPos(GameParameters.OppGoalCenter, new Position2D(-3.6, 1.66));
                Passer2FakePos = Poser1Pos2.Extend(2, 0);
                Passer2Pos2 = setVectorizeSignedPos(GameParameters.OppGoalCenter, new Position2D(-3, -1.83));

                #endregion
                Dictionary<int, SingleObjectState> temp = new Dictionary<int, SingleObjectState>();
                foreach (var item in Attendance)
                    temp.Add(item.Key, item.Value);

                PasserID = FindNearestRobotID(Model.BallState.Location, ref temp);
                Passer2ID = FindNearestRobotID(Passer2Pos1, ref temp);
                ShooterID = FindNearestRobotID(ShooterPos1, ref temp);
                Poser1ID = FindNearestRobotID(Poser1Pos1, ref temp);
            }

            //if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
            //    FreekickDefence.BallIsMovedStrategy = true;
            if ((PasserID != -1 && !Model.OurRobots.ContainsKey(PasserID)) || (ShooterID != -1 && !Model.OurRobots.ContainsKey(ShooterID))
                || (Passer2ID != -1 && !Model.OurRobots.ContainsKey(Passer2ID)) || !Model.OurRobots.ContainsKey(Passer2ID))
                return;
            #endregion
            #region states
            if (markShieldCircle1.Intersect(new Line(markShieldCircle1.Center, GameParameters.OurGoalCenter)).Count() > 1)
            {
                if (markShieldCircle1.Intersect(new Line(markShieldCircle1.Center, GameParameters.OurGoalCenter)).ElementAt(0).DistanceFrom(GameParameters.OurGoalCenter) <
                    markShieldCircle1.Intersect(new Line(markShieldCircle1.Center, GameParameters.OurGoalCenter)).ElementAt(1).DistanceFrom(GameParameters.OurGoalCenter))
                    PasserPos2 = markShieldCircle1.Intersect(new Line(markShieldCircle1.Center, GameParameters.OurGoalCenter)).ElementAt(0);
                else
                    PasserPos2 = markShieldCircle1.Intersect(new Line(markShieldCircle1.Center, GameParameters.OurGoalCenter)).ElementAt(1);
            }
            if (Model.OurRobots[Poser1ID].Location.DistanceFrom(Poser1Pos2) < 0.40)
            {
                if (markShieldCircle2.Intersect(new Line(markShieldCircle2.Center, GameParameters.OurGoalCenter)).Count() > 1)
                {
                    if (markShieldCircle2.Intersect(new Line(markShieldCircle2.Center, GameParameters.OurGoalCenter)).ElementAt(0).DistanceFrom(GameParameters.OurGoalCenter) <
                        markShieldCircle2.Intersect(new Line(markShieldCircle2.Center, GameParameters.OurGoalCenter)).ElementAt(1).DistanceFrom(GameParameters.OurGoalCenter))
                        Poser1Pos2 = markShieldCircle2.Intersect(new Line(markShieldCircle2.Center, GameParameters.OurGoalCenter)).ElementAt(0);
                    else
                        Poser1Pos2 = markShieldCircle2.Intersect(new Line(markShieldCircle2.Center, GameParameters.OurGoalCenter)).ElementAt(1);
                }
            }

            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < 0.15
                    && Model.OurRobots[Poser1ID].Location.DistanceFrom(Poser1Pos1) < 0.15
                    && Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < 0.15
                    && Model.OurRobots[Passer2ID].Location.DistanceFrom(Passer2Pos1) < 0.15)
                    counter++;


                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.FirstPass;
                    firstInState = true;
                    timeLimitCounter = 0;
                    counter = 0;
                }

                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    faildCounter++;
                else
                    faildCounter = Math.Max(faildCounter - 2, 0);
                if (faildCounter > 4)
                    CurrentState = (int)State.Finish;

                double minDist = double.MaxValue;
                foreach (var item in oppRobotKeys)
                {
                    if (Model.Opponents[item].Location.DistanceFrom(firstBallPos) < minDist)
                    {
                        minDist = Model.Opponents[item].Location.DistanceFrom(firstBallPos);
                        nearestRobotIDToBall = item;
                    }
                }
                minDist = double.MaxValue;
                foreach (var item in oppRobotKeys)
                {
                    if (Model.Opponents[item].Location.DistanceFrom(Passer2Pos2) < minDist)
                    {
                        minDist = Model.Opponents[item].Location.DistanceFrom(Passer2Pos2);
                        nearestRobotIDToPasser2Pos2 = item;
                    }
                }

                if (CurrentState == (int)State.FirstPass && (sync.Finished || sync.Failed))
                {
                    CurrentState = (int)State.Finish;
                }
                //var tempPos = ;


            }
            else if (CurrentState == (int)State.FirstPass && passed)
            {
                if (starkCatch.currentState == 3)
                {
                    CurrentState = (int)State.SecondPass;
                }
                //if (Model.BallState.Location.DistanceFrom(Model.OurRobots[Passer2ID].Location) > 1)
                //{
                //    CurrentState = (int)State.SecondPass;
                //}
                //if (starkCatch.currentState != 3 && starkCatch.currentState != 4)
                //{
                //    starkCatch.perform(engine, Model, Passer2ID, true, Passer2Pos2, true, 70);
                //}
            }
            markShieldCircle1 = new Circle(Model.Opponents[nearestRobotIDToBall].Location, 0.20);
            markShieldCircle2 = new Circle(Model.Opponents[nearestRobotIDToPasser2Pos2].Location, 0.20);

            //drawPos(Poser1Pos1, "Poser1Pos1", Color.FromArgb(1, 233, 146, 53));
            //drawPos(Poser1Pos2, "Poser1Pos2", Color.FromArgb(1, 233, 146, 53));
            //drawPos(Passer2Pos1, "Passer2Pos1", Color.FromArgb(1, 233, 146, 53));
            //drawPos(Passer2Pos2, "Passer2Pos2", Color.FromArgb(1, 233, 146, 53));
            //drawPos(ShooterPos1, "ShooterPos1", Color.FromArgb(1, 233, 146, 53));
            //drawPos(ShooterPos2, "ShooterPos2", Color.FromArgb(1, 233, 146, 53));
            //drawPos(Passer2FakePos, "passer fake pos", Color.FromArgb(1, 233, 146, 53));
            //drawPos(PasserPos2, "passerPos2", Color.FromArgb(1, 233, 146, 53));
            //drawPos(new Position2D(1, 0), waitCounter2.ToString());
            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {

            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();

            //if (firstBallPos.DistanceFrom(Model.BallState.Location) < 0.10)
            //{
            //    double PassSpeed = (firstBallPos.DistanceFrom(Passer2Pos2) - 2.7) * 1.08;
            //    Planner.AddRotate(Model, PasserID, Passer2Pos2, 20, kickPowerType.Speed, PassSpeed, isChip, RotateDelay, true);
            //}
            //else
            //    Planner.Add(PasserID, PasserPos2, (GameParameters.OppGoalCenter - Passer2Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
            #region states
            if (CurrentState == (int)State.First)
            {

                //Planner.ChangeDefaulteParams(PasserID, false);
                //Planner.SetParameter(PasserID, 3.5, 5);

                //Planner.Add(PasserID, PasserPos1, (Model.BallState.Location - Poser1Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser1ID, Poser1Pos1, (Model.BallState.Location - Poser1Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                //Planner.Add(PasserID, firstBallPos.Extend(0.20, 0), (GameParameters.OppGoalCenter - firstBallPos.Extend(-0.20, 0)).AngleInDegrees, PathType.Safe, true, true, true, true);

                
                if (Model.OurRobots[PasserID].Location.DistanceFrom(firstBallPos) < 0.30)
                {
                    double passSpeed = Model.BallState.Location.DistanceFrom(Passer2Pos1) * 0.65;
                    sync.SyncChipCatch(engine, Model, PasserID, 40, ShooterID, ShooterPos1, GameParameters.OppGoalCenter, passSpeed, KickSpeed, 20, false);
                    if (sync.Finished || sync.Failed)
                    {
                        CurrentState = (int)State.Finish;
                    }
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                        Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(engine, Model, PasserID, Position2D.Zero, false, 0.19);

                }
                Planner.Add(ShooterID, ShooterPos1, (GameParameters.OppRightCorner - ShootTarget).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Passer2ID, Passer2Pos1, (GameParameters.OppRightCorner - Passer2Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                
            }
            else if (CurrentState == (int)State.FirstPass)
            {
                KickSpeed = Program.MaxKickSpeed;
                isChip = true;


                //Planner.Add(Poser1ID, Poser1Pos2, (GameParameters.OppGoalCenter - Poser1Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                if (!passed)
                {
                    Planner.Add(Poser1ID, Poser1Pos2, (GameParameters.OppGoalCenter - Poser1Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);

                    waitCounter++;
                    if (waitCounter > 50)
                    {
                        Planner.Add(ShooterID, ShooterPos2, (GameParameters.OppGoalCenter - ShooterPos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                    }
                    if (waitCounter > 350 && starkCatch.currentState != 2)
                    {
                        CurrentState = (int)State.Finish;
                    }
                }


            }
            else if (CurrentState == (int)State.SecondPass)
            {
                //sync.kMotionChip = 1;
                Planner.Add(Poser1ID, Poser1Pos2, (GameParameters.OppGoalCenter - Poser1Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(PasserID, PasserPos2, (GameParameters.OppGoalCenter - PasserPos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                double passSpeed = Model.BallState.Location.DistanceFrom(ShooterPos2) - 1.1;
                sync.SyncChipPass(engine, Model, Passer2ID, 0, ShooterID, ShooterPos2, GameParameters.OppGoalCenter, passSpeed, 8, 0, false);

                //Planner.Add(ShooterID, ShooterPos2, (GameParameters.OppGoalCenter - ShooterPos2).AngleInDegrees, PathType.Safe, true, true, true, true);
            }
            #endregion
            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
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
        private void drawPos(Position2D centerPos, string posName)
        {
            drawPos(centerPos, posName, System.Drawing.Color.Black);
        }
        //private Position2D setVectorizeSignedPos(Position2D refrence/*,Position2D staticPoint*/, double distanceFromRefrence, double angle)
        //{
        //    Position2D myPt= new Position2D();
        //    Vector2D tmpVec = Vector2D.FromAngleSize(angle.ToDegree(), distanceFromRefrence);

        //    myPt = new Position2D((refrence + tmpVec).X, (refrence + tmpVec).Y) * sgn;
        //    return myPt;
        //}
        private Position2D setVectorizeSignedPos(Position2D refrence, Position2D staticPoint, double distanceFromRefrence)
        {
            Vector2D tmpVec = staticPoint - refrence;
            var tmpPos = refrence + tmpVec.GetNormalizeToCopy(distanceFromRefrence);
            return new Position2D(tmpPos.X, tmpPos.Y * sgn);
        }
        private Position2D setVectorizeSignedPos(Position2D refrence, Position2D staticPoint)
        {
            Vector2D tmpVec = staticPoint - refrence;
            var tmpPos = refrence + tmpVec;
            return new Position2D(tmpPos.X, tmpPos.Y * sgn);
        }
        private List<int> oppRobotsFinder(WorldModel model)
        {
            List<int> oppList = new List<int>();
            foreach (var item in model.Opponents.Keys)
            {
                oppList.Add(item);
            }
            return oppList;
        }
        enum State
        {
            First,
            FirstPass,
            SecondPass,
            Finish
        }
    }
}
