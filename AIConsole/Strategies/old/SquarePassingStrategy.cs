using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRL.SSL.AIConsole.Engine;
using MRL.SSL.GameDefinitions;
using MRL.SSL.CommonClasses.MathLibrary;
using MRL.SSL.Planning.MotionPlanner;
using MRL.SSL.Planning.GamePlanner.Types;
using MRL.SSL.AIConsole.Roles;
using System.Drawing;

namespace MRL.SSL.AIConsole.Strategies
{
    public class SquarePassingStrategy : StrategyBase
    {
        const double tresh = 0.01, angleTresh = 2, waitTresh = 80, finishTresh = 420, initDist = 0.22, maxWaitTresh = 120, passSpeedTresh = StaticVariables.PassSpeedTresh, behindBallTresh = 0.07, faildSpeedTresh = 0.8, faildOtDistTresh = 0.12, faildCatchDistTresh = 0.3, faildMaxCounter = 15, faildBallMovedDist = 0.06, maxFaildMovedDist = 0.2, faildSecondPassSpeedTresh = -0.85, faildBallDistSecondPass = 0.5;

        double width = 3.5, maxHeight = 1.5, minHeight = 1.5, KickPower, firstPassSpeed, secondPassSpeed;
        int[] positionersID;
        Position2D[] positionersPos;
        double[] positionersAng;
        bool first, debug = true, firstInState, firstPassIsChip, secondPassIsChip, firstPassChipOrigin, secondPassChipOrigin, rotateInPass,
            passed, shooted, nearShooter, secondPasskicked, catchPass, shooterIdxCalculated;
        bool[,] feasible1PassOt, feasible1PassCatch, feasible2Pass1stPass, feasible2Pass2ndPassOt, feasible2Pass2ndPassCatch;
        int [,] firstOrders;
        int timeLimitCounter, initialPosCounter, passerIdx, secondPasserIdx, shooterIdx, RotateDelay;
        Position2D  shootTarget;
        bool GoNormal, chipAllowd, shootIsChip;
        int faildCounter;
        bool backSensor;
        private void CalulateSquarePosAndAngles(WorldModel Model, ref Position2D[] poses, ref double[] anges, ref int passer)
        {
            double xMargin = 0.3, yMargin = 0.2;
            width = Math.Abs(Model.BallState.Location.Y + Math.Sign(Model.BallState.Location.Y) * Math.Abs(GameParameters.OurLeftCorner.Y)) - yMargin;
            if (Model.BallState.Location.X <= GameParameters.OppGoalCenter.X + minHeight)
            {
                if (Model.BallState.Location.Y >= 0)
                {
                    poses[0] = (new Position2D(Model.BallState.Location.X + initDist, Model.BallState.Location.Y));
                    passer = 0;
                }
                else
                {
                    poses[0] = (new Position2D(Model.BallState.Location.X + initDist, Model.BallState.Location.Y + width));
                    passer = 1;
                }

                for (int i = 1; i < 4; i++)
                {
                    poses[i] = (poses[i - 1].Extend(((i - 1) % 2) * maxHeight, (i - 2) * width));
                }
            }
            else
            {
                if (Model.BallState.Location.Y >= 0)
                {
                    poses[0] = (new Position2D(Math.Max(Model.BallState.Location.X + initDist - maxHeight, GameParameters.OppGoalCenter.X + xMargin), Model.BallState.Location.Y));
                    passer = 3;
                }
                else
                {
                    poses[0] = (new Position2D(Math.Max(Model.BallState.Location.X + initDist - maxHeight, GameParameters.OppGoalCenter.X + xMargin), Model.BallState.Location.Y + width));
                    passer = 2;
                }

                for (int i = 1; i < 4; i++)
                {
                    poses[i] = (poses[i - 1].Extend(((i - 1) % 2) * Math.Abs(poses[i - 1].X - (Model.BallState.Location.X + initDist)), (i - 2) * width));
                }
            }
            for (int i = 0; i < 4; i++)
            {
                anges[i] = ((Model.BallState.Location - poses[i]).AngleInDegrees);
            }
        }
        private bool CalculateIDs(WorldModel Model, Dictionary<int, SingleObjectState> attend, Position2D[] poses, ref int[] ids)
        {
            var tmpIds = attend.Keys.ToList();
            for (int i = 0; i < 4; i++)
            {
                double minDist = double.MaxValue;
                int minIdx = -1;
                foreach (var item in tmpIds.ToList())
                {
                    if (Model.OurRobots.ContainsKey(item) && Model.OurRobots[item].Location.DistanceFrom(poses[i]) < minDist)
                    {
                        minDist = Model.OurRobots[item].Location.DistanceFrom(poses[i]);
                        minIdx = item;
                    }
                }
                if (minIdx == -1)
                    return false;
                ids[i] = minIdx;
                tmpIds.Remove(ids[i]);
            }
            return true;
        }
        private int CalculatePasserIdx(GameStrategyEngine engine, WorldModel Model, Position2D[] poses, int[] ids, int passIdx, int secondPassIdx, bool firstPass, ref int ShootIdx, ref bool catchP, out bool Normal, out bool chipAllowd)
        {
            int SecondPassIdx = secondPassIdx;
            Normal = false;
            chipAllowd = false;
            if (firstPass)
            {
                if (passIdx == -1)
                    return -1;
                int tmpid1 = -1, tmpid2 = -1, tmpid3 = -1, tmpid4 = -1, tmpid5 = -1, tmpid6 = -1, tmpid7 = -1, tmpid8 = -1;
                for (int i = 0; i < ids.Length; i++)
                {
                    if (i == passIdx)
                        continue;

                    var toBall = (MarkingType)Model.markingStatesToBall[ids[i]];
                    var toTarget = (MarkingType)Model.markingStatesToTarget[ids[i]];

                    if ((tmpid1 == -1 || firstOrders[passerIdx, tmpid1] < firstOrders[passerIdx, i]) && feasible1PassOt[passIdx, i] && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
                        tmpid1 = i;

                    if ((tmpid2 == -1 || firstOrders[passerIdx, tmpid2] < firstOrders[passerIdx, i]) && feasible2Pass1stPass[passIdx, i] && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct) && toTarget != MarkingType.Blocked)
                        tmpid2 = i;

                    if (tmpid3 == -1 && feasible1PassCatch[passIdx, i] && (toBall != MarkingType.Blocked) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
                        tmpid3 = i;

                    if (tmpid4 == -1 && feasible1PassCatch[passIdx, i] && (toBall != MarkingType.Blocked))
                        tmpid4 = i;
                    
                    if ((tmpid5 == -1 || firstOrders[passerIdx, tmpid5] < firstOrders[passerIdx, i]) && feasible2Pass1stPass[passIdx, i] && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct))
                        tmpid5 = i;

                    if ((tmpid6 == -1 || firstOrders[passerIdx, tmpid6] < firstOrders[passerIdx, i]) && feasible2Pass1stPass[passIdx, i] && (toBall != MarkingType.Blocked) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
                        tmpid6 = i;

                    if ((tmpid7 == -1 || firstOrders[passerIdx, tmpid7] < firstOrders[passerIdx, i]) && feasible2Pass1stPass[passIdx, i] && toBall != MarkingType.Blocked)
                        tmpid7 = i;

                    if ((tmpid8 == -1 || firstOrders[passerIdx, tmpid8] < firstOrders[passerIdx, i]) && feasible2Pass1stPass[passIdx, i])
                        tmpid8 = i;
                }
                catchP = false;
                if (tmpid1 != -1)
                    ShootIdx = tmpid1;
                else if (tmpid2 != -1)
                {
                    SecondPassIdx = tmpid2;
                }
                else if (tmpid3 != -1)
                {
                    ShootIdx = tmpid3;
                    catchP = true;
                }
                else if (tmpid4 != -1)
                {
                    ShootIdx = tmpid4;
                    catchP = true;
                }
                else if (tmpid5 != -1)
                    SecondPassIdx = tmpid5;
                else if (tmpid6 != -1)
                {
                    chipAllowd = true;
                    SecondPassIdx = tmpid6;
                }
                else if (tmpid7 != -1)
                {
                    chipAllowd = true;
                    SecondPassIdx = tmpid7;
                }
                else if (tmpid8 != -1)
                {
                    chipAllowd = true;
                    SecondPassIdx = tmpid8;
                }
                else
                    SecondPassIdx = (passIdx + 1) % (ids.Length - 1);
            }
            else
            {
                if (secondPassIdx == -1)
                    return -1;
                int tmpid1 = -1, tmpid2 = -1, tmpid3 = -1, tmpid4 = -1, tmpid5 = -1, tmpid6 = -1, tmpid7 = -1, tmpid8 = -1;
                for (int i = 0; i < ids.Length; i++)
                {
                    if (i == SecondPassIdx)
                        continue;

                    var toBall = (MarkingType)Model.markingStatesToBall[ids[i]];
                    var toTarget = (MarkingType)Model.markingStatesToTarget[ids[i]];

                    if ((tmpid1 == -1 || firstOrders[SecondPassIdx, tmpid1] < firstOrders[SecondPassIdx , i]) && feasible2Pass2ndPassOt[SecondPassIdx, i] && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
                        tmpid1 = i;

                    if ((tmpid2 == -1 || firstOrders[SecondPassIdx, tmpid2] < firstOrders[SecondPassIdx , i]) && feasible2Pass2ndPassOt[SecondPassIdx, i] && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct) && (toTarget) != MarkingType.Blocked)
                        tmpid2 = i;

                    if ((tmpid3 == -1) && feasible2Pass2ndPassCatch[SecondPassIdx, i] && ((toBall) != MarkingType.Blocked) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
                        tmpid3 = i;


                    if ((tmpid4 == -1 || firstOrders[SecondPassIdx, tmpid4] < firstOrders[SecondPassIdx, i]) && feasible2Pass2ndPassOt[SecondPassIdx, i] && ((toBall) != MarkingType.Blocked) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
                        tmpid4 = i;


                    if (tmpid5 == -1 && feasible2Pass2ndPassCatch[SecondPassIdx, i] && ((toBall ) != MarkingType.Blocked) && (toTarget) != MarkingType.Blocked)
                        tmpid5 = i;

                    if (tmpid6 == -1 && feasible2Pass2ndPassCatch[SecondPassIdx, i] && toBall != MarkingType.Blocked && toTarget == MarkingType.Blocked)
                        tmpid6 = i;


                    if ((tmpid7 == -1 || firstOrders[SecondPassIdx, tmpid7] < firstOrders[SecondPassIdx, i]) && feasible2Pass2ndPassOt[SecondPassIdx, i] && toBall != MarkingType.Blocked && toTarget != MarkingType.Blocked)
                        tmpid7 = i;

                    if ((tmpid8 == -1 || firstOrders[SecondPassIdx, tmpid8] < firstOrders[SecondPassIdx, i]) && feasible2Pass2ndPassOt[SecondPassIdx, i] && toBall != MarkingType.Blocked && toTarget == MarkingType.Blocked)
                        tmpid8 = i;

                    //if ((tmpid8 == -1 || firstOrders[SecondPassIdx, tmpid8] < firstOrders[SecondPassIdx, i]) && feasible2Pass1stPass[SecondPassIdx, i])
                    //    tmpid8 = i;

                }
                catchP = false;
                if (tmpid1 != -1)
                    ShootIdx = tmpid1;
                else if (tmpid2 != -1)
                {
                    ShootIdx = tmpid2;
                }
                else if (tmpid3 != -1)
                {
                    ShootIdx = tmpid3;
                    catchP = true;
                }
                else if (tmpid4 != -1)
                {
                    ShootIdx = tmpid4;
                    chipAllowd = true;
                }
                else if (tmpid5 != -1)
                {
                    ShootIdx = tmpid5;
                    catchP = true;
                }
                else if (tmpid6 != -1)
                {
                    ShootIdx = tmpid6;
                    catchP = true;
                }
                else if (tmpid7 != -1)
                {
                    ShootIdx = tmpid7;
                    chipAllowd = true;
                }
                else if (tmpid8 != -1)
                {
                    ShootIdx = tmpid8;
                    chipAllowd = true;
                }
                else
                {
                    ShootIdx = (passIdx + 1) % (ids.Length - 1);
                    Normal = true;
                }
            }
            return SecondPassIdx;
        }
        //private int CalculatePasserIdx(GameStrategyEngine engine, WorldModel Model, Position2D[] poses, int[] ids, bool[,] feasibleWays, int passIdx, int secondPassIdx, bool firstPass, ref int ShootIdx, ref bool catchP)
        //{
        //    int SecondPassIdx = secondPassIdx;
        //    double shiftSecondPassTresh = 0.35;
        //    if (firstPass)
        //    {
        //        if (passIdx == -1)
        //            return -1;
        //        int tmpid1 = -1, tmpid2 = -1, tmpid3 = -1, tmpid4 = -1, tmpid5 = -1,tmpid6 = -1,tmpid7 = -1;
        //        for (int i = 0; i < ids.Length; i++)
        //        {
        //            if (i == passIdx)
        //                continue;
                    
        //            var toBall = (MarkingType)Model.markingStatesToBall[ids[i]];
        //            var toTarget = (MarkingType)Model.markingStatesToTarget[ids[i]];

        //            if (tmpid1 == -1 && feasibleWays[passIdx, i] && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid1 = i;
                    
        //            if (tmpid2 == -1 && feasible4SecondPass[passIdx, i] && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid2 = i;

        //            if (tmpid3 == -1 && feasibleWays[passIdx, i] && (toBall != MarkingType.Blocked) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid3 = i;

        //            if (tmpid4 == -1 && feasible4SecondPass[passIdx, i] && (toBall != MarkingType.Blocked) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid4 = i;

        //            if (tmpid5 == -1 && feasible4SecondPass[passIdx, i] && (toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid5 = i;

        //            if (tmpid6 == -1 && feasible4SecondPass[passIdx, i] && toBall != MarkingType.Blocked)
        //                tmpid6 = i;

        //            if (tmpid7 == -1 && feasible4SecondPass[passIdx, i])
        //                tmpid7 = i;
        //        }
        //        catchP = false;
        //        if (tmpid1 != -1)
        //            ShootIdx = tmpid1;
        //        else if (tmpid2 != -1)
        //        {
        //            ShootIdx = tmpid2;
        //            catchP = true;
        //        }
        //        else if (tmpid3 != -1)
        //            ShootIdx = tmpid3;
        //        else if (tmpid4 != -1)
        //        {
        //            ShootIdx = tmpid4;
        //            catchP = true;
        //        }
        //        else if (tmpid5 != -1)
        //            SecondPassIdx = tmpid5;
        //        else if (tmpid6 != -1)
        //            SecondPassIdx = tmpid6;
        //        else if (tmpid7 != -1)
        //            SecondPassIdx = tmpid7;
        //        else
        //            SecondPassIdx = (passIdx + 1) % (ids.Length - 1);
        //    }
        //    else
        //    {
        //        if (secondPassIdx == -1)
        //            return -1;
        //        int tmpid1 = -1, tmpid2 = -1, tmpid3 = -1, tmpid4 = -1, tmpid5 = -1, tmpid6 = -1, tmpid7 = -1;
        //        for (int i = 0; i < ids.Length; i++)
        //        {
        //            if (i == SecondPassIdx)
        //                continue;

        //            var toBall = (MarkingType)Model.markingStatesToBall[ids[i]];
        //            var toTarget = (MarkingType)Model.markingStatesToTarget[ids[i]];

        //            if (tmpid1 == -1 && feasibleWays[SecondPassIdx, i] && Model.BallState.Location.X - poses[i].X <= shiftSecondPassTresh && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid1 = i;
                    
        //            if (tmpid2 == -1 && feasible4SecondPass[SecondPassIdx, i] && ((toBall & MarkingType.Open2Direct) == MarkingType.Open2Direct) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid2 = i;

        //            if (tmpid3 == -1 && feasibleWays[SecondPassIdx, i] && Model.BallState.Location.X - poses[i].X <= shiftSecondPassTresh && (toBall != MarkingType.Blocked) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid3 = i;
                    
        //            if (tmpid4 == -1 && feasible4SecondPass[SecondPassIdx, i] && (toBall != MarkingType.Blocked) && (toTarget & MarkingType.Open2Direct) == MarkingType.Open2Direct)
        //                tmpid4 = i;

        //            if (tmpid5 == -1 && feasibleWays[SecondPassIdx, i] && Model.BallState.Location.X - poses[i].X <= shiftSecondPassTresh && toBall != MarkingType.Blocked)
        //                tmpid5 = i;
                    
        //            if (tmpid6 == -1 && feasible4SecondPass[SecondPassIdx, i] && toBall != MarkingType.Blocked)
        //                tmpid6 = i;

        //            if (tmpid7 == -1 && Model.BallState.Location.X - poses[i].X <= shiftSecondPassTresh && feasibleWays[SecondPassIdx, i])
        //                tmpid7 = i;
        //        }
        //        catchP = false;
        //        if (tmpid1 != -1)
        //            ShootIdx = tmpid1;
        //        else if (tmpid2 != -1)
        //        {
        //            ShootIdx = tmpid2;
        //            catchP = true;
        //        }
        //        else if (tmpid3 != -1)
        //            ShootIdx = tmpid3;
        //        else if (tmpid4 != -1)
        //        {
        //            ShootIdx = tmpid3;
        //            catchP = true;
        //        }
        //        else if (tmpid5 != -1)
        //            ShootIdx = tmpid5;
        //        else if (tmpid6 != -1)
        //        {
        //            ShootIdx = tmpid6;
        //            catchP = true;
        //        }
        //        else if (tmpid7 != -1)
        //            ShootIdx = tmpid7;
        //        else
        //            ShootIdx = (SecondPassIdx + 1) % (ids.Length - 1);
        //    }
        //    return SecondPassIdx;
        //}
        private bool IsBallPassed(WorldModel Model, Position2D[] poses, int[] ids, int passIdx, int secondIdx, int shootIdx, bool firstPass, bool PassChip, bool rotInPass)
        {
            if (Model.BallState.Speed.Size > passSpeedTresh)
            {
                if (firstPass)
                {
                    if (rotInPass)
                        return true;
                }
                else
                {
                    if (PassChip)
                    {
                        if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ids[secondIdx]].Location) > 0.4)
                            return true;
                    }
                    else
                    {
                        double innerFirst = Model.BallState.Speed.InnerProduct(poses[secondIdx] - poses[passIdx]);
                        double innerSecond = Model.BallState.Speed.InnerProduct(poses[shootIdx] - poses[secondIdx]);

                        if (innerFirst <= 0 && innerSecond > 0)
                        {
                            if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ids[secondIdx]].Location) > 0.4)
                                return true;
                        }
                        else if (innerFirst > 0 && innerSecond > 0)
                        {
                            if ((poses[shootIdx] - poses[secondIdx]).InnerProduct(Model.BallState.Location - poses[secondIdx]) < 0)
                            {
                                if (Model.BallState.Location.DistanceFrom(Model.OurRobots[ids[secondIdx]].Location) > 0.4)
                                    return true; ;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private bool CalculateShooterIdxAfterPass(WorldModel Model, Position2D[] poses, int[] ids, int passIdx, bool passIsChip, out int idx)
        {
            int passerID = positionersID[passerIdx];
            Line l = new Line();
            if (!passIsChip && Model.BallState.Speed.Size > 0.05)
            { 
                l = new Line(Model.BallState.Location, Model.BallState.Location + Model.BallState.Speed);
            }
            else if (passIsChip)
            {
                l = new Line(Model.OurRobots[ids[passIdx]].Location, Model.OurRobots[ids[passIdx]].Location + Vector2D.FromAngleSize(Model.OurRobots[ids[passIdx]].Angle.Value * Math.PI / 180, 1));
            }
            
            double minDist = double.MaxValue;
            int minIdx = -1;
            Position2D inter = Position2D.Zero;
            for (int i = 0; i < poses.Length; i++)
            {
                if (i == passIdx)
                    continue;
                if (l.IntersectWithLine(l.PerpenducilarLineToPoint(poses[i]), ref inter))
                {
                    double d = inter.DistanceFrom(poses[i]);
                    if (d < minDist)
                    {
                        Vector2D v1 = inter - Model.BallState.Location;
                        if (v1.Size > 0 && v1.InnerProduct(Model.BallState.Speed) > 0)
                        {
                            minDist = d;
                            minIdx = i;
                        }
                    }
                }

            }
            idx = minIdx;
            if (idx != -1)
                return true;
            return false;
        }

        public override void ResetState()
        {
            backSensor = true;
            GoNormal = false;
            chipAllowd = false;
            shootIsChip = false;
            faildCounter = 0;
            firstBallPos = Position2D.Zero;
            shootIsChip = false;
            first = true;
            firstInState = true;
            firstPassIsChip = false;
            secondPassIsChip = false;
            firstPassChipOrigin = false;
            secondPassChipOrigin = false;
            nearShooter = false;
            passed = false;
            shooted = false;
            secondPasskicked = false;
            catchPass = false;
            shooterIdxCalculated = false;
            GoNormal = false;
            rotateInPass = false;
            positionersID = new int[4];
            positionersPos = new Position2D[4];
            positionersAng = new double[4];
            timeLimitCounter = 0;
            initialPosCounter = 0;
            passerIdx = -1;
            secondPasserIdx = -1;
            shooterIdx = -1;
            RotateDelay = 60;
            faildCounter = 0;

            feasible1PassOt = new bool[4, 4];
            feasible1PassCatch = new bool[4, 4];
            feasible2Pass1stPass = new bool[4, 4];
            feasible2Pass2ndPassCatch = new bool[4, 4];
            feasible2Pass2ndPassOt = new bool[4, 4];
            
            firstOrders = new int[4, 4];

            firstOrders[0, 0] = firstOrders[3, 3] = firstOrders[1, 1] = firstOrders[2, 2] = -10000;
            firstOrders[0, 1] = 3; firstOrders[0, 2] = 2; firstOrders[0, 3] = 1;
            firstOrders[1, 0] = 3; firstOrders[1, 2] = 1; firstOrders[1, 3] = 2;
            firstOrders[2, 0] = 2; firstOrders[2, 1] = 1; firstOrders[2, 3] = 3;
            firstOrders[3, 0] = 1; firstOrders[3, 1] = 2; firstOrders[3, 2] = 3;

            feasible1PassOt[3, 2] = feasible1PassOt[2, 3] = feasible1PassOt[1, 0] = feasible1PassOt[0, 1] = feasible1PassOt[0, 3] = feasible1PassOt[1, 2] = feasible1PassOt[0, 2] = feasible1PassOt[1, 3] = true;
            feasible1PassCatch[3, 0] = feasible1PassCatch[3, 1] = feasible1PassCatch[2, 1] = feasible1PassCatch[2, 0] = true;
            feasible2Pass1stPass[3, 2] = feasible2Pass1stPass[2, 3] = feasible2Pass1stPass[1, 0] = feasible2Pass1stPass[0, 1] = feasible2Pass1stPass[3, 1] = feasible2Pass1stPass[2, 0] = feasible2Pass1stPass[3, 0] = feasible2Pass1stPass[2, 1] = feasible2Pass1stPass[1, 2] = feasible2Pass1stPass[0, 3] = true;

            feasible2Pass2ndPassOt[3, 2] = feasible2Pass2ndPassOt[2, 3] = feasible2Pass2ndPassOt[1, 0] = feasible2Pass2ndPassOt[0, 1] = feasible2Pass2ndPassOt[0, 3] = feasible2Pass2ndPassOt[1, 2] = true;
            feasible2Pass2ndPassCatch[3, 0] = feasible2Pass2ndPassCatch[3, 1] = feasible2Pass2ndPassCatch[2, 1] = feasible2Pass2ndPassCatch[2, 0] = true;

            //feasibleDirectPass = new bool[4, 4];
            //feasible4SecondPass = new bool[4, 4];
            //feasibleDirectPass[0, 1] = true; feasibleDirectPass[0, 2] = true; feasibleDirectPass[0, 3] = true;
            //feasibleDirectPass[1, 0] = true; feasibleDirectPass[1, 2] = true; feasibleDirectPass[1, 3] = true;
            //feasibleDirectPass[2, 3] = true;
            //feasibleDirectPass[3, 2] = true;

            //feasible4SecondPass[0, 1] = true; feasible4SecondPass[0, 2] = false; feasible4SecondPass[0, 3] = true;
            //feasible4SecondPass[1, 0] = true; feasible4SecondPass[1, 3] = false; feasible4SecondPass[1, 2] = true;
            //feasible4SecondPass[2, 0] = true; feasible4SecondPass[2, 3] = true;
            //feasible4SecondPass[3, 2] = true; feasible4SecondPass[3, 1] = true;
            
            shootTarget = GameParameters.OppGoalCenter;
            KickPower = Program.MaxKickSpeed;
            firstPassSpeed = 3;
            secondPassSpeed = 3;
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
            StrategyName = "SquarePassingStrategy";
            AttendanceSize = 4;
            About = "This strategy pass the ball between 4 player placed on a squere verties!";
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

        Position2D firstBallPos = Position2D.Zero;
        public override void DetermineNextState(GameStrategyEngine engine, WorldModel Model)
        {
            #region First
            if (first)
            {
                CalulateSquarePosAndAngles(Model, ref positionersPos, ref positionersAng, ref passerIdx);
                if (!CalculateIDs(Model, Attendance, positionersPos, ref positionersID))
                    return;
                firstBallPos = Model.BallState.Location;
                first = false;
            }
            #endregion
            if (passed && Model.BallState.Location.DistanceFrom(firstBallPos) > 0.3)
                FreekickDefence.BallIsMovedStrategy = true;
            #region States
            if (CurrentState == (int)State.First)
            {
                timeLimitCounter++;
                if (Model.OurRobots[positionersID[0]].Location.DistanceFrom(positionersPos[0]) < 0.23
                    && Model.OurRobots[positionersID[1]].Location.DistanceFrom(positionersPos[1]) < 0.23
                    && Model.OurRobots[positionersID[2]].Location.DistanceFrom(positionersPos[2]) < 0.23
                    && Model.OurRobots[positionersID[3]].Location.DistanceFrom(positionersPos[3]) < 0.23)
                    initialPosCounter++;
                if (initialPosCounter > waitTresh || timeLimitCounter > maxWaitTresh)
                {
                    CurrentState = (int)State.FirstPass;
                    firstInState = true;
                    timeLimitCounter = 0;
                    initialPosCounter = 0;
                }
                if (Model.BallState.Location.DistanceFrom(firstBallPos) > faildBallMovedDist)
                    CurrentState = (int)State.Finish;
            }
            else if (CurrentState == (int)State.FirstPass)
            {
                if (shooterIdx == -1 && secondPasserIdx != -1 && Model.BallState.Location.DistanceFrom(Model.OurRobots[positionersID[secondPasserIdx]].Location) < 0.3)
                {
                    CurrentState = (int)State.SecondPass;
                    firstInState = true;
                }
                else if (shooterIdx != -1)
                {
                    if (Model.BallState.Location.DistanceFrom(Model.OurRobots[positionersID[shooterIdx]].Location) < 0.3)
                        nearShooter = true;
                    if (nearShooter && Model.BallState.Speed.Size > passSpeedTresh && Model.BallState.Speed.InnerProduct(positionersPos[shooterIdx] - positionersPos[passerIdx]) <= 0)
                        shooted = true;
                    if (shooted && Model.BallState.Location.DistanceFrom(positionersPos[shooterIdx]) > 0.5)
                        CurrentState = (int)State.Finish;
                }
                if (passed)
                {
                    int idx = (shooterIdx != -1) ? shooterIdx : secondPasserIdx;
                    Vector2D refrence = positionersPos[idx] - Model.BallState.Location;
                    Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                    double distTresh = (shooterIdx != -1 && !catchPass) ? faildOtDistTresh : faildCatchDistTresh;

                    if (v.Y < faildSpeedTresh && (Model.BallState.Location.DistanceFrom(positionersPos[idx]) > distTresh))
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

            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (shooterIdx != -1 && Model.BallState.Location.DistanceFrom(Model.OurRobots[positionersID[shooterIdx]].Location) < 0.3)
                    nearShooter = true;
                if (nearShooter && secondPasserIdx != -1 && Model.BallState.Speed.InnerProduct(positionersPos[shooterIdx] - positionersPos[secondPasserIdx]) <= 0)
                    shooted = true;
                if (GoNormal || shooted && Model.BallState.Location.DistanceFrom(positionersPos[shooterIdx]) > 0.5)
                    CurrentState = (int)State.Finish;
          
                Vector2D refrence = positionersPos[shooterIdx] - Model.BallState.Location;
                Vector2D v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                double distTresh = (!catchPass) ? faildOtDistTresh : faildCatchDistTresh;
                if (passed && secondPasskicked)
                {
                    if (v.Y < faildSpeedTresh && (Model.BallState.Location.DistanceFrom(positionersPos[shooterIdx]) > distTresh))
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
                    refrence = positionersPos[secondPasserIdx] - Model.BallState.Location;
                    v = GameParameters.InRefrence(Model.BallState.Speed, refrence);
                    if (v.Y < faildSecondPassSpeedTresh && Model.BallState.Location.DistanceFrom(positionersPos[secondPasserIdx]) > faildBallDistSecondPass)
                    {
                        faildCounter++;
                        if (faildCounter > faildMaxCounter)
                            CurrentState = (int)State.Finish;
                    }
                    else
                        faildCounter = Math.Max(0, faildCounter - 5);

                }

            }
            #endregion
            #region PosAndAngles
            if (CurrentState == (int)State.FirstPass)
            {
                if (firstInState)
                {
                    shooterIdxCalculated = false;
                    
                    secondPasserIdx = CalculatePasserIdx(engine,Model,positionersPos,positionersID, passerIdx, secondPasserIdx, true, ref shooterIdx,ref catchPass, out GoNormal,out chipAllowd);
                    if (shooterIdx != -1)
                    {
                        //double goodness;
                        //var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, positionersPos[shooterIdx], out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                        //if (GoodPointInGoal.HasValue)
                        //    shootTarget = GoodPointInGoal.Value;
                        shootTarget = GameParameters.OppGoalCenter.Extend(0, -Math.Sign(positionersPos[shooterIdx].Y) * 0.2);
                        positionersAng[shooterIdx] = (shootTarget - positionersPos[shooterIdx]).AngleInDegrees;
                    }
                    passed = false;
                    firstInState = false;
                }

                if (IsBallPassed(Model, positionersPos,positionersID, passerIdx, secondPasserIdx, shooterIdx, true, firstPassIsChip, rotateInPass))
                    passed = true;
                if (passed)
                {
                    int idx;
                    if (!shooterIdxCalculated && CalculateShooterIdxAfterPass(Model, positionersPos, positionersID, passerIdx, firstPassIsChip, out idx))
                    {

                        if (Model.BallState.Location.DistanceFrom(firstBallPos) > 0.05)
                            shooterIdxCalculated = true;
                        if (shooterIdx != -1)
                        {
                            //double goodness;
                            //var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, positionersPos[idx], out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                            //if (GoodPointInGoal.HasValue)
                            //    shootTarget = GoodPointInGoal.Value;
                            shootTarget = GameParameters.OppGoalCenter.Extend(0, -Math.Sign(positionersPos[shooterIdx].Y) * 0.2);
                            shooterIdx = idx;
                        }
                        else
                            secondPasserIdx = idx;
                    }
                }

                if (!passed && !firstPassChipOrigin && (catchPass || chipAllowd))
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    if (shooterIdx == -1)
                        firstPassIsChip = obs.Meet(Model.BallState, new SingleObjectState(positionersPos[secondPasserIdx], Vector2D.Zero, 0), 0.07);
                    else
                        firstPassIsChip = obs.Meet(Model.BallState, new SingleObjectState(positionersPos[shooterIdx], Vector2D.Zero, 0), 0.07);
                }
                else if (passed && shooterIdx != -1)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    shootIsChip = obs.Meet(Model.OurRobots[positionersID[shooterIdx]], new SingleObjectState(Model.OurRobots[positionersID[shooterIdx]].Location + (shootTarget - Model.OurRobots[positionersID[shooterIdx]].Location).GetNormalizeToCopy(1.4), Vector2D.Zero, 0), 0.022);
                    if (shootIsChip)
                        KickPower = Model.BallState.Location.DistanceFrom(shootTarget) * 0.4;
                }
                else if (firstPassChipOrigin)
                    firstPassIsChip = true;

                if (shooterIdx != -1)
                    firstPassSpeed = (!firstPassIsChip) ? engine.GameInfo.CalculateKickSpeed(Model, positionersID[passerIdx], positionersPos[passerIdx], positionersPos[shooterIdx], firstPassIsChip, catchPass) : Model.BallState.Location.DistanceFrom(positionersPos[shooterIdx]) * 0.55;
                else
                    firstPassSpeed = (!firstPassIsChip) ? engine.GameInfo.CalculateKickSpeed(Model, positionersID[passerIdx], positionersPos[passerIdx], positionersPos[secondPasserIdx], firstPassIsChip, true) : Model.BallState.Location.DistanceFrom(positionersPos[secondPasserIdx]) * 0.55;
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (firstInState)
                {
                    shooterIdxCalculated = false;
                    secondPasskicked = false;
                    nearShooter = false;
                    secondPassIsChip = false;
                    shootIsChip = false;
                    KickPower = 8;
                    secondPasserIdx = CalculatePasserIdx(engine,Model,positionersPos, positionersID, passerIdx, secondPasserIdx, false, ref shooterIdx,ref catchPass, out GoNormal, out chipAllowd);
                    //double goodness;
                    //var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, positionersPos[shooterIdx], out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                    //if (GoodPointInGoal.HasValue)
                    //    shootTarget = GoodPointInGoal.Value;
                    shootTarget = GameParameters.OppGoalCenter.Extend(0, -Math.Sign(Model.BallState.Location.Y) * 0.2);
                    positionersAng[shooterIdx] = (shootTarget - positionersPos[shooterIdx]).AngleInDegrees;

                    firstInState = false;
                    passed = false;
                }

                if (IsBallPassed(Model, positionersPos, positionersID, passerIdx, secondPasserIdx, shooterIdx, false, (!secondPassChipOrigin) ? secondPassIsChip : false,true))
                    passed = true;
                if (passed)
                {
                    int idx;
                    if (!shooterIdxCalculated && CalculateShooterIdxAfterPass(Model, positionersPos, positionersID, secondPasserIdx, secondPassIsChip, out idx))
                    {
                        //double goodness;
                        //var GoodPointInGoal = engine.GameInfo.GetAGoodTargetPointInGoal(Model, null, positionersPos[shooterIdx], out goodness, GameParameters.OppGoalLeft, GameParameters.OppGoalRight, true, false, null);
                        //if (GoodPointInGoal.HasValue)
                        //    shootTarget = GoodPointInGoal.Value;
                        shootTarget = GameParameters.OppGoalCenter.Extend(0, -Math.Sign(positionersPos[shooterIdx].Y) * 0.2);
                        shooterIdxCalculated = true;
                        shooterIdx = idx;
                    }
                }
                if (!passed && !secondPassChipOrigin && (catchPass || chipAllowd))
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    secondPassIsChip = obs.Meet(Model.BallState, new SingleObjectState(positionersPos[shooterIdx], Vector2D.Zero, 0), 0.07);
                }
                else if (!passed)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    secondPassIsChip = obs.Meet(Model.OurRobots[positionersID[secondPasserIdx]], new SingleObjectState(Model.OurRobots[positionersID[secondPasserIdx]].Location + (positionersPos[shooterIdx] - Model.OurRobots[positionersID[secondPasserIdx]].Location).GetNormalizeToCopy(0.7), Vector2D.Zero, 0), 0.022);
                    //if (secondPassIsChip)
                    //    KickPower = Model.BallState.Location.DistanceFrom(positionersPos[shooterIdx]) * 0.55;
                }
                else if (passed)
                {
                    Obstacles obs = new Obstacles(Model);
                    obs.AddObstacle(1, 0, 0, 0, Model.OurRobots.Keys.ToList(), null);
                    shootIsChip = obs.Meet(Model.OurRobots[positionersID[shooterIdx]], new SingleObjectState(Model.OurRobots[positionersID[shooterIdx]].Location + (shootTarget - Model.OurRobots[positionersID[shooterIdx]].Location).GetNormalizeToCopy(1.4), Vector2D.Zero, 0), 0.022);
                    if (shootIsChip)
                        KickPower = Model.BallState.Location.DistanceFrom(shootTarget) * 0.4;
                }
                else if (secondPassChipOrigin)
                    secondPassIsChip = true;

                secondPassSpeed = (!secondPassIsChip) ? engine.GameInfo.CalculateKickSpeed(Model, positionersID[secondPasserIdx], positionersPos[secondPasserIdx], positionersPos[shooterIdx], secondPassIsChip, catchPass) : positionersPos[secondPasserIdx].DistanceFrom(positionersPos[shooterIdx]) * 0.7;
            }
            #endregion
            #region Debug
            if (debug)
            {
                for (int i = 0; i < positionersPos.Length; i++)
                {
                    if (i == passerIdx)
                        DrawingObjects.AddObject(new Circle(positionersPos[i], 0.15, new Pen(Color.Red, 0.01f)));
                    if(i == secondPasserIdx)
                        DrawingObjects.AddObject(new Circle(positionersPos[i], 0.15, new Pen(Color.Blue, 0.01f)));
                    
                    if (i == shooterIdx)
                        DrawingObjects.AddObject(new Circle(positionersPos[i], 0.15, new Pen(Color.YellowGreen, 0.01f)));

                    DrawingObjects.AddObject(new Circle(positionersPos[i], 0.1));
                    DrawingObjects.AddObject(new StringDraw("idx: " + i.ToString(), positionersPos[i] + new Vector2D(0.2, 0.2)));
                    DrawingObjects.AddObject(new StringDraw("Marking State ToBall: " + Model.markingStatesToBall[positionersID[i]].ToString(), positionersPos[i] + new Vector2D(-0.3, -0.3)),"markingtoball"+i);
                    DrawingObjects.AddObject(new StringDraw("Marking State Totar: " + Model.markingStatesToTarget[positionersID[i]].ToString(), positionersPos[i] + new Vector2D(-0.2, -0.2)), "markingtotar" + i);
                }
                DrawingObjects.AddObject(new StringDraw("1stPassIsChip: " + (firstPassIsChip), new Position2D(0, 1) + new Vector2D(0, 0)));
                DrawingObjects.AddObject(new StringDraw("1stPassChipOrigin: " + (firstPassChipOrigin), new Position2D(0, 1) + new Vector2D(0.2, 0)));
                DrawingObjects.AddObject(new StringDraw("1stPassSpeed: " + (firstPassSpeed), new Position2D(0, 1) + new Vector2D(0.4, 0)));
                DrawingObjects.AddObject(new StringDraw("passerIdx: " + (passerIdx), new Position2D(0, 1) + new Vector2D(0.6, 0)));

                DrawingObjects.AddObject(new StringDraw("2ndPassIsChip: " + (secondPassIsChip), new Position2D(0, -1) + new Vector2D(0, 0)));
                DrawingObjects.AddObject(new StringDraw("2ndPassChipOrigin: " + (secondPassChipOrigin), new Position2D(0, -1) + new Vector2D(0.2, 0)));
                DrawingObjects.AddObject(new StringDraw("2sndPassSpeed: " + (secondPassSpeed), new Position2D(0, -1) + new Vector2D(0.4, 0)));
                DrawingObjects.AddObject(new StringDraw("2ndPasserIdx: " + (secondPasserIdx), new Position2D(0, -1) + new Vector2D(0.6, 0)));

                DrawingObjects.AddObject(new StringDraw("ShootSpeed: " + (KickPower), new Position2D(1.5, 0) + new Vector2D(0.0, 0)));
                DrawingObjects.AddObject(new StringDraw("passed: " + (passed), new Position2D(1.5, 0) + new Vector2D(0.2, 0)));
                DrawingObjects.AddObject(new StringDraw("ShooterIdx: " + (shooterIdx), new Position2D(1.5, 0) + new Vector2D(0.4, 0)));
                DrawingObjects.AddObject(new StringDraw("shooted: " + (shooted), new Position2D(1.5, 0) + new Vector2D(0.6, 0)));


                DrawingObjects.AddObject(new Circle(shootTarget, 0.15, new Pen(Color.Brown, 0.01f)));
                DrawingObjects.AddObject(new StringDraw("State: " + ((State)CurrentState).ToString(), Position2D.Zero + new Vector2D(0.2, 0.2)));
                DrawingObjects.AddObject(new StringDraw("FirstInStateState: " + (firstInState), Position2D.Zero + new Vector2D(0.3, 0.2)));
                DrawingObjects.AddObject(new StringDraw("kiicked: " + ((secondPasserIdx != -1)?GetRole<CatchAndRotateBallRole>(positionersID[secondPasserIdx]).Kiick:false), Position2D.Zero + new Vector2D(0.4, 0.2)));
                DrawingObjects.AddObject(new StringDraw("secondPassKiked: " + secondPasskicked, Position2D.Zero + new Vector2D(0.5, 0.2)));
            }
            #endregion
        }

        public override Dictionary<int, RoleBase> RunStrategy(GameStrategyEngine engine, GameDefinitions.WorldModel Model, out Dictionary<int, CommonDelegate> Functions)
        {
            Dictionary<int, RoleBase> CurrentlyAssignedRoles = new Dictionary<int, RoleBase>();
            Functions = new Dictionary<int, CommonDelegate>();

            if (CurrentState == (int)State.First)
            {
                for (int i = 0; i < positionersID.Length; i++)
                {
                    Planner.ChangeDefaulteParams(positionersID[i], false);
                    Planner.SetParameter(positionersID[i], 4, 3);
                    Planner.Add(positionersID[i], positionersPos[i], positionersAng[i], PathType.UnSafe, true, true, true, true);
                }
            }
            else if (CurrentState == (int)State.FirstPass)
            {
                for (int i = 0; i < positionersID.Length; i++)
                {
                    if (i == passerIdx)
                    {
                        if (!passed)
                        {
                            Rotate r;
                            if (shooterIdx == -1)
                                r = Planner.AddRotate(Model, positionersID[i], positionersPos[secondPasserIdx], /*(Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(initDist) + Model.BallState.Location*/60, kickPowerType.Speed, firstPassSpeed, firstPassIsChip, RotateDelay, backSensor);
                            else
                            {
                                Position2D passTar = positionersPos[shooterIdx] + (shootTarget - positionersPos[shooterIdx]).GetNormalizeToCopy(behindBallTresh);
                                r = Planner.AddRotate(Model, positionersID[i], passTar, /*(Model.BallState.Location - GameParameters.OppGoalCenter).GetNormalizeToCopy(initDist) + Model.BallState.Location*/60, kickPowerType.Speed, firstPassSpeed, firstPassIsChip, RotateDelay, backSensor);
                            }
                            if (r.InKickState)
                                rotateInPass = true;
                        }
                        else
                        {
                            Planner.Add(positionersID[i], positionersPos[i], positionersAng[i], PathType.UnSafe, true, true, true, false);
                        }
                    }
                    else if (i == shooterIdx && passed)
                    {
                        if (!catchPass)
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, positionersID[shooterIdx], typeof(OneTouchRole)))
                                Functions[positionersID[shooterIdx]] = (eng, wmd) => GetRole<OneTouchRole>(positionersID[shooterIdx]).Perform(engine, Model, positionersID[shooterIdx], Model.OurRobots[positionersID[passerIdx]], firstPassIsChip, shootTarget, KickPower, shootIsChip,(firstPassIsChip)? 0:firstPassSpeed);
                        }
                        else
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, positionersID[shooterIdx], typeof(CatchAndRotateBallRole)))
                                Functions[positionersID[shooterIdx]] = (eng, wmd) => GetRole<CatchAndRotateBallRole>(positionersID[shooterIdx]).CatchAndRotate(engine, Model, positionersID[shooterIdx], shootTarget, firstPassIsChip, shootIsChip, true, KickPower);

                        }
                    }
                    else if (shooterIdx == -1 && i == secondPasserIdx && passed)
                    {

                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, positionersID[secondPasserIdx], typeof(CatchAndRotateBallRole)))
                            Functions[positionersID[secondPasserIdx]] = (eng, wmd) => GetRole<CatchAndRotateBallRole>(positionersID[secondPasserIdx]).CatchAndRotate(engine, Model, positionersID[secondPasserIdx], GameParameters.OppGoalCenter, firstPassIsChip, secondPassIsChip, true, secondPassSpeed, false, RotateDelay);
                    }
                    else
                        Planner.Add(positionersID[i], positionersPos[i], positionersAng[i], PathType.UnSafe, true, true, true, true);
                }
            }
            else if (CurrentState == (int)State.SecondPass)
            {
                if (GetRole<CatchAndRotateBallRole>(positionersID[secondPasserIdx]).Kiick)
                    secondPasskicked = true;

                for (int i = 0; i < positionersID.Length; i++)
                {
                    if (i == secondPasserIdx)
                    {
                        Position2D passTar = positionersPos[shooterIdx] + (shootTarget - positionersPos[shooterIdx]).GetNormalizeToCopy(behindBallTresh);
                        if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, positionersID[secondPasserIdx], typeof(CatchAndRotateBallRole)))
                            Functions[positionersID[secondPasserIdx]] = (eng, wmd) => GetRole<CatchAndRotateBallRole>(positionersID[secondPasserIdx]).CatchAndRotate(engine, Model, positionersID[secondPasserIdx], passTar, firstPassIsChip, secondPassIsChip, false, secondPassSpeed, false, RotateDelay);
                    }
                    else if (i == shooterIdx && passed && secondPasskicked)
                    {

                        if (!catchPass)
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, positionersID[shooterIdx], typeof(OneTouchRole)))
                                Functions[positionersID[shooterIdx]] = (eng, wmd) => GetRole<OneTouchRole>(positionersID[shooterIdx]).Perform(engine, Model, positionersID[shooterIdx], Model.OurRobots[positionersID[secondPasserIdx]], secondPassIsChip, shootTarget, KickPower, shootIsChip, (secondPassIsChip) ? 0 : secondPassSpeed);
                        }
                        else
                        {
                            if (StaticRoleAssigner.AssignRole(engine, Model, PreviouslyAssignedRoles, CurrentlyAssignedRoles, positionersID[shooterIdx], typeof(CatchAndRotateBallRole)))
                                Functions[positionersID[shooterIdx]] = (eng, wmd) => GetRole<CatchAndRotateBallRole>(positionersID[shooterIdx]).CatchAndRotate(engine, Model, positionersID[shooterIdx], shootTarget, secondPassIsChip, shootIsChip, true, KickPower);

                        }

                    }
                    else
                        Planner.Add(positionersID[i], positionersPos[i], positionersAng[i], PathType.UnSafe, true, true, true, true);
                }
            }

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
