using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;

namespace MRL.SSL.AIConsole.Strategies
{
    public class TurnAndRushStrategy:StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 120, finishTresh = 100, initDist = 0.22, maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;

        bool inrot = false, inPassState = false;
        bool first, passTargetCalculated, Debug = false, nearShooter, shooted;
        int PasserId, PositionerID2, PositionerID0, ShooterID, PositionerID1;
        Position2D PasserPos, PositionerPos2, PositionerPos1, PassTarget, ShootTarget, PositionerPos0, ShooterPos, firstBallPos, lastPositioner1Pos;
        Position2D tmpPos0;
        double PasserAngle, PickerAngle, PositionerAngle1, PositionerAngle2, RotateTeta, PassSpeed, KickPower, tmpAng0;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        Syncronizer sync;
        int Mode;
        bool chip, passed, chipOrigin, goOneTouch;
        bool backSensor;
        int ShooterAngle;
        double PositionerAngle0;
        bool firstInState;

        public override void ResetState()
        {
            firstInState = true;
            ShooterAngle = 0;
            tmpPos0 = Position2D.Zero;
            tmpAng0 = 0;
            backSensor = true;
            Mode = 0;
            goOneTouch = false;
            faildCounter = 0;
            nearShooter = false;
            shooted = false;
            chip = false;
            chipOrigin = false;
            passed = false;
            CurrentState = InitialState;
            first = true;
            passTargetCalculated = false;
            RotateTeta = 60;
            PassSpeed = 4;
            KickPower = Program.MaxKickSpeed;
            timeLimitCounter = 0;
            PasserId = -1;
            PositionerID2 = -1;
            PositionerID0 = 1;
            ShooterID = -1;
            PositionerID1 = -1;

            PositionerPos1 = Position2D.Zero;
            lastPositioner1Pos = Position2D.Zero;
            PositionerPos0 = Position2D.Zero;
            ShooterPos = Position2D.Zero;
            PasserPos = Position2D.Zero;
            PositionerPos2 = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = GameParameters.OppGoalCenter;
            firstBallPos = Position2D.Zero;

            PasserAngle = 0;
            PositionerAngle0 = 0;
            PositionerAngle2 = 0;
            PositionerAngle1 = 0;

            counter = 0;
            finishCounter = 0;
            RotateDelay = 60;
            inPassState = false;

            if (sync != null)
            {
                sync.Reset();
            }
            else
            {
                sync = new Syncronizer();
            }
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
            StrategyName = "TurnAndRush";
            AttendanceSize = 6;
            About = "this strategy will turn around the opp defense!";
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
        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
        }
        private void CalculateDefenderInfo(GameStrategyEngine engine, WorldModel Model, out Position2D defPos, out Position2D goalipos, out double defAng, out double goaliang)
        {
            List<DefenderCommand> defendcommands = new List<DefenderCommand>();
            int? id = null;
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(GoalieCornerRole)
            });
            defendcommands.Add(new DefenderCommand()
            {
                RoleType = typeof(DefenderCornerRole1),
                OppID = engine.GameInfo.OppTeam.Scores.Count > 0 ? engine.GameInfo.OppTeam.Scores.ElementAt(0).Key : id
            });
            var infos = FreekickDefence.Match(engine, Model, defendcommands, true);
            var gol = infos.SingleOrDefault(s => s.RoleType == typeof(GoalieCornerRole));
            var normal1 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole1));
            goalipos = (gol.DefenderPosition.HasValue) ? gol.DefenderPosition.Value : Position2D.Zero;
            goaliang = gol.Teta;
            defPos = (normal1.DefenderPosition.HasValue) ? normal1.DefenderPosition.Value : Position2D.Zero;
            defAng = normal1.Teta;
        }
        public override void DetermineNextState(GameStrategyEngine engine, GameDefinitions.WorldModel Model)
        {
            #region First
            if (first)
            {
                var tmpIds = RemoveGoaliID(Model, Attendance);
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
                PasserId = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (-Model.OurRobots[item].Location.X < minDist)
                    {
                        minDist = -Model.OurRobots[item].Location.X;
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                ShooterID = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID0 = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID1 = minIdx;

                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                PositionerID2 = minIdx;
                tmpIds.Remove(minIdx);
                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion
          
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if (!Model.OurRobots.ContainsKey(PasserId) || !Model.OurRobots.ContainsKey(ShooterID)
                || !Model.OurRobots.ContainsKey(PositionerID0) || !Model.OurRobots.ContainsKey(PositionerID1) || !Model.OurRobots.ContainsKey(PositionerID2))
                return;

            #region States
              if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserId].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[PositionerID0].Location.DistanceFrom(PositionerPos0) < 0.1
                    && Model.OurRobots[PositionerID1].Location.DistanceFrom(PositionerPos1) < 0.1
                    && Model.OurRobots[PositionerID2].Location.DistanceFrom(PositionerPos2) < 0.1)
                    counter++;

                if (counter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.Go;
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
            }
            else if (CurrentState == (int)State.Go)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = PassTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if ((v.Y < 0.1 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistSecondPass))
                    {
                        faildCounter++;
                        if (faildCounter > faildMaxCounter)
                            CurrentState = (int)State.Finish;
                    }
                    else
                        faildCounter = Math.Max(0, faildCounter - 5);
                }
                else
                {
                    if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist && Model.BallState.Location.DistanceFrom(firstBallPos) < maxFaildMovedDist)
                    {
                        faildCounter++;
                        if (faildCounter > 3)
                            CurrentState = (int)State.Finish;
                    }
                    else
                        faildCounter = Math.Max(0, faildCounter - 1);
                }

                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.3)
                    nearShooter = true;
                if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[ShooterID].Location - Model.OurRobots[PasserId].Location) <= 0)
                    shooted = true;
                if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > 0.35)
                    CurrentState = (int)State.Finish;
            }
            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
        }
        enum State
        {
            First,
            Pick,
            Go,
            Finish
        }
    }
}
