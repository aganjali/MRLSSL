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
    class PounderStrategy : StrategyBase
    {
        bool firstSetParameters = true;
        bool flag1 = true, flag2 = true, flag3 = true;
        double PassSpeed = 2;
        const double tresh = 0.01;
        int PasserID, Posser1ID, shooterID, Poser2ID, Poser3ID;
        Position2D PasserPos, ShooterPos1, ShootTarget, Pos, Pos1, Pos2, Pos3, ShooterPos22;
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
            RotateTeta = 0;
            PassSpeed = 3;
            KickPower = 150;
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
            //int ID = Model.OurRobots.Count;
            UseOnlyInMiddle = false;
            StrategyName = "F_PounderStrategy_Corner";
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

        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {
            if (firstSetParameters)
            {
                firstBallPos = Model.BallState.Location;
                //first positioning : pos1,pos2,pos3,pos4,pos5 
                PasserPos = GameParameters.OppGoalCenter + (Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(Model.BallState.Location.DistanceFrom(GameParameters.OppGoalCenter) + 0.3);
                Pos = Position2D.Zero;
                Pos1 = new Position2D(Pos.X,(Pos.Y+1.3));
                Pos2 = GameParameters.OppGoalCenter + (Position2D.Zero - GameParameters.OppGoalCenter).GetNormalizeToCopy(2);
                if (firstBallPos.Y < 0)
                {
                    Pos3 = new Position2D(Pos2.X, Pos2.Y + 1.5);
                    ShooterPos1 = new Position2D(Pos.X, Pos.Y + 2.5);

                    ShooterPos22 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((GameParameters.OppRightCorner - GameParameters.OppGoalCenter).AngleInRadians - 30 * Math.PI / 180, (1.9));
                }
                if (firstBallPos.Y > 0)
                {
                    Pos3 = new Position2D(Pos2.X, Pos2.Y - 1.5);
                    ShooterPos1 = new Position2D(Pos.X, Pos.Y - 2.5);
                    ShooterPos22 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize((GameParameters.OppLeftCorner - GameParameters.OppGoalCenter).AngleInRadians + 30 * Math.PI / 180, (1.9));
                }
                //second positioning : pos11,pos22,pos33,pos44,pos55
                double d = 1;
                Vector2D firstvec = (Position2D.Zero - GameParameters.OppGoalCenter).GetNormalizeToCopy(1.3);
                pos33 = GameParameters.OppGoalCenter + firstvec.GetNormalizeToCopy(1.5); ;
                pos333 = GameParameters.OppGoalCenter + firstvec.GetNormalizeToCopy(1.5 + d);

                pos22 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(firstvec.AngleInRadians + (20 * Math.PI / 180), 1.7);
                pos222 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(firstvec.AngleInRadians + (20 * Math.PI / 180), 1.7 + d);

                pos11 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(firstvec.AngleInRadians + (40 * Math.PI / 180), 2.1);
                pos111 = GameParameters.OppGoalCenter + Vector2D.FromAngleSize(firstvec.AngleInRadians + (40 * Math.PI / 180), 2.1+ d);
                if (firstBallPos.Y < 0)
                {
                    pos11 = new Position2D(pos11.X, -1 * pos11.Y);
                    pos111 = new Position2D(pos111.X, -1 * pos111.Y);

                    pos22 = new Position2D(pos22.X, -1 * pos22.Y);
                    pos222 = new Position2D(pos222.X, -1 * pos222.Y);

                    pos33 = new Position2D(pos33.X, -1 * pos33.Y);
                    pos333 = new Position2D(pos333.X, -1 * pos333.Y);
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

            //DrawingObjects.AddObject(new Circle(pos333, 0.2), "Pos11dfg65sdas6dasd46asdmn554aazdsd45ad645asasdfg45");
            //DrawingObjects.AddObject(new StringDraw("pos333", pos333), "Pos11dfg6asdd4asd5hghx,mnbvf464f555sd654g");

            //DrawingObjects.AddObject(new Circle(pos33, 0.2), "Pos11dfg65sdas6dasd46554aazdsd45ad645asasdfg45");
            //DrawingObjects.AddObject(new StringDraw("pos33", pos33), "Pos11dfg6asdd4asd5hghxf464f555sd654g");

            //DrawingObjects.AddObject(new Circle(pos222, 0.2), "Pos11dfg65sdas6d54aasd45as654gasd645asasdfg45");
            //DrawingObjects.AddObject(new StringDraw("pos222", pos222), "Pos11dfg6asdd4asd54as6zdf5sdasd65464f555sd654g");

            //DrawingObjects.AddObject(new Circle(pos22, 0.2), "Pos11dfg65sdas6d54aasd45as654gasdfg45");
            //DrawingObjects.AddObject(new StringDraw("pos22", pos22), "Pos11dfg6asdd4asd54as6zdf5sd64f555sd654g");

            //DrawingObjects.AddObject(new Circle(pos111, 0.2), "Pos11dfg65sdas6d54aasd45as654g");
            //DrawingObjects.AddObject(new StringDraw("Pos111", pos111), "Pos11dfg6asdd4asd54as6555sd654g");

            //DrawingObjects.AddObject(new Circle(pos11, 0.2), "Pos11dfg6dghf54h5sd654g");
            //DrawingObjects.AddObject(new StringDraw("Pos11", pos11), "Pos11dfg6asdds5df4g564dfg455sd654g");

            //DrawingObjects.AddObject(new Circle(ShooterPos22, 0.2), "Pos11dfg6dghfgewrgkiug8ug8ug8giujv8iuegd654g");
            //DrawingObjects.AddObject(new StringDraw("Pos11", pos11), "Pos11dfg6asdds5dflkirgjiegjuiegjigjhjgg455sd654g");

            //DrawingObjects.AddObject(new Circle(pos11, 0.2), "Pos11dfg6dghf54h5sd654g");
            //DrawingObjects.AddObject(new StringDraw("Pos11", pos11), "Pos11dfg6asdds5df4g564dfg455sd654g");



            #region first

            if (CurrentState == (int)State.First)
            {
                //if all posers go to position : CurrentState = (int)State.Go
                if (Model.OurRobots[PasserID].Location.DistanceFrom(PasserPos) < 0.2 &&
                    Model.OurRobots[shooterID].Location.DistanceFrom(Pos1) < 0.2 &&
                    Model.OurRobots[Poser2ID].Location.DistanceFrom(Pos2) < 0.2 &&
                    Model.OurRobots[Poser3ID].Location.DistanceFrom(Pos3) < 0.2 &&
                    Model.OurRobots[Posser1ID].Location.DistanceFrom(ShooterPos1) < 0.2)
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

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            //sync.kMotionChip = 1.2;
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();
            #region first
            if (CurrentState == (int)State.First)
            {
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, PasserID, typeof(ActiveRole)))
                    Functions[PasserID] = (eng, wmd) => GetRole<ActiveRole>(PasserID).PerformWithoutKick(engine, Model, PasserID, ShootTarget, false, 0.25);
                Planner.Add(Posser1ID, ShooterPos1, (ShootTarget - ShooterPos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(shooterID, Pos1, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser2ID, Pos2, (ShootTarget - Pos2).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                Planner.Add(Poser3ID, Pos3, (ShootTarget - Pos3).AngleInDegrees, PathType.UnSafe, true, true, true, true);
            }
            #endregion
            #region second

            else if (CurrentState == (int)State.Go)
            {
                PassSpeed = engine.GameInfo.CalculateKickSpeed(Model, PasserID, Model.BallState.Location, ShooterPos22, true, true);
                //PassSpeed = Math.Max(0.8, Model.OurRobots[PasserID].Location.DistanceFrom(ShooterPos22) * 0.6);

                //sync passer and shoter
                sync.SyncChipPass(engine, Model, PasserID, RotateTeta, shooterID, firstBallPos + (ShooterPos22 - firstBallPos).GetNormalizeToCopy(ShooterPos22.DistanceFrom(firstBallPos) - 0.2), ShootTarget, PassSpeed, KickPower, RotateDelay, backSensor);
                if (Passed)
                {
                    Planner.Add(PasserID, GameParameters.OurGoalCenter + (Position2D.Zero - GameParameters.OurGoalCenter).GetNormalizeToCopy(1.5), 180, PathType.UnSafe, true, true, true, true);
                }
                //pistions
                #region pistions
                if (!flag1)
                {
                    Planner.Add(Poser2ID, pos11, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos11) < 0.15)
                        flag1 = true;
                }
                else if (flag1)
                {
                    Planner.Add(Poser2ID, pos111, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (Model.OurRobots[Poser2ID].Location.DistanceFrom(pos111) < 0.15)
                        flag1 = false;
                }
                ////
                if (!flag2)
                {
                    Planner.Add(Poser3ID, pos22, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos22) < 0.15)
                        flag2 = true;
                }
                else if (flag2)
                {
                    Planner.Add(Poser3ID, pos222, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (Model.OurRobots[Poser3ID].Location.DistanceFrom(pos222) < 0.15)
                        flag2 = false;
                }
                ////
                if (!flag3)
                {
                    Planner.Add(Posser1ID, pos33, (ShootTarget - pos11).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (Model.OurRobots[Posser1ID].Location.DistanceFrom(pos33) < 0.1)
                        flag3 = true;
                }
                else if (flag3)
                {
                    Planner.Add(Posser1ID, pos333, (ShootTarget - Pos1).AngleInDegrees, PathType.UnSafe, true, true, true, true);
                    if (Model.OurRobots[Posser1ID].Location.DistanceFrom(pos333) < 0.1)
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


