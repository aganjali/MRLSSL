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
    class SimpleShootStrategy : StrategyBase
    {
        bool firstSetParameters = true;
        bool flag1 = true, flag2 = true, flag3 = true;
        double PassSpeed = 2.5;
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
        Position2D pos11, pos22, pos33, pos111, pos222, pos333;

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
            RotateTeta = 90;
            PassSpeed = 2.5;
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
            RotateDelay = 20;

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
            // int a = Model.OurRobots.Count;
            UseOnlyInMiddle = false;
            StrategyName = "F_SimpleShoot_Middle";
            AttendanceSize = 5;
            UseInMiddle = false;
            About = " Start of Middle!";
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

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {
            if (firstSetParameters)
            {
                firstBallPos = Model.BallState.Location;
                PasserPos = GameParameters.OppGoalCenter + (Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(Model.BallState.Location.DistanceFrom(GameParameters.OppGoalCenter) + 0.3);
                Vector2D FirstPos = (Model.BallState.Location - GameParameters.OppGoalCenter);
                Vector2D Shootpos = (Position2D.Zero - GameParameters.OppGoalCenter);

                Pos1 = GameParameters.OppGoalCenter.Extend(+1.5, 0);

                if (firstBallPos.Y < 0/*Right*/)
                {

                    Pos2 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians - (5 * Math.PI / 180) < 180) ? ((FirstPos.AngleInRadians - (5 * Math.PI / 180)) * -1) : (360 - FirstPos.AngleInRadians - (5 * Math.PI / 180)) * -1, 3);
                    Pos3 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians - (40 * Math.PI / 180) < 180) ? ((FirstPos.AngleInRadians - (40 * Math.PI / 180)) * -1) : (360 - FirstPos.AngleInRadians - (40 * Math.PI / 180)) * -1, 2);
                    ShooterPos1 = Position2D.Zero.Extend(0, +0.75);
                    pos33 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians - (40 * Math.PI / 180) < 180) ? (FirstPos.AngleInRadians - (40 * Math.PI / 180)) : 360 - FirstPos.AngleInRadians - (40 * Math.PI / 180), 1.35);
                    pos11 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians - (20 * Math.PI / 180) < 180) ? (FirstPos.AngleInRadians - (20 * Math.PI / 180)) : 360 - FirstPos.AngleInRadians - (20 * Math.PI / 180), 1.35);
                    pos22 = new Position2D(Pos1.X, Pos1.Y - 0.5);
                    pos222 = new Position2D(pos22.X + (Math.Abs(1)), pos22.Y);
                    pos333 = new Position2D(pos11.X, pos11.Y - 1);
                    pos111 = new Position2D(pos33.X, pos33.Y - 1);
                    ShooterPos22 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((Shootpos.AngleInRadians - (30 * Math.PI / 180) < 180) ? ((FirstPos.AngleInRadians - (30 * Math.PI / 180)) * -1) : (360 - FirstPos.AngleInRadians - (30 * Math.PI / 180)) * -1, 2);
                }


                if (firstBallPos.Y > 0/*Left*/)
                {
                    Pos2 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians + (5 * Math.PI / 180)) * -1, 3);
                    Pos3 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians + (40 * Math.PI / 180)) * -1, 2);
                    ShooterPos1 = Position2D.Zero.Extend(0, -0.75);
                    pos33 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(FirstPos.AngleInRadians + (40 * Math.PI / 180), 1.35);
                    pos11 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(FirstPos.AngleInRadians + (20 * Math.PI / 180), 1.35);
                    pos22 = new Position2D(Pos1.X, Pos1.Y + 0.5);
                    ShooterPos22 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((FirstPos.AngleInRadians + (20 * Math.PI / 180)) * -1, 2.25);
                    pos222 = new Position2D(pos22.X + (Math.Abs(1)), pos22.Y);
                    pos333 = new Position2D(pos11.X, pos11.Y + 1);
                    pos111 = new Position2D(pos33.X, pos33.Y + 1);
                    ShooterPos22 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((Shootpos.AngleInRadians + (30 * Math.PI / 180)) * -1, 2);
                }
                ShootTarget = GameParameters.OppGoalCenter;
                //set robots id : passerid,shooterid,poser1id,poser2id,poser3id
                AssignIDs(engine, Model, Attendance);
                //if ids and positions
                if (shooterID != -1 && Poser2ID != -1 && Poser3ID != -1 && Posser1ID != -1 && PasserID != -1)
                    firstSetParameters = false;
                else
                    return;
            }

            #region DrawingObjects
            //DrawingObjects.AddObject(new Circle(PasserPos, 0.2), "Pos11dfg645ad64sdfg45");
            //DrawingObjects.AddObject(new StringDraw("passerpos", PasserPos), "ghdxuhdxjduhjdhjdhjd74jh");
            //DrawingObjects.AddObject(new Circle(Pos1, 0.2), "Pos11df6dasfg45");
            //DrawingObjects.AddObject(new StringDraw("Pos1", Pos1), "gh5jdhshsbhsdh47jh");
            //DrawingObjects.AddObject(new Circle(Pos2, 0.2), "Po./mlsdmn5");
            //DrawingObjects.AddObject(new StringDraw("pos2", Pos2), "g3216Wjh");
            //DrawingObjects.AddObject(new Circle(Pos3, 0.2), "4aazdsd45ad645asasdfg45");
            //DrawingObjects.AddObject(new StringDraw("Pos3", Pos3), "ghaAutuofaqQEqjh");

            //DrawingObjects.AddObject(new Circle(pos111, 0.2), "Pos324g45");
            //DrawingObjects.AddObject(new StringDraw("pos111" + pos111.ToString(), pos111), "g9jksaflh,898989898h");
            //DrawingObjects.AddObject(new Circle(pos222, 0.2), "Posnghbhvbakjkjlkujg45");
            //DrawingObjects.AddObject(new StringDraw("pos222", pos333), "gh232323218h");
            //DrawingObjects.AddObject(new Circle(pos333, 0.2), "Posnnknn jg45");
            //DrawingObjects.AddObject(new StringDraw("pos333", pos333), "ghddf9898h");

            //DrawingObjects.AddObject(new Circle(ShooterPos1, 0.2), "Possdhdhey45");
            //DrawingObjects.AddObject(new StringDraw("ShooterPos1", ShooterPos1), "ghgjdjh");
            //DrawingObjects.AddObject(new Circle(ShooterPos22, 0.2), "Pkfjlskdoy45");
            //DrawingObjects.AddObject(new StringDraw("ShooterPos22", ShooterPos22), "ghgkdfjkkndjh");
            #endregion

            #region first

            if (CurrentState == (int)State.First)
            {
                //if all posers go to position : CurrentState = (int)State.Go
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < 0.1 &&
                    Model.OurRobots[Posser1ID].Location.DistanceFrom(Pos1) < 0.1 &&
                    Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < 0.1 &&
                    Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos3) < 0.1 &&
                    Model.OurRobots[shooterID].Location.DistanceFrom(ShooterPos1) < 0.1)
                    CurrentState = (int)State.Go;
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.07)
                    CurrentState = (int)State.Finish;

            }
            #endregion
            #region second old
            else if (CurrentState == (int)State.Go)
            {
                //if passed and shooter recieve shoot pose   : CurrentState = (int)State.finish
                if (Model.BallState.Speed.Size > 0.4)
                    Passed = true;
                if (Passed)
                    ++failcounter;
                if (Passed == true && failcounter > 60)
                    CurrentState = (int)State.Finish;
                if (Passed && Model.OurRobots[shooterID].Location.DistanceFrom(ShooterPos22) < 0.3 && Model.BallState.Location.DistanceFrom(ShooterPos22) < 0.5)
                    CurrentState = (int)State.Finish;
            }
            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            sync.kMotionChip = 1.2;
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            #region first
            if (CurrentState == (int)State.First)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                    Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(engine, Model, PasserID, ShootTarget, false, 0.30);
                Planner.Add(shooterID, ShooterPos1, (ShootTarget - ShooterPos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Posser1ID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, true, true, true);
            }
            #endregion
            #region second

            else if (CurrentState == (int)State.Go)
            {
                //sync passer and shoter
                sync.SyncChipPass(engine, Model, PasserID, RotateTeta, shooterID, firstBallPos + (ShooterPos22 - firstBallPos).GetNormalizeToCopy(ShooterPos22.DistanceFrom(firstBallPos) - 0.2), ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);
                if (Passed)
                {
                    Planner.Add(PasserID, GameParameters.OurGoalCenter + (Position2D.Zero - GameParameters.OurGoalCenter).GetNormalizeToCopy(1.5), 180, PathType.UnSafe, true, true, true, true);
                }
                //pistions
                #region pistions old
                if (!flag1)
                {
                    Planner.Add(Posser1ID, pos33, (ShootTarget - pos33).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    Planner.ChangeDefaulteParams(Posser1ID, false);
                    Planner.SetParameter(Posser1ID, 3);
                    if (Model.OurRobots[Posser1ID].Location.DistanceFrom(pos33) < 0.15)
                        flag1 = true;
                }
                else if (flag1)
                {
                    Planner.Add(Posser1ID, pos111, (ShootTarget - pos111).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    Planner.ChangeDefaulteParams(Posser1ID, false);
                    Planner.SetParameter(Posser1ID, 3);
                    if (Model.OurRobots[Posser1ID].Location.DistanceFrom(pos111) < 0.15)
                        flag1 = false;
                }
                //
                if (!flag2)
                {
                    Planner.Add(Poser2ID, pos22, (ShootTarget - pos22).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    Planner.ChangeDefaulteParams(Poser2ID, false);
                    Planner.SetParameter(Poser2ID, 2);
                    if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos22) < 0.15)
                        flag2 = true;
                }
                else if (flag2)
                {
                    Planner.Add(Poser2ID, pos222, (ShootTarget - pos222).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    Planner.ChangeDefaulteParams(Poser2ID, false);
                    Planner.SetParameter(Poser2ID, 2);
                    if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos222) < 0.15)
                        flag2 = false;
                }

                if (!flag3)
                {
                    Planner.Add(Poser3ID, pos11, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    Planner.ChangeDefaulteParams(Poser3ID, false);
                    Planner.SetParameter(Poser3ID, 2.5);
                    if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos11) < 0.15)
                        flag3 = true;
                }
                else if (flag3)
                {
                    Planner.Add(Poser3ID, pos333, (ShootTarget - pos333).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    Planner.ChangeDefaulteParams(Poser3ID, false);
                    Planner.SetParameter(Poser3ID, 2.5);
                    if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos333) < 0.15)
                        flag3 = false;
                }

                #endregion
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

        public enum State
        {
            First,
            Go,
            Finish
        }
    }
}

