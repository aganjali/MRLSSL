using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.GameDefinitions;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.AIConsole.Roles;


namespace MRL.SSL.AIConsole.Strategies
{
    class Predator12 :StrategyBase
    {
        bool firstSetParameters = true;
        bool flag1 = true, flag2 = true, flag3 = true;
        double PassSpeed = 2;
        const double tresh = 0.01;
        int PasserID, Posser1ID, shooterID, Poser2ID, Poser3ID;
        Position2D PasserPos, ShooterPos1, ShootTarget, Pos1, Pos2, Pos3, ShooterPos22;
        double PasserAngle, ShooterAngle, RotateTeta, KickPower = Program.MaxKickSpeed;
        int RotateDelay, failcounter;
        Syncronizer sync;
        bool chip, Passed = false;
        Position2D firstBallPos;
        bool backSensor;
        List<Position2D> BallPos = new List<Position2D>();
        List<Vector2D> VecPos = new List<Vector2D>();
        Position2D pos11, pos22, pos33,pos111, pos222, pos333;

        public override void ResetState()
        {
            Passed = false;
            firstSetParameters = true;
            flag1 = flag2 = flag3 = true;
            //
            UseInMiddle = true;
            backSensor = false;
            firstBallPos = Position2D.Zero;
            chip = false;
            failcounter = 0;
            CurrentState = InitialState;
            RotateTeta = 0;
            PassSpeed = 2;
            KickPower = 100;
            PasserID = -1;
            Posser1ID = -1;
            shooterID = -1;
            Poser2ID = -1;
            Poser3ID = -1;

            PasserPos = new Position2D();
            ShooterPos1 = new Position2D();
            ShootTarget = new Position2D();

            PasserAngle = 0;
            ShooterAngle = 0;
            RotateDelay = 0;

            inrot = false;
            sync = new Syncronizer();

        }

        public override void InitializeStates(GameStrategyEngine engine, GameDefinitions.WorldModel Model, Dictionary<int, GameDefinitions.SingleObjectState> attendance)
        {
            Attendance = attendance;
            CurrentState = (int)State.First;
            InitialState = 0;
            FinalState = 2;
            TrapState = 2;
        }

        public override void FillInformation()
        {
            //int ID = Model.OurRobots.Count;
            UseOnlyInMiddle = false;
            StrategyName = "F_PredatorStrategy_Corner";
            AttendanceSize = 5;
            UseInMiddle = false;
            About = " Start of Corner !";
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
            if (firstSetParameters)
            {
                firstBallPos = Model.BallState.Location;
                //first positioning : pos1,pos2,pos3,pos4,pos5 
                PasserPos = GameParameters.OppGoalCenter + (Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(Model.BallState.Location.DistanceFrom(GameParameters.OppGoalCenter) + 0.3);

                ShooterPos1 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((Position2D.Zero - GameParameters.OppGoalCenter).AngleInRadians, 5);
                Pos1 = firstBallPos + Vector2D.FromAngleSize((Position2D.Zero - firstBallPos).AngleInRadians, 0.8);
                Pos2 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((GameParameters.OppLeftCorner - GameParameters.OppGoalCenter).AngleInRadians + 70 * Math.PI / 180, (3));
                Pos3 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((GameParameters.OppRightCorner - GameParameters.OppGoalCenter).AngleInRadians - 70 * Math.PI / 180, (3));
                Vector2D FirstPos = (Position2D.Zero - GameParameters.OppGoalCenter);
               

                //second positioning : pos11,pos22,pos33,pos44,pos55

                pos22 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((GameParameters.OppLeftCorner - GameParameters.OppGoalCenter).AngleInRadians + 30 * Math.PI / 180, (2.3));

                pos33 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((GameParameters.OppRightCorner - GameParameters.OppGoalCenter).AngleInRadians - 30 * Math.PI / 180, (2.3));
                   pos333 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians + (60 * Math.PI / 180)) * 1, 1.25);
                 
               
                    pos222 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians + (60 * Math.PI / 180)) *- 1, 1.25);
                 

               
                //pos222 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((GameParameters.OppLeftCorner - GameParameters.OppGoalCenter).AngleInRadians + 20 * Math.PI / 180, (1.55));

                //pos333 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((GameParameters.OppRightCorner - GameParameters.OppGoalCenter).AngleInRadians - 30* Math.PI / 180, (1.55));

                ShooterPos22 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((Position2D.Zero - GameParameters.OppGoalCenter).AngleInRadians , (1.55));




                ShootTarget = GameParameters.OppGoalCenter;
                //set robots id : passerid,shooterid,poser1id,poser2id,poser3id
                AssignIDs(engine, Model, Attendance);
                //if ids and positions
                if (shooterID != -1 && Poser2ID != -1 && Poser3ID != -1 && Posser1ID != -1 && PasserID != -1)
                    firstSetParameters = false;
                else
                    return;

            }



            #region first

            if (CurrentState == (int)State.First)
            {
                //if all posers go to position : CurrentState = (int)State.Go
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < 0.1 &&
                    Model.OurRobots[shooterID].Location.DistanceFrom(ShooterPos1) < 0.1 &&
                    Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < 0.1 &&
                    Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos3) < 0.1 &&
                    Model.OurRobots[Posser1ID].Location.DistanceFrom(Pos1) < 0.1)
                    CurrentState = (int)State.Go;
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.07)
                    CurrentState = (int)State.Finish;

            }

            #endregion

            #region second

            else if (CurrentState == (int)State.Go)
            {
                //if passed and shooter recieve shoot pose   : CurrentState = (int)State.finish
                if (Model.BallState.Speed.Size > 0.4)
                    Passed = true;
                if (Passed)
                    ++failcounter;
                if (Passed == true && failcounter > 45 && Model.OurRobots[shooterID].Location.DistanceFrom(ShooterPos22) < 0.5)
                    CurrentState = (int)State.Finish;
                if (Passed && Model.OurRobots[shooterID].Location.DistanceFrom(ShooterPos22) < 0.3 && Model.BallState.Location.DistanceFrom(ShooterPos22) < 0.5)
                    CurrentState = (int)State.Finish;

            }

            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            //sync.kMotionChip = 1.2;
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            #region first
            if (CurrentState == (int)State.First)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                    Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(engine, Model, PasserID, ShootTarget, false, 0.25);
                Planner.Add(Posser1ID, Pos1, (ShootTarget - ShooterPos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(shooterID, ShooterPos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, true, true, true);
            }
            #endregion
            #region second

            else if (CurrentState == (int)State.Go)
            {
                PassSpeed = (Model.OurRobots[PasserID].Location.DistanceFrom(ShooterPos22) - 1);
                DrawingObjects.AddObject(new StringDraw(PassSpeed.ToString(), Position2D.Zero.Extend(-3, 0)));
                sync.SyncChipPass(engine, Model, PasserID, RotateTeta, shooterID, firstBallPos + (ShooterPos22 - firstBallPos).GetNormalizeToCopy(ShooterPos22.DistanceFrom(firstBallPos) - 0.2), ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);
              
               // sync.SyncChipPass(engine, Model, PasserID, RotateTeta, shooterID, firstBallPos + (ShooterPos22 - firstBallPos).GetNormalizeToCopy(ShooterPos22.DistanceFrom(firstBallPos) - 0.2), ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);
                if (Passed)
                {
                    Planner.Add(PasserID, GameParameters.OurGoalCenter + (Position2D.Zero - GameParameters.OurGoalCenter).GetNormalizeToCopy(1.5), 180, PathType.UnSafe, true, true, true, true);
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Posser1ID, typeof(DefenderCornerRole1)))
                        Functions[Posser1ID] = (eng, wmd) => GetRole<DefenderCornerRole1>(Posser1ID).Run(engine, Model, Posser1ID, GameParameters.OurGoalCenter.Extend(-2, -.5), 0.30);

                }
                //pistions && MOVEING
                #region MOVEING poser3id
               
                if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos33) < 0.15)
                {
                    //flag2 = true;
                    flag1 = false;
                }

                if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos333) < 0.70)
                {
                    // flag3 = true;
                    flag2 = false;
                }
                if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos333) < 0.15)
                {
                    flag3 = false;
                }
                if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos33) > 0.15 && flag1)
                {
                    Planner.Add(Poser3ID, pos33, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);


                }
                else if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos333) > 0.70 && flag2)
                {
                    Planner.Add(Poser3ID,pos333, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                }
                else if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos333) > 0.10 && flag3)
                {
                    Planner.Add(Poser3ID, pos333, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                }
                #endregion

                #region MOVEING poser2id

                if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos22) < 0.15)
                {
                    //flag2 = true;
                    flag1 = false;
                }

                if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos222) < 0.70)
                {
                    // flag3 = true;
                    flag2 = false;
                }
                if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos222) < 0.15)
                {
                    flag3 = false;
                }
                if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos22) > 0.15 && flag1)
                {
                    Planner.Add(Poser2ID, pos22, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);


                }
                else if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos222) > 0.70 && flag2)
                {
                    Planner.Add(Poser2ID, pos222, (ShootTarget - pos22).AngleInDegrees, PathType.UnSafe, true, true, true, true);


                }
                else if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos222) > 0.10 && flag3)
                {
                    Planner.Add(Poser2ID, pos222, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);

                }
                #endregion

                Planner.Add(shooterID, ShooterPos22, (PasserPos - ShooterPos22).AngleInDegrees, PathType.UnSafe, true, true, true, true);

                //Planner.Add(Posser1ID, Pos1, (ShootTarget - ShooterPos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);

               


                //if (flag2)
                //{
                //    Planner.Add(Posser1ID, pos33, (ShootTarget - pos22).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                //    Planner.ChangeDefaulteParams(Posser1ID, false);
                //    Planner.SetParameter(Posser1ID, 0.5);
                //    if (Model.OurRobots[Posser1ID].Location.DistanceFrom(pos33) < 0.15)
                //        flag2 = true;
                //}

            }
            #endregion
            return CurrentlyAssignedRoles;
        }
        bool inrot = false;
        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
        }
        private void AssignIDs(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> att)
        {
            firstBallPos = Model.BallState.Location;
            var tmpIds = RemoveGoaliID(Model, att);
            double minDist = double.MaxValue;
            int minIdx = -1;
            foreach (var item in tmpIds)
            {

                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                {
                    minDist = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            PasserID = minIdx;
            tmpIds.Remove(minIdx);

            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && -Model.OurRobots[item].Location.X < minDist)
                {
                    minDist = -Model.OurRobots[item].Location.X;
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Posser1ID = minIdx;
            tmpIds.Remove(minIdx);

            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X < minDist)
                {
                    minDist = Model.OurRobots[item].Location.X;
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            shooterID = minIdx;
            tmpIds.Remove(minIdx);

            minDist = double.MaxValue;
            minIdx = -1;
            foreach (var item in tmpIds)
            {
                if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X < minDist)
                {
                    minDist = Model.OurRobots[item].Location.X;
                    minIdx = item;
                }
            }
            if (minIdx == -1)
                return;
            Poser2ID = minIdx;
            tmpIds.Remove(minIdx);
            if (tmpIds.Count == 0 || !Model.OurRobots.ContainsKey(tmpIds[0]))
                return;
            Poser3ID = tmpIds[0];
        }
      
        public enum State
        {
            First,
            Go,
            Finish
        }
    }
}
