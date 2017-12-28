using System;
using System.Collections.Generic;
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
    public class Attack5JustChipStrategy : StrategyBase
    {
        const double tresh = 0.03, angleTresh = 2, waitTresh = 40, waitTresh2 = 15, finishTresh = 100, initDist = 0.22, maxWaitTresh = 420, passSpeedTresh = StaticVariables.PassSpeedTresh, faildFarPassSpeedTresh = 0.3, faildNearPassSpeedTresh = -0.05, faildBallDistSecondPass = 0.5, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2;
        double RotateTeta, PassSpeed, KickPower, AngleT;
        bool first, firstPosCalculated, checkWays2Goal, moveDefender, gotPoses, chip, chipOrigin, passed, inRotate, Debug = false, ballMoved, recalcRot, nearShooter, shooted;
        Dictionary<int, int> open2kick;
        int defenderID, passerID, shooterID, RotateDelay, stateOpen2Goal, rotateCounter;
        Dictionary<int, int> positionersID;
        int initialPosCounter, timeLimitCounter, finishCounter;
        Position2D passerPos, goaliPos, defenderPos, shootTarget, passTarget;
        Dictionary<int, Position2D> positionersPos;
        Dictionary<int, Position2D> buPositionersPos;
        double passerAng, goaliAng, defenderAng;
        Dictionary<int, double> positionersAng;
        Dictionary<int, double> buPositionersAng;
        Dictionary<int, bool> chekWays;
        Syncronizer sync;
        GetBallSkill getBallSkill;
        Position2D defenderMidPoint, firstBallPos;
        bool defenderRushed, defenderGoSync;
        int faildCounter;
        bool near;
        bool backSensor;
        public override void ResetState()
        {
            backSensor = true;
            faildCounter = 0;
            firstBallPos = Position2D.Zero;
            nearShooter = false;
            shooted = false;
            defenderGoSync = false;
            recalcRot = false;
            near = false;
            getBallSkill = new GetBallSkill();
            defenderRushed = false;
            first = true;
            chip = false;
            chipOrigin = false;
            passed = false;
            checkWays2Goal = false;
            firstPosCalculated = false;
            moveDefender = false;
            ballMoved = false;
            open2kick = new Dictionary<int, int>();
            gotPoses = false;
            inRotate = false;
            shooterID = -1;
            defenderID = -1;
            passerID = -1;
            AngleT = 70;
            RotateTeta = 90;
            PassSpeed = 3.5;
            KickPower = Program.MaxKickSpeed;
            rotateCounter = 2;
            positionersID = new Dictionary<int, int>();
            chekWays = new Dictionary<int, bool>();
            defenderMidPoint = Position2D.Zero;
            chekWays[0] = false;
            chekWays[1] = false;
            chekWays[2] = false;

            positionersID[0] = -1;
            positionersID[1] = -1;
            positionersID[2] = -1;
            stateOpen2Goal = -1;
            positionersPos = new Dictionary<int, Position2D>();
            buPositionersPos = new Dictionary<int, Position2D>();

            goaliPos = Position2D.Zero;
            defenderPos = Position2D.Zero;
            shootTarget = Position2D.Zero;
            passTarget = Position2D.Zero;

            RotateDelay = 10;

            positionersAng = new Dictionary<int, double>();
            buPositionersAng = new Dictionary<int, double>();

            passerAng = 0;
            goaliAng = 0;
            defenderAng = 0;

            initialPosCounter = 0;
            timeLimitCounter = 0;
            finishCounter = 0;

            if (sync != null)
            {
                sync.Reset();
            }
            else
            {
                sync = new Syncronizer();
            }
        }

        public override void InitializeStates(GameStrategyEngine engine, WorldModel Model, Dictionary<int, SingleObjectState> attendance)
        {
            Attendance = attendance;
            CurrentState = (int)State.First;
            InitialState = 0;
            FinalState = 4;
            TrapState = 4;
        }

        public override void FillInformation()
        {
            StrategyName = "Attack5JustChipStrategy";
            AttendanceSize = 6;
            About = "this strategy attack opponents with 5 attacker with chip!";
        }

        public override bool IsFeasiblel(GameStrategyEngine engine, WorldModel Model, ref GameStatus Status)
        {
            if (CurrentState == (int)State.Finish)
            {
                Status = GameStatus.Normal;
                return false;
            }
            return true;
        }
        private void FindShooter(GameStrategyEngine eng, WorldModel wmd, Dictionary<int, int> ids, ref bool open1, ref bool open2, ref bool open3)
        {
            Obstacles obs = new Obstacles(wmd);
            obs.AddObstacle(1, 0, 0, 0, wmd.OurRobots.Keys.ToList(), (eng.GameInfo.OppTeam.GoaliID.HasValue && wmd.Opponents.ContainsKey(eng.GameInfo.OppTeam.GoaliID.Value)) ? new List<int>() { eng.GameInfo.OppTeam.GoaliID.Value } : null);
            double radi = 0.08;
            open1 = (wmd.markingStatesToTarget[ids[0]] == (int)MarkingType.Open2Direct)// || wmd.markingStatesToTarget[ids[0]] == (int)MarkingType.Open2Chip)
                || (wmd.markingStatesToTarget[ids[0]] == (int)(MarkingType.Open2Direct | MarkingType.Near) || wmd.markingStatesToTarget[ids[0]] == (int)(MarkingType.Open2Chip | MarkingType.Near));//(wmd.markingStatesToTarget[ids[0]] == (int)MarkingType.Open2Direct || wmd.markingStatesToTarget[ids[0]] == (int)MarkingType.Open2Chip);
            open2 = !obs.Meet(wmd.OurRobots[ids[1]], new SingleObjectState(GameParameters.OppGoalCenter, Vector2D.Zero, 0), radi);//(wmd.markingStatesToTarget[ids[1]] == (int)MarkingType.Open2Direct || wmd.markingStatesToTarget[ids[1]] == (int)MarkingType.Open2Chip)
            //|| (wmd.markingStatesToTarget[ids[1]] == (int)(MarkingType.Open2Direct | MarkingType.Near) || wmd.markingStatesToTarget[ids[1]] == (int)(MarkingType.Open2Chip | MarkingType.Near));
            open3 = !obs.Meet(wmd.OurRobots[ids[2]], new SingleObjectState(GameParameters.OppGoalCenter, Vector2D.Zero, 0), radi);//(wmd.markingStatesToTarget[ids[2]] == (int)MarkingType.Open2Direct || wmd.markingStatesToTarget[ids[2]] == (int)MarkingType.Open2Chip)
            //|| (wmd.markingStatesToTarget[ids[2]] == (int)(MarkingType.Open2Direct | MarkingType.Near) || wmd.markingStatesToTarget[ids[2]] == (int)(MarkingType.Open2Chip | MarkingType.Near));
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
            var infos = FreekickDefence.Match(engine, Model, defendcommands , true);
            var gol = infos.SingleOrDefault(s => s.RoleType == typeof(GoalieCornerRole));
            var normal1 = infos.SingleOrDefault(s => s.RoleType == typeof(DefenderCornerRole1));
            goalipos = (gol.DefenderPosition.HasValue) ? gol.DefenderPosition.Value : Position2D.Zero;
            goaliang = gol.Teta;
            defPos = (normal1.DefenderPosition.HasValue) ? normal1.DefenderPosition.Value : Position2D.Zero;
            defAng = normal1.Teta;
        }
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {

            #region First

            if (first)
            {
                double minDist = double.MaxValue;
                int minIdx = -1;
                var tmpIds = RemoveGoaliID(Model, Attendance);
                double maxDist = double.MinValue;
                int maxIdx = -1;
                foreach (var item in tmpIds)
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.X > maxDist)
                    {
                        maxDist = Model.OurRobots[item].Location.X;
                        maxIdx = item;
                    }
                }
                if (maxIdx == -1)
                    return;
                defenderID = maxIdx;
                tmpIds.Remove(maxIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                    {
                        minDist = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                passerID = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                positionersID[0] = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y) < minDist)
                    {
                        minDist = Math.Abs(Model.OurRobots[item].Location.Y - Model.BallState.Location.Y);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                positionersID[1] = minIdx;
                tmpIds.Remove(minIdx);
                minDist = double.MaxValue;
                minIdx = -1;
                foreach (var item in tmpIds.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location) < minDist)
                    {
                        minDist = Model.OurRobots[item].Location.DistanceFrom(Model.BallState.Location);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return;
                positionersID[2] = minIdx;

                open2kick[positionersID[0]] = 0;
                open2kick[positionersID[1]] = 0;
                open2kick[positionersID[2]] = 0;
                foreach (var item in positionersID)
                {
                    positionersPos[item.Value] = Position2D.Zero;
                    positionersAng[item.Value] = 0;
                }
                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            int id0 = positionersID[0], id1 = positionersID[1], id2 = positionersID[2];
            #region States
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[passerID].Location.DistanceFrom(Model.BallState.Location) < 0.25
                    && Model.OurRobots[id0].Location.DistanceFrom(positionersPos[id0]) < 0.3
                    && Model.OurRobots[id1].Location.DistanceFrom(positionersPos[id1]) < 0.3
                    && Model.OurRobots[id2].Location.DistanceFrom(positionersPos[id2]) < 0.3
                    && Model.OurRobots[defenderID].Location.DistanceFrom(defenderPos) < tresh
                    && (!Model.GoalieID.HasValue || !Model.OurRobots.ContainsKey(Model.GoalieID.Value) || Model.OurRobots[Model.GoalieID.Value].Location.DistanceFrom(goaliPos) < tresh))
                    initialPosCounter++;
                if (initialPosCounter > 50 || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.FakeMove;
                    checkWays2Goal = true;
                    timeLimitCounter = 0;
                    initialPosCounter = 0;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.FakeMove)
            {
                timeLimitCounter++;
                if (Model.OurRobots[id0].Location.DistanceFrom(positionersPos[id0]) < 0.23
                    && Model.OurRobots[id1].Location.DistanceFrom(positionersPos[id1]) < 0.23
                    && Model.OurRobots[id2].Location.DistanceFrom(positionersPos[id2]) < 0.23)
                    initialPosCounter++;

                if (initialPosCounter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    checkWays2Goal = true;
                    //if (open2kick[id0] != 1)
                        CurrentState = (int)State.FakeMoveBack;
                    //else
                    //{
                    //    CurrentState = (int)State.Go;

                    //    shooterID = id0;
                    //    stateOpen2Goal = 1;
                    //    positionersPos[id0] = buPositionersPos[id0];
                    //    positionersAng[id0] = buPositionersAng[id0];
                    //    positionersPos[id1] = buPositionersPos[id1];
                    //    positionersAng[id1] = buPositionersAng[id1];
                    //    positionersPos[id2] = buPositionersPos[id2];
                    //    positionersAng[id2] = buPositionersAng[id2];
                    //}
                    timeLimitCounter = 0;
                    initialPosCounter = 0;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.FakeMoveBack)
            {
                timeLimitCounter++;
                if (Model.OurRobots[id0].Location.DistanceFrom(positionersPos[id0]) < 0.5
                    && Model.OurRobots[id1].Location.DistanceFrom(positionersPos[id1]) < 0.5
                    && Model.OurRobots[id2].Location.DistanceFrom(positionersPos[id2]) < 0.5)
                    initialPosCounter++;
                if (initialPosCounter > waitTresh2 || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.Go;
                    timeLimitCounter = 0;
                    initialPosCounter = 0;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.Go)
            {
                if (passed)
                    finishCounter++;
                if (sync.Finished || sync.Failed || finishCounter > finishTresh)
                    CurrentState = (int)State.Finish;

                Vector2D refrence = passTarget - passerPos;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                if (passed)
                {
                    if (shooterID == positionersID[1] || shooterID == positionersID[2])
                    {
                        if ((v.Y < 0.1 && Model.BallState.Location.DistanceFrom(Model.OurRobots[shooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[shooterID].Location) <= faildBallDistSecondPass))
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
                        if ((v.Y < faildFarPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[shooterID].Location) > faildBallDistSecondPass) || (v.Y < faildNearPassSpeedTresh && Model.BallState.Location.DistanceFrom(Model.OurRobots[shooterID].Location) <= faildBallDistSecondPass))
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

                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[shooterID].Location) < 0.3)
                    nearShooter = true;
                if (nearShooter && Model.BallState.Speed.InnerProduct(Model.OurRobots[shooterID].Location - Model.OurRobots[passerID].Location) <= 0)
                    shooted = true;
                if (shooted && Model.BallState.Location.DistanceFrom(Model.OurRobots[shooterID].Location) > 0.35)
                    CurrentState = (int)State.Finish;

            }
            #endregion

            #region DefendersInfo
            CalculateDefenderInfo(engine, Model, out defenderPos, out goaliPos, out defenderAng, out goaliAng);
            //if (CurrentState == (int)State.FakeMove && !sync.SyncStarted)
            //{
            //    defenderPos = new Position2D(2.1, Math.Sign(-Model.BallState.Location.Y) * 1.7);
            //    defenderAng = (shootTarget - defenderPos).AngleInDegrees;
            //}
            #endregion

            #region PosAndAngles
            if (CurrentState == (int)State.First)
            {
                shootTarget = GameParameters.OppGoalCenter;
                passerPos = Model.BallState.Location + new Vector2D(0, Math.Sign(Model.BallState.Location.Y) * initDist);
                passerAng = (shootTarget - passerPos).AngleInDegrees;
                if (!firstPosCalculated)
                {
                    //double width = 0.6, heigth = 1, step = 0.2;
                    //Position2D topLeft = new Position2D(-0.8 - width, -heigth / 2);
                    //bool chip = false;
                    //Position2D bestPoint = Position2D.Zero;
                    //engine.GameInfo.BestPassPoint(Model, shootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref bestPoint);
                    positionersPos[id0] = new Position2D(-0.5, Math.Sign(Model.BallState.Location.Y) * 1.3); //bestPoint;
                    positionersAng[id0] = (shootTarget - positionersPos[id0]).AngleInDegrees;

                    positionersPos[id1] = new Position2D(-1.8, Math.Sign(Model.BallState.Location.Y) * 0.2); //bestPoint;
                    positionersAng[id1] = (shootTarget - positionersPos[id1]).AngleInDegrees;
                    positionersPos[id2] = positionersPos[id1] + new Vector2D(0, -Math.Sign(Model.BallState.Location.Y) * 0.3);//bestPoint + new Vector2D(0, 0.3);
                    positionersAng[id2] = (shootTarget - positionersPos[id2]).AngleInDegrees;
                    firstPosCalculated = true;
                }
                //Position2D tmpp = Model.BallState.Location + new Vector2D(1.5, -Math.Sign(Model.BallState.Location.Y) * 0.5);
                //positionersPos[id0] = new Position2D(Math.Sign(tmpp.X) * Math.Min(Math.Abs(tmpp.X), GameParameters.OurGoalCenter.X - 0.2), Math.Sign(tmpp.Y) * Math.Min(Math.Abs(tmpp.Y), GameParameters.OurLeftCorner.Y - 0.2));

                //positionersAng[id0] = (shootTarget - positionersPos[id0]).AngleInDegrees;

                buPositionersPos[id0] = positionersPos[id0];
                buPositionersPos[id1] = positionersPos[id1];
                buPositionersPos[id2] = positionersPos[id2];

                buPositionersAng[id0] = positionersAng[id0];
                buPositionersAng[id1] = positionersAng[id1];
                buPositionersAng[id2] = positionersAng[id2];
            }
            else if (CurrentState == (int)State.FakeMove)
            {
                shootTarget = GameParameters.OppGoalCenter;
                if (checkWays2Goal)
                {

                    bool[] btmp = new bool[3];
                    FindShooter(engine, Model, positionersID, ref btmp[0], ref btmp[1], ref btmp[2]);
                    open2kick[id0] = (btmp[0]) ? 1 : 0;
                    if (btmp[0])
                        passTarget = GetPassPoint(engine, Model);

                    open2kick[id1] = 0;// (btmp[1]) ? 1 : 0;
                    open2kick[id2] = 0;// (btmp[2]) ? 1 : 0;

                    checkWays2Goal = false;
                }
                positionersPos[id2] = new Position2D(-2.8, Math.Sign(-Model.BallState.Location.Y) * 1.3);
                positionersAng[id2] = (shootTarget - positionersPos[id2]).AngleInDegrees;
                positionersPos[id1] = positionersPos[id2] + new Vector2D(0.3, 0);
                positionersAng[id1] = (shootTarget - positionersPos[id1]).AngleInDegrees;
                positionersPos[id0] = new Position2D(positionersPos[id0].X, Math.Sign(-Model.BallState.Location.Y) * 1.6);
                positionersAng[id0] = (shootTarget - positionersPos[id0]).AngleInDegrees;
                if (Model.OurRobots[id1].Location.DistanceFrom(positionersPos[id1]) < 0.23)
                {
                    chekWays[1] = true;
                    bool[] btmp = new bool[3];
                    FindShooter(engine, Model, positionersID, ref btmp[0], ref btmp[1], ref btmp[2]);
                    open2kick[id1] = (btmp[1]) ? 2 : open2kick[id1];
                    open2kick[id1] = (Model.BallState.Location.X <= -1.8) ? open2kick[id1] : 0;
                }
                if (Model.OurRobots[id2].Location.DistanceFrom(positionersPos[id2]) < 0.23)
                {
                    chekWays[2] = true;
                    bool[] btmp = new bool[3];
                    FindShooter(engine, Model, positionersID, ref btmp[0], ref btmp[1], ref btmp[2]);
                    open2kick[id2] = (btmp[2]) ? 2 : open2kick[id2];
                    open2kick[id2] = (Model.BallState.Location.X <= -1.8) ? open2kick[id2] : 0;

                }
            }
            else if (CurrentState == (int)State.FakeMoveBack)
            {
                firstPosCalculated = false;
                if (checkWays2Goal)
                {
                    bool[] btmp = new bool[3];
                    FindShooter(engine, Model, positionersID, ref btmp[0], ref btmp[1], ref btmp[2]);
                    if (!chekWays[1])
                    {
                        open2kick[id1] = (btmp[1]) ? 2 : open2kick[id1];
                        open2kick[id1] = (Model.BallState.Location.X <= -1.8) ? open2kick[id1] : 0;
                    }
                    if (!chekWays[2])
                    {
                        open2kick[id2] = (btmp[2]) ? 2 : open2kick[id2];
                        open2kick[id2] = (Model.BallState.Location.X <= -1.8) ? open2kick[id2] : 0;
                    }
                    #region Find Shooter
                    shooterID = -1;
                    //foreach (var item in open2kick.Keys)
                    //{
                    //    if (open2kick[item] == 1)
                    //    {
                    //        shooterID = item;
                    //        stateOpen2Goal = 1;
                    //        break;
                    //    }
                    //}
                    //if (shooterID == -1)
                    {
                        foreach (var item in open2kick.Keys)
                        {
                            if (open2kick[item] == 2)
                            {
                                shooterID = item;
                                stateOpen2Goal = 2;
                                break;
                            }
                        }
                        if (shooterID == positionersID[1])
                        {
                            if (open2kick[id2] == 2)
                            {
                                shooterID = id2;
                                stateOpen2Goal = 2;
                            }
                        }
                    }
                    if (shooterID == -1)
                    {
                        shooterID = id2;
                        stateOpen2Goal = 2;
                    }
                    //if (shooterID == -1)
                    //{
                    //    shooterID = defenderID;
                    //    stateOpen2Goal = 0;
                    //    moveDefender = true;
                    //    defenderMidPoint = new Position2D(defenderPos.X, Math.Sign(Model.BallState.Location.Y) * 1);
                    //    defenderGoSync = false;
                    //}
                    //else
                    //    moveDefender = false;
                    #endregion
                    passTarget = GetPassPoint(engine, Model);
                    checkWays2Goal = false;
                }
                positionersPos[id0] = buPositionersPos[id0];
                positionersAng[id0] = buPositionersAng[id0];
                positionersPos[id1] = buPositionersPos[id1];
                positionersAng[id1] = buPositionersAng[id1];
                positionersPos[id2] = buPositionersPos[id2];
                positionersAng[id2] = buPositionersAng[id2];
            }
            else if (CurrentState == (int)State.Go)
            {
                if (!firstPosCalculated)
                {

                    //buPositionersPos[id0] = (shooterID != positionersID[0] || stateOpen2Goal != 1) ? /*new Position2D(Model.BallState.Location.X + 1.5, Math.Sign(-Model.BallState.Location.Y) * 1.6) : buPositionersPos[id0];*/passTarget : buPositionersPos[id0];
                    //buPositionersPos[id1] = (shooterID != positionersID[1] || stateOpen2Goal != 1) ? new Position2D(-2.8, Math.Sign(-Model.BallState.Location.Y) * 1.3) : buPositionersPos[id1];
                    //buPositionersPos[id2] = (shooterID != positionersID[2] || stateOpen2Goal != 1) ? buPositionersPos[id1] + new Vector2D(0.3, 0) : buPositionersPos[id2];



                    //buPositionersAng[id0] = (shooterID != positionersID[0] || stateOpen2Goal != 1) ? (shootTarget - buPositionersPos[id0]).AngleInDegrees : buPositionersAng[id0];
                    //buPositionersAng[id1] = (shooterID != positionersID[1] || stateOpen2Goal != 1) ? (shootTarget - buPositionersPos[id1]).AngleInDegrees : buPositionersAng[id1];
                    //buPositionersAng[id2] = (shooterID != positionersID[2] || stateOpen2Goal != 1) ? (shootTarget - buPositionersPos[id2]).AngleInDegrees : buPositionersAng[id2];
                    if (shooterID == positionersID[0])
                    {
                        buPositionersPos[id1] = new Position2D(-2.8, Math.Sign(-Model.BallState.Location.Y) * 1.3);
                        buPositionersPos[id2] = buPositionersPos[id1] + new Vector2D(0.3, 0);
                        buPositionersAng[id1] = (shootTarget - buPositionersPos[id1]).AngleInDegrees;
                        buPositionersAng[id2] = (shootTarget - buPositionersPos[id2]).AngleInDegrees;
                    }
                    else if (shooterID == positionersID[1] || shooterID == positionersID[2])
                    {
                        buPositionersPos[id0] = new Position2D(buPositionersPos[id0].X, -buPositionersPos[id0].Y);
                        buPositionersPos[id2] = new Position2D(-2.8, Math.Sign(-Model.BallState.Location.Y) * 1.3);
                        buPositionersPos[id1] = buPositionersPos[id2] + new Vector2D(0.3, 0);
                        buPositionersAng[id0] = (shootTarget - buPositionersPos[id0]).AngleInDegrees;
                        buPositionersAng[id1] = (shootTarget - buPositionersPos[id1]).AngleInDegrees;
                        buPositionersAng[id2] = (shootTarget - buPositionersPos[id2]).AngleInDegrees;
                    }
                    else
                    {
                        buPositionersPos[id0] = new Position2D(buPositionersPos[id0].X, -buPositionersPos[id0].Y);
                        buPositionersPos[id1] = new Position2D(-2.8, Math.Sign(-Model.BallState.Location.Y) * 1.3);
                        buPositionersPos[id2] = buPositionersPos[id1] + new Vector2D(0.3, 0);
                        buPositionersAng[id0] = (shootTarget - buPositionersPos[id0]).AngleInDegrees;
                        buPositionersAng[id1] = (shootTarget - buPositionersPos[id1]).AngleInDegrees;
                        buPositionersAng[id2] = (shootTarget - buPositionersPos[id2]).AngleInDegrees;
                    }
                    gotPoses = false;

                    firstPosCalculated = true;
                }
                if (inRotate && Model.BallState.Speed.Size > passSpeedTresh)
                    passed = true;
                if (!passed && !chipOrigin)
                {
                    chip = chipOrigin;
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    chip = obs.Meet(Model.BallState, new SingleObjectState(passTarget, Vector2D.Zero, 0), 0.07);
                }
                if (!passed)
                {
                    PassSpeed = 4; // engine.GameInfo.CalculateKickSpeed(Model, passerID, Model.BallState.Location, passTarget, chip, true);
                    if (chip || passTarget.X < -1.4)
                        PassSpeed = Model.BallState.Location.DistanceFrom(passTarget) * 0.55;

                    double goodness;
                    var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, shootTarget, passTarget, out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    if (GoodPointInGoal.HasValue)
                        shootTarget = GoodPointInGoal.Value;
                }


                if ((sync.SyncStarted || shooterID == positionersID[1] || shooterID == positionersID[2]) && !gotPoses)
                {
                    positionersPos[id0] = buPositionersPos[id0];
                    positionersAng[id0] = buPositionersAng[id0];
                    positionersPos[id1] = buPositionersPos[id1];
                    positionersAng[id1] = buPositionersAng[id1];
                    positionersPos[id2] = buPositionersPos[id2];
                    positionersAng[id2] = buPositionersAng[id2];
                    //if (moveDefender)
                    //{
                    //    int tmpi = positionersID[0];
                    //    positionersID[0] = defenderID;
                    //    defenderID = tmpi;
                    //    positionersPos[positionersID[0]] = positionersPos[tmpi];
                    //    positionersAng[positionersID[0]] = positionersAng[tmpi];

                    //    positionersPos.Remove(tmpi);
                    //    positionersAng.Remove(tmpi);
                    //}
                    gotPoses = true;
                }
            }
            if (Debug)
            {
                DrawingObjects.AddObject(new StringDraw("State" + (State)CurrentState, new Position2D(0.5, 0.5)));
                DrawingObjects.AddObject(new Circle(positionersPos[positionersID[2]], 0.1, new System.Drawing.Pen(System.Drawing.Color.Red, 0.01f)));
                DrawingObjects.AddObject(new Circle(positionersPos[positionersID[1]], 0.1, new System.Drawing.Pen(System.Drawing.Color.Blue, 0.01f)));
                DrawingObjects.AddObject(new Circle(positionersPos[positionersID[0]], 0.1, new System.Drawing.Pen(System.Drawing.Color.Black, 0.01f)));
            }
            #endregion

        }
        Position2D GetPassPoint(GameStrategyEngine engine, WorldModel Model)
        {
            double width = 0.9, heigth = 0.6, step = 0.3;
            int id0 = positionersID[0];
            Position2D topLeft = new Position2D(buPositionersPos[id0].X + 0.1, (Math.Sign(Model.BallState.Location.Y) >= 0 ? 1.1 : -(1.1 + heigth)));
            Position2D bestPoint = Position2D.Zero;
            chip = chipOrigin;
            engine.GameInfo.BestPassPoint(Model, shootTarget, topLeft, width, heigth, (int)(width / step), (int)(heigth / step), ref chip, ref bestPoint);
            chipOrigin = chip;
            Position2D pTarget;
            if (shooterID == positionersID[1] || shooterID == positionersID[2])
            {
                pTarget = new Position2D(-2.7, -Math.Sign(Model.BallState.Location.Y) * 0.5);//buPositionersPos[shooterID];
                chipOrigin = true;
                chip = chipOrigin;
            }
            else
            {
                pTarget = bestPoint;
                Vector2D BallTarget = pTarget - Model.BallState.Location;
                Vector2D InitBall = (GameParameters.OppGoalCenter - Model.BallState.Location);
                double Teta = Vector2D.AngleBetweenInDegrees(BallTarget, InitBall);
                double sgn = Math.Sign(Teta);
                Teta = Math.Round(Math.Abs(Teta));
                Teta = Teta - (Teta % 10);
                if (Teta > 120 && Teta < 150)
                    Teta = 120;
                else if (Teta >= 150)
                    Teta = 180;
                else if (Teta > 15 && Teta < 30)
                    Teta = 30;
                else if (Teta < 15)
                    Teta = 0;
                Teta *= sgn;
                Teta = Teta.ToRadian();
                pTarget = Model.BallState.Location + Vector2D.FromAngleSize(InitBall.AngleInRadians + Teta, Model.BallState.Location.DistanceFrom(pTarget));
            }

            return pTarget;
        }
        private List<int> RemoveGoaliID(WorldModel Model, Dictionary<int, SingleObjectState> Attendance)
        {
            return (Model.GoalieID.HasValue) ? Attendance.Keys.Where(w => w != Model.GoalieID.Value).ToList() : Attendance.Keys.ToList();
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();

            int id0 = positionersID[0], id1 = positionersID[1], id2 = positionersID[2];
            #region comment
            //if (CurrentState != (int)State.Go)
            //{
            //    Planner.ChangeDefaulteParams(passerID, false);
            //    Planner.SetParameter(passerID, 2.5, 2);
            //    if (Planner.AddRotate(Model, passerID, shootTarget, Model.BallState.Location + (Model.BallState.Location - shootTarget).GetNormalizeToCopy(0.1), kickPowerType.Speed, PassSpeed, false, rotateCounter, true).IsInRotateDelay)
            //        rotateCounter++;
            //    foreach (var item in positionersID)
            //    {
            //        Planner.ChangeDefaulteParams(item.Value, false);
            //        Planner.SetParameter(item.Value, 6, 5);
            //        Planner.Add(item.Value, positionersPos[item.Value], positionersAng[item.Value], PathType.UnSafe, true, true, true, true);
            //    }
            //}
            //else if (CurrentState == (int)State.Go)
            //{
            //    Planner.ChangeDefaulteParams(shooterID, false);
            //    Planner.SetParameter(shooterID, 6, 5);
            //    foreach (var item in positionersID)
            //    {
            //        if (item.Value != shooterID)
            //        {
            //            Planner.ChangeDefaulteParams(item.Value, false);
            //            Planner.SetParameter(item.Value, 6, 5);
            //            Planner.Add(item.Value, positionersPos[item.Value], positionersAng[item.Value], PathType.UnSafe, true, true, true, true);
            //        }
            //    }


            //    double passChipDist = passTarget.DistanceFrom(passerPos) * 0.28;
            //    if (Debug)
            //        DrawingObjects.AddObject(new StringDraw("chip: " + chip, Position2D.Zero));
            //    if (!chip)
            //        sync.SyncDirectPass(engine, Model, passerID, Model.BallState.Location + (Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(0.1), shooterID, passTarget, shootTarget, PassSpeed, KickPower, RotateDelay, rotateCounter);
            //    else
            //        sync.SyncChipPass(engine, Model, passerID, Model.BallState.Location + (Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(0.1), shooterID, passTarget, shootTarget, PassSpeed, KickPower, RotateDelay, rotateCounter);
            //    inRotate = sync.InRotate;
            //}

            //if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
            //    Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, goaliPos, goaliAng);

            //if (sync.SyncStarted || CurrentState == (int)State.First)
            //{
            //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, defenderID, typeof(DefenderCornerRole1)))
            //        Functions[defenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(defenderID).Run(eng, wmd, defenderID, defenderPos, defenderAng);
            //}
            //else
            //    Planner.Add(defenderID, defenderPos, defenderAng, PathType.UnSafe, true, true, true, true);
            #endregion
            if (CurrentState == (int)State.First)
            {
                if (Planner.AddRotate(Model, passerID, shootTarget, passerPos, kickPowerType.Speed, PassSpeed, false, rotateCounter, true).IsInRotateDelay)
                {
                    rotateCounter++;
                }
                foreach (var item in positionersID)
                {
                    Planner.ChangeDefaulteParams(item.Value, false);
                    Planner.SetParameter(item.Value, 6, 5);
                    Planner.Add(item.Value, positionersPos[item.Value], positionersAng[item.Value], PathType.UnSafe, true, true, true, true);
                }

                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, defenderID, typeof(DefenderCornerRole1)))
                    Functions[defenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(defenderID).Run(eng, wmd, defenderID, defenderPos, defenderAng);

            }
            else if (CurrentState == (int)State.FakeMove)
            {
                if (!recalcRot && open2kick[id0] == 1)
                {
                    recalcRot = true;
                    Planner.SetReCalculateTeta(passerID, true);
                }
                if (Planner.AddRotate(Model, passerID, (open2kick[id0] == 1) ? passTarget : shootTarget, passerPos, kickPowerType.Speed, PassSpeed, chip, rotateCounter, true).IsInRotateDelay)
                {
                    rotateCounter++;
                }
                foreach (var item in positionersID)
                {
                    Planner.ChangeDefaulteParams(item.Value, false);
                    Planner.SetParameter(item.Value, 6, 5);
                    Planner.Add(item.Value, positionersPos[item.Value], positionersAng[item.Value], PathType.UnSafe, true, true, true, true);
                }
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, defenderID, typeof(DefenderCornerRole1)))
                    Functions[defenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(defenderID).Run(eng, wmd, defenderID, defenderPos, defenderAng);

            }
            else if (CurrentState == (int)State.FakeMoveBack)
            {
                if (!recalcRot)
                {
                    recalcRot = true;
                    Planner.SetReCalculateTeta(passerID, true);
                }
                if (shooterID == positionersID[1] || shooterID == positionersID[2])
                {
                    if (Planner.AddRotate(Model, passerID, passTarget, 0, kickPowerType.Speed, PassSpeed, chip, rotateCounter, backSensor).IsInRotateDelay)
                    {
                        rotateCounter++;
                    }
                }
                else
                {
                    if (Planner.AddRotate(Model, passerID, passTarget, passerPos, kickPowerType.Speed, PassSpeed, chip, rotateCounter, backSensor).IsInRotateDelay)
                    {
                        rotateCounter++;
                    }
                }
                foreach (var item in positionersID)
                {
                    Planner.ChangeDefaulteParams(item.Value, false);
                    Planner.SetParameter(item.Value, 6, 5);
                    Planner.Add(item.Value, positionersPos[item.Value], positionersAng[item.Value], PathType.UnSafe, true, true, true, true);
                }
                if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, defenderID, typeof(DefenderCornerRole1)))
                    Functions[defenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(defenderID).Run(eng, wmd, defenderID, defenderPos, defenderAng);

            }
            else if (CurrentState == (int)State.Go)
            {
                if (shooterID == positionersID[1] || shooterID == positionersID[2])
                {
                    if (Model.OurRobots[shooterID].Location.DistanceFrom(buPositionersPos[shooterID]) < 0.1)
                        rotateCounter++;
                    if (Planner.AddRotate(Model, passerID, passTarget, passerPos, kickPowerType.Speed, PassSpeed, chip, rotateCounter, true).InKickState)
                        inRotate = true;
                    foreach (var item in positionersID)
                    {
                        if (item.Value == shooterID && passed)
                            continue;
                        Planner.ChangeDefaulteParams(item.Value, false);
                        Planner.SetParameter(item.Value, 6, 5);
                        Planner.Add(item.Value, positionersPos[item.Value], positionersAng[item.Value], PathType.UnSafe, true, true, true, true);
                    }
                    if (passed)
                    {
                        double dist, DistFromBorder;

                        if (Model.BallState.Speed.Size < 0.4 && GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0.1, out dist, out DistFromBorder))
                            ballMoved = true;

                        if (ballMoved)
                        {
                            // getBallSkill.OutGoingBackTrack(engine, Model, shooterID, shootTarget);
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, shooterID, typeof(ActiveRole)))
                                Functions[shooterID] = (eng, wmd) => GetRole<ActiveRole>(shooterID).PerformForStrategy(eng, Model, shooterID, null, null, true, true, false);
                            Planner.AddKick(shooterID, kickPowerType.Speed, false, Program.MaxKickSpeed);
                        }
                        //else if (GameParameters.IsInDangerousZone(Model.BallState.Location, true, 0, out dist, out DistFromBorder))
                        //{
                        //    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, shooterID, typeof(OneTouchRole)))
                        //        Functions[shooterID] = (eng, wmd) => GetRole<OneTouchRole>(shooterID).Perform(eng, wmd, shooterID, true, new SingleObjectState(buPositionersPos[shooterID], Vector2D.Zero, 0), Model.OurRobots[passerID], true, shootTarget, Program.MaxKickSpeed, false, AngleT, !ballMoved);
                        //}
                        else
                        {
                            Position2D p = GameParameters.OppGoalCenter + (positionersPos[shooterID] - GameParameters.OppGoalCenter).GetNormalizeToCopy(GameParameters.SafeRadi(new SingleObjectState(-positionersPos[shooterID], Vector2D.Zero, 0), -0.2));
                            Planner.Add(shooterID, p, (shootTarget - p).AngleInDegrees, PathType.UnSafe, false, false, false, false);
                        }
                    }
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, defenderID, typeof(DefenderCornerRole1)))
                        Functions[defenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(defenderID).Run(eng, wmd, defenderID, defenderPos, defenderAng);

                }
                else if (shooterID == positionersID[0])
                {
                    RotateDelay = 10;
                    if (!chip)
                        sync.SyncDirectPass(engine, Model, passerID, passerPos, shooterID, passTarget, shootTarget, PassSpeed, KickPower, RotateDelay, rotateCounter);
                    else
                        sync.SyncChipPass(engine, Model, passerID, passerPos, shooterID, passTarget, shootTarget, PassSpeed, KickPower, RotateDelay, backSensor, rotateCounter);
                    if (sync.InPassState)
                        inRotate = true;
                    foreach (var item in positionersID)
                    {
                        if (item.Value == shooterID)
                            continue;
                        Planner.ChangeDefaulteParams(item.Value, false);
                        Planner.SetParameter(item.Value, 6, 5);
                        Planner.Add(item.Value, positionersPos[item.Value], positionersAng[item.Value], PathType.UnSafe, true, true, true, true);
                    }
                    if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, defenderID, typeof(DefenderCornerRole1)))
                        Functions[defenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(defenderID).Run(eng, wmd, defenderID, defenderPos, defenderAng);

                }
                else
                {
                    RotateDelay = 10;
                    if (Model.OurRobots[shooterID].Location.DistanceFrom(defenderMidPoint) < 0.3)
                        defenderGoSync = true;
                    if (!defenderGoSync)
                        rotateCounter++;
                    if (!defenderGoSync)
                    {
                        Planner.AddRotate(Model, passerID, passTarget, passerPos, kickPowerType.Speed, PassSpeed, chip, rotateCounter, true);
                        Planner.Add(shooterID, defenderMidPoint, 180, PathType.UnSafe, false, false, true, false);
                    }
                    else
                    {

                        if (!chip)
                            sync.SyncDirectPass(engine, Model, passerID, passerPos, shooterID, passTarget, shootTarget, PassSpeed, KickPower, RotateDelay, rotateCounter);
                        else
                            sync.SyncChipPass(engine, Model, passerID, passerPos, shooterID, passTarget, shootTarget, PassSpeed, KickPower, RotateDelay, backSensor, rotateCounter);

                        if (sync.InPassState)
                            inRotate = true;
                    }
                    if (Model.OurRobots[shooterID].Location.X < 1.0)
                        defenderRushed = true;
                    foreach (var item in positionersID)
                    {
                        if (item.Value == positionersID[0] && defenderRushed)
                            continue;
                        Planner.ChangeDefaulteParams(item.Value, false);
                        Planner.SetParameter(item.Value, 6, 5);
                        Planner.Add(item.Value, positionersPos[item.Value], positionersAng[item.Value], PathType.UnSafe, true, true, true, true);
                    }
                    if (defenderRushed)
                    {
                        defenderID = positionersID[0];
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, defenderID, typeof(DefenderCornerRole1)))
                            Functions[defenderID] = (eng, wmd) => GetRole<DefenderCornerRole1>(defenderID).Run(eng, wmd, defenderID, defenderPos, defenderAng);

                    }
                }
                if (passed && Model.BallState.Location.DistanceFrom(Model.OurRobots[passerID].Location) > 0.25)
                {
                    Position2D p = new Position2D(-1, Math.Sign(passerPos.Y) * 1.3);
                    Planner.Add(passerID, p, (shootTarget - p).AngleInDegrees, PathType.UnSafe, false, true, true, false);
                }
            }

            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, Model.GoalieID, typeof(GoalieCornerRole)))
                Functions[Model.GoalieID.Value] = (eng, wmd) => GetRole<GoalieCornerRole>(Model.GoalieID.Value).Run(engine, wmd, Model.GoalieID.Value, goaliPos, goaliAng, new DefenceInfo(), defenderPos, defenderID, true);

            return CurrentlyAssignedRoles;
        }

        public enum State
        {
            First,
            FakeMove,
            FakeMoveBack,
            Go,
            Finish
        }
    }
}
