using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.AIConsole.Roles;
using MRL.SSL.Planning.MotionPlanner;

namespace MRL.SSL.AIConsole.Strategies
{
    public class QueirozTestStrategy : StrategyBase
    {
        const double step = 0.5, passerShooterDist = 2.2, passSpeedTresh = StaticVariables.PassSpeedTresh;
        const double tresh = 0.01, angleTresh = 2, waitTresh = 30, wait2Tresh = 0, finishTresh = 200, initDist = 0.22, maxWaitTresh = 240, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        const double BallMovedTresh = 0.07, distOneTouchTresh = 0.8;
        double margin = 0.25;
        const double distBehindBallTresh = 0.07;

        bool first;
        int PasserID, DefenderID, ShooterID, Poser1ID, Poser2ID, Poser3ID;
        Position2D firstBallPos, secondBallPos;
        bool passed;
        Position2D PasserPos;
        int counter, finishCounter, RotateDelay, timeLimitCounter, faildCounter;
        Syncronizer sync;
        Position2D Pos1, Pos2, Pos3;
        bool firstInState;
        Position2D DefenderPos, GoaliePos, ShooterPos;
        Position2D PassTarget, SecondPassTarget;
        Position2D ShootTarget;
        double PasserAngle, DefenderAng, GoalieAng, Poser1Ang, Poser2Ang, Poser3Ang, ShooterAng;
        bool inRotate;
        int rotateCounter;
        bool isChip, chipOrigin;
        double PassSpeed;
        bool inPassState;
        bool backSensor;
        double KickSpeed;
        

        public override void ResetState()
        {
            first = true;
            firstInState = true;
            PasserID = -1;
            DefenderID = -1;
            ShooterID = -1;
            Poser1ID = -1;
            Poser2ID = -1;
            Poser3ID = -1;
            firstBallPos = Position2D.Zero;
            passed = false;
            timeLimitCounter = 0;
            PasserPos = Position2D.Zero;
            Pos1 = Position2D.Zero;
            Pos2 = Position2D.Zero;
            Pos3 = Position2D.Zero;
            DefenderPos = Position2D.Zero;
            PassTarget = Position2D.Zero;
            ShootTarget = Position2D.Zero;
            GoaliePos = Position2D.Zero;
            ShooterPos = Position2D.Zero;

            DefenderAng = 0;
            GoalieAng = 0;
            Poser1Ang = 0;
            Poser2Ang = 0;
            Poser3Ang = 0;
            ShooterAng = 0;
            PasserAngle = 0;

            RotateDelay = 60;
            inRotate = false;
            rotateCounter = 2;
            isChip = false;
            chipOrigin = false;
            PassSpeed = 0;
            backSensor = true;
            inPassState = false;
            KickSpeed = 8;
            SecondPassTarget = Position2D.Zero;
            secondBallPos = Position2D.Zero;
            goOffend = false;
            goDefened = false;
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
            StrategyName = "QueirozTest";
            AttendanceSize = 6;
            About = "this strategy will attack from sides by long passes!";
        }

        public override bool IsFeasiblel(GameStrategyEngine engine, GameDefinitions.WorldModel Model, ref GameStatus Status)
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
            if (first)
            {
                firstBallPos = Model.BallState.Location;
                AssignIDs(engine, Model, Attendance);
                first = false;
            }
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            if ((PasserID != -1 && !Model.OurRobots.ContainsKey(PasserID)) || (ShooterID != -1 && !Model.OurRobots.ContainsKey(ShooterID))
                || (Poser1ID != -1 && !Model.OurRobots.ContainsKey(Poser1ID)) || !Model.OurRobots.ContainsKey(Poser2ID) || !Model.OurRobots.ContainsKey(Poser3ID))
                return;

            #region State
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < tresh
                    && Model.OurRobots[Poser1ID].Location.DistanceFrom(Pos1) < 0.1
                    && Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < 0.1
                    && Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos3) < 0.1
                    && Model.OurRobots[DefenderID].Location.DistanceFrom(DefenderPos) < 0.1)
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
            }
            else if (CurrentState == (int)State.FirstPass)
            {
                if (passed)
                    finishCounter++;
                if (finishCounter > finishTresh + wait2Tresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = PassTarget - PasserPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if (sync.CatchState == 1)
                    {

                    }
                    else
                    {
                        if ((v.Y < 0.3 && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) <= faildBallDistSecondPass))
                        {
                            faildCounter++;
                            if (faildCounter > faildMaxCounter)
                                CurrentState = (int)State.Finish;
                        }
                        else
                            faildCounter = Math.Max(0, faildCounter - 5);
                    }
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

                if (sync.CatchState == (int)CatchAndRotateBallRole.State.Rotate && ++counter > wait2Tresh)
                {
                    counter = 0;
                    CurrentState = (int)State.SecondPass;
                    firstInState = true;
                    secondBallPos = Model.BallState.Location;
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                Vector2D refrence = SecondPassTarget - secondBallPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed && (v.Y < 0.1 && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[PasserID].Location) <= faildBallDistSecondPass))
                {
                    faildCounter++;
                    if (faildCounter > faildMaxCounter)
                        CurrentState = (int)State.Finish;
                }
                else
                    faildCounter = Math.Max(0, faildCounter - 5);
            }
            #endregion

            CalculateDefenderInfo(engine, Model, out DefenderPos, out GoaliePos, out DefenderAng, out GoalieAng);
            double sgn = Math.Sign(firstBallPos.Y);
            if (CurrentState == (int)State.First)
            {
                if (firstInState)
                {
                    ShootTarget = GameParameters.OppGoalCenter;
                    PasserPos = Model.BallState.Location + (Model.BallState.Location - ShootTarget).GetNormalizeToCopy(initDist);
                    PasserAngle = (ShootTarget - PasserPos).AngleInDegrees;
                    PassTarget = new Position2D(0.77, -sgn * 2.2);
                    SecondPassTarget = GameParameters.OppGoalCenter;//new Position2D(-GameParameters.OurGoalCenter.X / 2, sgn * Math.Abs(GameParameters.OurLeftCorner.Y) / 2);
                    Pos1.X = 3.23;
                    Pos1.Y = firstBallPos.Y;
                    Pos2.X = -0.48;
                    Pos2.Y = -sgn * 0.98;
                    Pos3.X = -3.3;
                    Pos3.Y = sgn * 0.9;
                    Pos3 = GameParameters.OppGoalCenter + (Pos3 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-Pos3, Vector2D.Zero, 0), margin));

                    firstInState = false;
                }

            }
            else if (CurrentState == (int)State.FirstPass)
            {
                if (firstInState)
                {
                    Planner.SetReCalculateTeta(PasserID, true);
                    firstInState = false;
                    ShooterID = Poser1ID;
                    Poser1ID = -1;
                    Circle c = new Circle(Model.BallState.Location, 0.9);
                    double minDist = double.MaxValue; int oppid = -1;
                    foreach (var item in Model.Opponents.Keys)
                    {
                        if (c.IsInCircle(Model.Opponents[item].Location) && Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                        {
                            oppid = item;
                            minDist = Model.Opponents[item].Location.DistanceFrom(Model.BallState.Location);
                        }
                    }
                    if (oppid == -1)
                    {
                        Pos2 = new Position2D(Model.BallState.Location.X - .3, Model.BallState.Location.Y + (-1 * Math.Sign(Model.BallState.Location.Y) * .4));
                        Poser2Ang = 180;
                    }
                    else
                    {
                        Vector2D ballOppPerp = (Model.Opponents[oppid].Location - Model.BallState.Location).GetPerp();
                        if (Model.BallState.Location.Y < 0)
                            ballOppPerp *= -1;
                        Pos2 = Model.Opponents[oppid].Location + ballOppPerp.GetNormalizeToCopy(0.15);
                        Poser2Ang = (Model.Opponents[oppid].Location - Pos2).AngleInDegrees;
                    }
                }

                if (inPassState && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (!passed)
                {
                    isChip = chipOrigin;
                    if (!chipOrigin)
                    {
                        Obstacles obs = new Obstacles(Model);
                        obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                        isChip = obs.Meet(Model.BallState, new SingleObjectState(PassTarget, Vector2D.Zero, 0), 0.07);
                    }

                    PassSpeed = 4.5;// engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, false, false);
                    if (isChip)
                        PassSpeed = Model.BallState.Location.DistanceFrom(PassTarget) * 0.5;
                }

            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (firstInState)
                {
                    int tmp = DefenderID;
                    DefenderID = PasserID;
                    PasserID = ShooterID;
                    ShooterID = tmp;

                    passed = false;
                    inPassState = false;
                    firstInState = false;
                }
                if (sync.CatchKicked && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (!passed)
                {
                    isChip = chipOrigin = true;

                    PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, PassTarget, isChip, true);
                    

                    if (isChip)
                        PassSpeed = Model.BallState.Location.DistanceFrom(SecondPassTarget) * 0.55;
                    //Console.WriteLine("PassSpeed :" + PassSpeed);
                }
                else
                {
                    //Console.WriteLine("PassSpeed :" + Model.BallState.Speed.Size);
                }
            }
            if (CurrentState == (int)State.SecondPass || passed)
            {
                Pos2 = new Position2D(-GameParameters.OurGoalCenter.X * 0.5, -sgn * (Math.Abs(GameParameters.OurLeftCorner.Y) / 2));
                Pos3.Y = -sgn * 1.1;
                Pos3 = GameParameters.OppGoalCenter + (Pos3 - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-Pos3, Vector2D.Zero, 0), margin));

                Pos1 = new Position2D(-0.7, -sgn * 0.2);
                ShooterPos = new Position2D(-GameParameters.OurGoalCenter.X * 0.6, sgn * (Math.Abs(GameParameters.OurLeftCorner.Y) - 0.4));

                if (DefenderID != -1 && Model.OurRobots[DefenderID].Location.DistanceFrom(Pos1) < 0.3)
                {
                    goDefened = true;
                }


            }

            if (CurrentState == (int)State.SecondPass)
            {
                if (ShooterID != -1 && (Model.OurRobots[ShooterID].Location.DistanceFrom(ShooterPos) < 0.1 || (Model.BallState.Location.DistanceFrom(Model.OurRobots[ShooterID].Location) < 0.5)))
                {
                    goOffend = true;
                }
            }

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
        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
        }
        private void AssignIDs(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> att)
        {
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
            DefenderID = minIdx;
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
            Poser3ID = minIdx;
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
            Poser1ID = tmpIds[0];
            ShooterID = -1;
        }
        bool goDefened = false, goOffend = false;

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Functions = new Dictionary<int, CommonDelegate>();
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            if (CurrentState == (int)State.First)
            {
                Planner.ChangeDefaulteParams(PasserID, false);
                Planner.SetParameter(PasserID, 3.5, 2);
                if (Planner.AddRotate(Model, PasserID, ShootTarget, PasserPos, kickPowerType.Speed, PassSpeed, isChip, rotateCounter, true).IsInRotateDelay)
                {
                    inRotate = true;
                    rotateCounter++;
                }
                Planner.Add(Poser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.Safe, true, true, true, true);

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                    Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, DefenderPos, DefenderAng);
                //Planner.Add(ShooterID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
            }
            else if (CurrentState == (int)State.FirstPass)
            {
                sync.CatchAndWait = true;
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.Safe, true, true, true, true);
                if (isChip)
                    sync.SyncChipCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, RotateDelay, false, kickPowerType.Speed, backSensor, rotateCounter);
                else
                    sync.SyncDirectCatch(engine, Model, PasserID, PasserPos, ShooterID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, false, kickPowerType.Speed, RotateDelay, rotateCounter);
                if (sync.InPassState)
                    inPassState = true;


                if (passed && sync.CatchState == 1)
                {
                    Planner.Add(DefenderID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                if (passed)
                {
                    Planner.Add(PasserID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);

                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                        Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, DefenderPos, DefenderAng);
                }

            }
            else if (CurrentState == (int)State.SecondPass)
            {
                sync.CatchAndWait = false;
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.Safe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.Safe, true, true, true, true);
                if (isChip)
                    sync.SyncChipCatch(engine, Model, DefenderID, PasserPos, PasserID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, RotateDelay, false, kickPowerType.Speed, backSensor, rotateCounter);
                else
                    sync.SyncDirectCatch(engine, Model, DefenderID, PasserPos, PasserID, PassTarget, SecondPassTarget, PassSpeed, PassSpeed, false, kickPowerType.Speed, RotateDelay, rotateCounter);

                if (!goDefened)
                {
                    Planner.Add(DefenderID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, DefenderID, typeof(DefenderCornerRole1)))
                        Functions[DefenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(DefenderID).Run(eng, wmd, DefenderID, DefenderPos, DefenderAng);
                }
                if (!goOffend)
                {
                    Planner.Add(ShooterID, ShooterPos, (ShootTarget - ShooterPos).AngleInDegrees, PathType.Safe, true, true, true, true);
                }
                else
                {
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, ShooterID, typeof(ActiveRole)))
                        Functions[ShooterID] = (eng, wmd) => GetRole<ActiveRole>(ShooterID).Perform(eng, Model, ShooterID, null);

                }
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, GoaliePos, GoalieAng, new DefenceInfo(), DefenderPos, DefenderID, true);

            PreviouslyAssignedRoles = CurrentlyAssignedRoles;
            return CurrentlyAssignedRoles;
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
